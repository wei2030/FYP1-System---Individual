using FYP1_System___Individual.Data;
using FYP1_System___Individual.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FYP1_System___Individual.Controllers
{
    public class EvaluatorsController : Controller
    {
        private readonly FYP1_System_Context _context;

        public EvaluatorsController(FYP1_System_Context context)
        {
            _context = context;
        }

        private bool IsAuthorized(string role)
        {
            var roleString = HttpContext.Session.GetString("Roles");
            var roles = roleString?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

            return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> AssignedProposals(int evaluatorId)
        {
            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Where(p => p.Evaluator1Id == evaluatorId || p.Evaluator2Id == evaluatorId)
                .ToListAsync();

            ViewBag.EvaluatorId = evaluatorId;
            return View(proposal);
        }

        public async Task<IActionResult> Evaluate(int id)
        {
            var proposal = await _context.Proposals
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (proposal == null) return NotFound();
            return View(proposal);
        }
        [HttpPost]
        public async Task<IActionResult> Evaluate(int id, [Bind("Id, EvaluationStatus, EvaluatorComment")] Proposal updatedProposal)
        {
            var proposal = await _context.Proposals.FindAsync(id);
            if(proposal == null) return NotFound();

            proposal.EvaluationStatus = updatedProposal.EvaluationStatus;
            proposal.EvaluatorComment = updatedProposal.EvaluatorComment;

            await _context.SaveChangesAsync();
            // should be session evaluatorId /////////////////////////////////////////////////////////////////
            return RedirectToAction("AssignedProposals", new { evaluatorId = proposal.Evaluator1Id ?? proposal.Evaluator2Id });
        }

        public IActionResult Downloads(string path)
        {
            if(string.IsNullOrEmpty(path)) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "documents", path);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "application/pdf";
            return PhysicalFile(filePath, contentType, Path.GetFileName(filePath));
        }
    }
}
