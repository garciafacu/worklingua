using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPPlan
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEPlanSuscripcion> ListarTodo()
        {
            return ServicioLectorFila.ALista<BEPlanSuscripcion>(oDatos.Leer("sp_Plan_Listar", null), Mapear);
        }

        public List<BEPlanSuscripcion> ListarTodoConBajas()
        {
            return ServicioLectorFila.ALista<BEPlanSuscripcion>(oDatos.Leer("sp_Plan_ListarTodos", null), Mapear);
        }

        public BEPlanSuscripcion ListarObjeto(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Plan_ObtenerPorId", Hdatos), Mapear);
        }

        public int Guardar(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@PrecioMensual", Objeto.PrecioMensual);
            Hdatos.Add("@CantidadLicencias", Objeto.CantidadLicencias);
            Hdatos.Add("@Destacado", Objeto.Destacado);

            if (Objeto.PlanId != 0)
            {
                Hdatos.Add("@PlanId", Objeto.PlanId);

                oDatos.LeerEscalar("sp_Plan_Modificar", Hdatos);

                return Objeto.PlanId;
            }

            Hdatos.Add("@Activo", Objeto.Activo);

            object identidad = oDatos.LeerEscalar("sp_Plan_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            object filas = oDatos.LeerEscalar("sp_Plan_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEPlanSuscripcion Mapear(DataRow Item)
        {
            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();

            oPlanBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
            oPlanBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oPlanBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oPlanBE.PrecioMensual = ServicioLectorFila.LeerDecimal(Item, "PrecioMensual");
            oPlanBE.CantidadLicencias = ServicioLectorFila.LeerEntero(Item, "CantidadLicencias");
            oPlanBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");
            oPlanBE.Destacado = ServicioLectorFila.LeerBooleano(Item, "Destacado");
            oPlanBE.Protegido = ServicioLectorFila.LeerBooleano(Item, "Protegido");

            return oPlanBE;
        }
    }

    public class MPPPlanCaracteristica
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEPlanCaracteristica> ListarTodo()
        {
            return ServicioLectorFila.ALista(oDatos.Leer("sp_PlanCaracteristica_Listar", null), Mapear);
        }

        public List<BEPlanCaracteristica> ListarPorPlan(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            return ServicioLectorFila.ALista(oDatos.Leer("sp_PlanCaracteristica_ListarPorPlan", Hdatos), Mapear);
        }

        public int Guardar(BEPlanCaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);
            Hdatos.Add("@Incluido", Objeto.Incluido);
            Hdatos.Add("@Detalle", Objeto.Detalle);

            object identidad = oDatos.LeerEscalar("sp_PlanCaracteristica_Guardar", Hdatos);

            return ServicioLectorFila.ATotalFilas(identidad);
        }

        public bool Baja(BEPlanCaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

            object filas = oDatos.LeerEscalar("sp_PlanCaracteristica_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEPlanCaracteristica Mapear(DataRow Item)
        {
            BEPlanCaracteristica oPlanCaracteristicaBE = new BEPlanCaracteristica();

            oPlanCaracteristicaBE.PlanCaracteristicaId = ServicioLectorFila.LeerEntero(Item, "PlanCaracteristicaId");
            oPlanCaracteristicaBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
            oPlanCaracteristicaBE.CaracteristicaId = ServicioLectorFila.LeerEntero(Item, "CaracteristicaId");
            oPlanCaracteristicaBE.Incluido = ServicioLectorFila.LeerBooleano(Item, "Incluido");
            oPlanCaracteristicaBE.Detalle = ServicioLectorFila.LeerTextoNulo(Item, "Detalle");

            return oPlanCaracteristicaBE;
        }
    }

    public class MPPCaracteristica
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BECaracteristica> ListarTodo()
        {
            return Listar("sp_Caracteristica_Listar");
        }

        public List<BECaracteristica> ListarTodoConBajas()
        {
            return Listar("sp_Caracteristica_ListarTodos");
        }

        public BECaracteristica ListarObjeto(BECaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Caracteristica_ObtenerPorId", Hdatos), Mapear);
        }

        public int Guardar(BECaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Orden", Objeto.Orden);

            if (Objeto.CaracteristicaId != 0)
            {
                Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

                oDatos.LeerEscalar("sp_Caracteristica_Modificar", Hdatos);

                return Objeto.CaracteristicaId;
            }

            Hdatos.Add("@Activo", Objeto.Activo);

            object identidad = oDatos.LeerEscalar("sp_Caracteristica_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BECaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

            object filas = oDatos.LeerEscalar("sp_Caracteristica_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BECaracteristica> Listar(string Consulta)
        {
            return ServicioLectorFila.ALista<BECaracteristica>(oDatos.Leer(Consulta, null), Mapear);
        }

        private BECaracteristica Mapear(DataRow Item)
        {
            BECaracteristica oCaracteristicaBE = new BECaracteristica();

            oCaracteristicaBE.CaracteristicaId = ServicioLectorFila.LeerEntero(Item, "CaracteristicaId");
            oCaracteristicaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oCaracteristicaBE.Orden = ServicioLectorFila.LeerEntero(Item, "Orden");
            oCaracteristicaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oCaracteristicaBE;
        }
    }

    public class MPPSuscripcion
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public BESuscripcionConPlan ListarObjeto(BESuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@Estado", Objeto.Estado);

            return LeerUna("sp_Suscripcion_ObtenerPorEmpresa", Hdatos);
        }

        public BESuscripcionConPlan ListarPorId(BESuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            return LeerUna("sp_Suscripcion_ObtenerPorId", Hdatos);
        }

        public List<BESuscripcionConPlan> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista<BESuscripcionConPlan>(oDatos.Leer("sp_Suscripcion_ListarPorEmpresa", Hdatos), Mapear);
        }

        public int Guardar(BESuscripcion Objeto)
        {
            if (Objeto.SuscripcionId != 0)
            {
                throw new NotImplementedException(
                    "No existe sp_Suscripcion_Modificar: la suscripción no se edita. " +
                    "El estado y la fecha de fin cambian con CambiarEstado.");
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@FechaInicio", Objeto.FechaInicio.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add(
                "@FechaFin",
                Objeto.FechaFin.HasValue
                    ? (object)Objeto.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
                    : null);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@RenovacionAutomatica", Objeto.RenovacionAutomatica);

            object identidad = oDatos.LeerEscalar("sp_Suscripcion_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool CambiarEstado(BESuscripcion Objeto, BEHistorialSuscripcion oHistorialBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);
            Hdatos.Add("@EstadoNuevo", Objeto.Estado);
            Hdatos.Add("@Observacion", oHistorialBE.Observacion);
            Hdatos.Add(
                "@FechaFin",
                Objeto.FechaFin.HasValue
                    ? (object)Objeto.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
                    : null);

            return oDatos.Escribir("sp_Suscripcion_CambiarEstado", Hdatos);
        }

        private BESuscripcionConPlan LeerUna(string consulta, Hashtable parametros)
        {
            return ServicioLectorFila.AObjeto(oDatos.Leer(consulta, parametros), Mapear);
        }

        private BESuscripcionConPlan Mapear(DataRow Item)
        {
            BESuscripcion oSuscripcionBE = new BESuscripcion();

            oSuscripcionBE.SuscripcionId = ServicioLectorFila.LeerEntero(Item, "SuscripcionId");
            oSuscripcionBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oSuscripcionBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
            oSuscripcionBE.FechaInicio = ServicioLectorFila.LeerFechaNula(Item, "FechaInicio") ?? default;
            oSuscripcionBE.FechaFin = ServicioLectorFila.LeerFechaNula(Item, "FechaFin");
            oSuscripcionBE.Estado = ServicioLectorFila.LeerTexto(Item, "Estado");
            oSuscripcionBE.RenovacionAutomatica = ServicioLectorFila.LeerBooleanoNulo(Item, "RenovacionAutomatica");

            return new BESuscripcionConPlan(
                oSuscripcionBE,
                ServicioLectorFila.LeerTexto(Item, "Plan"),
                ServicioLectorFila.LeerDecimal(Item, "PrecioMensual"),
                ServicioLectorFila.LeerEntero(Item, "CantidadLicencias"));
        }
    }

    public class MPPLicencia
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public int Asignar(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            if (Objeto.FechaVencimiento.HasValue)
            {
                Hdatos.Add("@FechaVencimiento", Objeto.FechaVencimiento.Value);
            }

            object identidad = oDatos.LeerEscalar("sp_Licencia_Asignar", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Revocar(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@LicenciaId", Objeto.LicenciaId);

            object filas = oDatos.LeerEscalar("sp_Licencia_Revocar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int RevocarPorSuscripcion(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            object filas = oDatos.LeerEscalar("sp_Licencia_RevocarPorSuscripcion", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        public int RevocarPorUsuario(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object filas = oDatos.LeerEscalar("sp_Licencia_RevocarPorUsuario", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        public int Migrar(int suscripcionOrigen, int suscripcionDestino, int cupoDestino, DateTime? fechaVencimiento)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionOrigen", suscripcionOrigen);
            Hdatos.Add("@SuscripcionDestino", suscripcionDestino);
            Hdatos.Add("@CupoDestino", cupoDestino);

            if (fechaVencimiento.HasValue)
            {
                Hdatos.Add("@FechaVencimiento", fechaVencimiento.Value);
            }

            DataTable Dt = oDatos.Leer("sp_Licencia_Migrar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return ServicioLectorFila.LeerEntero(Dt.Rows[0], "Migradas");
            }

            return 0;
        }

        public BELicencia ObtenerPorUsuario(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Licencia_ObtenerPorUsuario", Hdatos), Mapear);
        }

        public BELicencia ObtenerPorId(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@LicenciaId", Objeto.LicenciaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Licencia_ObtenerPorId", Hdatos), Mapear);
        }

        public int ContarActivas(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            DataTable Dt = oDatos.Leer("sp_Licencia_ContarActivas", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return ServicioLectorFila.LeerEntero(Dt.Rows[0], "Activas");
            }

            return 0;
        }

        public List<BEInventarioLicencia> ListarPorEmpresa(int empresaId, int suscripcionId, string rol)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", empresaId);
            Hdatos.Add("@SuscripcionId", suscripcionId);
            Hdatos.Add("@Rol", rol);

            return ServicioLectorFila.ALista(oDatos.Leer("sp_Licencia_ListarPorEmpresa", Hdatos), MapearInventario);
        }

        private BELicencia Mapear(DataRow Item)
        {
            BELicencia oLicenciaBE = new BELicencia();

            oLicenciaBE.LicenciaId = ServicioLectorFila.LeerEntero(Item, "LicenciaId");
            oLicenciaBE.SuscripcionId = ServicioLectorFila.LeerEntero(Item, "SuscripcionId");
            oLicenciaBE.UsuarioId = ServicioLectorFila.LeerEnteroNulo(Item, "UsuarioId");
            oLicenciaBE.CodigoLicencia = ServicioLectorFila.LeerGuidNulo(Item, "CodigoLicencia");
            oLicenciaBE.FechaAsignacion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAsignacion");
            oLicenciaBE.FechaVencimiento = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaVencimiento");
            oLicenciaBE.Estado = ServicioLectorFila.LeerTextoNulo(Item, "Estado");
            oLicenciaBE.EstadoSuscripcion = ServicioLectorFila.LeerTextoNulo(Item, "EstadoSuscripcion");

            return oLicenciaBE;
        }

        private BEInventarioLicencia MapearInventario(DataRow Item)
        {
            BEInventarioLicencia oInventarioBE = new BEInventarioLicencia();

            oInventarioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oInventarioBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oInventarioBE.Apellido = ServicioLectorFila.LeerTexto(Item, "Apellido");
            oInventarioBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oInventarioBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
            oInventarioBE.LicenciaId = ServicioLectorFila.LeerEnteroNulo(Item, "LicenciaId");
            oInventarioBE.CodigoLicencia = ServicioLectorFila.LeerGuidNulo(Item, "CodigoLicencia");
            oInventarioBE.FechaAsignacion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAsignacion");

            return oInventarioBE;
        }
    }
}
