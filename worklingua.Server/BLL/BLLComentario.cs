using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLComentario
    {
        public const int LongitudMaximaTexto = 1000;
        public const int PuntajeMinimo = 1;
        public const int PuntajeMaximo = 5;

        MPPComentario oMPPCom;
        BLLPlan oBLLPlan;
        BLLBitacora oBLLBit;
        BLLSeguridad oBLLSeg;

        public BLLComentario()
        {
            oMPPCom = new MPPComentario();
            oBLLPlan = new BLLPlan();
            oBLLBit = new BLLBitacora();
            oBLLSeg = new BLLSeguridad();
        }

        public List<BEComentarioConAutor> ListarTodo(int planId)
        {
            BEPlanSuscripcion oPlanBE = ObtenerPlanVigente(planId);

            BEComentario oFiltroBE = new BEComentario();
            oFiltroBE.PlanId = planId;

            List<BEComentarioConAutor> ListaComentarioBE = oMPPCom.ListarTodo(oFiltroBE);

            if (ListaComentarioBE == null)
            {
                return new List<BEComentarioConAutor>();
            }

            foreach (BEComentarioConAutor oComentario in ListaComentarioBE)
            {
                oComentario.Plan = oPlanBE.Nombre;
            }

            return ListaComentarioBE;
        }

        public List<BEComentarioConAutor> Buscar(BEFiltroComentario Objeto)
        {
            BEFiltroComentario oFiltro = new BEFiltroComentario(
                Objeto.PlanId.HasValue && Objeto.PlanId.Value > 0 ? Objeto.PlanId : null,
                Normalizar(Objeto.Texto),
                Objeto.Desde,
                Objeto.Hasta);

            List<BEComentarioConAutor> ListaComentarioBE = oMPPCom.Buscar(oFiltro);

            return ListaComentarioBE == null ? new List<BEComentarioConAutor>() : ListaComentarioBE;
        }

        public BEComentarioConAutor Guardar(BEComentario Objeto)
        {
            BEPlanSuscripcion oPlanBE = ObtenerPlanVigente(Objeto.PlanId);
            string limpio = Objeto.Texto == null ? string.Empty : Objeto.Texto.Trim();

            if (!Objeto.Puntaje.HasValue ||
                Objeto.Puntaje.Value < PuntajeMinimo ||
                Objeto.Puntaje.Value > PuntajeMaximo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La valoración debe ser de " + PuntajeMinimo + " a " + PuntajeMaximo + " estrellas.");
            }

            if (limpio.Length > LongitudMaximaTexto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El comentario no puede superar los " + LongitudMaximaTexto + " caracteres.");
            }

            BEComentarioConAutor oExistente = oMPPCom.ListarValoracion(Objeto);
            bool esModificacion = oExistente != null;

            BEComentario oComentarioBE = new BEComentario();

            oComentarioBE.ComentarioId = esModificacion ? oExistente.Comentario.ComentarioId : 0;
            oComentarioBE.UsuarioId = Objeto.UsuarioId;
            oComentarioBE.PlanId = Objeto.PlanId;
            oComentarioBE.Texto = limpio;
            oComentarioBE.Puntaje = Objeto.Puntaje;

            oComentarioBE.ComentarioId = oMPPCom.Guardar(oComentarioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oComentarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloComentario,
                esModificacion ? "Modificacion" : "Alta",
                "Valoración " + oComentarioBE.ComentarioId + " de " + oComentarioBE.Puntaje +
                " estrellas sobre el plan " + oPlanBE.Nombre + ".",
                BLLBitacora.NivelInformativo));

            BEComentarioConAutor oGuardado = oMPPCom.ListarObjeto(oComentarioBE);

            return oGuardado == null
                ? new BEComentarioConAutor(oComentarioBE, string.Empty, oPlanBE.Nombre)
                : oGuardado;
        }

        public BEComentarioConAutor ListarValoracion(BEComentario Objeto)
        {
            ObtenerPlanVigente(Objeto.PlanId);

            return oMPPCom.ListarValoracion(Objeto);
        }

        public List<BEResumenValoracion> ListarResumen()
        {
            List<BEResumenValoracion> ListaResumenBE = oMPPCom.ListarResumen();

            return ListaResumenBE == null ? new List<BEResumenValoracion>() : ListaResumenBE;
        }

        public bool Baja(BEComentario Objeto, BESesion oSesionBE)
        {
            BEComentarioConAutor oExistente = oMPPCom.ListarObjeto(Objeto);

            if (oExistente == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La opinión no existe.");
            }

            BEComentario oComentarioBE = oExistente.Comentario;

            if (!oComentarioBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La opinión ya fue eliminada.");
            }

            bool esPropio = oComentarioBE.UsuarioId == oSesionBE.UsuarioId;
            bool puedeModerar = oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.ComentarioModerar);

            if (!esPropio && !puedeModerar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.PermisoDenegado,
                    "Solo podés eliminar tus propias opiniones.");
            }

            bool resultado = oMPPCom.Baja(oComentarioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloComentario,
                "Baja",
                "Baja lógica del comentario " + oComentarioBE.ComentarioId + " sobre el plan " +
                oExistente.Plan + (esPropio ? ", por su autor." : ", por moderación."),
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        private BEPlanSuscripcion ObtenerPlanVigente(int planId)
        {
            List<BEPlanSuscripcion> ListaPlanBE = oBLLPlan.ListarTodo();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                if (oPlanBE.PlanId == planId)
                {
                    return oPlanBE;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.NoEncontrado, "El plan indicado no existe o ya no está vigente.");
        }

        private string Normalizar(string criterio)
        {
            return string.IsNullOrWhiteSpace(criterio) ? null : criterio.Trim();
        }
    }
}
