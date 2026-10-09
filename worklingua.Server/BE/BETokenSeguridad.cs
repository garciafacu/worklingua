namespace worklingua.Server.BE
{
    public class BETokenSeguridad
    {
        #region Propiedades
        public int TokenId { get; set; }
        public int UsuarioId { get; set; }
        public Guid Token { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaUso { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BETokenSeguridad()
        {

        }

        public BETokenSeguridad(
            int tokenId,
            int usuarioId,
            Guid token,
            string tipo,
            DateTime fechaGeneracion,
            DateTime fechaExpiracion,
            DateTime? fechaUso,
            bool activo)
        {
            this.TokenId = tokenId;
            this.UsuarioId = usuarioId;
            this.Token = token;
            this.Tipo = tipo;
            this.FechaGeneracion = fechaGeneracion;
            this.FechaExpiracion = fechaExpiracion;
            this.FechaUso = fechaUso;
            this.Activo = activo;
        }
    }
}
