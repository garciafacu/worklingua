using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarOferta
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí la empresa destinataria.")]
        public int EmpresaId { get; set; }

        public int? PlanId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateOnly? FechaDesde { get; set; }

        public DateOnly? FechaHasta { get; set; }

        public bool Activo { get; set; }
        #endregion

        public BEGuardarOferta()
        {

        }

        public BEGuardarOferta(
            int empresaId,
            int? planId,
            string titulo,
            string descripcion,
            DateOnly? fechaDesde,
            DateOnly? fechaHasta,
            bool activo)
        {
            this.EmpresaId = empresaId;
            this.PlanId = planId;
            this.Titulo = titulo;
            this.Descripcion = descripcion;
            this.FechaDesde = fechaDesde;
            this.FechaHasta = fechaHasta;
            this.Activo = activo;
        }
    }
}
