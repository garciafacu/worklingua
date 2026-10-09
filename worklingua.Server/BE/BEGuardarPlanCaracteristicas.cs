using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarPlanCaracteristicas
    {
        #region Propiedades
        [Required(ErrorMessage = "Falta la lista de características.")]
        public List<BEItemPlanCaracteristica> Caracteristicas { get; set; }
        #endregion

        public BEGuardarPlanCaracteristicas()
        {

        }

        public BEGuardarPlanCaracteristicas(List<BEItemPlanCaracteristica> caracteristicas)
        {
            this.Caracteristicas = caracteristicas;
        }
    }
}
