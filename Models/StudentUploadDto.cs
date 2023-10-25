using Nest;
using System.ComponentModel.DataAnnotations;

namespace StudentRegistration.Models
{
    public class StudentUploadDto
    {

        [Key]
        public int AdmissionNo { get; set; }
        [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = null!;


        public string? Address { get; set; }

        [Required(ErrorMessage = "Please enter DOB")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please enter Selected Course")]
        [Display(Name = "Selected Course")]
        public string SelectedCourse { get; set; } = null!;

        [Required(ErrorMessage = "Please enter your secured grade")]
        [Display(Name = "Secured Grade")]
        [Range(75, 100, ErrorMessage = "Grade Must be above 75% to register")]
        public string SecuredGrade { get; set; } = null!;

        public IFormFile Image { get; set; }

        public IFormFile File { get; set; }
    }
}

