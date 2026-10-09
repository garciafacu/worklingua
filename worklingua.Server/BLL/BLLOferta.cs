using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLOferta
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaDescripcion = 1000;

        MPPOferta oMPPOfe;
        BLLEmpresa oBLLEmp;
        BLLPlan oBLLPlan;
        BLLUsuario oBLLUsu;
        BLLBitacora oBLLBit;

        public BLLOferta()
        {
            oMPPOfe = new MPPOferta();
            oBLLEmp = new BLLEmpresa();
            oBLLPlan = new BLLPlan();
            oBLLUsu = new BLLUsuario();
            oBLLBit = new BLLBitacora();
        }

        public List<BEOfertaConDetalle> ListarTodo()
        {
            List<BEOfertaConDetalle> ListaOfertaBE = oMPPOfe.Listar(new BEFiltroOferta());

            return ListaOfertaBE == null ? new List<BEOfertaConDetalle>() : ListaOfertaBE;
        }

        public List<BEOfertaConDetalle> ListarVigentes(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BEFiltroOferta oFiltroBE = new BEFiltroOferta(
                null, oUsuarioBE.EmpresaId, DateOnly.FromDateTime(DateTime.Now));

            List<BEOfertaConDetalle> ListaOfertaBE = oMPPOfe.Listar(oFiltroBE);

            return ListaOfertaBE == null ? new List<BEOfertaConDetalle>() : ListaOfertaBE;
        }

        public BEOfertaConDetalle ListarObjeto(BEOferta Objeto)
        {
            List<BEOfertaConDetalle> ListaOfertaBE = oMPPOfe.Listar(new BEFiltroOferta(Objeto.OfertaId, null, null));

            if (ListaOfertaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La oferta no existe.");
            }

            return ListaOfertaBE[0];
        }

        public BEOfertaConDetalle Guardar(BEOferta Objeto, BESesion oSesionBE)
        {
            Validar(Objeto);

            bool esAlta = Objeto.OfertaId == 0;

            if (!esAlta)
            {
                ListarObjeto(Objeto);
            }

            Objeto.UsuarioId = oSesionBE.UsuarioId;
            Objeto.OfertaId = oMPPOfe.Guardar(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloOferta,
                esAlta ? "Alta" : "Modificacion",
                "Oferta " + Objeto.OfertaId + " (" + Objeto.Titulo + ") para la empresa " + Objeto.EmpresaId + ".",
                BLLBitacora.NivelInformativo));

            return ListarObjeto(Objeto);
        }

        public bool Baja(BEOferta Objeto, BESesion oSesionBE)
        {
            BEOfertaConDetalle oOferta = ListarObjeto(Objeto);

            if (!oOferta.Oferta.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "La oferta ya está dada de baja.");
            }

            bool resultado = oMPPOfe.Baja(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloOferta,
                "Baja",
                "Baja de la oferta " + Objeto.OfertaId + " (" + oOferta.Oferta.Titulo + ").",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        private void Validar(BEOferta Objeto)
        {
            Objeto.Titulo = Normalizar(Objeto.Titulo);
            Objeto.Descripcion = Normalizar(Objeto.Descripcion);

            if (Objeto.Titulo == null || Objeto.Titulo.Length > LongitudMaximaTitulo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El título es obligatorio y no puede superar los " + LongitudMaximaTitulo + " caracteres.");
            }

            if (Objeto.Descripcion == null || Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La descripción es obligatoria y no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }

            if (Objeto.FechaHasta.HasValue && Objeto.FechaHasta.Value < Objeto.FechaDesde)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La fecha de fin no puede ser anterior a la de inicio.");
            }

            BEEmpresa oFiltroEmpresaBE = new BEEmpresa();
            oFiltroEmpresaBE.EmpresaId = Objeto.EmpresaId;

            BEEmpresa oEmpresaBE = oBLLEmp.ListarObjeto(oFiltroEmpresaBE);

            if (oEmpresaBE.Protegido || !oEmpresaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Las ofertas se dirigen a empresas clientes activas.");
            }

            if (Objeto.PlanId.HasValue)
            {
                BEPlanSuscripcion oFiltroPlanBE = new BEPlanSuscripcion();
                oFiltroPlanBE.PlanId = Objeto.PlanId.Value;

                if (!oBLLPlan.ListarObjeto(oFiltroPlanBE).Activo)
                {
                    throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El plan sugerido está dado de baja.");
                }
            }
        }

        private string Normalizar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
