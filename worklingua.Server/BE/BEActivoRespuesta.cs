namespace worklingua.Server.BE
{
    /// <summary>Un activo pedagógico tal como lo ve el panel del ABM de Cursos.</summary>
    public class BEActivoRespuesta
    {
        #region Propiedades
        public int ActivoPedagogicoId { get; set; }
        public int CursoId { get; set; }
        /// <summary>Null es material general del curso; con valor, contenido de esa lección.</summary>
        public int? ModuloId { get; set; }
        public string Modulo { get; set; }
        public string Nombre { get; set; }
        public string TipoContenido { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public bool Activo { get; set; }
        public string Estado { get; set; }
        #endregion

        public BEActivoRespuesta()
        {

        }
    }
}
