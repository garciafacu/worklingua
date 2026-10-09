using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BERestablecerClave
    {
        #region Propiedades
        [Required(ErrorMessage = "El enlace de restablecimiento es inválido.")]
        public Guid Token { get; set; }

        [Required(ErrorMessage = "La clave nueva es obligatoria.")]
        [StringLength(200, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres.")]
        public string ClaveNueva { get; set; }
        #endregion

        public BERestablecerClave()
        {

        }

        public BERestablecerClave(Guid token, string claveNueva)
        {
            this.Token = token;
            this.ClaveNueva = claveNueva;
        }
    }
}
