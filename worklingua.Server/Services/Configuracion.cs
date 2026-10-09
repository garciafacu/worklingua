namespace worklingua.Server.Services
{
    public static class Configuracion
    {
        public static string CadenaConexion { get; private set; }
        public static string UrlBaseAplicacion { get; private set; }
        public static string RutaContenido { get; private set; }
        public static string ContactoCorreoDestino { get; private set; }

        public static string RolPorDefecto { get; private set; }
        public static string PlanPorDefecto { get; private set; }
        public static int HorasVigenciaConfirmacion { get; private set; }
        public static int HorasVigenciaRecupero { get; private set; }
        public static int HorasVigenciaInvitacion { get; private set; }
        public static int LongitudMinimaClave { get; private set; }

        public static string ReCaptchaUrlServicioWeb { get; private set; }
        public static string ReCaptchaSecretKey { get; private set; }

        public static string CifradoClave { get; private set; }

        public static string SuperAdminEmail { get; private set; }
        public static string SuperAdminClave { get; private set; }
        public static string SuperAdminNombre { get; private set; }
        public static string SuperAdminApellido { get; private set; }
        public static string SuperAdminRol { get; private set; }
        public static string SuperAdminEmpresaCuit { get; private set; }

        /// <summary>
        /// Rol cuyos usuarios necesitan una licencia para entrar a sus cursos
        /// (CU-001-005). Quien administra la empresa no consume cupo.
        /// </summary>
        public static string LicenciasRolAlcanzado { get; private set; }

        /// <summary>Fallos consecutivos que bloquean la cuenta (CU-003-004).</summary>
        public static int IntentosMaximos { get; private set; }

        /// <summary>Cuánto dura el bloqueo antes de liberarse solo.</summary>
        public static int MinutosBloqueo { get; private set; }

        public static string EmailRemitente { get; private set; }
        public static string EmailNombreRemitente { get; private set; }
        public static string EmailHost { get; private set; }
        public static int EmailPuerto { get; private set; }
        public static bool EmailUsarSsl { get; private set; }
        public static string EmailUsuario { get; private set; }
        public static string EmailPassword { get; private set; }

        public static decimal LimiteCuentaCorriente { get; private set; }
        public static decimal PorcentajeRecargoCuentaCorriente { get; private set; }
        public static int DiasVencimientoFactura { get; private set; }

        public static void Inicializar(IConfiguration configuracion, string rutaContenido)
        {
            CadenaConexion = configuracion.GetConnectionString("WorkLingua");
            UrlBaseAplicacion = configuracion["UrlBaseAplicacion"];
            RutaContenido = rutaContenido;

            IConfigurationSection contacto = configuracion.GetSection("Contacto");
            ContactoCorreoDestino = LeerTexto(contacto, "CorreoDestino", string.Empty);

            IConfigurationSection registro = configuracion.GetSection("Registro");
            RolPorDefecto = LeerTexto(registro, "RolPorDefecto", "Administrador Empresa");
            PlanPorDefecto = LeerTexto(registro, "PlanPorDefecto", "Free");
            HorasVigenciaConfirmacion = LeerEntero(registro, "HorasVigenciaConfirmacion", 24);
            HorasVigenciaRecupero = LeerEntero(registro, "HorasVigenciaRecupero", 1);
            HorasVigenciaInvitacion = LeerEntero(registro, "HorasVigenciaInvitacion", 72);
            LongitudMinimaClave = LeerEntero(registro, "LongitudMinimaClave", 8);

            IConfigurationSection reCaptcha = configuracion.GetSection("ReCaptcha");
            ReCaptchaUrlServicioWeb = LeerTexto(reCaptcha, "UrlServicioWeb", string.Empty);
            ReCaptchaSecretKey = LeerTexto(reCaptcha, "SecretKey", string.Empty);

            IConfigurationSection cifrado = configuracion.GetSection("Cifrado");
            CifradoClave = LeerTexto(cifrado, "Clave", string.Empty);

            IConfigurationSection superAdmin = configuracion.GetSection("SuperAdmin");
            SuperAdminEmail = LeerTexto(superAdmin, "Email", string.Empty);
            SuperAdminClave = LeerTexto(superAdmin, "Clave", string.Empty);
            SuperAdminNombre = LeerTexto(superAdmin, "Nombre", "Administrador");
            SuperAdminApellido = LeerTexto(superAdmin, "Apellido", "de Plataforma");
            SuperAdminRol = LeerTexto(superAdmin, "Rol", "Administrador de Plataforma");
            SuperAdminEmpresaCuit = LeerTexto(superAdmin, "EmpresaCuit", "30710000000");

            IConfigurationSection licencias = configuracion.GetSection("Licencias");
            LicenciasRolAlcanzado = LeerTexto(licencias, "RolAlcanzado", "Empleado");

            IConfigurationSection seguridad = configuracion.GetSection("Seguridad");
            IntentosMaximos = LeerEntero(seguridad, "IntentosMaximos", 5);
            MinutosBloqueo = LeerEntero(seguridad, "MinutosBloqueo", 15);

            IConfigurationSection email = configuracion.GetSection("Email");
            EmailRemitente = LeerTexto(email, "Remitente", string.Empty);
            EmailNombreRemitente = LeerTexto(email, "NombreRemitente", "WorkLingua");
            EmailHost = LeerTexto(email, "Host", string.Empty);
            EmailPuerto = LeerEntero(email, "Puerto", 587);
            EmailUsarSsl = LeerBooleano(email, "UsarSsl", true);
            EmailUsuario = LeerTexto(email, "Usuario", string.Empty);
            EmailPassword = LeerTexto(email, "Password", string.Empty);

            IConfigurationSection contratacion = configuracion.GetSection("Contratacion");
            LimiteCuentaCorriente = LeerDecimal(contratacion, "LimiteCuentaCorriente", 150000m);
            PorcentajeRecargoCuentaCorriente = LeerDecimal(contratacion, "PorcentajeRecargoCuentaCorriente", 10m);
            DiasVencimientoFactura = LeerEntero(contratacion, "DiasVencimientoFactura", 30);
        }

        public static bool SuperAdminConfigurado
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SuperAdminEmail)
                    && !string.IsNullOrWhiteSpace(SuperAdminClave);
            }
        }

        public static bool ReCaptchaConfigurado
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ReCaptchaSecretKey)
                    && !string.IsNullOrWhiteSpace(ReCaptchaUrlServicioWeb);
            }
        }

        public static bool CifradoConfigurado
        {
            get { return ServicioCifrado.DecodificarClave(CifradoClave) != null; }
        }

        public static bool SmtpConfigurado
        {
            get
            {
                return !string.IsNullOrWhiteSpace(EmailHost)
                    && !string.IsNullOrWhiteSpace(EmailRemitente);
            }
        }

        public static bool UsaAutenticacionEmail
        {
            get { return !string.IsNullOrWhiteSpace(EmailUsuario); }
        }

        private static string LeerTexto(IConfigurationSection seccion, string clave, string valorPorDefecto)
        {
            string valor = seccion[clave];

            return string.IsNullOrWhiteSpace(valor) ? valorPorDefecto : valor;
        }

        private static int LeerEntero(IConfigurationSection seccion, string clave, int valorPorDefecto)
        {
            int resultado;

            return int.TryParse(seccion[clave], out resultado) ? resultado : valorPorDefecto;
        }

        private static decimal LeerDecimal(IConfigurationSection seccion, string clave, decimal valorPorDefecto)
        {
            decimal resultado;

            return decimal.TryParse(
                seccion[clave],
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out resultado)
                ? resultado
                : valorPorDefecto;
        }

        private static bool LeerBooleano(IConfigurationSection seccion, string clave, bool valorPorDefecto)
        {
            bool resultado;

            return bool.TryParse(seccion[clave], out resultado) ? resultado : valorPorDefecto;
        }
    }
}
