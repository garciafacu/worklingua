namespace worklingua.Server.BE
{
    /// <summary>
    /// Las cifras de cabecera del panel.
    ///
    /// El caso de uso pide "retorno de inversión", pero la plataforma no tiene
    /// ningún dato del retorno. En su lugar van las de costo, que salen de lo
    /// facturado y del progreso real.
    /// </summary>
    public class BERendimientoResumen
    {
        #region Propiedades
        public int EmpleadosConActividad { get; set; }
        public int ModulosIniciados { get; set; }
        public int ModulosCompletados { get; set; }
        public int CursosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
        /// <summary>
        /// Lo facturado se cobra por empresa, no por departamento: con un
        /// departamento elegido los tres importes viajan en null y el panel no
        /// los muestra, en vez de atribuirle a un sector la inversión de toda
        /// la empresa.
        /// </summary>
        public decimal? Facturado { get; set; }
        /// <summary>Cero cuando todavía no se completó ningún módulo.</summary>
        public decimal? CostoPorModulo { get; set; }
        public decimal? CostoPorEmpleado { get; set; }
        #endregion

        public BERendimientoResumen()
        {

        }
    }
}
