using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [ApiController]
    public abstract class ControladorBase : ControllerBase
    {
        public const string HeaderTokenSesion = "X-Sesion-Token";

        private BLLSeguridad oBLLSeg;

        protected ControladorBase()
        {
            oBLLSeg = new BLLSeguridad();
        }

        protected BESesion ExigirPermiso(string permiso)
        {
            return oBLLSeg.ExigirPermiso(ObtenerTokenSesion(), permiso);
        }

        protected Guid ObtenerTokenSesion()
        {
            string valor = Request.Headers[HeaderTokenSesion].ToString();
            Guid token;

            if (string.IsNullOrWhiteSpace(valor) || !Guid.TryParse(valor, out token))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.SesionInvalida,
                    "Falta el header " + HeaderTokenSesion + " o su valor no es válido.");
            }

            return token;
        }

        protected Guid? ObtenerTokenSesionOpcional()
        {
            if (string.IsNullOrWhiteSpace(Request.Headers[HeaderTokenSesion].ToString()))
            {
                return null;
            }

            return ObtenerTokenSesion();
        }

        protected BEPermisoRespuesta MapearPermiso(BEPermiso oPermisoBE)
        {
            BEPermisoRespuesta item = new BEPermisoRespuesta();

            item.PermisoId = oPermisoBE.PermisoId;
            item.Nombre = oPermisoBE.Nombre;
            item.Descripcion = oPermisoBE.Descripcion;
            item.EsCompuesto = oPermisoBE.EsCompuesto;

            foreach (BEPermiso oHijoBE in oPermisoBE.ObtenerPermisosHijos())
            {
                item.Hijos.Add(MapearPermiso(oHijoBE));
            }

            return item;
        }
    }
}
