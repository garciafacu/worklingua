namespace worklingua.Server.BE
{
    public class BEComentarioConAutor
    {
        #region Propiedades
        public BEComentario Comentario { get; set; }
        public string Autor { get; set; }
        public string Plan { get; set; }
        #endregion

        public BEComentarioConAutor()
        {

        }

        public BEComentarioConAutor(BEComentario comentario, string autor, string plan)
        {
            this.Comentario = comentario;
            this.Autor = autor;
            this.Plan = plan;
        }
    }
}
