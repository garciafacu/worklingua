namespace worklingua.Server.BE
{
    public class BEEmpresa
    {
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
    }

    public class BEEmpresaAdminRespuesta : BEEmpresaRespuesta
    {
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
    }

    public class BEEmpresaRespuesta
    {
        public int EmpresaId { get; set; }
        public string RazonSocial { get; set; }
    }

    public class BEGuardarEmpresa
    {
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
    }

    public class BEDepartamento
    {
        public int DepartamentoId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaAlta { get; set; }
        public bool? Activo { get; set; }
        public int Empleados { get; set; }
    }

    public class BEDepartamentoAdminRespuesta : BEDepartamentoRespuesta
    {
        public bool Activo { get; set; }
        public int Empleados { get; set; }
        public DateTime? FechaAlta { get; set; }
    }

    public class BEDepartamentoRespuesta
    {
        public int DepartamentoId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class BEGuardarDepartamento
    {
        public int EmpresaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class BEPadron
    {
        public int EmpresaId { get; set; }
        public bool Confirmar { get; set; }
        public List<BEFilaPadron> Filas { get; set; } = new List<BEFilaPadron>();
    }

    public class BEFilaPadron
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }
        public string Departamento { get; set; }
    }

    public class BEResultadoFilaPadron
    {
        public int Fila { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public bool Valida { get; set; }
        public string Motivo { get; set; }
    }

    public class BEResultadoPadron
    {
        public int Leidas { get; set; }
        public int Validas { get; set; }
        public int ConError { get; set; }
        public bool Confirmado { get; set; }
        public int CupoDisponible { get; set; }
        public List<BEResultadoFilaPadron> Filas { get; set; } = new List<BEResultadoFilaPadron>();
    }
}
