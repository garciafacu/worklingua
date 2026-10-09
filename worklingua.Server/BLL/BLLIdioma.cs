using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

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

        MPPIdioma oMPPIdi;
        BLLTraduccion oBLLTra;

        public BLLIdioma()
        {
            oMPPIdi = new MPPIdioma();
            oBLLTra = new BLLTraduccion();
        }

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
                if (string.Equals(oIdiomaBE.CodigoISO, Normalizar(Objeto.CodigoISO), StringComparison.OrdinalIgnoreCase))
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
            Objeto.Nombre = Normalizar(Objeto.Nombre);
            Objeto.CodigoISO = Normalizar(Objeto.CodigoISO);

            if (string.IsNullOrWhiteSpace(Objeto.Nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre del idioma es obligatorio.");
            }

            if (Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (string.IsNullOrWhiteSpace(Objeto.CodigoISO))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El código ISO es obligatorio.");
            }

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
