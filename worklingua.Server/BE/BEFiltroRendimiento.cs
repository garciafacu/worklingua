namespace worklingua.Server.BE
{
    public class BEFiltroRendimiento
    {
        #region Propiedades
        public int EmpresaId { get; set; }
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        public int? DepartamentoId { get; set; }
        #endregion

        public BEFiltroRendimiento()
        {

        }
    }
}
