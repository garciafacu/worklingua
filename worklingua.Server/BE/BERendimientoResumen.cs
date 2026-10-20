namespace worklingua.Server.BE
{
    public class BERendimientoResumen
    {
        #region Propiedades
        public int EmpleadosConActividad { get; set; }
        public int ModulosIniciados { get; set; }
        public int ModulosCompletados { get; set; }
        public int CursosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
        public decimal? Facturado { get; set; }
        public decimal? CostoPorModulo { get; set; }
        public decimal? CostoPorEmpleado { get; set; }
        #endregion

        public BERendimientoResumen()
        {

        }
    }
}
