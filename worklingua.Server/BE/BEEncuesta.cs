namespace worklingua.Server.BE
{
    public class BEEncuesta
    {
        public int EncuestaId { get; set; }
        public int IdiomaId { get; set; }
        public string Pregunta { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaDesde { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public bool Activo { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaAlta { get; set; }

        public BEEncuesta()
        {
        }

        public BEEncuesta(
            int encuestaId,
            int idiomaId,
            string pregunta,
            string descripcion,
            DateOnly fechaDesde,
            DateOnly fechaVencimiento,
            bool activo,
            int usuarioId,
            DateTime fechaAlta)
        {
            this.EncuestaId = encuestaId;
            this.IdiomaId = idiomaId;
            this.Pregunta = pregunta;
            this.Descripcion = descripcion;
            this.FechaDesde = fechaDesde;
            this.FechaVencimiento = fechaVencimiento;
            this.Activo = activo;
            this.UsuarioId = usuarioId;
            this.FechaAlta = fechaAlta;
        }
    }

    public class BEEncuestaConDetalle
    {
        public BEEncuesta Encuesta { get; set; }
        public string Idioma { get; set; }
        public string CodigoISO { get; set; }
        public int TotalRespuestas { get; set; }
        public bool Vigente { get; set; }
        public int? OpcionElegidaId { get; set; }
        public List<BEOpcionEncuesta> Opciones { get; set; } = new List<BEOpcionEncuesta>();
    }

    public class BEGuardarEncuesta
    {
        public int IdiomaId { get; set; }
        public string Pregunta { get; set; }
        public string Descripcion { get; set; }
        public DateOnly? FechaDesde { get; set; }
        public DateOnly? FechaVencimiento { get; set; }
        public bool Activo { get; set; }
        public List<string> Opciones { get; set; } = new List<string>();
    }

    public class BEFiltroEncuesta
    {
        public int? EncuestaId { get; set; }
        public int? IdiomaId { get; set; }
        public string Idioma { get; set; }
        public DateOnly? VigentesAl { get; set; }

        public BEFiltroEncuesta()
        {
        }

        public BEFiltroEncuesta(int? encuestaId, int? idiomaId, DateOnly? vigentesAl)
        {
            this.EncuestaId = encuestaId;
            this.IdiomaId = idiomaId;
            this.VigentesAl = vigentesAl;
        }
    }

    public class BEOpcionEncuesta
    {
        public int OpcionEncuestaId { get; set; }
        public int EncuestaId { get; set; }
        public string Texto { get; set; }
        public int Orden { get; set; }
        public int Total { get; set; }

        public BEOpcionEncuesta()
        {
        }

        public BEOpcionEncuesta(int opcionEncuestaId, int encuestaId, string texto, int orden, int total)
        {
            this.OpcionEncuestaId = opcionEncuestaId;
            this.EncuestaId = encuestaId;
            this.Texto = texto;
            this.Orden = orden;
            this.Total = total;
        }
    }

    public class BEResponderEncuesta
    {
        public int EncuestaId { get; set; }
        public int OpcionEncuestaId { get; set; }

        public BEResponderEncuesta()
        {
        }

        public BEResponderEncuesta(int encuestaId, int opcionEncuestaId)
        {
            this.EncuestaId = encuestaId;
            this.OpcionEncuestaId = opcionEncuestaId;
        }
    }
}
