using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLFaq
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaPregunta = 300;
        private const int LongitudMaximaRespuesta = 2000;
        private const int LongitudMaximaEnlace = 200;
        private const int LongitudMaximaEtiqueta = 100;
        private const int OrdenMaximo = 1000;

        MPPFaq oMPPFaq = new MPPFaq();
        BLLIdioma oBLLIdi = new BLLIdioma();
        BLLBitacora oBLLBit = new BLLBitacora();

        public List<BECategoriaFaqConPreguntas> ListarPublicadas(BEFiltroFaq Objeto)
        {
            BEIdioma oIdiomaBE = ResolverIdioma(Objeto.Idioma);

            if (oIdiomaBE == null)
            {
                return new List<BECategoriaFaqConPreguntas>();
            }

            List<BECategoriaFaqConPreguntas> ListaBE = Armar(oIdiomaBE.IdiomaId);

            if (ListaBE.Count > 0)
            {
                return ListaBE;
            }

            BEIdioma oBaseBE = ResolverIdioma(BLLTraduccion.CodigoIdiomaBase);

            if (oBaseBE == null || oBaseBE.IdiomaId == oIdiomaBE.IdiomaId)
            {
                return ListaBE;
            }

            return Armar(oBaseBE.IdiomaId);
        }

        private List<BECategoriaFaqConPreguntas> Armar(int idiomaId)
        {
            List<BECategoriaFaq> ListaCategoriaBE =
                oMPPFaq.ListarCategorias(new BEFiltroFaq(null, null, idiomaId, true));

            List<BEPreguntaFrecuente> ListaPreguntaBE =
                oMPPFaq.ListarPreguntas(new BEFiltroFaq(null, null, idiomaId, true));

            List<BECategoriaFaqConPreguntas> ListaBE = new List<BECategoriaFaqConPreguntas>();

            foreach (BECategoriaFaq oCategoriaBE in ListaCategoriaBE)
            {
                BECategoriaFaqConPreguntas oItemBE = new BECategoriaFaqConPreguntas();
                oItemBE.Categoria = oCategoriaBE;

                foreach (BEPreguntaFrecuente oPreguntaBE in ListaPreguntaBE)
                {
                    if (oPreguntaBE.CategoriaFaqId == oCategoriaBE.CategoriaFaqId)
                    {
                        oItemBE.Preguntas.Add(oPreguntaBE);
                    }
                }

                if (oItemBE.Preguntas.Count > 0)
                {
                    ListaBE.Add(oItemBE);
                }
            }

            return ListaBE;
        }

        public List<BECategoriaFaq> ListarCategorias()
        {
            return oMPPFaq.ListarCategorias(new BEFiltroFaq());
        }

        public List<BEPreguntaFrecuente> ListarPreguntas(BEFiltroFaq Objeto)
        {
            return oMPPFaq.ListarPreguntas(Objeto);
        }

        private BECategoriaFaq ObtenerCategoria(int categoriaFaqId)
        {
            List<BECategoriaFaq> ListaBE =
                oMPPFaq.ListarCategorias(new BEFiltroFaq(categoriaFaqId, null, null, false));

            if (ListaBE.Count == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La categoría no existe.");
            }

            return ListaBE[0];
        }

        private BEPreguntaFrecuente ObtenerPregunta(int preguntaFrecuenteId)
        {
            List<BEPreguntaFrecuente> ListaBE =
                oMPPFaq.ListarPreguntas(new BEFiltroFaq(null, preguntaFrecuenteId, null, false));

            if (ListaBE.Count == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La pregunta no existe.");
            }

            return ListaBE[0];
        }

        public BECategoriaFaq GuardarCategoria(BECategoriaFaq Objeto, BESesion oSesionBE)
        {
            bool esAlta = Objeto.CategoriaFaqId == 0;

            Objeto.Titulo = ServicioTexto.Normalizar(Objeto.Titulo);

            ReglasNegocio.ExigirConLargoMaximo(Objeto.Titulo, LongitudMaximaTitulo,
                "El título es obligatorio y no puede superar los " + LongitudMaximaTitulo + " caracteres.");

            ValidarOrden(Objeto.Orden);
            ValidarIdioma(Objeto.IdiomaId);

            if (!esAlta)
            {
                ObtenerCategoria(Objeto.CategoriaFaqId);
            }

            Objeto.CategoriaFaqId = oMPPFaq.GuardarCategoria(Objeto);

            Registrar(oSesionBE, esAlta ? "CategoriaAlta" : "CategoriaModificacion",
                "Categoría " + Objeto.CategoriaFaqId + " (" + Objeto.Titulo + ").");

            return ObtenerCategoria(Objeto.CategoriaFaqId);
        }

        public bool BajaCategoria(BECategoriaFaq Objeto, BESesion oSesionBE)
        {
            BECategoriaFaq oCategoriaBE = ObtenerCategoria(Objeto.CategoriaFaqId);

            if (!oCategoriaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La categoría ya está dada de baja.");
            }

            bool resultado = oMPPFaq.BajaCategoria(Objeto);

            Registrar(oSesionBE, "CategoriaBaja",
                "Baja de la categoría " + Objeto.CategoriaFaqId + " (" + oCategoriaBE.Titulo + ").");

            return resultado;
        }

        public BECategoriaFaq CambiarEstadoCategoria(BECategoriaFaq Objeto, BESesion oSesionBE)
        {
            BECategoriaFaq oCategoriaBE = ObtenerCategoria(Objeto.CategoriaFaqId);

            if (oCategoriaBE.Activo == Objeto.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    Objeto.Activo ? "La categoría ya está vigente." : "La categoría ya está dada de baja.");
            }

            oMPPFaq.CambiarEstadoCategoria(Objeto);

            Registrar(oSesionBE, Objeto.Activo ? "CategoriaReactivacion" : "CategoriaBaja",
                "La categoría " + Objeto.CategoriaFaqId + " quedó " +
                (Objeto.Activo ? "vigente." : "dada de baja."));

            return ObtenerCategoria(Objeto.CategoriaFaqId);
        }

        public BEPreguntaFrecuente GuardarPregunta(BEPreguntaFrecuente Objeto, BESesion oSesionBE)
        {
            bool esAlta = Objeto.PreguntaFrecuenteId == 0;

            Objeto.Pregunta = ServicioTexto.Normalizar(Objeto.Pregunta);
            Objeto.Respuesta = ServicioTexto.Normalizar(Objeto.Respuesta);
            Objeto.Enlace = ServicioTexto.Normalizar(Objeto.Enlace);
            Objeto.EnlaceEtiqueta = ServicioTexto.Normalizar(Objeto.EnlaceEtiqueta);

            ValidarTextos(Objeto);
            ValidarEnlace(Objeto);
            ValidarOrden(Objeto.Orden);

            ObtenerCategoria(Objeto.CategoriaFaqId);

            if (!esAlta)
            {
                ObtenerPregunta(Objeto.PreguntaFrecuenteId);
            }

            Objeto.PreguntaFrecuenteId = oMPPFaq.GuardarPregunta(Objeto);

            Registrar(oSesionBE, esAlta ? "PreguntaAlta" : "PreguntaModificacion",
                "Pregunta " + Objeto.PreguntaFrecuenteId + " (" + Objeto.Pregunta + ").");

            return ObtenerPregunta(Objeto.PreguntaFrecuenteId);
        }

        public bool BajaPregunta(BEPreguntaFrecuente Objeto, BESesion oSesionBE)
        {
            BEPreguntaFrecuente oPreguntaBE = ObtenerPregunta(Objeto.PreguntaFrecuenteId);

            if (!oPreguntaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La pregunta ya está dada de baja.");
            }

            bool resultado = oMPPFaq.BajaPregunta(Objeto);

            Registrar(oSesionBE, "PreguntaBaja",
                "Baja de la pregunta " + Objeto.PreguntaFrecuenteId + " (" + oPreguntaBE.Pregunta + ").");

            return resultado;
        }

        public BEPreguntaFrecuente CambiarEstadoPregunta(BEPreguntaFrecuente Objeto, BESesion oSesionBE)
        {
            BEPreguntaFrecuente oPreguntaBE = ObtenerPregunta(Objeto.PreguntaFrecuenteId);

            if (oPreguntaBE.Activo == Objeto.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    Objeto.Activo ? "La pregunta ya está vigente." : "La pregunta ya está dada de baja.");
            }

            oMPPFaq.CambiarEstadoPregunta(Objeto);

            Registrar(oSesionBE, Objeto.Activo ? "PreguntaReactivacion" : "PreguntaBaja",
                "La pregunta " + Objeto.PreguntaFrecuenteId + " quedó " +
                (Objeto.Activo ? "vigente." : "dada de baja."));

            return ObtenerPregunta(Objeto.PreguntaFrecuenteId);
        }

        private void ValidarTextos(BEPreguntaFrecuente Objeto)
        {
            ReglasNegocio.ExigirConLargoMaximo(Objeto.Pregunta, LongitudMaximaPregunta,
                "La pregunta es obligatoria y no puede superar los " + LongitudMaximaPregunta + " caracteres.");
            ReglasNegocio.ExigirConLargoMaximo(Objeto.Respuesta, LongitudMaximaRespuesta,
                "La respuesta es obligatoria y no puede superar los " + LongitudMaximaRespuesta + " caracteres.");
        }

        private void ValidarEnlace(BEPreguntaFrecuente Objeto)
        {
            if (Objeto.Enlace == null)
            {
                Objeto.EnlaceEtiqueta = null;

                return;
            }

            if (Objeto.Enlace.Length > LongitudMaximaEnlace || !Objeto.Enlace.StartsWith("/"))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El enlace tiene que ser una ruta de la plataforma y empezar con /.");
            }

            if (Objeto.EnlaceEtiqueta == null ||
                Objeto.EnlaceEtiqueta.Length > LongitudMaximaEtiqueta)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El enlace necesita un texto de hasta " + LongitudMaximaEtiqueta + " caracteres.");
            }
        }

        private void ValidarOrden(int orden)
        {
            if (orden < 1 || orden > OrdenMaximo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El orden tiene que estar entre 1 y " + OrdenMaximo + ".");
            }
        }

        private void ValidarIdioma(int idiomaId)
        {
            foreach (BEIdioma oIdiomaBE in oBLLIdi.ListarTodoConBajas())
            {
                if (oIdiomaBE.IdiomaId == idiomaId)
                {
                    return;
                }
            }

            throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El idioma elegido no existe.");
        }

        private BEIdioma ResolverIdioma(string codigoISO)
        {
            BEIdioma oFiltroBE = new BEIdioma();

            oFiltroBE.CodigoISO = string.IsNullOrWhiteSpace(codigoISO)
                ? BLLTraduccion.CodigoIdiomaBase
                : codigoISO;

            return oBLLIdi.ResolverPorCodigo(oFiltroBE);
        }

        private void Registrar(BESesion oSesionBE, string accion, string descripcion)
        {
            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloFaq, accion, descripcion,
                BLLBitacora.NivelInformativo));
        }

        public void Validar(BEGuardarCategoriaFaq Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.IdiomaId, 1, int.MaxValue, "Elegí el idioma de la categoría.");

            ReglasNegocio.ExigirTexto(Objeto.Titulo, "El título de la categoría es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Titulo, 150, "El título no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.Orden, 1, 1000, "El orden tiene que estar entre 1 y 1000.");
        }

        public void Validar(BEGuardarPreguntaFrecuente Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.CategoriaFaqId, 1, int.MaxValue, "Elegí la categoría de la pregunta.");

            ReglasNegocio.ExigirTexto(Objeto.Pregunta, "La pregunta es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Pregunta, 300, "La pregunta no puede superar los 300 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Respuesta, "La respuesta es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Respuesta, 2000,
                "La respuesta no puede superar los 2000 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Enlace, 200, "El enlace no puede superar los 200 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.EnlaceEtiqueta, 100,
                "El texto del enlace no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.Orden, 1, 1000, "El orden tiene que estar entre 1 y 1000.");
        }
    }

    public class BLLContacto
    {
        ServicioEmailCuenta oServicioEmail = new ServicioEmailCuenta();
        ServicioCaptcha oServicioCaptcha = new ServicioCaptcha();
        BLLTicket oBLLTic = new BLLTicket();
        BLLBitacora oBLLBit = new BLLBitacora();

        public async Task<BEConsultaPublicaRespuesta> EnviarConsulta(BEEnviarConsultaContacto Objeto)
        {
            string nombreLimpio = LimpiarLinea(Objeto.Nombre);
            string emailLimpio = LimpiarLinea(Objeto.Email);
            string asuntoLimpio = LimpiarLinea(Objeto.Asunto);
            string mensajeLimpio = Objeto.Mensaje == null ? string.Empty : Objeto.Mensaje.Trim();

            ReglasNegocio.ExigirTexto(nombreLimpio, "Ingresá tu nombre.");
            ReglasNegocio.ExigirTexto(emailLimpio, "Ingresá tu correo.");

            ReglasNegocio.ExigirTexto(asuntoLimpio, "Ingresá un asunto.");
            ReglasNegocio.ExigirTexto(mensajeLimpio, "Ingresá tu mensaje.");

            if (!await oServicioCaptcha.ValidarAsync(Objeto.TokenCaptcha))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "No pudimos verificar el CAPTCHA. Volvé a intentarlo.");
            }

            BEEnviarConsultaContacto oLimpiaBE = new BEEnviarConsultaContacto(
                nombreLimpio, emailLimpio, asuntoLimpio, mensajeLimpio);

            oLimpiaBE.CodigoIdioma = Objeto.CodigoIdioma;

            BETicket oTicketBE = oBLLTic.GuardarConsultaPublica(oLimpiaBE);

            try
            {
                oServicioEmail.EnviarConsultaContacto(oLimpiaBE);
            }
            catch (Exception ex)
            {
                ServicioLog.Error("No se pudo avisar por correo la consulta de " + emailLimpio + ".", ex);

                oBLLBit.Guardar(new BEBitacoraEvento(
                    0, null, DateTime.Now, BLLBitacora.ModuloContacto, "ConsultaAvisoNoEnviado",
                    "Consulta #" + oTicketBE.TicketId + ": " + ex.Message, BLLBitacora.NivelError));
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, null, DateTime.Now, BLLBitacora.ModuloContacto, "ConsultaEnviada",
                "Consulta #" + oTicketBE.TicketId + " de " + emailLimpio + ": " + asuntoLimpio,
                BLLBitacora.NivelInformativo));

            return new BEConsultaPublicaRespuesta(
                "Tu consulta quedó registrada. Te responderemos a la brevedad.",
                oTicketBE.TicketId,
                oTicketBE.Token.Value);
        }

        public BETicketConDetalle ListarObjeto(BEFiltroTicket Objeto)
        {
            return oBLLTic.ListarObjetoPorToken(Objeto);
        }

        public BETicketConDetalle GuardarMensaje(BEResponderConsultaPublica Objeto)
        {
            return oBLLTic.ResponderVisitante(Objeto);
        }

        private string LimpiarLinea(string valor)
        {
            if (valor == null)
            {
                return string.Empty;
            }

            return valor.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        public void Validar(BEEnviarConsultaContacto Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "Ingresá tu nombre.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 100, "El nombre no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Email, "Ingresá tu correo.");
            ReglasNegocio.ExigirEmail(Objeto.Email, "El correo no tiene un formato válido.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Email, 150, "El correo no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Asunto, "Ingresá un asunto.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Asunto, 150, "El asunto no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Mensaje, "Ingresá tu mensaje.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Mensaje, 2000, "El mensaje no puede superar los 2000 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.TokenCaptcha, "Completá el CAPTCHA.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.CodigoIdioma, 5, "El código de idioma no es válido.");
        }

        public void Validar(BEResponderConsultaPublica Objeto)
        {
            ReglasNegocio.ExigirValor(Objeto.Token, "Falta el token del enlace.");

            ReglasNegocio.ExigirTexto(Objeto.Texto, "Escribí tu mensaje.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Texto, 2000, "El mensaje no puede superar los 2000 caracteres.");
        }
    }
}
