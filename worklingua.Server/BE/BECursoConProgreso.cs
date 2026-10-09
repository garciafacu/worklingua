using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BECursoConProgreso
    {
        #region Propiedades
        public BECurso Curso { get; set; }
        public List<BEModuloConProgreso> Modulos { get; set; }
        /// <summary>
        /// El material de apoyo del curso (CU-004-001): solo los activos
        /// publicados y vigentes. Viaja con el curso para que Mis cursos no
        /// tenga que pedirlo aparte.
        /// </summary>
        public List<BEActivoPedagogico> Activos { get; set; }
        public decimal PorcentajeAvance { get; set; }
        public string Estado { get; set; }
        public DateTime? UltimaActividad { get; set; }
        #endregion

        public BECursoConProgreso()
        {
            this.Modulos = new List<BEModuloConProgreso>();
            this.Activos = new List<BEActivoPedagogico>();
        }

        public BECursoConProgreso(
            BECurso curso,
            List<BEModuloConProgreso> modulos,
            decimal porcentajeAvance,
            string estado,
            DateTime? ultimaActividad)
        {
            this.Curso = curso;
            this.Modulos = modulos;
            this.Activos = new List<BEActivoPedagogico>();
            this.PorcentajeAvance = porcentajeAvance;
            this.Estado = estado;
            this.UltimaActividad = ultimaActividad;
        }
    }
}
