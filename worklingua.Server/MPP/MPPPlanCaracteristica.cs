using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPPlanCaracteristica
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPPlanCaracteristica()
        {
            oDatos = new Acceso();
        }

        public List<BEPlanCaracteristica> ListarTodo()
        {
            DataTable Dt = oDatos.Leer("sp_PlanCaracteristica_Listar", null);

            return Mapear(Dt);
        }

        public List<BEPlanCaracteristica> ListarPorPlan(BEPlanSuscripcion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            DataTable Dt = oDatos.Leer("sp_PlanCaracteristica_ListarPorPlan", Hdatos);

            return Mapear(Dt);
        }

        public int Guardar(BEPlanCaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);
            Hdatos.Add("@Incluido", Objeto.Incluido);
            Hdatos.Add("@Detalle", Objeto.Detalle);

            object identidad = oDatos.LeerEscalar("sp_PlanCaracteristica_Guardar", Hdatos);

            return ServicioLectorFila.ATotalFilas(identidad);
        }

        public bool Baja(BEPlanCaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

            object filas = oDatos.LeerEscalar("sp_PlanCaracteristica_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BEPlanCaracteristica> Mapear(DataTable Dt)
        {
            List<BEPlanCaracteristica> ListaPlanCaracteristicaBE = new List<BEPlanCaracteristica>();

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BEPlanCaracteristica oPlanCaracteristicaBE = new BEPlanCaracteristica();

                    oPlanCaracteristicaBE.PlanCaracteristicaId = ServicioLectorFila.LeerEntero(Item, "PlanCaracteristicaId");
                    oPlanCaracteristicaBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
                    oPlanCaracteristicaBE.CaracteristicaId = ServicioLectorFila.LeerEntero(Item, "CaracteristicaId");
                    oPlanCaracteristicaBE.Incluido = ServicioLectorFila.LeerBooleano(Item, "Incluido");
                    oPlanCaracteristicaBE.Detalle = ServicioLectorFila.LeerTextoNulo(Item, "Detalle");

                    ListaPlanCaracteristicaBE.Add(oPlanCaracteristicaBE);
                }

                return ListaPlanCaracteristicaBE;
            }
            else
            {
                return null;
            }
        }
    }
}
