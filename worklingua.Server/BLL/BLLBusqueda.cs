using System.Collections.Generic;
using System.Globalization;
using System.Text;
using worklingua.Server.BE;

namespace worklingua.Server.BLL
{
    public class BLLBusqueda
    {
        public const string AreaPublica = "Publica";
        public const string AreaPrivada = "Privada";

        public const string OrdenRelevancia = "Relevancia";
        public const string OrdenNombreAscendente = "NombreAsc";
        public const string OrdenNombreDescendente = "NombreDesc";

        private const int LongitudMaximaTermino = 200;
        private const int LongitudMinimaPalabra = 3;
        private const int CantidadMaximaPalabras = 10;

        private const string SeccionSitioPublico = "busqueda.seccion.sitioPublico";
        private const string SeccionAcceso = "busqueda.seccion.acceso";
        private const string SeccionCuenta = "layout.menu.grupo.cuenta";
        private const string SeccionMiWorklingua = "layout.menu.grupo.miWorklingua";
        private const string SeccionAdministracion = "layout.menu.grupo.administracion";
        private const string SeccionComunicacion = "layout.menu.grupo.comunicacion";
        private const string SeccionSeguridad = "layout.menu.grupo.seguridad";

        static readonly List<BEPaginaBuscable> Catalogo = new List<BEPaginaBuscable>
        {
            Publica("/", "layout.publico.inicio", "inicio", SeccionSitioPublico, 10),
            Publica("/catalogo", "layout.publico.catalogo", "catalogo", SeccionSitioPublico, 20),
            Publica("/novedades", "layout.publico.novedades", "novedades", SeccionSitioPublico, 30),
            Publica("/institucional", "layout.publico.institucional", "institucional", SeccionSitioPublico, 40),
            Publica("/preguntas-frecuentes", "layout.publico.preguntasFrecuentes", "preguntasFrecuentes", SeccionSitioPublico, 50),
            Publica("/contacto", "layout.publico.contacto", "contacto", SeccionSitioPublico, 60),
            Publica("/terminos", "legal.terminos.titulo", "terminos", SeccionSitioPublico, 70),
            Publica("/privacidad", "legal.privacidad.titulo", "privacidad", SeccionSitioPublico, 80),

            Publica("/login", "auth.login.titulo", "login", SeccionAcceso, 110),
            Publica("/registro", "auth.registro.titulo", "registro", SeccionAcceso, 120),
            Publica("/recuperar-clave", "auth.recuperar.titulo", "recuperarClave", SeccionAcceso, 130),

            Privada("/inicio", "layout.menu.panel", "perfil", SeccionCuenta, null, 210),
            Privada("/inicio/cambiar-clave", "layout.menu.miCuenta", "cambiarClave", SeccionCuenta, null, 220),

            Privada("/inicio/admin/cursos", "layout.menu.cursos", "cursos", SeccionMiWorklingua, Permisos.CursoListar, 310),
            Privada("/inicio/opiniones", "layout.menu.opiniones", "opiniones", SeccionMiWorklingua, null, 320),

            Privada("/inicio/admin/usuarios", "layout.menu.usuarios", "usuarios", SeccionAdministracion, Permisos.UsuarioListar, 410),
            Privada("/inicio/admin/empresas", "layout.menu.empresas", "empresas", SeccionAdministracion, Permisos.EmpresaListar, 420),
            Privada("/inicio/admin/planes", "layout.menu.planes", "planes", SeccionAdministracion, Permisos.PlanListar, 430),
            Privada("/inicio/admin/idiomas", "layout.menu.idiomas", "idiomas", SeccionAdministracion, Permisos.IdiomaListar, 440),
            Privada("/inicio/admin/culturas", "layout.menu.culturas", "culturas", SeccionAdministracion, Permisos.CulturaListar, 450),
            Privada("/inicio/admin/traducciones", "layout.menu.traducciones", "traducciones", SeccionAdministracion, Permisos.TraduccionListar, 460),

            Privada("/inicio/admin/noticias", "layout.menu.noticias", "noticias", SeccionComunicacion, Permisos.NoticiaListar, 510),
            Privada("/inicio/admin/newsletter", "layout.menu.newsletter", "newsletter", SeccionComunicacion, Permisos.NewsletterListar, 520),

            Privada("/inicio/admin/operadores", "layout.menu.operadores", "operadores", SeccionSeguridad, Permisos.OperadorListar, 610),
            Privada("/inicio/admin/roles", "layout.menu.roles", "roles", SeccionSeguridad, Permisos.RolListar, 620),
            Privada("/inicio/admin/permisos", "layout.menu.permisos", "permisos", SeccionSeguridad, Permisos.PermisoListar, 630),
            Privada("/inicio/admin/bitacora", "layout.menu.bitacora", "bitacora", SeccionSeguridad, Permisos.BitacoraListar, 640)
        };

