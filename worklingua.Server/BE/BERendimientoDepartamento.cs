namespace worklingua.Server.BE
{
    public class BERendimientoDepartamento
    {
        #region Propiedades
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
