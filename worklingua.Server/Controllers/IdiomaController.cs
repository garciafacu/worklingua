using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/idiomas")]
    public class IdiomaController : ControladorBase
    {
        BLLIdioma oBLLIdi;
        BLLBitacora oBLLBit;

        public IdiomaController()
        {
            oBLLIdi = new BLLIdioma();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEIdiomaRespuesta>), StatusCodes.Status200OK)]
        public ActionResult<List<BEIdiomaRespuesta>> Listar()
        {
            List<BEIdioma> ListaIdiomaBE = oBLLIdi.ListarTodo();
            List<BEIdiomaRespuesta> respuesta = new List<BEIdiomaRespuesta>();

            foreach (BEIdioma oIdiomaBE in ListaIdiomaBE)
            {
                BEIdiomaRespuesta item = new BEIdiomaRespuesta();

                item.IdiomaId = oIdiomaBE.IdiomaId;
                item.Nombre = oIdiomaBE.Nombre;
                item.CodigoISO = oIdiomaBE.CodigoISO;

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEIdiomaAdminRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEIdiomaAdminRespuesta>> ListarParaAdministracion()
        {
            ExigirPermiso(Permisos.IdiomaListar);

            List<BEIdioma> ListaIdiomaBE = oBLLIdi.ListarTodoConBajas();
            List<BEIdiomaAdminRespuesta> respuesta = new List<BEIdiomaAdminRespuesta>();

            foreach (BEIdioma oIdiomaBE in ListaIdiomaBE)
            {
                respuesta.Add(Mapear(oIdiomaBE));
            }

            return Ok(respuesta);
        }

        [HttpGet("{idiomaId:int}")]
        [ProducesResponseType(typeof(BEIdiomaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEIdiomaAdminRespuesta> Obtener(int idiomaId)
        {
            ExigirPermiso(Permisos.IdiomaListar);

            BEIdioma oFiltroBE = new BEIdioma();
            oFiltroBE.IdiomaId = idiomaId;

            return Ok(Mapear(oBLLIdi.ListarObjeto(oFiltroBE)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEIdiomaAdminRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEIdiomaAdminRespuesta> Crear([FromBody] BEGuardarIdioma peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.IdiomaAlta);

            BEIdioma oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BEIdioma oIdiomaBE = oBLLIdi.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloIdioma,
                "Alta",
                "Alta del idioma " + oIdiomaBE.Nombre + " (id " + oIdiomaBE.IdiomaId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener), new { idiomaId = oIdiomaBE.IdiomaId }, Mapear(oIdiomaBE));
        }

        [HttpPut("{idiomaId:int}")]
        [ProducesResponseType(typeof(BEIdiomaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEIdiomaAdminRespuesta> Modificar(
            int idiomaId, [FromBody] BEGuardarIdioma peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.IdiomaModificar);

            BEIdioma oIdiomaBE = oBLLIdi.Guardar(ADominio(idiomaId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloIdioma,
                "Modificacion",
                "Modificación del idioma " + oIdiomaBE.Nombre + " (id " + idiomaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oIdiomaBE));
        }

        [HttpPatch("{idiomaId:int}/estado")]
        [ProducesResponseType(typeof(BEIdiomaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEIdiomaAdminRespuesta> CambiarEstado(
            int idiomaId, [FromBody] BEIdioma oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.IdiomaModificar);

            oPeticionBE.IdiomaId = idiomaId;

            oBLLIdi.CambiarEstado(oPeticionBE);

            BEIdioma oIdiomaBE = oBLLIdi.ListarObjeto(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloIdioma,
                oPeticionBE.Activo == true ? "Activacion" : "Desactivacion",
                (oPeticionBE.Activo == true ? "Activación" : "Desactivación") + " del idioma de plataforma " +
                oIdiomaBE.Nombre + " (id " + idiomaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oIdiomaBE));
        }

        [HttpDelete("{idiomaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int idiomaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.IdiomaBaja);

            BEIdioma oIdiomaBE = new BEIdioma();
            oIdiomaBE.IdiomaId = idiomaId;

            oBLLIdi.Baja(oIdiomaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloIdioma,
                "Baja",
                "Baja lógica del idioma con id " + idiomaId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BEIdioma ADominio(int idiomaId, BEGuardarIdioma peticion)
        {
            BEIdioma oIdiomaBE = new BEIdioma();

            oIdiomaBE.IdiomaId = idiomaId;
            oIdiomaBE.Nombre = peticion.Nombre;
            oIdiomaBE.CodigoISO = peticion.CodigoISO;

            return oIdiomaBE;
        }

        private BEIdiomaAdminRespuesta Mapear(BEIdioma oIdiomaBE)
        {
            BEIdiomaAdminRespuesta item = new BEIdiomaAdminRespuesta();

            item.IdiomaId = oIdiomaBE.IdiomaId;
            item.Nombre = oIdiomaBE.Nombre;
            item.CodigoISO = oIdiomaBE.CodigoISO;
            item.Activo = oIdiomaBE.Activo == true;

            return item;
        }
    }
}
