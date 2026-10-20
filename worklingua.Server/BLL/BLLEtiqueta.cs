using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLEtiqueta
    {
        private const int LongitudMaximaNombre = 50;

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

        private string Normalizar(string nombre)
        {
            if (nombre == null)
            {
                return null;
            }

            return Regex.Replace(nombre.Trim(), @"\s+", " ");
        }

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
