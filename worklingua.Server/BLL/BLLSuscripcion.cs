using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLSuscripcion
    {
        public const string EstadoPendiente = "PENDIENTE";
        public const string EstadoActiva = "ACTIVA";
        public const string EstadoRechazada = "RECHAZADA";
        public const string EstadoCancelada = "CANCELADA";
        public const string EstadoFinalizada = "FINALIZADA";

        MPPSuscripcion oMPPSus;
        BLLPlan oBLLPlan;
        BLLBitacora oBLLBit;

        public BLLSuscripcion()
        {
            oMPPSus = new MPPSuscripcion();
            oBLLPlan = new BLLPlan();
            oBLLBit = new BLLBitacora();
        }

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
                0,
                Objeto.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloSuscripcion,
                "Alta",
                "Suscripción " + oSuscripcionBE.SuscripcionId + " al plan " + oPlanBE.Nombre +
                " para la empresa " + Objeto.EmpresaId + ".",
                BLLBitacora.NivelInformativo));

            return oSuscripcionBE;
        }
    }
}
