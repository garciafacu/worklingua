using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPActivoPedagogico
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPActivoPedagogico()
        {
            oDatos = new Acceso();
        }

        /// <summary>
        /// Todos los activos del curso, incluidos los dados de baja: el panel
        /// los muestra atenuados y permite reactivarlos.
        /// </summary>
        public List<BEActivoPedagogico> ListarPorCurso(BEActivoPedagogico Objeto)
        {
            List<BEActivoPedagogico> ListaBE = new List<BEActivoPedagogico>();

            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);

            DataTable Dt = oDatos.Leer("sp_ActivoPedagogico_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaBE.Add(Mapear(Item));
            }

            return ListaBE;
        }

        public BEActivoPedagogico ListarObjeto(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ActivoPedagogicoId", Objeto.ActivoPedagogicoId);

            DataTable Dt = oDatos.Leer("sp_ActivoPedagogico_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Alta(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@ModuloId", Objeto.ModuloId);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@TipoContenido", Objeto.TipoContenido);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@UrlArchivo", Objeto.UrlArchivo);

            // El parámetro se manda solo si hay estado: sin él el SP aplica su
            // default 'BORRADOR', que es lo que pide la subida normal. Mandarlo
            // en null escribiría NULL en una columna NOT NULL, no el default.
            if (!string.IsNullOrEmpty(Objeto.Estado))
            {
                Hdatos.Add("@Estado", Objeto.Estado);
            }

            object identidad = oDatos.LeerEscalar("sp_ActivoPedagogico_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        /// <summary>Publicar o volver a borrador.</summary>
        public bool CambiarEstado(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ActivoPedagogicoId", Objeto.ActivoPedagogicoId);
            Hdatos.Add("@Estado", Objeto.Estado);

            object filas = oDatos.LeerEscalar("sp_ActivoPedagogico_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        /// <summary>Baja lógica y reactivación.</summary>
        public bool CambiarActivo(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ActivoPedagogicoId", Objeto.ActivoPedagogicoId);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object filas = oDatos.LeerEscalar("sp_ActivoPedagogico_CambiarActivo", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEActivoPedagogico Mapear(DataRow Item)
        {
            BEActivoPedagogico oActivoBE = new BEActivoPedagogico();

            oActivoBE.ActivoPedagogicoId = ServicioLectorFila.LeerEntero(Item, "ActivoPedagogicoId");
            oActivoBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            oActivoBE.ModuloId = ServicioLectorFila.LeerEnteroNulo(Item, "ModuloId");
            oActivoBE.Modulo = ServicioLectorFila.LeerTextoNulo(Item, "Modulo");
            oActivoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oActivoBE.TipoContenido = ServicioLectorFila.LeerTextoNulo(Item, "TipoContenido");
            oActivoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oActivoBE.UrlArchivo = ServicioLectorFila.LeerTextoNulo(Item, "UrlArchivo");
            oActivoBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
            oActivoBE.Estado = ServicioLectorFila.LeerTextoNulo(Item, "Estado");

            return oActivoBE;
        }
    }
}
