# SharpMind Online Courses Platform

ASP.NET Core MVC (.NET 10) web app with Identity roles (Admin, Mentor, Student), EF Core + SQLite, seeded IT courses, enrollment workflow, tests, practical tasks, and progress tracking.

## Seeded Accounts

- Admin: `admin@courses.com` / `Admin123!`
- Mentor 1: `mentor1@courses.com` / `Mentor123!`
- Mentor 2: `mentor2@courses.com` / `Mentor123!`
- Mentor 3: `mentor3@courses.com` / `Mentor123!`
- Mentor 4: `mentor4@courses.com` / `Mentor123!`

## Features

- Course catalog with filtering (name, topic, level) and sorting (name, price, popularity)
- Student enrollment requests and mentor approval flow
- Mentor course management: modules, materials, tests, questions, practical tasks
- Test passing with auto scoring and last-result overwrite
- Practical submission with mentor grading and comments
- Student and course progress bars
- Admin panel for mentor creation, role updates, and deleting users/courses
- Mint & white responsive UI theme (Bootstrap 5 + custom CSS)

## Run

1. Restore packages
2. Create initial migration
3. Apply migration / run app

```powershell
cd C:\Users\Lenovo\Desktop\SharpMind\SharpMind
dotnet restore
dotnet ef migrations add InitialCreate
dotnet run
```

Database is auto-migrated and seeded at startup via `DbSeeder.SeedAsync`.

