namespace worklingua.Server.BE
{
    public class BEEnvioNewsletter
    {
        #region Propiedades
        public int EnvioId { get; set; }
        public int IdiomaId { get; set; }
        public int UsuarioId { get; set; }
        public string Asunto { get; set; }
        public DateTime FechaEnvio { get; set; }
        public int CantidadEnviados { get; set; }
        public int CantidadFallidos { get; set; }
        #endregion

        public BEEnvioNewsletter()
        {

        }

        public BEEnvioNewsletter(
            int envioId,
            int idiomaId,
            int usuarioId,
            string asunto,
            DateTime fechaEnvio,
            int cantidadEnviados,
            int cantidadFallidos)
        {
            this.EnvioId = envioId;
            this.IdiomaId = idiomaId;
            this.UsuarioId = usuarioId;
            this.Asunto = asunto;
            this.FechaEnvio = fechaEnvio;
            this.CantidadEnviados = cantidadEnviados;
            this.CantidadFallidos = cantidadFallidos;
        }
    }
}
