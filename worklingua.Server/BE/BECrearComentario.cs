using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BECrearComentario
    {
        #region Propiedades
        [Required(ErrorMessage = "Indicá sobre qué plan querés opinar.")]
        [Range(1, int.MaxValue, ErrorMessage = "El plan seleccionado no es válido.")]
        public int PlanId { get; set; }

        [Range(1, 5, ErrorMessage = "Elegí entre 1 y 5 estrellas.")]
        public int Puntaje { get; set; }

        [StringLength(1000, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
        public string Texto { get; set; }
        #endregion

        public BECrearComentario()
        {

        }

        public BECrearComentario(int planId, int puntaje, string texto)
        {
            this.PlanId = planId;
            this.Puntaje = puntaje;
            this.Texto = texto;
        }
    }
}
