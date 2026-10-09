using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El resultado de restaurar o clonar.
    ///
    /// `Omitidos` son los activos cuyo archivo ya no está en disco: es el
    /// camino alternativo 2 de CU-004-002 y el 1 de CU-004-004, resueltos
    /// salteando lo que no se puede traer en vez de heredar enlaces rotos.
    /// </summary>
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
