namespace worklingua.Server.BE
{
    public class BENoticia
    {
        #region Propiedades
        public int NoticiaId { get; set; }
        public int IdiomaId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BENoticia()
        {

        }

        public BENoticia(
            int noticiaId,
            int idiomaId,
            int usuarioId,
            string titulo,
            string resumen,
            string contenido,
            DateTime fechaPublicacion,
            DateTime fechaAlta,
            bool activo)
        {
            this.NoticiaId = noticiaId;
            this.IdiomaId = idiomaId;
            this.UsuarioId = usuarioId;
            this.Titulo = titulo;
            this.Resumen = resumen;
            this.Contenido = contenido;
            this.FechaPublicacion = fechaPublicacion;
            this.FechaAlta = fechaAlta;
            this.Activo = activo;
        }
    }
}
