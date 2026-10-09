namespace worklingua.Server.BE
{
    /// <summary>
    /// Una fila del inventario: un empleado de la empresa, con su licencia si la
    /// tiene. Los empleados sin licencia también viajan, porque son justamente a
    /// quienes se les puede asignar una.
    /// </summary>
    public class BEInventarioLicencia
    {
        #region Propiedades
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public int? LicenciaId { get; set; }
        public Guid? CodigoLicencia { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        #endregion

        public BEInventarioLicencia()
        {

        }

        public BEInventarioLicencia(
            int usuarioId,
            string nombre,
            string apellido,
            string email,
            string departamento,
            int? licenciaId,
            Guid? codigoLicencia,
            DateTime? fechaAsignacion)
        {
            this.UsuarioId = usuarioId;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Email = email;
            this.Departamento = departamento;
            this.LicenciaId = licenciaId;
            this.CodigoLicencia = codigoLicencia;
            this.FechaAsignacion = fechaAsignacion;
        }
    }
}
