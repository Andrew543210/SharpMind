using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharpMind.Models;
using SharpMind.Models.Identity;

namespace SharpMind.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> Modules => Set<CourseModule>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<PracticalTask> PracticalTasks => Set<PracticalTask>();
    public DbSet<PracticalSubmission> PracticalSubmissions => Set<PracticalSubmission>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>()
            .HasOne(c => c.Mentor)
            .WithMany()
            .HasForeignKey(c => c.MentorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CourseModule>()
            .HasOne(m => m.Course)
            .WithMany(c => c.Modules)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseModule>()
            .HasIndex(m => new { m.CourseId, m.Order })
            .IsUnique();

        builder.Entity<Material>()
            .HasOne(m => m.Module)
            .WithMany(mo => mo.Materials)
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Test>()
            .HasOne(t => t.Module)
            .WithOne(m => m.Test)
            .HasForeignKey<Test>(t => t.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Test>()
            .HasOne(t => t.Course)
            .WithMany(c => c.Tests)
            .HasForeignKey(t => t.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Question>()
            .HasOne(q => q.Test)
            .WithMany(t => t.Questions)
            .HasForeignKey(q => q.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AnswerOption>()
            .HasOne(a => a.Question)
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PracticalTask>()
            .HasOne(p => p.Module)
            .WithOne(m => m.PracticalTask)
            .HasForeignKey<PracticalTask>(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PracticalSubmission>()
            .HasOne(s => s.Task)
            .WithMany(t => t.Submissions)
            .HasForeignKey(s => s.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PracticalSubmission>()
            .HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TestResult>()
            .HasOne(r => r.Test)
            .WithMany(t => t.Results)
            .HasForeignKey(r => r.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TestResult>()
            .HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TestResult>()
            .HasIndex(r => new { r.TestId, r.StudentId })
            .IsUnique();

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasIndex(e => new { e.CourseId, e.StudentId })
            .IsUnique();

        builder.Entity<Certificate>()
            .HasOne(c => c.Course)
            .WithMany(c => c.Certificates)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Certificate>()
            .HasOne(c => c.Student)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Certificate>()
            .HasIndex(c => new { c.CourseId, c.StudentId })
            .IsUnique();

        builder.Entity<Certificate>()
            .HasIndex(c => c.UniqueNumber)
            .IsUnique();
    }
}


