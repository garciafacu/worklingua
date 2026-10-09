namespace worklingua.Server.BE
{
    /// <summary>
    /// El detalle por empleado. Viajan todos los activos, hayan tocado un curso
    /// o no: quien no empezó nada es lo más accionable del panel.
    /// </summary>
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
