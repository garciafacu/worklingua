using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEContratacionRespuesta
    {
        #region Propiedades
        public int SuscripcionId { get; set; }
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }
        public string NumeroFactura { get; set; }
        public List<BEPago> Pagos { get; set; }
        public List<BENotaCreditoDebito> Notas { get; set; }
        #endregion

        public BEContratacionRespuesta()
        {
            this.Pagos = new List<BEPago>();
            this.Notas = new List<BENotaCreditoDebito>();
        }

        public BEContratacionRespuesta(
            int suscripcionId,
            int planId,
            string plan,
            string estado,
            string motivo,
            DateOnly fechaInicio,
            DateOnly? fechaFin,
            decimal importe,
            string moneda,
            decimal tasaConversion,
            string numeroFactura,
            List<BEPago> pagos,
            List<BENotaCreditoDebito> notas)
        {
            this.SuscripcionId = suscripcionId;
            this.PlanId = planId;
            this.Plan = plan;
            this.Estado = estado;
            this.Motivo = motivo;
            this.FechaInicio = fechaInicio;
            this.FechaFin = fechaFin;
            this.Importe = importe;
            this.Moneda = moneda;
            this.TasaConversion = tasaConversion;
            this.NumeroFactura = numeroFactura;
            this.Pagos = pagos;
            this.Notas = notas;
        }
    }
}
