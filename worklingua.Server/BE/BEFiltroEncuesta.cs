namespace worklingua.Server.BE
{
    public class BEFiltroEncuesta
    {
        #region Propiedades
        public int? EncuestaId { get; set; }
        public int? IdiomaId { get; set; }

        /// <summary>Código ISO del idioma de la interfaz, para el listado del cliente.</summary>
        public string Idioma { get; set; }
        public DateOnly? VigentesAl { get; set; }
        #endregion

        public BEFiltroEncuesta()
        {

        }

        public BEFiltroEncuesta(int? encuestaId, int? idiomaId, DateOnly? vigentesAl)
        {
            this.EncuestaId = encuestaId;
            this.IdiomaId = idiomaId;
            this.VigentesAl = vigentesAl;
        }
    }
}
