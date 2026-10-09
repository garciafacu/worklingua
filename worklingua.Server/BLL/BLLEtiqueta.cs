using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// El diccionario de etiquetas con el que se clasifican los cursos
    /// (CU-004-005).
    ///
    /// El diccionario es global: la misma etiqueta sirve para cursos de
    /// cualquier empresa, que es lo que la vuelve útil para buscar. Por eso no
    /// tiene alcance propio; quién puede clasificar lo decide el curso.
    /// </summary>
    public class BLLEtiqueta
    {
        private const int LongitudMaximaNombre = 50;

        /// <summary>
        /// Paso 10: la etiqueta no admite caracteres especiales. Letras (con
        /// acentos y ñ), números, espacios y guiones.
        /// </summary>
        private static readonly Regex Permitidos =
            new Regex(@"^[\p{L}\p{Nd} \-]+$", RegexOptions.Compiled);

        MPPEtiqueta oMPPEti;

        public BLLEtiqueta()
        {
            oMPPEti = new MPPEtiqueta();
        }

        public List<BEEtiqueta> Listar()
        {
            return oMPPEti.Listar();
        }

        public List<BEEtiquetaCurso> ListarAsignadas(int cursoId)
        {
            return oMPPEti.ListarAsignadas(cursoId);
        }

        /// <summary>
        /// Completa las etiquetas de una lista de cursos con una sola consulta,
        /// en vez de una por fila.
        ///
        /// Lo usan el ABM de Cursos y Mis cursos: las dos pantallas muestran la
        /// clasificación, y así el armado vive en un solo lugar.
        /// </summary>
        public void AgregarACursos(List<BECurso> ListaCursoBE)
        {
            if (ListaCursoBE == null || ListaCursoBE.Count == 0)
            {
                return;
            }

            List<BEEtiquetaCurso> ListaAsignadaBE = ListarAsignadas(0);

            foreach (BECurso oCursoBE in ListaCursoBE)
            {
                oCursoBE.Etiquetas = new List<BEEtiqueta>();

                foreach (BEEtiquetaCurso oAsignadaBE in ListaAsignadaBE)
                {
                    if (oAsignadaBE.CursoId == oCursoBE.CursoId)
                    {
                        oCursoBE.Etiquetas.Add(
                            new BEEtiqueta(oAsignadaBE.EtiquetaId, oAsignadaBE.Nombre, true, null));
                    }
                }
            }
        }

        /// <summary>
        /// Da de alta una etiqueta en el diccionario (camino alternativo 1).
        /// Si ya existe devuelve la que había: el diccionario no admite
        /// duplicados y pedirla dos veces no es un error.
        /// </summary>
        public BEEtiqueta Crear(BEEtiqueta Objeto)
        {
            string nombre = Normalizar(Objeto.Nombre);

            Validar(nombre);

            BEEtiqueta oExistenteBE = Buscar(nombre);

            if (oExistenteBE != null)
            {
                return oExistenteBE;
            }

            BEEtiqueta oNuevaBE = new BEEtiqueta();
            oNuevaBE.Nombre = nombre;
            oNuevaBE.EtiquetaId = oMPPEti.Alta(oNuevaBE);
            oNuevaBE.Activo = true;

            return oNuevaBE;
        }

        /// <summary>
        /// Deja el curso con exactamente estas etiquetas. Acepta las que ya
        /// están en el diccionario: una que no existe es un error, porque
        /// crearla es una decisión que el usuario confirma aparte.
        /// </summary>
        public void AsignarACurso(int cursoId, List<BEEtiqueta> ListaBE)
        {
            List<int> etiquetaIds = new List<int>();

            if (ListaBE != null)
            {
                List<BEEtiqueta> ListaDiccionarioBE = Listar();

                foreach (BEEtiqueta oEtiquetaBE in ListaBE)
                {
                    etiquetaIds.Add(ExigirDelDiccionario(oEtiquetaBE, ListaDiccionarioBE));
                }
            }

            oMPPEti.Reemplazar(cursoId, etiquetaIds);
        }

        /// <summary>
        /// Resuelve una etiqueta pedida contra el diccionario, por id o por
        /// nombre. El nombre se compara sin acentos ni mayúsculas, igual que
        /// lo hace el índice único.
        /// </summary>
        private int ExigirDelDiccionario(BEEtiqueta Objeto, List<BEEtiqueta> ListaDiccionarioBE)
        {
            foreach (BEEtiqueta oEtiquetaBE in ListaDiccionarioBE)
            {
                bool coincidePorId = Objeto.EtiquetaId != 0 && oEtiquetaBE.EtiquetaId == Objeto.EtiquetaId;
                bool coincidePorNombre = Objeto.EtiquetaId == 0
                    && Clave(oEtiquetaBE.Nombre) == Clave(Normalizar(Objeto.Nombre));

                if (coincidePorId || coincidePorNombre)
                {
                    return oEtiquetaBE.EtiquetaId;
                }
            }

            string referencia = string.IsNullOrWhiteSpace(Objeto.Nombre)
                ? "con id " + Objeto.EtiquetaId
                : "\"" + Objeto.Nombre + "\"";

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La etiqueta " + referencia + " no existe en el diccionario.");
        }

        private BEEtiqueta Buscar(string nombre)
        {
            foreach (BEEtiqueta oEtiquetaBE in Listar())
            {
                if (Clave(oEtiquetaBE.Nombre) == Clave(nombre))
                {
                    return oEtiquetaBE;
                }
            }

            return null;
        }

        private void Validar(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "La etiqueta no puede estar vacía.");
            }

            if (nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La etiqueta no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (!Permitidos.IsMatch(nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La etiqueta solo admite letras, números, espacios y guiones.");
            }
        }

        /// <summary>Espacios de más colapsados y recortada.</summary>
        private string Normalizar(string nombre)
        {
            if (nombre == null)
            {
                return null;
            }

            return Regex.Replace(nombre.Trim(), @"\s+", " ");
        }

        /// <summary>
        /// Clave de comparación: sin mayúsculas ni acentos, para que
        /// "Recepción" y "recepcion" sean la misma etiqueta.
        /// </summary>
        private string Clave(string nombre)
        {
            if (nombre == null)
            {
                return string.Empty;
            }

            string descompuesto = nombre.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder limpio = new StringBuilder(descompuesto.Length);

            foreach (char caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                {
                    limpio.Append(caracter);
                }
            }

            return limpio.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
