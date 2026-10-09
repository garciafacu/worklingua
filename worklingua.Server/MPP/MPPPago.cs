using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPPago
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPPago()
        {
            oDatos = new Acceso();
        }

        public BEPago Guardar(
            BEPago Objeto,
            BENotaCreditoDebito oNotaImputadaBE,
            BEMovimientoCuentaCorriente oMovimientoBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@FacturaId", Objeto.FacturaId);
            Hdatos.Add("@FechaPago", Objeto.FechaPago);
            Hdatos.Add("@MedioPago", Objeto.MedioPago);
            Hdatos.Add("@Importe", Objeto.Importe);
            Hdatos.Add("@NumeroOperacion", Objeto.NumeroOperacion);
            Hdatos.Add("@Estado", Objeto.Estado);

            if (oNotaImputadaBE != null)
            {
                Hdatos.Add("@NotaId", oNotaImputadaBE.NotaId);
                Hdatos.Add("@EstadoNota", oNotaImputadaBE.Estado);
            }

            if (oMovimientoBE != null)
            {
                Hdatos.Add("@TipoMovimiento", oMovimientoBE.Tipo);
                Hdatos.Add("@ConceptoMovimiento", oMovimientoBE.Concepto);
                Hdatos.Add("@Debe", oMovimientoBE.Debe);
                Hdatos.Add("@Haber", oMovimientoBE.Haber);
            }

            DataTable Dt = oDatos.Leer("sp_Pago_Alta", Hdatos);

            return Dt.Rows.Count > 0 ? Mapear(Dt.Rows[0]) : null;
        }

        public List<BEPagoConFactura> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BEPagoConFactura> ListaPagoBE = new List<BEPagoConFactura>();
            DataTable Dt = oDatos.Leer("sp_Pago_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaPagoBE.Add(new BEPagoConFactura(Mapear(Item), ServicioLectorFila.LeerTexto(Item, "NumeroFactura")));
                }

                return ListaPagoBE;
            }
            else
            {
                return null;
            }
        }

        private BEPago Mapear(DataRow Item)
        {
            BEPago oPagoBE = new BEPago();

            oPagoBE.PagoId = ServicioLectorFila.LeerEntero(Item, "PagoId");
            oPagoBE.FacturaId = ServicioLectorFila.LeerEntero(Item, "FacturaId");
            oPagoBE.FechaPago = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaPago");
            oPagoBE.MedioPago = ServicioLectorFila.LeerTextoNulo(Item, "MedioPago");
            oPagoBE.Importe = ServicioLectorFila.TieneValor(Item, "Importe")
                ? ServicioLectorFila.LeerDecimal(Item, "Importe")
                : (decimal?)null;
            oPagoBE.NumeroOperacion = ServicioLectorFila.LeerTextoNulo(Item, "NumeroOperacion");
            oPagoBE.Estado = ServicioLectorFila.LeerTextoNulo(Item, "Estado");

            return oPagoBE;
        }
    }
}
