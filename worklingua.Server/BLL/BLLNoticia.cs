using System.Net.Mail;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLNoticia
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaResumen = 300;
        private const int LongitudMaximaContenido = 20000;

        static readonly DateTime FechaMinimaPublicacion = new DateTime(2000, 1, 1);

        MPPNoticia oMPPNot = new MPPNoticia();
        BLLIdioma oBLLIdi = new BLLIdioma();

        public List<BENoticia> ListarPublicadas(BEFiltroNoticia Objeto)
        {
            BEIdioma oFiltroBE = new BEIdioma();
            oFiltroBE.CodigoISO = Objeto.Idioma;

            BEIdioma oIdiomaBE = oBLLIdi.ResolverPorCodigo(oFiltroBE);

            if (oIdiomaBE == null)
            {
                return new List<BENoticia>();
            }

            List<BENoticia> ListaNoticiaBE = oMPPNot.ListarPublicadas(oIdiomaBE);

            if (ListaNoticiaBE == null)
            {
                BEIdioma oBaseFiltroBE = new BEIdioma();
                oBaseFiltroBE.CodigoISO = BLLTraduccion.CodigoIdiomaBase;

                BEIdioma oBaseBE = oBLLIdi.ResolverPorCodigo(oBaseFiltroBE);

                if (oBaseBE != null && oBaseBE.IdiomaId != oIdiomaBE.IdiomaId)
                {
                    ListaNoticiaBE = oMPPNot.ListarPublicadas(oBaseBE);
                }
            }

            return ListaNoticiaBE == null ? new List<BENoticia>() : ListaNoticiaBE;
        }

        public BENoticia ListarObjetoPublicada(BENoticia Objeto)
        {
            BENoticia oNoticiaBE = oMPPNot.ListarObjetoPublicada(Objeto);

            if (oNoticiaBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado, "La novedad no existe o ya no está publicada.");
            }

            return oNoticiaBE;
        }

        public List<BENoticia> ListarAdministracion(BEFiltroNoticia Objeto)
        {
            BEFiltroNoticia oFiltro = new BEFiltroNoticia(
                null,
                Objeto.IdiomaId.HasValue && Objeto.IdiomaId.Value > 0 ? Objeto.IdiomaId : null);

            List<BENoticia> ListaNoticiaBE = oMPPNot.ListarAdministracion(oFiltro);

            return ListaNoticiaBE == null ? new List<BENoticia>() : ListaNoticiaBE;
        }

        public BENoticia ListarObjeto(BENoticia Objeto)
        {
            BENoticia oNoticiaBE = oMPPNot.ListarObjeto(Objeto);

            if (oNoticiaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La noticia no existe.");
            }

            return oNoticiaBE;
        }

        public BENoticia Guardar(BENoticia Objeto)
        {
            Validar(Objeto);

            if (Objeto.NoticiaId != 0)
            {
                ListarObjeto(Objeto);
            }

            Objeto.NoticiaId = oMPPNot.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public bool Baja(BENoticia Objeto)
        {
            BENoticia oNoticiaBE = ListarObjeto(Objeto);

            if (!oNoticiaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La noticia ya está dada de baja.");
            }

            return oMPPNot.Baja(Objeto);
        }

        public bool EstaPublicada(BENoticia Objeto)
        {
            return Objeto.Activo && Objeto.FechaPublicacion <= DateTime.Now;
        }

        private void Validar(BENoticia Objeto)
        {
            Objeto.Titulo = ServicioTexto.Normalizar(Objeto.Titulo);
            Objeto.Resumen = ServicioTexto.Normalizar(Objeto.Resumen);
            Objeto.Contenido = ServicioTexto.Normalizar(Objeto.Contenido);
            Objeto.Sector = Sectores.Resolver(Objeto.Sector);

            ReglasNegocio.ExigirTexto(Objeto.Titulo, "El título de la noticia es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Titulo, LongitudMaximaTitulo,
                "El título no puede superar los " + LongitudMaximaTitulo + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Resumen, LongitudMaximaResumen,
                "El resumen no puede superar los " + LongitudMaximaResumen + " caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Contenido, "El contenido de la noticia es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Contenido, LongitudMaximaContenido,
                "El contenido no puede superar los " + LongitudMaximaContenido + " caracteres.");

            if (Objeto.FechaPublicacion == default(DateTime))
            {
                Objeto.FechaPublicacion = DateTime.Now;
            }

            if (Objeto.FechaPublicacion < FechaMinimaPublicacion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La fecha de publicación no es válida.");
            }

            ValidarIdioma(Objeto);
        }

        private void ValidarIdioma(BENoticia Objeto)
        {
            foreach (BEIdioma oIdiomaBE in oBLLIdi.ListarTodoConBajas())
            {
                if (oIdiomaBE.IdiomaId == Objeto.IdiomaId)
                {
                    return;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "El idioma elegido para la noticia no existe.");
        }

        public void Validar(BEFiltroNoticia Objeto)
        {
            ReglasNegocio.ExigirLargoMaximo(Objeto.Idioma, 5, "El código de idioma no puede superar los 5 caracteres.");
        }

        public void Validar(BEGuardarNoticia Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.IdiomaId, 1, int.MaxValue, "Elegí el idioma de la noticia.");

            ReglasNegocio.ExigirTexto(Objeto.Titulo, "El título de la noticia es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Titulo, 150, "El título no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Resumen, 300, "El resumen no puede superar los 300 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Contenido, "El contenido de la noticia es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Contenido, 20000,
                "El contenido no puede superar los 20000 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Sector, 30, "El sector no puede superar los 30 caracteres.");
        }
    }

    public class BLLSuscriptorNewsletter
    {
        private const int LongitudMaximaEmail = 150;
        private const int MinutosEntreConfirmaciones = 10;

        MPPSuscriptorNewsletter oMPPSus = new MPPSuscriptorNewsletter();
        BLLIdioma oBLLIdi = new BLLIdioma();
        BLLTraduccion oBLLTra = new BLLTraduccion();
        BLLBitacora oBLLBit = new BLLBitacora();
        ServicioEmailNewsletter oServicioEmail = new ServicioEmailNewsletter();

        public List<BESuscriptorNewsletter> ListarTodo()
        {
            List<BESuscriptorNewsletter> ListaSuscriptorBE = oMPPSus.ListarTodo();

            return ListaSuscriptorBE == null ? new List<BESuscriptorNewsletter>() : ListaSuscriptorBE;
        }

        public List<BESuscriptorNewsletter> ListarConfirmados(BEIdioma Objeto, List<string> Sectores)
        {
            List<BESuscriptorNewsletter> ListaSuscriptorBE = oMPPSus.ListarConfirmados(Objeto, Sectores);

            return ListaSuscriptorBE == null ? new List<BESuscriptorNewsletter>() : ListaSuscriptorBE;
        }

        public void Guardar(BESuscribirNewsletter Objeto)
        {
            string email = ValidarEmail(Objeto.Email);
            List<string> ListaSector = Sectores.ResolverLista(Objeto.Sectores);

            BEIdioma oFiltroIdiomaBE = new BEIdioma();
            oFiltroIdiomaBE.CodigoISO = Objeto.Idioma;

            BEIdioma oIdiomaBE = oBLLIdi.ResolverPorCodigo(oFiltroIdiomaBE);

            if (oIdiomaBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "No hay idiomas disponibles para la suscripción.");
            }

            BESuscriptorNewsletter oFiltroBE = new BESuscriptorNewsletter();
            oFiltroBE.Email = email;

            BESuscriptorNewsletter oExistenteBE = oMPPSus.ListarPorEmail(oFiltroBE);

            if (oExistenteBE != null)
            {
                oExistenteBE.Sectores = ListaSector;
                oMPPSus.ReemplazarSectores(oExistenteBE);
            }

            if (oExistenteBE != null && !DebeEnviarConfirmacion(oExistenteBE))
            {
                return;
            }

            BESuscriptorNewsletter oSuscriptorBE = new BESuscriptorNewsletter();
            oSuscriptorBE.SuscriptorId = oExistenteBE == null ? 0 : oExistenteBE.SuscriptorId;
            oSuscriptorBE.Email = email;
            oSuscriptorBE.IdiomaId = oIdiomaBE.IdiomaId;

            oSuscriptorBE.SuscriptorId = oMPPSus.Guardar(oSuscriptorBE);
            oSuscriptorBE.Sectores = ListaSector;

            oMPPSus.ReemplazarSectores(oSuscriptorBE);

            EnviarConfirmacion(oMPPSus.ListarObjeto(oSuscriptorBE), oIdiomaBE);
        }

        public void Confirmar(BESuscriptorNewsletter Objeto)
        {
            BESuscriptorNewsletter oSuscriptorBE = ListarPorToken(Objeto);

            if (!oSuscriptorBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La suscripción fue dada de baja. Si querés volver a recibir el newsletter, suscribite de nuevo.");
            }

            if (oSuscriptorBE.Confirmado)
            {
                return;
            }

            if (oSuscriptorBE.FechaAlta.AddHours(Configuracion.HorasVigenciaConfirmacion) <= DateTime.Now)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El enlace venció. Suscribite de nuevo para recibir uno nuevo.");
            }

            oMPPSus.Confirmar(oSuscriptorBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, null, DateTime.Now, BLLBitacora.ModuloNewsletter, "SuscripcionConfirmada",
                "Se confirmó la suscripción de " + oSuscriptorBE.Email + ".", BLLBitacora.NivelInformativo));
        }

        public void Baja(BESuscriptorNewsletter Objeto)
        {
            BESuscriptorNewsletter oSuscriptorBE = ListarPorToken(Objeto);

            if (!oSuscriptorBE.Activo)
            {
                return;
            }

            oMPPSus.Baja(oSuscriptorBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, null, DateTime.Now, BLLBitacora.ModuloNewsletter, "SuscripcionBaja",
                "Se dio de baja la suscripción de " + oSuscriptorBE.Email + ".", BLLBitacora.NivelInformativo));
        }

        private bool DebeEnviarConfirmacion(BESuscriptorNewsletter oExistenteBE)
        {
            if (!oExistenteBE.Activo)
            {
                return true;
            }

            if (oExistenteBE.Confirmado)
            {
                return false;
            }

            return oExistenteBE.FechaAlta.AddMinutes(MinutosEntreConfirmaciones) <= DateTime.Now;
        }

        private void EnviarConfirmacion(BESuscriptorNewsletter oSuscriptorBE, BEIdioma oIdiomaBE)
        {
            try
            {
                oServicioEmail.EnviarConfirmacion(oSuscriptorBE, oBLLTra.ListarPorCodigo(oIdiomaBE.CodigoISO));
            }
            catch (Exception ex)
            {
                ServicioLog.Error(
                    "No se pudo enviar la confirmación del newsletter a " + oSuscriptorBE.Email + ".", ex);

                oBLLBit.Guardar(new BEBitacoraEvento(
                    0, null, DateTime.Now, BLLBitacora.ModuloNewsletter, "ConfirmacionNoEnviada", ex.Message,
                    BLLBitacora.NivelError));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "No pudimos enviarte el correo de confirmación. Intentá nuevamente en unos minutos.");
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, null, DateTime.Now, BLLBitacora.ModuloNewsletter, "SuscripcionSolicitada",
                "Solicitud de suscripción de " + oSuscriptorBE.Email + " en idioma " + oIdiomaBE.CodigoISO + ".",
                BLLBitacora.NivelInformativo));
        }

        private BESuscriptorNewsletter ListarPorToken(BESuscriptorNewsletter Objeto)
        {
            BESuscriptorNewsletter oFiltroBE = new BESuscriptorNewsletter();
            oFiltroBE.Token = Objeto.Token;

            BESuscriptorNewsletter oSuscriptorBE = Objeto.Token == Guid.Empty ? null : oMPPSus.ListarPorToken(oFiltroBE);

            if (oSuscriptorBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El enlace no es válido o ya no está vigente.");
            }

            return oSuscriptorBE;
        }

        private string ValidarEmail(string email)
        {
            string limpio = email == null ? string.Empty : email.Trim().ToLowerInvariant();
            MailAddress direccion;

            if (limpio.Length == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Ingresá tu correo.");
            }

            ReglasNegocio.ExigirLargoMaximo(limpio, LongitudMaximaEmail,
                "El correo no puede superar los " + LongitudMaximaEmail + " caracteres.");

            if (!MailAddress.TryCreate(limpio, out direccion) || direccion.Address != limpio)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El correo no tiene un formato válido.");
            }

            return limpio;
        }

        public void Validar(BESuscribirNewsletter Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Email, "Ingresá tu correo.");
            ReglasNegocio.ExigirEmail(Objeto.Email, "El correo no tiene un formato válido.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Email, 150, "El correo no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Idioma, 5, "El código de idioma no puede superar los 5 caracteres.");
        }
    }

    public class BLLEnvioNewsletter
    {
        private const int LongitudMaximaAsunto = 200;
        private const int MaximoNoticias = 10;
        private const string FormatoFechaPorDefecto = "dd/MM/yyyy";

        MPPEnvioNewsletter oMPPEnv = new MPPEnvioNewsletter();
        BLLSuscriptorNewsletter oBLLSus = new BLLSuscriptorNewsletter();
        BLLNoticia oBLLNot = new BLLNoticia();
        BLLIdioma oBLLIdi = new BLLIdioma();
        BLLCultura oBLLCul = new BLLCultura();
        BLLTraduccion oBLLTra = new BLLTraduccion();
        BLLBitacora oBLLBit = new BLLBitacora();
        ServicioEmailNewsletter oServicioEmail = new ServicioEmailNewsletter();

        public List<BEEnvioNewsletter> ListarTodo()
        {
            List<BEEnvioNewsletter> ListaEnvioBE = oMPPEnv.ListarTodo();

            return ListaEnvioBE == null ? new List<BEEnvioNewsletter>() : ListaEnvioBE;
        }

        public BEVistaPreviaNewsletterRespuesta Previsualizar(BEEnviarNewsletter Objeto)
        {
            BEIdioma oIdiomaBE = ObtenerIdioma(Objeto);

            ValidarAsunto(Objeto);

            List<BENoticia> ListaNoticiaBE = ObtenerNoticias(Objeto, oIdiomaBE);
            List<string> ListaSector = SectoresDelEnvio(ListaNoticiaBE);

            string html = oServicioEmail.RenderizarNewsletter(
                Guid.Empty, ListaNoticiaBE, oBLLTra.ListarPorCodigo(oIdiomaBE.CodigoISO), FormatoFecha(oIdiomaBE));

            return new BEVistaPreviaNewsletterRespuesta(
                html, oBLLSus.ListarConfirmados(oIdiomaBE, ListaSector).Count, ListaSector);
        }

        public BEEnvioNewsletter Guardar(BEEnviarNewsletter Objeto, BESesion oSesionBE)
        {
            if (!Configuracion.SmtpConfigurado)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El envío de correos no está configurado en el servidor.");
            }

            BEIdioma oIdiomaBE = ObtenerIdioma(Objeto);
            string asunto = ValidarAsunto(Objeto);
            List<BENoticia> ListaNoticiaBE = ObtenerNoticias(Objeto, oIdiomaBE);
            List<BESuscriptorNewsletter> ListaSuscriptorBE =
                oBLLSus.ListarConfirmados(oIdiomaBE, SectoresDelEnvio(ListaNoticiaBE));

            if (ListaSuscriptorBE.Count == 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "No hay suscriptores confirmados para el idioma y los intereses de este envío.");
            }

            Dictionary<string, string> textos = oBLLTra.ListarPorCodigo(oIdiomaBE.CodigoISO);
            string formatoFecha = FormatoFecha(oIdiomaBE);
            int enviados = 0;
            int fallidos = 0;

            foreach (BESuscriptorNewsletter oSuscriptorBE in ListaSuscriptorBE)
            {
                try
                {
                    oServicioEmail.EnviarNewsletter(oSuscriptorBE, asunto, ListaNoticiaBE, textos, formatoFecha);
                    enviados = enviados + 1;
                }
                catch (Exception ex)
                {
                    ServicioLog.Error("No se pudo enviar el newsletter a " + oSuscriptorBE.Email + ".", ex);
                    fallidos = fallidos + 1;
                }
            }

            BEEnvioNewsletter oEnvioBE = new BEEnvioNewsletter(
                0, oIdiomaBE.IdiomaId, oSesionBE.UsuarioId, asunto, DateTime.Now, enviados, fallidos);

            oEnvioBE.EnvioId = oMPPEnv.Guardar(oEnvioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloNewsletter,
                "Envio",
                "Envío del newsletter '" + asunto + "' en idioma " + oIdiomaBE.CodigoISO + " con " +
                    ListaNoticiaBE.Count + " noticias: " + enviados + " enviados y " + fallidos + " fallidos.",
                fallidos > 0 ? BLLBitacora.NivelAdvertencia : BLLBitacora.NivelInformativo));

            return oEnvioBE;
        }

        private List<string> SectoresDelEnvio(List<BENoticia> ListaNoticiaBE)
        {
            List<string> ListaSector = new List<string>();

            foreach (BENoticia oNoticiaBE in ListaNoticiaBE)
            {
                if (string.IsNullOrWhiteSpace(oNoticiaBE.Sector))
                {
                    return new List<string>();
                }

                if (!ListaSector.Contains(oNoticiaBE.Sector))
                {
                    ListaSector.Add(oNoticiaBE.Sector);
                }
            }

            return ListaSector;
        }

        private BEIdioma ObtenerIdioma(BEEnviarNewsletter Objeto)
        {
            foreach (BEIdioma oIdiomaBE in oBLLIdi.ListarTodoConBajas())
            {
                if (oIdiomaBE.IdiomaId == Objeto.IdiomaId)
                {
                    return oIdiomaBE;
                }
            }

            throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El idioma del envío no existe.");
        }

        private string ValidarAsunto(BEEnviarNewsletter Objeto)
        {
            string asunto = Objeto.Asunto == null
                ? string.Empty
                : Objeto.Asunto.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (asunto.Length == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El asunto es obligatorio.");
            }

            ReglasNegocio.ExigirLargoMaximo(asunto, LongitudMaximaAsunto,
                "El asunto no puede superar los " + LongitudMaximaAsunto + " caracteres.");

            return asunto;
        }

        private List<BENoticia> ObtenerNoticias(BEEnviarNewsletter Objeto, BEIdioma oIdiomaBE)
        {
            List<int> ListaId = new List<int>();

            if (Objeto.NoticiaIds != null)
            {
                foreach (int noticiaId in Objeto.NoticiaIds)
                {
                    if (!ListaId.Contains(noticiaId))
                    {
                        ListaId.Add(noticiaId);
                    }
                }
            }

            if (ListaId.Count == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Elegí al menos una noticia.");
            }

            if (ListaId.Count > MaximoNoticias)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Un envío puede incluir hasta " + MaximoNoticias + " noticias.");
            }

            List<BENoticia> ListaNoticiaBE = new List<BENoticia>();

            foreach (int noticiaId in ListaId)
            {
                BENoticia oFiltroBE = new BENoticia();
                oFiltroBE.NoticiaId = noticiaId;

                BENoticia oNoticiaBE = oBLLNot.ListarObjeto(oFiltroBE);

                if (!oBLLNot.EstaPublicada(oNoticiaBE))
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "La noticia '" + oNoticiaBE.Titulo + "' no está publicada.");
                }

                if (oNoticiaBE.IdiomaId != oIdiomaBE.IdiomaId)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "La noticia '" + oNoticiaBE.Titulo + "' no está escrita en el idioma del envío.");
                }

                ListaNoticiaBE.Add(oNoticiaBE);
            }

            ListaNoticiaBE.Sort((primera, segunda) => segunda.FechaPublicacion.CompareTo(primera.FechaPublicacion));

            return ListaNoticiaBE;
        }

        private string FormatoFecha(BEIdioma oIdiomaBE)
        {
            string formato = null;

            foreach (BECulturaConIdioma oCulturaBE in oBLLCul.ListarTodo())
            {
                bool mismoIdioma = string.Equals(
                    oCulturaBE.CodigoIdioma == null ? null : oCulturaBE.CodigoIdioma.Trim(),
                    oIdiomaBE.CodigoISO,
                    StringComparison.OrdinalIgnoreCase);

                if (!mismoIdioma || string.IsNullOrWhiteSpace(oCulturaBE.Cultura.FormatoFecha))
                {
                    continue;
                }

                if (oCulturaBE.Cultura.EsPredeterminada || formato == null)
                {
                    formato = oCulturaBE.Cultura.FormatoFecha;
                }
            }

            return formato == null ? FormatoFechaPorDefecto : formato;
        }

        public void Validar(BEEnviarNewsletter Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.IdiomaId, 1, int.MaxValue, "Elegí el idioma del envío.");

            ReglasNegocio.ExigirTexto(Objeto.Asunto, "El asunto es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Asunto, 200, "El asunto no puede superar los 200 caracteres.");

            ReglasNegocio.ExigirValor(Objeto.NoticiaIds, "Elegí al menos una noticia.");
        }
    }
}
