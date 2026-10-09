namespace worklingua.Server.BE
{
    public class BEEncuesta
    {
        #region Propiedades
        public int EncuestaId { get; set; }
        public int IdiomaId { get; set; }
        public string Pregunta { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaDesde { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public bool Activo { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaAlta { get; set; }
        #endregion

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
}
