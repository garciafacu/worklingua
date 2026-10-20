using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
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
