using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El pedido de procesamiento del padrón.
    ///
    /// Con <see cref="Confirmar"/> en false solo se valida y no se escribe nada:
    /// es la previsualización. En true se mandan las invitaciones de las filas
    /// válidas. El archivo se vuelve a mandar entero en los dos pasos, así que
    /// el backend revalida todo y nunca confía en lo que ya validó antes.
    /// </summary>
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
