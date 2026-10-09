using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLTicket
    {
        public const string EstadoPendiente = "PENDIENTE";
        public const string EstadoEnProceso = "EN_PROCESO";
        public const string EstadoRespondido = "RESPONDIDO";
        public const string EstadoCerrado = "CERRADO";

        private const int LongitudMinimaAsunto = 3;
        private const int LongitudMaximaAsunto = 150;
        private const int LongitudMaximaTexto = 2000;

        static readonly string[] Estados = { EstadoPendiente, EstadoEnProceso, EstadoRespondido, EstadoCerrado };

        MPPTicket oMPPTic;
        BLLUsuario oBLLUsu;
        BLLSeguridad oBLLSeg;
        BLLSuscripcion oBLLSus;
        BLLCurso oBLLCur;
        BLLIdioma oBLLIdi;
        BLLTraduccion oBLLTra;
        BLLBitacora oBLLBit;
        ServicioEmailSoporte oServicioEmail;

        public BLLTicket()
        {
            oMPPTic = new MPPTicket();
            oBLLUsu = new BLLUsuario();
            oBLLSeg = new BLLSeguridad();
            oBLLSus = new BLLSuscripcion();
            oBLLCur = new BLLCurso();
            oBLLIdi = new BLLIdioma();
            oBLLTra = new BLLTraduccion();
            oBLLBit = new BLLBitacora();
            oServicioEmail = new ServicioEmailSoporte();
        }

        public BEOpcionesTicket ListarOpciones(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BEEmpresa oEmpresaBE = new BEEmpresa();
            oEmpresaBE.EmpresaId = oUsuarioBE.EmpresaId;

            return new BEOpcionesTicket(
                oBLLSus.ListarPorEmpresa(oEmpresaBE),
                oBLLCur.Buscar(new BEFiltroCurso(), oSesionBE));
        }

        public List<BETicketConDetalle> Listar(BEFiltroTicket Objeto, BESesion oSesionBE)
        {
            BEFiltroTicket oFiltroBE = new BEFiltroTicket(ValidarEstado(Objeto.Estado, true), null, null, null, Objeto.Administracion);

            if (Objeto.Administracion)
            {
                ExigirOperador(oSesionBE);
            }
            else
            {
                BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);
                oFiltroBE.EmpresaId = oUsuarioBE.EmpresaId;

                if (!oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.TicketVerEmpresa))
                {
                    oFiltroBE.UsuarioId = oUsuarioBE.UsuarioId;
                }
            }

            List<BETicketConDetalle> ListaTicketBE = oMPPTic.Listar(oFiltroBE);

            return ListaTicketBE == null ? new List<BETicketConDetalle>() : ListaTicketBE;
        }

        public BETicketConDetalle ListarObjeto(BEFiltroTicket Objeto, BESesion oSesionBE)
        {
            BETicketConDetalle oTicket = Obtener(Objeto.TicketId ?? 0);

            ValidarAcceso(oTicket, Objeto.Administracion, oSesionBE);

            List<BEMensajeTicketConAutor> ListaMensajeBE = oMPPTic.ListarMensajes(oTicket.Ticket);
            oTicket.Mensajes = ListaMensajeBE == null ? new List<BEMensajeTicketConAutor>() : ListaMensajeBE;

            return oTicket;
        }

        public BETicketConDetalle Guardar(BECrearTicket Objeto, BESesion oSesionBE)
        {
            string asunto = Normalizar(Objeto.Asunto);
            string texto = ValidarTexto(Objeto.Texto);

            if (asunto == null || asunto.Length < LongitudMinimaAsunto || asunto.Length > LongitudMaximaAsunto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El asunto tiene que tener entre " + LongitudMinimaAsunto + " y " + LongitudMaximaAsunto + " caracteres.");
            }

            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BESuscripcion oFiltroSuscripcionBE = new BESuscripcion();
            oFiltroSuscripcionBE.SuscripcionId = Objeto.SuscripcionId;

            BESuscripcionConPlan oSuscripcion = oBLLSus.ListarPorId(oFiltroSuscripcionBE);

            if (oSuscripcion.Suscripcion.EmpresaId != oUsuarioBE.EmpresaId)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La contratación no existe.");
            }

            if (Objeto.CursoId.HasValue)
            {
                BECurso oFiltroCursoBE = new BECurso();
                oFiltroCursoBE.CursoId = Objeto.CursoId.Value;

                if (!oBLLCur.ListarObjeto(oFiltroCursoBE, oSesionBE).Activo)
                {
                    throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El curso elegido no está disponible.");
                }
            }

            BEIdioma oFiltroIdiomaBE = new BEIdioma();
            oFiltroIdiomaBE.CodigoISO = Objeto.CodigoIdioma;

            BEIdioma oIdiomaBE = oBLLIdi.ResolverPorCodigo(oFiltroIdiomaBE);
            DateTime ahora = DateTime.Now;

            BETicket oTicketBE = new BETicket(
                0,
                oUsuarioBE.EmpresaId,
                oUsuarioBE.UsuarioId,
                Objeto.SuscripcionId,
                Objeto.CursoId,
                asunto,
                EstadoPendiente,
                ahora,
                ahora,
                null,
                oIdiomaBE == null ? (int?)null : oIdiomaBE.IdiomaId);

            BEMensajeTicket oMensajeBE = new BEMensajeTicket();
            oMensajeBE.Texto = texto;

            oTicketBE.TicketId = oMPPTic.Guardar(oTicketBE, oMensajeBE);

            Registrar(
                oSesionBE.UsuarioId,
                "TicketCreado",
                "Consulta #" + oTicketBE.TicketId + " (" + asunto + ") de la empresa " + oUsuarioBE.EmpresaId +
                " sobre la contratación " + Objeto.SuscripcionId + ".",
                BLLBitacora.NivelInformativo);

            BEFiltroTicket oFiltroBE = new BEFiltroTicket();
            oFiltroBE.TicketId = oTicketBE.TicketId;

            return ListarObjeto(oFiltroBE, oSesionBE);
        }

        public BETicketConDetalle GuardarMensaje(BEMensajeTicket Objeto, BESesion oSesionBE)
        {
            string texto = ValidarTexto(Objeto.Texto);
            BETicketConDetalle oTicket = Obtener(Objeto.TicketId);

            ValidarAcceso(oTicket, Objeto.EsOperador, oSesionBE);

            if (oTicket.Ticket.Estado == EstadoCerrado)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La consulta está cerrada. Si necesitás más ayuda, abrí una nueva.");
            }

            Objeto.Texto = texto;
            Objeto.UsuarioId = oSesionBE.UsuarioId;

            BETicket oEstadoBE = new BETicket();
            oEstadoBE.TicketId = oTicket.Ticket.TicketId;
            oEstadoBE.Estado = Objeto.EsOperador ? EstadoRespondido : EstadoPendiente;

            oMPPTic.GuardarMensaje(Objeto, oEstadoBE);

            Registrar(
                oSesionBE.UsuarioId,
                Objeto.EsOperador ? "TicketRespondido" : "TicketMensajeCliente",
                "Mensaje en la consulta #" + oTicket.Ticket.TicketId + "; estado " + oEstadoBE.Estado + ".",
                BLLBitacora.NivelInformativo);

            BEFiltroTicket oFiltroBE = new BEFiltroTicket();
            oFiltroBE.TicketId = oTicket.Ticket.TicketId;
            oFiltroBE.Administracion = Objeto.EsOperador;

            BETicketConDetalle oActualizado = ListarObjeto(oFiltroBE, oSesionBE);

            if (Objeto.EsOperador)
            {
                Avisar(oActualizado, EstadoRespondido, texto, oSesionBE);
            }

            return oActualizado;
        }

        public BETicketConDetalle CambiarEstado(BEFiltroTicket Objeto, BESesion oSesionBE)
        {
            string estado = ValidarEstado(Objeto.Estado, false);
            BETicketConDetalle oTicket = Obtener(Objeto.TicketId ?? 0);

            ValidarAcceso(oTicket, Objeto.Administracion, oSesionBE);

            if (oTicket.Ticket.Estado == estado)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "La consulta ya está en ese estado.");
            }

            if (!Objeto.Administracion && estado != EstadoCerrado)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.PermisoDenegado, "Solo podés cerrar tu consulta.");
            }

            if (Objeto.Administracion && estado != EstadoEnProceso && estado != EstadoCerrado)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Desde el Backoffice una consulta se toma (" + EstadoEnProceso + ") o se cierra (" + EstadoCerrado + ").");
            }

            if (!Objeto.Administracion && oTicket.Ticket.Estado == EstadoCerrado)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "La consulta ya está cerrada.");
            }

            BETicket oEstadoBE = new BETicket();
            oEstadoBE.TicketId = oTicket.Ticket.TicketId;
            oEstadoBE.Estado = estado;
            oEstadoBE.FechaCierre = estado == EstadoCerrado ? DateTime.Now : (DateTime?)null;

            oMPPTic.CambiarEstado(oEstadoBE);

            Registrar(
                oSesionBE.UsuarioId,
                "TicketEstado",
                "Consulta #" + oTicket.Ticket.TicketId + ": " + oTicket.Ticket.Estado + " → " + estado +
                (Objeto.Administracion ? " (operador)." : " (cliente)."),
                BLLBitacora.NivelInformativo);

            BEFiltroTicket oFiltroBE = new BEFiltroTicket();
            oFiltroBE.TicketId = oTicket.Ticket.TicketId;
            oFiltroBE.Administracion = Objeto.Administracion;

            BETicketConDetalle oActualizado = ListarObjeto(oFiltroBE, oSesionBE);

            if (Objeto.Administracion)
            {
                Avisar(oActualizado, estado, null, oSesionBE);
            }

            return oActualizado;
        }

        private BETicketConDetalle Obtener(int ticketId)
        {
            BETicket oFiltroBE = new BETicket();
            oFiltroBE.TicketId = ticketId;

            BETicketConDetalle oTicket = oMPPTic.ListarObjeto(oFiltroBE);

            if (oTicket == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La consulta no existe.");
            }

            return oTicket;
        }

        private void ValidarAcceso(BETicketConDetalle oTicket, bool administracion, BESesion oSesionBE)
        {
            if (administracion)
            {
                ExigirOperador(oSesionBE);

                return;
            }

            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            bool mismaEmpresa = oTicket.Ticket.EmpresaId == oUsuarioBE.EmpresaId;
            bool propio = oTicket.Ticket.UsuarioId == oUsuarioBE.UsuarioId;

            if (!mismaEmpresa || (!propio && !oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.TicketVerEmpresa)))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La consulta no existe.");
            }
        }

        private void ExigirOperador(BESesion oSesionBE)
        {
            if (!oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.TicketAtender))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.PermisoDenegado, "No tenés permiso para atender consultas.");
            }
        }

        private string ValidarEstado(string estado, bool opcional)
        {
            string limpio = Normalizar(estado);

            if (limpio == null && opcional)
            {
                return null;
            }

            if (limpio != null)
            {
                string normalizado = limpio.ToUpperInvariant();

                foreach (string valido in Estados)
                {
                    if (valido == normalizado)
                    {
                        return normalizado;
                    }
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "El estado tiene que ser " + string.Join(", ", Estados) + ".");
        }

        private string ValidarTexto(string texto)
        {
            string limpio = Normalizar(texto);

            if (limpio == null || limpio.Length > LongitudMaximaTexto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El mensaje es obligatorio y no puede superar los " + LongitudMaximaTexto + " caracteres.");
            }

            return limpio;
        }

        private void Avisar(BETicketConDetalle oTicket, string evento, string respuesta, BESesion oSesionBE)
        {
            try
            {
                Dictionary<string, string> textos = oBLLTra.ListarPorCodigo(oTicket.CodigoIdioma ?? "es");

                oServicioEmail.EnviarAviso(oTicket.AutorEmail, oTicket, evento, respuesta, textos);
            }
            catch (Exception ex)
            {
                ServicioLog.Error(
                    "Falló el aviso por email de la consulta #" + oTicket.Ticket.TicketId + " a " + oTicket.AutorEmail + ".", ex);

                Registrar(
                    oSesionBE.UsuarioId,
                    "Ticket:EmailNoEnviado",
                    "Consulta #" + oTicket.Ticket.TicketId + " (" + evento + "): " + ex.Message,
                    BLLBitacora.NivelError);
            }
        }

        private void Registrar(int usuarioId, string accion, string descripcion, string nivel)
        {
            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                usuarioId,
                DateTime.Now,
                BLLBitacora.ModuloSoporte,
                accion,
                descripcion,
                nivel));
        }

        private string Normalizar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
