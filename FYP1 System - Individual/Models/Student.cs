namespace FYP1_System___Individual.Models
{
    public class Student : User
    {
        // Foreign key
        public int? ProgramId { get; set; }
        public AcademicProgram? Program { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Proposal created
        public ICollection<Proposal>? Proposals { get; set; }
    }
}
