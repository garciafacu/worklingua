using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPEnvioNewsletter
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPEnvioNewsletter()
        {
            oDatos = new Acceso();
        }

        public List<BEEnvioNewsletter> ListarTodo()
        {
            List<BEEnvioNewsletter> ListaEnvioBE = new List<BEEnvioNewsletter>();
            DataTable Dt = oDatos.Leer("sp_EnvioNewsletter_Listar", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaEnvioBE.Add(Mapear(Item));
                }

                return ListaEnvioBE;
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEEnvioNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Asunto", Objeto.Asunto);
            Hdatos.Add("@CantidadEnviados", Objeto.CantidadEnviados);
            Hdatos.Add("@CantidadFallidos", Objeto.CantidadFallidos);

            object identidad = oDatos.LeerEscalar("sp_EnvioNewsletter_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private BEEnvioNewsletter Mapear(DataRow Item)
        {
            BEEnvioNewsletter oEnvioBE = new BEEnvioNewsletter();

            oEnvioBE.EnvioId = ServicioLectorFila.LeerEntero(Item, "EnvioId");
            oEnvioBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oEnvioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oEnvioBE.Asunto = ServicioLectorFila.LeerTexto(Item, "Asunto");
            oEnvioBE.FechaEnvio = ServicioLectorFila.LeerFechaHora(Item, "FechaEnvio");
            oEnvioBE.CantidadEnviados = ServicioLectorFila.LeerEntero(Item, "CantidadEnviados");
            oEnvioBE.CantidadFallidos = ServicioLectorFila.LeerEntero(Item, "CantidadFallidos");

            return oEnvioBE;
        }
    }
}
