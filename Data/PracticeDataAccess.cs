using Microsoft.Data.SqlClient;
using StudentApi.Models;
using System.Data;
using System.Threading.Tasks;

namespace StudentApi.Data
{
    public class PracticeDataAccess
    {
        private readonly string _connectionString;

        public PracticeDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Department>> GetAllDepartments()
        {
            var departments = new List<Department>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Name, Description FROM Practice_Departments", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            departments.Add(new Department
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Description = reader.GetString("Description")
                            });
                        }
                    }
                }
            }
            return departments;
        }

        public async Task<Department> GetDepartmentById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Name, Description FROM Practice_Departments WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Department
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Description = reader.GetString("Description")
                            };
                        }
                    }
                }
            }
            return null;
        }


        public async Task<List<Employee>> GetEmployeesByDepartmentId(int departmentId, int pageSize, DateTime? lastHireDate, int? lastId)
        {
            var employees = new List<Employee>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Build the WHERE clause for keyset pagination
                string whereClause = "WHERE DepartmentId = @DepartmentId";
                if (lastHireDate.HasValue && lastId.HasValue)
                {
                    // The core keyset logic: find records where the hire date is greater,
                    // or where the hire date is the same and the ID is greater.
                    whereClause += " AND (HireDate > @LastHireDate OR (HireDate = @LastHireDate AND Id > @LastId))";
                }

                // Construct the full SQL query
                string sqlQuery = $@"
                    SELECT TOP (@PageSize) Id, DepartmentId, Name, Position, Salary, HireDate
                    FROM Practice_Employees
                    {whereClause}
                    ORDER BY HireDate, Id";

                using (var command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentId", departmentId);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    if (lastHireDate.HasValue && lastId.HasValue)
                    {
                        command.Parameters.AddWithValue("@LastHireDate", lastHireDate.Value);
                        command.Parameters.AddWithValue("@LastId", lastId.Value);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                Id = reader.GetInt32("Id"),
                                DepartmentId = reader.GetInt32("DepartmentId"),
                                Name = reader.GetString("Name"),
                                Position = reader.GetString("Position"),
                                Salary = reader.GetDecimal("Salary"),
                                HireDate = reader.GetDateTime("HireDate")
                            });
                        }
                    }
                }
            }
            return employees;
        }
    }
}