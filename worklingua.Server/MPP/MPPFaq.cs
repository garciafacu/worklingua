using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPFaq
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BECategoriaFaq> ListarCategorias(BEFiltroFaq Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CategoriaFaqId", Objeto.CategoriaFaqId);
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@SoloActivas", Objeto.SoloActivas);

            List<BECategoriaFaq> ListaCategoriaBE = new List<BECategoriaFaq>();
            DataTable Dt = oDatos.Leer("sp_CategoriaFaq_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BECategoriaFaq oCategoriaBE = new BECategoriaFaq(
                    ServicioLectorFila.LeerEntero(Item, "CategoriaFaqId"),
                    ServicioLectorFila.LeerEntero(Item, "IdiomaId"),
                    ServicioLectorFila.LeerTexto(Item, "Titulo"),
                    ServicioLectorFila.LeerEntero(Item, "Orden"),
                    ServicioLectorFila.LeerBooleano(Item, "Activo"),
                    ServicioLectorFila.LeerFechaHora(Item, "FechaAlta"));

                oCategoriaBE.Idioma = ServicioLectorFila.LeerTexto(Item, "Idioma");
                oCategoriaBE.CodigoISO = ServicioLectorFila.LeerTexto(Item, "CodigoISO");
                oCategoriaBE.TotalPreguntas = ServicioLectorFila.LeerEntero(Item, "TotalPreguntas");

                ListaCategoriaBE.Add(oCategoriaBE);
            }

            return ListaCategoriaBE;
        }

        public int GuardarCategoria(BECategoriaFaq Objeto)
        {
            Hdatos = new Hashtable();
            string Consulta = "sp_CategoriaFaq_Alta";

            if (Objeto.CategoriaFaqId != 0)
            {
                Hdatos.Add("@CategoriaFaqId", Objeto.CategoriaFaqId);
                Consulta = "sp_CategoriaFaq_Modificar";
            }

            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Titulo", Objeto.Titulo);
            Hdatos.Add("@Orden", Objeto.Orden);
            Hdatos.Add("@Activo", Objeto.Activo);

            object resultado = oDatos.LeerEscalar(Consulta, Hdatos);

            return Objeto.CategoriaFaqId != 0 ? Objeto.CategoriaFaqId : Convert.ToInt32(resultado);
        }

        public bool BajaCategoria(BECategoriaFaq Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CategoriaFaqId", Objeto.CategoriaFaqId);

            object filas = oDatos.LeerEscalar("sp_CategoriaFaq_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarEstadoCategoria(BECategoriaFaq Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CategoriaFaqId", Objeto.CategoriaFaqId);
            Hdatos.Add("@Activo", Objeto.Activo);

            object filas = oDatos.LeerEscalar("sp_CategoriaFaq_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public List<BEPreguntaFrecuente> ListarPreguntas(BEFiltroFaq Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PreguntaFrecuenteId", Objeto.PreguntaFrecuenteId);
            Hdatos.Add("@CategoriaFaqId", Objeto.CategoriaFaqId);
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@SoloActivas", Objeto.SoloActivas);

            List<BEPreguntaFrecuente> ListaPreguntaBE = new List<BEPreguntaFrecuente>();
            DataTable Dt = oDatos.Leer("sp_PreguntaFrecuente_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BEPreguntaFrecuente oPreguntaBE = new BEPreguntaFrecuente(
                    ServicioLectorFila.LeerEntero(Item, "PreguntaFrecuenteId"),
                    ServicioLectorFila.LeerEntero(Item, "CategoriaFaqId"),
                    ServicioLectorFila.LeerTexto(Item, "Pregunta"),
                    ServicioLectorFila.LeerTexto(Item, "Respuesta"),
                    ServicioLectorFila.LeerTextoNulo(Item, "Enlace"),
                    ServicioLectorFila.LeerTextoNulo(Item, "EnlaceEtiqueta"),
                    ServicioLectorFila.LeerEntero(Item, "Orden"),
                    ServicioLectorFila.LeerBooleano(Item, "Activo"),
                    ServicioLectorFila.LeerFechaHora(Item, "FechaAlta"));

                oPreguntaBE.Categoria = ServicioLectorFila.LeerTexto(Item, "Categoria");
                oPreguntaBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");

                ListaPreguntaBE.Add(oPreguntaBE);
            }

            return ListaPreguntaBE;
        }

        public int GuardarPregunta(BEPreguntaFrecuente Objeto)
        {
            Hdatos = new Hashtable();
            string Consulta = "sp_PreguntaFrecuente_Alta";

            if (Objeto.PreguntaFrecuenteId != 0)
            {
                Hdatos.Add("@PreguntaFrecuenteId", Objeto.PreguntaFrecuenteId);
                Consulta = "sp_PreguntaFrecuente_Modificar";
            }

            Hdatos.Add("@CategoriaFaqId", Objeto.CategoriaFaqId);
            Hdatos.Add("@Pregunta", Objeto.Pregunta);
            Hdatos.Add("@Respuesta", Objeto.Respuesta);
            Hdatos.Add("@Enlace", Objeto.Enlace);
            Hdatos.Add("@EnlaceEtiqueta", Objeto.EnlaceEtiqueta);
            Hdatos.Add("@Orden", Objeto.Orden);
            Hdatos.Add("@Activo", Objeto.Activo);

            object resultado = oDatos.LeerEscalar(Consulta, Hdatos);

            return Objeto.PreguntaFrecuenteId != 0
                ? Objeto.PreguntaFrecuenteId
                : Convert.ToInt32(resultado);
        }

        public bool BajaPregunta(BEPreguntaFrecuente Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PreguntaFrecuenteId", Objeto.PreguntaFrecuenteId);

            object filas = oDatos.LeerEscalar("sp_PreguntaFrecuente_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarEstadoPregunta(BEPreguntaFrecuente Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PreguntaFrecuenteId", Objeto.PreguntaFrecuenteId);
            Hdatos.Add("@Activo", Objeto.Activo);

            object filas = oDatos.LeerEscalar("sp_PreguntaFrecuente_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }
    }
}
