namespace worklingua.Server.BE
{
    /// <summary>
    /// Un recurso multimedia de un curso (CU-004-001): imágenes, audios de
    /// pronunciación y vocabularios.
    ///
    /// Estado y Activo son cosas distintas: Estado dice si el contenido ya se
    /// puede usar (BORRADOR o PUBLICADO) y Activo es la baja lógica.
    /// </summary>
    public class BEActivoPedagogico
    {
        #region Propiedades
        public int ActivoPedagogicoId { get; set; }
        public int CursoId { get; set; }
        /// <summary>Null es material general del curso; con valor, contenido de esa lección.</summary>
        public int? ModuloId { get; set; }
        /// <summary>Nombre del módulo, para mostrarlo y para el snapshot XML del versionado.</summary>
        public string Modulo { get; set; }
        public string Nombre { get; set; }
        public string TipoContenido { get; set; }
        public string Descripcion { get; set; }
        /// <summary>Ruta relativa servida por la aplicación, por ejemplo /activos/xxx.png</summary>
        public string UrlArchivo { get; set; }
        public bool? Activo { get; set; }
        public string Estado { get; set; }
        #endregion

        public BEActivoPedagogico()
        {

        }

        public BEActivoPedagogico(
            int activoPedagogicoId,
            int cursoId,
            int? moduloId,
            string modulo,
            string nombre,
            string tipoContenido,
            string descripcion,
            string urlArchivo,
            bool? activo,
            string estado)
        {
            this.ActivoPedagogicoId = activoPedagogicoId;
            this.CursoId = cursoId;
            this.ModuloId = moduloId;
            this.Modulo = modulo;
            this.Nombre = nombre;
            this.TipoContenido = tipoContenido;
            this.Descripcion = descripcion;
            this.UrlArchivo = urlArchivo;
            this.Activo = activo;
            this.Estado = estado;
        }
    }
}
