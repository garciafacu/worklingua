using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BERecuperarClave
    {
        #region Propiedades
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; }
        #endregion

        public BERecuperarClave()
        {

        }

        public BERecuperarClave(string email)
        {
            this.Email = email;
        }
    }
}
