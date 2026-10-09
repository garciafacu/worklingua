using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPFactura
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPFactura()
        {
            oDatos = new Acceso();
        }

        public BEFactura Guardar(BEFactura Objeto, BEMovimientoCuentaCorriente oMovimientoBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);
            Hdatos.Add("@FechaEmision", Objeto.FechaEmision.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@FechaVencimiento", Objeto.FechaVencimiento.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Importe", Objeto.Importe);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@Moneda", Objeto.Moneda);
            Hdatos.Add("@TasaConversion", Objeto.TasaConversion);
            Hdatos.Add("@TipoMovimiento", oMovimientoBE.Tipo);
            Hdatos.Add("@ConceptoMovimiento", oMovimientoBE.Concepto);
            Hdatos.Add("@Debe", oMovimientoBE.Debe);
            Hdatos.Add("@Haber", oMovimientoBE.Haber);

            DataTable Dt = oDatos.Leer("sp_Factura_Alta", Hdatos);

            return Dt.Rows.Count > 0 ? Mapear(Dt.Rows[0]) : null;
        }

        public BEFactura ListarPorSuscripcion(BESuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            DataTable Dt = oDatos.Leer("sp_Factura_ObtenerPorSuscripcion", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public List<BEFacturaConPlan> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BEFacturaConPlan> ListaFacturaBE = new List<BEFacturaConPlan>();
            DataTable Dt = oDatos.Leer("sp_Factura_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaFacturaBE.Add(new BEFacturaConPlan(Mapear(Item), ServicioLectorFila.LeerTexto(Item, "Plan")));
                }

                return ListaFacturaBE;
            }
            else
            {
                return null;
            }
        }

        public bool CambiarEstado(BEFactura Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@FacturaId", Objeto.FacturaId);
            Hdatos.Add("@Estado", Objeto.Estado);

            object filas = oDatos.LeerEscalar("sp_Factura_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEFactura Mapear(DataRow Item)
        {
            BEFactura oFacturaBE = new BEFactura();

            oFacturaBE.FacturaId = ServicioLectorFila.LeerEntero(Item, "FacturaId");
            oFacturaBE.SuscripcionId = ServicioLectorFila.LeerEntero(Item, "SuscripcionId");
            oFacturaBE.NumeroFactura = ServicioLectorFila.LeerTexto(Item, "NumeroFactura");
            oFacturaBE.FechaEmision = ServicioLectorFila.LeerFechaNula(Item, "FechaEmision") ?? default;
            oFacturaBE.FechaVencimiento = ServicioLectorFila.LeerFechaNula(Item, "FechaVencimiento") ?? default;
            oFacturaBE.Importe = ServicioLectorFila.LeerDecimal(Item, "Importe");
            oFacturaBE.Estado = ServicioLectorFila.LeerTexto(Item, "Estado");
            oFacturaBE.Moneda = ServicioLectorFila.LeerTexto(Item, "Moneda").Trim();
            oFacturaBE.TasaConversion = ServicioLectorFila.LeerDecimal(Item, "TasaConversion");

            return oFacturaBE;
        }
    }
}
