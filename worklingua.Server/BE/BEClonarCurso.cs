using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El cuadro de diálogo del paso 7 de CU-004-002: lo único que se pide para
    /// clonar es el nombre de la copia.
    /// </summary>
    public class BEClonarCurso
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre de la copia es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; }
        #endregion

        public BEClonarCurso()
        {

        }

        public BEClonarCurso(string nombre)
        {
            this.Nombre = nombre;
        }
    }
}
