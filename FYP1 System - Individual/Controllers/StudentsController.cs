using System.Globalization;
using FYP1_System___Individual.Data;
using FYP1_System___Individual.Models;
using FYP1_System___Individual.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Controllers
{
    public class StudentsController : Controller
    {
        private readonly FYP1_System_Context _context;

        public StudentsController(FYP1_System_Context context)
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
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Home");

            var userId = HttpContext.Session.GetInt32("UserId");

            var student = await _context.Students
                .Include(s => s.Program)
                .Include(s => s.Proposals)
                    .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(s => s.Id == userId);

            if(student == null) return NotFound();

            var proposal = student.Proposals?.FirstOrDefault();
            ViewBag.Proposal = proposal;

            return View(student);

        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("Name,Email,Password,ProgramId,Role")] Student student)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Students.FirstOrDefaultAsync(s => s.Email == student.Email);
                if (existing != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name");
                    return View(student);
                }

                student.Role = "Student";
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name");
            return View(student);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email, [FromServices] IEmailSender emailSender)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Please enter your email.");
                return View();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
            if (student == null)
            {
                ModelState.AddModelError("Email", "Email not found.");
                return View();
            }

            student.PasswordResetToken = Guid.NewGuid().ToString();
            student.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            _context.Update(student);
            await _context.SaveChangesAsync();

            var resetUrl = Url.Action("ResetPassword", "Students", new { token = student.PasswordResetToken }, Request.Scheme);

            var emailBody = $"<p>Dear {student.Name},</p>" +
                $"<p>To reset your password, please click the link below:</p>" +
                $"<p><a href='{resetUrl}'>Reset Password</a></p>" +
                $"<p>This link will expire in 1 hour.</p>";

            await emailSender.SendEmailAsync(student.Email, "Password Reset Request", emailBody);

            ViewBag.Message = "If the email exists, a reset link has been sent.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var student = await _context.Students.FirstOrDefaultAsync( s =>
                s.PasswordResetToken == model.Token &&
                s.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (student == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired token");
                return View(model);
            }

            student.Password = model.NewPassword;
            student.PasswordResetToken = null;
            student.PasswordResetTokenExpiry = null;

            _context.Update(student);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your password has been reset successfully";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ManageStudents()
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            var userIdString = HttpContext.Session.GetInt32("UserId");
            if (userIdString == null) return RedirectToAction("Index", "Lecturers");
            int actualUserId = userIdString.Value;

            var committeeLecturer = await _context.Lecturers.FindAsync(actualUserId);
            if (committeeLecturer == null || committeeLecturer.ProgramId == null)
                return RedirectToAction("Index", "Lecturers");

            var students = await _context.Students
                .Where(s => s.ProgramId == committeeLecturer.ProgramId)
                .Include(s => s.Program)
                .ToListAsync();
            return View("ManageStudents", students);
        }

        public IActionResult Create()
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, Email, Password, ProgramId, Role")] Student student)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            if (ModelState.IsValid)
            {
                student.Role = "Student";
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name", student.ProgramId);
            return View(student);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name", student.ProgramId);
            return View(student);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Name, Email, Password, ProgramId, Role")] Student student)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                student.Role = "Student";
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Programs = new SelectList(_context.AcademicPrograms, "Id", "Name", student.ProgramId);
            return View(student);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            var student = await _context.Students.Include(s => s.Program).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturer");

            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Agreement(int proposalId)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Student");

            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null) return NotFound();
            return View(proposal);
        }
    }
}
