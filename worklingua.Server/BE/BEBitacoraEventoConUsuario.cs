namespace worklingua.Server.BE
{
    public class BEBitacoraEventoConUsuario
    {
        #region Propiedades
        public BEBitacoraEvento Evento { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        #endregion

        public BEBitacoraEventoConUsuario()
        {

        }

        public BEBitacoraEventoConUsuario(BEBitacoraEvento evento, string usuario, string email)
        {
            this.Evento = evento;
            this.Usuario = usuario;
            this.Email = email;
        }
    }
}
