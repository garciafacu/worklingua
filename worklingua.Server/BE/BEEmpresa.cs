namespace worklingua.Server.BE
{
    public class BEEmpresa
    {
        #region Propiedades
        public int EmpresaId { get; set; }
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }
        public bool Protegido { get; set; }
        public bool Seleccionable { get; set; }
        #endregion

        public BEEmpresa()
        {

        }

        public BEEmpresa(
            int empresaId,
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string provincia,
            string pais,
            DateTime fechaAlta,
            bool activo,
            bool protegido,
            bool seleccionable)
        {
            this.EmpresaId = empresaId;
            this.RazonSocial = razonSocial;
            this.CUIT = cuit;
            this.Email = email;
            this.Telefono = telefono;
            this.Direccion = direccion;
            this.Ciudad = ciudad;
            this.Provincia = provincia;
            this.Pais = pais;
            this.FechaAlta = fechaAlta;
            this.Activo = activo;
            this.Protegido = protegido;
            this.Seleccionable = seleccionable;
        }
    }
}
