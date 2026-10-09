using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPMovimientoCuentaCorriente
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPMovimientoCuentaCorriente()
        {
            oDatos = new Acceso();
        }

        public decimal ObtenerSaldo(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            object saldo = oDatos.LeerEscalar("sp_MovimientoCuentaCorriente_ObtenerSaldo", Hdatos);

            return saldo == null ? 0m : Convert.ToDecimal(saldo);
        }

        public List<BEMovimientoConComprobante> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BEMovimientoConComprobante> ListaMovimientoBE = new List<BEMovimientoConComprobante>();
            DataTable Dt = oDatos.Leer("sp_MovimientoCuentaCorriente_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BEMovimientoCuentaCorriente oMovimientoBE = new BEMovimientoCuentaCorriente(
                        ServicioLectorFila.LeerEntero(Item, "MovimientoId"),
                        ServicioLectorFila.LeerEntero(Item, "EmpresaId"),
                        ServicioLectorFila.LeerEnteroNulo(Item, "SuscripcionId"),
                        ServicioLectorFila.LeerEnteroNulo(Item, "FacturaId"),
                        ServicioLectorFila.LeerEnteroNulo(Item, "NotaId"),
                        ServicioLectorFila.LeerEnteroNulo(Item, "PagoId"),
                        ServicioLectorFila.LeerFechaHora(Item, "FechaMovimiento"),
                        ServicioLectorFila.LeerTexto(Item, "Tipo"),
                        ServicioLectorFila.LeerTexto(Item, "Concepto"),
                        ServicioLectorFila.LeerDecimal(Item, "Debe"),
                        ServicioLectorFila.LeerDecimal(Item, "Haber"));

                    ListaMovimientoBE.Add(new BEMovimientoConComprobante(
                        oMovimientoBE,
                        ServicioLectorFila.LeerDecimal(Item, "SaldoAcumulado"),
                        ServicioLectorFila.LeerTextoNulo(Item, "Comprobante"),
                        ServicioLectorFila.LeerTextoNulo(Item, "Plan")));
                }

                return ListaMovimientoBE;
            }
            else
            {
                return null;
            }
        }
    }
}
