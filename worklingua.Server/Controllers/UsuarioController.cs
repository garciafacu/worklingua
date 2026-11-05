using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;
using worklingua.Server.Services;

namespace worklingua.Server.Controllers
{
    [Route("api/usuarios")]
    public class UsuarioController : ControladorBase
    {
        BLLUsuario oBLLUsu = new BLLUsuario();
        BLLSesion oBLLSes = new BLLSesion();
        ServicioHash oServicioHash = new ServicioHash();

        [HttpGet("perfil")]
        [ProducesResponseType(typeof(BEPerfilRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<BEPerfilRespuesta> ObtenerPerfil()
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());

            return Ok(oBLLUsu.ObtenerPerfil(oSesionBE));
        }

        [HttpPut("perfil")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<BEMensajeRespuesta> ModificarPerfil([FromBody] BEModificarPerfil oModificarPerfilBE)
        {
            oBLLUsu.Validar(oModificarPerfilBE);

            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.Nombre = oModificarPerfilBE.Nombre;
            oUsuarioBE.Apellido = oModificarPerfilBE.Apellido;
            oUsuarioBE.FechaNacimiento = oModificarPerfilBE.FechaNacimiento;

            oBLLUsu.ModificarPerfil(oUsuarioBE, oSesionBE);

            return Ok(new BEMensajeRespuesta("Preferencias actualizadas exitosamente."));
        }

        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Registrar([FromBody] BERegistroUsuario oRegistroBE)
        {
            oBLLUsu.Validar(oRegistroBE);

            oRegistroBE.Nombre = oRegistroBE.Nombre.Trim();
            oRegistroBE.Apellido = oRegistroBE.Apellido.Trim();
            oRegistroBE.Documento = ServicioTexto.Normalizar(oRegistroBE.Documento);
            oRegistroBE.Email = oRegistroBE.Email.Trim().ToLowerInvariant();
            oRegistroBE.RazonSocial = oRegistroBE.RazonSocial.Trim();
            oRegistroBE.CUIT = oRegistroBE.CUIT.Trim();

            oBLLUsu.ValidarClave(oRegistroBE.Clave);
            oRegistroBE.Clave = oServicioHash.Hashear(oRegistroBE.Clave);

            await oBLLUsu.Registrar(oRegistroBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Registro exitoso. Te enviamos un correo para confirmar tu cuenta.";

            return StatusCode(StatusCodes.Status201Created, respuesta);
        }

        [HttpPost("confirmar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Confirmar([FromBody] BETokenSeguridad oTokenBE)
        {
            oBLLUsu.ConfirmarCuenta(oTokenBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Tu cuenta fue confirmada. Ya podés ingresar.";

            return Ok(respuesta);
        }

        [HttpPost("completar-invitacion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CompletarInvitacion([FromBody] BECompletarInvitacion oCompletarInvitacionBE)
        {
            oBLLUsu.Validar(oCompletarInvitacionBE);

            oBLLUsu.ValidarClave(oCompletarInvitacionBE.Clave);

            oCompletarInvitacionBE.Clave = oServicioHash.Hashear(oCompletarInvitacionBE.Clave);

            oBLLUsu.CompletarInvitacion(oCompletarInvitacionBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Tu cuenta quedó activa. Ingresá con la clave que definiste.";

            return Ok(respuesta);
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEUsuarioAdminRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEUsuarioAdminRespuesta>> ListarParaAdministracion()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioListar);

            List<BEUsuarioAdministracion> ListaUsuarioBE = oBLLUsu.ListarAdministracion(oSesionBE);
            List<BEUsuarioAdminRespuesta> respuesta = new List<BEUsuarioAdminRespuesta>();

            foreach (BEUsuarioAdministracion oUsuario in ListaUsuarioBE)
            {
                respuesta.Add(Mapear(oUsuario));
            }

            return Ok(respuesta);
        }

        [HttpPost("invitar")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Invitar([FromBody] BEInvitarUsuario oInvitarUsuarioBE)
        {
            oBLLUsu.Validar(oInvitarUsuarioBE);

            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioInvitar);

            BEUsuarioAdministracion oUsuarioAdministracionBE = ADominio(0, oInvitarUsuarioBE);
            oUsuarioAdministracionBE.Usuario.PasswordHash = oServicioHash.Hashear(Guid.NewGuid().ToString());

            oBLLUsu.Invitar(oUsuarioAdministracionBE, oSesionBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Se envió la invitación a " + oUsuarioAdministracionBE.Usuario.Email + ".";

            return StatusCode(StatusCodes.Status201Created, respuesta);
        }

        [HttpPost("{usuarioId:int}/reinvitar")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Reinvitar(int usuarioId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioInvitar);

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.UsuarioId = usuarioId;

            oBLLUsu.Reinvitar(oUsuarioBE, oSesionBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Se reenvió la invitación.";

            return Ok(respuesta);
        }

        [HttpPost("{usuarioId:int}/desbloquear")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEMensajeRespuesta> Desbloquear(int usuarioId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioModificar);

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.UsuarioId = usuarioId;

            BEUsuario oLiberadoBE = oBLLUsu.Desbloquear(oUsuarioBE, oSesionBE);

            return Ok(new BEMensajeRespuesta(
                "Acceso restaurado con éxito para " + oLiberadoBE.Email + "."));
        }

        [HttpPut("{usuarioId:int}")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Modificar(int usuarioId, [FromBody] BEInvitarUsuario oInvitarUsuarioBE)
        {
            oBLLUsu.Validar(oInvitarUsuarioBE);

            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioModificar);

            BEUsuarioAdministracion oUsuarioAdministracionBE = ADominio(usuarioId, oInvitarUsuarioBE);

            oBLLUsu.ModificarDesdeAdministracion(oUsuarioAdministracionBE, oSesionBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Se actualizaron los datos de " + oUsuarioAdministracionBE.Usuario.Email + ".";

            return Ok(respuesta);
        }

        [HttpDelete("{usuarioId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int usuarioId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioBaja);

            BEUsuario oUsuarioBE = new BEUsuario();
            oUsuarioBE.UsuarioId = usuarioId;

            oBLLUsu.Baja(oUsuarioBE, oSesionBE);

            return NoContent();
        }

        private BEUsuarioAdministracion ADominio(int usuarioId, BEInvitarUsuario oInvitarUsuarioBE)
        {
            BEUsuario oUsuarioBE = new BEUsuario();

            oUsuarioBE.UsuarioId = usuarioId;
            oUsuarioBE.EmpresaId = oInvitarUsuarioBE.EmpresaId;
            oUsuarioBE.DepartamentoId = oInvitarUsuarioBE.DepartamentoId;
            oUsuarioBE.Nombre = oInvitarUsuarioBE.Nombre.Trim();
            oUsuarioBE.Apellido = oInvitarUsuarioBE.Apellido.Trim();
            oUsuarioBE.Documento = ServicioTexto.Normalizar(oInvitarUsuarioBE.Documento);
            oUsuarioBE.Email = oInvitarUsuarioBE.Email.Trim().ToLowerInvariant();

            BEUsuarioAdministracion oUsuarioAdministracionBE = new BEUsuarioAdministracion();

            oUsuarioAdministracionBE.Usuario = oUsuarioBE;
            oUsuarioAdministracionBE.RolId = oInvitarUsuarioBE.RolId;

            return oUsuarioAdministracionBE;
        }

        private BEUsuarioAdminRespuesta Mapear(BEUsuarioAdministracion oUsuario)
        {
            BEUsuarioAdminRespuesta item = new BEUsuarioAdminRespuesta();

            item.UsuarioId = oUsuario.Usuario.UsuarioId;
            item.Nombre = oUsuario.Usuario.Nombre;
            item.Apellido = oUsuario.Usuario.Apellido;
            item.Documento = oUsuario.Usuario.Documento;
            item.Email = oUsuario.Usuario.Email;
            item.EmpresaId = oUsuario.Usuario.EmpresaId;
            item.Empresa = oUsuario.Empresa;
            item.DepartamentoId = oUsuario.Usuario.DepartamentoId;
            item.Departamento = oUsuario.Departamento;
            item.RolId = oUsuario.RolId;
            item.Roles = oUsuario.Roles;
            item.FechaAlta = oUsuario.Usuario.FechaAlta;
            item.UltimoAcceso = oUsuario.Usuario.UltimoAcceso;
            item.Activo = oUsuario.Usuario.Activo == true;
            item.BloqueadoHasta = oUsuario.Usuario.BloqueadoHasta;

            return item;
        }
    }

    [Route("api/operadores")]
    public class OperadorController : ControladorBase
    {
        BLLUsuario oBLLUsu = new BLLUsuario();
        ServicioHash oServicioHash = new ServicioHash();

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
            oBLLUsu.Validar(peticion);

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
            oBLLUsu.Validar(peticion);

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
            oUsuarioBE.Documento = ServicioTexto.Normalizar(peticion.Documento);
            oUsuarioBE.Email = peticion.Email.Trim().ToLowerInvariant();

            return oUsuarioBE;
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
