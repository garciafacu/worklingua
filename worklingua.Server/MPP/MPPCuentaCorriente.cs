using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPFactura
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

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

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Factura_Alta", Hdatos), Mapear);
        }

        public BEFactura ListarPorSuscripcion(BESuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Factura_ObtenerPorSuscripcion", Hdatos), Mapear);
        }

        public List<BEFacturaConPlan> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista(
                oDatos.Leer("sp_Factura_ListarPorEmpresa", Hdatos),
                Item => new BEFacturaConPlan(Mapear(Item), ServicioLectorFila.LeerTexto(Item, "Plan")));
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

    public class MPPPago
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

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

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Pago_Alta", Hdatos), Mapear);
        }

        public List<BEPagoConFactura> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista(
                oDatos.Leer("sp_Pago_ListarPorEmpresa", Hdatos),
                Item => new BEPagoConFactura(Mapear(Item), ServicioLectorFila.LeerTexto(Item, "NumeroFactura")));
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

    public class MPPNotaCreditoDebito
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

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

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_NotaCreditoDebito_Alta", Hdatos), Mapear);
        }

        public List<BENotaCreditoDebito> ListarDisponibles(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista<BENotaCreditoDebito>(oDatos.Leer("sp_NotaCreditoDebito_ListarDisponiblesPorEmpresa", Hdatos), Mapear);
        }

        public List<BENotaConFactura> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista(
                oDatos.Leer("sp_NotaCreditoDebito_ListarPorEmpresa", Hdatos),
                Item => new BENotaConFactura(Mapear(Item), ServicioLectorFila.LeerTexto(Item, "NumeroFactura")));
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

    public class MPPMovimientoCuentaCorriente
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

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

            return ServicioLectorFila.ALista(
                oDatos.Leer("sp_MovimientoCuentaCorriente_ListarPorEmpresa", Hdatos), Mapear);
        }

        private BEMovimientoConComprobante Mapear(DataRow Item)
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

            return new BEMovimientoConComprobante(
                oMovimientoBE,
                ServicioLectorFila.LeerDecimal(Item, "SaldoAcumulado"),
                ServicioLectorFila.LeerTextoNulo(Item, "Comprobante"),
                ServicioLectorFila.LeerTextoNulo(Item, "Plan"));
        }
    }
}
