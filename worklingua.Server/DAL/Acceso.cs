using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using worklingua.Server.Services;

namespace worklingua.Server.DAL
{
    public class Acceso
    {
        private SqlConnection oCnn;
        private SqlCommand Cmd;

        public Acceso()
        {
            oCnn = new SqlConnection(Configuracion.CadenaConexion);
        }

        public DataTable Leer(string Consulta, Hashtable Hdatos)
        {
            DataTable Dt = new DataTable();

            try
            {
                if (oCnn.State == ConnectionState.Closed)
                {
                    oCnn.Open();
                }

                Cmd = new SqlCommand(Consulta, oCnn);
                Cmd.CommandType = CommandType.StoredProcedure;

                if (Hdatos != null)
                {
                    foreach (string dato in Hdatos.Keys)
                    {
                        Cmd.Parameters.AddWithValue(dato, Hdatos[dato] ?? DBNull.Value);
                    }
                }

                SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                Da.Fill(Dt);

                return Dt;
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oCnn.State != ConnectionState.Closed)
                {
                    oCnn.Close();
                }
            }
        }

        public bool Escribir(string Consulta, Hashtable Hdatos)
        {
            try
            {
                if (oCnn.State == ConnectionState.Closed)
                {
                    oCnn.Open();
                }

                Cmd = new SqlCommand(Consulta, oCnn);
                Cmd.CommandType = CommandType.StoredProcedure;

                if (Hdatos != null)
                {
                    foreach (string dato in Hdatos.Keys)
                    {
                        Cmd.Parameters.AddWithValue(dato, Hdatos[dato] ?? DBNull.Value);
                    }
                }

                Cmd.ExecuteNonQuery();

                return true;
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (oCnn.State != ConnectionState.Closed)
                {
                    oCnn.Close();
                }
            }
        }

        public object LeerEscalar(string Consulta, Hashtable Hdatos)
        {
            try
            {
                if (oCnn.State == ConnectionState.Closed)
                {
                    oCnn.Open();
                }

                Cmd = new SqlCommand(Consulta, oCnn);
                Cmd.CommandType = CommandType.StoredProcedure;

                if (Hdatos != null)
                {
                    foreach (string dato in Hdatos.Keys)
                    {
                        Cmd.Parameters.AddWithValue(dato, Hdatos[dato] ?? DBNull.Value);
                    }
                }

                object resultado = Cmd.ExecuteScalar();

                return resultado == DBNull.Value ? null : resultado;
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
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