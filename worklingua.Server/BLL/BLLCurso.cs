using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLCurso
    {
        private const int LongitudMaximaTermino = 150;
        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;
        private const int LongitudMaximaIdioma = 50;
        private const int DuracionMaximaHoras = 10000;

        static readonly string[] NivelesValidos = { "Inicial", "Intermedio", "Avanzado" };

        /// <summary>Categorías sectoriales del negocio (CU-004-005).</summary>
        static readonly string[] SectoresValidos = { "TURISMO", "GASTRONOMIA", "IT" };

        MPPCurso oMPPCur;
        BLLSeguridad oBLLSeg;
        BLLUsuario oBLLUsu;
        BLLEtiqueta oBLLEti;

        public BLLCurso()
        {
            oMPPCur = new MPPCurso();
            oBLLSeg = new BLLSeguridad();
            oBLLUsu = new BLLUsuario();
            oBLLEti = new BLLEtiqueta();
        }

        public List<BECurso> Buscar(BEFiltroCurso Objeto, BESesion oSesionBE)
        {
            return Buscar(Objeto, oSesionBE, false);
        }

        /// <summary>
        /// El listado del Backoffice. Ve también los cursos fuera de su ventana
        /// de despliegue, que es desde donde se programan (CU-004-006).
        /// </summary>
        public List<BECurso> ListarParaAdministracion(BESesion oSesionBE)
        {
            return Buscar(new BEFiltroCurso(), oSesionBE, true);
        }

        private List<BECurso> Buscar(BEFiltroCurso Objeto, BESesion oSesionBE, bool incluirFueraDeVentana)
        {
            BEFiltroCurso oFiltro = new BEFiltroCurso(
                Normalizar(Objeto.Nombre),
                Normalizar(Objeto.Idioma),
                Normalizar(Objeto.Nivel));

            oFiltro.EmpresaId = EmpresaQueLimita(oSesionBE);
            oFiltro.Sector = ResolverSector(Normalizar(Objeto.Sector));
            oFiltro.EtiquetaId = Objeto.EtiquetaId;
            oFiltro.IncluirFueraDeVentana = incluirFueraDeVentana;

            List<BECurso> ListaCursoBE = oMPPCur.Buscar(oFiltro);

            if (ListaCursoBE == null)
            {
                return new List<BECurso>();
            }

            oBLLEti.AgregarACursos(ListaCursoBE);

            return ListaCursoBE;
        }

        public List<string> ListarIdiomas(BESesion oSesionBE)
        {
            BEFiltroCurso oAlcance = new BEFiltroCurso();
            oAlcance.EmpresaId = EmpresaQueLimita(oSesionBE);

            List<string> ListaIdioma = oMPPCur.ListarIdiomas(oAlcance);

            return ListaIdioma == null ? new List<string>() : ListaIdioma;
        }

        /// <summary>
        /// Un curso de otra empresa se informa como inexistente, igual que uno que
        /// no existe, para no revelar los cursos ajenos.
        /// </summary>
        public BECurso ListarObjeto(BECurso Objeto, BESesion oSesionBE)
        {
            BEFiltroCurso oAlcance = new BEFiltroCurso();
            oAlcance.EmpresaId = EmpresaQueLimita(oSesionBE);

            BECurso oCursoBE = oMPPCur.ListarObjeto(Objeto, oAlcance);

            if (oCursoBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El curso no existe.");
            }

            foreach (BEEtiquetaCurso oAsignadaBE in oBLLEti.ListarAsignadas(oCursoBE.CursoId))
            {
                oCursoBE.Etiquetas.Add(
                    new BEEtiqueta(oAsignadaBE.EtiquetaId, oAsignadaBE.Nombre, true, null));
            }

            return oCursoBE;
        }

        public BECurso Guardar(BECurso Objeto, BESesion oSesionBE)
        {
            Validar(Objeto);

            if (Objeto.CursoId != 0)
            {
                // La empresa dueña no cambia al modificar.
                Objeto.EmpresaId = ExigirGestion(Objeto, oSesionBE).EmpresaId;
            }
            else
            {
                // Con alcance total se da de alta un curso global del catálogo.
                Objeto.EmpresaId = EmpresaQueLimita(oSesionBE);
            }

            ValidarNombreDisponible(Objeto);

            Objeto.CursoId = oMPPCur.Guardar(Objeto);

            oBLLEti.AsignarACurso(Objeto.CursoId, Objeto.Etiquetas);

            return ListarObjeto(Objeto, oSesionBE);
        }

        public bool Baja(BECurso Objeto, BESesion oSesionBE)
        {
            BECurso oCursoBE = ExigirGestion(Objeto, oSesionBE);

            if (!oCursoBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "El curso ya está dado de baja.");
            }

            return oMPPCur.Baja(Objeto);
        }

        /// <summary>
        /// Devuelve el curso si quien opera puede modificarlo: los cursos globales
        /// solo los gestiona quien tiene alcance sobre todas las empresas.
        /// </summary>
        public BECurso ExigirGestion(BECurso Objeto, BESesion oSesionBE)
        {
            BECurso oCursoBE = ListarObjeto(Objeto, oSesionBE);

            if (!oCursoBE.EmpresaId.HasValue && EmpresaQueLimita(oSesionBE).HasValue)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.PermisoDenegado,
                    "Los cursos del catálogo de WorkLingua solo los administra la plataforma.");
            }

            return oCursoBE;
        }

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            if (oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.CursoVerTodasLasEmpresas))
            {
                return null;
            }

            return oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId).EmpresaId;
        }

        private void Validar(BECurso Objeto)
        {
            Objeto.Nombre = Recortar(Objeto.Nombre);
            Objeto.Descripcion = Recortar(Objeto.Descripcion);
            Objeto.Nivel = Recortar(Objeto.Nivel);
            Objeto.Idioma = Recortar(Objeto.Idioma);

            if (string.IsNullOrWhiteSpace(Objeto.Nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre del curso es obligatorio.");
            }

            if (Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre del curso no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }

            Objeto.Nivel = ResolverNivel(Objeto.Nivel);

            if (string.IsNullOrWhiteSpace(Objeto.Idioma))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El idioma que enseña el curso es obligatorio.");
            }

            if (Objeto.Idioma.Length > LongitudMaximaIdioma)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El idioma no puede superar los " + LongitudMaximaIdioma + " caracteres.");
            }

            if (Objeto.DuracionHoras.HasValue
                && (Objeto.DuracionHoras.Value < 1 || Objeto.DuracionHoras.Value > DuracionMaximaHoras))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La duración debe estar entre 1 y " + DuracionMaximaHoras + " horas.");
            }

            Objeto.Sector = ResolverSector(Objeto.Sector);

            ValidarVentana(Objeto);
        }

        /// <summary>
        /// El sector es una lista cerrada, como el nivel. Vacío es válido: un
        /// curso puede no estar clasificado todavía.
        /// </summary>
        private string ResolverSector(string sector)
        {
            if (string.IsNullOrWhiteSpace(sector))
            {
                return null;
            }

            foreach (string valido in SectoresValidos)
            {
                if (string.Equals(sector.Trim(), valido, StringComparison.OrdinalIgnoreCase))
                {
                    return valido;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "El sector debe ser " + string.Join(", ", SectoresValidos) + ".");
        }

        /// <summary>
        /// Camino alternativo 1 de CU-004-006. Las dos fechas en null son
        /// "siempre disponible", que es como quedan los cursos sin programar.
        ///
        /// No se exige que la publicación sea futura: el paso 10 del caso de
        /// uso lo pide, pero eso impediría corregir un curso ya publicado sin
        /// reescribirle la fecha de arranque.
        /// </summary>
        private void ValidarVentana(BECurso Objeto)
        {
            if (Objeto.FechaPublicacion.HasValue && Objeto.FechaFin.HasValue
                && Objeto.FechaFin.Value < Objeto.FechaPublicacion.Value)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El rango de fechas es inválido. Verifique los datos ingresados.");
            }
        }

        private string ResolverNivel(string nivel)
        {
            foreach (string valido in NivelesValidos)
            {
                if (string.Equals(nivel, valido, StringComparison.OrdinalIgnoreCase))
                {
                    return valido;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "El nivel debe ser " + string.Join(", ", NivelesValidos) + ".");
        }

        /// <summary>
        /// El nombre no se repite entre los cursos que ve la misma empresa: los
        /// globales y los propios. Un curso global se compara con todos.
        /// </summary>
        private void ValidarNombreDisponible(BECurso Objeto)
        {
            BEFiltroCurso oAlcance = new BEFiltroCurso();
            oAlcance.EmpresaId = Objeto.EmpresaId;

            List<BECurso> ListaCursoBE = oMPPCur.ListarTodoConBajas(oAlcance) ?? new List<BECurso>();

            foreach (BECurso oCursoBE in ListaCursoBE)
            {
                bool mismoNombre = string.Equals(
                    oCursoBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase);

                if (mismoNombre && oCursoBE.CursoId != Objeto.CursoId)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto, "Ya existe un curso con ese nombre.");
                }
            }
        }

        private string Recortar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }

        private string Normalizar(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
            {
                return null;
            }

            string limpio = criterio.Trim();

            return limpio.Length <= LongitudMaximaTermino
                ? limpio
                : limpio.Substring(0, LongitudMaximaTermino);
        }
    }
}
