using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/alertas")]
    public class AlertaController : ControladorBase
    {
        BLLAlerta oBLLAle;
        BLLBitacora oBLLBit;

        public AlertaController()
        {
            oBLLAle = new BLLAlerta();
            oBLLBit = new BLLBitacora();
        }

        /// <summary>
        /// El panel (pasos 3 a 5). Sin Alerta.VerTodasLasEmpresas la BLL acota
        /// la lista a la empresa del usuario de la sesión.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<BEAlertaRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEAlertaRespuesta>> Listar()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.AlertaListar);

            List<BEAlertaRespuesta> respuesta = new List<BEAlertaRespuesta>();

            foreach (BEAlerta oAlertaBE in oBLLAle.Listar(oSesionBE))
            {
                respuesta.Add(Mapear(oAlertaBE));
            }

            return Ok(respuesta);
        }

        /// <summary>
        /// A quiénes alcanzaría la condición, sin emitir nada. Pide
        /// Alerta.Alta porque es el paso previo a crear una.
        /// </summary>
        [HttpGet("alcance")]
        [ProducesResponseType(typeof(List<BEEmpleadoInactivo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEmpleadoInactivo>> Previsualizar(
            [FromQuery] int empresaId = 0,
            [FromQuery] int departamentoId = 0,
            [FromQuery] int diasInactividad = 0)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.AlertaAlta);

            BEAlerta oFiltroBE = new BEAlerta();
            oFiltroBE.EmpresaId = empresaId;
            oFiltroBE.DepartamentoId = departamentoId == 0 ? null : (int?)departamentoId;
            oFiltroBE.DiasInactividad = diasInactividad;

            return Ok(oBLLAle.Previsualizar(oFiltroBE, oSesionBE));
        }

        /// <summary>El escenario principal: guardar y emitir (pasos 6 a 14).</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BEEmisionRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEEmisionRespuesta> Crear([FromBody] BEGuardarAlerta peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.AlertaAlta);

            BEEmisionRespuesta respuesta = oBLLAle.GuardarYEmitir(ADominio(peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAlerta,
                "Alta",
                "Alta de la alerta " + respuesta.Alerta.Titulo + " (id " + respuesta.Alerta.AlertaId +
                "), emitida a " + respuesta.Destinatarios + " empleados.",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, respuesta);
        }

        /// <summary>Vuelve a emitir una alerta activa.</summary>
        [HttpPost("{alertaId:int}/emitir")]
        [ProducesResponseType(typeof(BEEmisionRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEEmisionRespuesta> Emitir(int alertaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.AlertaEmitir);

            BEAlerta oFiltroBE = new BEAlerta();
            oFiltroBE.AlertaId = alertaId;

            BEEmisionRespuesta respuesta = oBLLAle.Reemitir(oFiltroBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAlerta,
                "Emision",
                "Emisión de la alerta " + respuesta.Alerta.Titulo + " (id " + alertaId + ") a " +
                respuesta.Destinatarios + " empleados.",
                BLLBitacora.NivelInformativo));

            return Ok(respuesta);
        }

        /// <summary>Camino alternativo 3: activar o desactivar la regla.</summary>
        [HttpPatch("{alertaId:int}/estado")]
        [ProducesResponseType(typeof(BEAlertaRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEAlertaRespuesta> CambiarEstado(
            int alertaId, [FromBody] BEAlerta oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.AlertaBaja);

            oPeticionBE.AlertaId = alertaId;

            BEAlerta oAlertaBE = oBLLAle.CambiarEstado(oPeticionBE, oSesionBE);

            bool activada = oPeticionBE.Activo == true;

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAlerta,
                activada ? "Activacion" : "Desactivacion",
                (activada ? "Activación" : "Desactivación") + " de la alerta " +
                oAlertaBE.Titulo + " (id " + alertaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oAlertaBE));
        }

        private BEAlerta ADominio(BEGuardarAlerta peticion)
        {
            BEAlerta oAlertaBE = new BEAlerta();

            oAlertaBE.EmpresaId = peticion.EmpresaId;
            oAlertaBE.DepartamentoId =
                peticion.DepartamentoId.HasValue && peticion.DepartamentoId.Value != 0
                    ? peticion.DepartamentoId
                    : null;
            oAlertaBE.Titulo = peticion.Titulo;
            oAlertaBE.Mensaje = peticion.Mensaje;
            oAlertaBE.DiasInactividad = peticion.DiasInactividad;

            return oAlertaBE;
        }

        private BEAlertaRespuesta Mapear(BEAlerta oAlertaBE)
        {
            BEAlertaRespuesta respuesta = new BEAlertaRespuesta();

            respuesta.AlertaId = oAlertaBE.AlertaId;
            respuesta.EmpresaId = oAlertaBE.EmpresaId;
            respuesta.Empresa = oAlertaBE.Empresa;
            respuesta.DepartamentoId = oAlertaBE.DepartamentoId;
            respuesta.Departamento = oAlertaBE.Departamento;
            respuesta.Titulo = oAlertaBE.Titulo;
            respuesta.Mensaje = oAlertaBE.Mensaje;
            respuesta.DiasInactividad = oAlertaBE.DiasInactividad;
            respuesta.Activo = oAlertaBE.Activo.HasValue && oAlertaBE.Activo.Value;
            respuesta.FechaAlta = oAlertaBE.FechaAlta;
            respuesta.UltimaEmision = oAlertaBE.UltimaEmision;
            respuesta.Destinatarios = oAlertaBE.Destinatarios;

            return respuesta;
        }
    }
}
