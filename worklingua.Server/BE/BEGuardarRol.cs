using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarRol
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string Descripcion { get; set; }
        #endregion

        public BEGuardarRol()
        {

        }

        public BEGuardarRol(string nombre, string descripcion)
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
        }
    }
}
