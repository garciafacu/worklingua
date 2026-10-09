using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLEnvioNewsletter
    {
        private const int LongitudMaximaAsunto = 200;
        private const int MaximoNoticias = 10;
        private const string FormatoFechaPorDefecto = "dd/MM/yyyy";

        MPPEnvioNewsletter oMPPEnv;
        BLLSuscriptorNewsletter oBLLSus;
        BLLNoticia oBLLNot;
        BLLIdioma oBLLIdi;
        BLLCultura oBLLCul;
        BLLTraduccion oBLLTra;
        BLLBitacora oBLLBit;
        ServicioEmailNewsletter oServicioEmail;

        public BLLEnvioNewsletter()
        {
            oMPPEnv = new MPPEnvioNewsletter();
            oBLLSus = new BLLSuscriptorNewsletter();
            oBLLNot = new BLLNoticia();
            oBLLIdi = new BLLIdioma();
            oBLLCul = new BLLCultura();
            oBLLTra = new BLLTraduccion();
            oBLLBit = new BLLBitacora();
            oServicioEmail = new ServicioEmailNewsletter();
        }

        public List<BEEnvioNewsletter> ListarTodo()
        {
            List<BEEnvioNewsletter> ListaEnvioBE = oMPPEnv.ListarTodo();

            return ListaEnvioBE == null ? new List<BEEnvioNewsletter>() : ListaEnvioBE;
        }

        public string Previsualizar(BEEnviarNewsletter Objeto)
        {
            BEIdioma oIdiomaBE = ObtenerIdioma(Objeto);

            ValidarAsunto(Objeto);

            List<BENoticia> ListaNoticiaBE = ObtenerNoticias(Objeto, oIdiomaBE);

            return oServicioEmail.RenderizarNewsletter(
                Guid.Empty, ListaNoticiaBE, oBLLTra.ListarPorCodigo(oIdiomaBE.CodigoISO), FormatoFecha(oIdiomaBE));
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
            List<BESuscriptorNewsletter> ListaSuscriptorBE = oBLLSus.ListarConfirmados(oIdiomaBE);

            if (ListaSuscriptorBE.Count == 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "No hay suscriptores confirmados en el idioma del envío.");
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

            if (asunto.Length > LongitudMaximaAsunto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El asunto no puede superar los " + LongitudMaximaAsunto + " caracteres.");
            }

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
    }
}
