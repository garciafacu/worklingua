using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLNoticia
    {
        private const int LongitudMaximaTitulo = 150;
        private const int LongitudMaximaResumen = 300;
        private const int LongitudMaximaContenido = 20000;

        static readonly DateTime FechaMinimaPublicacion = new DateTime(2000, 1, 1);

        MPPNoticia oMPPNot;
        BLLIdioma oBLLIdi;

        public BLLNoticia()
        {
            oMPPNot = new MPPNoticia();
            oBLLIdi = new BLLIdioma();
        }

        public List<BENoticia> ListarPublicadas(BEFiltroNoticia Objeto)
        {
            BEIdioma oFiltroBE = new BEIdioma();
            oFiltroBE.CodigoISO = Objeto.Idioma;

            BEIdioma oIdiomaBE = oBLLIdi.ResolverPorCodigo(oFiltroBE);

            if (oIdiomaBE == null)
            {
                return new List<BENoticia>();
            }

            List<BENoticia> ListaNoticiaBE = oMPPNot.ListarPublicadas(oIdiomaBE);

            if (ListaNoticiaBE == null)
            {
                BEIdioma oBaseFiltroBE = new BEIdioma();
                oBaseFiltroBE.CodigoISO = BLLTraduccion.CodigoIdiomaBase;

                BEIdioma oBaseBE = oBLLIdi.ResolverPorCodigo(oBaseFiltroBE);

                if (oBaseBE != null && oBaseBE.IdiomaId != oIdiomaBE.IdiomaId)
                {
                    ListaNoticiaBE = oMPPNot.ListarPublicadas(oBaseBE);
                }
            }

            return ListaNoticiaBE == null ? new List<BENoticia>() : ListaNoticiaBE;
        }

        public BENoticia ListarObjetoPublicada(BENoticia Objeto)
        {
            BENoticia oNoticiaBE = oMPPNot.ListarObjetoPublicada(Objeto);

            if (oNoticiaBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado, "La novedad no existe o ya no está publicada.");
            }

            return oNoticiaBE;
        }

        public List<BENoticia> ListarAdministracion(BEFiltroNoticia Objeto)
        {
            BEFiltroNoticia oFiltro = new BEFiltroNoticia(
                null,
                Objeto.IdiomaId.HasValue && Objeto.IdiomaId.Value > 0 ? Objeto.IdiomaId : null);

            List<BENoticia> ListaNoticiaBE = oMPPNot.ListarAdministracion(oFiltro);

            return ListaNoticiaBE == null ? new List<BENoticia>() : ListaNoticiaBE;
        }

        public BENoticia ListarObjeto(BENoticia Objeto)
        {
            BENoticia oNoticiaBE = oMPPNot.ListarObjeto(Objeto);

            if (oNoticiaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La noticia no existe.");
            }

            return oNoticiaBE;
        }

        public BENoticia Guardar(BENoticia Objeto)
        {
            Validar(Objeto);

            if (Objeto.NoticiaId != 0)
            {
                ListarObjeto(Objeto);
            }

            Objeto.NoticiaId = oMPPNot.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public bool Baja(BENoticia Objeto)
        {
            BENoticia oNoticiaBE = ListarObjeto(Objeto);

            if (!oNoticiaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La noticia ya está dada de baja.");
            }

            return oMPPNot.Baja(Objeto);
        }

        public bool EstaPublicada(BENoticia Objeto)
        {
            return Objeto.Activo && Objeto.FechaPublicacion <= DateTime.Now;
        }

        private void Validar(BENoticia Objeto)
        {
            Objeto.Titulo = Recortar(Objeto.Titulo);
            Objeto.Resumen = Recortar(Objeto.Resumen);
            Objeto.Contenido = Recortar(Objeto.Contenido);

            if (string.IsNullOrWhiteSpace(Objeto.Titulo))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El título de la noticia es obligatorio.");
            }

            if (Objeto.Titulo.Length > LongitudMaximaTitulo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El título no puede superar los " + LongitudMaximaTitulo + " caracteres.");
            }

            if (Objeto.Resumen != null && Objeto.Resumen.Length > LongitudMaximaResumen)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El resumen no puede superar los " + LongitudMaximaResumen + " caracteres.");
            }

            if (string.IsNullOrWhiteSpace(Objeto.Contenido))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El contenido de la noticia es obligatorio.");
            }

            if (Objeto.Contenido.Length > LongitudMaximaContenido)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El contenido no puede superar los " + LongitudMaximaContenido + " caracteres.");
            }

            if (Objeto.FechaPublicacion == default(DateTime))
            {
                Objeto.FechaPublicacion = DateTime.Now;
            }

            if (Objeto.FechaPublicacion < FechaMinimaPublicacion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La fecha de publicación no es válida.");
            }

            ValidarIdioma(Objeto);
        }

        private void ValidarIdioma(BENoticia Objeto)
        {
            foreach (BEIdioma oIdiomaBE in oBLLIdi.ListarTodoConBajas())
            {
                if (oIdiomaBE.IdiomaId == Objeto.IdiomaId)
                {
                    return;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "El idioma elegido para la noticia no existe.");
        }

        private string Recortar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }
    }
}
