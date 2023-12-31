﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    [Display(Name = "Admission Number")]

    public int AdmissionNo { get; set; }

    [Required(ErrorMessage = "Please enter name"), MaxLength(30)]
    [Display(Name = "Student Name")]
    public string StudentName { get; set; } = null!;


    public string? Address { get; set; }

    [Required(ErrorMessage = "Please enter DOB")]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    [Range(typeof(DateTime), "01/01/2000", "01/01/2010",
        ErrorMessage = "Value for {0} must be between {1} and {2}")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Please enter Selected Course")]
    [Display(Name = "Selected Course")]
    public string SelectedCourse { get; set; } = null!;

    [Required(ErrorMessage = "Please enter your secured grade")]
    [Display(Name = "Secured Grade")]
    [Range(75, 100, ErrorMessage = "Grade Must be above 75% to register")]

    public string SecuredGrade { get; set; } = null!;

    public string? ImagePath { get; set; }

    public string? FilePath { get; set; }
}
