using System.ComponentModel.DataAnnotations;

namespace FYP1_System___Individual.Models
{
    public enum SupervisorStatus
    {
        PendingSupervisorSelection,
        SupervisorSelectionPendingApproval,
        SupervisorApproved
    }

    public class Proposal
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public DomainType ProjectType { get; set; }
        public string? FilePath { get; set; }

        public ProposalStatus? EvaluationStatus { get; set; }
        [StringLength(1000)]
        public string? EvaluatorComment { get; set; }

        public string? SupervisorComment { get; set; }

        public SupervisorStatus SupervisorStatus { get; set; } = SupervisorStatus.PendingSupervisorSelection;

        [Required]
        public string Semester { get; set; } = String.Empty;
        public string? Session { get; set; }

        // Foreign keys
        // created student
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        // selected supervisor
        public int? SupervisorId { get; set; }
        public Lecturer? Supervisor { get; set; }

        // tentative supervisor selected by student, pending approval
        public int? TentativeSupervisorId { get; set; }
        public Lecturer? TentativeSupervisor { get; set; }

        // assigned evaluator 1
        public int? Evaluator1Id { get; set; }
        public Lecturer? Evaluator1 {  get; set; }

        // assigned evaluator 2
        public int? Evaluator2Id { get; set; }
        public Lecturer? Evaluator2 { get; set; }
    }
}
