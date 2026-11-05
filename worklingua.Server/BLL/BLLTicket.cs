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

        MPPTicket oMPPTic = new MPPTicket();
        BLLUsuario oBLLUsu = new BLLUsuario();
        BLLSeguridad oBLLSeg = new BLLSeguridad();
        BLLSuscripcion oBLLSus = new BLLSuscripcion();
        BLLCurso oBLLCur = new BLLCurso();
        BLLIdioma oBLLIdi = new BLLIdioma();
        BLLTraduccion oBLLTra = new BLLTraduccion();
        BLLBitacora oBLLBit = new BLLBitacora();
        ServicioEmailSoporte oServicioEmail = new ServicioEmailSoporte();

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
            string asunto = ValidarAsunto(Objeto.Asunto);
            string texto = ValidarTexto(Objeto.Texto);

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
                oIdiomaBE == null ? (int?)null : oIdiomaBE.IdiomaId,
                null,
                null);

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

        public BETicket GuardarConsultaPublica(BEEnviarConsultaContacto Objeto)
        {
            string asunto = ValidarAsunto(Objeto.Asunto);
            string texto = ValidarTexto(Objeto.Mensaje);

            BEIdioma oFiltroIdiomaBE = new BEIdioma();
            oFiltroIdiomaBE.CodigoISO = Objeto.CodigoIdioma;

            BEIdioma oIdiomaBE = oBLLIdi.ResolverPorCodigo(oFiltroIdiomaBE);
            DateTime ahora = DateTime.Now;

            BETicket oTicketBE = new BETicket(
                0,
                null,
                null,
                null,
                null,
                asunto,
                EstadoPendiente,
                ahora,
                ahora,
                null,
                oIdiomaBE == null ? (int?)null : oIdiomaBE.IdiomaId,
                Objeto.Nombre,
                Objeto.Email);

            BEMensajeTicket oMensajeBE = new BEMensajeTicket();
            oMensajeBE.Texto = texto;

            BETicket oCreadoBE = oMPPTic.GuardarConsultaPublica(oTicketBE, oMensajeBE);

            if (oCreadoBE == null || oCreadoBE.Token == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "No pudimos registrar tu consulta. Intentá nuevamente en unos minutos.");
            }

            Registrar(
                null,
                "ConsultaPublicaCreada",
                "Consulta pública #" + oCreadoBE.TicketId + " (" + asunto + ") de " + Objeto.Email + ".",
                BLLBitacora.NivelInformativo);

            return oCreadoBE;
        }

        public BETicketConDetalle ListarObjetoPorToken(BEFiltroTicket Objeto)
        {
            if (Objeto.Token == null || Objeto.Token == Guid.Empty)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Falta el token del enlace.");
            }

            BETicket oFiltroBE = new BETicket();
            oFiltroBE.Token = Objeto.Token;

            BETicketConDetalle oTicket = oMPPTic.ListarObjeto(oFiltroBE);

            if (oTicket == null || !EsPublica(oTicket.Ticket))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La consulta no existe.");
            }

            List<BEMensajeTicketConAutor> ListaMensajeBE = oMPPTic.ListarMensajes(oTicket.Ticket);
            oTicket.Mensajes = ListaMensajeBE == null ? new List<BEMensajeTicketConAutor>() : ListaMensajeBE;

            return oTicket;
        }

        public BETicketConDetalle ResponderVisitante(BEResponderConsultaPublica Objeto)
        {
            string texto = ValidarTexto(Objeto.Texto);

            BEFiltroTicket oFiltroBE = new BEFiltroTicket();
            oFiltroBE.Token = Objeto.Token;

            BETicketConDetalle oTicket = ListarObjetoPorToken(oFiltroBE);

            if (oTicket.Ticket.Estado == EstadoCerrado)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La consulta está cerrada. Si necesitás más ayuda, escribinos de nuevo.");
            }

            BEMensajeTicket oMensajeBE = new BEMensajeTicket();
            oMensajeBE.TicketId = oTicket.Ticket.TicketId;
            oMensajeBE.Texto = texto;
            oMensajeBE.EsOperador = false;
            oMensajeBE.UsuarioId = null;

            BETicket oEstadoBE = new BETicket();
            oEstadoBE.TicketId = oTicket.Ticket.TicketId;
            oEstadoBE.Estado = EstadoPendiente;

            oMPPTic.GuardarMensaje(oMensajeBE, oEstadoBE);

            Registrar(
                null,
                "ConsultaPublicaMensaje",
                "Mensaje del visitante en la consulta pública #" + oTicket.Ticket.TicketId + ".",
                BLLBitacora.NivelInformativo);

            return ListarObjetoPorToken(oFiltroBE);
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

        private bool EsPublica(BETicket oTicketBE)
        {
            return oTicketBE.UsuarioId == null;
        }

        private void ValidarAcceso(BETicketConDetalle oTicket, bool administracion, BESesion oSesionBE)
        {
            if (administracion)
            {
                ExigirOperador(oSesionBE);

                return;
            }

            if (EsPublica(oTicket.Ticket))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La consulta no existe.");
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
            string limpio = ServicioTexto.Normalizar(estado);

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

        private string ValidarAsunto(string asunto)
        {
            string limpio = ServicioTexto.Normalizar(asunto);

            if (limpio == null || limpio.Length < LongitudMinimaAsunto || limpio.Length > LongitudMaximaAsunto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El asunto tiene que tener entre " + LongitudMinimaAsunto + " y " + LongitudMaximaAsunto + " caracteres.");
            }

            return limpio;
        }

        private string ValidarTexto(string texto)
        {
            string limpio = ServicioTexto.Normalizar(texto);

            ReglasNegocio.ExigirConLargoMaximo(limpio, LongitudMaximaTexto,
                "El mensaje es obligatorio y no puede superar los " + LongitudMaximaTexto + " caracteres.");

            return limpio;
        }

        private void Avisar(BETicketConDetalle oTicket, string evento, string respuesta, BESesion oSesionBE)
        {
            if (EsPublica(oTicket.Ticket))
            {
                return;
            }

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

        private void Registrar(int? usuarioId, string accion, string descripcion, string nivel)
        {
            oBLLBit.Guardar(new BEBitacoraEvento(
                0, usuarioId, DateTime.Now, BLLBitacora.ModuloSoporte, accion, descripcion, nivel));
        }

        public void Validar(BECrearTicket Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.SuscripcionId, 1, int.MaxValue,
                "Elegí la contratación sobre la que consultás.");

            ReglasNegocio.ExigirTexto(Objeto.Asunto, "El asunto es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Asunto, 150, "El asunto no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Texto, "Escribí tu consulta.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Texto, 2000, "La consulta no puede superar los 2000 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.CodigoIdioma, 5, "El código de idioma no es válido.");
        }

        public void Validar(BEEnviarMensajeTicket Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Texto, "Escribí el mensaje.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Texto, 2000, "El mensaje no puede superar los 2000 caracteres.");
        }

        public void Validar(BETicket Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Estado, "Falta el estado.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Estado, 20, "El estado no es válido.");
        }
    }

    public class BLLOferta
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaDescripcion = 1000;

        MPPOferta oMPPOfe = new MPPOferta();
        BLLEmpresa oBLLEmp = new BLLEmpresa();
        BLLPlan oBLLPlan = new BLLPlan();
        BLLUsuario oBLLUsu = new BLLUsuario();
        BLLBitacora oBLLBit = new BLLBitacora();

        public List<BEOfertaConDetalle> ListarTodo()
        {
            List<BEOfertaConDetalle> ListaOfertaBE = oMPPOfe.Listar(new BEFiltroOferta());

            return ListaOfertaBE == null ? new List<BEOfertaConDetalle>() : ListaOfertaBE;
        }

        public List<BEOfertaConDetalle> ListarVigentes(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BEFiltroOferta oFiltroBE = new BEFiltroOferta(
                null, oUsuarioBE.EmpresaId, DateOnly.FromDateTime(DateTime.Now));

            List<BEOfertaConDetalle> ListaOfertaBE = oMPPOfe.Listar(oFiltroBE);

            return ListaOfertaBE == null ? new List<BEOfertaConDetalle>() : ListaOfertaBE;
        }

        private BEOfertaConDetalle ListarObjeto(BEOferta Objeto)
        {
            List<BEOfertaConDetalle> ListaOfertaBE = oMPPOfe.Listar(new BEFiltroOferta(Objeto.OfertaId, null, null));

            if (ListaOfertaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La oferta no existe.");
            }

            return ListaOfertaBE[0];
        }

        public BEOfertaConDetalle Guardar(BEOferta Objeto, BESesion oSesionBE)
        {
            Validar(Objeto);

            bool esAlta = Objeto.OfertaId == 0;

            if (!esAlta)
            {
                ListarObjeto(Objeto);
            }

            Objeto.UsuarioId = oSesionBE.UsuarioId;
            Objeto.OfertaId = oMPPOfe.Guardar(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloOferta, esAlta ? "Alta" : "Modificacion",
                "Oferta " + Objeto.OfertaId + " (" + Objeto.Titulo + ") para la empresa " + Objeto.EmpresaId + ".",
                BLLBitacora.NivelInformativo));

            return ListarObjeto(Objeto);
        }

        public bool Baja(BEOferta Objeto, BESesion oSesionBE)
        {
            BEOfertaConDetalle oOferta = ListarObjeto(Objeto);

            if (!oOferta.Oferta.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "La oferta ya está dada de baja.");
            }

            bool resultado = oMPPOfe.Baja(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloOferta, "Baja",
                "Baja de la oferta " + Objeto.OfertaId + " (" + oOferta.Oferta.Titulo + ").",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        private void Validar(BEOferta Objeto)
        {
            Objeto.Titulo = ServicioTexto.Normalizar(Objeto.Titulo);
            Objeto.Descripcion = ServicioTexto.Normalizar(Objeto.Descripcion);

            ReglasNegocio.ExigirConLargoMaximo(Objeto.Titulo, LongitudMaximaTitulo,
                "El título es obligatorio y no puede superar los " + LongitudMaximaTitulo + " caracteres.");
            ReglasNegocio.ExigirConLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción es obligatoria y no puede superar los " + LongitudMaximaDescripcion + " caracteres.");

            if (Objeto.FechaHasta.HasValue && Objeto.FechaHasta.Value < Objeto.FechaDesde)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La fecha de fin no puede ser anterior a la de inicio.");
            }

            BEEmpresa oFiltroEmpresaBE = new BEEmpresa();
            oFiltroEmpresaBE.EmpresaId = Objeto.EmpresaId;

            BEEmpresa oEmpresaBE = oBLLEmp.ListarObjeto(oFiltroEmpresaBE);

            if (oEmpresaBE.Protegido || !oEmpresaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Las ofertas se dirigen a empresas clientes activas.");
            }

            if (Objeto.PlanId.HasValue)
            {
                BEPlanSuscripcion oFiltroPlanBE = new BEPlanSuscripcion();
                oFiltroPlanBE.PlanId = Objeto.PlanId.Value;

                if (!oBLLPlan.ListarObjeto(oFiltroPlanBE).Activo)
                {
                    throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El plan sugerido está dado de baja.");
                }
            }
        }

        public void Validar(BEGuardarOferta Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.EmpresaId, 1, int.MaxValue, "Elegí la empresa destinataria.");

            ReglasNegocio.ExigirTexto(Objeto.Titulo, "El título es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Titulo, 150, "El título no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Descripcion, "La descripción es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 1000,
                "La descripción no puede superar los 1000 caracteres.");

            ReglasNegocio.ExigirValor(Objeto.FechaDesde, "La fecha de inicio es obligatoria.");
        }
    }
}
