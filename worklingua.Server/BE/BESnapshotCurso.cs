using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El estado completo de un curso en un momento dado (CU-004-004).
    ///
    /// Es lo que `ServicioXml` convierte en documento y vuelve a leer, y lo que
    /// el clonado copia hacia un curso nuevo (CU-004-002).
    /// </summary>
    public class BESnapshotCurso
    {
        #region Propiedades
        public BECurso Curso { get; set; }
        public List<BEModulo> Modulos { get; set; }
        public List<BEActivoPedagogico> Activos { get; set; }
        #endregion

        public BESnapshotCurso()
        {
            Curso = new BECurso();
            Modulos = new List<BEModulo>();
            Activos = new List<BEActivoPedagogico>();
        }
    }
}
