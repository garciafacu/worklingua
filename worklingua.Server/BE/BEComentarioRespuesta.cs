namespace worklingua.Server.BE
{
    public class BEComentarioRespuesta
    {
        #region Propiedades
        public int ComentarioId { get; set; }
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string Autor { get; set; }
        public string Texto { get; set; }
        public int? Puntaje { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool EsPropio { get; set; }
        public bool PuedeEliminar { get; set; }
        #endregion

        public BEComentarioRespuesta()
        {

        }

        public BEComentarioRespuesta(
            int comentarioId,
            int planId,
            string plan,
            string autor,
            string texto,
            int? puntaje,
            DateTime fechaAlta,
            bool esPropio,
            bool puedeEliminar)
        {
            this.ComentarioId = comentarioId;
            this.PlanId = planId;
            this.Plan = plan;
            this.Autor = autor;
            this.Texto = texto;
            this.Puntaje = puntaje;
            this.FechaAlta = fechaAlta;
            this.EsPropio = esPropio;
            this.PuedeEliminar = puedeEliminar;
        }
    }
}
