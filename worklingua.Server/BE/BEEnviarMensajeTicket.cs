using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEEnviarMensajeTicket
    {
        #region Propiedades
        [Required(ErrorMessage = "Escribí el mensaje.")]
        [StringLength(2000, ErrorMessage = "El mensaje no puede superar los 2000 caracteres.")]
        public string Texto { get; set; }
        #endregion

        public BEEnviarMensajeTicket()
        {

        }

        public BEEnviarMensajeTicket(string texto)
        {
            this.Texto = texto;
        }
    }
}
