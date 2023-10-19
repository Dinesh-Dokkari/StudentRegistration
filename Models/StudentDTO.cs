using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentRegistration.Models;

public partial class StudentDTO
{
    [Key]
    public int AdmissionNo { get; set; }

    [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
    [Display(Name = "Student Name")]
    public string StudentName { get; set; } = null!;


    public string? Address { get; set; }

    [Column(TypeName = "date")]
    [Required(ErrorMessage = "Please enter DOB")]
    [Display(Name = "Date of Birth")]

    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Please enter Selected Course")]
    [Display(Name = "Selected Course")]
    public string SelectedCourse { get; set; } = null!;

    [Required(ErrorMessage = "Please enter your secured grade")]
    [Display(Name = "Secured Grade")]
    [Range(75, 100, ErrorMessage = "Grade Must be above 75% to register")]

    public string SecuredGrade { get; set; } = null!;
}
