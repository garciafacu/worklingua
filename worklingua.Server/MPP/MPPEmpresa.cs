using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPEmpresa
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPEmpresa()
        {
            oDatos = new Acceso();
        }

        public List<BEEmpresa> ListarTodo()
        {
            return Listar("sp_Empresa_Listar");
        }

        public List<BEEmpresa> ListarSeleccionables()
        {
            return Listar("sp_Empresa_ListarSeleccionables");
        }

        public BEEmpresa ListarObjeto(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            DataTable Dt = oDatos.Leer("sp_Empresa_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.EmpresaId != 0)
            {
                Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
                Hdatos.Add("@RazonSocial", Objeto.RazonSocial);
                Hdatos.Add("@Email", Objeto.Email);
                Hdatos.Add("@Telefono", Objeto.Telefono);
                Hdatos.Add("@Direccion", Objeto.Direccion);
                Hdatos.Add("@Ciudad", Objeto.Ciudad);
                Hdatos.Add("@Provincia", Objeto.Provincia);
                Hdatos.Add("@Pais", Objeto.Pais);

                oDatos.LeerEscalar("sp_Empresa_Modificar", Hdatos);

                return Objeto.EmpresaId;
            }

            Hdatos.Add("@RazonSocial", Objeto.RazonSocial);
            Hdatos.Add("@CUIT", Objeto.CUIT);
            Hdatos.Add("@Email", Objeto.Email);
            Hdatos.Add("@Telefono", Objeto.Telefono);
            Hdatos.Add("@Direccion", Objeto.Direccion);
            Hdatos.Add("@Ciudad", Objeto.Ciudad);
            Hdatos.Add("@Provincia", Objeto.Provincia);
            Hdatos.Add("@Pais", Objeto.Pais);
            Hdatos.Add("@Seleccionable", Objeto.Seleccionable);

            object identidad = oDatos.LeerEscalar("sp_Empresa_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            object filas = oDatos.LeerEscalar("sp_Empresa_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BEEmpresa> Listar(string Consulta)
        {
            List<BEEmpresa> ListaEmpresaBE = new List<BEEmpresa>();
            DataTable Dt = oDatos.Leer(Consulta, null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaEmpresaBE.Add(Mapear(Item));
                }

                return ListaEmpresaBE;
            }
            else
            {
                return null;
            }
        }

        private BEEmpresa Mapear(DataRow Item)
        {
            BEEmpresa oEmpresaBE = new BEEmpresa();

            oEmpresaBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oEmpresaBE.RazonSocial = ServicioLectorFila.LeerTexto(Item, "RazonSocial");
            oEmpresaBE.CUIT = ServicioLectorFila.LeerTexto(Item, "CUIT").Trim();
            oEmpresaBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oEmpresaBE.Telefono = ServicioLectorFila.LeerTextoNulo(Item, "Telefono");
            oEmpresaBE.Direccion = ServicioLectorFila.LeerTextoNulo(Item, "Direccion");
            oEmpresaBE.Ciudad = ServicioLectorFila.LeerTextoNulo(Item, "Ciudad");
            oEmpresaBE.Provincia = ServicioLectorFila.LeerTextoNulo(Item, "Provincia");
            oEmpresaBE.Pais = ServicioLectorFila.LeerTextoNulo(Item, "Pais");
            oEmpresaBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oEmpresaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");
            oEmpresaBE.Protegido = ServicioLectorFila.LeerBooleano(Item, "Protegido");
            oEmpresaBE.Seleccionable = ServicioLectorFila.LeerBooleano(Item, "Seleccionable");

            return oEmpresaBE;
        }
    }
}
