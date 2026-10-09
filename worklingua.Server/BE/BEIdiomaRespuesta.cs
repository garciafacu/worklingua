namespace worklingua.Server.BE
{
    public class BEIdiomaRespuesta
    {
        #region Propiedades
        public int IdiomaId { get; set; }
        public string Nombre { get; set; }
        public string CodigoISO { get; set; }
        #endregion

        public BEIdiomaRespuesta()
        {

        }

        public BEIdiomaRespuesta(int idiomaId, string nombre, string codigoISO)
        {
            this.IdiomaId = idiomaId;
            this.Nombre = nombre;
            this.CodigoISO = codigoISO;
        }
    }
}
