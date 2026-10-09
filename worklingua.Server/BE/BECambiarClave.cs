using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BECambiarClave
    {
        #region Propiedades
        [Required(ErrorMessage = "La clave actual es obligatoria.")]
        public string ClaveActual { get; set; }

        [Required(ErrorMessage = "La clave nueva es obligatoria.")]
        [StringLength(200, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres.")]
        public string ClaveNueva { get; set; }
        #endregion

        public BECambiarClave()
        {

        }

        public BECambiarClave(string claveActual, string claveNueva)
        {
            this.ClaveActual = claveActual;
            this.ClaveNueva = claveNueva;
        }
    }
}
