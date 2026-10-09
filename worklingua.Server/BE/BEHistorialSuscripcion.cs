namespace worklingua.Server.BE
{
    public class BEHistorialSuscripcion
    {
        #region Propiedades
        public int HistorialId { get; set; }
        public int SuscripcionId { get; set; }
        public DateTime? FechaMovimiento { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string Observacion { get; set; }
        #endregion

        public BEHistorialSuscripcion()
        {

        }

        public BEHistorialSuscripcion(
            int historialId,
            int suscripcionId,
            DateTime? fechaMovimiento,
            string estadoAnterior,
            string estadoNuevo,
            string observacion)
        {
            this.HistorialId = historialId;
            this.SuscripcionId = suscripcionId;
            this.FechaMovimiento = fechaMovimiento;
            this.EstadoAnterior = estadoAnterior;
            this.EstadoNuevo = estadoNuevo;
            this.Observacion = observacion;
        }
    }
}
