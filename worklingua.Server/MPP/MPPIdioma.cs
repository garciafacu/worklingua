using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPIdioma
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPIdioma()
        {
            oDatos = new Acceso();
        }

        public List<BEIdioma> ListarTodo()
        {
            List<BEIdioma> ListaIdiomaBE = new List<BEIdioma>();
            DataTable Dt = oDatos.Leer("sp_Idioma_Listar", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaIdiomaBE.Add(Mapear(Item));
                }

                return ListaIdiomaBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEIdioma> ListarTodoConBajas()
        {
            List<BEIdioma> ListaIdiomaBE = new List<BEIdioma>();
            DataTable Dt = oDatos.Leer("sp_Idioma_ListarTodos", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaIdiomaBE.Add(Mapear(Item));
                }

                return ListaIdiomaBE;
            }
            else
            {
                return null;
            }
        }

        public BEIdioma ListarObjeto(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            DataTable Dt = oDatos.Leer("sp_Idioma_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.IdiomaId != 0)
            {
                Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@CodigoISO", Objeto.CodigoISO);

                oDatos.LeerEscalar("sp_Idioma_Modificar", Hdatos);

                return Objeto.IdiomaId;
            }

            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@CodigoISO", Objeto.CodigoISO);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object identidad = oDatos.LeerEscalar("sp_Idioma_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            object filas = oDatos.LeerEscalar("sp_Idioma_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarEstado(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object filas = oDatos.LeerEscalar("sp_Idioma_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEIdioma Mapear(DataRow Item)
        {
            BEIdioma oIdiomaBE = new BEIdioma();

            oIdiomaBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oIdiomaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oIdiomaBE.CodigoISO = ServicioLectorFila.LeerTexto(Item, "CodigoISO").Trim();
            oIdiomaBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");

            return oIdiomaBE;
        }
    }
}
