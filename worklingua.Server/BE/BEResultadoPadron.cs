using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEResultadoPadron
    {
        #region Propiedades
        public int Leidas { get; set; }
        public int Validas { get; set; }
        public int ConError { get; set; }
        public bool Confirmado { get; set; }
        public int CupoDisponible { get; set; }
        public List<BEResultadoFilaPadron> Filas { get; set; }
        #endregion

        public BEResultadoPadron()
        {
            this.Filas = new List<BEResultadoFilaPadron>();
        }
    }
}
