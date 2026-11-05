using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using worklingua.Server.Services;

namespace worklingua.Server.DAL
{
    public class Acceso
    {
        private SqlConnection oCnn;

        public Acceso()
        {
            oCnn = new SqlConnection(Configuracion.CadenaConexion);
        }

        public DataTable Leer(string Consulta, Hashtable Hdatos)
        {
            return Ejecutar(Consulta, Hdatos, Cmd =>
            {
                DataTable Dt = new DataTable();
                new SqlDataAdapter(Cmd).Fill(Dt);
                return Dt;
            });
        }

        public bool Escribir(string Consulta, Hashtable Hdatos)
        {
            return Ejecutar(Consulta, Hdatos, Cmd =>
            {
                Cmd.ExecuteNonQuery();
                return true;
            });
        }

        public object LeerEscalar(string Consulta, Hashtable Hdatos)
        {
            return Ejecutar(Consulta, Hdatos, Cmd =>
            {
                object resultado = Cmd.ExecuteScalar();
                return resultado == DBNull.Value ? null : resultado;
            });
        }

        private T Ejecutar<T>(string Consulta, Hashtable Hdatos, Func<SqlCommand, T> accion)
        {
            try
            {
                if (oCnn.State == ConnectionState.Closed)
                {
                    oCnn.Open();
                }

                SqlCommand Cmd = new SqlCommand(Consulta, oCnn);
                Cmd.CommandType = CommandType.StoredProcedure;

                if (Hdatos != null)
                {
                    foreach (string dato in Hdatos.Keys)
                    {
                        Cmd.Parameters.AddWithValue(dato, Hdatos[dato] ?? DBNull.Value);
                    }
                }

                return accion(Cmd);
            }
            finally
            {
                if (oCnn.State != ConnectionState.Closed)
                {
                    oCnn.Close();
                }
            }
        }
    }
}
