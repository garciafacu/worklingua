namespace worklingua.Server.BE
{
    public class BEAlerta
    {
        public int AlertaId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public int DiasInactividad { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? UltimaEmision { get; set; }
        public int Destinatarios { get; set; }
        public int UsuarioId { get; set; }
    }

    public class BEAlertaRespuesta
    {
        public int AlertaId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public int DiasInactividad { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? UltimaEmision { get; set; }
        public int Destinatarios { get; set; }
    }

    public class BEGuardarAlerta
    {
        public int EmpresaId { get; set; }
        public int? DepartamentoId { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public int DiasInactividad { get; set; }
    }

    public class BEEmpleadoInactivo
    {
        public int UsuarioId { get; set; }
        public string Empleado { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public int DiasSinActividad { get; set; }
    }

    public class BEEmisionRespuesta
    {
        public BEAlertaRespuesta Alerta { get; set; }
        public int Destinatarios { get; set; }
        public List<BEEmpleadoInactivo> Empleados { get; set; } = new List<BEEmpleadoInactivo>();
    }

    public class BENotificacion
    {
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public bool? Leida { get; set; }
        public DateTime? FechaEnvio { get; set; }
    }

    public class BEBandejaNotificaciones
    {
        public int NoLeidas { get; set; }
        public List<BENotificacion> Notificaciones { get; set; } = new List<BENotificacion>();
    }
}
