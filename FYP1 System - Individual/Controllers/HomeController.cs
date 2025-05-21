using System.Diagnostics;
using FYP1_System___Individual.Data;
using FYP1_System___Individual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Controllers
{
    public class HomeController : Controller
    {
        private readonly FYP1_System_Context _context;

        public HomeController(FYP1_System_Context context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u  => u.Email == email && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Roles", user.Role);

                var roles = user.Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Admin");
                else if (roles.Contains("Lecturer", StringComparer.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Lecturers");
                else if (roles.Contains("Student", StringComparer.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Students");
                else
                    return RedirectToAction("Privacy", "Home");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
