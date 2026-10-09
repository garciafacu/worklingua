using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BLL;
using worklingua.Server.Services;

namespace worklingua.Server.Middleware
{
    public class ManejadorExcepcionesNegocio : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;

        public ManejadorExcepcionesNegocio(IProblemDetailsService problemDetails)
        {
            _problemDetails = problemDetails;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext contexto,
            Exception excepcion,
            CancellationToken cancellationToken)
        {
            ExcepcionNegocio negocio = excepcion as ExcepcionNegocio;

            if (negocio == null)
            {
                ServicioLog.Error(
                    "Error no controlado procesando " + contexto.Request.Path + ".", excepcion);

                return false;
            }

            int estado = ResolverEstado(negocio.Tipo);
            contexto.Response.StatusCode = estado;

            ProblemDetails detalle = new ProblemDetails();
            detalle.Status = estado;
            detalle.Title = TituloPara(negocio.Tipo);
            detalle.Detail = negocio.Message;

            if (!string.IsNullOrEmpty(negocio.Sugerencia))
            {
                detalle.Extensions["sugerencia"] = negocio.Sugerencia;
            }

            ProblemDetailsContext contextoProblema = new ProblemDetailsContext
            {
                HttpContext = contexto,
                Exception = negocio,
                ProblemDetails = detalle
            };

            return await _problemDetails.TryWriteAsync(contextoProblema);
        }

        private int ResolverEstado(TipoErrorNegocio tipo)
        {
            switch (tipo)
            {
                case TipoErrorNegocio.Validacion:
                    return StatusCodes.Status400BadRequest;
                case TipoErrorNegocio.CredencialesInvalidas:
                    return StatusCodes.Status401Unauthorized;
                case TipoErrorNegocio.SesionInvalida:
                    return StatusCodes.Status401Unauthorized;
                case TipoErrorNegocio.CuentaNoConfirmada:
                    return StatusCodes.Status403Forbidden;
                case TipoErrorNegocio.CuentaBloqueada:
                    return StatusCodes.Status403Forbidden;
                case TipoErrorNegocio.PermisoDenegado:
                    return StatusCodes.Status403Forbidden;
                case TipoErrorNegocio.NoEncontrado:
                    return StatusCodes.Status404NotFound;
                case TipoErrorNegocio.Conflicto:
                    return StatusCodes.Status409Conflict;
                default:
                    return StatusCodes.Status400BadRequest;
            }
        }

        private string TituloPara(TipoErrorNegocio tipo)
        {
            switch (tipo)
            {
                case TipoErrorNegocio.Validacion:
                    return "Datos inválidos";
                case TipoErrorNegocio.CredencialesInvalidas:
                    return "Credenciales inválidas";
                case TipoErrorNegocio.SesionInvalida:
                    return "Sesión inválida";
                case TipoErrorNegocio.CuentaNoConfirmada:
                    return "Cuenta sin confirmar";
                case TipoErrorNegocio.CuentaBloqueada:
                    return "Cuenta bloqueada";
                case TipoErrorNegocio.PermisoDenegado:
                    return "Permiso denegado";
                case TipoErrorNegocio.NoEncontrado:
                    return "No encontrado";
                case TipoErrorNegocio.Conflicto:
                    return "Conflicto";
                default:
                    return "Solicitud inválida";
            }
        }
    }
}
