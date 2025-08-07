using Microsoft.Data.SqlClient;
using System.Data;

namespace StudentApi.Data
{
    public class SqlDataAccess
    {
        private readonly string _connectionString;

        public SqlDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string storedProcedure, SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteScalar();
                }
            }
        }

        public DataTable ExecuteQuery(string storedProcedure, SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    command.Parameters.AddRange(parameters);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }
    }
}