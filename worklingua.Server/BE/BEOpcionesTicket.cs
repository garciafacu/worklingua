using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEOpcionesTicket
    {
        #region Propiedades
        public List<BESuscripcionConPlan> Contrataciones { get; set; }
        public List<BECurso> Cursos { get; set; }
        #endregion

        public BEOpcionesTicket()
        {
            this.Contrataciones = new List<BESuscripcionConPlan>();
            this.Cursos = new List<BECurso>();
        }

        public BEOpcionesTicket(List<BESuscripcionConPlan> contrataciones, List<BECurso> cursos)
        {
            this.Contrataciones = contrataciones;
            this.Cursos = cursos;
        }
    }
}
