using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Business_Logic_Layer.Services;
using Data_Access_Layer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Data;
using StudentRegistration.Models;
using System.Web;
using Elasticsearch.Net;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.IO.Image;

namespace StudentRegistration.Controllers
{
    public class StudentsController : Controller
    {
        //private readonly StudentDbContext _context;
        private readonly IStudentService _service;
        private readonly IMapper _map;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public StudentsController(IStudentService service, IMapper map, Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment)
        {
            _service = service;
            _map = map;
            _environment = Environment;
        }

        // GET: Students
        public async Task<IActionResult> Index(string searchString ,int pg=1)
        {
            //if (isUploaded)
            //{
            //    ViewBag.Uploaded = "Student Uploaded Successfully";
            //}
            try
            {
                var Students = await _service.GetAll();

                if (!String.IsNullOrEmpty(searchString) && int.TryParse(searchString, out int i))
                {   

                    Students = Students.Where(s=>s.AdmissionNo == i);
                    if (Students.Count() == 0)
                    {
                        ViewBag.notfound = "Admission Number " + i + " Not Found";

                    }
                }
                else if(!String.IsNullOrEmpty(searchString))
                {
                    ViewBag.Search = "Please Enter a valid Admission Number to get Student Data";

                }

                if (Students != null)
                {
                    var StudentsDTO = _map.Map<IEnumerable<StudentDTO>>(Students);


                    const int pageSize = 5;
                    if (pg < 1)
                    {
                        pg = 1;
                    }

                    int recsCount = StudentsDTO.Count();

                    var pager = new Pager(recsCount, pg, pageSize);

                    int recSkip = (pg - 1) * pageSize;

                    var data = StudentsDTO.Skip(recSkip).Take(pager.PageSize).ToList();

                    this.ViewBag.Pager = pager;

                    return View(data);
                }
                else
                {
                    return View();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return NotFound();
            }
        }



        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var students = await _service.GetAll();
                if (id == null || students.Count() == 0)
                {
                    return NotFound();
                }
                var Student = await _service.Details(id, new List<string>(),a => a.AdmissionNo == id);
                if (Student == null)
                {
                    return NotFound();
                }
                var ModelDTO = _map.Map<StudentDTO>(Student);


                return View(ModelDTO);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentUploadDto student)
        {
            var now = DateTime.Now;
            ViewBag.Date=now;


            if (ModelState.IsValid)
            {
                string uniqueImageName = null;
                string uniqueFileName = null;


                if(student.Image != null)
                {
                    string uploadfolder = Path.Combine(_environment.WebRootPath, "Images");
                    uniqueImageName = Guid.NewGuid().ToString() + "_" + student.Image.FileName;

                    var extension= Path.GetExtension(uniqueImageName);

                    if (extension.ToLower().Equals(".png") || extension.ToLower().Equals(".jpg")
                        || extension.ToLower().Equals(".jpeg"))
                    {

                        string filepath = Path.Combine(uploadfolder, uniqueImageName);
                        student.Image.CopyTo(new FileStream(filepath, FileMode.Create));
                    }
                    else
                    {
                        ViewBag.Image = "Please Upload .png ,.jpg,.jpeg formats only!!";
                    }
                }
                if (student.File != null)
                {
                    string uploadfolder = Path.Combine(_environment.WebRootPath, "Files");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + student.File.FileName;

                    var extension = Path.GetExtension(uniqueFileName);

                    if (extension.ToLower().Equals(".pdf"))
                    {
                        string filepath = Path.Combine(uploadfolder, uniqueFileName);
                        student.File.CopyTo(new FileStream(filepath, FileMode.Create));
                    }
                    else
                    {
                        ViewBag.File = "Please Upload .pdf format only!!";
                        return View(student);
                    }
                }


                    StudentDTO studentDTO = new StudentDTO
                    {
                        AdmissionNo = student.AdmissionNo,
                        StudentName = student.StudentName,
                        Address = student.Address,
                        DateOfBirth = student.DateOfBirth,
                        SelectedCourse = student.SelectedCourse,
                        SecuredGrade = student.SecuredGrade,
                        ImagePath = uniqueImageName,
                        FilePath = uniqueFileName
                    };

                    var newstudent = _map.Map<Student>(studentDTO);
                    int result = await _service.Create(newstudent);

                return RedirectToAction("Index");

            }
            return View();

        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                var student = await _service.GetAll();

                if (id == null || student.Count() == 0)
                {
                    return NotFound();
                }
                var Stu = await _service.Edit(id);
                var StudentDTO = _map.Map<StudentDTO>(Stu);
                var StudentUploaddto = _map.Map<StudentUploadDto>(StudentDTO);
                if (StudentUploaddto == null)
                {
                    return NotFound();
                }
                return View(StudentUploaddto);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,StudentUploadDto student)
        {
            if (id != student.AdmissionNo)
            {
                return NotFound();
            }
            if (ModelState.IsValid == false)
            {
                try
                {
                    string uniqueImageName = null;
                    string uniqueFileName = null;

                    if (student.Image != null)
                    {
                        string uploadfolder = Path.Combine(_environment.WebRootPath, "Images");
                        uniqueImageName = Guid.NewGuid().ToString() + "_" + student.Image.FileName;

                        var extension = Path.GetExtension(uniqueImageName);

                        if (extension.ToLower().Equals(".png") || extension.ToLower().Equals(".jpg")
                            || extension.ToLower().Equals(".jpeg"))
                        {

                            string filepath = Path.Combine(uploadfolder, uniqueImageName);
                            student.Image.CopyTo(new FileStream(filepath, FileMode.Create));
                        }
                        else
                        {
                            ViewBag.Image = "Please Upload .png ,.jpg,.jpeg formats only!!";
                        }
                    }
                    if (student.File != null)
                    {
                        string uploadfolder = Path.Combine(_environment.WebRootPath, "Files");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + student.File.FileName;

                        var extension = Path.GetExtension(uniqueFileName);

                        if (extension.ToLower().Equals(".pdf"))
                        {
                            string filepath = Path.Combine(uploadfolder, uniqueFileName);
                            student.File.CopyTo(new FileStream(filepath, FileMode.Create));
                        }
                        else
                        {
                            ViewBag.File = "Please Upload .pdf format only!!";
                            return View(student);
                        }
                    }
                    StudentDTO studentDTO = new StudentDTO
                    {
                        AdmissionNo = student.AdmissionNo,
                        StudentName = student.StudentName,
                        Address = student.Address,
                        DateOfBirth = student.DateOfBirth,
                        SelectedCourse = student.SelectedCourse,
                        SecuredGrade = student.SecuredGrade,
                        ImagePath = uniqueImageName,
                        FilePath = uniqueFileName
                    };

                    var newstudent = _map.Map<Student>(studentDTO);

                    int result = await _service.Edit(id, newstudent);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;

                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        //GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var students = await _service.GetAll();


                if (id == null || students.Count() == 0)
                {
                    return NotFound();
                }
                var student = await _service.Delete(id, new List<string>(),a => a.AdmissionNo == id);
                if (student == null)
                {
                    return NotFound();
                }
                var studentDTO = _map.Map<StudentDTO>(student);
                return View(studentDTO);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }

        }

        //POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var students = await _service.GetAll();



                if (students.Count() == 0)
                {
                    return Problem("Entity set Students is null.");
                }
                int result = await _service.DeleteConfirmed(id);
            return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        public async Task<IActionResult> DownloadPDF(int id )
        {
            var students = await _service.GetAll();
            if (id == null || students.Count() == 0)
            {
                return NotFound();
            }
            var student = await _service.Details(id, new List<string>(), a => a.AdmissionNo == id);
            if (student == null)
            {
                return NotFound();
            }
            var model = _map.Map<StudentDTO>(student);

            MemoryStream memoryStream = new MemoryStream();

            PdfWriter writer = new PdfWriter(memoryStream);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            Paragraph header = new Paragraph("UNIVERSITY JOINING APPLICATION")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(20);

            document.Add(header);

            Image img1 = new Image(ImageDataFactory
                .Create(Url.Content(@"C:\Users\DDOKKARI\Pictures\images.jpg")));
            document.Add(img1);

            LineSeparator ls = new LineSeparator(new SolidLine());
            Paragraph subheader = new Paragraph("Enroll now to the most Reputed Universities online!!!")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(15);
            document.Add(subheader);

            document.Add(ls);
            Paragraph paragraph2 = new Paragraph("Uploaded Image");
            document.Add(paragraph2);

            Image img2 = new Image(ImageDataFactory
                .Create(Url.Content(@"C:\Users\DDOKKARI\Desktop\Projects\StudentRegistration\wwwroot\Images\" + ($"{model.ImagePath}")
                )));
            document.Add(img2);

            Paragraph paragraph1 = new Paragraph("The Details You entered for the Registration are as follows : ");
            document.Add(paragraph1);

            document.Add(new Paragraph(
                $"Student Admission Number : {model.AdmissionNo}\n\nStudent Name : {model.StudentName}\n\n Student Address : {model.Address}\n\n Student Date_Of_Birth : {model.DateOfBirth}\n\n Student Selected Course : {model.SelectedCourse}\n\n Student Secured Grade : {model.SecuredGrade}"
                    ));
            document.Close();
            byte[] pdfBytes = memoryStream.ToArray();
            System.IO.File.WriteAllBytes($"{model.StudentName}" + ".pdf", pdfBytes);

            return File(pdfBytes, "application/pdf", $"{model.StudentName}"+".pdf");




        }
        //private bool StudentExists(int id)
        //{
        //    return (_context.Students?.Any(e => e.AdmissionNo == id)).GetValueOrDefault();
        //}
    }
}
