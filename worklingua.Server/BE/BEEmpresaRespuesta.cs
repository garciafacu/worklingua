namespace worklingua.Server.BE
{
    public class BEEmpresaRespuesta
    {
        #region Propiedades
        public int EmpresaId { get; set; }
        public string RazonSocial { get; set; }
        #endregion

        public BEEmpresaRespuesta()
        {

        }

        public BEEmpresaRespuesta(int empresaId, string razonSocial)
        {
            this.EmpresaId = empresaId;
            this.RazonSocial = razonSocial;
        }
    }
}
