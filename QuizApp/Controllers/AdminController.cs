using Microsoft.AspNetCore.Mvc;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly QuizAppDbContext _context;

        public AdminController(QuizAppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            //trimite lista de dificultati catre view
            ViewBag.Quizzes = _context.Quizzes.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddQuestion(int quizId, string questionText, string answer1, string answer2, string answer3, string answer4, int correctAnswer)
        {
            if (quizId == 0 || string.IsNullOrEmpty(questionText) || string.IsNullOrEmpty(answer1) ||
                string.IsNullOrEmpty(answer2) || string.IsNullOrEmpty(answer3) || string.IsNullOrEmpty(answer4))
            {
                ViewBag.Message = "All fields are required!";
                ViewBag.Quizzes = _context.Quizzes.ToList();
                return View("Dashboard");
            }

            // adauga întrebarea
            var newQuestion = new Question
            {
                QuizId = quizId,
                QuestionText = questionText
            };

            _context.Questions.Add(newQuestion);
            _context.SaveChanges();

            // adauag raspunsurile
            var answers = new List<Answer>
            {
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer1, IsCorrect = (correctAnswer == 1) },
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer2, IsCorrect = (correctAnswer == 2) },
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer3, IsCorrect = (correctAnswer == 3) },
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer4, IsCorrect = (correctAnswer == 4) }
            };

            _context.Answers.AddRange(answers);
            _context.SaveChanges();

            ViewBag.Message = "Question added successfully!";
            ViewBag.Quizzes = _context.Quizzes.ToList();
            return View("Dashboard");
        }
    }
}
