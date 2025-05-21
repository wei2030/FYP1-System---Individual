using System.ComponentModel.DataAnnotations.Schema;

namespace FYP1_System___Individual.Models
{
    public class Lecturer : User
    {
        public DomainType? Domain { get; set; }

        // Foreign key
        public int? ProgramId { get; set; }
        public AcademicProgram? Program { get; set; }

        // Proposed tentative supervise
        [InverseProperty("TentativeSupervisor")]
        public ICollection<Proposal>? TentativeSupervisedProposals { get; set; }

        // Assigned supervise proposal
        [InverseProperty("Supervisor")]
        public ICollection<Proposal>? SupervisedProposals { get; set; }

        // Assigned evaluate proposal
        [InverseProperty("Evaluator1")]
        public ICollection<Proposal>? EvaluatedAsFirst { get; set; }

        [InverseProperty("Evaluator2")]
        public ICollection<Proposal>? EvaluatedAsSecond { get; set; }
    }
}
