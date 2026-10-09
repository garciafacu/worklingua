namespace worklingua.Server.BE
{
    /// <summary>
    /// Una fila del reporte de ganancias. <see cref="Clave"/> identifica el
    /// período según la agrupación pedida (2026-09-22, 2026-W39, 2026-09 o 2026)
    /// y la interfaz la formatea para mostrarla.
    /// </summary>
    public class BEGananciaPeriodo
    {
        #region Propiedades
        public string Clave { get; set; }
        public decimal Facturado { get; set; }
        public decimal Cobrado { get; set; }
        public decimal NotasCredito { get; set; }
        public decimal Neto { get; set; }
        #endregion

        public BEGananciaPeriodo()
        {

        }

        public BEGananciaPeriodo(string clave, decimal facturado, decimal cobrado, decimal notasCredito, decimal neto)
        {
            this.Clave = clave;
            this.Facturado = facturado;
            this.Cobrado = cobrado;
            this.NotasCredito = notasCredito;
            this.Neto = neto;
        }
    }
}
