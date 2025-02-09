using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Data;
using QuizApp.Models;
using System.Security.Claims;

namespace QuizApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuizAppDbContext _context;
        
        //injecteaza contextul bazei de date
        public AccountController(QuizAppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult HomePage()
        {
            return View();
        }

        public ActionResult Register()
        {
            ViewBag.Message = ViewBag.Message ?? "";
            return View(new User());
        }

        [HttpPost]
        public ActionResult Register(User model)
        {
            // validare pt email
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // verificare email este deja existent
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Acest email este deja folosit.");
                return View(model);
            }

            // valori implicite
            model.Role = "user";
            model.TotalScore = 0;

            // salvare utilizator in bd
            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            if (user != null && user.Password == password)
            {
                // autentifica utilizatorul
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                if (user.Role == "admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Profile");
                }
            }

            ViewBag.Message = "Email sau parola incorecta!";
            return View();
        }

        // functie logout
        public async Task<IActionResult> Logout()
        {
            // deconecteaza utilizator
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("HomePage", "Account");
        }
    }
}
