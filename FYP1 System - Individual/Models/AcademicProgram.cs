namespace FYP1_System___Individual.Models
{
    public class AcademicProgram
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        
        // Lecturer list in a program
        public ICollection<Lecturer> Lecturers { get; set;} = new List<Lecturer>();

        // Student list in a program
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
