namespace worklingua.Server.BE
{
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
