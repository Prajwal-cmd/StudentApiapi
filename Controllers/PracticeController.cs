using Microsoft.AspNetCore.Mvc;
using StudentApi.Data;
using StudentApi.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StudentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeController : ControllerBase
    {
        private readonly PracticeDataAccess _dataAccess;

        public PracticeController(PracticeDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _dataAccess.GetAllDepartments();
            var dto = departments.Select(d => new { d.Id, d.Name, d.Description });
            return Ok(dto);
        }

        [HttpGet("department/{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _dataAccess.GetDepartmentById(id);
            if (department == null)
            {
                return NotFound();
            }
            return Ok(new { department.Id, department.Name, department.Description });
        }

        [HttpGet("department/{id}/employees")]
        public async Task<IActionResult> GetEmployeesByDepartment(
            int id,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? lastHireDate = null,
            [FromQuery] int? lastId = null)
        {
            if (pageSize < 1)
            {
                return BadRequest("Page size must be positive.");
            }

            var employees = await _dataAccess.GetEmployeesByDepartmentId(id, pageSize, lastHireDate, lastId);

            var pagedResult = new PagedResult<EmployeeDto>
            {
                Items = employees.Select(e => new EmployeeDto
                {
                    Id = e.Id, // We need to expose Id for the next request
                    Name = e.Name,
                    Position = e.Position,
                    Salary = e.Salary,
                    HireDate = e.HireDate
                }).ToList(),
            };

            return Ok(pagedResult);
        }
    }
}