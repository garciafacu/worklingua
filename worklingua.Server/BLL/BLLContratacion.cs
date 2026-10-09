using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLContratacion
    {
        BLLSuscripcion oBLLSus;
        BLLLicencia oBLLLic;
        BLLFactura oBLLFac;
        BLLPago oBLLPag;
        BLLNotaCreditoDebito oBLLNot;
        BLLMovimientoCuentaCorriente oBLLMov;
        BLLPlan oBLLPlan;
        BLLEmpresa oBLLEmp;
        BLLUsuario oBLLUsu;
        BLLCultura oBLLCul;
        BLLTraduccion oBLLTra;
        BLLBitacora oBLLBit;
        ServicioValidacionTarjeta oServicioTarjeta;
        ServicioEmailContratacion oServicioEmail;

        public BLLContratacion()
        {
            oBLLSus = new BLLSuscripcion();
            oBLLLic = new BLLLicencia();
            oBLLFac = new BLLFactura();
            oBLLPag = new BLLPago();
            oBLLNot = new BLLNotaCreditoDebito();
            oBLLMov = new BLLMovimientoCuentaCorriente();
            oBLLPlan = new BLLPlan();
            oBLLEmp = new BLLEmpresa();
            oBLLUsu = new BLLUsuario();
            oBLLCul = new BLLCultura();
            oBLLTra = new BLLTraduccion();
            oBLLBit = new BLLBitacora();
            oServicioTarjeta = new ServicioValidacionTarjeta();
            oServicioEmail = new ServicioEmailContratacion();
        }

        public BECotizacionContratacion Cotizar(BEContratarPlan Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);
            BEEmpresa oEmpresaBE = ObtenerEmpresaContratante(oUsuarioBE);
            BEPlanSuscripcion oPlanBE = ObtenerPlanContratable(Objeto.PlanId);
            BECulturaConIdioma oCulturaBE = ObtenerCultura(Objeto.CodigoCultura);
            BESuscripcionConPlan oVigente = ObtenerVigente(oEmpresaBE);

            ValidarPlanDistinto(oVigente, oPlanBE);

            BESuscripcion oVigenteBE = oVigente == null ? null : oVigente.Suscripcion;
            decimal credito = CalcularCreditoCambioPlan(oVigenteBE, ObtenerFacturaVigente(oVigenteBE));
            decimal saldoNotas = SumarSaldo(oBLLNot.ListarDisponibles(oEmpresaBE));
            decimal saldoCuenta = oBLLMov.ObtenerSaldo(oEmpresaBE);

            return new BECotizacionContratacion(
                oPlanBE.PlanId,
                oPlanBE.Nombre,
                oVigente == null ? null : oVigente.Plan,
                oPlanBE.PrecioMensual,
                oCulturaBE.Cultura.Moneda,
                oCulturaBE.Cultura.SimboloMoneda,
                oCulturaBE.Cultura.TasaConversion,
                credito,
                saldoNotas,
                saldoNotas + CalcularSaldoAplicable(credito, saldoCuenta, saldoNotas),
                saldoCuenta,
                Configuracion.LimiteCuentaCorriente,
                CalcularDisponibleCuentaCorriente(saldoCuenta, credito),
                Configuracion.PorcentajeRecargoCuentaCorriente);
        }

        public BEResultadoContratacion Contratar(BEContratarPlan Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);
            BEEmpresa oEmpresaBE = ObtenerEmpresaContratante(oUsuarioBE);
            BEPlanSuscripcion oPlanBE = ObtenerPlanContratable(Objeto.PlanId);
            BECulturaConIdioma oCulturaBE = ObtenerCultura(Objeto.CodigoCultura);
            BESuscripcionConPlan oVigente = ObtenerVigente(oEmpresaBE);

            ValidarPlanDistinto(oVigente, oPlanBE);

            BESuscripcion oVigenteBE = oVigente == null ? null : oVigente.Suscripcion;
            BEFactura oFacturaVigenteBE = ObtenerFacturaVigente(oVigenteBE);
            decimal credito = CalcularCreditoCambioPlan(oVigenteBE, oFacturaVigenteBE);
            List<BENotaCreditoDebito> ListaNotaBE = oBLLNot.ListarDisponibles(oEmpresaBE);
            decimal saldoNotas = SumarSaldo(ListaNotaBE);
            decimal saldoCuenta = oBLLMov.ObtenerSaldo(oEmpresaBE);
            decimal creditoAplicable = CalcularSaldoAplicable(credito, saldoCuenta, saldoNotas);

            decimal tasa = oCulturaBE.Cultura.TasaConversion;
            decimal importe = oPlanBE.PrecioMensual;
            decimal importeNotaCredito = AjustarAlTope(ConvertirABase(Objeto.ImporteNotaCredito, tasa), importe, tasa);
            decimal importeCuentaCorriente = AjustarAlTope(
                ConvertirABase(Objeto.ImporteCuentaCorriente, tasa), importe - importeNotaCredito, tasa);

            ValidarImportes(importe, importeNotaCredito, importeCuentaCorriente, saldoNotas + creditoAplicable);

            decimal importeTarjeta = importe - importeNotaCredito - importeCuentaCorriente;
            decimal recargo = CalcularRecargo(importeCuentaCorriente);
            BEResultadoValidacionTarjeta oTarjetaBE = null;

            if (importeTarjeta > 0)
            {
                oTarjetaBE = oServicioTarjeta.Validar(Objeto.Tarjeta);

                if (!oTarjetaBE.FormatoValido)
                {
                    throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, oTarjetaBE.Motivo);
                }
            }

            BEResultadoContratacion oResultadoBE = new BEResultadoContratacion();
            oResultadoBE.Plan = oPlanBE.Nombre;
            oResultadoBE.Importe = importe;
            oResultadoBE.Moneda = oCulturaBE.Cultura.Moneda;
            oResultadoBE.TasaConversion = tasa;
            oResultadoBE.Suscripcion = new BESuscripcion(
                0, oEmpresaBE.EmpresaId, oPlanBE.PlanId, Hoy(), null, BLLSuscripcion.EstadoPendiente, false);
            oResultadoBE.Suscripcion.SuscripcionId = oBLLSus.Guardar(oResultadoBE.Suscripcion);

            Registrar(
                oUsuarioBE.UsuarioId,
                "ContratacionIniciada",
                "Contratación " + oResultadoBE.Suscripcion.SuscripcionId + " del plan " + oPlanBE.Nombre +
                " para la empresa " + oEmpresaBE.EmpresaId + " por ARS " + importe +
                " (tarjeta " + importeTarjeta + ", NC " + importeNotaCredito + ", cuenta corriente " +
                importeCuentaCorriente + ", moneda " + oCulturaBE.Cultura.Moneda + ").",
                BLLBitacora.NivelInformativo);

            oResultadoBE.Motivo = ObtenerMotivoRechazo(
                oTarjetaBE, importeCuentaCorriente, saldoCuenta + importe + recargo - importeTarjeta - credito);

            if (oResultadoBE.Motivo != null)
            {
                Rechazar(oResultadoBE, oUsuarioBE, oEmpresaBE, oCulturaBE);

                return oResultadoBE;
            }

            try
            {
                Aprobar(
                    oResultadoBE,
                    oEmpresaBE,
                    oCulturaBE.Cultura,
                    oVigenteBE,
                    oFacturaVigenteBE,
                    ListaNotaBE,
                    credito,
                    creditoAplicable,
                    importe,
                    importeTarjeta,
                    importeNotaCredito,
                    importeCuentaCorriente,
                    recargo,
                    oTarjetaBE);
            }
            catch (Exception ex)
            {
                Registrar(
                    oUsuarioBE.UsuarioId,
                    "ContratacionFallida",
                    "La contratación " + oResultadoBE.Suscripcion.SuscripcionId +
                    " quedó sin completar: " + ex.Message,
                    BLLBitacora.NivelError);

                throw;
            }

            MigrarLicencias(oVigenteBE, oResultadoBE, oUsuarioBE.UsuarioId);

            Registrar(
                oUsuarioBE.UsuarioId,
                "ContratacionAprobada",
                "Contratación " + oResultadoBE.Suscripcion.SuscripcionId + " del plan " + oPlanBE.Nombre +
                " aprobada" + (oResultadoBE.Factura == null ? "" : " con la factura " + oResultadoBE.Factura.NumeroFactura) + ".",
                BLLBitacora.NivelInformativo);

            RegistrarNotas(oUsuarioBE.UsuarioId, oResultadoBE);
            Avisar(oUsuarioBE, oEmpresaBE, oResultadoBE, oCulturaBE);

            return oResultadoBE;
        }

        public BEResultadoContratacion Cancelar(BECancelarContratacion Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);
            BEEmpresa oEmpresaBE = ObtenerEmpresaContratante(oUsuarioBE);
            BECulturaConIdioma oCulturaBE = ObtenerCultura(Objeto.CodigoCultura);

            BESuscripcion oFiltroBE = new BESuscripcion();
            oFiltroBE.SuscripcionId = Objeto.SuscripcionId;

            BESuscripcionConPlan oSuscripcion = oBLLSus.ListarPorId(oFiltroBE);

            if (oSuscripcion.Suscripcion.EmpresaId != oEmpresaBE.EmpresaId)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La contratación no existe.");
            }

            BEResultadoContratacion oResultadoBE = new BEResultadoContratacion();
            oResultadoBE.Plan = oSuscripcion.Plan;
            oResultadoBE.Suscripcion = oSuscripcion.Suscripcion;

            string estado = oSuscripcion.Suscripcion.Estado;

            if (estado != BLLSuscripcion.EstadoActiva && estado != BLLSuscripcion.EstadoPendiente)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Solo se pueden cancelar contrataciones activas o pendientes.");
            }

            if (estado == BLLSuscripcion.EstadoActiva)
            {
                BEFactura oFacturaBE = ObtenerFacturaVigente(oSuscripcion.Suscripcion);

                if (oFacturaBE == null)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto,
                        "El plan gratuito no se cancela: contratá otro plan para cambiarlo.");
                }

                decimal saldoCuenta = oBLLMov.ObtenerSaldo(oEmpresaBE);
                decimal saldoNotas = SumarSaldo(oBLLNot.ListarDisponibles(oEmpresaBE));

                oResultadoBE.Factura = oFacturaBE;
                oResultadoBE.Importe = oFacturaBE.Importe;
                oResultadoBE.Moneda = oFacturaBE.Moneda;
                oResultadoBE.TasaConversion = oFacturaBE.TasaConversion;

                EmitirNota(
                    oResultadoBE,
                    oEmpresaBE,
                    oFacturaBE,
                    BLLNotaCreditoDebito.TipoCredito,
                    oFacturaBE.Importe,
                    CalcularSaldoAplicable(oFacturaBE.Importe, saldoCuenta, saldoNotas),
                    "Cancelación de la contratación " + oSuscripcion.Suscripcion.SuscripcionId +
                    " (factura " + oFacturaBE.NumeroFactura + ")",
                    oFacturaBE.Moneda,
                    oFacturaBE.TasaConversion);

                oFacturaBE.Estado = BLLFactura.EstadoAnulada;
                oBLLFac.CambiarEstado(oFacturaBE);
            }

            CambiarEstado(
                oResultadoBE.Suscripcion,
                BLLSuscripcion.EstadoCancelada,
                estado == BLLSuscripcion.EstadoActiva ? Hoy() : (DateOnly?)null,
                "Cancelada por el cliente.");

            // La contratación deja de existir, así que sus licencias dejan de
            // habilitar: se revocan todas y el plan gratuito que entra en su
            // lugar arranca con el cupo libre (CU-001-005).
            int revocadas = oBLLLic.RevocarPorSuscripcion(oResultadoBE.Suscripcion.SuscripcionId);

            if (revocadas > 0)
            {
                Registrar(
                    oUsuarioBE.UsuarioId,
                    "LicenciasRevocadas",
                    "Se revocaron " + revocadas + " licencias de la contratación " +
                    oResultadoBE.Suscripcion.SuscripcionId + " al cancelarla.",
                    BLLBitacora.NivelInformativo);
            }

            if (estado == BLLSuscripcion.EstadoActiva)
            {
                oBLLSus.AsignarPlanInicial(oUsuarioBE);
            }

            Registrar(
                oUsuarioBE.UsuarioId,
                "ContratacionCancelada",
                "Contratación " + oResultadoBE.Suscripcion.SuscripcionId + " del plan " + oResultadoBE.Plan +
                " cancelada por la empresa " + oEmpresaBE.EmpresaId + ".",
                BLLBitacora.NivelInformativo);

            RegistrarNotas(oUsuarioBE.UsuarioId, oResultadoBE);
            Avisar(oUsuarioBE, oEmpresaBE, oResultadoBE, oCulturaBE);

            return oResultadoBE;
        }

        private void Aprobar(
            BEResultadoContratacion oResultadoBE,
            BEEmpresa oEmpresaBE,
            BECultura oCulturaBE,
            BESuscripcion oVigenteBE,
            BEFactura oFacturaVigenteBE,
            List<BENotaCreditoDebito> ListaNotaBE,
            decimal credito,
            decimal creditoAplicable,
            decimal importe,
            decimal importeTarjeta,
            decimal importeNotaCredito,
            decimal importeCuentaCorriente,
            decimal recargo,
            BEResultadoValidacionTarjeta oTarjetaBE)
        {
            int suscripcionId = oResultadoBE.Suscripcion.SuscripcionId;

            if (credito > 0)
            {
                BENotaCreditoDebito oNotaParcialBE = EmitirNota(
                    oResultadoBE,
                    oEmpresaBE,
                    oFacturaVigenteBE,
                    BLLNotaCreditoDebito.TipoCredito,
                    credito,
                    creditoAplicable,
                    "Crédito por el período no usado de la factura " + oFacturaVigenteBE.NumeroFactura +
                    " al cambiar a la contratación " + suscripcionId,
                    oCulturaBE.Moneda,
                    oCulturaBE.TasaConversion);

                if (oNotaParcialBE.SaldoDisponible > 0)
                {
                    ListaNotaBE.Add(oNotaParcialBE);
                }
            }

            if (importe > 0)
            {
                BEFactura oFacturaBE = new BEFactura(
                    0,
                    suscripcionId,
                    null,
                    Hoy(),
                    Hoy().AddDays(Configuracion.DiasVencimientoFactura),
                    importe,
                    importeCuentaCorriente > 0 ? BLLFactura.EstadoACuenta : BLLFactura.EstadoPagada,
                    oCulturaBE.Moneda,
                    oCulturaBE.TasaConversion);

                oResultadoBE.Factura = oBLLFac.Guardar(
                    oFacturaBE,
                    Movimiento(
                        oEmpresaBE,
                        BLLMovimientoCuentaCorriente.TipoFactura,
                        "Contratación del plan " + oResultadoBE.Plan,
                        importe,
                        0m));

                if (recargo > 0)
                {
                    EmitirNota(
                        oResultadoBE,
                        oEmpresaBE,
                        oResultadoBE.Factura,
                        BLLNotaCreditoDebito.TipoDebito,
                        recargo,
                        0m,
                        "Recargo del " + Configuracion.PorcentajeRecargoCuentaCorriente +
                        "% por pago en cuenta corriente (factura " + oResultadoBE.Factura.NumeroFactura + ")",
                        oCulturaBE.Moneda,
                        oCulturaBE.TasaConversion);
                }

                if (importeTarjeta > 0)
                {
                    string referencia = oTarjetaBE.Marca + " ****" + oTarjetaBE.UltimosDigitos;

                    oResultadoBE.Pagos.Add(oBLLPag.Guardar(
                        Pago(oResultadoBE.Factura, BLLPago.MedioTarjeta, importeTarjeta,
                            referencia + " " + oTarjetaBE.CodigoAutorizacion, BLLPago.EstadoAprobado),
                        null,
                        Movimiento(
                            oEmpresaBE,
                            BLLMovimientoCuentaCorriente.TipoPago,
                            "Pago con tarjeta " + referencia,
                            0m,
                            importeTarjeta)));
                }

                ImputarNotasCredito(oResultadoBE, ListaNotaBE, importeNotaCredito);

                if (importeCuentaCorriente > 0)
                {
                    oResultadoBE.Pagos.Add(oBLLPag.Guardar(
                        Pago(oResultadoBE.Factura, BLLPago.MedioCuentaCorriente, importeCuentaCorriente,
                            null, BLLPago.EstadoACuenta),
                        null,
                        null));
                }
            }

            if (oVigenteBE != null)
            {
                CambiarEstado(
                    oVigenteBE,
                    BLLSuscripcion.EstadoFinalizada,
                    Hoy(),
                    "Reemplazada por la contratación " + suscripcionId + ".");
            }

            CambiarEstado(
                oResultadoBE.Suscripcion,
                BLLSuscripcion.EstadoActiva,
                importe > 0 ? Hoy().AddMonths(1) : (DateOnly?)null,
                "Pago aprobado.");
        }

        /// <summary>
        /// Al cambiar de plan las licencias vigentes se mudan a la contratación
        /// nueva. Si el plan destino tiene menos cupo, el excedente queda
        /// revocado: se conserva a quien la tiene hace más tiempo (CU-001-005).
        ///
        /// Va después de Aprobar y no adentro porque necesita las dos
        /// suscripciones ya con su estado definitivo.
        /// </summary>
        private void MigrarLicencias(
            BESuscripcion oVigenteBE, BEResultadoContratacion oResultadoBE, int usuarioId)
        {
            if (oVigenteBE == null)
            {
                return;
            }

            int asignadas = oBLLLic.ContarAsignadas(oVigenteBE.SuscripcionId);

            if (asignadas == 0)
            {
                return;
            }

            BESuscripcionConPlan oOrigenBE = ObtenerConPlan(oVigenteBE.SuscripcionId);
            BESuscripcionConPlan oDestinoBE = ObtenerConPlan(oResultadoBE.Suscripcion.SuscripcionId);

            int migradas = oBLLLic.Migrar(oOrigenBE, oDestinoBE);

            Registrar(
                usuarioId,
                "LicenciasMigradas",
                "Se migraron " + migradas + " de " + asignadas + " licencias a la contratación " +
                oDestinoBE.Suscripcion.SuscripcionId + " (plan " + oDestinoBE.Plan + ", cupo " +
                oDestinoBE.CantidadLicencias + ").",
                BLLBitacora.NivelInformativo);
        }

        private BESuscripcionConPlan ObtenerConPlan(int suscripcionId)
        {
            BESuscripcion oFiltroBE = new BESuscripcion();
            oFiltroBE.SuscripcionId = suscripcionId;

            return oBLLSus.ListarPorId(oFiltroBE);
        }

        private void Rechazar(
            BEResultadoContratacion oResultadoBE,
            BEUsuario oUsuarioBE,
            BEEmpresa oEmpresaBE,
            BECulturaConIdioma oCulturaBE)
        {
            CambiarEstado(oResultadoBE.Suscripcion, BLLSuscripcion.EstadoRechazada, null, oResultadoBE.Motivo);

            Registrar(
                oUsuarioBE.UsuarioId,
                "ContratacionRechazada",
                "Contratación " + oResultadoBE.Suscripcion.SuscripcionId + " del plan " + oResultadoBE.Plan +
                " rechazada: " + oResultadoBE.Motivo,
                BLLBitacora.NivelAdvertencia);

            Avisar(oUsuarioBE, oEmpresaBE, oResultadoBE, oCulturaBE);
        }

        private void ImputarNotasCredito(
            BEResultadoContratacion oResultadoBE,
            List<BENotaCreditoDebito> ListaNotaBE,
            decimal importeNotaCredito)
        {
            decimal restante = importeNotaCredito;

            foreach (BENotaCreditoDebito oNotaBE in ListaNotaBE)
            {
                if (restante <= 0)
                {
                    break;
                }

                decimal aplicado = Math.Min(oNotaBE.SaldoDisponible, restante);

                if (aplicado <= 0)
                {
                    continue;
                }

                oNotaBE.SaldoDisponible = oNotaBE.SaldoDisponible - aplicado;
                oNotaBE.Estado = oNotaBE.SaldoDisponible == 0
                    ? BLLNotaCreditoDebito.EstadoAplicada
                    : BLLNotaCreditoDebito.EstadoDisponible;

                oResultadoBE.Pagos.Add(oBLLPag.Guardar(
                    Pago(oResultadoBE.Factura, BLLPago.MedioNotaCredito, aplicado, oNotaBE.Numero, BLLPago.EstadoAprobado),
                    oNotaBE,
                    null));

                restante = restante - aplicado;
            }

            if (restante > 0)
            {
                throw new InvalidOperationException(
                    "Las notas de crédito disponibles no alcanzaron para imputar ARS " + importeNotaCredito + ".");
            }
        }

        private BENotaCreditoDebito EmitirNota(
            BEResultadoContratacion oResultadoBE,
            BEEmpresa oEmpresaBE,
            BEFactura oFacturaBE,
            string tipo,
            decimal importe,
            decimal saldoDisponible,
            string motivo,
            string moneda,
            decimal tasaConversion)
        {
            bool esCredito = tipo == BLLNotaCreditoDebito.TipoCredito;
            string estado = !esCredito
                ? BLLNotaCreditoDebito.EstadoEmitida
                : saldoDisponible > 0 ? BLLNotaCreditoDebito.EstadoDisponible : BLLNotaCreditoDebito.EstadoAplicada;

            BENotaCreditoDebito oNotaBE = new BENotaCreditoDebito(
                0,
                oFacturaBE.FacturaId,
                tipo,
                null,
                DateTime.Now,
                importe,
                saldoDisponible,
                motivo,
                moneda,
                tasaConversion,
                estado);

            BENotaCreditoDebito oGuardadaBE = oBLLNot.Guardar(
                oNotaBE,
                Movimiento(
                    oEmpresaBE,
                    esCredito ? BLLMovimientoCuentaCorriente.TipoNotaCredito : BLLMovimientoCuentaCorriente.TipoNotaDebito,
                    motivo,
                    esCredito ? 0m : importe,
                    esCredito ? importe : 0m));

            oResultadoBE.Notas.Add(oGuardadaBE);

            return oGuardadaBE;
        }

        private void CambiarEstado(BESuscripcion oSuscripcionBE, string estado, DateOnly? fechaFin, string observacion)
        {
            oSuscripcionBE.Estado = estado;

            if (fechaFin.HasValue)
            {
                oSuscripcionBE.FechaFin = fechaFin;
            }

            BEHistorialSuscripcion oHistorialBE = new BEHistorialSuscripcion();
            oHistorialBE.SuscripcionId = oSuscripcionBE.SuscripcionId;
            oHistorialBE.EstadoNuevo = estado;
            oHistorialBE.Observacion = observacion;

            oBLLSus.CambiarEstado(oSuscripcionBE, oHistorialBE);
        }

        private BEEmpresa ObtenerEmpresaContratante(BEUsuario oUsuarioBE)
        {
            BEEmpresa oFiltroBE = new BEEmpresa();
            oFiltroBE.EmpresaId = oUsuarioBE.EmpresaId;

            BEEmpresa oEmpresaBE = oBLLEmp.ListarObjeto(oFiltroBE);

            if (oEmpresaBE.Protegido)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La empresa de la plataforma no contrata planes.");
            }

            if (!oEmpresaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La empresa está dada de baja y no puede contratar planes.");
            }

            return oEmpresaBE;
        }

        private BEPlanSuscripcion ObtenerPlanContratable(int planId)
        {
            BEPlanSuscripcion oFiltroBE = new BEPlanSuscripcion();
            oFiltroBE.PlanId = planId;

            BEPlanSuscripcion oPlanBE = oBLLPlan.ListarObjeto(oFiltroBE);

            if (!oPlanBE.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "El plan ya no está disponible.");
            }

            return oPlanBE;
        }

        private BECulturaConIdioma ObtenerCultura(string codigoCultura)
        {
            if (!string.IsNullOrWhiteSpace(codigoCultura))
            {
                foreach (BECulturaConIdioma oCulturaBE in oBLLCul.ListarTodo())
                {
                    if (string.Equals(oCulturaBE.Cultura.Codigo, codigoCultura.Trim(), StringComparison.OrdinalIgnoreCase)
                        && oCulturaBE.Cultura.TasaConversion > 0)
                    {
                        return oCulturaBE;
                    }
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La cultura de la operación no existe o está desactivada.");
        }

        private BESuscripcionConPlan ObtenerVigente(BEEmpresa oEmpresaBE)
        {
            BESuscripcion oFiltroBE = new BESuscripcion();
            oFiltroBE.EmpresaId = oEmpresaBE.EmpresaId;

            return oBLLSus.ListarObjeto(oFiltroBE);
        }

        private BEFactura ObtenerFacturaVigente(BESuscripcion oSuscripcionBE)
        {
            if (oSuscripcionBE == null)
            {
                return null;
            }

            BEFactura oFacturaBE = oBLLFac.ListarPorSuscripcion(oSuscripcionBE);

            if (oFacturaBE == null || oFacturaBE.Estado == BLLFactura.EstadoAnulada)
            {
                return null;
            }

            return oFacturaBE;
        }

        private void ValidarPlanDistinto(BESuscripcionConPlan oVigente, BEPlanSuscripcion oPlanBE)
        {
            if (oVigente != null && oVigente.Suscripcion.PlanId == oPlanBE.PlanId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Tu empresa ya tiene contratado el plan " + oPlanBE.Nombre + ".");
            }
        }

        private void ValidarImportes(
            decimal importe,
            decimal importeNotaCredito,
            decimal importeCuentaCorriente,
            decimal notaCreditoUtilizable)
        {
            if (importeNotaCredito < 0 || importeCuentaCorriente < 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Los importes no pueden ser negativos.");
            }

            if (importe == 0 && (importeNotaCredito > 0 || importeCuentaCorriente > 0))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El plan gratuito no lleva medios de pago.");
            }

            if (importeNotaCredito + importeCuentaCorriente > importe)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Lo que pagás con nota de crédito y cuenta corriente supera el importe del plan.");
            }

            if (importeNotaCredito > notaCreditoUtilizable)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El importe con nota de crédito supera tu saldo a favor.");
            }
        }

        private string ObtenerMotivoRechazo(
            BEResultadoValidacionTarjeta oTarjetaBE,
            decimal importeCuentaCorriente,
            decimal saldoProyectado)
        {
            if (oTarjetaBE != null && !oTarjetaBE.Aprobada)
            {
                return oTarjetaBE.Motivo;
            }

            if (importeCuentaCorriente > 0 && saldoProyectado > Configuracion.LimiteCuentaCorriente)
            {
                return "La operación supera el límite de crédito en cuenta corriente de la empresa.";
            }

            return null;
        }

        private decimal CalcularCreditoCambioPlan(BESuscripcion oSuscripcionBE, BEFactura oFacturaBE)
        {
            if (oSuscripcionBE == null || oFacturaBE == null || !oSuscripcionBE.FechaFin.HasValue)
            {
                return 0m;
            }

            int diasPeriodo = oSuscripcionBE.FechaFin.Value.DayNumber - oSuscripcionBE.FechaInicio.DayNumber;
            int diasRestantes = oSuscripcionBE.FechaFin.Value.DayNumber - Hoy().DayNumber;

            if (diasPeriodo <= 0 || diasRestantes <= 0)
            {
                return 0m;
            }

            if (diasRestantes > diasPeriodo)
            {
                diasRestantes = diasPeriodo;
            }

            return Math.Round(oFacturaBE.Importe * diasRestantes / diasPeriodo, 2, MidpointRounding.AwayFromZero);
        }

        private decimal CalcularSaldoAplicable(decimal importeNota, decimal saldoCuenta, decimal saldoNotas)
        {
            decimal deuda = saldoCuenta + saldoNotas;

            if (deuda <= 0)
            {
                return importeNota;
            }

            return Math.Max(0m, importeNota - deuda);
        }

        private decimal CalcularDisponibleCuentaCorriente(decimal saldoCuenta, decimal credito)
        {
            decimal margen = Configuracion.LimiteCuentaCorriente - saldoCuenta + credito;

            if (margen <= 0)
            {
                return 0m;
            }

            decimal factor = 1m + Configuracion.PorcentajeRecargoCuentaCorriente / 100m;

            return Math.Round(margen / factor, 2, MidpointRounding.ToZero);
        }

        private decimal CalcularRecargo(decimal importeCuentaCorriente)
        {
            if (importeCuentaCorriente <= 0 || Configuracion.PorcentajeRecargoCuentaCorriente <= 0)
            {
                return 0m;
            }

            return Math.Round(
                importeCuentaCorriente * Configuracion.PorcentajeRecargoCuentaCorriente / 100m,
                2,
                MidpointRounding.AwayFromZero);
        }

        private decimal ConvertirABase(decimal importeMoneda, decimal tasa)
        {
            return Math.Round(importeMoneda / tasa, 2, MidpointRounding.AwayFromZero);
        }

        private decimal AjustarAlTope(decimal importe, decimal tope, decimal tasa)
        {
            decimal tolerancia = Math.Round(0.01m / tasa, 2, MidpointRounding.AwayFromZero) + 0.01m;

            if (importe > tope && importe - tope <= tolerancia)
            {
                return Math.Max(0m, tope);
            }

            return importe;
        }

        private decimal SumarSaldo(List<BENotaCreditoDebito> ListaNotaBE)
        {
            decimal total = 0m;

            foreach (BENotaCreditoDebito oNotaBE in ListaNotaBE)
            {
                total = total + oNotaBE.SaldoDisponible;
            }

            return total;
        }

        private BEMovimientoCuentaCorriente Movimiento(
            BEEmpresa oEmpresaBE, string tipo, string concepto, decimal debe, decimal haber)
        {
            BEMovimientoCuentaCorriente oMovimientoBE = new BEMovimientoCuentaCorriente();

            oMovimientoBE.EmpresaId = oEmpresaBE.EmpresaId;
            oMovimientoBE.Tipo = tipo;
            oMovimientoBE.Concepto = concepto.Length > 200 ? concepto.Substring(0, 200) : concepto;
            oMovimientoBE.Debe = debe;
            oMovimientoBE.Haber = haber;

            return oMovimientoBE;
        }

        private BEPago Pago(BEFactura oFacturaBE, string medio, decimal importe, string numeroOperacion, string estado)
        {
            return new BEPago(0, oFacturaBE.FacturaId, DateTime.Now, medio, importe, numeroOperacion, estado);
        }

        private void RegistrarNotas(int usuarioId, BEResultadoContratacion oResultadoBE)
        {
            foreach (BENotaCreditoDebito oNotaBE in oResultadoBE.Notas)
            {
                Registrar(
                    usuarioId,
                    oNotaBE.Tipo == BLLNotaCreditoDebito.TipoCredito ? "NotaCreditoEmitida" : "NotaDebitoEmitida",
                    oNotaBE.Numero + " por ARS " + oNotaBE.Importe + ": " + oNotaBE.Motivo + ".",
                    BLLBitacora.NivelInformativo);
            }
        }

        private void Avisar(
            BEUsuario oUsuarioBE,
            BEEmpresa oEmpresaBE,
            BEResultadoContratacion oResultadoBE,
            BECulturaConIdioma oCulturaBE)
        {
            Dictionary<string, string> textos = oBLLTra.ListarPorCodigo(oCulturaBE.CodigoIdioma);

            EnviarAviso(oUsuarioBE.Email, oUsuarioBE.Nombre, oUsuarioBE.UsuarioId, oResultadoBE, oCulturaBE, textos);

            if (!string.IsNullOrWhiteSpace(oEmpresaBE.Email)
                && !string.Equals(oEmpresaBE.Email.Trim(), oUsuarioBE.Email, StringComparison.OrdinalIgnoreCase))
            {
                EnviarAviso(oEmpresaBE.Email.Trim(), oEmpresaBE.RazonSocial, oUsuarioBE.UsuarioId, oResultadoBE, oCulturaBE, textos);
            }
        }

        private void EnviarAviso(
            string destinatario,
            string nombre,
            int usuarioId,
            BEResultadoContratacion oResultadoBE,
            BECulturaConIdioma oCulturaBE,
            Dictionary<string, string> textos)
        {
            try
            {
                oServicioEmail.EnviarAviso(destinatario, nombre, oResultadoBE, oCulturaBE.Cultura, textos);
            }
            catch (Exception ex)
            {
                ServicioLog.Error(
                    "Falló el aviso por email de la contratación " + oResultadoBE.Suscripcion.SuscripcionId +
                    " a " + destinatario + ".", ex);

                Registrar(
                    usuarioId,
                    "Contratacion:EmailNoEnviado",
                    "Contratación " + oResultadoBE.Suscripcion.SuscripcionId + " (" + oResultadoBE.Suscripcion.Estado +
                    "), destinatario " + destinatario + ": " + ex.Message,
                    BLLBitacora.NivelError);
            }
        }

        private void Registrar(int usuarioId, string accion, string descripcion, string nivel)
        {
            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                usuarioId,
                DateTime.Now,
                BLLBitacora.ModuloSuscripcion,
                accion,
                descripcion,
                nivel));
        }

        private DateOnly Hoy()
        {
            return DateOnly.FromDateTime(DateTime.Now);
        }
    }
}
