using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLInstalacion
    {
        MPPUsuario oMPPUsu;
        MPPRol oMPPRol;
        BLLRol oBLLRol;
        BLLEmpresa oBLLEmp;
        BLLBitacora oBLLBit;
        ServicioHash oServicioHash;

        public BLLInstalacion()
        {
            oMPPUsu = new MPPUsuario();
            oMPPRol = new MPPRol();
            oBLLRol = new BLLRol();
            oBLLEmp = new BLLEmpresa();
            oBLLBit = new BLLBitacora();
            oServicioHash = new ServicioHash();
        }

        public void AsegurarSuperAdmin()
        {
            if (!Configuracion.SuperAdminConfigurado)
            {
                ServicioLog.Advertencia(
                    "SuperAdmin no configurado: falta 'SuperAdmin:Email' o 'SuperAdmin:Clave' en " +
                    "worklingua.Server/appsettings.Local.json. No se creó ninguna cuenta de plataforma.");

                return;
            }

            BERol oRolBE = ObtenerRolObligatorio();

            int permisosNuevos = oMPPRol.SincronizarPermisos(oRolBE);

            ServicioLog.Informacion(
                "Rol '" + oRolBE.Nombre + "': permisos sincronizados, " + permisosNuevos +
                " asignados en este arranque.");

            string email = Configuracion.SuperAdminEmail.Trim().ToLowerInvariant();
            BEUsuario oUsuarioBE = ObtenerPorEmail(email);

            if (oUsuarioBE != null)
            {
                AsignarRol(oUsuarioBE.UsuarioId, oRolBE.RolId);

                ServicioLog.Informacion(
                    "La cuenta de plataforma " + email + " ya existía: se conservó su clave y se " +
                    "verificó la asignación del rol '" + oRolBE.Nombre + "'.");

                return;
            }

            Crear(email, oRolBE);
        }

        private BERol ObtenerRolObligatorio()
        {
            BERol oRolBE = oBLLRol.ObtenerPorNombre(Configuracion.SuperAdminRol);

            if (oRolBE == null)
            {
                throw new InvalidOperationException(
                    "El rol '" + Configuracion.SuperAdminRol + "' configurado en SuperAdmin:Rol no " +
                    "existe entre los roles activos de la tabla Rol. Ejecutar " +
                    "docs/seed.sql contra WorkLinguaDB.");
            }

            return oRolBE;
        }

        private BEEmpresa ObtenerEmpresaObligatoria()
        {
            BEEmpresa oEmpresaBE = oBLLEmp.ObtenerPorCuit(Configuracion.SuperAdminEmpresaCuit);

            if (oEmpresaBE == null)
            {
                throw new InvalidOperationException(
                    "No existe una empresa activa con el CUIT '" + Configuracion.SuperAdminEmpresaCuit +
                    "' configurado en SuperAdmin:EmpresaCuit. Ejecutar " +
                    "docs/seed.sql contra WorkLinguaDB.");
            }

            return oEmpresaBE;
        }

        private void Crear(string email, BERol oRolBE)
        {
            BEEmpresa oEmpresaBE = ObtenerEmpresaObligatoria();

            BEUsuario oUsuarioBE = new BEUsuario();

            oUsuarioBE.EmpresaId = oEmpresaBE.EmpresaId;
            oUsuarioBE.Nombre = Configuracion.SuperAdminNombre;
            oUsuarioBE.Apellido = Configuracion.SuperAdminApellido;
            oUsuarioBE.Email = email;
            oUsuarioBE.PasswordHash = oServicioHash.Hashear(Configuracion.SuperAdminClave);
            oUsuarioBE.Activo = true;

            oUsuarioBE.UsuarioId = oMPPUsu.Guardar(oUsuarioBE);

            AsignarRol(oUsuarioBE.UsuarioId, oRolBE.RolId);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloInstalacion,
                "SuperAdminCreado",
                "Alta de la cuenta de plataforma " + email + " con rol " + oRolBE.Nombre + ".",
                null));

            ServicioLog.Informacion(
                "Cuenta de plataforma creada: " + email + " (rol '" + oRolBE.Nombre + "', empresa '" +
                oEmpresaBE.RazonSocial + "'). Ya puede iniciar sesión.");
        }

        private void AsignarRol(int usuarioId, int rolId)
        {
            BEUsuarioRol oUsuarioRolBE = new BEUsuarioRol();

            oUsuarioRolBE.UsuarioId = usuarioId;
            oUsuarioRolBE.RolId = rolId;

            oMPPUsu.AsignarRol(oUsuarioRolBE);
        }

        private BEUsuario ObtenerPorEmail(string email)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.Email = email;

            return oMPPUsu.ObtenerPorEmail(oFiltroBE);
        }
    }
}
