using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarIdioma
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre del idioma es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El código ISO es obligatorio.")]
        [StringLength(5, MinimumLength = 2, ErrorMessage = "El código ISO debe tener entre 2 y 5 caracteres.")]
        public string CodigoISO { get; set; }
        #endregion

        public BEGuardarIdioma()
        {

        }

        public BEGuardarIdioma(string nombre, string codigoISO)
        {
            this.Nombre = nombre;
            this.CodigoISO = codigoISO;
        }
    }
}
