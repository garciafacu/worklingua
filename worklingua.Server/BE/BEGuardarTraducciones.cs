using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarTraducciones
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Hay que elegir un idioma.")]
        public int IdiomaId { get; set; }

        [Required(ErrorMessage = "No hay traducciones para guardar.")]
        [MinLength(1, ErrorMessage = "No hay traducciones para guardar.")]
        public List<BETraduccionItem> Traducciones { get; set; }
        #endregion

        public BEGuardarTraducciones()
        {

        }

        public BEGuardarTraducciones(int idiomaId, List<BETraduccionItem> traducciones)
        {
            this.IdiomaId = idiomaId;
            this.Traducciones = traducciones;
        }
    }
}
