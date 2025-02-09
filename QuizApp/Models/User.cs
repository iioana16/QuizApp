using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username-ul este obligatoriu.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Formatul email-ului nu este valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [MinLength(8, ErrorMessage = "Parola trebuie sa aiba cel putin 8 caractere.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Parola trebuie sa contina cel putin o litera mare, o litera mica, o cifra si un caracter special.")]
        public string Password { get; set; }

        [NotMapped] //sa nu creeze coloana in bd
        [Required(ErrorMessage = "Repetarea parolei este obligatorie.")]
        [Compare("Password", ErrorMessage = "Parolele nu se potrivesc.")]
        public string RepeatPassword { get; set; }

        public string Role { get; set; } = "user";

        public int TotalScore { get; set; }
    }
}

