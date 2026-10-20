using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPEtiqueta
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPEtiqueta()
        {
            oDatos = new Acceso();
        }

        public List<BEEtiqueta> Listar()
        {
            List<BEEtiqueta> ListaBE = new List<BEEtiqueta>();

            Hdatos = new Hashtable();

            DataTable Dt = oDatos.Leer("sp_Etiqueta_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BEEtiqueta oEtiquetaBE = new BEEtiqueta();

                oEtiquetaBE.EtiquetaId = ServicioLectorFila.LeerEntero(Item, "EtiquetaId");
                oEtiquetaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
                oEtiquetaBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
                oEtiquetaBE.FechaAlta = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAlta");

                ListaBE.Add(oEtiquetaBE);
            }

            return ListaBE;
        }

        public int Alta(BEEtiqueta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);

            object identidad = oDatos.LeerEscalar("sp_Etiqueta_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public List<BEEtiquetaCurso> ListarAsignadas(int cursoId)
        {
            List<BEEtiquetaCurso> ListaBE = new List<BEEtiquetaCurso>();

            Hdatos = new Hashtable();

            if (cursoId != 0)
            {
                Hdatos.Add("@CursoId", cursoId);
            }

            DataTable Dt = oDatos.Leer("sp_CursoEtiqueta_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BEEtiquetaCurso oAsignadaBE = new BEEtiquetaCurso();

                oAsignadaBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
                oAsignadaBE.EtiquetaId = ServicioLectorFila.LeerEntero(Item, "EtiquetaId");
                oAsignadaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");

                ListaBE.Add(oAsignadaBE);
            }

            return ListaBE;
        }

        public void Reemplazar(int cursoId, List<int> etiquetaIds)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", cursoId);
            Hdatos.Add("@EtiquetaIds", string.Join(",", etiquetaIds));

            oDatos.LeerEscalar("sp_CursoEtiqueta_Reemplazar", Hdatos);
        }
    }
}
