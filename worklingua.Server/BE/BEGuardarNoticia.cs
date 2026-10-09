using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarNoticia
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí el idioma de la noticia.")]
        public int IdiomaId { get; set; }

        [Required(ErrorMessage = "El título de la noticia es obligatorio.")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; }

        [StringLength(300, ErrorMessage = "El resumen no puede superar los 300 caracteres.")]
        public string Resumen { get; set; }

        [Required(ErrorMessage = "El contenido de la noticia es obligatorio.")]
        [StringLength(20000, ErrorMessage = "El contenido no puede superar los 20000 caracteres.")]
        public string Contenido { get; set; }

        public DateTime? FechaPublicacion { get; set; }
        #endregion

        public BEGuardarNoticia()
        {

        }

        public BEGuardarNoticia(
            int idiomaId,
            string titulo,
            string resumen,
            string contenido,
            DateTime? fechaPublicacion)
        {
            this.IdiomaId = idiomaId;
            this.Titulo = titulo;
            this.Resumen = resumen;
            this.Contenido = contenido;
            this.FechaPublicacion = fechaPublicacion;
        }
    }
}
