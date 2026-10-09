using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLCuentaCorriente
    {
        BLLMovimientoCuentaCorriente oBLLMov;
        BLLFactura oBLLFac;
        BLLPago oBLLPag;
        BLLNotaCreditoDebito oBLLNot;
        BLLUsuario oBLLUsu;

        public BLLCuentaCorriente()
        {
            oBLLMov = new BLLMovimientoCuentaCorriente();
            oBLLFac = new BLLFactura();
            oBLLPag = new BLLPago();
            oBLLNot = new BLLNotaCreditoDebito();
            oBLLUsu = new BLLUsuario();
        }

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
}
