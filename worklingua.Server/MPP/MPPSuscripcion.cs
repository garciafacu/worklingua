using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPSuscripcion
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPSuscripcion()
        {
            oDatos = new Acceso();
        }

        public BESuscripcionConPlan ListarObjeto(BESuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@Estado", Objeto.Estado);

            return LeerUna("sp_Suscripcion_ObtenerPorEmpresa", Hdatos);
        }

        public BESuscripcionConPlan ListarPorId(BESuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            return LeerUna("sp_Suscripcion_ObtenerPorId", Hdatos);
        }

        public List<BESuscripcionConPlan> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BESuscripcionConPlan> ListaSuscripcionBE = new List<BESuscripcionConPlan>();
            DataTable Dt = oDatos.Leer("sp_Suscripcion_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaSuscripcionBE.Add(Mapear(Item));
                }

                return ListaSuscripcionBE;
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BESuscripcion Objeto)
        {
            if (Objeto.SuscripcionId != 0)
            {
                throw new NotImplementedException(
                    "No existe sp_Suscripcion_Modificar: la suscripción no se edita. " +
                    "El estado y la fecha de fin cambian con CambiarEstado.");
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@FechaInicio", Objeto.FechaInicio.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add(
                "@FechaFin",
                Objeto.FechaFin.HasValue
                    ? (object)Objeto.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
                    : null);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@RenovacionAutomatica", Objeto.RenovacionAutomatica);

            object identidad = oDatos.LeerEscalar("sp_Suscripcion_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool CambiarEstado(BESuscripcion Objeto, BEHistorialSuscripcion oHistorialBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);
            Hdatos.Add("@EstadoNuevo", Objeto.Estado);
            Hdatos.Add("@Observacion", oHistorialBE.Observacion);
            Hdatos.Add(
                "@FechaFin",
                Objeto.FechaFin.HasValue
                    ? (object)Objeto.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
                    : null);

            return oDatos.Escribir("sp_Suscripcion_CambiarEstado", Hdatos);
        }

        private BESuscripcionConPlan LeerUna(string consulta, Hashtable parametros)
        {
            DataTable Dt = oDatos.Leer(consulta, parametros);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        private BESuscripcionConPlan Mapear(DataRow Item)
        {
            BESuscripcion oSuscripcionBE = new BESuscripcion();

            oSuscripcionBE.SuscripcionId = ServicioLectorFila.LeerEntero(Item, "SuscripcionId");
            oSuscripcionBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oSuscripcionBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
            oSuscripcionBE.FechaInicio = ServicioLectorFila.LeerFechaNula(Item, "FechaInicio") ?? default;
            oSuscripcionBE.FechaFin = ServicioLectorFila.LeerFechaNula(Item, "FechaFin");
            oSuscripcionBE.Estado = ServicioLectorFila.LeerTexto(Item, "Estado");
            oSuscripcionBE.RenovacionAutomatica = ServicioLectorFila.LeerBooleanoNulo(Item, "RenovacionAutomatica");

            return new BESuscripcionConPlan(
                oSuscripcionBE,
                ServicioLectorFila.LeerTexto(Item, "Plan"),
                ServicioLectorFila.LeerDecimal(Item, "PrecioMensual"),
                ServicioLectorFila.LeerEntero(Item, "CantidadLicencias"));
        }
    }
}
