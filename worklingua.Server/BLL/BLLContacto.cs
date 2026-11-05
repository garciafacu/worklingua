using worklingua.Server.BE;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLContacto
    {
        ServicioEmailCuenta oServicioEmail = new ServicioEmailCuenta();
        BLLBitacora oBLLBit = new BLLBitacora();

        public void EnviarConsulta(BEEnviarConsultaContacto Objeto)
        {
            string nombreLimpio = LimpiarLinea(Objeto.Nombre);
            string emailLimpio = LimpiarLinea(Objeto.Email);
            string asuntoLimpio = LimpiarLinea(Objeto.Asunto);
            string mensajeLimpio = Objeto.Mensaje == null ? string.Empty : Objeto.Mensaje.Trim();

            ReglasNegocio.ExigirTexto(nombreLimpio, "Ingresá tu nombre.");
            ReglasNegocio.ExigirTexto(emailLimpio, "Ingresá tu correo.");

            ReglasNegocio.ExigirTexto(asuntoLimpio, "Ingresá un asunto.");
            ReglasNegocio.ExigirTexto(mensajeLimpio, "Ingresá tu mensaje.");

            try
            {
                oServicioEmail.EnviarConsultaContacto(
                    new BEEnviarConsultaContacto(nombreLimpio, emailLimpio, asuntoLimpio, mensajeLimpio));
            }
            catch (Exception ex)
            {
                ServicioLog.Error("No se pudo enviar la consulta de contacto de " + emailLimpio + ".", ex);

                oBLLBit.Guardar(new BEBitacoraEvento(
                    0, null, DateTime.Now, BLLBitacora.ModuloContacto, "ConsultaNoEnviada",
                    ex.Message, BLLBitacora.NivelError));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "No pudimos enviar tu consulta. Intentá nuevamente en unos minutos.");
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, null, DateTime.Now, BLLBitacora.ModuloContacto, "ConsultaEnviada",
                "Consulta de " + emailLimpio + ": " + asuntoLimpio, BLLBitacora.NivelInformativo));
        }

        private string LimpiarLinea(string valor)
        {
            if (valor == null)
            {
                return string.Empty;
            }

            return valor.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        public void Validar(BEEnviarConsultaContacto Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "Ingresá tu nombre.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 100, "El nombre no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Email, "Ingresá tu correo.");
            ReglasNegocio.ExigirEmail(Objeto.Email, "El correo no tiene un formato válido.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Email, 150, "El correo no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Asunto, "Ingresá un asunto.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Asunto, 150, "El asunto no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.Mensaje, "Ingresá tu mensaje.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Mensaje, 2000, "El mensaje no puede superar los 2000 caracteres.");
        }
    }
}
