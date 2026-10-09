using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/comentarios")]
    public class ComentarioController : ControladorBase
    {
        BLLComentario oBLLCom;
        BLLSesion oBLLSes;
        BLLSeguridad oBLLSeg;

        public ComentarioController()
        {
            oBLLCom = new BLLComentario();
            oBLLSes = new BLLSesion();
            oBLLSeg = new BLLSeguridad();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEComentarioRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEComentarioRespuesta>> Listar([FromQuery] int planId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.ComentarioParticipar);
            List<BEComentarioConAutor> ListaComentarioBE = oBLLCom.ListarTodo(planId);

            return Ok(Mapear(ListaComentarioBE, oSesionBE.UsuarioId));
        }

        [HttpGet("buscar")]
        [ProducesResponseType(typeof(List<BEComentarioRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEComentarioRespuesta>> Buscar(
            [FromQuery] BEFiltroComentario oFiltroBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.ComentarioParticipar);

            List<BEComentarioConAutor> ListaComentarioBE = oBLLCom.Buscar(oFiltroBE);

            return Ok(Mapear(ListaComentarioBE, oSesionBE.UsuarioId));
        }

        [HttpGet("valoracion")]
        [ProducesResponseType(typeof(BEComentarioRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEComentarioRespuesta> ObtenerValoracion([FromQuery] int planId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.ComentarioParticipar);

            BEComentario oFiltroBE = new BEComentario();
            oFiltroBE.UsuarioId = oSesionBE.UsuarioId;
            oFiltroBE.PlanId = planId;

            BEComentarioConAutor oValoracion = oBLLCom.ListarValoracion(oFiltroBE);

            if (oValoracion == null)
            {
                return NoContent();
            }

            return Ok(Mapear(oValoracion, oSesionBE.UsuarioId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEComentarioRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEComentarioRespuesta> Crear([FromBody] BECrearComentario peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.ComentarioParticipar);

            BEComentario oComentarioBE = new BEComentario();

            oComentarioBE.UsuarioId = oSesionBE.UsuarioId;
            oComentarioBE.PlanId = peticion.PlanId;
            oComentarioBE.Puntaje = peticion.Puntaje;
            oComentarioBE.Texto = peticion.Texto;

            BEComentarioConAutor guardado = oBLLCom.Guardar(oComentarioBE);

            return Ok(Mapear(guardado, oSesionBE.UsuarioId));
        }

        [HttpDelete("{comentarioId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Eliminar(int comentarioId)
        {
            BESesion oSesionBE = ObtenerSesionActiva();

            BEComentario oComentarioBE = new BEComentario();
            oComentarioBE.ComentarioId = comentarioId;

            oBLLCom.Baja(oComentarioBE, oSesionBE);

            return NoContent();
        }

        private BESesion ObtenerSesionActiva()
        {
            return oBLLSes.ObtenerActiva(ObtenerTokenSesion());
        }

        private List<BEComentarioRespuesta> Mapear(
            List<BEComentarioConAutor> comentarios, int usuarioActual)
        {
            bool puedeModerar = oBLLSeg.Tiene(usuarioActual, Permisos.ComentarioModerar);
            List<BEComentarioRespuesta> respuesta = new List<BEComentarioRespuesta>();

            foreach (BEComentarioConAutor oComentario in comentarios)
            {
                respuesta.Add(Mapear(oComentario, usuarioActual, puedeModerar));
            }

            return respuesta;
        }

        private BEComentarioRespuesta Mapear(BEComentarioConAutor oComentario, int usuarioActual)
        {
            return Mapear(
                oComentario,
                usuarioActual,
                oBLLSeg.Tiene(usuarioActual, Permisos.ComentarioModerar));
        }

        private BEComentarioRespuesta Mapear(
            BEComentarioConAutor oComentario, int usuarioActual, bool puedeModerar)
        {
            BEComentarioRespuesta item = new BEComentarioRespuesta();

            item.ComentarioId = oComentario.Comentario.ComentarioId;
            item.PlanId = oComentario.Comentario.PlanId;
            item.Plan = oComentario.Plan;
            item.Autor = oComentario.Autor;
            item.Texto = oComentario.Comentario.Texto;
            item.Puntaje = oComentario.Comentario.Puntaje;
            item.FechaAlta = oComentario.Comentario.FechaAlta;
            item.EsPropio = oComentario.Comentario.UsuarioId == usuarioActual;

            item.PuedeEliminar = item.EsPropio || puedeModerar;

            return item;
        }
    }
}
