using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLLicencia
    {
        public const string EstadoActiva = "ACTIVA";
        public const string EstadoRevocada = "REVOCADA";

        MPPLicencia oMPPLic;
        MPPUsuario oMPPUsu;
        BLLSeguridad oBLLSeg;
        BLLSuscripcion oBLLSus;
        BLLEmpresa oBLLEmp;

        public BLLLicencia()
        {
            oMPPLic = new MPPLicencia();
            oMPPUsu = new MPPUsuario();
            oBLLSeg = new BLLSeguridad();
            oBLLSus = new BLLSuscripcion();
            oBLLEmp = new BLLEmpresa();
        }

        public BEInventarioRespuesta ObtenerInventario(int empresaId, BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue)
            {
                empresaId = empresaQueLimita.Value;
            }

            if (empresaId == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Elegí la empresa.");
            }

            BEEmpresa oEmpresaBE = ObtenerEmpresaObligatoria(empresaId);
            BESuscripcionConPlan oSuscripcionBE = ObtenerSuscripcionVigente(empresaId);

            BEInventarioRespuesta respuesta = new BEInventarioRespuesta();

            respuesta.EmpresaId = empresaId;
            respuesta.Empresa = oEmpresaBE.RazonSocial;
            respuesta.Plan = oSuscripcionBE.Plan;
            respuesta.Contratadas = oSuscripcionBE.CantidadLicencias;

            List<BEInventarioLicencia> ListaInventarioBE = oMPPLic.ListarPorEmpresa(
                empresaId,
                oSuscripcionBE.Suscripcion.SuscripcionId,
                Configuracion.LicenciasRolAlcanzado);

            if (ListaInventarioBE != null)
            {
                respuesta.Empleados = ListaInventarioBE;
            }

            respuesta.Asignadas = ContarAsignadas(oSuscripcionBE.Suscripcion.SuscripcionId);
            respuesta.Disponibles = respuesta.Contratadas - respuesta.Asignadas;

            return respuesta;
        }

        public BELicencia Asignar(BEAsignarLicencia Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerUsuarioObligatorio(Objeto.UsuarioId);

            ExigirAlcanceSobre(oUsuarioBE.EmpresaId, oSesionBE);

            BESuscripcionConPlan oSuscripcionBE = ObtenerSuscripcionVigente(oUsuarioBE.EmpresaId);
            int suscripcionId = oSuscripcionBE.Suscripcion.SuscripcionId;

            if (!NecesitaLicencia(oUsuarioBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Solo los empleados necesitan una licencia para acceder a los cursos.");
            }

            if (TieneLicenciaVigente(oUsuarioBE.UsuarioId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El empleado ya tiene una licencia asignada.");
            }

            if (ContarAsignadas(suscripcionId) >= oSuscripcionBE.CantidadLicencias)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Debe realizar un Upgrade de su plan para obtener más cupos.");
            }

            BELicencia oLicenciaBE = new BELicencia();

            oLicenciaBE.SuscripcionId = suscripcionId;
            oLicenciaBE.UsuarioId = oUsuarioBE.UsuarioId;
            oLicenciaBE.FechaVencimiento = AVencimiento(oSuscripcionBE);

            oLicenciaBE.LicenciaId = oMPPLic.Asignar(oLicenciaBE);
            oLicenciaBE.Estado = EstadoActiva;

            return oLicenciaBE;
        }

        public BEUsuario Revocar(BELicencia Objeto, BESesion oSesionBE)
        {
            BELicencia oLicenciaBE = ObtenerLicenciaObligatoria(Objeto.LicenciaId);
            BEUsuario oUsuarioBE = ObtenerUsuarioObligatorio(oLicenciaBE.UsuarioId.Value);

            ExigirAlcanceSobre(oUsuarioBE.EmpresaId, oSesionBE);

            if (!oMPPLic.Revocar(oLicenciaBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La licencia ya estaba revocada.");
            }

            return oUsuarioBE;
        }

        public bool TieneLicenciaVigente(int usuarioId)
        {
            BELicencia oFiltroBE = new BELicencia();
            oFiltroBE.UsuarioId = usuarioId;

            BELicencia oLicenciaBE = oMPPLic.ObtenerPorUsuario(oFiltroBE);

            return oLicenciaBE != null &&
                   oLicenciaBE.Estado == EstadoActiva &&
                   oLicenciaBE.EstadoSuscripcion == BLLSuscripcion.EstadoActiva;
        }

        public void ExigirLicenciaVigente(BEUsuario oUsuarioBE)
        {
            if (!NecesitaLicencia(oUsuarioBE))
            {
                return;
            }

            if (TieneLicenciaVigente(oUsuarioBE.UsuarioId))
            {
                return;
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.PermisoDenegado,
                "Todavía no tenés una licencia asignada. Pedile a quien administra tu empresa " +
                "que te asigne una para poder acceder a tus cursos.");
        }

        private bool NecesitaLicencia(BEUsuario oUsuarioBE)
        {
            List<BERol> ListaRolBE = oMPPUsu.ObtenerRoles(oUsuarioBE);

            if (ListaRolBE == null)
            {
                return false;
            }

            foreach (BERol oRolBE in ListaRolBE)
            {
                if (string.Equals(
                        oRolBE.Nombre == null ? null : oRolBE.Nombre.Trim(),
                        Configuracion.LicenciasRolAlcanzado.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public int RevocarPorSuscripcion(int suscripcionId)
        {
            BELicencia oFiltroBE = new BELicencia();
            oFiltroBE.SuscripcionId = suscripcionId;

            return oMPPLic.RevocarPorSuscripcion(oFiltroBE);
        }

        public int RevocarPorUsuario(int usuarioId)
        {
            BELicencia oFiltroBE = new BELicencia();
            oFiltroBE.UsuarioId = usuarioId;

            return oMPPLic.RevocarPorUsuario(oFiltroBE);
        }

        public int Migrar(BESuscripcionConPlan oOrigenBE, BESuscripcionConPlan oDestinoBE)
        {
            if (oOrigenBE == null || oDestinoBE == null)
            {
                return 0;
            }

            return oMPPLic.Migrar(
                oOrigenBE.Suscripcion.SuscripcionId,
                oDestinoBE.Suscripcion.SuscripcionId,
                oDestinoBE.CantidadLicencias,
                AVencimiento(oDestinoBE));
        }

        public int ObtenerCupoDisponible(int empresaId)
        {
            BESuscripcionConPlan oSuscripcionBE = ObtenerSuscripcionVigente(empresaId);

            int disponibles = oSuscripcionBE.CantidadLicencias -
                ContarAsignadas(oSuscripcionBE.Suscripcion.SuscripcionId);

            return disponibles < 0 ? 0 : disponibles;
        }

        public int ContarAsignadas(int suscripcionId)
        {
            BELicencia oFiltroBE = new BELicencia();
            oFiltroBE.SuscripcionId = suscripcionId;

            return oMPPLic.ContarActivas(oFiltroBE);
        }

        private DateTime? AVencimiento(BESuscripcionConPlan oSuscripcionBE)
        {
            DateOnly? fechaFin = oSuscripcionBE.Suscripcion.FechaFin;

            return fechaFin.HasValue ? fechaFin.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;
        }

        private BESuscripcionConPlan ObtenerSuscripcionVigente(int empresaId)
        {
            BESuscripcion oFiltroBE = new BESuscripcion();
            oFiltroBE.EmpresaId = empresaId;

            BESuscripcionConPlan oSuscripcionBE = oBLLSus.ListarObjeto(oFiltroBE);

            if (oSuscripcionBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La empresa no tiene una contratación activa: no hay licencias para administrar.");
            }

            return oSuscripcionBE;
        }

        private BELicencia ObtenerLicenciaObligatoria(int licenciaId)
        {
            BELicencia oFiltroBE = new BELicencia();
            oFiltroBE.LicenciaId = licenciaId;

            BELicencia oLicenciaBE = oMPPLic.ObtenerPorId(oFiltroBE);

            if (oLicenciaBE == null || !oLicenciaBE.UsuarioId.HasValue)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La licencia no existe.");
            }

            return oLicenciaBE;
        }

        private BEUsuario ObtenerUsuarioObligatorio(int usuarioId)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = usuarioId;

            BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

            if (oUsuarioBE == null || oUsuarioBE.Activo != true)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El empleado no existe.");
            }

            return oUsuarioBE;
        }

        private BEEmpresa ObtenerEmpresaObligatoria(int empresaId)
        {
            BEEmpresa oFiltroBE = new BEEmpresa();
            oFiltroBE.EmpresaId = empresaId;

            BEEmpresa oEmpresaBE = oBLLEmp.ListarObjeto(oFiltroBE);

            if (oEmpresaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La empresa no existe.");
            }

            return oEmpresaBE;
        }

        private void ExigirAlcanceSobre(int empresaId, BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue && empresaId != empresaQueLimita.Value)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La licencia no existe.");
            }
        }

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            if (oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.LicenciaVerTodasLasEmpresas))
            {
                return null;
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = oSesionBE.UsuarioId;

            BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

            if (oUsuarioBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.SesionInvalida, "El usuario de la sesión no existe.");
            }

            return oUsuarioBE.EmpresaId;
        }
    }
}
