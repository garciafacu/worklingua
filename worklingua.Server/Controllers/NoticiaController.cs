using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/noticias")]
    public class NoticiaController : ControladorBase
    {
        BLLNoticia oBLLNot;
        BLLBitacora oBLLBit;

        public NoticiaController()
        {
            oBLLNot = new BLLNoticia();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BENoticiaRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<List<BENoticiaRespuesta>> ListarPublicadas([FromQuery] BEFiltroNoticia oFiltroBE)
        {
            List<BENoticia> ListaNoticiaBE = oBLLNot.ListarPublicadas(oFiltroBE);
            List<BENoticiaRespuesta> respuesta = new List<BENoticiaRespuesta>();

            foreach (BENoticia oNoticiaBE in ListaNoticiaBE)
            {
                respuesta.Add(Mapear(oNoticiaBE));
            }

            return Ok(respuesta);
        }

        [HttpGet("{noticiaId:int}")]
        [ProducesResponseType(typeof(BENoticiaRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BENoticiaRespuesta> ObtenerPublicada(int noticiaId)
        {
            BENoticia oFiltroBE = new BENoticia();
            oFiltroBE.NoticiaId = noticiaId;

            return Ok(Mapear(oBLLNot.ListarObjetoPublicada(oFiltroBE)));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BENoticia>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BENoticia>> ListarParaAdministracion([FromQuery] BEFiltroNoticia oFiltroBE)
        {
            ExigirPermiso(Permisos.NoticiaListar);

            return Ok(oBLLNot.ListarAdministracion(oFiltroBE));
        }

        [HttpGet("{noticiaId:int}/administracion")]
        [ProducesResponseType(typeof(BENoticia), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BENoticia> Obtener(int noticiaId)
        {
            ExigirPermiso(Permisos.NoticiaListar);

            BENoticia oFiltroBE = new BENoticia();
            oFiltroBE.NoticiaId = noticiaId;

            return Ok(oBLLNot.ListarObjeto(oFiltroBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BENoticia), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BENoticia> Crear([FromBody] BEGuardarNoticia peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.NoticiaAlta);

            BENoticia oNoticiaBE = ADominio(0, peticion);
            oNoticiaBE.UsuarioId = oSesionBE.UsuarioId;

            oNoticiaBE = oBLLNot.Guardar(oNoticiaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloNoticia,
                "Alta",
                "Alta de la noticia " + oNoticiaBE.Titulo + " (id " + oNoticiaBE.NoticiaId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(nameof(Obtener), new { noticiaId = oNoticiaBE.NoticiaId }, oNoticiaBE);
        }

        [HttpPut("{noticiaId:int}")]
        [ProducesResponseType(typeof(BENoticia), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BENoticia> Modificar(int noticiaId, [FromBody] BEGuardarNoticia peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.NoticiaModificar);

            BENoticia oNoticiaBE = oBLLNot.Guardar(ADominio(noticiaId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloNoticia,
                "Modificacion",
                "Modificación de la noticia " + oNoticiaBE.Titulo + " (id " + noticiaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(oNoticiaBE);
        }

        [HttpDelete("{noticiaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int noticiaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.NoticiaBaja);

            BENoticia oNoticiaBE = new BENoticia();
            oNoticiaBE.NoticiaId = noticiaId;

            oBLLNot.Baja(oNoticiaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloNoticia,
                "Baja",
                "Baja lógica de la noticia con id " + noticiaId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BENoticia ADominio(int noticiaId, BEGuardarNoticia peticion)
        {
            BENoticia oNoticiaBE = new BENoticia();

            oNoticiaBE.NoticiaId = noticiaId;
            oNoticiaBE.IdiomaId = peticion.IdiomaId;
            oNoticiaBE.Titulo = peticion.Titulo;
            oNoticiaBE.Resumen = peticion.Resumen;
            oNoticiaBE.Contenido = peticion.Contenido;
            oNoticiaBE.FechaPublicacion = peticion.FechaPublicacion.HasValue
                ? peticion.FechaPublicacion.Value
                : default(DateTime);

            return oNoticiaBE;
        }

        private BENoticiaRespuesta Mapear(BENoticia oNoticiaBE)
        {
            BENoticiaRespuesta item = new BENoticiaRespuesta();

            item.NoticiaId = oNoticiaBE.NoticiaId;
            item.IdiomaId = oNoticiaBE.IdiomaId;
            item.Titulo = oNoticiaBE.Titulo;
            item.Resumen = oNoticiaBE.Resumen;
            item.Contenido = oNoticiaBE.Contenido;
            item.FechaPublicacion = oNoticiaBE.FechaPublicacion;

            return item;
        }
    }
}
