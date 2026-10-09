using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/planes")]
    public class PlanController : ControladorBase
    {
        BLLPlan oBLLPlan;
        BLLPlanCaracteristica oBLLPlaCar;
        BLLBitacora oBLLBit;
        BLLComentario oBLLCom;

        public PlanController()
        {
            oBLLPlan = new BLLPlan();
            oBLLPlaCar = new BLLPlanCaracteristica();
            oBLLBit = new BLLBitacora();
            oBLLCom = new BLLComentario();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEPlanRespuesta>), StatusCodes.Status200OK)]
        public ActionResult<List<BEPlanRespuesta>> Listar()
        {
            List<BEPlanSuscripcion> ListaPlanBE = oBLLPlan.ListarTodo();
            List<BEPlanCaracteristica> ListaMatrizBE = oBLLPlaCar.ListarTodo();
            List<BEResumenValoracion> ListaResumenBE = oBLLCom.ListarResumen();
            List<BEPlanRespuesta> respuesta = new List<BEPlanRespuesta>();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                BEPlanRespuesta item = new BEPlanRespuesta();

                item.PlanId = oPlanBE.PlanId;
                item.Nombre = oPlanBE.Nombre;
                item.Descripcion = oPlanBE.Descripcion;
                item.PrecioMensual = oPlanBE.PrecioMensual;
                item.CantidadLicencias = oPlanBE.CantidadLicencias;
                item.Destacado = oPlanBE.Destacado;
                item.Caracteristicas = MapearCaracteristicas(ListaMatrizBE, oPlanBE.PlanId);

                MapearValoracion(item, ListaResumenBE);

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("{planId:int}/caracteristicas")]
        [ProducesResponseType(typeof(List<BEPlanCaracteristicaRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEPlanCaracteristicaRespuesta>> ListarCaracteristicas(int planId)
        {
            ExigirPermiso(Permisos.PlanListar);

            BEPlanSuscripcion oFiltroBE = new BEPlanSuscripcion();
            oFiltroBE.PlanId = planId;

            return Ok(MapearCaracteristicas(oBLLPlaCar.ListarPorPlan(oFiltroBE), planId));
        }

        [HttpPut("{planId:int}/caracteristicas")]
        [ProducesResponseType(typeof(List<BEPlanCaracteristicaRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEPlanCaracteristicaRespuesta>> GuardarCaracteristicas(
            int planId, [FromBody] BEGuardarPlanCaracteristicas peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PlanModificar);

            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();
            oPlanBE.PlanId = planId;

            List<BEPlanCaracteristica> ListaPedidaBE = new List<BEPlanCaracteristica>();

            foreach (BEItemPlanCaracteristica item in peticion.Caracteristicas)
            {
                BEPlanCaracteristica oPedidaBE = new BEPlanCaracteristica();

                oPedidaBE.PlanId = planId;
                oPedidaBE.CaracteristicaId = item.CaracteristicaId;
                oPedidaBE.Incluido = item.Incluido;
                oPedidaBE.Detalle = item.Detalle;

                ListaPedidaBE.Add(oPedidaBE);
            }

            List<BEPlanCaracteristica> ListaGuardadaBE =
                oBLLPlaCar.Reemplazar(oPlanBE, ListaPedidaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPlan,
                "Caracteristicas",
                "El plan con id " + planId + " quedó con " + ListaGuardadaBE.Count +
                " características asociadas.",
                BLLBitacora.NivelInformativo));

            return Ok(MapearCaracteristicas(ListaGuardadaBE, planId));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEPlanSuscripcion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEPlanSuscripcion>> ListarParaAdministracion()
        {
            ExigirPermiso(Permisos.PlanListar);

            List<BEPlanSuscripcion> ListaPlanBE = oBLLPlan.ListarTodo();

            return Ok(ListaPlanBE);
        }

        [HttpGet("{planId:int}")]
        [ProducesResponseType(typeof(BEPlanSuscripcion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEPlanSuscripcion> Obtener(int planId)
        {
            ExigirPermiso(Permisos.PlanListar);

            BEPlanSuscripcion oFiltroBE = new BEPlanSuscripcion();
            oFiltroBE.PlanId = planId;

            return Ok(oBLLPlan.ListarObjeto(oFiltroBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEPlanSuscripcion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPlanSuscripcion> Crear([FromBody] BEGuardarPlan peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PlanAlta);

            BEPlanSuscripcion oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BEPlanSuscripcion oPlanBE = oBLLPlan.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPlan,
                "Alta",
                "Alta del plan " + oPlanBE.Nombre + " (id " + oPlanBE.PlanId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(nameof(Obtener), new { planId = oPlanBE.PlanId }, oPlanBE);
        }

        [HttpPut("{planId:int}")]
        [ProducesResponseType(typeof(BEPlanSuscripcion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPlanSuscripcion> Modificar(int planId, [FromBody] BEGuardarPlan peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PlanModificar);

            BEPlanSuscripcion oPlanBE = oBLLPlan.Guardar(ADominio(planId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPlan,
                "Modificacion",
                "Modificación del plan " + oPlanBE.Nombre + " (id " + planId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(oPlanBE);
        }

        [HttpDelete("{planId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int planId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PlanBaja);

            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();
            oPlanBE.PlanId = planId;

            oBLLPlan.Baja(oPlanBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPlan,
                "Baja",
                "Baja lógica del plan con id " + planId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BEPlanSuscripcion ADominio(int planId, BEGuardarPlan peticion)
        {
            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();

            oPlanBE.PlanId = planId;
            oPlanBE.Nombre = peticion.Nombre;
            oPlanBE.Descripcion = peticion.Descripcion;
            oPlanBE.PrecioMensual = peticion.PrecioMensual;
            oPlanBE.CantidadLicencias = peticion.CantidadLicencias;
            oPlanBE.Destacado = peticion.Destacado;

            return oPlanBE;
        }

        private void MapearValoracion(BEPlanRespuesta item, List<BEResumenValoracion> ListaResumenBE)
        {
            item.PromedioValoracion = 0;
            item.CantidadValoraciones = 0;

            foreach (BEResumenValoracion oResumenBE in ListaResumenBE)
            {
                if (oResumenBE.PlanId == item.PlanId)
                {
                    item.PromedioValoracion = oResumenBE.Promedio;
                    item.CantidadValoraciones = oResumenBE.Cantidad;

                    return;
                }
            }
        }

        private List<BEPlanCaracteristicaRespuesta> MapearCaracteristicas(
            List<BEPlanCaracteristica> ListaMatrizBE, int planId)
        {
            List<BEPlanCaracteristicaRespuesta> respuesta = new List<BEPlanCaracteristicaRespuesta>();

            foreach (BEPlanCaracteristica oPlanCaracteristicaBE in ListaMatrizBE)
            {
                if (oPlanCaracteristicaBE.PlanId != planId)
                {
                    continue;
                }

                BEPlanCaracteristicaRespuesta item = new BEPlanCaracteristicaRespuesta();

                item.CaracteristicaId = oPlanCaracteristicaBE.CaracteristicaId;
                item.Incluido = oPlanCaracteristicaBE.Incluido;
                item.Detalle = oPlanCaracteristicaBE.Detalle;

                respuesta.Add(item);
            }

            return respuesta;
        }
    }
}
