namespace worklingua.Server.BE
{
    public class BESuscriptorNewsletter
    {
        #region Propiedades
        public int SuscriptorId { get; set; }
        public string Email { get; set; }
        public int IdiomaId { get; set; }
        public Guid Token { get; set; }
        public bool Confirmado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BESuscriptorNewsletter()
        {

        }

        public BESuscriptorNewsletter(
            int suscriptorId,
            string email,
            int idiomaId,
            Guid token,
            bool confirmado,
            DateTime fechaAlta,
            DateTime? fechaConfirmacion,
            DateTime? fechaBaja,
            bool activo)
        {
            this.SuscriptorId = suscriptorId;
            this.Email = email;
            this.IdiomaId = idiomaId;
            this.Token = token;
            this.Confirmado = confirmado;
            this.FechaAlta = fechaAlta;
            this.FechaConfirmacion = fechaConfirmacion;
            this.FechaBaja = fechaBaja;
            this.Activo = activo;
        }
    }
}
