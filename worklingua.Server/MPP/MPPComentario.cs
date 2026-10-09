using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPComentario
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPComentario()
        {
            oDatos = new Acceso();
        }

        public List<BEComentarioConAutor> ListarTodo(BEComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);

            List<BEComentarioConAutor> ListaComentarioBE = new List<BEComentarioConAutor>();
            DataTable Dt = oDatos.Leer("sp_Comentario_Listar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaComentarioBE.Add(Mapear(Item));
                }

                return ListaComentarioBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEComentarioConAutor> Buscar(BEFiltroComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@FechaDesde", Objeto.Desde);
            Hdatos.Add("@FechaHasta", Objeto.Hasta);

            List<BEComentarioConAutor> ListaComentarioBE = new List<BEComentarioConAutor>();
            DataTable Dt = oDatos.Leer("sp_Comentario_Buscar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaComentarioBE.Add(Mapear(Item));
                }

                return ListaComentarioBE;
            }
            else
            {
                return null;
            }
        }

        public BEComentarioConAutor ListarObjeto(BEComentario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ComentarioId", Objeto.ComentarioId);

            DataTable Dt = oDatos.Leer("sp_Comentario_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
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

            DataTable Dt = oDatos.Leer("sp_Comentario_ObtenerValoracion", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
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
