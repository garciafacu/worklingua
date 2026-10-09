using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLTokenSeguridad
    {
        public const string TipoConfirmacion = "CONFIRMACION";
        public const string TipoRecupero = "RECUPERO";
        public const string TipoInvitacion = "INVITACION";

        MPPTokenSeguridad oMPPTok;

        public BLLTokenSeguridad()
        {
            oMPPTok = new MPPTokenSeguridad();
        }

        public BETokenSeguridad Guardar(BETokenSeguridad Objeto)
        {
            int horas = HorasVigencia(Objeto.Tipo);

            Objeto.FechaGeneracion = DateTime.Now;
            Objeto.FechaExpiracion = DateTime.Now.AddHours(horas);

            return oMPPTok.Guardar(Objeto);
        }

        private int HorasVigencia(string tipo)
        {
            if (tipo == TipoConfirmacion)
            {
                return Configuracion.HorasVigenciaConfirmacion;
            }

            if (tipo == TipoInvitacion)
            {
                return Configuracion.HorasVigenciaInvitacion;
            }

            return Configuracion.HorasVigenciaRecupero;
        }

        public int InvalidarPorUsuarioYTipo(BETokenSeguridad Objeto)
        {
            return oMPPTok.InvalidarPorUsuarioYTipo(Objeto);
        }

        public BETokenSeguridad Consumir(BETokenSeguridad Objeto)
        {
            BETokenSeguridad oAlmacenadoBE = oMPPTok.ListarObjeto(Objeto);

            bool invalido = oAlmacenadoBE == null
                || oAlmacenadoBE.Tipo != Objeto.Tipo
                || !oAlmacenadoBE.Activo
                || oAlmacenadoBE.FechaUso.HasValue
                || oAlmacenadoBE.FechaExpiracion <= DateTime.Now;

            if (invalido || !oMPPTok.MarcarUsado(Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El enlace no es válido, ya fue utilizado o venció. Solicitá uno nuevo.");
            }

            return oAlmacenadoBE;
        }
    }
}
