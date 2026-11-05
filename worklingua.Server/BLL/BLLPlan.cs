using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLPlan
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaDescripcion = 500;

        MPPPlan oMPPPlan = new MPPPlan();

        public List<BEPlanSuscripcion> ListarTodo()
        {
            List<BEPlanSuscripcion> ListaPlanBE = oMPPPlan.ListarTodo();

            return ListaPlanBE == null ? new List<BEPlanSuscripcion>() : ListaPlanBE;
        }

        private List<BEPlanSuscripcion> ListarTodoConBajas()
        {
            List<BEPlanSuscripcion> ListaPlanBE = oMPPPlan.ListarTodoConBajas();

            return ListaPlanBE == null ? new List<BEPlanSuscripcion>() : ListaPlanBE;
        }

        public BEPlanSuscripcion ListarObjeto(BEPlanSuscripcion Objeto)
        {
            BEPlanSuscripcion oPlanBE = oMPPPlan.ListarObjeto(Objeto);

            if (oPlanBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El plan no existe.");
            }

            return oPlanBE;
        }

        public BEPlanSuscripcion Guardar(BEPlanSuscripcion Objeto)
        {
            Validar(Objeto);

            if (Objeto.PlanId != 0)
            {
                ListarObjeto(Objeto);
            }

            ValidarNombreDisponible(Objeto);

            Objeto.PlanId = oMPPPlan.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public BEPlanSuscripcion ObtenerPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            List<BEPlanSuscripcion> ListaPlanBE = ListarTodo();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                if (string.Equals(
                        oPlanBE.Nombre.Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return oPlanBE;
                }
            }

            return null;
        }

        public bool Baja(BEPlanSuscripcion Objeto)
        {
            BEPlanSuscripcion oPlanBE = ListarObjeto(Objeto);

            if (oPlanBE.Protegido)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El plan " + oPlanBE.Nombre + " está protegido y no se puede dar de baja.");
            }

            if (!oPlanBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "El plan ya está dado de baja.");
            }

            return oMPPPlan.Baja(Objeto);
        }

        private void Validar(BEPlanSuscripcion Objeto)
        {
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.Descripcion = ServicioTexto.Normalizar(Objeto.Descripcion);

            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del plan es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre del plan no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");

            if (Objeto.PrecioMensual < 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El precio mensual no puede ser negativo.");
            }

            if (Objeto.CantidadLicencias < 1)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El plan debe incluir al menos una licencia.");
            }
        }

        private void ValidarNombreDisponible(BEPlanSuscripcion Objeto)
        {
            List<BEPlanSuscripcion> ListaPlanBE = ListarTodoConBajas();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                bool mismoNombre = string.Equals(
                    oPlanBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase);

                if (mismoNombre && oPlanBE.PlanId != Objeto.PlanId)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto, "Ya existe un plan con ese nombre.");
                }
            }
        }

        public void Validar(BEGuardarPlan Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del plan es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 100, "El nombre no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 500,
                "La descripción no puede superar los 500 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.PrecioMensual, 0m, 99999999.99m,
                "El precio mensual no puede ser negativo.");

            ReglasNegocio.ExigirRango(Objeto.CantidadLicencias, 1, int.MaxValue,
                "El plan debe incluir al menos una licencia.");
        }
    }

    public class BLLPlanCaracteristica
    {
        private const int LongitudMaximaDetalle = 100;

        MPPPlanCaracteristica oMPPPlaCar = new MPPPlanCaracteristica();
        BLLCaracteristica oBLLCar = new BLLCaracteristica();
        BLLPlan oBLLPlan = new BLLPlan();

        public List<BEPlanCaracteristica> ListarTodo()
        {
            List<BEPlanCaracteristica> ListaPlanCaracteristicaBE = oMPPPlaCar.ListarTodo();

            return ListaPlanCaracteristicaBE == null
                ? new List<BEPlanCaracteristica>()
                : ListaPlanCaracteristicaBE;
        }

        public List<BEPlanCaracteristica> ListarPorPlan(BEPlanSuscripcion Objeto)
        {
            oBLLPlan.ListarObjeto(Objeto);

            List<BEPlanCaracteristica> ListaPlanCaracteristicaBE = oMPPPlaCar.ListarPorPlan(Objeto);

            return ListaPlanCaracteristicaBE == null
                ? new List<BEPlanCaracteristica>()
                : ListaPlanCaracteristicaBE;
        }

        public List<BEPlanCaracteristica> Reemplazar(
            BEPlanSuscripcion oPlanBE, List<BEPlanCaracteristica> ListaPedida)
        {
            oBLLPlan.ListarObjeto(oPlanBE);

            List<BEPlanCaracteristica> ListaValidada = Validar(oPlanBE, ListaPedida);
            List<BEPlanCaracteristica> ListaActual = ListarPorPlan(oPlanBE);

            foreach (BEPlanCaracteristica oActualBE in ListaActual)
            {
                if (!Contiene(ListaValidada, oActualBE.CaracteristicaId))
                {
                    oMPPPlaCar.Baja(oActualBE);
                }
            }

            foreach (BEPlanCaracteristica oPedidaBE in ListaValidada)
            {
                oMPPPlaCar.Guardar(oPedidaBE);
            }

            return ListarPorPlan(oPlanBE);
        }

        private List<BEPlanCaracteristica> Validar(
            BEPlanSuscripcion oPlanBE, List<BEPlanCaracteristica> ListaPedida)
        {
            List<BEPlanCaracteristica> ListaValidada = new List<BEPlanCaracteristica>();

            if (ListaPedida == null)
            {
                return ListaValidada;
            }

            List<BECaracteristica> ListaCaracteristicaBE = oBLLCar.ListarTodo();

            foreach (BEPlanCaracteristica oPedidaBE in ListaPedida)
            {
                if (!ExisteCaracteristica(ListaCaracteristicaBE, oPedidaBE.CaracteristicaId))
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "Una de las características seleccionadas no existe o está dada de baja.");
                }

                if (Contiene(ListaValidada, oPedidaBE.CaracteristicaId))
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "Una característica no puede repetirse dentro del mismo plan.");
                }

                BEPlanCaracteristica oValidadaBE = new BEPlanCaracteristica();

                oValidadaBE.PlanId = oPlanBE.PlanId;
                oValidadaBE.CaracteristicaId = oPedidaBE.CaracteristicaId;
                oValidadaBE.Incluido = oPedidaBE.Incluido;
                oValidadaBE.Detalle = NormalizarDetalle(oPedidaBE.Detalle);

                ListaValidada.Add(oValidadaBE);
            }

            return ListaValidada;
        }

        private bool ExisteCaracteristica(List<BECaracteristica> Lista, int caracteristicaId)
        {
            foreach (BECaracteristica oCaracteristicaBE in Lista)
            {
                if (oCaracteristicaBE.CaracteristicaId == caracteristicaId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool Contiene(List<BEPlanCaracteristica> Lista, int caracteristicaId)
        {
            foreach (BEPlanCaracteristica oPlanCaracteristicaBE in Lista)
            {
                if (oPlanCaracteristicaBE.CaracteristicaId == caracteristicaId)
                {
                    return true;
                }
            }

            return false;
        }

        private string NormalizarDetalle(string detalle)
        {
            if (string.IsNullOrWhiteSpace(detalle))
            {
                return null;
            }

            string limpio = detalle.Trim();

            ReglasNegocio.ExigirLargoMaximo(limpio, LongitudMaximaDetalle,
                "El detalle no puede superar los " + LongitudMaximaDetalle + " caracteres.");

            return limpio;
        }

        public void Validar(BEGuardarPlanCaracteristicas Objeto)
        {
            if (Objeto.Caracteristicas != null)
            {
                foreach (BEItemPlanCaracteristica oItemBE in Objeto.Caracteristicas)
                {
                    Validar(oItemBE);
                }
            }

            ReglasNegocio.ExigirValor(Objeto.Caracteristicas, "Falta la lista de características.");
        }

        private void Validar(BEItemPlanCaracteristica Objeto)
        {
            if (Objeto == null)
            {
                return;
            }

            ReglasNegocio.ExigirRango(Objeto.CaracteristicaId, 1, int.MaxValue,
                "La característica seleccionada no es válida.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Detalle, 100, "El detalle no puede superar los 100 caracteres.");
        }
    }

    public class BLLCaracteristica
    {
        private const int LongitudMaximaNombre = 100;
        private const int OrdenMaximo = 100000;

        MPPCaracteristica oMPPCar = new MPPCaracteristica();

        public List<BECaracteristica> ListarTodo()
        {
            List<BECaracteristica> ListaCaracteristicaBE = oMPPCar.ListarTodo();

            return ListaCaracteristicaBE == null
                ? new List<BECaracteristica>()
                : ListaCaracteristicaBE;
        }

        private List<BECaracteristica> ListarTodoConBajas()
        {
            List<BECaracteristica> ListaCaracteristicaBE = oMPPCar.ListarTodoConBajas();

            return ListaCaracteristicaBE == null
                ? new List<BECaracteristica>()
                : ListaCaracteristicaBE;
        }

        public BECaracteristica ListarObjeto(BECaracteristica Objeto)
        {
            BECaracteristica oCaracteristicaBE = oMPPCar.ListarObjeto(Objeto);

            if (oCaracteristicaBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado, "La característica no existe.");
            }

            return oCaracteristicaBE;
        }

        public BECaracteristica Guardar(BECaracteristica Objeto)
        {
            Validar(Objeto);

            if (Objeto.CaracteristicaId != 0)
            {
                ListarObjeto(Objeto);
            }

            ValidarNombreDisponible(Objeto);

            Objeto.CaracteristicaId = oMPPCar.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public bool Baja(BECaracteristica Objeto)
        {
            BECaracteristica oCaracteristicaBE = ListarObjeto(Objeto);

            if (!oCaracteristicaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La característica ya está dada de baja.");
            }

            return oMPPCar.Baja(Objeto);
        }

        private void Validar(BECaracteristica Objeto)
        {
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);

            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre de la característica es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");

            if (Objeto.Orden < 0 || Objeto.Orden > OrdenMaximo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El orden debe estar entre 0 y " + OrdenMaximo + ".");
            }
        }

        private void ValidarNombreDisponible(BECaracteristica Objeto)
        {
            List<BECaracteristica> ListaCaracteristicaBE = ListarTodoConBajas();

            foreach (BECaracteristica oCaracteristicaBE in ListaCaracteristicaBE)
            {
                bool mismoNombre = string.Equals(
                    oCaracteristicaBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase);

                if (mismoNombre && oCaracteristicaBE.CaracteristicaId != Objeto.CaracteristicaId)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto, "Ya existe una característica con ese nombre.");
                }
            }
        }

        public void Validar(BEGuardarCaracteristica Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre de la característica es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 100, "El nombre no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.Orden, 0, 100000, "El orden debe estar entre 0 y 100000.");
        }
    }

    public class BLLSuscripcion
    {
        public const string EstadoPendiente = "PENDIENTE";
        public const string EstadoActiva = "ACTIVA";
        public const string EstadoRechazada = "RECHAZADA";
        public const string EstadoCancelada = "CANCELADA";
        public const string EstadoFinalizada = "FINALIZADA";

        MPPSuscripcion oMPPSus = new MPPSuscripcion();
        BLLPlan oBLLPlan = new BLLPlan();
        BLLBitacora oBLLBit = new BLLBitacora();

        public BESuscripcionConPlan ListarObjeto(BESuscripcion Objeto)
        {
            Objeto.Estado = EstadoActiva;

            return oMPPSus.ListarObjeto(Objeto);
        }

        public BESuscripcionConPlan ListarPorId(BESuscripcion Objeto)
        {
            BESuscripcionConPlan oSuscripcion = oMPPSus.ListarPorId(Objeto);

            if (oSuscripcion == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La contratación no existe.");
            }

            return oSuscripcion;
        }

        public List<BESuscripcionConPlan> ListarPorEmpresa(BEEmpresa Objeto)
        {
            List<BESuscripcionConPlan> ListaSuscripcionBE = oMPPSus.ListarPorEmpresa(Objeto);

            return ListaSuscripcionBE == null ? new List<BESuscripcionConPlan>() : ListaSuscripcionBE;
        }

        public int Guardar(BESuscripcion Objeto)
        {
            return oMPPSus.Guardar(Objeto);
        }

        public bool CambiarEstado(BESuscripcion Objeto, BEHistorialSuscripcion oHistorialBE)
        {
            return oMPPSus.CambiarEstado(Objeto, oHistorialBE);
        }

        public bool EsCancelable(BESuscripcionConPlan Objeto)
        {
            if (Objeto == null)
            {
                return false;
            }

            return Objeto.Suscripcion.Estado == EstadoActiva && Objeto.PrecioMensual > 0;
        }

        public BESuscripcion AsignarPlanInicial(BEUsuario Objeto)
        {
            BEPlanSuscripcion oPlanBE = oBLLPlan.ObtenerPorNombre(Configuracion.PlanPorDefecto);

            if (oPlanBE == null)
            {
                throw new InvalidOperationException(
                    "El plan '" + Configuracion.PlanPorDefecto + "' configurado en " +
                    "Registro:PlanPorDefecto no existe entre los planes activos de la tabla " +
                    "PlanSuscripcion. Ejecutar docs/seed.sql contra WorkLinguaDB.");
            }

            BESuscripcion oSuscripcionBE = new BESuscripcion();

            oSuscripcionBE.EmpresaId = Objeto.EmpresaId;
            oSuscripcionBE.PlanId = oPlanBE.PlanId;
            oSuscripcionBE.FechaInicio = DateOnly.FromDateTime(DateTime.Now);
            oSuscripcionBE.FechaFin = null;
            oSuscripcionBE.Estado = EstadoActiva;
            oSuscripcionBE.RenovacionAutomatica = true;

            oSuscripcionBE.SuscripcionId = oMPPSus.Guardar(oSuscripcionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, Objeto.UsuarioId, DateTime.Now, BLLBitacora.ModuloSuscripcion, "Alta",
                "Suscripción " + oSuscripcionBE.SuscripcionId + " al plan " + oPlanBE.Nombre + " para la empresa " + Objeto.EmpresaId + ".",
                BLLBitacora.NivelInformativo));

            return oSuscripcionBE;
        }
    }
}
