namespace worklingua.Server.BE
{
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
