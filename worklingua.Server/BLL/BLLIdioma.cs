using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLIdioma
    {
        private const int LongitudMaximaNombre = 60;
        private const int LongitudMinimaCodigo = 2;
        private const int LongitudMaximaCodigo = 5;

        static readonly TimeSpan DuracionCache = TimeSpan.FromMinutes(10);
        static List<BEIdioma> CacheIdiomas;
        static DateTime VencimientoCache;

        MPPIdioma oMPPIdi = new MPPIdioma();
        BLLTraduccion oBLLTra = new BLLTraduccion();

        public List<BEIdioma> ListarTodo()
        {
            if (CacheIdiomas != null && DateTime.Now < VencimientoCache)
            {
                return CacheIdiomas;
            }

            List<BEIdioma> ListaIdiomaBE = oMPPIdi.ListarTodo();

            if (ListaIdiomaBE == null)
            {
                ListaIdiomaBE = new List<BEIdioma>();
            }

            CacheIdiomas = ListaIdiomaBE;
            VencimientoCache = DateTime.Now.Add(DuracionCache);

            return ListaIdiomaBE;
        }

        public BEIdioma ResolverPorCodigo(BEIdioma Objeto)
        {
            BEIdioma oBaseBE = null;

            foreach (BEIdioma oIdiomaBE in ListarTodo())
            {
                if (string.Equals(oIdiomaBE.CodigoISO, ServicioTexto.Normalizar(Objeto.CodigoISO), StringComparison.OrdinalIgnoreCase))
                {
                    return oIdiomaBE;
                }

                if (EsIdiomaBase(oIdiomaBE))
                {
                    oBaseBE = oIdiomaBE;
                }
            }

            return oBaseBE;
        }

        public List<BEIdioma> ListarTodoConBajas()
        {
            List<BEIdioma> ListaIdiomaBE = oMPPIdi.ListarTodoConBajas();

            return ListaIdiomaBE == null ? new List<BEIdioma>() : ListaIdiomaBE;
        }

        public BEIdioma ListarObjeto(BEIdioma Objeto)
        {
            BEIdioma oIdiomaBE = oMPPIdi.ListarObjeto(Objeto);

            if (oIdiomaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El idioma no existe.");
            }

            return oIdiomaBE;
        }

        public BEIdioma Guardar(BEIdioma Objeto)
        {
            Validar(Objeto);

            bool esAlta = Objeto.IdiomaId == 0;

            if (!esAlta)
            {
                ListarObjeto(Objeto);
            }

            if (ExisteOtroConNombreOCodigo(Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Ya existe un idioma con ese nombre o con ese código ISO.");
            }

            Objeto.IdiomaId = oMPPIdi.Guardar(Objeto);

            if (esAlta)
            {
                oBLLTra.GenerarClavesFaltantes(Objeto);
            }

            InvalidarCache();

            return ListarObjeto(Objeto);
        }

        public bool Baja(BEIdioma Objeto)
        {
            BEIdioma oIdiomaBE = ListarObjeto(Objeto);

            if (oIdiomaBE.Activo != true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "El idioma ya está dado de baja.");
            }

            bool resultado = oMPPIdi.Baja(Objeto);

            InvalidarCache();

            return resultado;
        }

        public bool CambiarEstado(BEIdioma Objeto)
        {
            BEIdioma oIdiomaBE = ListarObjeto(Objeto);

            bool activar = Objeto.Activo.HasValue && Objeto.Activo.Value;

            if (oIdiomaBE.Activo == activar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    activar ? "El idioma ya está activo." : "El idioma ya está desactivado.");
            }

            if (!activar && EsIdiomaBase(oIdiomaBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El español no se puede desactivar: es el idioma base al que " +
                    "recurre la interfaz cuando falta una traducción.");
            }

            bool resultado = oMPPIdi.CambiarEstado(Objeto);

            InvalidarCache();

            return resultado;
        }

        private bool EsIdiomaBase(BEIdioma oIdiomaBE)
        {
            return string.Equals(
                oIdiomaBE.CodigoISO == null ? null : oIdiomaBE.CodigoISO.Trim(),
                BLLTraduccion.CodigoIdiomaBase,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool ExisteOtroConNombreOCodigo(BEIdioma Objeto)
        {
            List<BEIdioma> ListaIdiomaBE = ListarTodoConBajas();

            foreach (BEIdioma oIdiomaBE in ListaIdiomaBE)
            {
                if (oIdiomaBE.IdiomaId == Objeto.IdiomaId)
                {
                    continue;
                }

                if (string.Equals(
                        oIdiomaBE.Nombre, Objeto.Nombre, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        oIdiomaBE.CodigoISO, Objeto.CodigoISO, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void InvalidarCache()
        {
            CacheIdiomas = null;
            VencimientoCache = DateTime.MinValue;
        }

        private void Validar(BEIdioma Objeto)
        {
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.CodigoISO = ServicioTexto.Normalizar(Objeto.CodigoISO);

            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del idioma es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirTexto(Objeto.CodigoISO, "El código ISO es obligatorio.");

            Objeto.CodigoISO = Objeto.CodigoISO.ToLowerInvariant();

            if (Objeto.CodigoISO.Length < LongitudMinimaCodigo
                || Objeto.CodigoISO.Length > LongitudMaximaCodigo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El código ISO debe tener entre " + LongitudMinimaCodigo + " y " +
                    LongitudMaximaCodigo + " caracteres, por ejemplo 'es' o 'pt-br'.");
            }

            foreach (char caracter in Objeto.CodigoISO)
            {
                if (!char.IsLetter(caracter) && caracter != '-')
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "El código ISO solo admite letras y guiones.");
                }
            }
        }

        public void Validar(BEGuardarIdioma Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del idioma es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 60, "El nombre no puede superar los 60 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.CodigoISO, "El código ISO es obligatorio.");
            ReglasNegocio.ExigirLargo(Objeto.CodigoISO, 2, 5, "El código ISO debe tener entre 2 y 5 caracteres.");
        }
    }

    public class BLLCultura
    {
        private const int LongitudMaximaCodigo = 10;
        private const int LongitudMinimaCodigo = 2;
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaMoneda = 50;
        private const int LongitudMaximaSimbolo = 10;
        private const int LongitudMaximaFormatoFecha = 30;
        private const decimal TasaMaxima = 1000000m;

        static readonly TimeSpan DuracionCache = TimeSpan.FromMinutes(10);
        static List<BECulturaConIdioma> CacheCulturas;
        static DateTime VencimientoCache;

        MPPCultura oMPPCul = new MPPCultura();
        BLLIdioma oBLLIdi = new BLLIdioma();

        public List<BECulturaConIdioma> ListarTodo()
        {
            if (CacheCulturas != null && DateTime.Now < VencimientoCache)
            {
                return CacheCulturas;
            }

            List<BECulturaConIdioma> ListaCulturaBE = oMPPCul.ListarTodo();

            if (ListaCulturaBE == null)
            {
                ListaCulturaBE = new List<BECulturaConIdioma>();
            }

            CacheCulturas = ListaCulturaBE;
            VencimientoCache = DateTime.Now.Add(DuracionCache);

            return ListaCulturaBE;
        }

        public List<BECulturaConIdioma> ListarTodoConBajas()
        {
            List<BECulturaConIdioma> ListaCulturaBE = oMPPCul.ListarTodoConBajas();

            return ListaCulturaBE == null ? new List<BECulturaConIdioma>() : ListaCulturaBE;
        }

        public BECulturaConIdioma ListarObjeto(BECultura Objeto)
        {
            BECulturaConIdioma oCulturaBE = oMPPCul.ListarObjeto(Objeto);

            if (oCulturaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La cultura no existe.");
            }

            return oCulturaBE;
        }

        public BECulturaConIdioma Guardar(BECultura Objeto)
        {
            Validar(Objeto);

            if (Objeto.CulturaId != 0)
            {
                ListarObjeto(Objeto);
            }

            if (ExisteOtraConCodigo(Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Ya existe una cultura con ese código.");
            }

            Objeto.CulturaId = oMPPCul.Guardar(Objeto);

            InvalidarCache();

            return ListarObjeto(Objeto);
        }

        public bool Baja(BECultura Objeto)
        {
            BECulturaConIdioma oCulturaBE = ListarObjeto(Objeto);

            if (!oCulturaBE.Cultura.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La cultura ya está dada de baja.");
            }

            bool resultado = oMPPCul.Baja(Objeto);

            InvalidarCache();

            return resultado;
        }

        public bool CambiarEstado(BECultura Objeto)
        {
            ListarObjeto(Objeto);

            bool resultado = oMPPCul.CambiarEstado(Objeto);

            InvalidarCache();

            return resultado;
        }

        private bool ExisteOtraConCodigo(BECultura Objeto)
        {
            List<BECulturaConIdioma> ListaCulturaBE = ListarTodoConBajas();

            foreach (BECulturaConIdioma oCulturaBE in ListaCulturaBE)
            {
                if (oCulturaBE.Cultura.CulturaId == Objeto.CulturaId)
                {
                    continue;
                }

                if (string.Equals(
                    oCulturaBE.Cultura.Codigo, Objeto.Codigo, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void InvalidarCache()
        {
            CacheCulturas = null;
            VencimientoCache = DateTime.MinValue;
        }

        private void Validar(BECultura Objeto)
        {
            Objeto.Codigo = ServicioTexto.Normalizar(Objeto.Codigo);
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.Moneda = ServicioTexto.Normalizar(Objeto.Moneda);
            Objeto.SimboloMoneda = ServicioTexto.Normalizar(Objeto.SimboloMoneda);
            Objeto.FormatoFecha = ServicioTexto.Normalizar(Objeto.FormatoFecha);

            ReglasNegocio.ExigirTexto(Objeto.Codigo, "El código de cultura es obligatorio, por ejemplo es-AR.");
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre de la cultura es obligatorio.");
            ReglasNegocio.ExigirTexto(Objeto.Moneda, "La moneda es obligatoria, por ejemplo ARS.");
            ReglasNegocio.ExigirTexto(Objeto.SimboloMoneda, "El símbolo de la moneda es obligatorio.");
            ReglasNegocio.ExigirTexto(Objeto.FormatoFecha, "El formato de fecha es obligatorio, por ejemplo dd/MM/yyyy.");

            ExigirLargo(Objeto.Codigo, LongitudMaximaCodigo, "El código de cultura");
            ExigirLargo(Objeto.Nombre, LongitudMaximaNombre, "El nombre");
            ExigirLargo(Objeto.Moneda, LongitudMaximaMoneda, "La moneda");
            ExigirLargo(Objeto.SimboloMoneda, LongitudMaximaSimbolo, "El símbolo de la moneda");
            ExigirLargo(Objeto.FormatoFecha, LongitudMaximaFormatoFecha, "El formato de fecha");

            if (Objeto.Codigo.Length < LongitudMinimaCodigo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El código de cultura debe tener al menos " + LongitudMinimaCodigo +
                    " caracteres, por ejemplo es-AR.");
            }

            foreach (char caracter in Objeto.Codigo)
            {
                if (!char.IsLetter(caracter) && caracter != '-')
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion,
                        "El código de cultura solo admite letras y guiones.");
                }
            }

            Objeto.SeparadorDecimal = ResolverSeparador(Objeto.SeparadorDecimal, "decimal");
            Objeto.SeparadorMiles = ResolverSeparador(Objeto.SeparadorMiles, "de miles");

            if (Objeto.SeparadorDecimal == Objeto.SeparadorMiles)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El separador decimal y el de miles no pueden ser el mismo carácter.");
            }

            if (Objeto.TasaConversion <= 0m || Objeto.TasaConversion > TasaMaxima)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La tasa de conversión debe ser mayor que cero y menor o igual a " + TasaMaxima + ".");
            }

            if (!ExisteIdioma(Objeto.IdiomaId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El idioma de plataforma seleccionado no existe o está desactivado.");
            }
        }

        private bool ExisteIdioma(int idiomaId)
        {
            List<BEIdioma> ListaIdiomaBE = oBLLIdi.ListarTodo();

            foreach (BEIdioma oIdiomaBE in ListaIdiomaBE)
            {
                if (oIdiomaBE.IdiomaId == idiomaId)
                {
                    return true;
                }
            }

            return false;
        }

        private string ResolverSeparador(string separador, string cual)
        {
            if (string.IsNullOrEmpty(separador))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El separador " + cual + " es obligatorio.");
            }

            if (separador.Length != 1)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El separador " + cual + " tiene que ser un solo carácter.");
            }

            return separador;
        }

        private void ExigirLargo(string valor, int maximo, string etiqueta)
        {
            ReglasNegocio.ExigirLargoMaximo(valor, maximo,
                etiqueta + " no puede superar los " + maximo + " caracteres.");
        }

        public void Validar(BEGuardarCultura Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Codigo, "El código de cultura es obligatorio.");
            ReglasNegocio.ExigirLargo(Objeto.Codigo, 2, 10,
                "El código debe tener entre 2 y 10 caracteres, por ejemplo es-AR.");

            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre de la cultura es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 100, "El nombre no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.IdiomaId, 1, int.MaxValue, "Seleccioná el idioma de plataforma.");

            ReglasNegocio.ExigirTexto(Objeto.Moneda, "La moneda es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Moneda, 50, "La moneda no puede superar los 50 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.SimboloMoneda, "El símbolo de la moneda es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.SimboloMoneda, 10, "El símbolo no puede superar los 10 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.FormatoFecha, "El formato de fecha es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.FormatoFecha, 30,
                "El formato de fecha no puede superar los 30 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.SeparadorDecimal, "El separador decimal es obligatorio.");
            ReglasNegocio.ExigirLargo(Objeto.SeparadorDecimal, 1, 1, "El separador decimal es un solo carácter.");

            ReglasNegocio.ExigirTexto(Objeto.SeparadorMiles, "El separador de miles es obligatorio.");
            ReglasNegocio.ExigirLargo(Objeto.SeparadorMiles, 1, 1, "El separador de miles es un solo carácter.");

            ReglasNegocio.ExigirRango(Objeto.TasaConversion, 0.000001m, 1000000m,
                "La tasa de conversión debe ser mayor que cero.");
        }
    }

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

        MPPTraduccion oMPPTra = new MPPTraduccion();

        public Dictionary<string, string> ListarPorCodigo(string codigoISO)
        {
            string codigo = ServicioTexto.Normalizar(codigoISO);

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
            Objeto.Clave = ServicioTexto.Normalizar(Objeto.Clave);

            Objeto.Texto = Objeto.Texto == null ? string.Empty : Objeto.Texto.Trim();

            ReglasNegocio.ExigirTexto(Objeto.Clave, "La clave de la traducción es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Clave, LongitudMaximaClave,
                "La clave no puede superar los " + LongitudMaximaClave + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Texto, LongitudMaximaTexto,
                "El texto de la clave " + Objeto.Clave + " no puede superar los " + LongitudMaximaTexto + " caracteres.");
        }

        private string Acotar(string criterio)
        {
            string limpio = ServicioTexto.Normalizar(criterio);

            if (limpio == null)
            {
                return null;
            }

            return limpio.Length <= LongitudMaximaTermino
                ? limpio
                : limpio.Substring(0, LongitudMaximaTermino);
        }

        public void Validar(BEGuardarTraducciones Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.IdiomaId, 1, int.MaxValue, "Hay que elegir un idioma.");

            if (Objeto.Traducciones != null)
            {
                foreach (BETraduccionItem oItemBE in Objeto.Traducciones)
                {
                    Validar(oItemBE);
                }
            }

            ReglasNegocio.ExigirValor(Objeto.Traducciones, "No hay traducciones para guardar.");
            ReglasNegocio.ExigirCantidadMinima(Objeto.Traducciones, 1, "No hay traducciones para guardar.");
        }

        private void Validar(BETraduccionItem Objeto)
        {
            if (Objeto == null)
            {
                return;
            }

            ReglasNegocio.ExigirTexto(Objeto.Clave, "La clave de la traducción es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Clave, 150, "La clave no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Texto, 1000, "El texto no puede superar los 1000 caracteres.");
        }
    }
}
