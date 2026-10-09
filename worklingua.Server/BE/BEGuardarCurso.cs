using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarCurso
    {
        #region Propiedades
        [Required(ErrorMessage = "Indicá el idioma que enseña el curso.")]
        [StringLength(50, ErrorMessage = "El idioma no puede superar los 50 caracteres.")]
        public string Idioma { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El nivel es obligatorio.")]
        [StringLength(20, ErrorMessage = "El nivel no puede superar los 20 caracteres.")]
        public string Nivel { get; set; }

        [Range(1, 10000, ErrorMessage = "La duración debe estar entre 1 y 10000 horas.")]
        public int? DuracionHoras { get; set; }

        /// <summary>Categoría sectorial (CU-004-005). Vacío es "sin clasificar".</summary>
        [StringLength(30, ErrorMessage = "El sector no puede superar los 30 caracteres.")]
        public string Sector { get; set; }

        /// <summary>
        /// Ventana de despliegue (CU-004-006). Las dos vacías significan que el
        /// curso está disponible siempre.
        /// </summary>
        public DateOnly? FechaPublicacion { get; set; }
        public DateOnly? FechaFin { get; set; }

        /// <summary>
        /// Etiquetas del curso. Tienen que existir en el diccionario: crearlas
        /// es una confirmación aparte (camino alternativo 1).
        /// </summary>
        public List<int> EtiquetaIds { get; set; }
        #endregion

        public BEGuardarCurso()
        {

        }

        public BEGuardarCurso(string idioma, string nombre, string descripcion, string nivel, int? duracionHoras)
        {
            this.Idioma = idioma;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Nivel = nivel;
            this.DuracionHoras = duracionHoras;
        }
    }
}
