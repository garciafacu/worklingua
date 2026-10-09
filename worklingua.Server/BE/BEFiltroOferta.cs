namespace worklingua.Server.BE
{
    public class BEFiltroOferta
    {
        #region Propiedades
        public int? OfertaId { get; set; }
        public int? EmpresaId { get; set; }
        public DateOnly? VigentesAl { get; set; }
        #endregion

        public BEFiltroOferta()
        {

        }

        public BEFiltroOferta(int? ofertaId, int? empresaId, DateOnly? vigentesAl)
        {
            this.OfertaId = ofertaId;
            this.EmpresaId = empresaId;
            this.VigentesAl = vigentesAl;
        }
    }
}
