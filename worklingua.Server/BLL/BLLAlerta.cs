using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLAlerta
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaMensaje = 2000;
        private const int DiasMinimos = 1;
        private const int DiasMaximos = 365;

        MPPAlerta oMPPAle = new MPPAlerta();
        BLLSeguridad oBLLSeg = new BLLSeguridad();
        BLLEmpresa oBLLEmp = new BLLEmpresa();
        BLLDepartamento oBLLDep = new BLLDepartamento();

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

        private BEAlerta ListarObjeto(BEAlerta Objeto, BESesion oSesionBE)
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

        private List<BEEmpleadoInactivo> ObtenerInactivos(BEAlerta Objeto)
        {
            return oMPPAle.ListarInactivos(Objeto, Configuracion.LicenciasRolAlcanzado);
        }

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

            Objeto.DepartamentoId = oBLLDep.ResolverParaUsuario(Objeto.DepartamentoId, Objeto.EmpresaId);
        }

        private void Validar(BEAlerta Objeto)
        {
            Objeto.Titulo = Normalizar(Objeto.Titulo);
            Objeto.Mensaje = Normalizar(Objeto.Mensaje);

            if (string.IsNullOrEmpty(Objeto.Titulo) || string.IsNullOrEmpty(Objeto.Mensaje))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Debe completar el título y el contenido del mensaje.");
            }

            ReglasNegocio.ExigirLargoMaximo(Objeto.Titulo, LongitudMaximaTitulo,
                "El título no puede superar los " + LongitudMaximaTitulo + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Mensaje, LongitudMaximaMensaje,
                "El mensaje no puede superar los " + LongitudMaximaMensaje + " caracteres.");

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

            oBLLEmp.ListarObjeto(oFiltroBE);
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

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            return oBLLSeg.EmpresaQueLimita(oSesionBE, Permisos.AlertaVerTodasLasEmpresas);
        }

        public void Validar(BEGuardarAlerta Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Titulo, "Debe completar el título y el contenido del mensaje.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Titulo, 150, "El título no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Mensaje, "Debe completar el título y el contenido del mensaje.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Mensaje, 2000, "El mensaje no puede superar los 2000 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.DiasInactividad, 1, 365,
                "La condición de inactividad va de 1 a 365 días.");
        }
    }

    public class BLLNotificacion
    {
        private const int TopeBandeja = 20;

        MPPNotificacion oMPPNot = new MPPNotificacion();

        public BEBandejaNotificaciones ObtenerBandeja(BESesion oSesionBE)
        {
            BEBandejaNotificaciones respuesta = new BEBandejaNotificaciones();

            respuesta.Notificaciones = oMPPNot.ListarPorUsuario(oSesionBE.UsuarioId, TopeBandeja);
            respuesta.NoLeidas = oMPPNot.ContarNoLeidas(oSesionBE.UsuarioId);

            return respuesta;
        }

        public BEBandejaNotificaciones MarcarLeidas(int? notificacionId, BESesion oSesionBE)
        {
            oMPPNot.MarcarLeidas(oSesionBE.UsuarioId, notificacionId);

            return ObtenerBandeja(oSesionBE);
        }
    }
}
