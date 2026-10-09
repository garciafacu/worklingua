using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLTraduccion
    {
        private const int LongitudMaximaClave = 150;
        private const int LongitudMaximaTexto = 1000;
        private const int LongitudMaximaTermino = 150;

        public const string CodigoIdiomaBase = "es";

        static readonly TimeSpan DuracionCache = TimeSpan.FromMinutes(10);
        static Dictionary<string, Dictionary<string, string>> CacheBundles =
            new Dictionary<string, Dictionary<string, string>>();
        static DateTime VencimientoCache;

        static readonly object CandadoCache = new object();

        MPPTraduccion oMPPTra;

        public BLLTraduccion()
        {
            oMPPTra = new MPPTraduccion();
        }

        public Dictionary<string, string> ListarPorCodigo(string codigoISO)
        {
            string codigo = Normalizar(codigoISO);

            if (codigo == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Falta el código de idioma.");
            }

            codigo = codigo.ToLowerInvariant();

            lock (CandadoCache)
            {
                if (DateTime.Now < VencimientoCache && CacheBundles.ContainsKey(codigo))
                {
                    return CacheBundles[codigo];
                }
            }

            BEIdioma oFiltroBE = new BEIdioma();
            oFiltroBE.CodigoISO = codigo;

            Dictionary<string, string> Bundle = oMPPTra.ListarPorCodigo(oFiltroBE);

            if (Bundle == null)
            {
                Bundle = new Dictionary<string, string>();
            }

            lock (CandadoCache)
            {
                if (DateTime.Now >= VencimientoCache)
                {
                    CacheBundles = new Dictionary<string, Dictionary<string, string>>();
                    VencimientoCache = DateTime.Now.Add(DuracionCache);
                }

                CacheBundles[codigo] = Bundle;
            }

            return Bundle;
        }

        public List<BETraduccionAdministracion> ListarAdministracion(BEFiltroTraduccion Objeto)
        {
            if (Objeto.IdiomaId <= 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Hay que elegir un idioma.");
            }

            BEFiltroTraduccion oFiltro = new BEFiltroTraduccion(
                Objeto.IdiomaId,
                Acotar(Objeto.Clave),
                Acotar(Objeto.Texto),
                Objeto.SoloPendientes);

            List<BETraduccionAdministracion> ListaTraduccionBE = oMPPTra.ListarAdministracion(oFiltro);

            return ListaTraduccionBE == null
                ? new List<BETraduccionAdministracion>()
                : ListaTraduccionBE;
        }

        public int GuardarLote(BEIdioma oIdiomaBE, List<BETraduccion> ListaTraduccionBE)
        {
            if (oIdiomaBE.IdiomaId <= 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Hay que elegir un idioma.");
            }

            if (ListaTraduccionBE == null || ListaTraduccionBE.Count == 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "No hay traducciones para guardar.");
            }

            int guardadas = 0;

            foreach (BETraduccion oTraduccionBE in ListaTraduccionBE)
            {
                Validar(oTraduccionBE);

                oTraduccionBE.IdiomaId = oIdiomaBE.IdiomaId;

                if (oMPPTra.Guardar(oTraduccionBE))
                {
                    guardadas = guardadas + 1;
                }
            }

            InvalidarCache();

            return guardadas;
        }

        public int GenerarClavesFaltantes(BEIdioma Objeto)
        {
            int generadas = oMPPTra.GenerarClavesFaltantes(Objeto);

            if (generadas > 0)
            {
                InvalidarCache();
            }

            return generadas;
        }

        public List<BETraduccion> ListarClaves()
        {
            List<BETraduccion> ListaTraduccionBE = oMPPTra.ListarClaves();

            return ListaTraduccionBE == null ? new List<BETraduccion>() : ListaTraduccionBE;
        }

        private void InvalidarCache()
        {
            lock (CandadoCache)
            {
                CacheBundles = new Dictionary<string, Dictionary<string, string>>();
                VencimientoCache = DateTime.MinValue;
            }
        }

        private void Validar(BETraduccion Objeto)
        {
            Objeto.Clave = Normalizar(Objeto.Clave);

            Objeto.Texto = Objeto.Texto == null ? string.Empty : Objeto.Texto.Trim();

            if (string.IsNullOrWhiteSpace(Objeto.Clave))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La clave de la traducción es obligatoria.");
            }

            if (Objeto.Clave.Length > LongitudMaximaClave)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La clave no puede superar los " + LongitudMaximaClave + " caracteres.");
            }

            if (Objeto.Texto.Length > LongitudMaximaTexto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El texto de la clave " + Objeto.Clave + " no puede superar los " +
                    LongitudMaximaTexto + " caracteres.");
            }
        }

        private string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }

        private string Acotar(string criterio)
        {
            string limpio = Normalizar(criterio);

            if (limpio == null)
            {
                return null;
            }

            return limpio.Length <= LongitudMaximaTermino
                ? limpio
                : limpio.Substring(0, LongitudMaximaTermino);
        }
    }
}
