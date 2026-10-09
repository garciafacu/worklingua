using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEFiltroNoticia
    {
        #region Propiedades
        [StringLength(5, ErrorMessage = "El código de idioma no puede superar los 5 caracteres.")]
        public string Idioma { get; set; }

        public int? IdiomaId { get; set; }
        #endregion

        public BEFiltroNoticia()
        {

        }

        public BEFiltroNoticia(string idioma, int? idiomaId)
        {
            this.Idioma = idioma;
            this.IdiomaId = idiomaId;
        }
    }
}
