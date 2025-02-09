namespace QuizApp.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string DifficultyLevel { get; set; } // Easy=1, Medium=2, Hard=3
    }

}
