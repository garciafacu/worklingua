using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BECursoRespuesta
    {
        #region Propiedades
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public int? DuracionHoras { get; set; }
        public string Idioma { get; set; }
        public bool EsGlobal { get; set; }
        /// <summary>Categoría sectorial (CU-004-005).</summary>
        public string Sector { get; set; }
        /// <summary>Etiquetas lógicas del curso (CU-004-005).</summary>
        public List<BEEtiqueta> Etiquetas { get; set; }
        #endregion

        public BECursoRespuesta()
        {

        }

        public BECursoRespuesta(
            int cursoId,
            string nombre,
            string descripcion,
            string nivel,
            int? duracionHoras,
            string idioma,
            bool esGlobal)
        {
            this.CursoId = cursoId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Nivel = nivel;
            this.DuracionHoras = duracionHoras;
            this.Idioma = idioma;
            this.EsGlobal = esGlobal;
        }
    }
}
