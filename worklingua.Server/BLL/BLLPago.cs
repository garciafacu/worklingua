using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLPago
    {
        public const string MedioTarjeta = "TARJETA";
        public const string MedioNotaCredito = "NOTA_CREDITO";
        public const string MedioCuentaCorriente = "CUENTA_CORRIENTE";

        public const string EstadoAprobado = "APROBADO";
        public const string EstadoACuenta = "A_CUENTA";

        MPPPago oMPPPag;

        public BLLPago()
        {
            oMPPPag = new MPPPago();
        }

        public BEPago Guardar(
            BEPago Objeto,
            BENotaCreditoDebito oNotaImputadaBE,
            BEMovimientoCuentaCorriente oMovimientoBE)
        {
            return oMPPPag.Guardar(Objeto, oNotaImputadaBE, oMovimientoBE);
        }

        public List<BEPagoConFactura> ListarPorEmpresa(BEEmpresa Objeto)
        {
            List<BEPagoConFactura> ListaPagoBE = oMPPPag.ListarPorEmpresa(Objeto);

            return ListaPagoBE == null ? new List<BEPagoConFactura>() : ListaPagoBE;
        }
    }
}
