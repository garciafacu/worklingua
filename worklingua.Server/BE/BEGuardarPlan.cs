using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarPlan
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "El precio mensual no puede ser negativo.")]
        public decimal PrecioMensual { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El plan debe incluir al menos una licencia.")]
        public int CantidadLicencias { get; set; }

        public bool Destacado { get; set; }
        #endregion

        public BEGuardarPlan()
        {

        }

        public BEGuardarPlan(
            string nombre,
            string descripcion,
            decimal precioMensual,
            int cantidadLicencias,
            bool destacado)
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.PrecioMensual = precioMensual;
            this.CantidadLicencias = cantidadLicencias;
            this.Destacado = destacado;
        }
    }
}
