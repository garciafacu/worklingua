using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLCuentaCorriente
    {
        BLLMovimientoCuentaCorriente oBLLMov = new BLLMovimientoCuentaCorriente();
        BLLFactura oBLLFac = new BLLFactura();
        BLLPago oBLLPag = new BLLPago();
        BLLNotaCreditoDebito oBLLNot = new BLLNotaCreditoDebito();
        BLLUsuario oBLLUsu = new BLLUsuario();

        public BEEstadoCuentaCorriente ListarObjeto(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BEEmpresa oEmpresaBE = new BEEmpresa();
            oEmpresaBE.EmpresaId = oUsuarioBE.EmpresaId;

            decimal saldoNotas = 0m;

            foreach (BENotaCreditoDebito oNotaBE in oBLLNot.ListarDisponibles(oEmpresaBE))
            {
                saldoNotas = saldoNotas + oNotaBE.SaldoDisponible;
            }

            return new BEEstadoCuentaCorriente(
                oBLLMov.ObtenerSaldo(oEmpresaBE),
                saldoNotas,
                Configuracion.LimiteCuentaCorriente,
                oBLLMov.ListarPorEmpresa(oEmpresaBE),
                oBLLFac.ListarPorEmpresa(oEmpresaBE),
                oBLLPag.ListarPorEmpresa(oEmpresaBE),
                oBLLNot.ListarPorEmpresa(oEmpresaBE));
        }
    }

    public class BLLFactura
    {
        public const string EstadoPagada = "PAGADA";
        public const string EstadoACuenta = "A_CUENTA";
        public const string EstadoAnulada = "ANULADA";

        MPPFactura oMPPFac = new MPPFactura();

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

    public class BLLPago
    {
        public const string MedioTarjeta = "TARJETA";
        public const string MedioNotaCredito = "NOTA_CREDITO";
        public const string MedioCuentaCorriente = "CUENTA_CORRIENTE";

        public const string EstadoAprobado = "APROBADO";
        public const string EstadoACuenta = "A_CUENTA";

        MPPPago oMPPPag = new MPPPago();

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

    public class BLLNotaCreditoDebito
    {
        public const string TipoCredito = "NC";
        public const string TipoDebito = "ND";

        public const string EstadoDisponible = "DISPONIBLE";
        public const string EstadoAplicada = "APLICADA";
        public const string EstadoEmitida = "EMITIDA";

        MPPNotaCreditoDebito oMPPNot = new MPPNotaCreditoDebito();

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

    public class BLLMovimientoCuentaCorriente
    {
        public const string TipoFactura = "FACTURA";
        public const string TipoNotaCredito = "NC";
        public const string TipoNotaDebito = "ND";
        public const string TipoPago = "PAGO";

        MPPMovimientoCuentaCorriente oMPPMov = new MPPMovimientoCuentaCorriente();

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
