using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLSeguridad
    {
        MPPUsuario oMPPUsu;
        BLLPermiso oBLLPer;
        BLLSesion oBLLSes;
        BLLBitacora oBLLBit;

        public BLLSeguridad()
        {
            oMPPUsu = new MPPUsuario();
            oBLLPer = new BLLPermiso();
            oBLLSes = new BLLSesion();
            oBLLBit = new BLLBitacora();
        }

        public List<string> ObtenerPermisos(int usuarioId)
        {
            List<string> codigos = new List<string>();

            foreach (BEPermiso oPermisoBE in ObtenerRaices(usuarioId))
            {
                AgregarNombres(oPermisoBE, codigos);
            }

            codigos.Sort(StringComparer.Ordinal);

            return codigos;
        }

        public bool Tiene(int usuarioId, string permiso)
        {
            foreach (BEPermiso oPermisoBE in ObtenerRaices(usuarioId))
            {
                if (oPermisoBE.BuscarPermisoPorNombre(permiso) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private List<BEPermiso> ObtenerRaices(int usuarioId)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = usuarioId;

            return oBLLPer.ObtenerAsignados(oMPPUsu.ObtenerPermisos(oFiltroBE));
        }

        private void AgregarNombres(BEPermiso oPermisoBE, List<string> codigos)
        {
            if (codigos.Contains(oPermisoBE.Nombre))
            {
                return;
            }

            codigos.Add(oPermisoBE.Nombre);

            foreach (BEPermiso oHijoBE in oPermisoBE.ObtenerPermisosHijos())
            {
                AgregarNombres(oHijoBE, codigos);
            }
        }

        public BESesion ExigirPermiso(Guid token, string permiso)
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(token);

            if (Tiene(oSesionBE.UsuarioId, permiso))
            {
                return oSesionBE;
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloSeguridad,
                "PermisoDenegado",
                "Falta el permiso " + permiso + ".",
                "WARN"));

            throw new ExcepcionNegocio(
                TipoErrorNegocio.PermisoDenegado, "No tenés permiso para realizar esta acción.");
        }
    }
}
