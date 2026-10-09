namespace worklingua.Server.BE
{
    /// <summary>
    /// Una etiqueta asignada a un curso. Es lo que devuelve el listado de
    /// asignaciones, que trae el curso al que pertenece cada una.
    /// </summary>
    public class BEEtiquetaCurso
    {
        #region Propiedades
        public int CursoId { get; set; }
        public int EtiquetaId { get; set; }
        public string Nombre { get; set; }
        #endregion

        public BEEtiquetaCurso()
        {

        }

        public BEEtiquetaCurso(int cursoId, int etiquetaId, string nombre)
        {
            this.CursoId = cursoId;
            this.EtiquetaId = etiquetaId;
            this.Nombre = nombre;
        }
    }
}
