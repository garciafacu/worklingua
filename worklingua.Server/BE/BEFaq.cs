namespace worklingua.Server.BE
{
    public class BECategoriaFaq
    {
        public int CategoriaFaqId { get; set; }
        public int IdiomaId { get; set; }
        public string Titulo { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Idioma { get; set; }
        public string CodigoISO { get; set; }
        public int TotalPreguntas { get; set; }

        public BECategoriaFaq()
        {
        }

        public BECategoriaFaq(
            int categoriaFaqId,
            int idiomaId,
            string titulo,
            int orden,
            bool activo,
            DateTime fechaAlta)
        {
            this.CategoriaFaqId = categoriaFaqId;
            this.IdiomaId = idiomaId;
            this.Titulo = titulo;
            this.Orden = orden;
            this.Activo = activo;
            this.FechaAlta = fechaAlta;
        }
    }

    public class BECategoriaFaqConPreguntas
    {
        public BECategoriaFaq Categoria { get; set; }
        public List<BEPreguntaFrecuente> Preguntas { get; set; } = new List<BEPreguntaFrecuente>();
    }

    public class BEGuardarCategoriaFaq
    {
        public int IdiomaId { get; set; }
        public string Titulo { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }

    public class BEPreguntaFrecuente
    {
        public int PreguntaFrecuenteId { get; set; }
        public int CategoriaFaqId { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
        public string Enlace { get; set; }
        public string EnlaceEtiqueta { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Categoria { get; set; }
        public int IdiomaId { get; set; }

        public BEPreguntaFrecuente()
        {
        }

        public BEPreguntaFrecuente(
            int preguntaFrecuenteId,
            int categoriaFaqId,
            string pregunta,
            string respuesta,
            string enlace,
            string enlaceEtiqueta,
            int orden,
            bool activo,
            DateTime fechaAlta)
        {
            this.PreguntaFrecuenteId = preguntaFrecuenteId;
            this.CategoriaFaqId = categoriaFaqId;
            this.Pregunta = pregunta;
            this.Respuesta = respuesta;
            this.Enlace = enlace;
            this.EnlaceEtiqueta = enlaceEtiqueta;
            this.Orden = orden;
            this.Activo = activo;
            this.FechaAlta = fechaAlta;
        }
    }

    public class BEGuardarPreguntaFrecuente
    {
        public int CategoriaFaqId { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
        public string Enlace { get; set; }
        public string EnlaceEtiqueta { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }

    public class BEFiltroFaq
    {
        public int? CategoriaFaqId { get; set; }
        public int? PreguntaFrecuenteId { get; set; }
        public int? IdiomaId { get; set; }
        public string Idioma { get; set; }
        public bool SoloActivas { get; set; }

        public BEFiltroFaq()
        {
        }

        public BEFiltroFaq(int? categoriaFaqId, int? preguntaFrecuenteId, int? idiomaId, bool soloActivas)
        {
            this.CategoriaFaqId = categoriaFaqId;
            this.PreguntaFrecuenteId = preguntaFrecuenteId;
            this.IdiomaId = idiomaId;
            this.SoloActivas = soloActivas;
        }
    }

    public class BEConsultaPublicaRespuesta
    {
        public string Mensaje { get; set; }
        public int TicketId { get; set; }
        public Guid Token { get; set; }

        public BEConsultaPublicaRespuesta()
        {
        }

        public BEConsultaPublicaRespuesta(string mensaje, int ticketId, Guid token)
        {
            this.Mensaje = mensaje;
            this.TicketId = ticketId;
            this.Token = token;
        }
    }

    public class BEResponderConsultaPublica
    {
        public Guid? Token { get; set; }
        public string Texto { get; set; }
    }

    public class BEEnviarConsultaContacto
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public string TokenCaptcha { get; set; }
        public string CodigoIdioma { get; set; }

        public BEEnviarConsultaContacto()
        {
        }

        public BEEnviarConsultaContacto(string nombre, string email, string asunto, string mensaje)
        {
            this.Nombre = nombre;
            this.Email = email;
            this.Asunto = asunto;
            this.Mensaje = mensaje;
        }
    }
}
