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

        [StringLength(30, ErrorMessage = "El sector no puede superar los 30 caracteres.")]
        public string Sector { get; set; }

        public int? EtiquetaId { get; set; }

        [BindNever]
        public int? EmpresaId { get; set; }

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
