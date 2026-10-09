using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEContratarPlan
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí un plan.")]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "Falta la cultura de la operación.")]
        [StringLength(10, ErrorMessage = "El código de cultura no puede superar los 10 caracteres.")]
        public string CodigoCultura { get; set; }

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El importe con nota de crédito no es válido.")]
        public decimal ImporteNotaCredito { get; set; }

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El importe en cuenta corriente no es válido.")]
        public decimal ImporteCuentaCorriente { get; set; }

        public BETarjeta Tarjeta { get; set; }
        #endregion

        public BEContratarPlan()
        {

        }

        public BEContratarPlan(
            int planId,
            string codigoCultura,
            decimal importeNotaCredito,
            decimal importeCuentaCorriente,
            BETarjeta tarjeta)
        {
            this.PlanId = planId;
            this.CodigoCultura = codigoCultura;
            this.ImporteNotaCredito = importeNotaCredito;
            this.ImporteCuentaCorriente = importeCuentaCorriente;
            this.Tarjeta = tarjeta;
        }
    }
}
