using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/idiomas")]
    public class IdiomaController : ControladorBase
    {
        BLLIdioma oBLLIdi = new BLLIdioma();
        BLLBitacora oBLLBit = new BLLBitacora();

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
            oBLLIdi.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.IdiomaAlta);

            BEIdioma oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BEIdioma oIdiomaBE = oBLLIdi.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloIdioma, "Alta",
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
            oBLLIdi.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.IdiomaModificar);

            BEIdioma oIdiomaBE = oBLLIdi.Guardar(ADominio(idiomaId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloIdioma, "Modificacion",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloIdioma,
                oPeticionBE.Activo == true ? "Activacion" : "Desactivacion",
                (oPeticionBE.Activo == true ? "Activación" : "Desactivación") + " del idioma de plataforma " + oIdiomaBE.Nombre + " (id " + idiomaId + ").",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloIdioma, "Baja",
                "Baja lógica del idioma con id " + idiomaId + ".", BLLBitacora.NivelInformativo));

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

    [Route("api/culturas")]
    public class CulturaController : ControladorBase
    {
        BLLCultura oBLLCul = new BLLCultura();
        BLLBitacora oBLLBit = new BLLBitacora();

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
            oBLLCul.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CulturaAlta);

            BECultura oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BECulturaConIdioma oCultura = oBLLCul.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCultura, "Alta",
                "Alta de la cultura " + oCultura.Cultura.Codigo + " (id " + oCultura.Cultura.CulturaId + ").",
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
            oBLLCul.Validar(peticion);

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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCultura,
                oPeticionBE.Activo ? "Activacion" : "Desactivacion",
                (oPeticionBE.Activo ? "Activación" : "Desactivación") + " de la cultura " + oCultura.Cultura.Codigo + " (id " + culturaId + ").",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCultura, "Baja",
                "Baja lógica de la cultura con id " + culturaId + ".", BLLBitacora.NivelInformativo));

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

    [Route("api/traducciones")]
    public class TraduccionController : ControladorBase
    {
        BLLTraduccion oBLLTra = new BLLTraduccion();
        BLLIdioma oBLLIdi = new BLLIdioma();
        BLLBitacora oBLLBit = new BLLBitacora();

        [HttpGet("{codigoISO}")]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Dictionary<string, string>> ObtenerBundle(string codigoISO)
        {
            return Ok(oBLLTra.ListarPorCodigo(codigoISO));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BETraduccionAdministracion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BETraduccionAdministracion>> ListarParaAdministracion(
            [FromQuery] BEFiltroTraduccion oFiltroBE)
        {
            ExigirPermiso(Permisos.TraduccionListar);

            List<BETraduccionAdministracion> ListaTraduccionBE = oBLLTra.ListarAdministracion(oFiltroBE);

            return Ok(ListaTraduccionBE);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEMensajeRespuesta> Guardar([FromBody] BEGuardarTraducciones peticion)
        {
            oBLLTra.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.TraduccionModificar);

            BEIdioma oFiltroBE = new BEIdioma();
            oFiltroBE.IdiomaId = peticion.IdiomaId;

            BEIdioma oIdiomaBE = oBLLIdi.ListarObjeto(oFiltroBE);

            int guardadas = oBLLTra.GuardarLote(oFiltroBE, ADominio(peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloTraduccion, "Modificacion",
                "Se guardaron " + guardadas + " traducciones del idioma " + oIdiomaBE.Nombre + " (id " + peticion.IdiomaId + ").",
                BLLBitacora.NivelInformativo));

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = guardadas == 1
                ? "Se guardó 1 traducción."
                : "Se guardaron " + guardadas + " traducciones.";

            return Ok(respuesta);
        }

        private List<BETraduccion> ADominio(BEGuardarTraducciones peticion)
        {
            List<BETraduccion> ListaTraduccionBE = new List<BETraduccion>();

            foreach (BETraduccionItem item in peticion.Traducciones)
            {
                BETraduccion oTraduccionBE = new BETraduccion();

                oTraduccionBE.IdiomaId = peticion.IdiomaId;
                oTraduccionBE.Clave = item.Clave;
                oTraduccionBE.Texto = item.Texto;
                oTraduccionBE.Activo = true;

                ListaTraduccionBE.Add(oTraduccionBE);
            }

            return ListaTraduccionBE;
        }
    }
}
