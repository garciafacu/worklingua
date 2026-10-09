namespace worklingua.Server.BE
{
    public class BEDepartamento
    {
        #region Propiedades
        public int DepartamentoId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaAlta { get; set; }
        public bool? Activo { get; set; }
        /// <summary>Usuarios activos asignados. Lo calcula el SP.</summary>
        public int Empleados { get; set; }
        #endregion

        public BEDepartamento()
        {

        }

        public BEDepartamento(
            int departamentoId,
            int empresaId,
            string empresa,
            string nombre,
            string descripcion,
            DateTime? fechaAlta,
            bool? activo,
            int empleados)
        {
            this.DepartamentoId = departamentoId;
            this.EmpresaId = empresaId;
            this.Empresa = empresa;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.FechaAlta = fechaAlta;
            this.Activo = activo;
            this.Empleados = empleados;
        }
    }
}
