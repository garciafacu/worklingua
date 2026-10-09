using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/empresas")]
    public class EmpresaController : ControladorBase
    {
        BLLEmpresa oBLLEmp;
        BLLBitacora oBLLBit;

        public EmpresaController()
        {
            oBLLEmp = new BLLEmpresa();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEEmpresaRespuesta>), StatusCodes.Status200OK)]
        public ActionResult<List<BEEmpresaRespuesta>> Listar()
        {
            List<BEEmpresa> ListaEmpresaBE = oBLLEmp.ListarSeleccionables();
            List<BEEmpresaRespuesta> respuesta = new List<BEEmpresaRespuesta>();

            foreach (BEEmpresa oEmpresaBE in ListaEmpresaBE)
            {
                BEEmpresaRespuesta item = new BEEmpresaRespuesta();

                item.EmpresaId = oEmpresaBE.EmpresaId;
                item.RazonSocial = oEmpresaBE.RazonSocial;

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEEmpresaAdminRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEmpresaAdminRespuesta>> ListarParaAdministracion()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaListar);

            List<BEEmpresa> ListaEmpresaBE = oBLLEmp.ListarAdministracion(oSesionBE);
            List<BEEmpresaAdminRespuesta> respuesta = new List<BEEmpresaAdminRespuesta>();

            foreach (BEEmpresa oEmpresaBE in ListaEmpresaBE)
            {
                respuesta.Add(Mapear(oEmpresaBE));
            }

            return Ok(respuesta);
        }

        [HttpGet("{empresaId:int}")]
        [ProducesResponseType(typeof(BEEmpresaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEEmpresaAdminRespuesta> Obtener(int empresaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaListar);

            BEEmpresa oFiltroBE = new BEEmpresa();
            oFiltroBE.EmpresaId = empresaId;

            return Ok(Mapear(oBLLEmp.ListarObjeto(oFiltroBE, oSesionBE)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEEmpresaAdminRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEEmpresaAdminRespuesta> Crear([FromBody] BEGuardarEmpresa peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaAlta);

            BEEmpresa oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Seleccionable = true;

            BEEmpresa oEmpresaBE = oBLLEmp.Guardar(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEmpresa,
                "Alta",
                "Alta de la empresa " + oEmpresaBE.RazonSocial + " (id " + oEmpresaBE.EmpresaId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener), new { empresaId = oEmpresaBE.EmpresaId }, Mapear(oEmpresaBE));
        }

        [HttpPut("{empresaId:int}")]
        [ProducesResponseType(typeof(BEEmpresaAdminRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEEmpresaAdminRespuesta> Modificar(
            int empresaId, [FromBody] BEGuardarEmpresa peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaModificar);

            BEEmpresa oEmpresaBE = oBLLEmp.Guardar(ADominio(empresaId, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEmpresa,
                "Modificacion",
                "Modificación de la empresa " + oEmpresaBE.RazonSocial + " (id " + empresaId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oEmpresaBE));
        }

        [HttpDelete("{empresaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int empresaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaBaja);

            BEEmpresa oEmpresaBE = new BEEmpresa();
            oEmpresaBE.EmpresaId = empresaId;

            oBLLEmp.Baja(oEmpresaBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEmpresa,
                "Baja",
                "Baja lógica de la empresa con id " + empresaId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BEEmpresa ADominio(int empresaId, BEGuardarEmpresa peticion)
        {
            BEEmpresa oEmpresaBE = new BEEmpresa();

            oEmpresaBE.EmpresaId = empresaId;
            oEmpresaBE.RazonSocial = peticion.RazonSocial;
            oEmpresaBE.CUIT = peticion.CUIT;
            oEmpresaBE.Email = peticion.Email;
            oEmpresaBE.Telefono = peticion.Telefono;
            oEmpresaBE.Direccion = peticion.Direccion;
            oEmpresaBE.Ciudad = peticion.Ciudad;
            oEmpresaBE.Provincia = peticion.Provincia;
            oEmpresaBE.Pais = peticion.Pais;

            return oEmpresaBE;
        }

        private BEEmpresaAdminRespuesta Mapear(BEEmpresa oEmpresaBE)
        {
            BEEmpresaAdminRespuesta item = new BEEmpresaAdminRespuesta();

            item.EmpresaId = oEmpresaBE.EmpresaId;
            item.RazonSocial = oEmpresaBE.RazonSocial;
            item.CUIT = oEmpresaBE.CUIT;
            item.Email = oEmpresaBE.Email;
            item.Telefono = oEmpresaBE.Telefono;
            item.Direccion = oEmpresaBE.Direccion;
            item.Ciudad = oEmpresaBE.Ciudad;
            item.Provincia = oEmpresaBE.Provincia;
            item.Pais = oEmpresaBE.Pais;
            item.Activo = oEmpresaBE.Activo;
            item.Protegido = oEmpresaBE.Protegido;
            item.Seleccionable = oEmpresaBE.Seleccionable;

            return item;
        }
    }
}
