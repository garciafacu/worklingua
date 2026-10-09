using System.Collections.Generic;
using System.Net.Mail;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLSuscriptorNewsletter
    {
        private const int LongitudMaximaEmail = 150;
        private const int MinutosEntreConfirmaciones = 10;

        MPPSuscriptorNewsletter oMPPSus;
        BLLIdioma oBLLIdi;
        BLLTraduccion oBLLTra;
        BLLBitacora oBLLBit;
        ServicioEmailNewsletter oServicioEmail;

        public BLLSuscriptorNewsletter()
        {
            oMPPSus = new MPPSuscriptorNewsletter();
            oBLLIdi = new BLLIdioma();
            oBLLTra = new BLLTraduccion();
            oBLLBit = new BLLBitacora();
            oServicioEmail = new ServicioEmailNewsletter();
        }

        public List<BESuscriptorNewsletter> ListarTodo()
        {
            List<BESuscriptorNewsletter> ListaSuscriptorBE = oMPPSus.ListarTodo();

            return ListaSuscriptorBE == null ? new List<BESuscriptorNewsletter>() : ListaSuscriptorBE;
        }

        public List<BESuscriptorNewsletter> ListarConfirmados(BEIdioma Objeto)
        {
            List<BESuscriptorNewsletter> ListaSuscriptorBE = oMPPSus.ListarConfirmados(Objeto);

            return ListaSuscriptorBE == null ? new List<BESuscriptorNewsletter>() : ListaSuscriptorBE;
        }

        public void Guardar(BESuscribirNewsletter Objeto)
        {
            string email = ValidarEmail(Objeto.Email);

            BEIdioma oFiltroIdiomaBE = new BEIdioma();
            oFiltroIdiomaBE.CodigoISO = Objeto.Idioma;

            BEIdioma oIdiomaBE = oBLLIdi.ResolverPorCodigo(oFiltroIdiomaBE);

            if (oIdiomaBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "No hay idiomas disponibles para la suscripción.");
            }

            BESuscriptorNewsletter oFiltroBE = new BESuscriptorNewsletter();
            oFiltroBE.Email = email;

            BESuscriptorNewsletter oExistenteBE = oMPPSus.ListarPorEmail(oFiltroBE);

            if (oExistenteBE != null && !DebeEnviarConfirmacion(oExistenteBE))
            {
                return;
            }

            BESuscriptorNewsletter oSuscriptorBE = new BESuscriptorNewsletter();
            oSuscriptorBE.SuscriptorId = oExistenteBE == null ? 0 : oExistenteBE.SuscriptorId;
            oSuscriptorBE.Email = email;
            oSuscriptorBE.IdiomaId = oIdiomaBE.IdiomaId;

            oSuscriptorBE.SuscriptorId = oMPPSus.Guardar(oSuscriptorBE);

            EnviarConfirmacion(oMPPSus.ListarObjeto(oSuscriptorBE), oIdiomaBE);
        }

        public void Confirmar(BETokenNewsletter Objeto)
        {
            BESuscriptorNewsletter oSuscriptorBE = ListarPorToken(Objeto);

            if (!oSuscriptorBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La suscripción fue dada de baja. Si querés volver a recibir el newsletter, suscribite de nuevo.");
            }

            if (oSuscriptorBE.Confirmado)
            {
                return;
            }

            if (oSuscriptorBE.FechaAlta.AddHours(Configuracion.HorasVigenciaConfirmacion) <= DateTime.Now)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El enlace venció. Suscribite de nuevo para recibir uno nuevo.");
            }

            oMPPSus.Confirmar(oSuscriptorBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                null,
                DateTime.Now,
                BLLBitacora.ModuloNewsletter,
                "SuscripcionConfirmada",
                "Se confirmó la suscripción de " + oSuscriptorBE.Email + ".",
                BLLBitacora.NivelInformativo));
        }

        public void Baja(BETokenNewsletter Objeto)
        {
            BESuscriptorNewsletter oSuscriptorBE = ListarPorToken(Objeto);

            if (!oSuscriptorBE.Activo)
            {
                return;
            }

            oMPPSus.Baja(oSuscriptorBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                null,
                DateTime.Now,
                BLLBitacora.ModuloNewsletter,
                "SuscripcionBaja",
                "Se dio de baja la suscripción de " + oSuscriptorBE.Email + ".",
                BLLBitacora.NivelInformativo));
        }

        private bool DebeEnviarConfirmacion(BESuscriptorNewsletter oExistenteBE)
        {
            if (!oExistenteBE.Activo)
            {
                return true;
            }

            if (oExistenteBE.Confirmado)
            {
                return false;
            }

            return oExistenteBE.FechaAlta.AddMinutes(MinutosEntreConfirmaciones) <= DateTime.Now;
        }

        private void EnviarConfirmacion(BESuscriptorNewsletter oSuscriptorBE, BEIdioma oIdiomaBE)
        {
            try
            {
                oServicioEmail.EnviarConfirmacion(oSuscriptorBE, oBLLTra.ListarPorCodigo(oIdiomaBE.CodigoISO));
            }
            catch (Exception ex)
            {
                ServicioLog.Error(
                    "No se pudo enviar la confirmación del newsletter a " + oSuscriptorBE.Email + ".", ex);

                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    null,
                    DateTime.Now,
                    BLLBitacora.ModuloNewsletter,
                    "ConfirmacionNoEnviada",
                    ex.Message,
                    BLLBitacora.NivelError));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "No pudimos enviarte el correo de confirmación. Intentá nuevamente en unos minutos.");
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                null,
                DateTime.Now,
                BLLBitacora.ModuloNewsletter,
                "SuscripcionSolicitada",
                "Solicitud de suscripción de " + oSuscriptorBE.Email + " en idioma " + oIdiomaBE.CodigoISO + ".",
                BLLBitacora.NivelInformativo));
        }

        private BESuscriptorNewsletter ListarPorToken(BETokenNewsletter Objeto)
        {
            BESuscriptorNewsletter oFiltroBE = new BESuscriptorNewsletter();
            oFiltroBE.Token = Objeto.Token;

            BESuscriptorNewsletter oSuscriptorBE = Objeto.Token == Guid.Empty ? null : oMPPSus.ListarPorToken(oFiltroBE);

            if (oSuscriptorBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El enlace no es válido o ya no está vigente.");
            }

            return oSuscriptorBE;
        }

        private string ValidarEmail(string email)
        {
            string limpio = email == null ? string.Empty : email.Trim().ToLowerInvariant();
            MailAddress direccion;

            if (limpio.Length == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Ingresá tu correo.");
            }

            if (limpio.Length > LongitudMaximaEmail)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El correo no puede superar los " + LongitudMaximaEmail + " caracteres.");
            }

            if (!MailAddress.TryCreate(limpio, out direccion) || direccion.Address != limpio)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El correo no tiene un formato válido.");
            }

            return limpio;
        }
    }
}
