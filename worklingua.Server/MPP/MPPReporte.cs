using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPReporte
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEGananciaPeriodo> ListarGanancias(BEFiltroReporte Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Desde", Objeto.Desde.Value.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Hasta", Objeto.Hasta.Value.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Agrupacion", Objeto.Agrupacion);
            Hdatos.Add("@Provincia", Objeto.Provincia);

            List<BEGananciaPeriodo> ListaGananciaBE = new List<BEGananciaPeriodo>();
            DataTable Dt = oDatos.Leer("sp_Reporte_Ganancias", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaGananciaBE.Add(new BEGananciaPeriodo(
                    ServicioLectorFila.LeerTexto(Item, "Clave"),
                    ServicioLectorFila.LeerDecimal(Item, "Facturado"),
                    ServicioLectorFila.LeerDecimal(Item, "Cobrado"),
                    ServicioLectorFila.LeerDecimal(Item, "NotasCredito"),
                    ServicioLectorFila.LeerDecimal(Item, "Neto")));
            }

            return ListaGananciaBE;
        }

        public List<BEGananciaZona> ListarGananciasPorZona(BEFiltroReporte Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Desde", Objeto.Desde.Value.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Hasta", Objeto.Hasta.Value.ToDateTime(TimeOnly.MinValue));

            List<BEGananciaZona> ListaZonaBE = new List<BEGananciaZona>();
            DataTable Dt = oDatos.Leer("sp_Reporte_GananciasPorZona", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaZonaBE.Add(new BEGananciaZona(
                    ServicioLectorFila.LeerTexto(Item, "Zona"),
                    ServicioLectorFila.LeerDecimal(Item, "Facturado"),
                    ServicioLectorFila.LeerDecimal(Item, "Cobrado"),
                    ServicioLectorFila.LeerDecimal(Item, "NotasCredito"),
                    ServicioLectorFila.LeerDecimal(Item, "Neto"),
                    ServicioLectorFila.LeerEntero(Item, "Empresas")));
            }

            return ListaZonaBE;
        }

        public BETablero ObtenerTablero()
        {
            Hdatos = new Hashtable();

            DataTable Dt = oDatos.Leer("sp_Reporte_Tablero", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                return new BETablero();
            }

            DataRow Item = Dt.Rows[0];

            BETablero oTableroBE = new BETablero();

            oTableroBE.IngresosMes = ServicioLectorFila.LeerDecimal(Item, "IngresosMes");
            oTableroBE.IngresosMesAnterior = ServicioLectorFila.LeerDecimal(Item, "IngresosMesAnterior");
            oTableroBE.DeudaTotal = ServicioLectorFila.LeerDecimal(Item, "DeudaTotal");
            oTableroBE.EmpresasConDeuda = ServicioLectorFila.LeerEntero(Item, "EmpresasConDeuda");
            oTableroBE.EmpresasActivas = ServicioLectorFila.LeerEntero(Item, "EmpresasActivas");
            oTableroBE.ContratacionesActivas = ServicioLectorFila.LeerEntero(Item, "ContratacionesActivas");
            oTableroBE.LicenciasAsignadas = ServicioLectorFila.LeerEntero(Item, "LicenciasAsignadas");
            oTableroBE.LicenciasContratadas = ServicioLectorFila.LeerEntero(Item, "LicenciasContratadas");
            oTableroBE.EmpresasSinCupo = ServicioLectorFila.LeerEntero(Item, "EmpresasSinCupo");

            return oTableroBE;
        }

        public List<BEContratacionPorPlan> ListarContratacionesPorPlan()
        {
            Hdatos = new Hashtable();

            List<BEContratacionPorPlan> ListaPlanBE = new List<BEContratacionPorPlan>();
            DataTable Dt = oDatos.Leer("sp_Reporte_ContratacionesPorPlan", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaPlanBE.Add(new BEContratacionPorPlan(
                    ServicioLectorFila.LeerTexto(Item, "Plan"),
                    ServicioLectorFila.LeerEntero(Item, "Cantidad")));
            }

            return ListaPlanBE;
        }
    }

    public class MPPRendimiento
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public BERendimientoResumen ObtenerResumen(BEFiltroRendimiento Objeto)
        {
            DataTable Dt = oDatos.Leer("sp_Progreso_RendimientoResumen", ArmarFiltro(Objeto, true));

            if (Dt.Rows.Count == 0)
            {
                return new BERendimientoResumen();
            }

            DataRow Item = Dt.Rows[0];

            BERendimientoResumen oResumenBE = new BERendimientoResumen();

            oResumenBE.EmpleadosConActividad = ServicioLectorFila.LeerEntero(Item, "EmpleadosConActividad");
            oResumenBE.ModulosIniciados = ServicioLectorFila.LeerEntero(Item, "ModulosIniciados");
            oResumenBE.ModulosCompletados = ServicioLectorFila.LeerEntero(Item, "ModulosCompletados");
            oResumenBE.CursosCompletados = ServicioLectorFila.LeerEntero(Item, "CursosCompletados");
            oResumenBE.AvancePromedio = ServicioLectorFila.LeerDecimal(Item, "AvancePromedio");
            oResumenBE.Facturado = ServicioLectorFila.LeerDecimal(Item, "Facturado");

            return oResumenBE;
        }

        public List<BERendimientoDepartamento> ListarPorDepartamento(BEFiltroRendimiento Objeto)
        {
            List<BERendimientoDepartamento> ListaBE = new List<BERendimientoDepartamento>();

            DataTable Dt = oDatos.Leer("sp_Progreso_RendimientoPorDepartamento", ArmarFiltro(Objeto, false));

            foreach (DataRow Item in Dt.Rows)
            {
                BERendimientoDepartamento oDepartamentoBE = new BERendimientoDepartamento();

                oDepartamentoBE.DepartamentoId = ServicioLectorFila.LeerEnteroNulo(Item, "DepartamentoId");
                oDepartamentoBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
                oDepartamentoBE.Empleados = ServicioLectorFila.LeerEntero(Item, "Empleados");
                oDepartamentoBE.EmpleadosConActividad = ServicioLectorFila.LeerEntero(Item, "EmpleadosConActividad");
                oDepartamentoBE.ModulosCompletados = ServicioLectorFila.LeerEntero(Item, "ModulosCompletados");
                oDepartamentoBE.AvancePromedio = ServicioLectorFila.LeerDecimal(Item, "AvancePromedio");

                ListaBE.Add(oDepartamentoBE);
            }

            return ListaBE;
        }

        public List<BERendimientoEmpleado> ListarPorEmpleado(BEFiltroRendimiento Objeto)
        {
            List<BERendimientoEmpleado> ListaBE = new List<BERendimientoEmpleado>();

            DataTable Dt = oDatos.Leer("sp_Progreso_RendimientoPorEmpleado", ArmarFiltro(Objeto, true));

            foreach (DataRow Item in Dt.Rows)
            {
                BERendimientoEmpleado oEmpleadoBE = new BERendimientoEmpleado();

                oEmpleadoBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
                oEmpleadoBE.Empleado = ServicioLectorFila.LeerTexto(Item, "Empleado");
                oEmpleadoBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
                oEmpleadoBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
                oEmpleadoBE.ModulosIniciados = ServicioLectorFila.LeerEntero(Item, "ModulosIniciados");
                oEmpleadoBE.ModulosCompletados = ServicioLectorFila.LeerEntero(Item, "ModulosCompletados");
                oEmpleadoBE.AvancePromedio = ServicioLectorFila.LeerDecimal(Item, "AvancePromedio");
                oEmpleadoBE.UltimaActividad = ServicioLectorFila.LeerFechaHoraNula(Item, "UltimaActividad");

                ListaBE.Add(oEmpleadoBE);
            }

            return ListaBE;
        }

        private Hashtable ArmarFiltro(BEFiltroRendimiento Objeto, bool conDepartamento)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            if (Objeto.Desde.HasValue)
            {
                Hdatos.Add("@Desde", Objeto.Desde.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (Objeto.Hasta.HasValue)
            {
                Hdatos.Add("@Hasta", Objeto.Hasta.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (conDepartamento && Objeto.DepartamentoId.HasValue && Objeto.DepartamentoId.Value != 0)
            {
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId.Value);
            }

            return Hdatos;
        }
    }
}
