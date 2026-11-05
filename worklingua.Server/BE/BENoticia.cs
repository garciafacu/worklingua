namespace worklingua.Server.BE
{
    public class BENoticia
    {
        public int NoticiaId { get; set; }
        public int IdiomaId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
    }

    public class BENoticiaRespuesta
    {
        public int NoticiaId { get; set; }
        public int IdiomaId { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }

    public class BEGuardarNoticia
    {
        public int IdiomaId { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Contenido { get; set; }
        public DateTime? FechaPublicacion { get; set; }
    }

    public class BEFiltroNoticia
    {
        public string Idioma { get; set; }
        public int? IdiomaId { get; set; }

        public BEFiltroNoticia()
        {
        }

        public BEFiltroNoticia(string idioma, int? idiomaId)
        {
            this.Idioma = idioma;
            this.IdiomaId = idiomaId;
        }
    }

    public class BESuscriptorNewsletter
    {
        public int SuscriptorId { get; set; }
        public string Email { get; set; }
        public int IdiomaId { get; set; }
        public Guid Token { get; set; }
        public bool Confirmado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }
    }

    public class BESuscriptorNewsletterRespuesta
    {
        public int SuscriptorId { get; set; }
        public string Email { get; set; }
        public int IdiomaId { get; set; }
        public bool Confirmado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }

        public BESuscriptorNewsletterRespuesta()
        {
        }

        public BESuscriptorNewsletterRespuesta(
            int suscriptorId,
            string email,
            int idiomaId,
            bool confirmado,
            DateTime fechaAlta,
            DateTime? fechaConfirmacion,
            DateTime? fechaBaja,
            bool activo)
        {
            this.SuscriptorId = suscriptorId;
            this.Email = email;
            this.IdiomaId = idiomaId;
            this.Confirmado = confirmado;
            this.FechaAlta = fechaAlta;
            this.FechaConfirmacion = fechaConfirmacion;
            this.FechaBaja = fechaBaja;
            this.Activo = activo;
        }
    }

    public class BESuscribirNewsletter
    {
        public string Email { get; set; }
        public string Idioma { get; set; }
    }

    public class BEEnviarNewsletter
    {
        public int IdiomaId { get; set; }
        public string Asunto { get; set; }
        public List<int> NoticiaIds { get; set; }
    }

    public class BEEnvioNewsletter
    {
        public int EnvioId { get; set; }
        public int IdiomaId { get; set; }
        public int UsuarioId { get; set; }
        public string Asunto { get; set; }
        public DateTime FechaEnvio { get; set; }
        public int CantidadEnviados { get; set; }
        public int CantidadFallidos { get; set; }

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

    public class BEVistaPreviaNewsletterRespuesta
    {
        public string Html { get; set; }
        public int CantidadDestinatarios { get; set; }

        public BEVistaPreviaNewsletterRespuesta()
        {
        }

        public BEVistaPreviaNewsletterRespuesta(string html, int cantidadDestinatarios)
        {
            this.Html = html;
            this.CantidadDestinatarios = cantidadDestinatarios;
        }
    }
}
