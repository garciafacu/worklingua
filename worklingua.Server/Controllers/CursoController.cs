using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/cursos")]
    public class CursoController : ControladorBase
    {
        BLLCurso oBLLCur = new BLLCurso();
        BLLBitacora oBLLBit = new BLLBitacora();

        [HttpGet]
        [ProducesResponseType(typeof(List<BECursoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECursoRespuesta>> Buscar([FromQuery] BEFiltroCurso oFiltroBE)
        {
            oBLLCur.Validar(oFiltroBE);

            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            List<BECurso> ListaCursoBE = oBLLCur.Buscar(oFiltroBE, oSesionBE);
            List<BECursoRespuesta> respuesta = new List<BECursoRespuesta>();

            foreach (BECurso oCursoBE in ListaCursoBE)
            {
                BECursoRespuesta item = new BECursoRespuesta();

                item.CursoId = oCursoBE.CursoId;
                item.Nombre = oCursoBE.Nombre;
                item.Descripcion = oCursoBE.Descripcion;
                item.Nivel = oCursoBE.Nivel;
                item.DuracionHoras = oCursoBE.DuracionHoras;
                item.Idioma = oCursoBE.Idioma;
                item.EsGlobal = !oCursoBE.EmpresaId.HasValue;
                item.Sector = oCursoBE.Sector;
                item.Etiquetas = oCursoBE.Etiquetas;

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("idiomas")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<string>> ListarIdiomas()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLCur.ListarIdiomas(oSesionBE));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BECurso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECurso>> ListarParaAdministracion()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLCur.ListarParaAdministracion(oSesionBE));
        }

        [HttpGet("{cursoId:int}")]
        [ProducesResponseType(typeof(BECurso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BECurso> Obtener(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = cursoId;

            return Ok(oBLLCur.ListarObjeto(oFiltroBE, oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BECurso), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECurso> Crear([FromBody] BEGuardarCurso peticion)
        {
            oBLLCur.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CursoAlta);

            BECurso oCursoBE = oBLLCur.Guardar(ADominio(0, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "Alta",
                "Alta del curso " + oCursoBE.Nombre + " (id " + oCursoBE.CursoId + ").", BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener), new { cursoId = oCursoBE.CursoId }, oCursoBE);
        }

        [HttpPut("{cursoId:int}")]
        [ProducesResponseType(typeof(BECurso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECurso> Modificar(
            int cursoId, [FromBody] BEGuardarCurso peticion)
        {
            oBLLCur.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BECurso oCursoBE = oBLLCur.Guardar(ADominio(cursoId, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "Modificacion",
                "Modificación del curso " + oCursoBE.Nombre + " (id " + cursoId + ").", BLLBitacora.NivelInformativo));

            return Ok(oCursoBE);
        }

        [HttpDelete("{cursoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoBaja);

            BECurso oCursoBE = new BECurso();
            oCursoBE.CursoId = cursoId;

            oBLLCur.Baja(oCursoBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "Baja",
                "Baja lógica del curso con id " + cursoId + ".", BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BECurso ADominio(int cursoId, BEGuardarCurso peticion)
        {
            BECurso oCursoBE = new BECurso();

            oCursoBE.CursoId = cursoId;
            oCursoBE.Idioma = peticion.Idioma;
            oCursoBE.Nombre = peticion.Nombre;
            oCursoBE.Descripcion = peticion.Descripcion;
            oCursoBE.Nivel = peticion.Nivel;
            oCursoBE.DuracionHoras = peticion.DuracionHoras;
            oCursoBE.Sector = peticion.Sector;
            oCursoBE.FechaPublicacion = peticion.FechaPublicacion;
            oCursoBE.FechaFin = peticion.FechaFin;

            if (peticion.EtiquetaIds != null)
            {
                foreach (int etiquetaId in peticion.EtiquetaIds)
                {
                    oCursoBE.Etiquetas.Add(new BEEtiqueta(etiquetaId, null, true, null));
                }
            }

            return oCursoBE;
        }
    }

    [Route("api/cursos/{cursoId:int}/modulos")]
    public class ModuloController : ControladorBase
    {
        BLLModulo oBLLMod = new BLLModulo();

        [HttpGet]
        [ProducesResponseType(typeof(List<BEModulo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEModulo>> Listar(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            BECurso oCursoBE = new BECurso();
            oCursoBE.CursoId = cursoId;

            return Ok(oBLLMod.ListarPorCurso(oCursoBE, oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEModulo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEModulo> Crear(int cursoId, [FromBody] BEGuardarModulo peticion)
        {
            oBLLMod.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEModulo oModuloBE = new BEModulo(
                0, cursoId, peticion.Nombre, peticion.Descripcion, peticion.Contenido, peticion.OrdenModulo, true);

            return Ok(oBLLMod.Guardar(oModuloBE, oSesionBE));
        }

        [HttpDelete("{moduloId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Eliminar(int cursoId, int moduloId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEModulo oModuloBE = new BEModulo();
            oModuloBE.CursoId = cursoId;
            oModuloBE.ModuloId = moduloId;

            oBLLMod.Baja(oModuloBE, oSesionBE);

            return NoContent();
        }
    }

    [Route("api/etiquetas")]
    public class EtiquetaController : ControladorBase
    {
        BLLEtiqueta oBLLEti = new BLLEtiqueta();
        BLLBitacora oBLLBit = new BLLBitacora();

        [HttpGet]
        [ProducesResponseType(typeof(List<BEEtiqueta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEtiqueta>> Listar()
        {
            ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLEti.Listar());
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEEtiqueta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEEtiqueta> Crear([FromBody] BEEtiqueta oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEEtiqueta oEtiquetaBE = oBLLEti.Crear(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "AltaEtiqueta",
                "Alta de la etiqueta " + oEtiquetaBE.Nombre + " (id " + oEtiquetaBE.EtiquetaId + ").",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, oEtiquetaBE);
        }
    }

    [Route("api/progreso")]
    public class ProgresoController : ControladorBase
    {
        BLLProgreso oBLLPro = new BLLProgreso();

        [HttpGet("mio")]
        [ProducesResponseType(typeof(List<BECursoConProgreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECursoConProgreso>> ListarMio()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgreso);

            return Ok(oBLLPro.ListarPorUsuario(oSesionBE));
        }

        [HttpGet("empresa")]
        [ProducesResponseType(typeof(List<BEProgresoUsuario>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEProgresoUsuario>> ListarEmpresa()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgresoEmpresa);

            return Ok(oBLLPro.ListarPorEmpresa(oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<BECursoConProgreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BECursoConProgreso>> Guardar([FromBody] BEGuardarProgreso peticion)
        {
            oBLLPro.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgreso);

            return Ok(oBLLPro.Guardar(peticion, oSesionBE));
        }
    }

    [Route("api/activos")]
    public class ActivoController : ControladorBase
    {
        BLLActivoPedagogico oBLLAct = new BLLActivoPedagogico();
        BLLBitacora oBLLBit = new BLLBitacora();

        [HttpGet]
        [ProducesResponseType(typeof(List<BEActivoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEActivoRespuesta>> Listar([FromQuery] int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.CursoId = cursoId;

            List<BEActivoRespuesta> respuesta = new List<BEActivoRespuesta>();

            foreach (BEActivoPedagogico oActivoBE in oBLLAct.ListarPorCurso(oFiltroBE, oSesionBE))
            {
                respuesta.Add(Mapear(oActivoBE));
            }

            return Ok(respuesta);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEActivoRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEActivoRespuesta> Crear(
            [FromForm] int cursoId,
            [FromForm] string nombre,
            [FromForm] string tipoContenido,
            [FromForm] string descripcion,
            [FromForm] bool confirmarNombre,
            [FromForm] int? moduloId,
            IFormFile archivo)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEActivoPedagogico oActivoBE = new BEActivoPedagogico();
            oActivoBE.CursoId = cursoId;
            oActivoBE.ModuloId = moduloId;
            oActivoBE.Nombre = nombre;
            oActivoBE.TipoContenido = tipoContenido;
            oActivoBE.Descripcion = descripcion;

            BEActivoPedagogico oGuardadoBE =
                oBLLAct.Guardar(oActivoBE, ALeido(archivo), confirmarNombre, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloActivo, "Alta",
                "Alta del activo " + oGuardadoBE.Nombre + " (id " + oGuardadoBE.ActivoPedagogicoId + ") en el curso " + oGuardadoBE.CursoId + ".",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, Mapear(oGuardadoBE));
        }

        [HttpPatch("{activoId:int}/publicacion")]
        [ProducesResponseType(typeof(BEActivoRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEActivoRespuesta> CambiarPublicacion(
            int activoId, [FromBody] BEActivoPedagogico oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            oPeticionBE.ActivoPedagogicoId = activoId;

            BEActivoPedagogico oActivoBE = oBLLAct.CambiarEstado(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloActivo,
                oActivoBE.Estado == BLLActivoPedagogico.EstadoPublicado ? "Publicacion" : "VueltaABorrador",
                "El activo " + oActivoBE.Nombre + " (id " + activoId + ") quedó en estado " + oActivoBE.Estado + ".",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oActivoBE));
        }

        [HttpPatch("{activoId:int}/estado")]
        [ProducesResponseType(typeof(BEActivoRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEActivoRespuesta> CambiarEstado(
            int activoId, [FromBody] BEActivoPedagogico oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            oPeticionBE.ActivoPedagogicoId = activoId;

            BEActivoPedagogico oActivoBE = oBLLAct.CambiarActivo(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloActivo,
                oActivoBE.Activo == true ? "Reactivacion" : "Baja",
                "El activo " + oActivoBE.Nombre + " (id " + activoId + ") quedó " + (oActivoBE.Activo == true ? "vigente." : "dado de baja."),
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oActivoBE));
        }

        private BEArchivoSubido ALeido(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            using (MemoryStream memoria = new MemoryStream())
            {
                archivo.CopyTo(memoria);

                return new BEArchivoSubido(archivo.FileName, memoria.ToArray());
            }
        }

        private BEActivoRespuesta Mapear(BEActivoPedagogico oActivoBE)
        {
            BEActivoRespuesta respuesta = new BEActivoRespuesta();

            respuesta.ActivoPedagogicoId = oActivoBE.ActivoPedagogicoId;
            respuesta.CursoId = oActivoBE.CursoId;
            respuesta.ModuloId = oActivoBE.ModuloId;
            respuesta.Modulo = oActivoBE.Modulo;
            respuesta.Nombre = oActivoBE.Nombre;
            respuesta.TipoContenido = oActivoBE.TipoContenido;
            respuesta.Descripcion = oActivoBE.Descripcion;
            respuesta.UrlArchivo = oActivoBE.UrlArchivo;
            respuesta.Activo = oActivoBE.Activo.HasValue && oActivoBE.Activo.Value;
            respuesta.Estado = oActivoBE.Estado;

            return respuesta;
        }
    }

    [Route("api/cursos")]
    public class VersionCursoController : ControladorBase
    {
        BLLVersionCurso oBLLVer = new BLLVersionCurso();
        BLLBitacora oBLLBit = new BLLBitacora();

        [HttpGet("{cursoId:int}/versiones")]
        [ProducesResponseType(typeof(List<BEVersionContenido>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEVersionContenido>> Listar(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLVer.ListarPorCurso(cursoId, oSesionBE));
        }

        [HttpGet("versiones/{versionId:int}")]
        [ProducesResponseType(typeof(BEVersionContenido), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEVersionContenido> Obtener(int versionId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLVer.Obtener(versionId, oSesionBE));
        }

        [HttpPost("{cursoId:int}/versiones")]
        [ProducesResponseType(typeof(BEVersionContenido), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEVersionContenido> Crear(
            int cursoId, [FromBody] BEVersionContenido oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEVersionContenido oVersionBE = oBLLVer.Crear(
                cursoId, oPeticionBE == null ? null : oPeticionBE.Observaciones, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "Snapshot",
                "Punto de restauración " + oVersionBE.NumeroVersion + " del curso " + cursoId + ".",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, oVersionBE);
        }

        [HttpPost("versiones/{versionId:int}/restaurar")]
        [ProducesResponseType(typeof(BEResultadoRestauracion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEResultadoRestauracion> Restaurar(int versionId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEResultadoRestauracion respuesta = oBLLVer.Restaurar(versionId, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "Restauracion",
                "Restauración de la versión " + versionId + " en el curso " + respuesta.CursoId + " (" + respuesta.Omitidos + " activos omitidos).",
                BLLBitacora.NivelInformativo));

            return Ok(respuesta);
        }

        [HttpDelete("versiones/{versionId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Eliminar(int versionId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEVersionContenido oVersionBE = oBLLVer.Eliminar(versionId, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "BajaVersion",
                "Baja del punto de restauración " + oVersionBE.NumeroVersion + " del curso " + oVersionBE.CursoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        [HttpGet("versiones/comparar")]
        [ProducesResponseType(typeof(List<BEDiferenciaVersion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEDiferenciaVersion>> Comparar(
            [FromQuery] int izquierda, [FromQuery] int derecha)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLVer.Comparar(izquierda, derecha, oSesionBE));
        }

        [HttpPost("{cursoId:int}/clonar")]
        [ProducesResponseType(typeof(BEResultadoRestauracion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEResultadoRestauracion> Clonar(
            int cursoId, [FromBody] BECurso peticion)
        {
            oBLLVer.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.CursoAlta);

            BEResultadoRestauracion respuesta = oBLLVer.Clonar(
                cursoId, peticion == null ? null : peticion.Nombre, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "Clonacion",
                "Clonación del curso " + cursoId + " en el curso " + respuesta.CursoId + " (" + respuesta.Omitidos + " activos omitidos).",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, respuesta);
        }
    }
}
