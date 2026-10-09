using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BECompletarInvitacion
    {
        #region Propiedades
        [Required(ErrorMessage = "El enlace de invitación es inválido.")]
        public Guid Token { get; set; }

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(200, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres.")]
        public string Clave { get; set; }
        #endregion

        public BECompletarInvitacion()
        {

        }

        public BECompletarInvitacion(Guid token, string clave)
        {
            this.Token = token;
            this.Clave = clave;
        }
    }
}
