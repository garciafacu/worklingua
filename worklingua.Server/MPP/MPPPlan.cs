using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPPlan
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPPlan()
        {
            oDatos = new Acceso();
        }

        public List<BEPlanSuscripcion> ListarTodo()
        {
            List<BEPlanSuscripcion> ListaPlanBE = new List<BEPlanSuscripcion>();
            DataTable Dt = oDatos.Leer("sp_Plan_Listar", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaPlanBE.Add(Mapear(Item));
                }

                return ListaPlanBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEPlanSuscripcion> ListarTodoConBajas()
        {
            List<BEPlanSuscripcion> ListaPlanBE = new List<BEPlanSuscripcion>();
            DataTable Dt = oDatos.Leer("sp_Plan_ListarTodos", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaPlanBE.Add(Mapear(Item));
                }

                return ListaPlanBE;
            }
            else
            {
                return null;
            }
        }

        public BEPlanSuscripcion ListarObjeto(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            DataTable Dt = oDatos.Leer("sp_Plan_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.PlanId != 0)
            {
                Hdatos.Add("@PlanId", Objeto.PlanId);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@Descripcion", Objeto.Descripcion);
                Hdatos.Add("@PrecioMensual", Objeto.PrecioMensual);
                Hdatos.Add("@CantidadLicencias", Objeto.CantidadLicencias);
                Hdatos.Add("@Destacado", Objeto.Destacado);

                oDatos.LeerEscalar("sp_Plan_Modificar", Hdatos);

                return Objeto.PlanId;
            }

            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@PrecioMensual", Objeto.PrecioMensual);
            Hdatos.Add("@CantidadLicencias", Objeto.CantidadLicencias);
            Hdatos.Add("@Activo", Objeto.Activo);
            Hdatos.Add("@Destacado", Objeto.Destacado);

            object identidad = oDatos.LeerEscalar("sp_Plan_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            object filas = oDatos.LeerEscalar("sp_Plan_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEPlanSuscripcion Mapear(DataRow Item)
        {
            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();

            oPlanBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
            oPlanBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oPlanBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oPlanBE.PrecioMensual = ServicioLectorFila.LeerDecimal(Item, "PrecioMensual");
            oPlanBE.CantidadLicencias = ServicioLectorFila.LeerEntero(Item, "CantidadLicencias");
            oPlanBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");
            oPlanBE.Destacado = ServicioLectorFila.LeerBooleano(Item, "Destacado");
            oPlanBE.Protegido = ServicioLectorFila.LeerBooleano(Item, "Protegido");

            return oPlanBE;
        }
    }
}
