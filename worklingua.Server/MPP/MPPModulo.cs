using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPModulo
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPModulo()
        {
            oDatos = new Acceso();
        }

        public List<BEModulo> ListarPorCurso(BECurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);

            List<BEModulo> ListaModuloBE = new List<BEModulo>();
            DataTable Dt = oDatos.Leer("sp_Modulo_Listar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaModuloBE.Add(new BEModulo(
                        ServicioLectorFila.LeerEntero(Item, "ModuloId"),
                        ServicioLectorFila.LeerEntero(Item, "CursoId"),
                        ServicioLectorFila.LeerTexto(Item, "Nombre"),
                        ServicioLectorFila.LeerTextoNulo(Item, "Descripcion"),
                        ServicioLectorFila.LeerTextoNulo(Item, "Contenido"),
                        ServicioLectorFila.LeerEntero(Item, "OrdenModulo"),
                        ServicioLectorFila.LeerBooleanoNulo(Item, "Activo")));
                }

                return ListaModuloBE;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Alta de un módulo. Un módulo no se modifica desde el ABM: se da de
        /// baja y se vuelve a cargar, así el avance ya registrado no cambia de
        /// sentido. Para restaurar una versión está ActualizarTextos.
        /// </summary>
        public int Guardar(BEModulo Objeto)
        {
            if (Objeto.ModuloId != 0)
            {
                throw new NotImplementedException(
                    "Un módulo se da de baja y se vuelve a cargar; para restaurar una versión usar ActualizarTextos.");
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Contenido", Objeto.Contenido);
            Hdatos.Add("@OrdenModulo", Objeto.OrdenModulo);

            object identidad = oDatos.LeerEscalar("sp_Modulo_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        /// <summary>
        /// Devuelve el objetivo, el contenido y el orden que tenía el módulo en
        /// una versión. No toca el nombre: es con lo que se reconcilian los
        /// módulos entre la versión y el curso.
        /// </summary>
        public bool ActualizarTextos(BEModulo Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ModuloId", Objeto.ModuloId);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Contenido", Objeto.Contenido);
            Hdatos.Add("@OrdenModulo", Objeto.OrdenModulo);

            object filas = oDatos.LeerEscalar("sp_Modulo_Modificar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool Baja(BEModulo Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ModuloId", Objeto.ModuloId);

            object filas = oDatos.LeerEscalar("sp_Modulo_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }
    }
}
