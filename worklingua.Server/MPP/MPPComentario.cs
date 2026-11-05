using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPComentario
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEComentarioConAutor> ListarTodo(BEComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            return ServicioLectorFila.ALista<BEComentarioConAutor>(oDatos.Leer("sp_Comentario_Listar", Hdatos), Mapear);
        }

        public List<BEComentarioConAutor> Buscar(BEFiltroComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@FechaDesde", Objeto.Desde);
            Hdatos.Add("@FechaHasta", Objeto.Hasta);

            return ServicioLectorFila.ALista<BEComentarioConAutor>(oDatos.Leer("sp_Comentario_Buscar", Hdatos), Mapear);
        }

        public BEComentarioConAutor ListarObjeto(BEComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ComentarioId", Objeto.ComentarioId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Comentario_ObtenerPorId", Hdatos), Mapear);
        }

        public bool Baja(BEComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ComentarioId", Objeto.ComentarioId);

            object filas = oDatos.LeerEscalar("sp_Comentario_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public BEComentarioConAutor ListarValoracion(BEComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@PlanId", Objeto.PlanId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Comentario_ObtenerValoracion", Hdatos), Mapear);
        }

        public List<BEResumenValoracion> ListarResumen()
        {
            List<BEResumenValoracion> ListaResumenBE = new List<BEResumenValoracion>();
            DataTable Dt = oDatos.Leer("sp_Comentario_ListarResumen", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BEResumenValoracion oResumenBE = new BEResumenValoracion();

                    oResumenBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
                    oResumenBE.Promedio = ServicioLectorFila.LeerDecimal(Item, "Promedio");
                    oResumenBE.Cantidad = ServicioLectorFila.LeerEntero(Item, "Cantidad");

                    ListaResumenBE.Add(oResumenBE);
                }

                return ListaResumenBE;
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEComentario Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.ComentarioId != 0)
            {
                Hdatos.Add("@ComentarioId", Objeto.ComentarioId);
                Hdatos.Add("@Texto", Objeto.Texto);
                Hdatos.Add("@Puntaje", Objeto.Puntaje);

                oDatos.LeerEscalar("sp_Comentario_Modificar", Hdatos);

                return Objeto.ComentarioId;
            }

            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@Puntaje", Objeto.Puntaje);

            object identidad = oDatos.LeerEscalar("sp_Comentario_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private BEComentarioConAutor Mapear(DataRow Item)
        {
            BEComentario oComentarioBE = new BEComentario();

            oComentarioBE.ComentarioId = ServicioLectorFila.LeerEntero(Item, "ComentarioId");
            oComentarioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oComentarioBE.PlanId = ServicioLectorFila.LeerEntero(Item, "PlanId");
            oComentarioBE.Texto = ServicioLectorFila.LeerTexto(Item, "Texto");
            oComentarioBE.Puntaje = ServicioLectorFila.LeerEnteroNulo(Item, "Puntaje");
            oComentarioBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");

            oComentarioBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return new BEComentarioConAutor(
                oComentarioBE,
                ServicioLectorFila.LeerTexto(Item, "Usuario"),
                ServicioLectorFila.LeerTexto(Item, "Plan"));
        }
    }
}
