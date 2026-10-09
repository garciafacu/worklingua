namespace worklingua.Server.BE
{
    public class BEIdioma
    {
        #region Propiedades
        public int IdiomaId { get; set; }
        public string Nombre { get; set; }
        public string CodigoISO { get; set; }
        public bool? Activo { get; set; }
        #endregion

        public BEIdioma()
        {

        }

        public BEIdioma(int idiomaId, string nombre, string codigoISO, bool? activo)
        {
            this.IdiomaId = idiomaId;
            this.Nombre = nombre;
            this.CodigoISO = codigoISO;
            this.Activo = activo;
        }
    }
}
