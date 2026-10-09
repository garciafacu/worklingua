namespace worklingua.Server.BE
{
    public class BETraduccion
    {
        #region Propiedades
        public int TraduccionId { get; set; }
        public int IdiomaId { get; set; }
        public string Clave { get; set; }
        public string Texto { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BETraduccion()
        {

        }

        public BETraduccion(int traduccionId, int idiomaId, string clave, string texto, bool activo)
        {
            this.TraduccionId = traduccionId;
            this.IdiomaId = idiomaId;
            this.Clave = clave;
            this.Texto = texto;
            this.Activo = activo;
        }
    }
}
