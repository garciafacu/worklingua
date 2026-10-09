using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLMovimientoCuentaCorriente
    {
        public const string TipoFactura = "FACTURA";
        public const string TipoNotaCredito = "NC";
        public const string TipoNotaDebito = "ND";
        public const string TipoPago = "PAGO";

        MPPMovimientoCuentaCorriente oMPPMov;

        public BLLMovimientoCuentaCorriente()
        {
            oMPPMov = new MPPMovimientoCuentaCorriente();
        }

        public decimal ObtenerSaldo(BEEmpresa Objeto)
        {
            return oMPPMov.ObtenerSaldo(Objeto);
        }

        public List<BEMovimientoConComprobante> ListarPorEmpresa(BEEmpresa Objeto)
        {
            List<BEMovimientoConComprobante> ListaMovimientoBE = oMPPMov.ListarPorEmpresa(Objeto);

            return ListaMovimientoBE == null ? new List<BEMovimientoConComprobante>() : ListaMovimientoBE;
        }
    }
}
