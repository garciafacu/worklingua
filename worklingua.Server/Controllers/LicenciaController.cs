using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/licencias")]
    public class LicenciaController : ControladorBase
    {
        BLLLicencia oBLLLic;
        BLLBitacora oBLLBit;

        public LicenciaController()
        {
            oBLLLic = new BLLLicencia();
            oBLLBit = new BLLBitacora();
        }

        /// <summary>
        /// El inventario de licencias. Sin Licencia.VerTodasLasEmpresas la BLL
        /// ignora empresaId y devuelve el de la empresa propia.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BEInventarioRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEInventarioRespuesta> ObtenerInventario([FromQuery] int empresaId = 0)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.LicenciaListar);

            return Ok(oBLLLic.ObtenerInventario(empresaId, oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEMensajeRespuesta> Asignar([FromBody] BEAsignarLicencia peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.LicenciaAsignar);

            BELicencia oLicenciaBE = oBLLLic.Asignar(peticion, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloLicencia,
                "Asignacion",
                "Licencia " + oLicenciaBE.LicenciaId + " asignada al usuario " +
                oLicenciaBE.UsuarioId + " sobre la contratación " + oLicenciaBE.SuscripcionId + ".",
                BLLBitacora.NivelInformativo));

            return Ok(new BEMensajeRespuesta("Licencia asignada exitosamente."));
        }

        [HttpDelete("{licenciaId:int}")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEMensajeRespuesta> Revocar(int licenciaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.LicenciaRevocar);

            BELicencia oFiltroBE = new BELicencia();
            oFiltroBE.LicenciaId = licenciaId;

            BEUsuario oUsuarioBE = oBLLLic.Revocar(oFiltroBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloLicencia,
                "Revocacion",
                "Licencia " + licenciaId + " revocada a " + oUsuarioBE.Email + ". Cupo liberado.",
                BLLBitacora.NivelInformativo));

            return Ok(new BEMensajeRespuesta("Licencia revocada. Cupo liberado exitosamente."));
        }
    }
}
