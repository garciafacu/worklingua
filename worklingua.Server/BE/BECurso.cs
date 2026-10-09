using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BECurso
    {
        #region Propiedades
        public int CursoId { get; set; }
        public string Idioma { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public int? DuracionHoras { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public int? EmpresaId { get; set; }
        /// <summary>Categoría sectorial (CU-004-005): TURISMO, GASTRONOMIA o IT.</summary>
        public string Sector { get; set; }
        /// <summary>
        /// Ventana de despliegue (CU-004-006). Las dos en null significan que
        /// el curso está disponible siempre.
        /// </summary>
        public DateOnly? FechaPublicacion { get; set; }
        public DateOnly? FechaFin { get; set; }
        /// <summary>Etiquetas lógicas del curso (CU-004-005).</summary>
        public List<BEEtiqueta> Etiquetas { get; set; }
        #endregion

        public BECurso()
        {
            this.Etiquetas = new List<BEEtiqueta>();
        }

        public BECurso(
            int cursoId,
            string idioma,
            string nombre,
            string descripcion,
            string nivel,
            int? duracionHoras,
            bool activo,
            DateTime fechaAlta,
            int? empresaId)
        {
            this.CursoId = cursoId;
            this.Idioma = idioma;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Nivel = nivel;
            this.DuracionHoras = duracionHoras;
            this.Activo = activo;
            this.FechaAlta = fechaAlta;
            this.EmpresaId = empresaId;
            this.Etiquetas = new List<BEEtiqueta>();
        }
    }
}
