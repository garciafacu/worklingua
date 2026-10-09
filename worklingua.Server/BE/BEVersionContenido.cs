namespace worklingua.Server.BE
{
    /// <summary>
    /// Un punto de restauración de un curso (CU-004-004).
    ///
    /// `ContenidoXml` viaja solo cuando se pide la versión completa: la línea
    /// de tiempo se dibuja con los metadatos.
    /// </summary>
    public class BEVersionContenido
    {
        #region Propiedades
        public int VersionContenidoId { get; set; }
        public int CursoId { get; set; }
        public string NumeroVersion { get; set; }
        public DateTime? FechaVersion { get; set; }
        public string Observaciones { get; set; }
        public int? UsuarioId { get; set; }
        public string Autor { get; set; }
        public string ContenidoXml { get; set; }
        /// <summary>Tamaño del documento, para que el historial lo muestre.</summary>
        public int LargoXml { get; set; }
        #endregion

        public BEVersionContenido()
        {

        }
    }
}
