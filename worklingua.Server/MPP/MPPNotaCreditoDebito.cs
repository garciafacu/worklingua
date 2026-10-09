using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPNotaCreditoDebito
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPNotaCreditoDebito()
        {
            oDatos = new Acceso();
        }

        public BENotaCreditoDebito Guardar(BENotaCreditoDebito Objeto, BEMovimientoCuentaCorriente oMovimientoBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@FacturaId", Objeto.FacturaId);
            Hdatos.Add("@Tipo", Objeto.Tipo);
            Hdatos.Add("@Importe", Objeto.Importe);
            Hdatos.Add("@SaldoDisponible", Objeto.SaldoDisponible);
            Hdatos.Add("@Motivo", Objeto.Motivo);
            Hdatos.Add("@Moneda", Objeto.Moneda);
            Hdatos.Add("@TasaConversion", Objeto.TasaConversion);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@TipoMovimiento", oMovimientoBE.Tipo);
            Hdatos.Add("@Debe", oMovimientoBE.Debe);
            Hdatos.Add("@Haber", oMovimientoBE.Haber);

            DataTable Dt = oDatos.Leer("sp_NotaCreditoDebito_Alta", Hdatos);

            return Dt.Rows.Count > 0 ? Mapear(Dt.Rows[0]) : null;
        }

        public List<BENotaCreditoDebito> ListarDisponibles(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BENotaCreditoDebito> ListaNotaBE = new List<BENotaCreditoDebito>();
            DataTable Dt = oDatos.Leer("sp_NotaCreditoDebito_ListarDisponiblesPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaNotaBE.Add(Mapear(Item));
                }

                return ListaNotaBE;
            }
            else
            {
                return null;
            }
        }

        public List<BENotaConFactura> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BENotaConFactura> ListaNotaBE = new List<BENotaConFactura>();
            DataTable Dt = oDatos.Leer("sp_NotaCreditoDebito_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaNotaBE.Add(new BENotaConFactura(Mapear(Item), ServicioLectorFila.LeerTexto(Item, "NumeroFactura")));
                }

                return ListaNotaBE;
            }
            else
            {
                return null;
            }
        }

        private BENotaCreditoDebito Mapear(DataRow Item)
        {
            BENotaCreditoDebito oNotaBE = new BENotaCreditoDebito();

            oNotaBE.NotaId = ServicioLectorFila.LeerEntero(Item, "NotaId");
            oNotaBE.FacturaId = ServicioLectorFila.LeerEntero(Item, "FacturaId");
            oNotaBE.Tipo = ServicioLectorFila.LeerTexto(Item, "Tipo").Trim();
            oNotaBE.Numero = ServicioLectorFila.LeerTexto(Item, "Numero");
            oNotaBE.FechaEmision = ServicioLectorFila.LeerFechaHora(Item, "FechaEmision");
            oNotaBE.Importe = ServicioLectorFila.LeerDecimal(Item, "Importe");
            oNotaBE.SaldoDisponible = ServicioLectorFila.LeerDecimal(Item, "SaldoDisponible");
            oNotaBE.Motivo = ServicioLectorFila.LeerTexto(Item, "Motivo");
            oNotaBE.Moneda = ServicioLectorFila.LeerTexto(Item, "Moneda").Trim();
            oNotaBE.TasaConversion = ServicioLectorFila.LeerDecimal(Item, "TasaConversion");
            oNotaBE.Estado = ServicioLectorFila.LeerTexto(Item, "Estado");

            return oNotaBE;
        }
    }
}
