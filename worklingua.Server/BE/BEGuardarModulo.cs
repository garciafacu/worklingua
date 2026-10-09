using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarModulo
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre del módulo es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del módulo no puede superar los 150 caracteres.")]
        public string Nombre { get; set; }

        /// <summary>El objetivo del módulo.</summary>
        [StringLength(500, ErrorMessage = "El objetivo no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        /// <summary>El texto de la lección.</summary>
        [StringLength(8000, ErrorMessage = "El contenido no puede superar los 8000 caracteres.")]
        public string Contenido { get; set; }

        [Range(1, 1000, ErrorMessage = "El orden tiene que estar entre 1 y 1000.")]
        public int OrdenModulo { get; set; }
        #endregion

        public BEGuardarModulo()
        {

        }

        public BEGuardarModulo(string nombre, string descripcion, string contenido, int ordenModulo)
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Contenido = contenido;
            this.OrdenModulo = ordenModulo;
        }
    }
}
