namespace worklingua.Server.BE
{
    public class BEComentario
    {
        #region Propiedades
        public int ComentarioId { get; set; }
        public int UsuarioId { get; set; }
        public int PlanId { get; set; }
        public string Texto { get; set; }
        public int? Puntaje { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BEComentario()
        {

        }

        public BEComentario(
            int comentarioId,
            int usuarioId,
            int planId,
            string texto,
            int? puntaje,
            DateTime fechaAlta,
            bool activo)
        {
            this.ComentarioId = comentarioId;
            this.UsuarioId = usuarioId;
            this.PlanId = planId;
            this.Texto = texto;
            this.Puntaje = puntaje;
            this.FechaAlta = fechaAlta;
            this.Activo = activo;
        }
    }
}
