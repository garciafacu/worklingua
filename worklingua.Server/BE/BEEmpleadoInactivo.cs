namespace worklingua.Server.BE
{
    /// <summary>
    /// Un empleado que cumple la condición de inactividad de una alerta.
    /// Es quien recibe la notificación.
    /// </summary>
    public class BEEmpleadoInactivo
    {
        #region Propiedades
        public int UsuarioId { get; set; }
        public string Empleado { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public int DiasSinActividad { get; set; }
        #endregion

        public BEEmpleadoInactivo()
        {

        }

        public BEEmpleadoInactivo(
            int usuarioId, string empleado, string email, string departamento, int diasSinActividad)
        {
            this.UsuarioId = usuarioId;
            this.Empleado = empleado;
            this.Email = email;
            this.Departamento = departamento;
            this.DiasSinActividad = diasSinActividad;
        }
    }
}
