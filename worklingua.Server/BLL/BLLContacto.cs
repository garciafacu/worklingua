using worklingua.Server.BE;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLContacto
    {
        ServicioEmailCuenta oServicioEmail;
        BLLBitacora oBLLBit;

        public BLLContacto()
        {
            oServicioEmail = new ServicioEmailCuenta();
            oBLLBit = new BLLBitacora();
        }

        public void EnviarConsulta(BEEnviarConsultaContacto Objeto)
        {
            string nombreLimpio = LimpiarLinea(Objeto.Nombre);
            string emailLimpio = LimpiarLinea(Objeto.Email);
            string asuntoLimpio = LimpiarLinea(Objeto.Asunto);
            string mensajeLimpio = Objeto.Mensaje == null ? string.Empty : Objeto.Mensaje.Trim();

            if (string.IsNullOrWhiteSpace(nombreLimpio))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Ingresá tu nombre.");
            }

            if (string.IsNullOrWhiteSpace(emailLimpio))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Ingresá tu correo.");
            }

            if (string.IsNullOrWhiteSpace(asuntoLimpio))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Ingresá un asunto.");
            }

            if (string.IsNullOrWhiteSpace(mensajeLimpio))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Ingresá tu mensaje.");
            }

            try
            {
                oServicioEmail.EnviarConsultaContacto(
                    new BEEnviarConsultaContacto(nombreLimpio, emailLimpio, asuntoLimpio, mensajeLimpio));
            }
            catch (Exception ex)
            {
                ServicioLog.Error("No se pudo enviar la consulta de contacto de " + emailLimpio + ".", ex);

                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    null,
                    DateTime.Now,
                    BLLBitacora.ModuloContacto,
                    "ConsultaNoEnviada",
                    ex.Message,
                    "ERROR"));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "No pudimos enviar tu consulta. Intentá nuevamente en unos minutos.");
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                null,
                DateTime.Now,
                BLLBitacora.ModuloContacto,
                "ConsultaEnviada",
                "Consulta de " + emailLimpio + ": " + asuntoLimpio,
                BLLBitacora.NivelInformativo));
        }

        private string LimpiarLinea(string valor)
        {
            if (valor == null)
            {
                return string.Empty;
            }

            return valor.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        }
    }
}
