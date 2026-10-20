using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEResultadoRestauracion
    {
        #region Propiedades
        public int CursoId { get; set; }
        public int Omitidos { get; set; }
        public List<string> NombresOmitidos { get; set; }
        #endregion

        public BEResultadoRestauracion()
        {
            NombresOmitidos = new List<string>();
        }
    }
}
