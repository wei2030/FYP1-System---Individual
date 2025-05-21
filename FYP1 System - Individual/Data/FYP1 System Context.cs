using FYP1_System___Individual.Models;
using Microsoft.EntityFrameworkCore;

namespace FYP1_System___Individual.Data
{
    public class FYP1_System_Context : DbContext
    {
        public FYP1_System_Context(DbContextOptions<FYP1_System_Context> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<AcademicProgram> AcademicPrograms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Lecturer>().ToTable("Lecturers");

            // Supervisor relationship
            modelBuilder.Entity<Proposal>()
                .HasOne(p => p.Supervisor)
                .WithMany(l => l.SupervisedProposals)
                .HasForeignKey(p => p.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evaluator1 relationship
            modelBuilder.Entity<Proposal>()
                .HasOne(p => p.Evaluator1)
                .WithMany(l => l.EvaluatedAsFirst)
                .HasForeignKey(p => p.Evaluator1Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Evaluator2 relationship
            modelBuilder.Entity<Proposal>()
                .HasOne(p => p.Evaluator2)
                .WithMany(l => l.EvaluatedAsSecond)
                .HasForeignKey(p => p.Evaluator2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
