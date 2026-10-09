using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// Encuestas de una pregunta con fecha de vencimiento (Formulario, puntos 3 y
    /// 10). Responden los usuarios con sesión, una sola vez cada uno, y el
    /// resultado se devuelve en el momento para graficarlo.
    /// </summary>
    public class BLLEncuesta
    {
        private const int LongitudMaximaPregunta = 300;
        private const int LongitudMaximaDescripcion = 600;
        private const int LongitudMaximaOpcion = 200;
        private const int MinimoOpciones = 2;
        private const int MaximoOpciones = 10;

        MPPEncuesta oMPPEnc;
        BLLIdioma oBLLIdi;
        BLLBitacora oBLLBit;

        public BLLEncuesta()
        {
            oMPPEnc = new MPPEncuesta();
            oBLLIdi = new BLLIdioma();
            oBLLBit = new BLLBitacora();
        }

        /// <summary>
        /// Encuestas vigentes en el idioma de la interfaz, con el español como
        /// respaldo. Mientras el usuario no responde una encuesta vigente no
        /// recibe el detalle de los votos, para no condicionar su respuesta.
        /// </summary>
        public List<BEEncuestaConDetalle> ListarVigentes(BEFiltroEncuesta Objeto, BESesion oSesionBE)
        {
            BEIdioma oIdiomaBE = ResolverIdioma(Objeto.Idioma);

            if (oIdiomaBE == null)
            {
                return new List<BEEncuestaConDetalle>();
            }

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

            List<BEEncuestaConDetalle> ListaEncuestaBE =
                oMPPEnc.Listar(new BEFiltroEncuesta(null, oIdiomaBE.IdiomaId, hoy));

            if (ListaEncuestaBE == null)
            {
                BEIdioma oBaseBE = ResolverIdioma(BLLTraduccion.CodigoIdiomaBase);

                if (oBaseBE != null && oBaseBE.IdiomaId != oIdiomaBE.IdiomaId)
                {
                    ListaEncuestaBE = oMPPEnc.Listar(new BEFiltroEncuesta(null, oBaseBE.IdiomaId, hoy));
                }
            }

            if (ListaEncuestaBE == null)
            {
                return new List<BEEncuestaConDetalle>();
            }

            Completar(ListaEncuestaBE, oSesionBE);

            return ListaEncuestaBE;
        }

        public List<BEEncuestaConDetalle> ListarTodo()
        {
            List<BEEncuestaConDetalle> ListaEncuestaBE = oMPPEnc.Listar(new BEFiltroEncuesta());

            if (ListaEncuestaBE == null)
            {
                return new List<BEEncuestaConDetalle>();
            }

            Completar(ListaEncuestaBE, null);

            return ListaEncuestaBE;
        }

        public BEEncuestaConDetalle ListarObjeto(BEEncuesta Objeto)
        {
            List<BEEncuestaConDetalle> ListaEncuestaBE =
                oMPPEnc.Listar(new BEFiltroEncuesta(Objeto.EncuestaId, null, null));

            if (ListaEncuestaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La encuesta no existe.");
            }

            Completar(ListaEncuestaBE, null);

            return ListaEncuestaBE[0];
        }

        /// <summary>
        /// Alta y modificación. Las opciones solo se reemplazan mientras la
        /// encuesta no tenga respuestas: cambiarlas después dejaría los votos
        /// apuntando a textos distintos de los que se votaron.
        /// </summary>
        public BEEncuestaConDetalle Guardar(BEEncuesta Objeto, List<string> Opciones, BESesion oSesionBE)
        {
            bool esAlta = Objeto.EncuestaId == 0;
            int respuestas = 0;

            if (!esAlta)
            {
                respuestas = ListarObjeto(Objeto).TotalRespuestas;
            }

            List<string> ListaOpciones = Normalizar(Opciones);

            Validar(Objeto, ListaOpciones, esAlta || respuestas == 0);

            Objeto.UsuarioId = oSesionBE.UsuarioId;
            Objeto.EncuestaId = oMPPEnc.Guardar(Objeto);

            if (respuestas == 0 && ListaOpciones.Count > 0)
            {
                GuardarOpciones(Objeto, ListaOpciones);
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEncuesta,
                esAlta ? "Alta" : "Modificacion",
                "Encuesta " + Objeto.EncuestaId + " (" + Objeto.Pregunta + ").",
                BLLBitacora.NivelInformativo));

            return ListarObjeto(Objeto);
        }

        public bool Baja(BEEncuesta Objeto, BESesion oSesionBE)
        {
            BEEncuestaConDetalle oDetalleBE = ListarObjeto(Objeto);

            if (!oDetalleBE.Encuesta.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "La encuesta ya está dada de baja.");
            }

            bool resultado = oMPPEnc.Baja(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEncuesta,
                "Baja",
                "Baja de la encuesta " + Objeto.EncuestaId + " (" + oDetalleBE.Encuesta.Pregunta + ").",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        /// <summary>
        /// Registra el voto y devuelve la encuesta con los resultados ya
        /// actualizados, que es lo que grafica la pantalla.
        /// </summary>
        public BEEncuestaConDetalle Responder(BEResponderEncuesta Objeto, BESesion oSesionBE)
        {
            BEEncuesta oFiltroBE = new BEEncuesta();
            oFiltroBE.EncuestaId = Objeto.EncuestaId;

            BEEncuestaConDetalle oDetalleBE = ListarObjeto(oFiltroBE);

            if (!oDetalleBE.Vigente)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La encuesta está cerrada: ya no admite respuestas.");
            }

            if (!PerteneceALaEncuesta(oDetalleBE, Objeto.OpcionEncuestaId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La opción elegida no pertenece a esta encuesta.");
            }

            if (YaRespondio(Objeto.EncuestaId, oSesionBE))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "Ya respondiste esta encuesta.");
            }

            oMPPEnc.GuardarRespuesta(Objeto, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEncuesta,
                "Respuesta",
                "Respuesta a la encuesta " + Objeto.EncuestaId + ".",
                BLLBitacora.NivelInformativo));

            List<BEEncuestaConDetalle> ListaEncuestaBE =
                oMPPEnc.Listar(new BEFiltroEncuesta(Objeto.EncuestaId, null, null));

            Completar(ListaEncuestaBE, oSesionBE);

            return ListaEncuestaBE[0];
        }

        /// <summary>
        /// Carga en cada encuesta sus opciones, la vigencia y, si hay sesión, la
        /// opción que eligió ese usuario.
        /// </summary>
        private void Completar(List<BEEncuestaConDetalle> ListaEncuestaBE, BESesion oSesionBE)
        {
            List<BEOpcionEncuesta> ListaOpcionBE = oMPPEnc.ListarOpciones(new BEFiltroEncuesta());

            Dictionary<int, int> Respuestas = oSesionBE == null
                ? new Dictionary<int, int>()
                : oMPPEnc.ListarRespuestasDeUsuario(new BEFiltroEncuesta(), oSesionBE);

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

            foreach (BEEncuestaConDetalle oDetalleBE in ListaEncuestaBE)
            {
                int encuestaId = oDetalleBE.Encuesta.EncuestaId;

                oDetalleBE.Vigente = oDetalleBE.Encuesta.Activo
                    && oDetalleBE.Encuesta.FechaDesde <= hoy
                    && oDetalleBE.Encuesta.FechaVencimiento >= hoy;

                if (Respuestas.ContainsKey(encuestaId))
                {
                    oDetalleBE.OpcionElegidaId = Respuestas[encuestaId];
                }

                bool ocultarVotos = oSesionBE != null
                    && oDetalleBE.OpcionElegidaId == null
                    && oDetalleBE.Vigente;

                oDetalleBE.Opciones = new List<BEOpcionEncuesta>();

                if (ListaOpcionBE == null)
                {
                    continue;
                }

                foreach (BEOpcionEncuesta oOpcionBE in ListaOpcionBE)
                {
                    if (oOpcionBE.EncuestaId != encuestaId)
                    {
                        continue;
                    }

                    oDetalleBE.Opciones.Add(new BEOpcionEncuesta(
                        oOpcionBE.OpcionEncuestaId,
                        oOpcionBE.EncuestaId,
                        oOpcionBE.Texto,
                        oOpcionBE.Orden,
                        ocultarVotos ? 0 : oOpcionBE.Total));
                }
            }
        }

        private void GuardarOpciones(BEEncuesta Objeto, List<string> Opciones)
        {
            oMPPEnc.BajaOpciones(Objeto);

            int orden = 1;

            foreach (string texto in Opciones)
            {
                BEOpcionEncuesta oOpcionBE = new BEOpcionEncuesta();

                oOpcionBE.EncuestaId = Objeto.EncuestaId;
                oOpcionBE.Texto = texto;
                oOpcionBE.Orden = orden;

                oMPPEnc.GuardarOpcion(oOpcionBE);

                orden = orden + 1;
            }
        }

        private bool PerteneceALaEncuesta(BEEncuestaConDetalle oDetalleBE, int opcionEncuestaId)
        {
            foreach (BEOpcionEncuesta oOpcionBE in oDetalleBE.Opciones)
            {
                if (oOpcionBE.OpcionEncuestaId == opcionEncuestaId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool YaRespondio(int encuestaId, BESesion oSesionBE)
        {
            Dictionary<int, int> Respuestas =
                oMPPEnc.ListarRespuestasDeUsuario(new BEFiltroEncuesta(encuestaId, null, null), oSesionBE);

            return Respuestas.ContainsKey(encuestaId);
        }

        private List<string> Normalizar(List<string> Opciones)
        {
            List<string> ListaOpciones = new List<string>();

            if (Opciones == null)
            {
                return ListaOpciones;
            }

            foreach (string texto in Opciones)
            {
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    ListaOpciones.Add(texto.Trim());
                }
            }

            return ListaOpciones;
        }

        private void Validar(BEEncuesta Objeto, List<string> Opciones, bool exigirOpciones)
        {
            Objeto.Pregunta = Recortar(Objeto.Pregunta);
            Objeto.Descripcion = Recortar(Objeto.Descripcion);

            if (Objeto.Pregunta == null || Objeto.Pregunta.Length > LongitudMaximaPregunta)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La pregunta es obligatoria y no puede superar los " + LongitudMaximaPregunta + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El texto de ayuda no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }

            if (Objeto.FechaVencimiento < Objeto.FechaDesde)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La fecha de vencimiento no puede ser anterior a la de inicio.");
            }

            ValidarIdioma(Objeto);

            if (!exigirOpciones)
            {
                return;
            }

            if (Opciones.Count < MinimoOpciones || Opciones.Count > MaximoOpciones)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La encuesta necesita entre " + MinimoOpciones + " y " + MaximoOpciones + " opciones.");
            }

            foreach (string texto in Opciones)
            {
                if (texto.Length > LongitudMaximaOpcion)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "Cada opción puede tener hasta " + LongitudMaximaOpcion + " caracteres.");
                }
            }
        }

        private void ValidarIdioma(BEEncuesta Objeto)
        {
            foreach (BEIdioma oIdiomaBE in oBLLIdi.ListarTodoConBajas())
            {
                if (oIdiomaBE.IdiomaId == Objeto.IdiomaId)
                {
                    return;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "El idioma elegido para la encuesta no existe.");
        }

        private BEIdioma ResolverIdioma(string codigoISO)
        {
            BEIdioma oFiltroBE = new BEIdioma();

            oFiltroBE.CodigoISO = string.IsNullOrWhiteSpace(codigoISO)
                ? BLLTraduccion.CodigoIdiomaBase
                : codigoISO;

            return oBLLIdi.ResolverPorCodigo(oFiltroBE);
        }

        private string Recortar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
