using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPNoticia
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPNoticia()
        {
            oDatos = new Acceso();
        }

        public List<BENoticia> ListarAdministracion(BEFiltroNoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return MapearLista(oDatos.Leer("sp_Noticia_ListarAdministracion", Hdatos));
        }

        public List<BENoticia> ListarPublicadas(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return MapearLista(oDatos.Leer("sp_Noticia_ListarPublicadas", Hdatos));
        }

        public BENoticia ListarObjeto(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

            DataTable Dt = oDatos.Leer("sp_Noticia_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public BENoticia ListarObjetoPublicada(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

            DataTable Dt = oDatos.Leer("sp_Noticia_ObtenerPublicada", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BENoticia Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.NoticiaId != 0)
            {
                Hdatos.Add("@NoticiaId", Objeto.NoticiaId);
                Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
                Hdatos.Add("@Titulo", Objeto.Titulo);
                Hdatos.Add("@Resumen", Objeto.Resumen);
                Hdatos.Add("@Contenido", Objeto.Contenido);
                Hdatos.Add("@FechaPublicacion", Objeto.FechaPublicacion);

                oDatos.LeerEscalar("sp_Noticia_Modificar", Hdatos);

                return Objeto.NoticiaId;
            }

            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Titulo", Objeto.Titulo);
            Hdatos.Add("@Resumen", Objeto.Resumen);
            Hdatos.Add("@Contenido", Objeto.Contenido);
            Hdatos.Add("@FechaPublicacion", Objeto.FechaPublicacion);

            object identidad = oDatos.LeerEscalar("sp_Noticia_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

            object filas = oDatos.LeerEscalar("sp_Noticia_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BENoticia> MapearLista(DataTable Dt)
        {
            List<BENoticia> ListaNoticiaBE = new List<BENoticia>();

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaNoticiaBE.Add(Mapear(Item));
                }

                return ListaNoticiaBE;
            }
            else
            {
                return null;
            }
        }

        private BENoticia Mapear(DataRow Item)
        {
            BENoticia oNoticiaBE = new BENoticia();

            oNoticiaBE.NoticiaId = ServicioLectorFila.LeerEntero(Item, "NoticiaId");
            oNoticiaBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oNoticiaBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oNoticiaBE.Titulo = ServicioLectorFila.LeerTexto(Item, "Titulo");
            oNoticiaBE.Resumen = ServicioLectorFila.LeerTextoNulo(Item, "Resumen");
            oNoticiaBE.Contenido = ServicioLectorFila.LeerTexto(Item, "Contenido");
            oNoticiaBE.FechaPublicacion = ServicioLectorFila.LeerFechaHora(Item, "FechaPublicacion");
            oNoticiaBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oNoticiaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oNoticiaBE;
        }
    }
}
