using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLBitacora
    {
        public const string ModuloAutenticacion = "Autenticacion";
        public const string ModuloUsuario = "Usuario";
        public const string ModuloComentario = "Comentario";
        public const string ModuloContacto = "Contacto";
        public const string ModuloSeguridad = "Seguridad";
        public const string ModuloPlan = "Plan";
        public const string ModuloCurso = "Curso";
        public const string ModuloCaracteristica = "Caracteristica";
        public const string ModuloIdioma = "Idioma";
        public const string ModuloEmpresa = "Empresa";
        public const string ModuloTraduccion = "Traduccion";
        public const string ModuloCultura = "Cultura";
        public const string ModuloInstalacion = "Instalacion";
        public const string ModuloSuscripcion = "Suscripcion";
        public const string ModuloRol = "Rol";
        public const string ModuloPermiso = "Permiso";
        public const string ModuloOperador = "Operador";
        public const string ModuloNoticia = "Noticia";
        public const string ModuloNewsletter = "Newsletter";
        public const string ModuloSoporte = "Soporte";
        public const string ModuloOferta = "Oferta";
        public const string ModuloCuentaCorriente = "CuentaCorriente";
        public const string ModuloEncuesta = "Encuesta";
        public const string ModuloDepartamento = "Departamento";
        public const string ModuloLicencia = "Licencia";
        public const string ModuloAlerta = "Alerta";
        public const string ModuloActivo = "ActivoPedagogico";

        public const string NivelInformativo = "INFO";
        public const string NivelAdvertencia = "WARN";
        public const string NivelError = "ERROR";

        private const int TamanioPaginaPorDefecto = 25;
        private const int TamanioPaginaMaximo = 100;
        private const int LargoMaximoTexto = 200;

        MPPBitacora oMPPBit = new MPPBitacora();

        public void Guardar(BEBitacoraEvento Objeto)
        {
            try
            {
                oMPPBit.Guardar(Objeto);
            }
            catch (Exception ex)
            {
                ServicioLog.Error(
                    "No se pudo registrar en bitácora el evento " + Objeto.Modulo + "/" + Objeto.Accion + ".", ex);
            }
        }

        public BEPaginaBitacora Buscar(BEFiltroBitacora Objeto)
        {
            Normalizar(Objeto);

            BEPaginaBitacora oPaginaBE = oMPPBit.Buscar(Objeto);

            oPaginaBE.TotalPaginas =
                (oPaginaBE.TotalRegistros + oPaginaBE.TamanioPagina - 1) / oPaginaBE.TamanioPagina;

            return oPaginaBE;
        }

        private void Normalizar(BEFiltroBitacora Objeto)
        {
            Objeto.Texto = Recortar(Objeto.Texto);
            Objeto.Usuario = Recortar(Objeto.Usuario);
            Objeto.Modulo = Recortar(Objeto.Modulo);
            Objeto.Accion = Recortar(Objeto.Accion);
            Objeto.Nivel = ValidarNivel(Objeto.Nivel);

            ValidarRangoDeFechas(Objeto.Desde, Objeto.Hasta);

            if (!Objeto.Pagina.HasValue || Objeto.Pagina.Value < 1)
            {
                Objeto.Pagina = 1;
            }

            if (!Objeto.TamanioPagina.HasValue || Objeto.TamanioPagina.Value < 1)
            {
                Objeto.TamanioPagina = TamanioPaginaPorDefecto;
            }

            if (Objeto.TamanioPagina > TamanioPaginaMaximo)
            {
                Objeto.TamanioPagina = TamanioPaginaMaximo;
            }
        }

        private string Recortar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            string limpio = valor.Trim();

            return limpio.Length <= LargoMaximoTexto
                ? limpio
                : limpio.Substring(0, LargoMaximoTexto);
        }

        private string ValidarNivel(string nivel)
        {
            string limpio = Recortar(nivel);

            if (limpio == null)
            {
                return null;
            }

            string normalizado = limpio.ToUpperInvariant();

            if (normalizado != NivelInformativo
                && normalizado != NivelAdvertencia
                && normalizado != NivelError)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nivel tiene que ser " + NivelInformativo + ", " + NivelAdvertencia
                        + " o " + NivelError + ".");
            }

            return normalizado;
        }

        private void ValidarRangoDeFechas(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }
        }

        public void Validar(BEFiltroBitacora Objeto)
        {
            ReglasNegocio.ExigirLargoMaximo(Objeto.Texto, 200,
                "El término de búsqueda no puede superar los 200 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Usuario, 200,
                "El usuario buscado no puede superar los 200 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Modulo, 80, "El módulo no puede superar los 80 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Accion, 120, "La acción no puede superar los 120 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Nivel, 20, "El nivel no puede superar los 20 caracteres.");

            ReglasNegocio.ExigirRango(Objeto.Pagina, 1, int.MaxValue, "La página tiene que ser mayor o igual a 1.");

            ReglasNegocio.ExigirRango(Objeto.TamanioPagina, 1, 100,
                "El tamaño de página tiene que estar entre 1 y 100.");
        }
    }
}
