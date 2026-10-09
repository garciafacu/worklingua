using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BELogin
    {
        #region Propiedades
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La clave es obligatoria.")]
        public string Clave { get; set; }
        #endregion

        public BELogin()
        {

        }

        public BELogin(string email, string clave)
        {
            this.Email = email;
            this.Clave = clave;
        }
    }
}
