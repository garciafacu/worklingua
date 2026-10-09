namespace worklingua.Server.BE
{
    public class BEBitacoraEvento
    {
        #region Propiedades
        public int BitacoraId { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime FechaEvento { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        #endregion

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
}
