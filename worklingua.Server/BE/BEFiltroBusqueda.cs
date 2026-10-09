using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEFiltroBusqueda
    {
        #region Propiedades
        [StringLength(200, ErrorMessage = "El término de búsqueda no puede superar los 200 caracteres.")]
        public string Texto { get; set; }

        [StringLength(5, ErrorMessage = "El código de idioma no puede superar los 5 caracteres.")]
        public string Idioma { get; set; }

        [StringLength(10, ErrorMessage = "El área no puede superar los 10 caracteres.")]
        public string Area { get; set; }

        [StringLength(150, ErrorMessage = "La sección no puede superar los 150 caracteres.")]
        public string Seccion { get; set; }

        [StringLength(15, ErrorMessage = "El criterio de orden no puede superar los 15 caracteres.")]
        public string OrdenarPor { get; set; }

        public bool? SoloTitulo { get; set; }
        #endregion

        public BEFiltroBusqueda()
        {

        }

        public BEFiltroBusqueda(
            string texto,
            string idioma,
            string area,
            string seccion,
            string ordenarPor,
            bool? soloTitulo)
        {
            this.Texto = texto;
            this.Idioma = idioma;
            this.Area = area;
            this.Seccion = seccion;
            this.OrdenarPor = ordenarPor;
            this.SoloTitulo = soloTitulo;
        }
    }
}
