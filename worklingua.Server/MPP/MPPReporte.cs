using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    /// <summary>
    /// Consultas de los reportes. Son de solo lectura: agregan lo que ya
    /// grabaron la contratación y la facturación.
    /// </summary>
    public class MPPReporte
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPReporte()
        {
            oDatos = new Acceso();
        }

        public List<BEGananciaPeriodo> ListarGanancias(BEFiltroReporte Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Desde", Objeto.Desde.Value.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Hasta", Objeto.Hasta.Value.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Agrupacion", Objeto.Agrupacion);
            Hdatos.Add("@Provincia", Objeto.Provincia);

            List<BEGananciaPeriodo> ListaGananciaBE = new List<BEGananciaPeriodo>();
            DataTable Dt = oDatos.Leer("sp_Reporte_Ganancias", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaGananciaBE.Add(new BEGananciaPeriodo(
                    ServicioLectorFila.LeerTexto(Item, "Clave"),
                    ServicioLectorFila.LeerDecimal(Item, "Facturado"),
                    ServicioLectorFila.LeerDecimal(Item, "Cobrado"),
                    ServicioLectorFila.LeerDecimal(Item, "NotasCredito"),
                    ServicioLectorFila.LeerDecimal(Item, "Neto")));
            }

            return ListaGananciaBE;
        }

        public List<BEGananciaZona> ListarGananciasPorZona(BEFiltroReporte Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Desde", Objeto.Desde.Value.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Hasta", Objeto.Hasta.Value.ToDateTime(TimeOnly.MinValue));

            List<BEGananciaZona> ListaZonaBE = new List<BEGananciaZona>();
            DataTable Dt = oDatos.Leer("sp_Reporte_GananciasPorZona", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaZonaBE.Add(new BEGananciaZona(
                    ServicioLectorFila.LeerTexto(Item, "Zona"),
                    ServicioLectorFila.LeerDecimal(Item, "Facturado"),
                    ServicioLectorFila.LeerDecimal(Item, "Cobrado"),
                    ServicioLectorFila.LeerDecimal(Item, "NotasCredito"),
                    ServicioLectorFila.LeerDecimal(Item, "Neto"),
                    ServicioLectorFila.LeerEntero(Item, "Empresas")));
            }

            return ListaZonaBE;
        }

        public BETablero ObtenerTablero()
        {
            Hdatos = new Hashtable();

            DataTable Dt = oDatos.Leer("sp_Reporte_Tablero", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                return new BETablero();
            }

            DataRow Item = Dt.Rows[0];

            BETablero oTableroBE = new BETablero();

            oTableroBE.IngresosMes = ServicioLectorFila.LeerDecimal(Item, "IngresosMes");
            oTableroBE.IngresosMesAnterior = ServicioLectorFila.LeerDecimal(Item, "IngresosMesAnterior");
            oTableroBE.DeudaTotal = ServicioLectorFila.LeerDecimal(Item, "DeudaTotal");
            oTableroBE.EmpresasConDeuda = ServicioLectorFila.LeerEntero(Item, "EmpresasConDeuda");
            oTableroBE.EmpresasActivas = ServicioLectorFila.LeerEntero(Item, "EmpresasActivas");
            oTableroBE.ContratacionesActivas = ServicioLectorFila.LeerEntero(Item, "ContratacionesActivas");
            oTableroBE.LicenciasAsignadas = ServicioLectorFila.LeerEntero(Item, "LicenciasAsignadas");
            oTableroBE.LicenciasContratadas = ServicioLectorFila.LeerEntero(Item, "LicenciasContratadas");
            oTableroBE.EmpresasSinCupo = ServicioLectorFila.LeerEntero(Item, "EmpresasSinCupo");

            return oTableroBE;
        }

        public List<BEContratacionPorPlan> ListarContratacionesPorPlan()
        {
            Hdatos = new Hashtable();

            List<BEContratacionPorPlan> ListaPlanBE = new List<BEContratacionPorPlan>();
            DataTable Dt = oDatos.Leer("sp_Reporte_ContratacionesPorPlan", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaPlanBE.Add(new BEContratacionPorPlan(
                    ServicioLectorFila.LeerTexto(Item, "Plan"),
                    ServicioLectorFila.LeerEntero(Item, "Cantidad")));
            }

            return ListaPlanBE;
        }
    }
}
