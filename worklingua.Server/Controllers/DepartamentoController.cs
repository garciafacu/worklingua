using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/departamentos")]
    public class DepartamentoController : ControladorBase
    {
        BLLDepartamento oBLLDep;
        BLLBitacora oBLLBit;

        public DepartamentoController()
        {
            oBLLDep = new BLLDepartamento();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEDepartamentoAdminRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEDepartamentoAdminRespuesta>> Listar()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoListar);

            List<BEDepartamento> ListaDepartamentoBE = oBLLDep.ListarAdministracion(oSesionBE);
            List<BEDepartamentoAdminRespuesta> respuesta = new List<BEDepartamentoAdminRespuesta>();

            foreach (BEDepartamento oDepartamentoBE in ListaDepartamentoBE)
            {
                respuesta.Add(Mapear(oDepartamentoBE));
            }

            return Ok(respuesta);
        }

        /// <summary>
        /// Los departamentos asignables a un usuario de esa empresa, para el
        /// selector del ABM de Usuarios. Pide Usuario.Modificar porque es ahí
        /// donde se usa; sin alcance total la BLL ignora empresaId y devuelve
        /// los de la empresa propia.
        /// </summary>
        [HttpGet("asignables")]
        [ProducesResponseType(typeof(List<BEDepartamentoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEDepartamentoRespuesta>> ListarAsignables([FromQuery] int empresaId = 0)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioModificar);

            BEDepartamento oFiltroBE = new BEDepartamento();
            oFiltroBE.EmpresaId = empresaId;

            List<BEDepartamentoRespuesta> respuesta = new List<BEDepartamentoRespuesta>();

            foreach (BEDepartamento oDepartamentoBE in oBLLDep.ListarAsignables(oFiltroBE, oSesionBE))
            {
                BEDepartamentoRespuesta item = new BEDepartamentoRespuesta();

                item.DepartamentoId = oDepartamentoBE.DepartamentoId;
                item.EmpresaId = oDepartamentoBE.EmpresaId;
                item.Empresa = oDepartamentoBE.Empresa;
                item.Nombre = oDepartamentoBE.Nombre;
                item.Descripcion = oDepartamentoBE.Descripcion;

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("{departamentoId:int}")]
        [ProducesResponseType(typeof(BEDepartamentoAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEDepartamentoAdminRespuesta> Obtener(int departamentoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoListar);

            BEDepartamento oFiltroBE = new BEDepartamento();
            oFiltroBE.DepartamentoId = departamentoId;

            return Ok(Mapear(oBLLDep.ListarObjeto(oFiltroBE, oSesionBE)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEDepartamentoAdminRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEDepartamentoAdminRespuesta> Crear([FromBody] BEGuardarDepartamento peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoAlta);

            BEDepartamento oDepartamentoBE = oBLLDep.Guardar(ADominio(0, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloDepartamento,
                "Alta",
                "Alta del departamento " + oDepartamentoBE.Nombre + " en " + oDepartamentoBE.Empresa +
                " (id " + oDepartamentoBE.DepartamentoId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener),
                new { departamentoId = oDepartamentoBE.DepartamentoId },
                Mapear(oDepartamentoBE));
        }

        [HttpPut("{departamentoId:int}")]
        [ProducesResponseType(typeof(BEDepartamentoAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEDepartamentoAdminRespuesta> Modificar(
            int departamentoId, [FromBody] BEGuardarDepartamento peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoModificar);

            BEDepartamento oDepartamentoBE = oBLLDep.Guardar(ADominio(departamentoId, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloDepartamento,
                "Modificacion",
                "Modificación del departamento " + oDepartamentoBE.Nombre + " (id " + departamentoId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oDepartamentoBE));
        }

        [HttpPatch("{departamentoId:int}/estado")]
        [ProducesResponseType(typeof(BEDepartamentoAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEDepartamentoAdminRespuesta> CambiarEstado(
            int departamentoId, [FromBody] BEDepartamento oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoModificar);

            oPeticionBE.DepartamentoId = departamentoId;

            oBLLDep.CambiarEstado(oPeticionBE, oSesionBE);

            BEDepartamento oDepartamentoBE = oBLLDep.ListarObjeto(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloDepartamento,
                oPeticionBE.Activo == true ? "Activacion" : "Desactivacion",
                (oPeticionBE.Activo == true ? "Activación" : "Desactivación") + " del departamento " +
                oDepartamentoBE.Nombre + " (id " + departamentoId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oDepartamentoBE));
        }

        [HttpDelete("{departamentoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int departamentoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoBaja);

            BEDepartamento oDepartamentoBE = new BEDepartamento();
            oDepartamentoBE.DepartamentoId = departamentoId;

            oBLLDep.Baja(oDepartamentoBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloDepartamento,
                "Baja",
                "Baja lógica del departamento con id " + departamentoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BEDepartamento ADominio(int departamentoId, BEGuardarDepartamento peticion)
        {
            BEDepartamento oDepartamentoBE = new BEDepartamento();

            oDepartamentoBE.DepartamentoId = departamentoId;
            oDepartamentoBE.EmpresaId = peticion.EmpresaId;
            oDepartamentoBE.Nombre = peticion.Nombre;
            oDepartamentoBE.Descripcion = peticion.Descripcion;

            return oDepartamentoBE;
        }

        private BEDepartamentoAdminRespuesta Mapear(BEDepartamento oDepartamentoBE)
        {
            BEDepartamentoAdminRespuesta item = new BEDepartamentoAdminRespuesta();

            item.DepartamentoId = oDepartamentoBE.DepartamentoId;
            item.EmpresaId = oDepartamentoBE.EmpresaId;
            item.Empresa = oDepartamentoBE.Empresa;
            item.Nombre = oDepartamentoBE.Nombre;
            item.Descripcion = oDepartamentoBE.Descripcion;
            item.Activo = oDepartamentoBE.Activo == true;
            item.Empleados = oDepartamentoBE.Empleados;
            item.FechaAlta = oDepartamentoBE.FechaAlta;

            return item;
        }
    }
}
