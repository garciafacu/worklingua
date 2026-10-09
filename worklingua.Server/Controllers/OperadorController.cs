using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;
using worklingua.Server.Services;

namespace worklingua.Server.Controllers
{
    [Route("api/operadores")]
    public class OperadorController : ControladorBase
    {
        BLLUsuario oBLLUsu;
        ServicioHash oServicioHash;

        public OperadorController()
        {
            oBLLUsu = new BLLUsuario();
            oServicioHash = new ServicioHash();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEUsuarioAdminRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEUsuarioAdminRespuesta>> Listar()
        {
            ExigirPermiso(Permisos.OperadorListar);

            List<BEUsuarioAdministracion> ListaOperadorBE = oBLLUsu.ListarOperadores();
            List<BEUsuarioAdminRespuesta> respuesta = new List<BEUsuarioAdminRespuesta>();

            foreach (BEUsuarioAdministracion oOperadorBE in ListaOperadorBE)
            {
                respuesta.Add(Mapear(oOperadorBE));
            }

            return Ok(respuesta);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEUsuarioAdminRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEUsuarioAdminRespuesta> Crear([FromBody] BEGuardarOperador peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorAlta);

            BEUsuario oPeticionBE = ADominio(0, peticion);
            oPeticionBE.PasswordHash = oServicioHash.Hashear(Guid.NewGuid().ToString());

            BEUsuario oOperadorBE = oBLLUsu.InvitarOperador(oPeticionBE, oSesionBE);

            return StatusCode(StatusCodes.Status201Created, Mapear(oOperadorBE));
        }

        [HttpPut("{usuarioId:int}")]
        [ProducesResponseType(typeof(BEUsuarioAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEUsuarioAdminRespuesta> Modificar(int usuarioId, [FromBody] BEGuardarOperador peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorModificar);

            BEUsuario oOperadorBE = oBLLUsu.ModificarOperador(ADominio(usuarioId, peticion), oSesionBE);

            return Ok(Mapear(oOperadorBE));
        }

        [HttpDelete("{usuarioId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int usuarioId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorBaja);

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.UsuarioId = usuarioId;

            oBLLUsu.BajaOperador(oUsuarioBE, oSesionBE);

            return NoContent();
        }

        [HttpPost("{usuarioId:int}/reinvitar")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Reinvitar(int usuarioId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorAlta);

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.UsuarioId = usuarioId;

            oBLLUsu.ReinvitarOperador(oUsuarioBE, oSesionBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Se reenvió la invitación.";

            return Ok(respuesta);
        }

        /// <summary>
        /// Desbloqueo manual de un operador (CU-003-004, camino alternativo 4).
        /// Va acá porque los operadores no aparecen en el ABM de Usuarios: sin
        /// esta acción no habría forma de desbloquear a uno.
        /// </summary>
        [HttpPost("{usuarioId:int}/desbloquear")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEMensajeRespuesta> Desbloquear(int usuarioId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorModificar);

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.UsuarioId = usuarioId;

            BEUsuario oLiberadoBE = oBLLUsu.DesbloquearOperador(oUsuarioBE, oSesionBE);

            return Ok(new BEMensajeRespuesta(
                "Acceso restaurado con éxito para " + oLiberadoBE.Email + "."));
        }

        [HttpGet("{usuarioId:int}/roles")]
        [ProducesResponseType(typeof(List<BERolRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BERolRespuesta>> ObtenerRoles(int usuarioId)
        {
            ExigirPermiso(Permisos.OperadorListar);

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = usuarioId;

            List<BERol> ListaRolBE = oBLLUsu.ObtenerRoles(oFiltroBE);
            List<BERolRespuesta> respuesta = new List<BERolRespuesta>();

            foreach (BERol oRolBE in ListaRolBE)
            {
                respuesta.Add(new BERolRespuesta(oRolBE.RolId, oRolBE.Nombre, oRolBE.Descripcion));
            }

            return Ok(respuesta);
        }

        [HttpPost("{usuarioId:int}/roles/{rolId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult AsignarRol(int usuarioId, int rolId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorModificar);

            oBLLUsu.AsignarRol(new BEUsuarioRol(0, usuarioId, rolId, null), oSesionBE);

            return NoContent();
        }

        [HttpDelete("{usuarioId:int}/roles/{rolId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult QuitarRol(int usuarioId, int rolId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OperadorModificar);

            oBLLUsu.QuitarRol(new BEUsuarioRol(0, usuarioId, rolId, null), oSesionBE);

            return NoContent();
        }

        private BEUsuario ADominio(int usuarioId, BEGuardarOperador peticion)
        {
            BEUsuario oUsuarioBE = new BEUsuario();

            oUsuarioBE.UsuarioId = usuarioId;
            oUsuarioBE.Nombre = peticion.Nombre.Trim();
            oUsuarioBE.Apellido = peticion.Apellido.Trim();
            oUsuarioBE.Documento = Opcional(peticion.Documento);
            oUsuarioBE.Email = peticion.Email.Trim().ToLowerInvariant();

            return oUsuarioBE;
        }

        private string Opcional(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private BEUsuarioAdminRespuesta Mapear(BEUsuarioAdministracion oOperadorBE)
        {
            BEUsuarioAdminRespuesta item = Mapear(oOperadorBE.Usuario);

            item.Empresa = oOperadorBE.Empresa;
            item.RolId = oOperadorBE.RolId;
            item.Roles = oOperadorBE.Roles;

            return item;
        }

        private BEUsuarioAdminRespuesta Mapear(BEUsuario oUsuarioBE)
        {
            BEUsuarioAdminRespuesta item = new BEUsuarioAdminRespuesta();

            item.UsuarioId = oUsuarioBE.UsuarioId;
            item.Nombre = oUsuarioBE.Nombre;
            item.Apellido = oUsuarioBE.Apellido;
            item.Documento = oUsuarioBE.Documento;
            item.Email = oUsuarioBE.Email;
            item.EmpresaId = oUsuarioBE.EmpresaId;
            item.Roles = new List<string>();
            item.FechaAlta = oUsuarioBE.FechaAlta;
            item.UltimoAcceso = oUsuarioBE.UltimoAcceso;
            item.Activo = oUsuarioBE.Activo == true;
            item.BloqueadoHasta = oUsuarioBE.BloqueadoHasta;

            return item;
        }
    }
}
