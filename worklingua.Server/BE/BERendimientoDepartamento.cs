namespace worklingua.Server.BE
{
    /// <summary>Una fila de la comparación entre departamentos.</summary>
    public class BERendimientoDepartamento
    {
        #region Propiedades
        /// <summary>Null agrupa a los empleados sin departamento.</summary>
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public int Empleados { get; set; }
        public int EmpleadosConActividad { get; set; }
        public int ModulosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
        #endregion

        public BERendimientoDepartamento()
        {

        }
    }
}
