using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLSesion
    {
        MPPSesion oMPPSes;
        BLLBitacora oBLLBit;

        public BLLSesion()
        {
            oMPPSes = new MPPSesion();
            oBLLBit = new BLLBitacora();
        }

        public BESesion Guardar(BESesion Objeto)
        {
            return oMPPSes.Guardar(Objeto);
        }

        public BESesion ObtenerActiva(Guid token)
        {
            BESesion oFiltroBE = new BESesion();
            oFiltroBE.Token = token;

            BESesion oSesionBE = oMPPSes.ListarObjeto(oFiltroBE);

            if (oSesionBE == null || oSesionBE.Activa != true)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    oSesionBE == null ? null : (int?)oSesionBE.UsuarioId,
                    DateTime.Now,
                    BLLBitacora.ModuloSeguridad,
                    "SesionInvalida",
                    oSesionBE == null
                        ? "Pedido con un token de sesión que no existe."
                        : "Pedido con un token de una sesión ya cerrada.",
                    BLLBitacora.NivelAdvertencia));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.SesionInvalida, "La sesión no es válida o ya fue cerrada.");
            }

            return oSesionBE;
        }

        public bool Baja(Guid token)
        {
            BESesion oSesionBE = new BESesion();
            oSesionBE.Token = token;

            return oMPPSes.Baja(oSesionBE);
        }

        public int CerrarPorUsuario(BESesion Objeto)
        {
            int cerradas = oMPPSes.CerrarPorUsuario(Objeto);

            if (cerradas > 0)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    Objeto.UsuarioId,
                    DateTime.Now,
                    BLLBitacora.ModuloSeguridad,
                    "SesionesCerradas",
                    "Se invalidaron " + cerradas + " sesiones abiertas de la cuenta.",
                    BLLBitacora.NivelAdvertencia));
            }

            return cerradas;
        }
    }
}
