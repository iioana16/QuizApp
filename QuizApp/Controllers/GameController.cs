using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace QuizApp.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly QuizAppDbContext _context;

        public GameController(QuizAppDbContext context)
        {
            _context = context;
        }

        // setare dificultate
        [HttpPost]
        public IActionResult SetDifficulty(string difficulty)
        {
            // mapare in int
            int difficultyInt = difficulty switch
            {
                "1" => 1,
                "2" => 2,
                "3" => 3,
                _ => 1
            };

            // salvare in sesiune
            HttpContext.Session.SetInt32("Difficulty", difficultyInt);

            return RedirectToAction("StartGame", "Game");
        }

        public IActionResult Start()
        {
            return View();
        }

        //initializare quiz pe baza dificultate
        public IActionResult StartGame()
        {
            if (HttpContext.Session == null)
            {
                throw new InvalidOperationException("Session is not configured properly.");
            }

            // preia dificultatea din sesiune
            var difficultyId = HttpContext.Session.GetInt32("Difficulty");

            if (!difficultyId.HasValue)//daca nu setez dificultate
            {
                return RedirectToAction("Index", "Profile");
            }

            //cautam quiz asociat cu dificultatea
            var quiz = _context.Quizzes.FirstOrDefault(q => q.Id == difficultyId.Value);

            if (quiz == null)//daca nu avem quiz pt dificultate
            {
                return RedirectToAction("Index", "Profile");
            }

            // cutam intrebari pt quiz
            var questions = _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == quiz.Id)
                .OrderBy(q => Guid.NewGuid()) //intrebari random
                .Take(5)
                .ToList();

            if (!questions.Any())//daca nu s-au gasit intrebari
            {
                return RedirectToAction("Index", "Profile");
            }

            // salvam id-urile intrebarilor in sesiune
            HttpContext.Session.SetString("QuestionIds", string.Join(",", questions.Select(q => q.Id)));
            HttpContext.Session.SetInt32("CurrentIndex", 0);
            HttpContext.Session.SetInt32("Score", 0);
            HttpContext.Session.SetInt32("CorrectAnswers", 0); // resetam corectAnswers

            return RedirectToAction("Question");
        }


        //afisare intrebare curenta
        public IActionResult Question()
        {
            // ia id-urile intrebarilor din sesiune
            var questionIdsString = HttpContext.Session.GetString("QuestionIds");
            var index = HttpContext.Session.GetInt32("CurrentIndex") ?? 0;

            if (string.IsNullOrEmpty(questionIdsString))// daca nu sunt intrebari
            {
                return RedirectToAction("Index", "Profile");
            }

            // transformare id in lista intrebari
            var questionIds = questionIdsString.Split(',').Select(int.Parse).ToList();

            // verificare index valid
            if (index >= questionIds.Count)
            {
                return RedirectToAction("Finish");
            }

            // cauta intrebare in bd
            var question = _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == questionIds[index]);

            if (question == null)
            {
                return RedirectToAction("Finish");
            }

            ViewBag.Index = index + 1; // afisare intrebare curenta
            return View(question);
        }

        // cand utilizator raspunde se apeleaza
        [HttpPost]
        public IActionResult Answer(int answerId)
        {
            var questionIdsString = HttpContext.Session.GetString("QuestionIds");
            var index = HttpContext.Session.GetInt32("CurrentIndex") ?? 0;
            var correctAnswers = HttpContext.Session.GetInt32("CorrectAnswers") ?? 0;
            var difficultyId = HttpContext.Session.GetInt32("Difficulty") ?? 1; // default la easy

            if (string.IsNullOrEmpty(questionIdsString))
            {
                return RedirectToAction("Index", "Profile");
            }

            var questionIds = questionIdsString.Split(',').Select(int.Parse).ToList();

            if (index >= questionIds.Count)
            {
                return RedirectToAction("Finish");
            }

            var question = _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == questionIds[index]);

            if (question == null)
            {
                return RedirectToAction("Finish");
            }

            var answer = question.Answers.FirstOrDefault(a => a.Id == answerId);
            if (answer != null && answer.IsCorrect)
            {
                correctAnswers++;
                HttpContext.Session.SetInt32("CorrectAnswers", correctAnswers);

                // calculam scor
                var score = correctAnswers * difficultyId;
                HttpContext.Session.SetInt32("Score", score);
            }

            HttpContext.Session.SetInt32("CurrentIndex", index + 1);

            if (index + 1 >= questionIds.Count)
            {
                return RedirectToAction("Finish");
            }

            return RedirectToAction("Question");
        }

        // finalizare joc
        public IActionResult Finish()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.CurrentUserId = user.Id;

            var correctAnswers = HttpContext.Session.GetInt32("CorrectAnswers") ?? 0;
            var difficultyId = HttpContext.Session.GetInt32("Difficulty") ?? 1;  // default easy

            if (difficultyId == 0)//daca diff valida
            {
                return RedirectToAction("Index", "Profile");
            }

            var quiz = _context.Quizzes.FirstOrDefault(q => q.Id == difficultyId);
            if (quiz == null)
            {
                return RedirectToAction("Index", "Profile");
            }

            // Final score calculation
            var finalScore = correctAnswers * difficultyId;

            // adaugare in bd
            var result = new Result
            {
                UserId = user.Id,
                QuizId = quiz.Id,
                Score = finalScore,
                DateCompleted = DateTime.Now
            };

            _context.Results.Add(result);
            user.TotalScore += finalScore;
            _context.SaveChanges();

            // pt afisare sa se ia username si scpr
            var allUsersScores = _context.Users
                .OrderByDescending(u => u.TotalScore)
                .Select(u => new { u.Id, u.Username, u.TotalScore })
                .ToList();

            // afisare descrescator dupa score
            var userRank = allUsersScores
                .Select((userScore, index) => new { userScore.Id, userScore.Username, userScore.TotalScore, Rank = index + 1 })
                .FirstOrDefault(u => u.Id == user.Id);

            // pt afisare
            ViewBag.UserRank = userRank;
            ViewBag.AllUsersScores = allUsersScores;
            ViewBag.CorrectAnswers = correctAnswers;

            var model = new Tuple<int, List<QuizApp.Models.User>>(finalScore, allUsersScores.Select(u => new QuizApp.Models.User { Id = u.Id, Username = u.Username, TotalScore = u.TotalScore }).ToList());
            return View(model);
        }


    }
}
