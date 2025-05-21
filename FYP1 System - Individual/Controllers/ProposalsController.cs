﻿using FYP1_System___Individual.Data;
using FYP1_System___Individual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Controllers
{
    public class ProposalsController : Controller
    {
        private readonly FYP1_System_Context _context;

        public ProposalsController(FYP1_System_Context context)
        {
            _context = context;
        }

        private bool IsAuthorized(string role)
        {
            var roles = HttpContext.Session.GetString("Roles");
            return roles != null && roles.Split(',').Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> Index()
        {
            var proposals = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.Evaluator1)
                .Include(p => p.Evaluator2)
                .ToListAsync();
            return View(proposals);
        }

        public IActionResult Create1()
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            return View();
        }
        [HttpGet]
        public IActionResult Create2(DomainType projectType)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Students");

            var proposal = new Proposal { ProjectType = projectType, StudentId = userId.Value, SupervisorStatus = SupervisorStatus.PendingSupervisorSelection };
            return View(proposal);
        }
        [HttpPost]
        public async Task<IActionResult> Create2([Bind("Id, Title, ProjectType, FilePath, Semester, Session, StudentId")] Proposal proposal)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Students");

            proposal.StudentId = userId.Value;
            proposal.SupervisorStatus = SupervisorStatus.PendingSupervisorSelection;

            if (!ModelState.IsValid)
            {
                return View(proposal);
            }

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Students");
        }

        [HttpGet]
        public async Task<IActionResult> SelectSupervisor(int proposalId)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null || proposal.StudentId != HttpContext.Session.GetInt32("UserId")) return NotFound();

            if (proposal.SupervisorStatus == SupervisorStatus.SupervisorApproved) return BadRequest("Supervisor already approved, changes are not allowed");

            if (proposal.SupervisorStatus != SupervisorStatus.PendingSupervisorSelection && proposal.SupervisorStatus != SupervisorStatus.SupervisorSelectionPendingApproval)
                return BadRequest("Invalid supervisor status for selection");

            var supervisors = await _context.Lecturers.Where(l => l.Domain == proposal.ProjectType).ToListAsync();

            ViewBag.Supervisors = new SelectList(supervisors, "Id", "Name");
            return View(proposal);
        }
        [HttpPost]
        public async Task<IActionResult> SelectSupervisor(int proposalId, int supervisorId)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null || proposal.StudentId != HttpContext.Session.GetInt32("UserId")) return NotFound();

            var supervisor = await _context.Lecturers.FindAsync(supervisorId);
            if (supervisor == null || supervisor.Domain != proposal.ProjectType)
            {
                ModelState.AddModelError("", "Invalid supervisor selected");
                var supervisors = await _context.Lecturers.Where(l => l.Domain == proposal.ProjectType).ToListAsync();
                ViewBag.Supervisors = new SelectList(supervisors, "Id", "Name");
                return View(proposal);
            }

            if (proposal.TentativeSupervisorId != supervisorId || proposal.SupervisorStatus != SupervisorStatus.SupervisorSelectionPendingApproval)
            {
                proposal.TentativeSupervisorId = supervisorId;
                proposal.SupervisorStatus = SupervisorStatus.SupervisorSelectionPendingApproval;
                await _context.SaveChangesAsync();

                TempData["Message"] = "Supervisor selection submitted and pending committee approval";
            }
            else
            {
                TempData["Message"] = "No changes made, selected supervisor is same as current";
            }

            return RedirectToAction("Index", "Students");
        }

        public async Task<IActionResult> Edit1(int id)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var proposal = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == id);
            if (proposal == null) return NotFound();
            return View(proposal);
        }
        [HttpGet]
        public async Task<IActionResult> Edit2(int id, DomainType projectType)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var proposal = await _context.Proposals.FindAsync(id);
            if (proposal == null) return NotFound();

            proposal.ProjectType = projectType;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null|| proposal.StudentId != userId.Value) return Forbid();

            ViewBag.StudentId = userId.Value;
            ViewBag.Lecturers = new SelectList(_context.Lecturers.Where(l => l.Domain == proposal.ProjectType), "Id", "Name", proposal.SupervisorId);

            return View(proposal);
        }
        [HttpPost]
        public async Task<IActionResult> Edit2(int id, [Bind("Id, Title, ProjectType, FilePath, Semester, Session, StudentId, SupervisorId")] Proposal proposal)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            if (id != proposal.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.StudentId = proposal.StudentId;
                ViewBag.Lecturers = new SelectList(_context.Lecturers.Where(l => l.Domain == proposal.ProjectType), "Id", "Name", proposal.SupervisorId);

                return View(proposal);
            }

            var supervisor = await _context.Lecturers.FindAsync(proposal.SupervisorId);
            if (supervisor == null || supervisor.Domain != proposal.ProjectType)
            {
                ModelState.AddModelError("SupervisorId", "Selected supervisor does not match the selected project type");
                ViewBag.StudentId = proposal.StudentId;
                ViewBag.Lecturers = new SelectList(_context.Lecturers.Where(l => l.Domain == proposal.ProjectType), "Id", "Name", proposal.SupervisorId);

                return View(proposal);
            }

            _context.Update(proposal);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Students");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();
            if (proposal.StudentId != userId.Value) return Forbid();

            return View(proposal);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthorized("Student")) return RedirectToAction("Index", "Students");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals.FindAsync(id);
            if(proposal != null)
            {
                if (proposal.StudentId != userId.Value) return Forbid();

                _context.Proposals.Remove(proposal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Students");
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!(IsAuthorized("Student") || IsAuthorized("Supervisor"))) return RedirectToAction("Index", "Students");

            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.Evaluator1)
                .Include(p => p.Evaluator2)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (proposal == null) return NotFound();
            return View(proposal);
        }
    }
}
