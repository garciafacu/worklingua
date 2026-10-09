using Microsoft.Extensions.FileProviders;
using worklingua.Server.BLL;
using worklingua.Server.Middleware;
using worklingua.Server.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

Configuracion.Inicializar(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepcionesNegocio>();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

ValidarConfiguracion();
SembrarSuperAdmin();

app.UseExceptionHandler();

// Los activos pedagógicos los sube el Backoffice en tiempo de ejecución, así
// que viven fuera de wwwroot y los sirve este middleware. MapStaticAssets solo
// conoce lo que estaba al compilar.
ServicioArchivos.AsegurarCarpeta();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(ServicioArchivos.RutaFisicaActivos()),
    RequestPath = "/" + ServicioArchivos.CarpetaActivos
});

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

static void SembrarSuperAdmin()
{
    BLLInstalacion oBLLIns = new BLLInstalacion();

    if (!Configuracion.SuperAdminConfigurado)
    {
        oBLLIns.AsegurarSuperAdmin();

        return;
    }

    try
    {
        oBLLIns.AsegurarSuperAdmin();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            "No se pudo asegurar la cuenta de Administrador de Plataforma configurada en " +
            "'SuperAdmin:Email'. Revisar que docs/seed.sql esté ejecutado " +
            "contra WorkLinguaDB. Detalle: " + ex.Message, ex);
    }
}

static void ValidarConfiguracion()
{
    if (string.IsNullOrWhiteSpace(Configuracion.CadenaConexion))
    {
        throw new InvalidOperationException(
            "Falta la connection string 'ConnectionStrings:WorkLingua'. Definirla en " +
            "worklingua.Server/appsettings.Local.json, con la estructura de secciones que " +
            "documenta appsettings.json.");
    }

    if (string.IsNullOrWhiteSpace(Configuracion.UrlBaseAplicacion))
    {
        throw new InvalidOperationException(
            "Falta 'UrlBaseAplicacion'. Es la base de los enlaces que se envían por correo.");
    }

    if (string.IsNullOrWhiteSpace(Configuracion.ContactoCorreoDestino))
    {
        throw new InvalidOperationException(
            "Falta 'Contacto:CorreoDestino'. Es la casilla que recibe las consultas del formulario de contacto.");
    }

    if (!Configuracion.ReCaptchaConfigurado)
    {
        throw new InvalidOperationException(
            "Falta la configuración de reCAPTCHA: 'ReCaptcha:SecretKey' o "  +
            "'ReCaptcha:UrlServicioWeb'. La Secret Key va en " +
            "worklingua.Server/appsettings.Local.json; appsettings.json documenta la " +
            "estructura con el valor vacío. Sin ella el registro público quedaría sin " +
            "protección contra bots.");
    }

    if (!Configuracion.CifradoConfigurado)
    {
        throw new InvalidOperationException(
            "Falta la clave de cifrado simétrico 'Cifrado:Clave', o no es una cadena Base64 de " +
            ServicioCifrado.LongitudClaveBytes + " bytes. Va en " +
            "worklingua.Server/appsettings.Local.json. Sin ella no se puede leer ni escribir el " +
            "documento de los usuarios.");
    }

    if (!Configuracion.SmtpConfigurado)
    {
        ServicioLog.Advertencia(
            "SMTP no configurado: falta 'Email:Host' o 'Email:Remitente'. " +
            "Todo intento de enviar un correo va a fallar hasta configurarlo.");

        return;
    }

    ServicioLog.Informacion(
        "SMTP activo: " + Configuracion.EmailHost + ":" + Configuracion.EmailPuerto +
        " (SSL " + (Configuracion.EmailUsarSsl ? "sí" : "no") +
        ", autenticación " + (Configuracion.UsaAutenticacionEmail ? "sí" : "no") + ").");
}
