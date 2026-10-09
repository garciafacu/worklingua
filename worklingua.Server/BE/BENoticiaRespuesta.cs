namespace worklingua.Server.BE
{
    public class BENoticiaRespuesta
    {
        #region Propiedades
        public int NoticiaId { get; set; }
        public int IdiomaId { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaPublicacion { get; set; }
        #endregion

        public BENoticiaRespuesta()
        {

        }

        public BENoticiaRespuesta(
            int noticiaId,
            int idiomaId,
            string titulo,
            string resumen,
            string contenido,
            DateTime fechaPublicacion)
        {
            this.NoticiaId = noticiaId;
            this.IdiomaId = idiomaId;
            this.Titulo = titulo;
            this.Resumen = resumen;
            this.Contenido = contenido;
            this.FechaPublicacion = fechaPublicacion;
        }
    }
}
