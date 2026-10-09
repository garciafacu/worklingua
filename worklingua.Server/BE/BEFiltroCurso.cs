using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace worklingua.Server.BE
{
    public class BEFiltroCurso
    {
        #region Propiedades
        [StringLength(150, ErrorMessage = "El término de búsqueda no puede superar los 150 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(50, ErrorMessage = "El idioma no puede superar los 50 caracteres.")]
        public string Idioma { get; set; }

        [StringLength(20, ErrorMessage = "El nivel no puede superar los 20 caracteres.")]
        public string Nivel { get; set; }

        /// <summary>Categoría sectorial (CU-004-005). La BLL la normaliza.</summary>
        [StringLength(30, ErrorMessage = "El sector no puede superar los 30 caracteres.")]
        public string Sector { get; set; }

        /// <summary>Etiqueta por la que filtrar; cero o null no filtra.</summary>
        public int? EtiquetaId { get; set; }

        /// <summary>
        /// Alcance: null ve todos los cursos; con valor, los globales y los de esa
        /// empresa. Lo fija siempre la BLL a partir de la sesión.
        /// </summary>
        [BindNever]
        public int? EmpresaId { get; set; }

        /// <summary>
        /// Incluye los cursos fuera de su ventana de despliegue (CU-004-006).
        /// Solo lo pone el Backoffice: el catálogo nunca ofrece un curso que no
        /// está publicado.
        /// </summary>
        [BindNever]
        public bool IncluirFueraDeVentana { get; set; }
        #endregion

        public BEFiltroCurso()
        {

        }

        public BEFiltroCurso(string nombre, string idioma, string nivel)
        {
            this.Nombre = nombre;
            this.Idioma = idioma;
            this.Nivel = nivel;
        }
    }
}
