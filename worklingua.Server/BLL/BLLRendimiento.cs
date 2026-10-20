using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLRendimiento
    {
        private const int AniosMaximos = 5;

        MPPRendimiento oMPPRen;
        MPPUsuario oMPPUsu;
        BLLSeguridad oBLLSeg;
        BLLEmpresa oBLLEmp;
        BLLDepartamento oBLLDep;

        public BLLRendimiento()
        {
            oMPPRen = new MPPRendimiento();
            oMPPUsu = new MPPUsuario();
            oBLLSeg = new BLLSeguridad();
            oBLLEmp = new BLLEmpresa();
            oBLLDep = new BLLDepartamento();
        }

        public BERendimiento Obtener(BEFiltroRendimiento Objeto, BESesion oSesionBE)
        {
            Normalizar(Objeto, oSesionBE);

            BEEmpresa oEmpresaBE = ObtenerEmpresaObligatoria(Objeto.EmpresaId);

            Objeto.DepartamentoId = oBLLDep.ResolverParaUsuario(
                Objeto.DepartamentoId, Objeto.EmpresaId);

            BERendimiento respuesta = new BERendimiento();

            respuesta.EmpresaId = oEmpresaBE.EmpresaId;
            respuesta.Empresa = oEmpresaBE.RazonSocial;
            respuesta.Resumen = oMPPRen.ObtenerResumen(Objeto);
            respuesta.Departamentos = oMPPRen.ListarPorDepartamento(Objeto);
            respuesta.Empleados = oMPPRen.ListarPorEmpleado(Objeto);

            CalcularCostos(respuesta.Resumen, Objeto.DepartamentoId.HasValue);

            return respuesta;
        }

        private void CalcularCostos(BERendimientoResumen oResumenBE, bool porDepartamento)
        {
            if (porDepartamento)
            {
                oResumenBE.Facturado = null;
                oResumenBE.CostoPorModulo = null;
                oResumenBE.CostoPorEmpleado = null;

                return;
            }

            decimal facturado = oResumenBE.Facturado ?? 0m;

            oResumenBE.CostoPorModulo = oResumenBE.ModulosCompletados == 0
                ? 0m
                : decimal.Round(facturado / oResumenBE.ModulosCompletados, 2);

            oResumenBE.CostoPorEmpleado = oResumenBE.EmpleadosConActividad == 0
                ? 0m
                : decimal.Round(facturado / oResumenBE.EmpleadosConActividad, 2);
        }

        private void Normalizar(BEFiltroRendimiento Objeto, BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue)
            {
                Objeto.EmpresaId = empresaQueLimita.Value;
            }

            if (Objeto.EmpresaId == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Elegí la empresa.");
            }

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

            if (!Objeto.Hasta.HasValue)
            {
                Objeto.Hasta = hoy;
            }

            if (!Objeto.Desde.HasValue)
            {
                Objeto.Desde = new DateOnly(hoy.Year, 1, 1);
            }

            if (Objeto.Hasta.Value < Objeto.Desde.Value)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Rango de fechas inválido.");
            }

            if (Objeto.Desde.Value.AddYears(AniosMaximos) < Objeto.Hasta.Value)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El panel abarca como máximo " + AniosMaximos + " años.");
            }
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

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            if (oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.CursoVerTodasLasEmpresas))
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
