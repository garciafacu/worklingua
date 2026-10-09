using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BESuscribirNewsletter
    {
        #region Propiedades
        [Required(ErrorMessage = "Ingresá tu correo.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string Email { get; set; }

        [StringLength(5, ErrorMessage = "El código de idioma no puede superar los 5 caracteres.")]
        public string Idioma { get; set; }
        #endregion

        public BESuscribirNewsletter()
        {

        }

        public BESuscribirNewsletter(string email, string idioma)
        {
            this.Email = email;
            this.Idioma = idioma;
        }
    }
}
