namespace worklingua.Server.BE
{
    public class BELicencia
    {
        #region Propiedades
        public int LicenciaId { get; set; }
        public int SuscripcionId { get; set; }
        public int? UsuarioId { get; set; }
        public Guid? CodigoLicencia { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estado { get; set; }
        public string EstadoSuscripcion { get; set; }
        #endregion

        public BELicencia()
        {

        }

        public BELicencia(
            int licenciaId,
            int suscripcionId,
            int? usuarioId,
            Guid? codigoLicencia,
            DateTime? fechaAsignacion,
            DateTime? fechaVencimiento,
            string estado,
            string estadoSuscripcion)
        {
            this.LicenciaId = licenciaId;
            this.SuscripcionId = suscripcionId;
            this.UsuarioId = usuarioId;
            this.CodigoLicencia = codigoLicencia;
            this.FechaAsignacion = fechaAsignacion;
            this.FechaVencimiento = fechaVencimiento;
            this.Estado = estado;
            this.EstadoSuscripcion = estadoSuscripcion;
        }
    }
}
