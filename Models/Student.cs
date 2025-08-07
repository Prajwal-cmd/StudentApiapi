using System.ComponentModel.DataAnnotations;

namespace StudentApi.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [RegularExpression(@"^([a-zA-Z0-9._%+-]+)@([a-zA-Z0-9.-]+\.[a-zA-Z]{2,})$", ErrorMessage = "Email must be a valid address (e.g., user@domain.com).")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
        public string Gender { get; set; }

        [StringLength(50, ErrorMessage = "Major cannot exceed 50 characters.")]
        public string? Major { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters.")]
        public string? ImageUrl { get; set; } 
    }
}