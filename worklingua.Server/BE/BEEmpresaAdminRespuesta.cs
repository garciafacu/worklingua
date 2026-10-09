namespace worklingua.Server.BE
{
    public class BEEmpresaAdminRespuesta
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
        public bool Activo { get; set; }
        public bool Protegido { get; set; }
        public bool Seleccionable { get; set; }
        #endregion

        public BEEmpresaAdminRespuesta()
        {

        }

        public BEEmpresaAdminRespuesta(
            int empresaId,
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string provincia,
            string pais,
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
            this.Activo = activo;
            this.Protegido = protegido;
            this.Seleccionable = seleccionable;
        }
    }
}
