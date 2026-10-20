namespace worklingua.Server.BE
{
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
