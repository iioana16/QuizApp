using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using System.Security.Claims;

namespace QuizApp.Controllers
{
    [Authorize] // necesita autentificare
    public class ProfileController : Controller
    {
        private readonly QuizAppDbContext _context;

        //injecteaza contextul bazei de date
        public ProfileController(QuizAppDbContext context)
        {
            _context = context;
        }

        [Authorize] // asigurate ca utilizatorul este autentificat
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // gaseste utilizatorul din baza de date
            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user != null)
            {
                // preia scorurile anterioare ale utilizatorului
                var previousResults = _context.Results
                    .Where(r => r.UserId == user.Id)
                    .Include(r => r.Quiz)  //pt a scoate si dificultatea
                    .OrderByDescending(r => r.DateCompleted)
                    .Take(5) // preia ultimele 5 scoruri
                    .ToList();

                // trimite utilizatorul si scorurile anterioare la view
                ViewBag.Username = user.Username;
                ViewBag.PreviousResults = previousResults;
                return View(user);
            }

            return RedirectToAction("Login", "Account");
        }
    }
}
