using System.Collections.Generic;

namespace worklingua.Server.BE
{
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
