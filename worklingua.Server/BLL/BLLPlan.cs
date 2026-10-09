using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLPlan
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaDescripcion = 500;

        MPPPlan oMPPPlan;

        public BLLPlan()
        {
            oMPPPlan = new MPPPlan();
        }

        public List<BEPlanSuscripcion> ListarTodo()
        {
            List<BEPlanSuscripcion> ListaPlanBE = oMPPPlan.ListarTodo();

            return ListaPlanBE == null ? new List<BEPlanSuscripcion>() : ListaPlanBE;
        }

        public List<BEPlanSuscripcion> ListarTodoConBajas()
        {
            List<BEPlanSuscripcion> ListaPlanBE = oMPPPlan.ListarTodoConBajas();

            return ListaPlanBE == null ? new List<BEPlanSuscripcion>() : ListaPlanBE;
        }

        public BEPlanSuscripcion ListarObjeto(BEPlanSuscripcion Objeto)
        {
            BEPlanSuscripcion oPlanBE = oMPPPlan.ListarObjeto(Objeto);

            if (oPlanBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El plan no existe.");
            }

            return oPlanBE;
        }

        public BEPlanSuscripcion Guardar(BEPlanSuscripcion Objeto)
        {
            Validar(Objeto);

            if (Objeto.PlanId != 0)
            {
                ListarObjeto(Objeto);
            }

            ValidarNombreDisponible(Objeto);

            Objeto.PlanId = oMPPPlan.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public BEPlanSuscripcion ObtenerPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            List<BEPlanSuscripcion> ListaPlanBE = ListarTodo();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                if (string.Equals(
                        oPlanBE.Nombre.Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return oPlanBE;
                }
            }

            return null;
        }

        public bool Baja(BEPlanSuscripcion Objeto)
        {
            BEPlanSuscripcion oPlanBE = ListarObjeto(Objeto);

            if (oPlanBE.Protegido)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El plan " + oPlanBE.Nombre + " está protegido y no se puede dar de baja.");
            }

            if (!oPlanBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "El plan ya está dado de baja.");
            }

            return oMPPPlan.Baja(Objeto);
        }

        private void Validar(BEPlanSuscripcion Objeto)
        {
            Objeto.Nombre = Normalizar(Objeto.Nombre);
            Objeto.Descripcion = Normalizar(Objeto.Descripcion);

            if (string.IsNullOrWhiteSpace(Objeto.Nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre del plan es obligatorio.");
            }

            if (Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre del plan no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }

            if (Objeto.PrecioMensual < 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El precio mensual no puede ser negativo.");
            }

            if (Objeto.CantidadLicencias < 1)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El plan debe incluir al menos una licencia.");
            }
        }

        private void ValidarNombreDisponible(BEPlanSuscripcion Objeto)
        {
            List<BEPlanSuscripcion> ListaPlanBE = ListarTodoConBajas();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                bool mismoNombre = string.Equals(
                    oPlanBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase);

                if (mismoNombre && oPlanBE.PlanId != Objeto.PlanId)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto, "Ya existe un plan con ese nombre.");
                }
            }
        }

        private string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }
    }
}
