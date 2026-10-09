using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarProgreso
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí un módulo.")]
        public int ModuloId { get; set; }

        [Required(ErrorMessage = "Falta la acción sobre el módulo.")]
        [StringLength(20, ErrorMessage = "La acción no es válida.")]
        public string Accion { get; set; }
        #endregion

        public BEGuardarProgreso()
        {

        }

        public BEGuardarProgreso(int moduloId, string accion)
        {
            this.ModuloId = moduloId;
            this.Accion = accion;
        }
    }
}
