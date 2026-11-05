using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/planes")]
    public class PlanController : ControladorBase
    {
        BLLPlan oBLLPlan = new BLLPlan();
        BLLPlanCaracteristica oBLLPlaCar = new BLLPlanCaracteristica();
        BLLBitacora oBLLBit = new BLLBitacora();
        BLLComentario oBLLCom = new BLLComentario();

        [HttpGet]
        [ProducesResponseType(typeof(List<BEPlanRespuesta>), StatusCodes.Status200OK)]
        public ActionResult<List<BEPlanRespuesta>> Listar()
        {
            List<BEPlanSuscripcion> ListaPlanBE = oBLLPlan.ListarTodo();
            List<BEPlanCaracteristica> ListaMatrizBE = oBLLPlaCar.ListarTodo();
            List<BEResumenValoracion> ListaResumenBE = oBLLCom.ListarResumen();
            List<BEPlanRespuesta> respuesta = new List<BEPlanRespuesta>();

            foreach (BEPlanSuscripcion oPlanBE in ListaPlanBE)
            {
                BEPlanRespuesta item = new BEPlanRespuesta();

                item.PlanId = oPlanBE.PlanId;
                item.Nombre = oPlanBE.Nombre;
                item.Descripcion = oPlanBE.Descripcion;
                item.PrecioMensual = oPlanBE.PrecioMensual;
                item.CantidadLicencias = oPlanBE.CantidadLicencias;
                item.Destacado = oPlanBE.Destacado;
                item.Caracteristicas = MapearCaracteristicas(ListaMatrizBE, oPlanBE.PlanId);

                MapearValoracion(item, ListaResumenBE);

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("{planId:int}/caracteristicas")]
        [ProducesResponseType(typeof(List<BEItemPlanCaracteristica>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEItemPlanCaracteristica>> ListarCaracteristicas(int planId)
        {
            ExigirPermiso(Permisos.PlanListar);

            BEPlanSuscripcion oFiltroBE = new BEPlanSuscripcion();
            oFiltroBE.PlanId = planId;

            return Ok(MapearCaracteristicas(oBLLPlaCar.ListarPorPlan(oFiltroBE), planId));
        }

        [HttpPut("{planId:int}/caracteristicas")]
        [ProducesResponseType(typeof(List<BEItemPlanCaracteristica>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEItemPlanCaracteristica>> GuardarCaracteristicas(
            int planId, [FromBody] BEGuardarPlanCaracteristicas peticion)
        {
            oBLLPlaCar.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.PlanModificar);

            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();
            oPlanBE.PlanId = planId;

            List<BEPlanCaracteristica> ListaPedidaBE = new List<BEPlanCaracteristica>();

            foreach (BEItemPlanCaracteristica item in peticion.Caracteristicas)
            {
                BEPlanCaracteristica oPedidaBE = new BEPlanCaracteristica();

                oPedidaBE.PlanId = planId;
                oPedidaBE.CaracteristicaId = item.CaracteristicaId;
                oPedidaBE.Incluido = item.Incluido;
                oPedidaBE.Detalle = item.Detalle;

                ListaPedidaBE.Add(oPedidaBE);
            }

            List<BEPlanCaracteristica> ListaGuardadaBE =
                oBLLPlaCar.Reemplazar(oPlanBE, ListaPedidaBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloPlan, "Caracteristicas",
                "El plan con id " + planId + " quedó con " + ListaGuardadaBE.Count + " características asociadas.",
                BLLBitacora.NivelInformativo));

            return Ok(MapearCaracteristicas(ListaGuardadaBE, planId));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEPlanSuscripcion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEPlanSuscripcion>> ListarParaAdministracion()
        {
            ExigirPermiso(Permisos.PlanListar);

            List<BEPlanSuscripcion> ListaPlanBE = oBLLPlan.ListarTodo();

            return Ok(ListaPlanBE);
        }

        [HttpGet("{planId:int}")]
        [ProducesResponseType(typeof(BEPlanSuscripcion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEPlanSuscripcion> Obtener(int planId)
        {
            ExigirPermiso(Permisos.PlanListar);

            BEPlanSuscripcion oFiltroBE = new BEPlanSuscripcion();
            oFiltroBE.PlanId = planId;

            return Ok(oBLLPlan.ListarObjeto(oFiltroBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEPlanSuscripcion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPlanSuscripcion> Crear([FromBody] BEGuardarPlan peticion)
        {
            oBLLPlan.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.PlanAlta);

            BEPlanSuscripcion oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BEPlanSuscripcion oPlanBE = oBLLPlan.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloPlan, "Alta",
                "Alta del plan " + oPlanBE.Nombre + " (id " + oPlanBE.PlanId + ").", BLLBitacora.NivelInformativo));

            return CreatedAtAction(nameof(Obtener), new { planId = oPlanBE.PlanId }, oPlanBE);
        }

        [HttpPut("{planId:int}")]
        [ProducesResponseType(typeof(BEPlanSuscripcion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPlanSuscripcion> Modificar(int planId, [FromBody] BEGuardarPlan peticion)
        {
            oBLLPlan.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.PlanModificar);

            BEPlanSuscripcion oPlanBE = oBLLPlan.Guardar(ADominio(planId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloPlan, "Modificacion",
                "Modificación del plan " + oPlanBE.Nombre + " (id " + planId + ").", BLLBitacora.NivelInformativo));

            return Ok(oPlanBE);
        }

        [HttpDelete("{planId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int planId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.PlanBaja);

            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();
            oPlanBE.PlanId = planId;

            oBLLPlan.Baja(oPlanBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloPlan, "Baja",
                "Baja lógica del plan con id " + planId + ".", BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BEPlanSuscripcion ADominio(int planId, BEGuardarPlan peticion)
        {
            BEPlanSuscripcion oPlanBE = new BEPlanSuscripcion();

            oPlanBE.PlanId = planId;
            oPlanBE.Nombre = peticion.Nombre;
            oPlanBE.Descripcion = peticion.Descripcion;
            oPlanBE.PrecioMensual = peticion.PrecioMensual;
            oPlanBE.CantidadLicencias = peticion.CantidadLicencias;
            oPlanBE.Destacado = peticion.Destacado;

            return oPlanBE;
        }

        private void MapearValoracion(BEPlanRespuesta item, List<BEResumenValoracion> ListaResumenBE)
        {
            item.PromedioValoracion = 0;
            item.CantidadValoraciones = 0;

            foreach (BEResumenValoracion oResumenBE in ListaResumenBE)
            {
                if (oResumenBE.PlanId == item.PlanId)
                {
                    item.PromedioValoracion = oResumenBE.Promedio;
                    item.CantidadValoraciones = oResumenBE.Cantidad;

                    return;
                }
            }
        }

        private List<BEItemPlanCaracteristica> MapearCaracteristicas(
            List<BEPlanCaracteristica> ListaMatrizBE, int planId)
        {
            List<BEItemPlanCaracteristica> respuesta = new List<BEItemPlanCaracteristica>();

            foreach (BEPlanCaracteristica oPlanCaracteristicaBE in ListaMatrizBE)
            {
                if (oPlanCaracteristicaBE.PlanId != planId)
                {
                    continue;
                }

                BEItemPlanCaracteristica item = new BEItemPlanCaracteristica();

                item.CaracteristicaId = oPlanCaracteristicaBE.CaracteristicaId;
                item.Incluido = oPlanCaracteristicaBE.Incluido;
                item.Detalle = oPlanCaracteristicaBE.Detalle;

                respuesta.Add(item);
            }

            return respuesta;
        }
    }

    [Route("api/caracteristicas")]
    public class CaracteristicaController : ControladorBase
    {
        BLLCaracteristica oBLLCar = new BLLCaracteristica();
        BLLBitacora oBLLBit = new BLLBitacora();

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
            oBLLCar.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CaracteristicaAlta);

            BECaracteristica oPeticionBE = ADominio(0, peticion);
            oPeticionBE.Activo = true;

            BECaracteristica oCaracteristicaBE = oBLLCar.Guardar(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCaracteristica, "Alta",
                "Alta de la característica " + oCaracteristicaBE.Nombre + " (id " + oCaracteristicaBE.CaracteristicaId + ").",
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
            oBLLCar.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CaracteristicaModificar);

            BECaracteristica oCaracteristicaBE = oBLLCar.Guardar(ADominio(caracteristicaId, peticion));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCaracteristica, "Modificacion",
                "Modificación de la característica " + oCaracteristicaBE.Nombre + " (id " + caracteristicaId + ").",
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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCaracteristica, "Baja",
                "Baja lógica de la característica con id " + caracteristicaId + ".", BLLBitacora.NivelInformativo));

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

    [Route("api/suscripciones")]
    public class SuscripcionController : ControladorBase
    {
        BLLSuscripcion oBLLSus = new BLLSuscripcion();
        BLLUsuario oBLLUsu = new BLLUsuario();
        BLLSesion oBLLSes = new BLLSesion();

        [HttpGet("mia")]
        [ProducesResponseType(typeof(BESuscripcionRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BESuscripcionRespuesta> ObtenerLaMia()
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BESuscripcion oFiltroBE = new BESuscripcion();
            oFiltroBE.EmpresaId = oUsuarioBE.EmpresaId;

            BESuscripcionConPlan oSuscripcion = oBLLSus.ListarObjeto(oFiltroBE);

            if (oSuscripcion == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado,
                    "Tu empresa todavía no tiene un plan asignado.");
            }

            BESuscripcionRespuesta respuesta = new BESuscripcionRespuesta();

            respuesta.SuscripcionId = oSuscripcion.Suscripcion.SuscripcionId;
            respuesta.PlanId = oSuscripcion.Suscripcion.PlanId;
            respuesta.Plan = oSuscripcion.Plan;
            respuesta.Estado = oSuscripcion.Suscripcion.Estado;
            respuesta.FechaInicio = oSuscripcion.Suscripcion.FechaInicio;
            respuesta.FechaFin = oSuscripcion.Suscripcion.FechaFin;
            respuesta.PrecioMensual = oSuscripcion.PrecioMensual;
            respuesta.CantidadLicencias = oSuscripcion.CantidadLicencias;
            respuesta.Cancelable = oBLLSus.EsCancelable(oSuscripcion);

            return Ok(respuesta);
        }
    }

    [Route("api/licencias")]
    public class LicenciaController : ControladorBase
    {
        BLLLicencia oBLLLic = new BLLLicencia();
        BLLBitacora oBLLBit = new BLLBitacora();

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
        public ActionResult<BEMensajeRespuesta> Asignar([FromBody] BELicencia peticion)
        {
            oBLLLic.Validar(peticion);

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
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloLicencia, "Revocacion",
                "Licencia " + licenciaId + " revocada a " + oUsuarioBE.Email + ". Cupo liberado.",
                BLLBitacora.NivelInformativo));

            return Ok(new BEMensajeRespuesta("Licencia revocada. Cupo liberado exitosamente."));
        }
    }
}
