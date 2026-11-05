namespace worklingua.Server.BE
{
    public class BETicket
    {
        public int TicketId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public int SuscripcionId { get; set; }
        public int? CursoId { get; set; }
        public string Asunto { get; set; }
        public string Estado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime FechaUltimoMovimiento { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? IdiomaId { get; set; }

        public BETicket()
        {
        }

        public BETicket(
            int ticketId,
            int empresaId,
            int usuarioId,
            int suscripcionId,
            int? cursoId,
            string asunto,
            string estado,
            DateTime fechaAlta,
            DateTime fechaUltimoMovimiento,
            DateTime? fechaCierre,
            int? idiomaId)
        {
            this.TicketId = ticketId;
            this.EmpresaId = empresaId;
            this.UsuarioId = usuarioId;
            this.SuscripcionId = suscripcionId;
            this.CursoId = cursoId;
            this.Asunto = asunto;
            this.Estado = estado;
            this.FechaAlta = fechaAlta;
            this.FechaUltimoMovimiento = fechaUltimoMovimiento;
            this.FechaCierre = fechaCierre;
            this.IdiomaId = idiomaId;
        }
    }

    public class BETicketConDetalle
    {
        public BETicket Ticket { get; set; }
        public string CodigoIdioma { get; set; }
        public string Empresa { get; set; }
        public string Autor { get; set; }
        public string AutorNombre { get; set; }
        public string AutorEmail { get; set; }
        public string Plan { get; set; }
        public string EstadoSuscripcion { get; set; }
        public string Curso { get; set; }
        public int CantidadMensajes { get; set; }
        public List<BEMensajeTicketConAutor> Mensajes { get; set; }

        public BETicketConDetalle()
        {
            this.Mensajes = new List<BEMensajeTicketConAutor>();
        }

        public BETicketConDetalle(
            BETicket ticket,
            string codigoIdioma,
            string empresa,
            string autor,
            string autorNombre,
            string autorEmail,
            string plan,
            string estadoSuscripcion,
            string curso,
            int cantidadMensajes,
            List<BEMensajeTicketConAutor> mensajes)
        {
            this.Ticket = ticket;
            this.CodigoIdioma = codigoIdioma;
            this.Empresa = empresa;
            this.Autor = autor;
            this.AutorNombre = autorNombre;
            this.AutorEmail = autorEmail;
            this.Plan = plan;
            this.EstadoSuscripcion = estadoSuscripcion;
            this.Curso = curso;
            this.CantidadMensajes = cantidadMensajes;
            this.Mensajes = mensajes;
        }
    }

    public class BECrearTicket
    {
        public int SuscripcionId { get; set; }
        public int? CursoId { get; set; }
        public string Asunto { get; set; }
        public string Texto { get; set; }
        public string CodigoIdioma { get; set; }
    }

    public class BEMensajeTicket
    {
        public int MensajeId { get; set; }
        public int TicketId { get; set; }
        public int UsuarioId { get; set; }
        public bool EsOperador { get; set; }
        public string Texto { get; set; }
        public DateTime FechaEnvio { get; set; }

        public BEMensajeTicket()
        {
        }

        public BEMensajeTicket(
            int mensajeId,
            int ticketId,
            int usuarioId,
            bool esOperador,
            string texto,
            DateTime fechaEnvio)
        {
            this.MensajeId = mensajeId;
            this.TicketId = ticketId;
            this.UsuarioId = usuarioId;
            this.EsOperador = esOperador;
            this.Texto = texto;
            this.FechaEnvio = fechaEnvio;
        }
    }

    public class BEMensajeTicketConAutor
    {
        public BEMensajeTicket Mensaje { get; set; }
        public string Autor { get; set; }

        public BEMensajeTicketConAutor()
        {
        }

        public BEMensajeTicketConAutor(BEMensajeTicket mensaje, string autor)
        {
            this.Mensaje = mensaje;
            this.Autor = autor;
        }
    }

    public class BEEnviarMensajeTicket
    {
        public string Texto { get; set; }
    }

    public class BEFiltroTicket
    {
        public string Estado { get; set; }
        public int? EmpresaId { get; set; }
        public int? UsuarioId { get; set; }
        public int? TicketId { get; set; }
        public bool Administracion { get; set; }

        public BEFiltroTicket()
        {
        }

        public BEFiltroTicket(string estado, int? empresaId, int? usuarioId, int? ticketId, bool administracion)
        {
            this.Estado = estado;
            this.EmpresaId = empresaId;
            this.UsuarioId = usuarioId;
            this.TicketId = ticketId;
            this.Administracion = administracion;
        }
    }

    public class BEOpcionesTicket
    {
        public List<BESuscripcionConPlan> Contrataciones { get; set; }
        public List<BECurso> Cursos { get; set; }

        public BEOpcionesTicket()
        {
            this.Contrataciones = new List<BESuscripcionConPlan>();
            this.Cursos = new List<BECurso>();
        }

        public BEOpcionesTicket(List<BESuscripcionConPlan> contrataciones, List<BECurso> cursos)
        {
            this.Contrataciones = contrataciones;
            this.Cursos = cursos;
        }
    }

    public class BEOferta
    {
        public int OfertaId { get; set; }
        public int EmpresaId { get; set; }
        public int? PlanId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public bool Activo { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaAlta { get; set; }

        public BEOferta()
        {
        }

        public BEOferta(
            int ofertaId,
            int empresaId,
            int? planId,
            string titulo,
            string descripcion,
            DateOnly fechaDesde,
            DateOnly? fechaHasta,
            bool activo,
            int usuarioId,
            DateTime fechaAlta)
        {
            this.OfertaId = ofertaId;
            this.EmpresaId = empresaId;
            this.PlanId = planId;
            this.Titulo = titulo;
            this.Descripcion = descripcion;
            this.FechaDesde = fechaDesde;
            this.FechaHasta = fechaHasta;
            this.Activo = activo;
            this.UsuarioId = usuarioId;
            this.FechaAlta = fechaAlta;
        }
    }

    public class BEOfertaConDetalle
    {
        public BEOferta Oferta { get; set; }
        public string Empresa { get; set; }
        public string Plan { get; set; }
        public bool PlanActivo { get; set; }

        public BEOfertaConDetalle()
        {
        }

        public BEOfertaConDetalle(BEOferta oferta, string empresa, string plan, bool planActivo)
        {
            this.Oferta = oferta;
            this.Empresa = empresa;
            this.Plan = plan;
            this.PlanActivo = planActivo;
        }
    }

    public class BEGuardarOferta
    {
        public int EmpresaId { get; set; }
        public int? PlanId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateOnly? FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public bool Activo { get; set; }
    }

    public class BEFiltroOferta
    {
        public int? OfertaId { get; set; }
        public int? EmpresaId { get; set; }
        public DateOnly? VigentesAl { get; set; }

        public BEFiltroOferta()
        {
        }

        public BEFiltroOferta(int? ofertaId, int? empresaId, DateOnly? vigentesAl)
        {
            this.OfertaId = ofertaId;
            this.EmpresaId = empresaId;
            this.VigentesAl = vigentesAl;
        }
    }
}
