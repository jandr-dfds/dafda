namespace Academia.Domain
{
    public class StudentEnrolled
    {
        public const string MessageType = "student-enrolled";

        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}