namespace worklingua.Server.BE
{
    public class BEBitacoraEvento
    {
        public int BitacoraId { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime FechaEvento { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }

        public BEBitacoraEvento()
        {
        }

        public BEBitacoraEvento(
            int bitacoraId,
            int? usuarioId,
            DateTime fechaEvento,
            string modulo,
            string accion,
            string descripcion,
            string nivel)
        {
            this.BitacoraId = bitacoraId;
            this.UsuarioId = usuarioId;
            this.FechaEvento = fechaEvento;
            this.Modulo = modulo;
            this.Accion = accion;
            this.Descripcion = descripcion;
            this.Nivel = nivel;
        }
    }

    public class BEBitacoraEventoConUsuario
    {
        public int BitacoraId { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime FechaEvento { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
    }

    public class BEFiltroBitacora
    {
        public string Texto { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Nivel { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? Pagina { get; set; }
        public int? TamanioPagina { get; set; }
    }

    public class BEPaginaBitacora
    {
        public List<BEBitacoraEventoConUsuario> Registros { get; set; } = new List<BEBitacoraEventoConUsuario>();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
