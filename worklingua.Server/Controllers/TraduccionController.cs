using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/traducciones")]
    public class TraduccionController : ControladorBase
    {
        BLLTraduccion oBLLTra;
        BLLIdioma oBLLIdi;
        BLLBitacora oBLLBit;

        public TraduccionController()
        {
            oBLLTra = new BLLTraduccion();
            oBLLIdi = new BLLIdioma();
            oBLLBit = new BLLBitacora();
        }

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
            BESesion oSesionBE = ExigirPermiso(Permisos.TraduccionModificar);

            BEIdioma oFiltroBE = new BEIdioma();
            oFiltroBE.IdiomaId = peticion.IdiomaId;

            BEIdioma oIdiomaBE = oBLLIdi.ListarObjeto(oFiltroBE);

            int guardadas = oBLLTra.GuardarLote(oFiltroBE, ADominio(peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloTraduccion,
                "Modificacion",
                "Se guardaron " + guardadas + " traducciones del idioma " +
                oIdiomaBE.Nombre + " (id " + peticion.IdiomaId + ").",
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
