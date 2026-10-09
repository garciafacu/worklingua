using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPVersionContenido
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPVersionContenido()
        {
            oDatos = new Acceso();
        }

        /// <summary>
        /// Guarda el punto de restauración. El número de versión lo calcula el
        /// SP dentro de su propia transacción.
        /// </summary>
        public int Alta(BEVersionContenido Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@ContenidoXml", Objeto.ContenidoXml);
            Hdatos.Add("@Observaciones", Objeto.Observaciones);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object identidad = oDatos.LeerEscalar("sp_VersionContenido_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        /// <summary>La línea de tiempo, sin el XML.</summary>
        public List<BEVersionContenido> ListarPorCurso(int cursoId)
        {
            List<BEVersionContenido> ListaBE = new List<BEVersionContenido>();

            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", cursoId);

            DataTable Dt = oDatos.Leer("sp_VersionContenido_ListarPorCurso", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaBE.Add(Mapear(Item));
            }

            return ListaBE;
        }

        public BEVersionContenido ListarObjeto(int versionContenidoId)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@VersionContenidoId", versionContenidoId);

            DataTable Dt = oDatos.Leer("sp_VersionContenido_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                return null;
            }

            BEVersionContenido oVersionBE = Mapear(Dt.Rows[0]);

            oVersionBE.ContenidoXml = ServicioLectorFila.LeerTextoNulo(Dt.Rows[0], "ContenidoXml");

            return oVersionBE;
        }

        /// <summary>
        /// Borra el punto de restauración. Es físico: una versión no tiene
        /// nada colgando y el pedido es limpiar el historial.
        /// </summary>
        public bool Baja(int versionContenidoId)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@VersionContenidoId", versionContenidoId);

            object filas = oDatos.LeerEscalar("sp_VersionContenido_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEVersionContenido Mapear(DataRow Item)
        {
            BEVersionContenido oVersionBE = new BEVersionContenido();

            oVersionBE.VersionContenidoId = ServicioLectorFila.LeerEntero(Item, "VersionContenidoId");
            oVersionBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            oVersionBE.NumeroVersion = ServicioLectorFila.LeerTextoNulo(Item, "NumeroVersion");
            oVersionBE.FechaVersion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaVersion");
            oVersionBE.Observaciones = ServicioLectorFila.LeerTextoNulo(Item, "Observaciones");
            oVersionBE.UsuarioId = ServicioLectorFila.LeerEnteroNulo(Item, "UsuarioId");
            oVersionBE.Autor = ServicioLectorFila.LeerTextoNulo(Item, "Autor");
            oVersionBE.LargoXml = ServicioLectorFila.LeerEntero(Item, "LargoXml");

            return oVersionBE;
        }
    }
}
