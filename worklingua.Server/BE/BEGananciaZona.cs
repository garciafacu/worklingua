namespace worklingua.Server.BE
{
    /// <summary>
    /// Ganancias agrupadas por zona: la provincia de la empresa cliente, o
    /// "Sin zona" cuando la empresa no la tiene cargada.
    /// </summary>
    public class BEGananciaZona
    {
        #region Propiedades
        public string Zona { get; set; }
        public decimal Facturado { get; set; }
        public decimal Cobrado { get; set; }
        public decimal NotasCredito { get; set; }
        public decimal Neto { get; set; }
        public int Empresas { get; set; }
        #endregion

        public BEGananciaZona()
        {

        }

        public BEGananciaZona(
            string zona,
            decimal facturado,
            decimal cobrado,
            decimal notasCredito,
            decimal neto,
            int empresas)
        {
            this.Zona = zona;
            this.Facturado = facturado;
            this.Cobrado = cobrado;
            this.NotasCredito = notasCredito;
            this.Neto = neto;
            this.Empresas = empresas;
        }
    }
}
