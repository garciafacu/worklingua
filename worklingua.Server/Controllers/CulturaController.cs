using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/culturas")]
    public class CulturaController : ControladorBase
    {
        BLLCultura oBLLCul;
        BLLBitacora oBLLBit;

        public CulturaController()
        {
            oBLLCul = new BLLCultura();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BECulturaRespuesta>), StatusCodes.Status200OK)]
        public ActionResult<List<BECulturaRespuesta>> Listar()
        {
            List<BECulturaConIdioma> ListaCulturaBE = oBLLCul.ListarTodo();
            List<BECulturaRespuesta> respuesta = new List<BECulturaRespuesta>();

            foreach (BECulturaConIdioma oCultura in ListaCulturaBE)
            {
                respuesta.Add(MapearPublico(oCultura));
            }

            return Ok(respuesta);
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BECulturaAdminRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECulturaAdminRespuesta>> ListarParaAdministracion()
        {
            ExigirPermiso(Permisos.CulturaListar);

            List<BECulturaConIdioma> ListaCulturaBE = oBLLCul.ListarTodoConBajas();
            List<BECulturaAdminRespuesta> respuesta = new List<BECulturaAdminRespuesta>();

            foreach (BECulturaConIdioma oCultura in ListaCulturaBE)
            {
                respuesta.Add(Mapear(oCultura));
            }

            return Ok(respuesta);
        }

        [HttpGet("{culturaId:int}")]
        [ProducesResponseType(typeof(BECulturaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BECulturaAdminRespuesta> Obtener(int culturaId)
        {
            ExigirPermiso(Permisos.CulturaListar);

            BECultura oFiltroBE = new BECultura();
            oFiltroBE.CulturaId = culturaId;

            return Ok(Mapear(oBLLCul.ListarObjeto(oFiltroBE)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BECulturaAdminRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECulturaAdminRespuesta> Crear([FromBody] BEGuardarCultura peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CulturaAlta);

            BECultura oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BECulturaConIdioma oCultura = oBLLCul.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCultura,
                "Alta",
                "Alta de la cultura " + oCultura.Cultura.Codigo +
                " (id " + oCultura.Cultura.CulturaId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener), new { culturaId = oCultura.Cultura.CulturaId }, Mapear(oCultura));
        }

        [HttpPut("{culturaId:int}")]
        [ProducesResponseType(typeof(BECulturaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECulturaAdminRespuesta> Modificar(
            int culturaId, [FromBody] BEGuardarCultura peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CulturaModificar);

            BECulturaConIdioma oCultura = oBLLCul.Guardar(ADominio(culturaId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCultura,
                "Modificacion",
                "Modificación de la cultura " + oCultura.Cultura.Codigo +
                " (id " + culturaId + "), tasa de conversión " +
                oCultura.Cultura.TasaConversion + ".",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oCultura));
        }

        [HttpPatch("{culturaId:int}/estado")]
        [ProducesResponseType(typeof(BECulturaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BECulturaAdminRespuesta> CambiarEstado(
            int culturaId, [FromBody] BECultura oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CulturaModificar);

            oPeticionBE.CulturaId = culturaId;

            oBLLCul.CambiarEstado(oPeticionBE);

            BECulturaConIdioma oCultura = oBLLCul.ListarObjeto(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCultura,
                oPeticionBE.Activo ? "Activacion" : "Desactivacion",
                (oPeticionBE.Activo ? "Activación" : "Desactivación") + " de la cultura " +
                oCultura.Cultura.Codigo + " (id " + culturaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oCultura));
        }

        [HttpDelete("{culturaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int culturaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CulturaBaja);

            BECultura oCulturaBE = new BECultura();
            oCulturaBE.CulturaId = culturaId;

            oBLLCul.Baja(oCulturaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCultura,
                "Baja",
                "Baja lógica de la cultura con id " + culturaId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BECultura ADominio(int culturaId, BEGuardarCultura peticion)
        {
            BECultura oCulturaBE = new BECultura();

            oCulturaBE.CulturaId = culturaId;
            oCulturaBE.Codigo = peticion.Codigo;
            oCulturaBE.Nombre = peticion.Nombre;
            oCulturaBE.IdiomaId = peticion.IdiomaId;
            oCulturaBE.Moneda = peticion.Moneda;
            oCulturaBE.SimboloMoneda = peticion.SimboloMoneda;
            oCulturaBE.FormatoFecha = peticion.FormatoFecha;
            oCulturaBE.SeparadorDecimal = peticion.SeparadorDecimal;
            oCulturaBE.SeparadorMiles = peticion.SeparadorMiles;
            oCulturaBE.TasaConversion = peticion.TasaConversion;
            oCulturaBE.EsPredeterminada = peticion.EsPredeterminada;

            return oCulturaBE;
        }

        private BECulturaRespuesta MapearPublico(BECulturaConIdioma oCultura)
        {
            BECulturaRespuesta item = new BECulturaRespuesta();

            Completar(item, oCultura);

            return item;
        }

        private BECulturaAdminRespuesta Mapear(BECulturaConIdioma oCultura)
        {
            BECulturaAdminRespuesta item = new BECulturaAdminRespuesta();

            Completar(item, oCultura);

            item.IdiomaId = oCultura.Cultura.IdiomaId;
            item.Idioma = oCultura.Idioma;
            item.Activo = oCultura.Cultura.Activo;

            return item;
        }

        private void Completar(BECulturaRespuesta item, BECulturaConIdioma oCultura)
        {
            item.CulturaId = oCultura.Cultura.CulturaId;
            item.Codigo = oCultura.Cultura.Codigo;
            item.Nombre = oCultura.Cultura.Nombre;
            item.CodigoIdioma = oCultura.CodigoIdioma;
            item.Moneda = oCultura.Cultura.Moneda;
            item.SimboloMoneda = oCultura.Cultura.SimboloMoneda;
            item.FormatoFecha = oCultura.Cultura.FormatoFecha;
            item.SeparadorDecimal = oCultura.Cultura.SeparadorDecimal;
            item.SeparadorMiles = oCultura.Cultura.SeparadorMiles;
            item.TasaConversion = oCultura.Cultura.TasaConversion;
            item.EsPredeterminada = oCultura.Cultura.EsPredeterminada;
        }
    }
}
