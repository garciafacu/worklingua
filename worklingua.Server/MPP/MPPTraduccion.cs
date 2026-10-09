using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPTraduccion
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPTraduccion()
        {
            oDatos = new Acceso();
        }

        public Dictionary<string, string> ListarPorCodigo(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CodigoISO", Objeto.CodigoISO);

            Dictionary<string, string> Bundle = new Dictionary<string, string>();
            DataTable Dt = oDatos.Leer("sp_Traduccion_ListarPorCodigo", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    string clave = ServicioLectorFila.LeerTexto(Item, "Clave");

                    if (!string.IsNullOrEmpty(clave))
                    {
                        Bundle[clave] = ServicioLectorFila.LeerTexto(Item, "Texto");
                    }
                }

                return Bundle;
            }
            else
            {
                return null;
            }
        }

        public List<BETraduccionAdministracion> ListarAdministracion(BEFiltroTraduccion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Clave", Objeto.Clave);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@SoloPendientes", Objeto.SoloPendientes);

            List<BETraduccionAdministracion> ListaTraduccionBE = new List<BETraduccionAdministracion>();
            DataTable Dt = oDatos.Leer("sp_Traduccion_ListarAdministracion", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaTraduccionBE.Add(Mapear(Item));
                }

                return ListaTraduccionBE;
            }
            else
            {
                return null;
            }
        }

        public bool Guardar(BETraduccion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Clave", Objeto.Clave);
            Hdatos.Add("@Texto", Objeto.Texto);

            object filas = oDatos.LeerEscalar("sp_Traduccion_Guardar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int GenerarClavesFaltantes(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            object filas = oDatos.LeerEscalar("sp_Traduccion_GenerarClavesFaltantes", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        public List<BETraduccion> ListarClaves()
        {
            List<BETraduccion> ListaTraduccionBE = new List<BETraduccion>();
            DataTable Dt = oDatos.Leer("sp_Traduccion_ListarClaves", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BETraduccion oTraduccionBE = new BETraduccion();

                    oTraduccionBE.Clave = ServicioLectorFila.LeerTexto(Item, "Clave");
                    oTraduccionBE.Texto = ServicioLectorFila.LeerTexto(Item, "Texto");

                    ListaTraduccionBE.Add(oTraduccionBE);
                }

                return ListaTraduccionBE;
            }
            else
            {
                return null;
            }
        }

        private BETraduccionAdministracion Mapear(DataRow Item)
        {
            BETraduccionAdministracion oTraduccionBE = new BETraduccionAdministracion();

            oTraduccionBE.Clave = ServicioLectorFila.LeerTexto(Item, "Clave");
            oTraduccionBE.TextoEspanol = ServicioLectorFila.LeerTexto(Item, "TextoEspanol");
            oTraduccionBE.Texto = ServicioLectorFila.LeerTexto(Item, "Texto");
            oTraduccionBE.Pendiente = ServicioLectorFila.LeerBooleano(Item, "Pendiente");

            return oTraduccionBE;
        }
    }
}
