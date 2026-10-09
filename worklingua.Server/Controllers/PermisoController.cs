using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/permisos")]
    public class PermisoController : ControladorBase
    {
        BLLPermiso oBLLPer;
        BLLBitacora oBLLBit;

        public PermisoController()
        {
            oBLLPer = new BLLPermiso();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEPermisoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEPermisoRespuesta>> Listar()
        {
            ExigirPermiso(Permisos.PermisoListar);

            List<BEPermiso> ListaPermisoBE = oBLLPer.ListarJerarquia();
            List<BEPermisoRespuesta> respuesta = new List<BEPermisoRespuesta>();

            foreach (BEPermiso oPermisoBE in ListaPermisoBE)
            {
                respuesta.Add(MapearPermiso(oPermisoBE));
            }

            return Ok(respuesta);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEPermisoRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPermisoRespuesta> Crear([FromBody] BEGuardarPermiso peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PermisoAlta);

            BEPermiso oPermisoBE = oBLLPer.Guardar(ADominio(0, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPermiso,
                "Alta",
                "Alta de la familia " + oPermisoBE.Nombre + " (id " + oPermisoBE.PermisoId + ").",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, MapearPermiso(oPermisoBE));
        }

        [HttpPut("{permisoId:int}")]
        [ProducesResponseType(typeof(BEPermisoRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPermisoRespuesta> Modificar(int permisoId, [FromBody] BEGuardarPermiso peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PermisoModificar);

            BEPermiso oPermisoBE = oBLLPer.Guardar(ADominio(permisoId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPermiso,
                "Modificacion",
                "Modificación de la familia " + oPermisoBE.Nombre + " (id " + permisoId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(MapearPermiso(oPermisoBE));
        }

        [HttpDelete("{permisoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int permisoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PermisoBaja);

            BEPermisoCompuesto oPermisoBE = new BEPermisoCompuesto();
            oPermisoBE.PermisoId = permisoId;

            oBLLPer.Baja(oPermisoBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPermiso,
                "Baja",
                "Baja de la familia con id " + permisoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        [HttpPost("{permisoId:int}/hijos/{hijoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult AgregarHijo(int permisoId, int hijoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PermisoModificar);

            oBLLPer.AgregarHijo(new BEPermisoJerarquia(0, permisoId, hijoId));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPermiso,
                "AgregarHijo",
                "Se agregó el permiso con id " + hijoId + " a la familia con id " + permisoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        [HttpDelete("{permisoId:int}/hijos/{hijoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult QuitarHijo(int permisoId, int hijoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PermisoModificar);

            oBLLPer.QuitarHijo(new BEPermisoJerarquia(0, permisoId, hijoId));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloPermiso,
                "QuitarHijo",
                "Se quitó el permiso con id " + hijoId + " de la familia con id " + permisoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BEPermiso ADominio(int permisoId, BEGuardarPermiso peticion)
        {
            BEPermisoCompuesto oPermisoBE = new BEPermisoCompuesto();

            oPermisoBE.PermisoId = permisoId;
            oPermisoBE.Nombre = peticion.Nombre;
            oPermisoBE.Descripcion = peticion.Descripcion;

            return oPermisoBE;
        }
    }
}