        BLLTraduccion oBLLTra;
        BLLSeguridad oBLLSeg;

        public BLLBusqueda()
        {
            oBLLTra = new BLLTraduccion();
            oBLLSeg = new BLLSeguridad();
        }

        public List<BEPaginaBuscable> Buscar(BEFiltroBusqueda Objeto, BESesion oSesionBE)
        {
            string area = ValidarArea(Objeto.Area);
            string seccion = Normalizar(Objeto.Seccion);
            string ordenarPor = ValidarOrdenarPor(Objeto.OrdenarPor);
            string frase = NormalizarTexto(Acotar(Objeto.Texto));
            List<string> palabras = ObtenerPalabras(frase);

            if (palabras.Count == 0 && area == null && seccion == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Ingresá qué buscar o elegí un criterio.");
            }

            Dictionary<string, string> Textos = oBLLTra.ListarPorCodigo(ResolverIdioma(Objeto.Idioma));
            bool soloTitulo = Objeto.SoloTitulo == true;

            List<BEPaginaBuscable> ListaPaginaBE = new List<BEPaginaBuscable>();
            Dictionary<BEPaginaBuscable, int> Relevancias = new Dictionary<BEPaginaBuscable, int>();
            Dictionary<BEPaginaBuscable, string> Nombres = new Dictionary<BEPaginaBuscable, string>();

            foreach (BEPaginaBuscable oPaginaBE in PaginasAccesibles(oSesionBE))
            {
                if (area != null && oPaginaBE.Area != area)
                {
                    continue;
                }

                if (seccion != null && oPaginaBE.ClaveSeccion != seccion)
                {
                    continue;
                }

                int relevancia = CalcularRelevancia(oPaginaBE, Textos, frase, palabras, soloTitulo);

                if (relevancia > 0)
                {
                    ListaPaginaBE.Add(oPaginaBE);
                    Relevancias.Add(oPaginaBE, relevancia);
                    Nombres.Add(oPaginaBE, NormalizarTexto(Traducir(Textos, oPaginaBE.ClaveTitulo)));
                }
            }

            ListaPaginaBE.Sort((primera, segunda) =>
            {
                int comparacion = Comparar(primera, segunda, ordenarPor, Relevancias, Nombres);

                return comparacion != 0 ? comparacion : primera.Orden.CompareTo(segunda.Orden);
            });

            return ListaPaginaBE;
        }

        private int Comparar(
            BEPaginaBuscable primera,
            BEPaginaBuscable segunda,
            string ordenarPor,
            Dictionary<BEPaginaBuscable, int> Relevancias,
            Dictionary<BEPaginaBuscable, string> Nombres)
        {
            if (ordenarPor == OrdenNombreAscendente)
            {
                return string.CompareOrdinal(Nombres[primera], Nombres[segunda]);
            }

            if (ordenarPor == OrdenNombreDescendente)
            {
                return string.CompareOrdinal(Nombres[segunda], Nombres[primera]);
            }

            return Relevancias[segunda].CompareTo(Relevancias[primera]);
        }

        public List<BESeccionBusquedaRespuesta> ListarSecciones(BESesion oSesionBE)
        {
            List<BESeccionBusquedaRespuesta> ListaSeccionBE = new List<BESeccionBusquedaRespuesta>();
            List<string> claves = new List<string>();

            foreach (BEPaginaBuscable oPaginaBE in PaginasAccesibles(oSesionBE))
            {
                if (!claves.Contains(oPaginaBE.ClaveSeccion))
                {
                    claves.Add(oPaginaBE.ClaveSeccion);
                    ListaSeccionBE.Add(new BESeccionBusquedaRespuesta(oPaginaBE.ClaveSeccion, oPaginaBE.Area));
                }
            }

            return ListaSeccionBE;
        }

        private List<BEPaginaBuscable> PaginasAccesibles(BESesion oSesionBE)
        {
            List<BEPaginaBuscable> ListaPaginaBE = new List<BEPaginaBuscable>();
            bool conSesion = oSesionBE != null && oSesionBE.UsuarioId > 0;
            List<string> permisos = conSesion ? oBLLSeg.ObtenerPermisos(oSesionBE.UsuarioId) : new List<string>();

            foreach (BEPaginaBuscable oPaginaBE in Catalogo)
            {
                if (oPaginaBE.RequiereSesion && !conSesion)
                {
                    continue;
                }

                if (oPaginaBE.Permiso != null && !permisos.Contains(oPaginaBE.Permiso))
                {
                    continue;
                }

                ListaPaginaBE.Add(oPaginaBE);
            }

            return ListaPaginaBE;
        }

