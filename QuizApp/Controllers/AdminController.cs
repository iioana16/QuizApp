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
            ViewBag.Quizzes = _context.Quizzes.ToList();
            ViewBag.Questions = _context.Questions.ToList();
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

            // adauga intrebarea
            var newQuestion = new Question
            {
                QuizId = quizId,
                QuestionText = questionText
            };

            _context.Questions.Add(newQuestion);
            _context.SaveChanges();

            // adauga raspunsurile
            var answers = new List<Answer>
            {
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer1, IsCorrect = (correctAnswer == 1) },
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer2, IsCorrect = (correctAnswer == 2) },
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer3, IsCorrect = (correctAnswer == 3) },
                new Answer { QuestionId = newQuestion.Id, AnswerText = answer4, IsCorrect = (correctAnswer == 4) }
            };

            _context.Answers.AddRange(answers);
            _context.SaveChanges();

            TempData["MessageAdd"] = "Question added successfully!";
            ViewBag.Quizzes = _context.Quizzes.ToList();
            return RedirectToAction("Dashboard");
            //ViewBag.Message = "Question added successfully!";
            //return View("Dashboard");
        }

        [HttpPost]
        public IActionResult DeleteQuestion(int questionId)
        {
            var question = _context.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question != null)
            {
                var answers = _context.Answers.Where(a => a.QuestionId == questionId);
                _context.Answers.RemoveRange(answers);
                _context.Questions.Remove(question);
                _context.SaveChanges();

                TempData["MessageDelete"] = "Question deleted successfully!";
            }
            return RedirectToAction("Dashboard");
        }
    }
}
