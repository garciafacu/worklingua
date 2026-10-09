using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLFactura
    {
        public const string EstadoPagada = "PAGADA";
        public const string EstadoACuenta = "A_CUENTA";
        public const string EstadoAnulada = "ANULADA";

        MPPFactura oMPPFac;

        public BLLFactura()
        {
            oMPPFac = new MPPFactura();
        }

        public BEFactura Guardar(BEFactura Objeto, BEMovimientoCuentaCorriente oMovimientoBE)
        {
            return oMPPFac.Guardar(Objeto, oMovimientoBE);
        }

        public BEFactura ListarPorSuscripcion(BESuscripcion Objeto)
        {
            return oMPPFac.ListarPorSuscripcion(Objeto);
        }

        public List<BEFacturaConPlan> ListarPorEmpresa(BEEmpresa Objeto)
        {
            List<BEFacturaConPlan> ListaFacturaBE = oMPPFac.ListarPorEmpresa(Objeto);

            return ListaFacturaBE == null ? new List<BEFacturaConPlan>() : ListaFacturaBE;
        }

        public bool CambiarEstado(BEFactura Objeto)
        {
            return oMPPFac.CambiarEstado(Objeto);
        }
    }
}
