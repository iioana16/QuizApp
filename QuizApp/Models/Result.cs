namespace QuizApp.Models
{
    public class Result
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public DateTime DateCompleted { get; set; }

        public User User { get; set; } // Relație cu User
        public Quiz Quiz { get; set; } // Relație cu Quiz
    }

}
