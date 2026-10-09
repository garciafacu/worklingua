using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLPlanCaracteristica
    {
        private const int LongitudMaximaDetalle = 100;

        MPPPlanCaracteristica oMPPPlaCar;
        BLLCaracteristica oBLLCar;
        BLLPlan oBLLPlan;

        public BLLPlanCaracteristica()
        {
            oMPPPlaCar = new MPPPlanCaracteristica();
            oBLLCar = new BLLCaracteristica();
            oBLLPlan = new BLLPlan();
        }

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

            if (limpio.Length > LongitudMaximaDetalle)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El detalle no puede superar los " + LongitudMaximaDetalle + " caracteres.");
            }

            return limpio;
        }
    }
}
