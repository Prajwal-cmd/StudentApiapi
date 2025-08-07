using Microsoft.AspNetCore.Mvc;
using StudentApi.Data;
using StudentApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace StudentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly SqlDataAccess _dataAccess;

        public StudentsController(SqlDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }


        [HttpPost]
        public async Task<IActionResult> AddStudent([FromForm] Student student, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { Errors = errors });
            }

            var checkParams = new[] { new SqlParameter("@Email", student.Email ?? (object)DBNull.Value) };
            var dataTable = _dataAccess.ExecuteQuery("sp_Student_GetAll", new SqlParameter[0]);
            if (dataTable.Rows.Cast<DataRow>().Any(row => row["Email"].ToString() == student.Email))
            {
                return Conflict(new { Error = "Email already exists in the database." });
            }

            if (Image != null && Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }
                student.ImageUrl = $"/images/{fileName}";
            }

            var parameters = new[]
            {
        new SqlParameter("@FirstName", student.FirstName ?? (object)DBNull.Value),
        new SqlParameter("@LastName", student.LastName ?? (object)DBNull.Value),
        new SqlParameter("@Email", student.Email ?? (object)DBNull.Value),
        new SqlParameter("@DateOfBirth", student.DateOfBirth),
        new SqlParameter("@Gender", student.Gender ?? (object)DBNull.Value),
        new SqlParameter("@Major", (object)student.Major ?? DBNull.Value),
        new SqlParameter("@ImageUrl", (object)student.ImageUrl ?? DBNull.Value)
    };

            try
            {
                decimal studentIdDecimal = (decimal)_dataAccess.ExecuteScalar("sp_Student_Add", parameters);
                int studentId = Convert.ToInt32(studentIdDecimal);
                return Ok(new { StudentId = studentId, Message = "Student added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while adding the student.", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromForm] Student student, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { Errors = errors });
            }

            if (id != student.StudentId) return BadRequest(new { Error = "Student ID mismatch." });

            var allStudents = _dataAccess.ExecuteQuery("sp_Student_GetAll", new SqlParameter[0]);
            var existingEmailStudent = allStudents.Rows.Cast<DataRow>()
                .FirstOrDefault(row => (int)row["StudentId"] != id && row["Email"].ToString() == student.Email);

            if (existingEmailStudent != null)
            {
                return Conflict(new { Error = "Email already exists in the database." });
            }

            var currentStudentParams = new[] { new SqlParameter("@StudentId", id) };
            var currentStudentData = _dataAccess.ExecuteQuery("sp_Student_GetById", currentStudentParams);

            if (currentStudentData.Rows.Count == 0)
            {
                return NotFound(new { Error = "Student not found." });
            }

            var currentImageUrl = currentStudentData.Rows[0]["ImageUrl"] != DBNull.Value
                ? currentStudentData.Rows[0]["ImageUrl"].ToString()
                : null;

            if (Image != null && Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }
                student.ImageUrl = $"/images/{fileName}";
            }
            else
            {
                student.ImageUrl = currentImageUrl;
            }

            var parameters = new[]
            {
        new SqlParameter("@StudentId", student.StudentId),
        new SqlParameter("@FirstName", student.FirstName ?? (object)DBNull.Value),
        new SqlParameter("@LastName", student.LastName ?? (object)DBNull.Value),
        new SqlParameter("@Email", student.Email ?? (object)DBNull.Value),
        new SqlParameter("@DateOfBirth", student.DateOfBirth),
        new SqlParameter("@Gender", student.Gender ?? (object)DBNull.Value),
        new SqlParameter("@Major", (object)student.Major ?? DBNull.Value),
        new SqlParameter("@ImageUrl", (object)student.ImageUrl ?? DBNull.Value)
    };

            try
            {
                _dataAccess.ExecuteNonQuery("sp_Student_Update", parameters);
                return Ok(new { Message = "Student updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while updating the student.", Details = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAllStudents()
        {
            var dataTable = _dataAccess.ExecuteQuery("sp_Student_GetAll", new SqlParameter[0]);
            var students = new List<Student>();
            foreach (DataRow row in dataTable.Rows)
            {
                students.Add(new Student
                {
                    StudentId = (int)row["StudentId"],
                    FirstName = row["FirstName"].ToString(),
                    LastName = row["LastName"].ToString(),
                    Email = row["Email"].ToString(),
                    DateOfBirth = (DateTime)row["DateOfBirth"],
                    Gender = row["Gender"].ToString(),
                    Major = row["Major"] != DBNull.Value ? row["Major"].ToString() : null,
                    CreatedDate = (DateTime)row["CreatedDate"],
                    ImageUrl = row["ImageUrl"] != DBNull.Value ? row["ImageUrl"].ToString() : null
                });
            }
            return Ok(students);
        }
        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            var parameters = new[] { new SqlParameter("@StudentId", id) };
            var dataTable = _dataAccess.ExecuteQuery("sp_Student_GetById", parameters);
            if (dataTable.Rows.Count == 0) return NotFound();
            var row = dataTable.Rows[0];
            var student = new Student
            {
                StudentId = (int)row["StudentId"],
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Email = row["Email"].ToString(),
                DateOfBirth = (DateTime)row["DateOfBirth"],
                Gender = row["Gender"].ToString(),
                Major = row["Major"] != DBNull.Value ? row["Major"].ToString() : null,
                CreatedDate = (DateTime)row["CreatedDate"],
                ImageUrl = row["ImageUrl"] != DBNull.Value ? row["ImageUrl"].ToString() : null
            };
            return Ok(student);
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var parameters = new[] { new SqlParameter("@StudentId", id) };
            try
            {
                _dataAccess.ExecuteNonQuery("sp_Student_Delete", parameters);
                return Ok(new { Message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while deleting the student.", Details = ex.Message });
            }
        }
    }
}