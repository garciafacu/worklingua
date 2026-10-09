using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLNotaCreditoDebito
    {
        public const string TipoCredito = "NC";
        public const string TipoDebito = "ND";

        public const string EstadoDisponible = "DISPONIBLE";
        public const string EstadoAplicada = "APLICADA";
        public const string EstadoEmitida = "EMITIDA";

        MPPNotaCreditoDebito oMPPNot;

        public BLLNotaCreditoDebito()
        {
            oMPPNot = new MPPNotaCreditoDebito();
        }

        public BENotaCreditoDebito Guardar(BENotaCreditoDebito Objeto, BEMovimientoCuentaCorriente oMovimientoBE)
        {
            return oMPPNot.Guardar(Objeto, oMovimientoBE);
        }

        public List<BENotaCreditoDebito> ListarDisponibles(BEEmpresa Objeto)
        {
            List<BENotaCreditoDebito> ListaNotaBE = oMPPNot.ListarDisponibles(Objeto);

            return ListaNotaBE == null ? new List<BENotaCreditoDebito>() : ListaNotaBE;
        }

        public List<BENotaConFactura> ListarPorEmpresa(BEEmpresa Objeto)
        {
            List<BENotaConFactura> ListaNotaBE = oMPPNot.ListarPorEmpresa(Objeto);

            return ListaNotaBE == null ? new List<BENotaConFactura>() : ListaNotaBE;
        }
    }
}
