namespace worklingua.Server.BE
{
    public class BECulturaAdminRespuesta : BECulturaRespuesta
    {
        #region Propiedades
        public int IdiomaId { get; set; }
        public string Idioma { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BECulturaAdminRespuesta()
        {

        }

        public BECulturaAdminRespuesta(int idiomaId, string idioma, bool activo)
        {
            this.IdiomaId = idiomaId;
            this.Idioma = idioma;
            this.Activo = activo;
        }
    }
}
