using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarPermiso
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre de la familia es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
        public string Descripcion { get; set; }
        #endregion

        public BEGuardarPermiso()
        {

        }

        public BEGuardarPermiso(string nombre, string descripcion)
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
        }
    }
}
