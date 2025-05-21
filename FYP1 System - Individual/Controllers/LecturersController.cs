using FYP1_System___Individual.Data;
using FYP1_System___Individual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Controllers
{
    public class LecturersController : Controller
    {
        private readonly FYP1_System_Context _context;

        public LecturersController(FYP1_System_Context context)
        {
            _context = context;
        }

        private bool IsAuthorized(string role)
        {
            var roleString = HttpContext.Session.GetString("Roles");
            var roles = roleString?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

            return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized("Lecturer")) return RedirectToAction("Index", "Home");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers
                .Include(l => l.Program)
                .Include(l => l.SupervisedProposals)
                .FirstOrDefaultAsync(l => l.Id == userId);

            if (lecturer == null) return NotFound();

            var proposalsPendingApproval = await _context.Proposals
                    .Include(p => p.Student)
                    .Include(p => p.Supervisor)
                    .Where(p => p.SupervisorStatus == SupervisorStatus.SupervisorSelectionPendingApproval)
                    .ToListAsync();
            
            ViewBag.ProposalsPendingApproval = proposalsPendingApproval;

            return View(lecturer);
        }

        public async Task<IActionResult> LecturerList()
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.Include(l => l.Program).ToListAsync();
            return View(lecturer);
        }

        public IActionResult Create()
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, Email, Password, ProgramId, Role")] Lecturer lecturer)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                lecturer.Role = "Lecturer";
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();
                return RedirectToAction("LecturerList");
            }
            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name", lecturer.ProgramId);
            return View(lecturer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(x => x.Id == id);
            if(lecturer == null) return NotFound();

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name", lecturer.ProgramId);
            return View(lecturer);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Name, Email, Password, ProgramId, Role")] Lecturer lecturer)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            if (id != lecturer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                lecturer.Role = "Lecturer";
                _context.Update(lecturer);
                await _context.SaveChangesAsync();
                return RedirectToAction("LecturerList");
            }

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name", lecturer.ProgramId);
            return View(lecturer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.Include(l => l.Program).FirstOrDefaultAsync(x => x.Id == id);
            if (lecturer == null) return NotFound();
            return View(lecturer);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthorized("Admin")) return RedirectToAction("Index", "Home");

            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer != null)
            {
                _context.Lecturers.Remove(lecturer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("LecturerList");
        }
    }
}
