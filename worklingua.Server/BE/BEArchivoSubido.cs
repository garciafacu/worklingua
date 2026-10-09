namespace worklingua.Server.BE
{
    /// <summary>
    /// Un archivo que llega en la petición, ya leído a memoria.
    ///
    /// El Controller convierte el IFormFile a esto: la BLL valida y guarda sin
    /// conocer tipos de ASP.NET, que es lo que la mantiene independiente de la
    /// capa web.
    /// </summary>
    public class BEArchivoSubido
    {
        #region Propiedades
        public string NombreOriginal { get; set; }
        public byte[] Contenido { get; set; }
        #endregion

        public BEArchivoSubido()
        {

        }

        public BEArchivoSubido(string nombreOriginal, byte[] contenido)
        {
            this.NombreOriginal = nombreOriginal;
            this.Contenido = contenido;
        }
    }
}
