namespace worklingua.Server.BE
{
    public class BEDepartamentoRespuesta
    {
        #region Propiedades
        public int DepartamentoId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        #endregion

        public BEDepartamentoRespuesta()
        {

        }

        public BEDepartamentoRespuesta(
            int departamentoId, int empresaId, string empresa, string nombre, string descripcion)
        {
            this.DepartamentoId = departamentoId;
            this.EmpresaId = empresaId;
            this.Empresa = empresa;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
        }
    }
}
