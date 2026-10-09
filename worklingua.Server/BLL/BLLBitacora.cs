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

        MPPBitacora oMPPBit;

        public BLLBitacora()
        {
            oMPPBit = new MPPBitacora();
        }

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

            return oMPPBit.Buscar(Objeto);
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
    }
}
