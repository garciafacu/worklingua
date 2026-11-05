namespace worklingua.Server.BE
{
    public class BEPlanSuscripcion
    {
        public int PlanId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Activo { get; set; }
        public bool Destacado { get; set; }
        public bool Protegido { get; set; }
    }

    public class BEPlanRespuesta
    {
        public int PlanId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Destacado { get; set; }
        public decimal PromedioValoracion { get; set; }
        public int CantidadValoraciones { get; set; }
        public List<BEItemPlanCaracteristica> Caracteristicas { get; set; }
    }

    public class BEGuardarPlan
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Destacado { get; set; }
    }

    public class BEPlanCaracteristica
    {
        public int PlanCaracteristicaId { get; set; }
        public int PlanId { get; set; }
        public int CaracteristicaId { get; set; }
        public bool Incluido { get; set; }
        public string Detalle { get; set; }
    }

    public class BEGuardarPlanCaracteristicas
    {
        public List<BEItemPlanCaracteristica> Caracteristicas { get; set; }
    }

    public class BEItemPlanCaracteristica
    {
        public int CaracteristicaId { get; set; }
        public bool Incluido { get; set; }
        public string Detalle { get; set; }
    }

    public class BECaracteristica
    {
        public int CaracteristicaId { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }

    public class BECaracteristicaRespuesta
    {
        public int CaracteristicaId { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }
    }

    public class BEGuardarCaracteristica
    {
        public string Nombre { get; set; }
        public int Orden { get; set; }
    }

    public class BESuscripcion
    {
        public int SuscripcionId { get; set; }
        public int EmpresaId { get; set; }
        public int PlanId { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public string Estado { get; set; }
        public bool? RenovacionAutomatica { get; set; }

        public BESuscripcion()
        {
        }

        public BESuscripcion(
            int suscripcionId,
            int empresaId,
            int planId,
            DateOnly fechaInicio,
            DateOnly? fechaFin,
            string estado,
            bool? renovacionAutomatica)
        {
            this.SuscripcionId = suscripcionId;
            this.EmpresaId = empresaId;
            this.PlanId = planId;
            this.FechaInicio = fechaInicio;
            this.FechaFin = fechaFin;
            this.Estado = estado;
            this.RenovacionAutomatica = renovacionAutomatica;
        }
    }

    public class BESuscripcionConPlan
    {
        public BESuscripcion Suscripcion { get; set; }
        public string Plan { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }

        public BESuscripcionConPlan()
        {
        }

        public BESuscripcionConPlan(
            BESuscripcion suscripcion, string plan, decimal precioMensual, int cantidadLicencias)
        {
            this.Suscripcion = suscripcion;
            this.Plan = plan;
            this.PrecioMensual = precioMensual;
            this.CantidadLicencias = cantidadLicencias;
        }
    }

    public class BESuscripcionRespuesta
    {
        public int SuscripcionId { get; set; }
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string Estado { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Cancelable { get; set; }
    }

    public class BEHistorialSuscripcion
    {
        public int HistorialId { get; set; }
        public int SuscripcionId { get; set; }
        public DateTime? FechaMovimiento { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string Observacion { get; set; }
    }

    public class BELicencia
    {
        public int LicenciaId { get; set; }
        public int SuscripcionId { get; set; }
        public int? UsuarioId { get; set; }
        public Guid? CodigoLicencia { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estado { get; set; }
        public string EstadoSuscripcion { get; set; }
    }

    public class BEInventarioLicencia
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public int? LicenciaId { get; set; }
        public Guid? CodigoLicencia { get; set; }
        public DateTime? FechaAsignacion { get; set; }
    }

    public class BEInventarioRespuesta
    {
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public string Plan { get; set; }
        public int Contratadas { get; set; }
        public int Asignadas { get; set; }
        public int Disponibles { get; set; }
        public List<BEInventarioLicencia> Empleados { get; set; } = new List<BEInventarioLicencia>();
    }
}
