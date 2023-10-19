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

namespace StudentRegistration.Controllers
{
    public class StudentsController : Controller
    {
        //private readonly StudentDbContext _context;
        private readonly IStudentService _service;
        private readonly IMapper _map;

        public StudentsController(IStudentService service, IMapper map)
        {
            _service = service;
            _map = map;
        }

        // GET: Students
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                var Students = await _service.GetAll();
                if (!String.IsNullOrEmpty(searchString))
                {
                    Students = Students.Where(s=>s.AdmissionNo == int.Parse(searchString));
                }


                var StudentsDTO = _map.Map<IEnumerable<StudentDTO>>(Students);
                return View(StudentsDTO);
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
        public async Task<IActionResult> Create([Bind("AdmissionNo,StudentName,Address,DateOfBirth,SelectedCourse,SecuredGrade")] StudentDTO student)
        {
            try
            {
                var newstudent = _map.Map<Student>(student);
                int result = await _service.Create(newstudent);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
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
                if (StudentDTO == null)
                {
                    return NotFound();
                }
                return View(StudentDTO);

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
        public async Task<IActionResult> Edit(int id, [Bind("AdmissionNo,StudentName,Address,DateOfBirth,SelectedCourse,SecuredGrade")] StudentDTO student)
        {
            if (id != student.AdmissionNo)
            {
                return NotFound();
            }
            if (ModelState.IsValid != false)
            {
                try
                {

                    var Stu = _map.Map<Student>(student);
                    int result = await _service.Edit(id,Stu);
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

        //private bool StudentExists(int id)
        //{
        //    return (_context.Students?.Any(e => e.AdmissionNo == id)).GetValueOrDefault();
        //}
    }
}
