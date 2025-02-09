namespace QuizApp.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }

        public Quiz Quiz { get; set; } // Relație cu Quiz

        public List<Answer> Answers { get; set; } = new List<Answer>();
    }

}
