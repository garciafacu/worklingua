using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEItemPlanCaracteristica
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "La característica seleccionada no es válida.")]
        public int CaracteristicaId { get; set; }

        public bool Incluido { get; set; }

        [StringLength(100, ErrorMessage = "El detalle no puede superar los 100 caracteres.")]
        public string Detalle { get; set; }
        #endregion

        public BEItemPlanCaracteristica()
        {

        }

        public BEItemPlanCaracteristica(int caracteristicaId, bool incluido, string detalle)
        {
            this.CaracteristicaId = caracteristicaId;
            this.Incluido = incluido;
            this.Detalle = detalle;
        }
    }
}
