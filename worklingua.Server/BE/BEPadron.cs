using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPadron
    {
        #region Propiedades
        public int EmpresaId { get; set; }
        public bool Confirmar { get; set; }
        public List<BEFilaPadron> Filas { get; set; }
        #endregion

        public BEPadron()
        {
            this.Filas = new List<BEFilaPadron>();
        }
    }
}
