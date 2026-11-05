using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/empresas")]
    public class EmpresaController : ControladorBase
    {
        BLLEmpresa oBLLEmp = new BLLEmpresa();
        BLLBitacora oBLLBit = new BLLBitacora();

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
            oBLLEmp.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaAlta);

            BEEmpresa oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Seleccionable = true;

            BEEmpresa oEmpresaBE = oBLLEmp.Guardar(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloEmpresa, "Alta",
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
            oBLLEmp.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.EmpresaModificar);

            BEEmpresa oEmpresaBE = oBLLEmp.Guardar(ADominio(empresaId, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloEmpresa, "Modificacion",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloEmpresa, "Baja",
                "Baja lógica de la empresa con id " + empresaId + ".", BLLBitacora.NivelInformativo));

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

    [Route("api/departamentos")]
    public class DepartamentoController : ControladorBase
    {
        BLLDepartamento oBLLDep = new BLLDepartamento();
        BLLBitacora oBLLBit = new BLLBitacora();

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
            oBLLDep.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoAlta);

            BEDepartamento oDepartamentoBE = oBLLDep.Guardar(ADominio(0, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloDepartamento, "Alta",
                "Alta del departamento " + oDepartamentoBE.Nombre + " en " + oDepartamentoBE.Empresa + " (id " + oDepartamentoBE.DepartamentoId + ").",
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
            oBLLDep.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.DepartamentoModificar);

            BEDepartamento oDepartamentoBE = oBLLDep.Guardar(ADominio(departamentoId, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloDepartamento, "Modificacion",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloDepartamento,
                oPeticionBE.Activo == true ? "Activacion" : "Desactivacion",
                (oPeticionBE.Activo == true ? "Activación" : "Desactivación") + " del departamento " + oDepartamentoBE.Nombre + " (id " + departamentoId + ").",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloDepartamento, "Baja",
                "Baja lógica del departamento con id " + departamentoId + ".", BLLBitacora.NivelInformativo));

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

    [Route("api/padron")]
    public class PadronController : ControladorBase
    {
        BLLPadron oBLLPad = new BLLPadron();

        [HttpPost]
        [ProducesResponseType(typeof(BEResultadoPadron), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEResultadoPadron> Procesar([FromBody] BEPadron peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioInvitar);

            return Ok(oBLLPad.Procesar(peticion, oSesionBE));
        }
    }
}
