namespace worklingua.Server.BE
{
    public class BEDepartamentoAdminRespuesta : BEDepartamentoRespuesta
    {
        #region Propiedades
        public bool Activo { get; set; }
        public int Empleados { get; set; }
        public DateTime? FechaAlta { get; set; }
        #endregion

        public BEDepartamentoAdminRespuesta()
        {

        }

        public BEDepartamentoAdminRespuesta(bool activo, int empleados, DateTime? fechaAlta)
        {
            this.Activo = activo;
            this.Empleados = empleados;
            this.FechaAlta = fechaAlta;
        }
    }
}
