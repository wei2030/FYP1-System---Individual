using FYP1_System___Individual.Data;
using FYP1_System___Individual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Controllers
{
    public class CommitteesController : Controller
    {
        private readonly FYP1_System_Context _context;

        public CommitteesController(FYP1_System_Context context)
        {
            _context = context;
        }

        private bool IsAuthorized(string role)
        {
            var roleString = HttpContext.Session.GetString("Roles");
            var roles = roleString?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

            return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> AssignDomain()
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturers");

            var userIdString = HttpContext.Session.GetInt32("UserId");
            if (userIdString == null) return RedirectToAction("Index", "Lecturers");
            int actualUserId = userIdString.Value;

            var committeeLecturer = await _context.Lecturers.FindAsync(actualUserId);
            if (committeeLecturer == null || committeeLecturer.ProgramId == null)
                return RedirectToAction("Index", "Lecturers");

            var lecturers = await _context.Lecturers.Include(l => l.Program).Where(l => l.ProgramId == committeeLecturer.ProgramId).ToListAsync();
            return View(lecturers);
        }
        [HttpPost]
        public async Task<IActionResult> AssignDomain(int lecturerId, DomainType domain)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturers");

            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (lecturer != null)
            {
                lecturer.Domain = domain;
                _context.Lecturers.Update(lecturer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AssignDomain");
        }

        public async Task<IActionResult> ManageProposals(string? semester = null, string? session = null)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Lecturers");

            IQueryable<Proposal> query = _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor);

            if (!string.IsNullOrEmpty(semester))
            {
                query = query.Where(p => p.Semester == semester);
            }

            if (!string.IsNullOrEmpty(session))
            {
                query = query.Where(p => p.Session == session);
            }

            var proposals = await query.ToListAsync();

            ViewBag.Semester = semester;
            ViewBag.Session = session;

            return View(proposals);
        }

        public async Task<IActionResult> ProposalDetails(int id)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.Evaluator1)
                .Include(p => p.Evaluator2)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();
            return View(proposal);
        }

        public async Task<IActionResult> PendingSupervisorApprovals()
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Home");

            var committeeUserId = HttpContext.Session.GetInt32("UserId");
            var committee = await _context.Lecturers.FindAsync(committeeUserId);

            if (committee == null || committee.ProgramId == null) return RedirectToAction("Index", "Home");

            var proposals = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.TentativeSupervisor)
                .Where(p => p.SupervisorStatus == SupervisorStatus.SupervisorSelectionPendingApproval && p.Student.ProgramId == committee.ProgramId)
                .ToListAsync();

            return View(proposals);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSupervisor(int proposalId)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null) return NotFound();

            if (proposal.SupervisorStatus != SupervisorStatus.SupervisorSelectionPendingApproval) return BadRequest("Supervisor selection is not pending approval");

            proposal.SupervisorId = proposal.TentativeSupervisorId;
            proposal.SupervisorStatus = SupervisorStatus.SupervisorApproved;
            proposal.TentativeSupervisorId = null;
            _context.Proposals.Update(proposal);

            var lecturer = await _context.Lecturers.FindAsync(proposal.SupervisorId);
            if (lecturer != null && !lecturer.Role.Split(',').Contains("Supervisor", StringComparer.OrdinalIgnoreCase))
            {
                lecturer.AddRole("Supervisor");
                _context.Lecturers.Update(lecturer);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("PendingSupervisorApprovals");
        }

        [HttpPost]
        public async Task<IActionResult> RejectSupervisor(int proposalId)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null) return NotFound();

            if (proposal.SupervisorStatus != SupervisorStatus.SupervisorSelectionPendingApproval) return BadRequest("Supervisor selection is not pending approval");

            proposal.TentativeSupervisorId = null;
            proposal.SupervisorStatus = SupervisorStatus.PendingSupervisorSelection;
            _context.Proposals.Update(proposal);
            await _context.SaveChangesAsync();

            return RedirectToAction("PendingSupervisorApprovals");
        }

        [HttpGet]
        public async Task<IActionResult> AssignEvaluators(int proposalId)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == proposalId);
            if (proposal == null) return NotFound();

            var eligibleLecturers = _context.Lecturers
                .Where(l => l.Domain == proposal.ProjectType && l.Id != proposal.SupervisorId)
                .ToList();

            ViewBag.Lecturers = new SelectList(eligibleLecturers, "Id", "Name");
            return View(proposal);
        }
        [HttpPost]
        public async Task<IActionResult> AssignEvaluators(int id, int evaluator1Id, int evaluator2Id)
        {
            if (!IsAuthorized("Committee")) return RedirectToAction("Index", "Home");

            var proposal = await _context.Proposals
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();

            if (evaluator1Id == evaluator2Id)
            {
                ModelState.AddModelError("", "Evaluator 1 and Evaluator 2 cannot be the same.");
            }

            if (evaluator1Id == proposal.SupervisorId || evaluator2Id == proposal.SupervisorId)
            {
                ModelState.AddModelError("", "Supervisor cannot be assigned as evaluator.");
            }

            if (!ModelState.IsValid)
            {
                var eligibleLecturers = _context.Lecturers
                    .Where(l => l.Domain == proposal.ProjectType && l.Id != proposal.SupervisorId)
                    .ToList();

                ViewBag.Lecturers = new SelectList(eligibleLecturers, "Id", "Name");
                return View(proposal);
            }

            proposal.Evaluator1Id = evaluator1Id;
            proposal.Evaluator2Id = evaluator2Id;

            var evaluator1 = await _context.Lecturers.FindAsync(evaluator1Id);
            var evaluator2 = await _context.Lecturers.FindAsync(evaluator2Id);

            if (evaluator1 != null && !evaluator1.Role.Split(',').Contains("Evaluator", StringComparer.OrdinalIgnoreCase))
            {
                evaluator1.AddRole("Evaluator");
                _context.Lecturers.Update(evaluator1);
            }

            if (evaluator2 != null && !evaluator2.Role.Split(',').Contains("Evaluator", StringComparer.OrdinalIgnoreCase))
            {
                evaluator2.AddRole("Evaluator");
                _context.Lecturers.Update(evaluator2);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageProposals");
        }
    }
}
