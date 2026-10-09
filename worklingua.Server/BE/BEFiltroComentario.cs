using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEFiltroComentario
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "El plan seleccionado no es válido.")]
        public int? PlanId { get; set; }

        [StringLength(200, ErrorMessage = "El término de búsqueda no puede superar los 200 caracteres.")]
        public string Texto { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }
        #endregion

        public BEFiltroComentario()
        {

        }

        public BEFiltroComentario(int? planId, string texto, DateTime? desde, DateTime? hasta)
        {
            this.PlanId = planId;
            this.Texto = texto;
            this.Desde = desde;
            this.Hasta = hasta;
        }
    }
}
