using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// Alertas preventivas de deserción (CU-001-010).
    ///
    /// Una alerta es una regla: título, mensaje y una condición de inactividad
    /// sobre un grupo de empleados. Al guardarla se emite, y se puede volver a
    /// emitir cuando el administrador quiera. Desactivarla detiene los envíos
    /// sin borrar nada (camino alternativo 3).
    ///
    /// No hay procesos de fondo en el proyecto: el envío automático que
    /// describe el caso de uso se implementa como emisión a pedido.
    ///
    /// Sin Alerta.VerTodasLasEmpresas se ven y se gestionan únicamente las
    /// alertas de la empresa del usuario, igual que Departamentos o Licencias.
    /// </summary>
    public class BLLAlerta
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaMensaje = 2000;
        private const int DiasMinimos = 1;
        private const int DiasMaximos = 365;

        MPPAlerta oMPPAle;
        MPPUsuario oMPPUsu;
        BLLSeguridad oBLLSeg;
        BLLEmpresa oBLLEmp;
        BLLDepartamento oBLLDep;

        public BLLAlerta()
        {
            oMPPAle = new MPPAlerta();
            oMPPUsu = new MPPUsuario();
            oBLLSeg = new BLLSeguridad();
            oBLLEmp = new BLLEmpresa();
            oBLLDep = new BLLDepartamento();
        }

        public List<BEAlerta> Listar(BESesion oSesionBE)
        {
            BEAlerta oFiltroBE = new BEAlerta();
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue)
            {
                oFiltroBE.EmpresaId = empresaQueLimita.Value;
            }

            return oMPPAle.Listar(oFiltroBE);
        }

        /// <summary>Una alerta de otra empresa se informa como inexistente.</summary>
        public BEAlerta ListarObjeto(BEAlerta Objeto, BESesion oSesionBE)
        {
            BEAlerta oAlertaBE = oMPPAle.ListarObjeto(Objeto);

            if (oAlertaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La alerta no existe.");
            }

            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue && oAlertaBE.EmpresaId != empresaQueLimita.Value)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La alerta no existe.");
            }

            return oAlertaBE;
        }

        /// <summary>
        /// El escenario principal: valida, verifica que alguien cumpla la
        /// condición, guarda la regla y emite (pasos 9 a 14).
        ///
        /// La verificación va ANTES del alta: si nadie está inactivo no se
        /// guarda nada, que es lo que espera quien recibe el mensaje del camino
        /// alternativo 2 y corrige la condición.
        /// </summary>
        public BEEmisionRespuesta GuardarYEmitir(BEAlerta Objeto, BESesion oSesionBE)
        {
            Validar(Objeto);
            ResolverAlcance(Objeto, oSesionBE);

            List<BEEmpleadoInactivo> ListaInactivosBE = ObtenerInactivos(Objeto);

            ExigirAlgunInactivo(ListaInactivosBE);

            Objeto.UsuarioId = oSesionBE.UsuarioId;
            Objeto.AlertaId = oMPPAle.Alta(Objeto);

            return Emitir(Objeto.AlertaId, ListaInactivosBE);
        }

        /// <summary>
        /// Vuelve a emitir una alerta ya configurada. Recalcula quién está
        /// inactivo hoy: el sentido de reemitir es alcanzar a quienes se
        /// colgaron desde la última vez.
        /// </summary>
        public BEEmisionRespuesta Reemitir(BEAlerta Objeto, BESesion oSesionBE)
        {
            BEAlerta oAlertaBE = ListarObjeto(Objeto, oSesionBE);

            if (oAlertaBE.Activo != true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La alerta está desactivada y no se emite.");
            }

            List<BEEmpleadoInactivo> ListaInactivosBE = ObtenerInactivos(oAlertaBE);

            ExigirAlgunInactivo(ListaInactivosBE);

            return Emitir(oAlertaBE.AlertaId, ListaInactivosBE);
        }

        /// <summary>Camino alternativo 3: desactivar detiene los envíos.</summary>
        public BEAlerta CambiarEstado(BEAlerta Objeto, BESesion oSesionBE)
        {
            BEAlerta oAlertaBE = ListarObjeto(Objeto, oSesionBE);

            bool activar = Objeto.Activo.HasValue && Objeto.Activo.Value;

            if (oAlertaBE.Activo == activar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    activar ? "La alerta ya está activa." : "La alerta ya está desactivada.");
            }

            oMPPAle.CambiarEstado(Objeto);

            return ListarObjeto(Objeto, oSesionBE);
        }

        /// <summary>
        /// Previsualiza a quiénes alcanzaría una condición, sin emitir nada.
        /// Es lo que deja ver el formulario antes de confirmar.
        /// </summary>
        public List<BEEmpleadoInactivo> Previsualizar(BEAlerta Objeto, BESesion oSesionBE)
        {
            ValidarCondicion(Objeto);
            ResolverAlcance(Objeto, oSesionBE);

            return ObtenerInactivos(Objeto);
        }

        private BEEmisionRespuesta Emitir(int alertaId, List<BEEmpleadoInactivo> ListaInactivosBE)
        {
            List<int> usuarioIds = new List<int>();

            foreach (BEEmpleadoInactivo oEmpleadoBE in ListaInactivosBE)
            {
                usuarioIds.Add(oEmpleadoBE.UsuarioId);
            }

            BEEmisionRespuesta respuesta = new BEEmisionRespuesta();

            respuesta.Destinatarios = oMPPAle.Emitir(alertaId, usuarioIds);
            respuesta.Empleados = ListaInactivosBE;

            BEAlerta oFiltroBE = new BEAlerta();
            oFiltroBE.AlertaId = alertaId;

            respuesta.Alerta = Mapear(oMPPAle.ListarObjeto(oFiltroBE));

            return respuesta;
        }

        /// <summary>
        /// El rol alcanzado es el mismo que necesita licencia: solo esas
        /// cuentas pueden entrar a Mis cursos, así que son las únicas a las que
        /// tiene sentido pedirles que retomen la capacitación.
        /// </summary>
        private List<BEEmpleadoInactivo> ObtenerInactivos(BEAlerta Objeto)
        {
            return oMPPAle.ListarInactivos(Objeto, Configuracion.LicenciasRolAlcanzado);
        }

        /// <summary>Camino alternativo 2.</summary>
        private void ExigirAlgunInactivo(List<BEEmpleadoInactivo> ListaInactivosBE)
        {
            if (ListaInactivosBE.Count == 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Los usuarios seleccionados no registran inactividad. Seleccione otra condición.");
            }
        }

        private void ResolverAlcance(BEAlerta Objeto, BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue)
            {
                Objeto.EmpresaId = empresaQueLimita.Value;
            }

            if (Objeto.EmpresaId == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Elegí la empresa de la alerta.");
            }

            ExigirEmpresaExistente(Objeto.EmpresaId);

            // Null queda como "toda la empresa"; uno de otra empresa da 400.
            Objeto.DepartamentoId = oBLLDep.ResolverParaUsuario(Objeto.DepartamentoId, Objeto.EmpresaId);
        }

        /// <summary>Camino alternativo 1.</summary>
        private void Validar(BEAlerta Objeto)
        {
            Objeto.Titulo = Normalizar(Objeto.Titulo);
            Objeto.Mensaje = Normalizar(Objeto.Mensaje);

            if (string.IsNullOrEmpty(Objeto.Titulo) || string.IsNullOrEmpty(Objeto.Mensaje))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Debe completar el título y el contenido del mensaje.");
            }

            if (Objeto.Titulo.Length > LongitudMaximaTitulo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El título no puede superar los " + LongitudMaximaTitulo + " caracteres.");
            }

            if (Objeto.Mensaje.Length > LongitudMaximaMensaje)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El mensaje no puede superar los " + LongitudMaximaMensaje + " caracteres.");
            }

            ValidarCondicion(Objeto);
        }

        private void ValidarCondicion(BEAlerta Objeto)
        {
            if (Objeto.DiasInactividad < DiasMinimos || Objeto.DiasInactividad > DiasMaximos)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La condición de inactividad va de " + DiasMinimos + " a " + DiasMaximos + " días.");
            }
        }

        private void ExigirEmpresaExistente(int empresaId)
        {
            BEEmpresa oFiltroBE = new BEEmpresa();
            oFiltroBE.EmpresaId = empresaId;

            if (oBLLEmp.ListarObjeto(oFiltroBE) == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "La empresa no existe.");
            }
        }

        private string Normalizar(string valor)
        {
            return valor == null ? null : valor.Trim();
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

        /// <summary>
        /// Mismo patrón que el resto del Backoffice: sin alcance sobre todas
        /// las empresas, se ve únicamente la propia.
        /// </summary>
        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            if (oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.AlertaVerTodasLasEmpresas))
            {
                return null;
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = oSesionBE.UsuarioId;

            BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

            if (oUsuarioBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.SesionInvalida, "El usuario de la sesión no existe.");
            }

            return oUsuarioBE.EmpresaId;
        }
    }
}
