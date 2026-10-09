using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BECrearTicket
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí la contratación sobre la que consultás.")]
        public int SuscripcionId { get; set; }

        public int? CursoId { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El asunto no puede superar los 150 caracteres.")]
        public string Asunto { get; set; }

        [Required(ErrorMessage = "Escribí tu consulta.")]
        [StringLength(2000, ErrorMessage = "La consulta no puede superar los 2000 caracteres.")]
        public string Texto { get; set; }

        [StringLength(5, ErrorMessage = "El código de idioma no es válido.")]
        public string CodigoIdioma { get; set; }
        #endregion

        public BECrearTicket()
        {

        }

        public BECrearTicket(int suscripcionId, int? cursoId, string asunto, string texto, string codigoIdioma)
        {
            this.SuscripcionId = suscripcionId;
            this.CursoId = cursoId;
            this.Asunto = asunto;
            this.Texto = texto;
            this.CodigoIdioma = codigoIdioma;
        }
    }
}
