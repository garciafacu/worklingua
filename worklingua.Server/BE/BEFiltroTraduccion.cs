namespace worklingua.Server.BE
{
    public class BEFiltroTraduccion
    {
        #region Propiedades
        public int IdiomaId { get; set; }
        public string Clave { get; set; }
        public string Texto { get; set; }
        public bool SoloPendientes { get; set; }
        #endregion

        public BEFiltroTraduccion()
        {

        }

        public BEFiltroTraduccion(int idiomaId, string clave, string texto, bool soloPendientes)
        {
            this.IdiomaId = idiomaId;
            this.Clave = clave;
            this.Texto = texto;
            this.SoloPendientes = soloPendientes;
        }
    }
}
