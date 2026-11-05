namespace worklingua.Server.BE
{
    public class BEGananciaPeriodo
    {
        public string Clave { get; set; }
        public decimal Facturado { get; set; }
        public decimal Cobrado { get; set; }
        public decimal NotasCredito { get; set; }
        public decimal Neto { get; set; }

        public BEGananciaPeriodo()
        {
        }

        public BEGananciaPeriodo(string clave, decimal facturado, decimal cobrado, decimal notasCredito, decimal neto)
        {
            this.Clave = clave;
            this.Facturado = facturado;
            this.Cobrado = cobrado;
            this.NotasCredito = notasCredito;
            this.Neto = neto;
        }
    }

    public class BEGananciaZona
    {
        public string Zona { get; set; }
        public decimal Facturado { get; set; }
        public decimal Cobrado { get; set; }
        public decimal NotasCredito { get; set; }
        public decimal Neto { get; set; }
        public int Empresas { get; set; }

        public BEGananciaZona()
        {
        }

        public BEGananciaZona(
            string zona,
            decimal facturado,
            decimal cobrado,
            decimal notasCredito,
            decimal neto,
            int empresas)
        {
            this.Zona = zona;
            this.Facturado = facturado;
            this.Cobrado = cobrado;
            this.NotasCredito = notasCredito;
            this.Neto = neto;
            this.Empresas = empresas;
        }
    }

    public class BEFiltroReporte
    {
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        public string Agrupacion { get; set; }
        public string Provincia { get; set; }
    }

    public class BETablero
    {
        public decimal IngresosMes { get; set; }
        public decimal IngresosMesAnterior { get; set; }
        public decimal DeudaTotal { get; set; }
        public int EmpresasConDeuda { get; set; }
        public int EmpresasActivas { get; set; }
        public int ContratacionesActivas { get; set; }
        public int LicenciasAsignadas { get; set; }
        public int LicenciasContratadas { get; set; }
        public int EmpresasSinCupo { get; set; }
        public List<BEContratacionPorPlan> Planes { get; set; } = new List<BEContratacionPorPlan>();
    }

    public class BERendimiento
    {
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public BERendimientoResumen Resumen { get; set; } = new BERendimientoResumen();
        public List<BERendimientoDepartamento> Departamentos { get; set; } = new List<BERendimientoDepartamento>();
        public List<BERendimientoEmpleado> Empleados { get; set; } = new List<BERendimientoEmpleado>();
    }

    public class BERendimientoDepartamento
    {
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public int Empleados { get; set; }
        public int EmpleadosConActividad { get; set; }
        public int ModulosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
    }

    public class BERendimientoEmpleado
    {
        public int UsuarioId { get; set; }
        public string Empleado { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public int ModulosIniciados { get; set; }
        public int ModulosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
        public DateTime? UltimaActividad { get; set; }
    }

    public class BERendimientoResumen
    {
        public int EmpleadosConActividad { get; set; }
        public int ModulosIniciados { get; set; }
        public int ModulosCompletados { get; set; }
        public int CursosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
        public decimal? Facturado { get; set; }
        public decimal? CostoPorModulo { get; set; }
        public decimal? CostoPorEmpleado { get; set; }
    }

    public class BEFiltroRendimiento
    {
        public int EmpresaId { get; set; }
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        public int? DepartamentoId { get; set; }
    }
}
