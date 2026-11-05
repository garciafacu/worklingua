using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLReporte
    {
        public const string AgrupacionDia = "DIA";
        public const string AgrupacionSemana = "SEMANA";
        public const string AgrupacionMes = "MES";
        public const string AgrupacionAnio = "ANIO";

        private const int AniosMaximos = 5;

        MPPReporte oMPPRep = new MPPReporte();

        public List<BEGananciaPeriodo> ListarGanancias(BEFiltroReporte Objeto)
        {
            Normalizar(Objeto);

            return oMPPRep.ListarGanancias(Objeto);
        }

        public List<BEGananciaZona> ListarGananciasPorZona(BEFiltroReporte Objeto)
        {
            Normalizar(Objeto);

            return oMPPRep.ListarGananciasPorZona(Objeto);
        }

        public BETablero ObtenerTablero()
        {
            BETablero oTableroBE = oMPPRep.ObtenerTablero();

            oTableroBE.Planes = oMPPRep.ListarContratacionesPorPlan();

            return oTableroBE;
        }

        private void Normalizar(BEFiltroReporte Objeto)
        {
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

            if (!Objeto.Desde.HasValue)
            {
                Objeto.Desde = new DateOnly(hoy.Year, 1, 1);
            }

            if (!Objeto.Hasta.HasValue)
            {
                Objeto.Hasta = hoy;
            }

            if (Objeto.Hasta.Value < Objeto.Desde.Value)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La fecha de fin no puede ser anterior a la de inicio.");
            }

            if (Objeto.Desde.Value < Objeto.Hasta.Value.AddYears(-AniosMaximos))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El reporte abarca como máximo " + AniosMaximos + " años.");
            }

            Objeto.Agrupacion = NormalizarAgrupacion(Objeto.Agrupacion);
            Objeto.Provincia = string.IsNullOrWhiteSpace(Objeto.Provincia) ? null : Objeto.Provincia.Trim();
        }

        private string NormalizarAgrupacion(string agrupacion)
        {
            if (string.IsNullOrWhiteSpace(agrupacion))
            {
                return AgrupacionMes;
            }

            string valor = agrupacion.Trim().ToUpperInvariant();

            if (valor == AgrupacionDia || valor == AgrupacionSemana
                || valor == AgrupacionMes || valor == AgrupacionAnio)
            {
                return valor;
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La agrupación tiene que ser " + AgrupacionDia + ", " + AgrupacionSemana + ", "
                    + AgrupacionMes + " o " + AgrupacionAnio + ".");
        }
    }

    public class BLLRendimiento
    {
        private const int AniosMaximos = 5;

        MPPRendimiento oMPPRen = new MPPRendimiento();
        BLLSeguridad oBLLSeg = new BLLSeguridad();
        BLLEmpresa oBLLEmp = new BLLEmpresa();
        BLLDepartamento oBLLDep = new BLLDepartamento();

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

            return oBLLEmp.ListarObjeto(oFiltroBE);
        }

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            return oBLLSeg.EmpresaQueLimita(oSesionBE, Permisos.CursoVerTodasLasEmpresas);
        }
    }
}
