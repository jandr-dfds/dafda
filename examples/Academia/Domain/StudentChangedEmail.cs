namespace Academia.Domain
{
    public class StudentChangedEmail
    {
        public const string MessageType = "student-changed-email";

        public string StudentId { get; set; }
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
    }
}