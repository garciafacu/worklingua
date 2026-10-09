using System.Collections.Generic;
using System.IO;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// Activos pedagógicos de un curso (CU-004-001): imágenes, audios de
    /// pronunciación y vocabularios.
    ///
    /// El caso de uso habla de modelos 3D para simulaciones inmersivas; sin
    /// motor de Realidad Aumentada que los consuma, el tipo visual que la
    /// plataforma sí puede mostrar es la imagen.
    ///
    /// El caso de uso aloja el archivo en la nube; acá se guarda en disco y la
    /// aplicación lo sirve como archivo estático. No hay conversión de formatos
    /// (camino alternativo 2) ni subida por bloques (camino alternativo 3): un
    /// formato no aceptado se rechaza y el tope de tamaño evita la latencia.
    ///
    /// El alcance lo decide el curso: se reusa BLLCurso.ExigirGestion, así un
    /// curso de otra empresa responde igual que en el ABM de Cursos.
    /// </summary>
    public class BLLActivoPedagogico
    {
        public const string EstadoBorrador = "BORRADOR";
        public const string EstadoPublicado = "PUBLICADO";

        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;

        /// <summary>Tope de tamaño del archivo, en megabytes.</summary>
        public const int MegabytesMaximos = 10;

        /// <summary>
        /// Extensiones aceptadas por tipo de recurso. Es la validación del paso
        /// 6: lo que no entra acá no se sube.
        /// </summary>
        private static readonly Dictionary<string, string[]> ExtensionesPorTipo =
            new Dictionary<string, string[]>
            {
                { "IMAGEN", new[] { ".png", ".jpg", ".jpeg", ".webp" } },
                { "AUDIO", new[] { ".mp3", ".wav", ".ogg" } },
                { "VOCABULARIO", new[] { ".pdf", ".csv", ".txt" } }
            };

        MPPActivoPedagogico oMPPAct;
        MPPModulo oMPPMod;
        BLLCurso oBLLCur;

        public BLLActivoPedagogico()
        {
            oMPPAct = new MPPActivoPedagogico();
            oMPPMod = new MPPModulo();
            oBLLCur = new BLLCurso();
        }

        public List<BEActivoPedagogico> ListarPorCurso(BEActivoPedagogico Objeto, BESesion oSesionBE)
        {
            ExigirCursoVisible(Objeto.CursoId, oSesionBE);

            return oMPPAct.ListarPorCurso(Objeto);
        }

        /// <summary>
        /// Los activos que un alumno puede consumir: publicados y vigentes.
        ///
        /// No valida alcance porque no lo decide: lo llama quien ya resolvió a
        /// qué cursos llega el usuario (Mis cursos, con su licencia ya exigida).
        /// Acá vive una sola vez qué significa "disponible", para que la
        /// pantalla no decida sobre un borrador.
        /// </summary>
        public List<BEActivoPedagogico> ListarPublicados(int cursoId)
        {
            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.CursoId = cursoId;

            List<BEActivoPedagogico> ListaBE = new List<BEActivoPedagogico>();

            foreach (BEActivoPedagogico oActivoBE in oMPPAct.ListarPorCurso(oFiltroBE))
            {
                if (oActivoBE.Activo == true && oActivoBE.Estado == EstadoPublicado)
                {
                    ListaBE.Add(oActivoBE);
                }
            }

            return ListaBE;
        }

        /// <summary>
        /// El escenario principal: valida, resuelve el nombre, guarda el
        /// archivo y registra el activo como borrador (pasos 5 a 13).
        ///
        /// El archivo se escribe DESPUÉS de todas las validaciones: si algo
        /// falla no queda un huérfano en disco.
        /// </summary>
        public BEActivoPedagogico Guardar(
            BEActivoPedagogico Objeto, BEArchivoSubido oArchivoBE, bool confirmarNombre, BESesion oSesionBE)
        {
            BECurso oCursoBE = ExigirCursoGestionable(Objeto.CursoId, oSesionBE);

            if (!oCursoBE.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "El curso está dado de baja.");
            }

            Objeto.Nombre = Normalizar(Objeto.Nombre);
            Objeto.Descripcion = Normalizar(Objeto.Descripcion);
            Objeto.TipoContenido = ResolverTipo(Objeto.TipoContenido);
            Objeto.ModuloId = ResolverModulo(Objeto.ModuloId, Objeto.CursoId);

            ValidarTextos(Objeto);

            string extension = ValidarArchivo(oArchivoBE, Objeto.TipoContenido);

            Objeto.Nombre = ResolverNombre(Objeto, confirmarNombre);

            Objeto.UrlArchivo = ServicioArchivos.Guardar(oArchivoBE.Contenido, extension);
            Objeto.Estado = EstadoBorrador;
            Objeto.Activo = true;
            Objeto.ActivoPedagogicoId = oMPPAct.Alta(Objeto);

            return ObtenerObligatorio(Objeto.ActivoPedagogicoId);
        }

        public BEActivoPedagogico CambiarEstado(BEActivoPedagogico Objeto, BESesion oSesionBE)
        {
            BEActivoPedagogico oActivoBE = ExigirGestionable(Objeto.ActivoPedagogicoId, oSesionBE);

            string estado = ResolverEstado(Objeto.Estado);

            if (oActivoBE.Activo != true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "El activo está dado de baja.");
            }

            if (oActivoBE.Estado == estado)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    estado == EstadoPublicado
                        ? "El activo ya está publicado."
                        : "El activo ya está en borrador.");
            }

            Objeto.Estado = estado;
            oMPPAct.CambiarEstado(Objeto);

            return ObtenerObligatorio(Objeto.ActivoPedagogicoId);
        }

        /// <summary>
        /// Baja lógica y reactivación. Reactivar vuelve a ocupar el nombre, así
        /// que se valida de nuevo contra el índice único del curso.
        /// </summary>
        public BEActivoPedagogico CambiarActivo(BEActivoPedagogico Objeto, BESesion oSesionBE)
        {
            BEActivoPedagogico oActivoBE = ExigirGestionable(Objeto.ActivoPedagogicoId, oSesionBE);

            bool activar = Objeto.Activo.HasValue && Objeto.Activo.Value;

            if (oActivoBE.Activo == activar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    activar ? "El activo ya está vigente." : "El activo ya está dado de baja.");
            }

            if (activar && ExisteNombre(oActivoBE.CursoId, oActivoBE.Nombre, oActivoBE.ActivoPedagogicoId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Ya existe un activo con ese nombre en el curso.");
            }

            oMPPAct.CambiarActivo(Objeto);

            return ObtenerObligatorio(Objeto.ActivoPedagogicoId);
        }

        /// <summary>
        /// Camino alternativo 1: con el nombre ocupado se propone una
        /// nomenclatura y se corta, salvo que el usuario ya la haya aceptado.
        /// </summary>
        private string ResolverNombre(BEActivoPedagogico Objeto, bool confirmarNombre)
        {
            if (!ExisteNombre(Objeto.CursoId, Objeto.Nombre, 0))
            {
                return Objeto.Nombre;
            }

            string sugerido = SugerirNombre(Objeto.CursoId, Objeto.Nombre);

            if (confirmarNombre)
            {
                return sugerido;
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Conflicto,
                "El nombre ya existe. ¿Desea guardarlo como " + sugerido + "?",
                sugerido);
        }

        /// <summary>
        /// "Nombre" ocupado pasa a "Nombre Versión 2", y si esa también está
        /// ocupada sigue con la 3. Busca sobre la lista del curso, que ya está
        /// en memoria, en vez de consultar una vez por intento.
        /// </summary>
        private string SugerirNombre(int cursoId, string nombre)
        {
            List<string> ocupados = NombresVigentes(cursoId);
            int version = 2;

            while (true)
            {
                string candidato = Recortar(nombre + " Versión " + version);

                if (!ocupados.Contains(candidato.ToUpperInvariant()))
                {
                    return candidato;
                }

                version++;
            }
        }

        private bool ExisteNombre(int cursoId, string nombre, int excluirId)
        {
            foreach (BEActivoPedagogico oActivoBE in ListarVigentes(cursoId))
            {
                if (oActivoBE.ActivoPedagogicoId != excluirId &&
                    string.Equals(oActivoBE.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> NombresVigentes(int cursoId)
        {
            List<string> nombres = new List<string>();

            foreach (BEActivoPedagogico oActivoBE in ListarVigentes(cursoId))
            {
                nombres.Add(oActivoBE.Nombre.ToUpperInvariant());
            }

            return nombres;
        }

        /// <summary>
        /// Los activos que ocupan nombre. Los dados de baja no cuentan: el
        /// índice único del curso también los deja afuera.
        /// </summary>
        private List<BEActivoPedagogico> ListarVigentes(int cursoId)
        {
            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.CursoId = cursoId;

            List<BEActivoPedagogico> ListaBE = new List<BEActivoPedagogico>();

            foreach (BEActivoPedagogico oActivoBE in oMPPAct.ListarPorCurso(oFiltroBE))
            {
                if (oActivoBE.Activo == true)
                {
                    ListaBE.Add(oActivoBE);
                }
            }

            return ListaBE;
        }

        /// <summary>Paso 6: obligatorios, peso y formato. Devuelve la extensión.</summary>
        private string ValidarArchivo(BEArchivoSubido oArchivoBE, string tipoContenido)
        {
            if (oArchivoBE == null || oArchivoBE.Contenido == null || oArchivoBE.Contenido.Length == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Adjuntá el archivo del activo.");
            }

            if (oArchivoBE.Contenido.Length > MegabytesMaximos * 1024 * 1024)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El archivo no puede superar los " + MegabytesMaximos + " MB.");
            }

            string extension = Path.GetExtension(oArchivoBE.NombreOriginal ?? string.Empty).ToLowerInvariant();
            string[] permitidas = ExtensionesPorTipo[tipoContenido];

            if (Array.IndexOf(permitidas, extension) < 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El formato del archivo no es compatible. Para ese tipo de recurso se aceptan: " +
                    string.Join(", ", permitidas) + ".");
            }

            return extension;
        }

        private void ValidarTextos(BEActivoPedagogico Objeto)
        {
            if (string.IsNullOrEmpty(Objeto.Nombre) || Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El título del activo es obligatorio y no puede superar los " +
                    LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }
        }

        private string ResolverTipo(string tipoContenido)
        {
            string tipo = (tipoContenido ?? string.Empty).Trim().ToUpperInvariant();

            if (!ExtensionesPorTipo.ContainsKey(tipo))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Elegí el tipo de recurso.");
            }

            return tipo;
        }

        /// <summary>
        /// Dónde se muestra el activo: sin módulo es material general del curso
        /// y con módulo es el contenido de esa lección.
        ///
        /// El módulo tiene que ser de este curso y estar vigente; si no, el
        /// activo quedaría colgando de una lección que el alumno no ve.
        /// </summary>
        private int? ResolverModulo(int? moduloId, int cursoId)
        {
            if (!moduloId.HasValue || moduloId.Value == 0)
            {
                return null;
            }

            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = cursoId;

            List<BEModulo> ListaModuloBE = oMPPMod.ListarPorCurso(oFiltroBE) ?? new List<BEModulo>();

            foreach (BEModulo oModuloBE in ListaModuloBE)
            {
                if (oModuloBE.ModuloId == moduloId.Value)
                {
                    return moduloId;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "El módulo elegido no pertenece al curso.");
        }

        private string ResolverEstado(string estado)
        {
            string valor = (estado ?? string.Empty).Trim().ToUpperInvariant();

            if (valor != EstadoBorrador && valor != EstadoPublicado)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El estado del activo no es válido.");
            }

            return valor;
        }

        /// <summary>Un activo de un curso que la sesión no puede gestionar no existe.</summary>
        private BEActivoPedagogico ExigirGestionable(int activoPedagogicoId, BESesion oSesionBE)
        {
            BEActivoPedagogico oActivoBE = ObtenerObligatorio(activoPedagogicoId);

            ExigirCursoGestionable(oActivoBE.CursoId, oSesionBE);

            return oActivoBE;
        }

        private BEActivoPedagogico ObtenerObligatorio(int activoPedagogicoId)
        {
            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.ActivoPedagogicoId = activoPedagogicoId;

            BEActivoPedagogico oActivoBE = oMPPAct.ListarObjeto(oFiltroBE);

            if (oActivoBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El activo no existe.");
            }

            return oActivoBE;
        }

        private BECurso ExigirCursoGestionable(int cursoId, BESesion oSesionBE)
        {
            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = cursoId;

            return oBLLCur.ExigirGestion(oFiltroBE, oSesionBE);
        }

        private void ExigirCursoVisible(int cursoId, BESesion oSesionBE)
        {
            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = cursoId;

            oBLLCur.ListarObjeto(oFiltroBE, oSesionBE);
        }

        private string Normalizar(string valor)
        {
            if (valor == null)
            {
                return null;
            }

            string limpio = valor.Trim();

            return limpio.Length == 0 ? null : limpio;
        }

        private string Recortar(string texto)
        {
            return texto.Length <= LongitudMaximaNombre
                ? texto
                : texto.Substring(0, LongitudMaximaNombre);
        }
    }
}
