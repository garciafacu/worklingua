using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

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

        MPPCurso oMPPCur = new MPPCurso();
        BLLSeguridad oBLLSeg = new BLLSeguridad();
        BLLUsuario oBLLUsu = new BLLUsuario();
        BLLEtiqueta oBLLEti = new BLLEtiqueta();

        public List<BECurso> Buscar(BEFiltroCurso Objeto, BESesion oSesionBE)
        {
            return Buscar(Objeto, oSesionBE, false);
        }

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
                Objeto.EmpresaId = ExigirGestion(Objeto, oSesionBE).EmpresaId;
            }
            else
            {
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
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.Descripcion = ServicioTexto.Normalizar(Objeto.Descripcion);
            Objeto.Nivel = ServicioTexto.Normalizar(Objeto.Nivel);
            Objeto.Idioma = ServicioTexto.Normalizar(Objeto.Idioma);

            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del curso es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre del curso no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");

            Objeto.Nivel = ResolverNivel(Objeto.Nivel);

            ReglasNegocio.ExigirTexto(Objeto.Idioma, "El idioma que enseña el curso es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Idioma, LongitudMaximaIdioma,
                "El idioma no puede superar los " + LongitudMaximaIdioma + " caracteres.");

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

        private string ResolverSector(string sector)
        {
            return Sectores.Resolver(sector);
        }

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

        public void Validar(BEFiltroCurso Objeto)
        {
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 150,
                "El término de búsqueda no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Idioma, 50, "El idioma no puede superar los 50 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Nivel, 20, "El nivel no puede superar los 20 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Sector, 30, "El sector no puede superar los 30 caracteres.");
        }

        public void Validar(BEGuardarCurso Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Idioma, "Indicá el idioma que enseña el curso.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Idioma, 50, "El idioma no puede superar los 50 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del curso es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 150, "El nombre no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 500,
                "La descripción no puede superar los 500 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Nivel, "El nivel es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nivel, 20, "El nivel no puede superar los 20 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.DuracionHoras, 1, 10000, "La duración debe estar entre 1 y 10000 horas.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Sector, 30, "El sector no puede superar los 30 caracteres.");
        }
    }

    public class BLLModulo
    {
        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;
        private const int LongitudMaximaContenido = 8000;

        MPPModulo oMPPMod = new MPPModulo();
        BLLCurso oBLLCur = new BLLCurso();
        BLLBitacora oBLLBit = new BLLBitacora();

        public List<BEModulo> ListarPorCurso(BECurso Objeto, BESesion oSesionBE)
        {
            oBLLCur.ListarObjeto(Objeto, oSesionBE);

            List<BEModulo> ListaModuloBE = oMPPMod.ListarPorCurso(Objeto);

            return ListaModuloBE == null ? new List<BEModulo>() : ListaModuloBE;
        }

        public BEModulo Guardar(BEModulo Objeto, BESesion oSesionBE)
        {
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.Descripcion = ServicioTexto.Normalizar(Objeto.Descripcion);
            Objeto.Contenido = ServicioTexto.Normalizar(Objeto.Contenido);

            ReglasNegocio.ExigirConLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre del módulo es obligatorio y no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "El objetivo no puede superar los " + LongitudMaximaDescripcion + " caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Contenido, LongitudMaximaContenido,
                "El contenido no puede superar los " + LongitudMaximaContenido + " caracteres.");

            if (Objeto.OrdenModulo < 1)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El orden del módulo tiene que ser mayor que cero.");
            }

            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = Objeto.CursoId;

            BECurso oCursoBE = oBLLCur.ExigirGestion(oFiltroBE, oSesionBE);

            if (!oCursoBE.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "El curso está dado de baja.");
            }

            foreach (BEModulo oModuloBE in ListarPorCurso(oFiltroBE, oSesionBE))
            {
                if (oModuloBE.OrdenModulo == Objeto.OrdenModulo)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto,
                        "Ya hay un módulo con el orden " + Objeto.OrdenModulo + " en este curso.");
                }
            }

            Objeto.ModuloId = oMPPMod.Guardar(Objeto);
            Objeto.Activo = true;

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "ModuloAlta",
                "Módulo " + Objeto.ModuloId + " (" + Objeto.Nombre + ") en el curso " + oCursoBE.Nombre + ".",
                BLLBitacora.NivelInformativo));

            return Objeto;
        }

        public bool Baja(BEModulo Objeto, BESesion oSesionBE)
        {
            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = Objeto.CursoId;

            oBLLCur.ExigirGestion(oFiltroBE, oSesionBE);

            BEModulo oModuloBE = null;

            foreach (BEModulo oItemBE in ListarPorCurso(oFiltroBE, oSesionBE))
            {
                if (oItemBE.ModuloId == Objeto.ModuloId)
                {
                    oModuloBE = oItemBE;
                }
            }

            if (oModuloBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El módulo no existe o ya está dado de baja.");
            }

            bool resultado = oMPPMod.Baja(oModuloBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloCurso, "ModuloBaja",
                "Baja del módulo " + oModuloBE.ModuloId + " (" + oModuloBE.Nombre + ") del curso " + oModuloBE.CursoId + ".",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        public void Validar(BEGuardarModulo Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del módulo es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 150,
                "El nombre del módulo no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 500,
                "El objetivo no puede superar los 500 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Contenido, 8000,
                "El contenido no puede superar los 8000 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.OrdenModulo, 1, 1000, "El orden tiene que estar entre 1 y 1000.");
        }
    }

    public class BLLEtiqueta
    {
        private const int LongitudMaximaNombre = 50;

        private static readonly Regex Permitidos =
            new Regex(@"^[\p{L}\p{Nd} \-]+$", RegexOptions.Compiled);

        MPPEtiqueta oMPPEti = new MPPEtiqueta();

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

            ReglasNegocio.ExigirLargoMaximo(nombre, LongitudMaximaNombre,
                "La etiqueta no puede superar los " + LongitudMaximaNombre + " caracteres.");

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

    public class BLLProgreso
    {
        public const string EstadoNoIniciado = "NO_INICIADO";
        public const string EstadoEnCurso = "EN_CURSO";
        public const string EstadoCompletado = "COMPLETADO";

        public const string AccionIniciar = "INICIAR";
        public const string AccionCompletar = "COMPLETAR";

        MPPProgreso oMPPPro = new MPPProgreso();
        BLLUsuario oBLLUsu = new BLLUsuario();
        BLLLicencia oBLLLic = new BLLLicencia();
        BLLBitacora oBLLBit = new BLLBitacora();
        BLLActivoPedagogico oBLLAct = new BLLActivoPedagogico();
        BLLEtiqueta oBLLEti = new BLLEtiqueta();

        public List<BECursoConProgreso> ListarPorUsuario(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            oBLLLic.ExigirLicenciaVigente(oUsuarioBE);

            List<BECursoConProgreso> ListaCursoBE = Calcular(oMPPPro.ListarPorUsuario(oUsuarioBE));

            AgregarActivosYClasificacion(ListaCursoBE);

            return ListaCursoBE;
        }

        private void AgregarActivosYClasificacion(List<BECursoConProgreso> ListaCursoBE)
        {
            List<BECurso> ListaSoloCursoBE = new List<BECurso>();

            foreach (BECursoConProgreso oCursoBE in ListaCursoBE)
            {
                oCursoBE.Activos = oBLLAct.ListarPublicados(oCursoBE.Curso.CursoId);
                ListaSoloCursoBE.Add(oCursoBE.Curso);
            }

            oBLLEti.AgregarACursos(ListaSoloCursoBE);
        }

        public List<BEProgresoUsuario> ListarPorEmpresa(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BEEmpresa oEmpresaBE = new BEEmpresa();
            oEmpresaBE.EmpresaId = oUsuarioBE.EmpresaId;

            List<BEProgresoUsuario> ListaUsuarioBE = oMPPPro.ListarPorEmpresa(oEmpresaBE);

            if (ListaUsuarioBE == null)
            {
                return new List<BEProgresoUsuario>();
            }

            foreach (BEProgresoUsuario oUsuario in ListaUsuarioBE)
            {
                Calcular(oUsuario.Cursos);
            }

            return ListaUsuarioBE;
        }

        public List<BECursoConProgreso> Guardar(BEGuardarProgreso Objeto, BESesion oSesionBE)
        {
            string accion = Objeto.Accion == null ? string.Empty : Objeto.Accion.Trim().ToUpperInvariant();

            if (accion != AccionIniciar && accion != AccionCompletar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La acción tiene que ser " + AccionIniciar + " o " + AccionCompletar + ".");
            }

            List<BECursoConProgreso> ListaCursoBE = ListarPorUsuario(oSesionBE);
            BECursoConProgreso oCurso = null;
            BEModuloConProgreso oModulo = null;

            foreach (BECursoConProgreso oItemCurso in ListaCursoBE)
            {
                foreach (BEModuloConProgreso oItemModulo in oItemCurso.Modulos)
                {
                    if (oItemModulo.Modulo.ModuloId == Objeto.ModuloId)
                    {
                        oCurso = oItemCurso;
                        oModulo = oItemModulo;
                    }
                }
            }

            if (oModulo == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El módulo no existe o no está disponible.");
            }

            BEProgreso oActualBE = oModulo.Progreso;
            DateTime ahora = DateTime.Now;

            if (accion == AccionIniciar && oActualBE != null)
            {
                return ListaCursoBE;
            }

            if (accion == AccionCompletar && oActualBE != null && oActualBE.Estado == EstadoCompletado)
            {
                return ListaCursoBE;
            }

            BEProgreso oProgresoBE = accion == AccionIniciar
                ? new BEProgreso(0, oSesionBE.UsuarioId, Objeto.ModuloId, 0m, ahora, null, EstadoEnCurso)
                : new BEProgreso(
                    oActualBE == null ? 0 : oActualBE.ProgresoId,
                    oSesionBE.UsuarioId,
                    Objeto.ModuloId,
                    100m,
                    oActualBE == null || !oActualBE.FechaInicio.HasValue ? ahora : oActualBE.FechaInicio,
                    ahora,
                    EstadoCompletado);

            oMPPPro.Guardar(oProgresoBE);

            Registrar(
                oSesionBE.UsuarioId,
                accion == AccionIniciar ? "ModuloIniciado" : "ModuloCompletado",
                "Módulo " + oModulo.Modulo.Nombre + " del curso " + oCurso.Curso.Nombre + ".");

            List<BECursoConProgreso> ListaActualizadaBE = ListarPorUsuario(oSesionBE);

            foreach (BECursoConProgreso oItemCurso in ListaActualizadaBE)
            {
                if (oItemCurso.Curso.CursoId == oCurso.Curso.CursoId
                    && oItemCurso.Estado == EstadoCompletado
                    && oCurso.Estado != EstadoCompletado)
                {
                    Registrar(oSesionBE.UsuarioId, "CursoCompletado", "Curso " + oCurso.Curso.Nombre + " completado.");
                }
            }

            return ListaActualizadaBE;
        }

        private List<BECursoConProgreso> Calcular(List<BECursoConProgreso> ListaCursoBE)
        {
            if (ListaCursoBE == null)
            {
                return new List<BECursoConProgreso>();
            }

            foreach (BECursoConProgreso oCurso in ListaCursoBE)
            {
                int total = oCurso.Modulos.Count;
                int iniciados = 0;
                int completados = 0;
                DateTime? ultimaActividad = null;

                foreach (BEModuloConProgreso oModulo in oCurso.Modulos)
                {
                    if (oModulo.Progreso == null)
                    {
                        continue;
                    }

                    iniciados++;

                    if (oModulo.Progreso.Estado == EstadoCompletado)
                    {
                        completados++;
                    }

                    DateTime? fecha = oModulo.Progreso.FechaFinalizacion ?? oModulo.Progreso.FechaInicio;

                    if (fecha.HasValue && (!ultimaActividad.HasValue || fecha.Value > ultimaActividad.Value))
                    {
                        ultimaActividad = fecha;
                    }
                }

                oCurso.PorcentajeAvance = total == 0 ? 0m : Math.Round(completados * 100m / total, 0);
                oCurso.UltimaActividad = ultimaActividad;

                if (iniciados == 0)
                {
                    oCurso.Estado = EstadoNoIniciado;
                }
                else if (completados == total)
                {
                    oCurso.Estado = EstadoCompletado;
                }
                else
                {
                    oCurso.Estado = EstadoEnCurso;
                }
            }

            return ListaCursoBE;
        }

        private void Registrar(int usuarioId, string accion, string descripcion)
        {
            oBLLBit.Guardar(new BEBitacoraEvento(
                0, usuarioId, DateTime.Now, BLLBitacora.ModuloCurso, accion, descripcion,
                BLLBitacora.NivelInformativo));
        }

        public void Validar(BEGuardarProgreso Objeto)
        {
            ReglasNegocio.ExigirRango(Objeto.ModuloId, 1, int.MaxValue, "Elegí un módulo.");

            ReglasNegocio.ExigirTexto(Objeto.Accion, "Falta la acción sobre el módulo.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Accion, 20, "La acción no es válida.");
        }
    }

    public static class Sectores
    {
        public static readonly string[] Validos = { "TURISMO", "GASTRONOMIA", "IT" };

        public static string Resolver(string sector)
        {
            if (string.IsNullOrWhiteSpace(sector))
            {
                return null;
            }

            foreach (string valido in Validos)
            {
                if (string.Equals(sector.Trim(), valido, StringComparison.OrdinalIgnoreCase))
                {
                    return valido;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "El sector debe ser " + string.Join(", ", Validos) + ".");
        }
    }
}