        private int CalcularRelevancia(
            BEPaginaBuscable oPaginaBE,
            Dictionary<string, string> Textos,
            string frase,
            List<string> palabras,
            bool soloTitulo)
        {
            if (palabras.Count == 0)
            {
                return 1;
            }

            string titulo = NormalizarTexto(Traducir(Textos, oPaginaBE.ClaveTitulo));
            string contenido = soloTitulo
                ? titulo
                : titulo + " " +
                  NormalizarTexto(Traducir(Textos, oPaginaBE.ClaveSeccion)) + " " +
                  NormalizarTexto(Traducir(Textos, oPaginaBE.ClaveDescripcion)) + " " +
                  NormalizarTexto(Traducir(Textos, oPaginaBE.ClavePalabrasClave));

            if (ContarPresentes(contenido, palabras) != palabras.Count)
            {
                return 0;
            }

            if (titulo.Contains(frase))
            {
                return 3;
            }

            return ContarPresentes(titulo, palabras) == palabras.Count ? 2 : 1;
        }

        private int ContarPresentes(string texto, List<string> palabras)
        {
            int presentes = 0;

            foreach (string palabra in palabras)
            {
                if (texto.Contains(palabra))
                {
                    presentes = presentes + 1;
                }
            }

            return presentes;
        }

        private List<string> ObtenerPalabras(string frase)
        {
            List<string> palabras = new List<string>();
            List<string> cortas = new List<string>();

            if (frase == null)
            {
                return palabras;
            }

            foreach (string palabra in frase.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                List<string> destino = palabra.Length >= LongitudMinimaPalabra ? palabras : cortas;

                if (!destino.Contains(palabra) && destino.Count < CantidadMaximaPalabras)
                {
                    destino.Add(palabra);
                }
            }

            return palabras.Count > 0 ? palabras : cortas;
        }

        private string ResolverIdioma(string idioma)
        {
            string codigo = Normalizar(idioma);

            return codigo == null ? BLLTraduccion.CodigoIdiomaBase : codigo;
        }

        private string ValidarArea(string area)
        {
            string valor = Normalizar(area);

            if (valor == null || valor == AreaPublica || valor == AreaPrivada)
            {
                return valor;
            }

            throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El área elegida no es válida.");
        }

        private string ValidarOrdenarPor(string ordenarPor)
        {
            string valor = Normalizar(ordenarPor);

            if (valor == null)
            {
                return OrdenRelevancia;
            }

            if (valor == OrdenRelevancia || valor == OrdenNombreAscendente || valor == OrdenNombreDescendente)
            {
                return valor;
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "El criterio de orden elegido no es válido.");
        }

        private string Traducir(Dictionary<string, string> Textos, string clave)
        {
            string texto;

            return Textos.TryGetValue(clave, out texto) ? texto : string.Empty;
        }

        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            string descompuesto = texto.Normalize(NormalizationForm.FormD);
            StringBuilder resultado = new StringBuilder();
            bool ultimoFueEspacio = false;

            foreach (char caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(caracter))
                {
                    resultado.Append(char.ToLowerInvariant(caracter));
                    ultimoFueEspacio = false;
                }
                else if (!ultimoFueEspacio && resultado.Length > 0)
                {
                    resultado.Append(' ');
                    ultimoFueEspacio = true;
                }
            }

            return resultado.ToString().Trim();
        }

        private string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }

        private string Acotar(string texto)
        {
            string limpio = Normalizar(texto);

            if (limpio == null)
            {
                return null;
            }

            return limpio.Length <= LongitudMaximaTermino ? limpio : limpio.Substring(0, LongitudMaximaTermino);
        }

        private static BEPaginaBuscable Publica(string ruta, string claveTitulo, string id, string claveSeccion, int orden)
        {
            return new BEPaginaBuscable(
                ruta,
                claveTitulo,
                "busqueda.pagina." + id + ".descripcion",
                claveSeccion,
                "busqueda.pagina." + id + ".palabrasClave",
                AreaPublica,
                false,
                null,
                orden);
        }

        private static BEPaginaBuscable Privada(
            string ruta,
            string claveTitulo,
            string id,
            string claveSeccion,
            string permiso,
            int orden)
        {
            return new BEPaginaBuscable(
                ruta,
                claveTitulo,
                "busqueda.pagina." + id + ".descripcion",
                claveSeccion,
                "busqueda.pagina." + id + ".palabrasClave",
                AreaPrivada,
                true,
                permiso,
                orden);
        }
    }
}
