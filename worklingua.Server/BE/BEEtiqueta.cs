namespace worklingua.Server.BE
{
    /// <summary>
    /// Una etiqueta del diccionario global (CU-004-005).
    ///
    /// El diccionario es compartido: la misma etiqueta sirve para cursos de
    /// cualquier empresa, que es lo que la hace útil para buscar.
    /// </summary>
    public class BEEtiqueta
    {
        #region Propiedades
        public int EtiquetaId { get; set; }
        public string Nombre { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        #endregion

        public BEEtiqueta()
        {

        }

        public BEEtiqueta(int etiquetaId, string nombre, bool? activo, DateTime? fechaAlta)
        {
            this.EtiquetaId = etiquetaId;
            this.Nombre = nombre;
            this.Activo = activo;
            this.FechaAlta = fechaAlta;
        }
    }
}
