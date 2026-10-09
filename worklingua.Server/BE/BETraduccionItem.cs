using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BETraduccionItem
    {
        #region Propiedades
        [Required(ErrorMessage = "La clave de la traducción es obligatoria.")]
        [StringLength(150, ErrorMessage = "La clave no puede superar los 150 caracteres.")]
        public string Clave { get; set; }

        [StringLength(1000, ErrorMessage = "El texto no puede superar los 1000 caracteres.")]
        public string Texto { get; set; }
        #endregion

        public BETraduccionItem()
        {

        }

        public BETraduccionItem(string clave, string texto)
        {
            this.Clave = clave;
            this.Texto = texto;
        }
    }
}
