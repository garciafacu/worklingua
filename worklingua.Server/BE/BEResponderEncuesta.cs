using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEResponderEncuesta
    {
        #region Propiedades
        public int EncuestaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elegí una opción para responder.")]
        public int OpcionEncuestaId { get; set; }
        #endregion

        public BEResponderEncuesta()
        {

        }

        public BEResponderEncuesta(int encuestaId, int opcionEncuestaId)
        {
            this.EncuestaId = encuestaId;
            this.OpcionEncuestaId = opcionEncuestaId;
        }
    }
}
