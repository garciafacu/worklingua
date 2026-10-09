using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPCaracteristica
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPCaracteristica()
        {
            oDatos = new Acceso();
        }

        public List<BECaracteristica> ListarTodo()
        {
            return Listar("sp_Caracteristica_Listar");
        }

        public List<BECaracteristica> ListarTodoConBajas()
        {
            return Listar("sp_Caracteristica_ListarTodos");
        }

        public BECaracteristica ListarObjeto(BECaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

            DataTable Dt = oDatos.Leer("sp_Caracteristica_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BECaracteristica Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.CaracteristicaId != 0)
            {
                Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@Orden", Objeto.Orden);

                oDatos.LeerEscalar("sp_Caracteristica_Modificar", Hdatos);

                return Objeto.CaracteristicaId;
            }

            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Orden", Objeto.Orden);
            Hdatos.Add("@Activo", Objeto.Activo);

            object identidad = oDatos.LeerEscalar("sp_Caracteristica_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BECaracteristica Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CaracteristicaId", Objeto.CaracteristicaId);

            object filas = oDatos.LeerEscalar("sp_Caracteristica_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BECaracteristica> Listar(string Consulta)
        {
            List<BECaracteristica> ListaCaracteristicaBE = new List<BECaracteristica>();
            DataTable Dt = oDatos.Leer(Consulta, null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaCaracteristicaBE.Add(Mapear(Item));
                }

                return ListaCaracteristicaBE;
            }
            else
            {
                return null;
            }
        }

        private BECaracteristica Mapear(DataRow Item)
        {
            BECaracteristica oCaracteristicaBE = new BECaracteristica();

            oCaracteristicaBE.CaracteristicaId = ServicioLectorFila.LeerEntero(Item, "CaracteristicaId");
            oCaracteristicaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oCaracteristicaBE.Orden = ServicioLectorFila.LeerEntero(Item, "Orden");
            oCaracteristicaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oCaracteristicaBE;
        }
    }
}
