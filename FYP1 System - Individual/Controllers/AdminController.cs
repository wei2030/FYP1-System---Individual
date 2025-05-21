using FYP1_System___Individual.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Controllers
{
    public class AdminController : Controller
    {
        private readonly FYP1_System_Context _context;

        public AdminController(FYP1_System_Context context)
        {
            _context = context;
        }

        private bool IsAuthorized(string role)
        {
            var roles = HttpContext.Session.GetString("Roles");
            return roles != null && roles.Split(',').Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        //public async Task<IActionResult> Index()
        //{
        //    if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

        //    ViewBag.ProgramCount = await _context.AcademicPrograms.CountAsync();
        //    ViewBag.LecturerCount = await _context.Lecturers.CountAsync();
        //    ViewBag.StudentCount = await _context.Students.CountAsync();
        //    ViewBag.CommitteesPerProgram = await _context.AcademicPrograms
        //        .Select(p => new
        //        {
        //            ProgramName = p.Name,
        //            CommitteeCount = p.Lecturers.Count(l => l.HasRole("Committee"))
        //        })
        //        .ToListAsync();

        //    return View();
        //}

        public async Task<IActionResult> Index()
        {
            var totalPrograms = await _context.AcademicPrograms.CountAsync();
            var totalLecturers = await _context.Lecturers.CountAsync();

            var lecturers = await _context.Lecturers.ToListAsync();

            var totalCommittees = lecturers
                .Where(l => l.Role != null && l.Role.Split(',').Contains("Committee"))
                .Count();

            ViewBag.TotalPrograms = totalPrograms;
            ViewBag.TotalLecturers = totalLecturers;
            ViewBag.TotalCommittees = totalCommittees;

            return View();
        }


        public async Task<IActionResult> ManageCommittees()
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.Include(l => l.Program).ToListAsync();
            return View(lecturer);
        }

        [HttpPost]
        public async Task<IActionResult> AddCommittee(int lecturerId)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.FindAsync(lecturerId);

            if (lecturer != null)
            {
                lecturer.AddRole("Committee");
                _context.Update(lecturer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageCommittees");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCommittee(int lecturerId)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.FindAsync(lecturerId);

            if (lecturer != null)
            {
                lecturer.RemoveRole("Committee");
                _context.Update(lecturer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageCommittees");
        }
    }
}
