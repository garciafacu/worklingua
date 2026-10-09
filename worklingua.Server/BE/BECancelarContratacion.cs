using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BECancelarContratacion
    {
        #region Propiedades
        public int SuscripcionId { get; set; }

        [Required(ErrorMessage = "Falta la cultura de la operación.")]
        [StringLength(10, ErrorMessage = "El código de cultura no puede superar los 10 caracteres.")]
        public string CodigoCultura { get; set; }
        #endregion

        public BECancelarContratacion()
        {

        }

        public BECancelarContratacion(int suscripcionId, string codigoCultura)
        {
            this.SuscripcionId = suscripcionId;
            this.CodigoCultura = codigoCultura;
        }
    }
}
