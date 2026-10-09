using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BECambiarEstadoTicket
    {
        #region Propiedades
        [Required(ErrorMessage = "Falta el estado.")]
        [StringLength(20, ErrorMessage = "El estado no es válido.")]
        public string Estado { get; set; }
        #endregion

        public BECambiarEstadoTicket()
        {

        }

        public BECambiarEstadoTicket(string estado)
        {
            this.Estado = estado;
        }
    }
}
