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
using System.IO;

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

        public async Task<IActionResult> Index(string searchString ,string sortOrder,int pagesizeinput=5, int pg = 1)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewData["AddressSortParm"] = sortOrder == "Address" ? "Address_desc" : "Address";

            ViewBag.pagesize=pagesizeinput;
            try
            {
                var Students = await _service.GetAll();

                if (!String.IsNullOrEmpty(searchString) && int.TryParse(searchString, out int i))
                {
                    Students = Students.Where(s => s.AdmissionNo == i);
                    if (Students.Count() == 0)
                    {
                        ViewBag.notfound = "Admission Number " + i + " Not Found";
                    }
                }

                else if (!String.IsNullOrEmpty(searchString))
                {
                    searchString=searchString.ToLower();

                    Students = Students.Where(s=>s.StudentName.ToLower().Contains(searchString) ||
                                s.Address.ToLower().Contains(searchString) || s.SelectedCourse.ToLower().Contains(searchString) ||
                                s.DateOfBirth.ToString().Contains(searchString));
                    if (Students.Count() == 0)
                    {
                        ViewBag.notfound = "Please enter valid information";
                    }
                }

                switch (sortOrder)
                {
                    case "Name_desc":
                        Students = Students.OrderByDescending(s => s.StudentName);
                        break;
                    case "Address":
                        Students = Students.OrderBy(s => s.Address);
                        break;
                    case "Address_desc":
                        Students = Students.OrderByDescending(s => s.Address);
                        break;
                    default:
                        Students = Students.OrderBy(s => s.StudentName);
                        break;
                }


                if (Students != null)
                {
                    var StudentsDTO = _map.Map<IEnumerable<StudentDTO>>(Students);
                    int recordCount = StudentsDTO.Count();
                    var paging = new Paging(recordCount, pg, pagesizeinput);
                    int recSkip = (pg - 1) * pagesizeinput;
                    var data = StudentsDTO.Skip(recSkip).Take(paging.PageSize).ToList();
                    this.ViewBag.Pager = paging;
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentUploadDto student)
        {
            var now = DateTime.Now;
            ViewBag.Date=now.Date;


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

        public async Task<IActionResult> Edit(int? id)
        {
            var student = await _service.Edit(id);

            StudentEditDto studentEditModel = new StudentEditDto
            {
                AdmissionNo = student.AdmissionNo,
                StudentName = student.StudentName,
                Address = student.Address,
                DateOfBirth = student.DateOfBirth,
                SelectedCourse = student.SelectedCourse,
                SecuredGrade = student.SecuredGrade,
                ExistingImagePath = student.ImagePath,
                ExistingFilePath = student.FilePath

            };
            if (student.ImagePath != null && student.FilePath != null)
            {

                String str1 = student.ImagePath;
                String str2 = student.FilePath;
                String[] strlist = str1.Split("_");
                ViewBag.ImagePath = strlist[1];

                String[] strlist2 = str2.Split("_");
                ViewBag.FilePath = strlist2[1];
            }

            return View(studentEditModel);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentEditDto model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var student = await _service.Edit(id);
                    student.AdmissionNo = model.AdmissionNo;
                    student.StudentName = model.StudentName;
                    student.Address = model.Address;
                    student.SelectedCourse = model.SelectedCourse;
                    student.SecuredGrade = model.SecuredGrade;

                    string uniqueImageName = null;

                    if (model.Image != null)
                    {
                        string uploadfolder = Path.Combine(_environment.WebRootPath, "Images");
                        uniqueImageName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;

                        var extension = Path.GetExtension(uniqueImageName);

                        if (extension.ToLower().Equals(".png") || extension.ToLower().Equals(".jpg")
                            || extension.ToLower().Equals(".jpeg"))
                        {
                            if ((model.Image != null) || (student.ImagePath != model.ExistingImagePath))
                            {
                                if (model.ExistingImagePath != null)
                                {
                                    string filePath = Path.Combine(_environment.WebRootPath,
                                        "Images", model.ExistingImagePath);
                                    System.IO.File.Delete(filePath);
                                }
                            }

                            string filepath = Path.Combine(uploadfolder, uniqueImageName);
                            using (var fileStream = new FileStream(filepath, FileMode.Create))
                            {
                                model.Image.CopyTo(fileStream);
                            }

                            student.ImagePath = uniqueImageName;

                        }
                        else
                        {
                            ViewBag.Image = "Please Upload .png ,.jpg,.jpeg formats only!!";
                            student.ImagePath = model.ExistingImagePath;
                        }
                    }

                    string uniqueFileName = null;

                    if (model.File != null)
                    {
                        string uploadfolder = Path.Combine(_environment.WebRootPath, "Files");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;

                        var extension = Path.GetExtension(uniqueFileName);

                        if (extension.ToLower().Equals(".pdf"))
                        {
                            if (student.FilePath != model.ExistingFilePath || model.File != null)
                            {
                                if (model.ExistingFilePath != null)
                                {
                                    string filePath = Path.Combine(_environment.WebRootPath,
                                        "Files", model.ExistingFilePath);
                                    System.IO.File.Delete(filePath);
                                }
                            }
                            string filepath = Path.Combine(uploadfolder, uniqueFileName);
                            using (var fileStream = new FileStream(filepath, FileMode.Create))
                            {
                                model.File.CopyTo(fileStream);
                            }

                            student.FilePath = uniqueFileName;

                        }
                        else
                        {
                            ViewBag.File = "Please Upload .pdf format only!!";
                            student.FilePath = model.ExistingFilePath;

                            return View(model);
                        }
                    }
                    int result = await _service.Edit(id,student);

                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;

                }
                return RedirectToAction(nameof(Index));
            }
            return View();
        }


        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var student = await _service.Delete(id, new List<string>(),a => a.AdmissionNo == id);
                if (student == null)
                {
                    return NotFound();
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

        [HttpPost]
        public async Task<ActionResult> DeleteMultiple(IFormCollection formCollection)
        {


            string[] ids = formCollection["selectedStudents"];

            foreach (string id in ids)
            {
                var student = await _service.DeleteConfirmed(int.Parse(id));


            }
            return RedirectToAction("Index");
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

            return File(pdfBytes, "application/pdf", $"{model.StudentName}"+"Details.pdf");

        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var student = await _service.Edit(id);

            var path = Path.Combine(_environment.WebRootPath, "Images",student.FilePath);
            var memory = new MemoryStream();

            if (System.IO.File.Exists(path))
            {
                var net = new System.Net.WebClient();
                var data = net.DownloadData(path);
                var content = new System.IO.MemoryStream(data);
                memory = content;

            }
            memory.Position = 0;
            String str = student.FilePath;
            String[] strlist = str.Split("_");


            return File(memory.ToArray(), "application/pdf", $"{strlist[1]}"+".pdf");
        }

        public async Task<IActionResult> DownloadImageFile(int id)
        {
            var student = await _service.Edit(id);

            var path = Path.Combine(_environment.WebRootPath, "Images", student.ImagePath);
            var memory = new MemoryStream();

            if (System.IO.File.Exists(path))
            {
                var net = new System.Net.WebClient();
                var data = net.DownloadData(path);
                var content = new System.IO.MemoryStream(data);
                memory = content;

            }
            memory.Position = 0;
            String str = student.ImagePath;
            String[] strlist = str.Split("_");


            return File(memory.ToArray(), "image/png", $"{strlist[1]}" + ".png");
        }


    }
}
