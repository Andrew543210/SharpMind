using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharpMind.Models;
using SharpMind.Models.Identity;

namespace SharpMind.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await dbContext.Database.MigrateAsync();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(userManager, "admin@courses.com", "admin@courses.com", "Admin", "User", "Admin123!", AppRoles.Admin);

        var mentors = new List<ApplicationUser>
        {
            await EnsureUserAsync(userManager, "mentor1@courses.com", "mentor1", "Mentor", "One", "Mentor123!", AppRoles.Mentor),
            await EnsureUserAsync(userManager, "mentor2@courses.com", "mentor2", "Mentor", "Two", "Mentor123!", AppRoles.Mentor),
            await EnsureUserAsync(userManager, "mentor3@courses.com", "mentor3", "Mentor", "Three", "Mentor123!", AppRoles.Mentor),
            await EnsureUserAsync(userManager, "mentor4@courses.com", "mentor4", "Mentor", "Four", "Mentor123!", AppRoles.Mentor)
        };

        _ = admin;

        if (await dbContext.Courses.AnyAsync())
        {
            return;
        }

        var seedCourses = new[]
        {
            new { Title = "C# Advanced", Description = "Advanced patterns, async, performance and architecture in C#.", Topic = CourseTopic.Programming, Level = CourseLevel.Prosunutyi, Mentor = mentors[0] },
            new { Title = "JavaScript Full-Stack", Description = "Modern JavaScript from backend services to interactive frontend.", Topic = CourseTopic.WebDevelopment, Level = CourseLevel.Serednii, Mentor = mentors[1] },
            new { Title = "Python for Data Science", Description = "Data wrangling, visualization and ML essentials with Python.", Topic = CourseTopic.DataScience, Level = CourseLevel.Serednii, Mentor = mentors[2] },
            new { Title = "DevOps z nulia", Description = "CI/CD, containers, cloud basics and monitoring from scratch.", Topic = CourseTopic.DevOps, Level = CourseLevel.Pochatkovyi, Mentor = mentors[3] }
        };

        foreach (var item in seedCourses)
        {
            var course = new Course
            {
                Title = item.Title,
                Description = item.Description,
                Topic = item.Topic,
                Level = item.Level,
                Price = 0,
                MentorId = item.Mentor.Id,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            for (var moduleIndex = 1; moduleIndex <= 5; moduleIndex++)
            {
                var module = new CourseModule
                {
                    Title = $"Module {moduleIndex}: {item.Title} block",
                    Description = $"Focused learning outcomes for module {moduleIndex} in {item.Title}.",
                    Order = moduleIndex
                };

                for (var materialIndex = 1; materialIndex <= 3; materialIndex++)
                {
                    module.Materials.Add(new Material
                    {
                        Title = $"Material {moduleIndex}.{materialIndex}",
                        Content = $"Theory and useful links for {item.Title} module {moduleIndex}, material {materialIndex}."
                    });
                }

                var test = new Test
                {
                    Title = $"Quiz for module {moduleIndex}"
                };

                for (var questionIndex = 1; questionIndex <= 5; questionIndex++)
                {
                    var question = new Question
                    {
                        Text = $"{item.Title}: module {moduleIndex} question {questionIndex}"
                    };

                    for (var optionIndex = 1; optionIndex <= 4; optionIndex++)
                    {
                        question.AnswerOptions.Add(new AnswerOption
                        {
                            Text = $"Option {optionIndex}",
                            IsCorrect = optionIndex == 1
                        });
                    }

                    test.Questions.Add(question);
                }

                module.Test = test;
                module.PracticalTask = new PracticalTask
                {
                    Title = $"Practical task {moduleIndex}",
                    Description = $"Complete a practical implementation task for module {moduleIndex}."
                };

                course.Modules.Add(module);
            }

            dbContext.Courses.Add(course);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string username,
        string firstName,
        string lastName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to seed user {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}

