using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEEnviarNewsletter
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí el idioma del envío.")]
        public int IdiomaId { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(200, ErrorMessage = "El asunto no puede superar los 200 caracteres.")]
        public string Asunto { get; set; }

        [Required(ErrorMessage = "Elegí al menos una noticia.")]
        public List<int> NoticiaIds { get; set; }
        #endregion

        public BEEnviarNewsletter()
        {

        }

        public BEEnviarNewsletter(int idiomaId, string asunto, List<int> noticiaIds)
        {
            this.IdiomaId = idiomaId;
            this.Asunto = asunto;
            this.NoticiaIds = noticiaIds;
        }
    }
}
