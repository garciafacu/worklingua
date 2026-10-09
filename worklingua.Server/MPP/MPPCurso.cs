using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPCurso
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPCurso()
        {
            oDatos = new Acceso();
        }

        public List<BECurso> Buscar(BEFiltroCurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Idioma", Objeto.Idioma);
            Hdatos.Add("@Nivel", Objeto.Nivel);
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@Sector", Objeto.Sector);
            Hdatos.Add("@IncluirFueraDeVentana", Objeto.IncluirFueraDeVentana);

            if (Objeto.EtiquetaId.HasValue && Objeto.EtiquetaId.Value != 0)
            {
                Hdatos.Add("@EtiquetaId", Objeto.EtiquetaId.Value);
            }

            List<BECurso> ListaCursoBE = new List<BECurso>();
            DataTable Dt = oDatos.Leer("sp_Curso_Buscar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaCursoBE.Add(Mapear(Item));
                }

                return ListaCursoBE;
            }
            else
            {
                return null;
            }
        }

        public List<BECurso> ListarTodoConBajas(BEFiltroCurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BECurso> ListaCursoBE = new List<BECurso>();
            DataTable Dt = oDatos.Leer("sp_Curso_ListarTodos", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaCursoBE.Add(Mapear(Item));
                }

                return ListaCursoBE;
            }
            else
            {
                return null;
            }
        }

        public List<string> ListarIdiomas(BEFiltroCurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<string> ListaIdioma = new List<string>();
            DataTable Dt = oDatos.Leer("sp_Curso_ListarIdiomas", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaIdioma.Add(ServicioLectorFila.LeerTexto(Item, "Idioma"));
                }

                return ListaIdioma;
            }
            else
            {
                return null;
            }
        }

        public BECurso ListarObjeto(BECurso Objeto, BEFiltroCurso oAlcance)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@EmpresaId", oAlcance.EmpresaId);

            DataTable Dt = oDatos.Leer("sp_Curso_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BECurso Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.CursoId != 0)
            {
                Hdatos.Add("@CursoId", Objeto.CursoId);
                Hdatos.Add("@Idioma", Objeto.Idioma);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@Descripcion", Objeto.Descripcion);
                Hdatos.Add("@Nivel", Objeto.Nivel);
                Hdatos.Add("@DuracionHoras", Objeto.DuracionHoras);
                AgregarClasificacion(Objeto);

                oDatos.LeerEscalar("sp_Curso_Modificar", Hdatos);

                return Objeto.CursoId;
            }

            Hdatos.Add("@Idioma", Objeto.Idioma);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Nivel", Objeto.Nivel);
            Hdatos.Add("@DuracionHoras", Objeto.DuracionHoras);
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            AgregarClasificacion(Objeto);

            object identidad = oDatos.LeerEscalar("sp_Curso_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        /// <summary>
        /// Sector y ventana de despliegue (CU-004-005 y CU-004-006). Los
        /// parámetros se omiten cuando no hay valor: el SP los tiene con
        /// default NULL y así "sin programar" no necesita un DBNull explícito.
        /// </summary>
        private void AgregarClasificacion(BECurso Objeto)
        {
            if (!string.IsNullOrEmpty(Objeto.Sector))
            {
                Hdatos.Add("@Sector", Objeto.Sector);
            }

            if (Objeto.FechaPublicacion.HasValue)
            {
                Hdatos.Add("@FechaPublicacion", Objeto.FechaPublicacion.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (Objeto.FechaFin.HasValue)
            {
                Hdatos.Add("@FechaFin", Objeto.FechaFin.Value.ToDateTime(TimeOnly.MinValue));
            }
        }

        public bool Baja(BECurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);

            object filas = oDatos.LeerEscalar("sp_Curso_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BECurso Mapear(DataRow Item)
        {
            BECurso oCursoBE = new BECurso();

            oCursoBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            oCursoBE.Idioma = ServicioLectorFila.LeerTexto(Item, "Idioma");
            oCursoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oCursoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oCursoBE.Nivel = ServicioLectorFila.LeerTexto(Item, "Nivel");
            oCursoBE.DuracionHoras = ServicioLectorFila.LeerEnteroNulo(Item, "DuracionHoras");
            oCursoBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");
            oCursoBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oCursoBE.EmpresaId = ServicioLectorFila.LeerEnteroNulo(Item, "EmpresaId");
            oCursoBE.Sector = ServicioLectorFila.LeerTextoNulo(Item, "Sector");
            oCursoBE.FechaPublicacion = ServicioLectorFila.LeerFechaNula(Item, "FechaPublicacion");
            oCursoBE.FechaFin = ServicioLectorFila.LeerFechaNula(Item, "FechaFin");

            return oCursoBE;
        }
    }
}
