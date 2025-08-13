using System;
using System.Collections.Generic;

namespace StudentApi.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; } // Required for keyset pagination
        public string Name { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; } // Required for keyset pagination
    }
}