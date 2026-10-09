using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;
using worklingua.Server.Services;

namespace worklingua.Server.Controllers
{
    [Route("api/roles")]
    public class RolController : ControladorBase
    {
        BLLRol oBLLRol;
        BLLBitacora oBLLBit;

        public RolController()
        {
            oBLLRol = new BLLRol();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BERolRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BERolRespuesta>> Listar()
        {
            ExigirPermiso(Permisos.UsuarioListar);

            List<BERol> ListaRolBE = oBLLRol.ListarTodo();
            List<BERolRespuesta> respuesta = new List<BERolRespuesta>();

            foreach (BERol oRolBE in ListaRolBE)
            {
                if (EsRolDePlataforma(oRolBE))
                {
                    continue;
                }

                respuesta.Add(Mapear(oRolBE));
            }

            return Ok(respuesta);
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BERolRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BERolRespuesta>> ListarParaAdministracion()
        {
            ExigirPermiso(Permisos.RolListar);

            List<BERol> ListaRolBE = oBLLRol.ListarTodo();
            List<BERolRespuesta> respuesta = new List<BERolRespuesta>();

            foreach (BERol oRolBE in ListaRolBE)
            {
                respuesta.Add(Mapear(oRolBE));
            }

            return Ok(respuesta);
        }

        [HttpGet("{rolId:int}")]
        [ProducesResponseType(typeof(BERolRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BERolRespuesta> Obtener(int rolId)
        {
            ExigirPermiso(Permisos.RolListar);

            BERol oFiltroBE = new BERol();
            oFiltroBE.RolId = rolId;

            return Ok(Mapear(oBLLRol.ListarObjeto(oFiltroBE)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BERolRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BERolRespuesta> Crear([FromBody] BEGuardarRol peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.RolAlta);

            BERol oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BERol oRolBE = oBLLRol.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloRol,
                "Alta",
                "Alta del rol " + oRolBE.Nombre + " (id " + oRolBE.RolId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(nameof(Obtener), new { rolId = oRolBE.RolId }, Mapear(oRolBE));
        }

        [HttpPut("{rolId:int}")]
        [ProducesResponseType(typeof(BERolRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BERolRespuesta> Modificar(int rolId, [FromBody] BEGuardarRol peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.RolModificar);

            BERol oRolBE = oBLLRol.Guardar(ADominio(rolId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloRol,
                "Modificacion",
                "Modificación del rol " + oRolBE.Nombre + " (id " + rolId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oRolBE));
        }

        [HttpDelete("{rolId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int rolId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.RolBaja);

            BERol oRolBE = new BERol();
            oRolBE.RolId = rolId;

            oBLLRol.Baja(oRolBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloRol,
                "Baja",
                "Baja del rol con id " + rolId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        [HttpGet("{rolId:int}/permisos")]
        [ProducesResponseType(typeof(List<BEPermisoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEPermisoRespuesta>> ObtenerPermisos(int rolId)
        {
            ExigirPermiso(Permisos.RolListar);

            BERol oFiltroBE = new BERol();
            oFiltroBE.RolId = rolId;

            List<BEPermiso> ListaPermisoBE = oBLLRol.ObtenerPermisos(oFiltroBE);
            List<BEPermisoRespuesta> respuesta = new List<BEPermisoRespuesta>();

            foreach (BEPermiso oPermisoBE in ListaPermisoBE)
            {
                respuesta.Add(MapearPermiso(oPermisoBE));
            }

            return Ok(respuesta);
        }

        [HttpPost("{rolId:int}/permisos/{permisoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult AsignarPermiso(int rolId, int permisoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.RolModificar);

            oBLLRol.AsignarPermiso(new BERolPermiso(0, rolId, permisoId));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloRol,
                "AsignarPermiso",
                "Se asignó el permiso con id " + permisoId + " al rol con id " + rolId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        [HttpDelete("{rolId:int}/permisos/{permisoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult QuitarPermiso(int rolId, int permisoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.RolModificar);

            oBLLRol.QuitarPermiso(new BERolPermiso(0, rolId, permisoId));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloRol,
                "QuitarPermiso",
                "Se quitó el permiso con id " + permisoId + " del rol con id " + rolId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BERol ADominio(int rolId, BEGuardarRol peticion)
        {
            BERol oRolBE = new BERol();

            oRolBE.RolId = rolId;
            oRolBE.Nombre = peticion.Nombre;
            oRolBE.Descripcion = peticion.Descripcion;

            return oRolBE;
        }

        private BERolRespuesta Mapear(BERol oRolBE)
        {
            BERolRespuesta item = new BERolRespuesta();

            item.RolId = oRolBE.RolId;
            item.Nombre = oRolBE.Nombre;
            item.Descripcion = oRolBE.Descripcion;

            return item;
        }

        private bool EsRolDePlataforma(BERol oRolBE)
        {
            return string.Equals(
                oRolBE.Nombre.Trim(),
                Configuracion.SuperAdminRol.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
