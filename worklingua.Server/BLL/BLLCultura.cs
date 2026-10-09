using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
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

        MPPCultura oMPPCul;
        BLLIdioma oBLLIdi;

        public BLLCultura()
        {
            oMPPCul = new MPPCultura();
            oBLLIdi = new BLLIdioma();
        }

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
            Objeto.Codigo = Normalizar(Objeto.Codigo);
            Objeto.Nombre = Normalizar(Objeto.Nombre);
            Objeto.Moneda = Normalizar(Objeto.Moneda);
            Objeto.SimboloMoneda = Normalizar(Objeto.SimboloMoneda);
            Objeto.FormatoFecha = Normalizar(Objeto.FormatoFecha);

            ExigirTexto(Objeto.Codigo, "El código de cultura es obligatorio, por ejemplo es-AR.");
            ExigirTexto(Objeto.Nombre, "El nombre de la cultura es obligatorio.");
            ExigirTexto(Objeto.Moneda, "La moneda es obligatoria, por ejemplo ARS.");
            ExigirTexto(Objeto.SimboloMoneda, "El símbolo de la moneda es obligatorio.");
            ExigirTexto(Objeto.FormatoFecha, "El formato de fecha es obligatorio, por ejemplo dd/MM/yyyy.");

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

        private void ExigirTexto(string valor, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        private void ExigirLargo(string valor, int maximo, string etiqueta)
        {
            if (valor != null && valor.Length > maximo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    etiqueta + " no puede superar los " + maximo + " caracteres.");
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
    }
}
