using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/caracteristicas")]
    public class CaracteristicaController : ControladorBase
    {
        BLLCaracteristica oBLLCar;
        BLLBitacora oBLLBit;

        public CaracteristicaController()
        {
            oBLLCar = new BLLCaracteristica();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BECaracteristicaRespuesta>), StatusCodes.Status200OK)]
        public ActionResult<List<BECaracteristicaRespuesta>> Listar()
        {
            List<BECaracteristica> ListaCaracteristicaBE = oBLLCar.ListarTodo();
            List<BECaracteristicaRespuesta> respuesta = new List<BECaracteristicaRespuesta>();

            foreach (BECaracteristica oCaracteristicaBE in ListaCaracteristicaBE)
            {
                BECaracteristicaRespuesta item = new BECaracteristicaRespuesta();

                item.CaracteristicaId = oCaracteristicaBE.CaracteristicaId;
                item.Nombre = oCaracteristicaBE.Nombre;
                item.Orden = oCaracteristicaBE.Orden;

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BECaracteristica>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECaracteristica>> ListarParaAdministracion()
        {
            ExigirPermiso(Permisos.CaracteristicaListar);

            List<BECaracteristica> ListaCaracteristicaBE = oBLLCar.ListarTodo();

            return Ok(ListaCaracteristicaBE);
        }

        [HttpGet("{caracteristicaId:int}")]
        [ProducesResponseType(typeof(BECaracteristica), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BECaracteristica> Obtener(int caracteristicaId)
        {
            ExigirPermiso(Permisos.CaracteristicaListar);

            BECaracteristica oFiltroBE = new BECaracteristica();
            oFiltroBE.CaracteristicaId = caracteristicaId;

            return Ok(oBLLCar.ListarObjeto(oFiltroBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BECaracteristica), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECaracteristica> Crear(
            [FromBody] BEGuardarCaracteristica peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CaracteristicaAlta);

            BECaracteristica oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BECaracteristica oCaracteristicaBE = oBLLCar.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCaracteristica,
                "Alta",
                "Alta de la característica " + oCaracteristicaBE.Nombre +
                " (id " + oCaracteristicaBE.CaracteristicaId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener),
                new { caracteristicaId = oCaracteristicaBE.CaracteristicaId },
                oCaracteristicaBE);
        }

        [HttpPut("{caracteristicaId:int}")]
        [ProducesResponseType(typeof(BECaracteristica), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECaracteristica> Modificar(
            int caracteristicaId, [FromBody] BEGuardarCaracteristica peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CaracteristicaModificar);

            BECaracteristica oCaracteristicaBE = oBLLCar.Guardar(ADominio(caracteristicaId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCaracteristica,
                "Modificacion",
                "Modificación de la característica " + oCaracteristicaBE.Nombre +
                " (id " + caracteristicaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(oCaracteristicaBE);
        }

        [HttpDelete("{caracteristicaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int caracteristicaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CaracteristicaBaja);

            BECaracteristica oCaracteristicaBE = new BECaracteristica();
            oCaracteristicaBE.CaracteristicaId = caracteristicaId;

            oBLLCar.Baja(oCaracteristicaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCaracteristica,
                "Baja",
                "Baja lógica de la característica con id " + caracteristicaId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BECaracteristica ADominio(int caracteristicaId, BEGuardarCaracteristica peticion)
        {
            BECaracteristica oCaracteristicaBE = new BECaracteristica();

            oCaracteristicaBE.CaracteristicaId = caracteristicaId;
            oCaracteristicaBE.Nombre = peticion.Nombre;
            oCaracteristicaBE.Orden = peticion.Orden;

            return oCaracteristicaBE;
        }
    }
}
