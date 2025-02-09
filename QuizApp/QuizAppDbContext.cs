using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data;

public class QuizAppDbContext : DbContext
{
    // constructor care primeste optiunile de configurare a DbContext
    public QuizAppDbContext(DbContextOptions<QuizAppDbContext> options)
        : base(options)
    {
    }

    // Definirea tabelelor ca seturi de date
    public DbSet<User> Users { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // configurarea cheilor primare
        builder.Entity<User>().ToTable("Users").HasKey(u => u.Id);
        builder.Entity<Quiz>().ToTable("Quizzes").HasKey(q => q.Id);
        builder.Entity<Question>().ToTable("Questions").HasKey(qu => qu.Id);
        builder.Entity<Answer>().ToTable("Answers").HasKey(a => a.Id);
        builder.Entity<Result>().ToTable("Results").HasKey(r => r.Id);

        base.OnModelCreating(builder);    
    }
}
