namespace worklingua.Server.BE
{
    public class BESesion
    {
        #region Propiedades
        public int SesionId { get; set; }
        public int UsuarioId { get; set; }
        public Guid Token { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool? Activa { get; set; }
        #endregion

        public BESesion()
        {

        }

        public BESesion(
            int sesionId,
            int usuarioId,
            Guid token,
            DateTime fechaInicio,
            DateTime? fechaFin,
            bool? activa)
        {
            this.SesionId = sesionId;
            this.UsuarioId = usuarioId;
            this.Token = token;
            this.FechaInicio = fechaInicio;
            this.FechaFin = fechaFin;
            this.Activa = activa;
        }
    }
}
