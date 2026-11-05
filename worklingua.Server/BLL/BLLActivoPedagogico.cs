using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLActivoPedagogico
    {
        public const string EstadoBorrador = "BORRADOR";
        public const string EstadoPublicado = "PUBLICADO";

        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;

        public const int MegabytesMaximos = 10;

        private static readonly Dictionary<string, string[]> ExtensionesPorTipo =
            new Dictionary<string, string[]>
            {
                { "IMAGEN", new[] { ".png", ".jpg", ".jpeg", ".webp" } },
                { "AUDIO", new[] { ".mp3", ".wav", ".ogg" } },
                { "VOCABULARIO", new[] { ".pdf", ".csv", ".txt" } }
            };

        MPPActivoPedagogico oMPPAct = new MPPActivoPedagogico();
        MPPModulo oMPPMod = new MPPModulo();
        BLLCurso oBLLCur = new BLLCurso();

        public List<BEActivoPedagogico> ListarPorCurso(BEActivoPedagogico Objeto, BESesion oSesionBE)
        {
            ExigirCursoVisible(Objeto.CursoId, oSesionBE);

            return oMPPAct.ListarPorCurso(Objeto);
        }

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

        public BEActivoPedagogico Guardar(
            BEActivoPedagogico Objeto, BEArchivoSubido oArchivoBE, bool confirmarNombre, BESesion oSesionBE)
        {
            BECurso oCursoBE = ExigirCursoGestionable(Objeto.CursoId, oSesionBE);

            if (!oCursoBE.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "El curso está dado de baja.");
            }

            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.Descripcion = ServicioTexto.Normalizar(Objeto.Descripcion);
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

        private string SugerirNombre(int cursoId, string nombre)
        {
            return NombresConVersion.Sugerir(nombre, NombresVigentes(cursoId), LongitudMaximaNombre);
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

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
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
    }

    public static class NombresConVersion
    {
        public static string Sugerir(string nombre, ICollection<string> ocupados, int largoMaximo)
        {
            int version = 2;

            while (true)
            {
                string candidato = Recortar(nombre + " Versión " + version, largoMaximo);

                if (!ocupados.Contains(candidato.ToUpperInvariant()))
                {
                    return candidato;
                }

                version++;
            }
        }

        private static string Recortar(string texto, int largoMaximo)
        {
            return texto.Length <= largoMaximo ? texto : texto.Substring(0, largoMaximo);
        }
    }
}
