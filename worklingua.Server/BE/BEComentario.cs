namespace worklingua.Server.BE
{
    public class BEComentario
    {
        public int ComentarioId { get; set; }
        public int UsuarioId { get; set; }
        public int PlanId { get; set; }
        public string Texto { get; set; }
        public int? Puntaje { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
    }

    public class BEComentarioConAutor
    {
        public BEComentario Comentario { get; set; }
        public string Autor { get; set; }
        public string Plan { get; set; }

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

    public class BEComentarioRespuesta
    {
        public int ComentarioId { get; set; }
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string Autor { get; set; }
        public string Texto { get; set; }
        public int? Puntaje { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool EsPropio { get; set; }
        public bool PuedeEliminar { get; set; }
    }

    public class BECrearComentario
    {
        public int PlanId { get; set; }
        public int Puntaje { get; set; }
        public string Texto { get; set; }
    }

    public class BEFiltroComentario
    {
        public int? PlanId { get; set; }
        public string Texto { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }

        public BEFiltroComentario()
        {
        }

        public BEFiltroComentario(int? planId, string texto, DateTime? desde, DateTime? hasta)
        {
            this.PlanId = planId;
            this.Texto = texto;
            this.Desde = desde;
            this.Hasta = hasta;
        }
    }

    public class BEResumenValoracion
    {
        public int PlanId { get; set; }
        public decimal Promedio { get; set; }
        public int Cantidad { get; set; }
    }
}
