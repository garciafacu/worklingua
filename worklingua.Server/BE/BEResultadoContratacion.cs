using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEResultadoContratacion
    {
        #region Propiedades
        public BESuscripcion Suscripcion { get; set; }
        public string Plan { get; set; }
        public string Motivo { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }
        public BEFactura Factura { get; set; }
        public List<BEPago> Pagos { get; set; }
        public List<BENotaCreditoDebito> Notas { get; set; }
        #endregion

        public BEResultadoContratacion()
        {
            this.Pagos = new List<BEPago>();
            this.Notas = new List<BENotaCreditoDebito>();
        }

        public BEResultadoContratacion(
            BESuscripcion suscripcion,
            string plan,
            string motivo,
            decimal importe,
            string moneda,
            decimal tasaConversion,
            BEFactura factura,
            List<BEPago> pagos,
            List<BENotaCreditoDebito> notas)
        {
            this.Suscripcion = suscripcion;
            this.Plan = plan;
            this.Motivo = motivo;
            this.Importe = importe;
            this.Moneda = moneda;
            this.TasaConversion = tasaConversion;
            this.Factura = factura;
            this.Pagos = pagos;
            this.Notas = notas;
        }
    }
}
