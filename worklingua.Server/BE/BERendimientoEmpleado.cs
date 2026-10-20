namespace worklingua.Server.BE
{
    public class BERendimientoEmpleado
    {
        #region Propiedades
        public int UsuarioId { get; set; }
        public string Empleado { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public int ModulosIniciados { get; set; }
        public int ModulosCompletados { get; set; }
        public decimal AvancePromedio { get; set; }
        public DateTime? UltimaActividad { get; set; }
        #endregion

        public BERendimientoEmpleado()
        {

        }
    }
}
