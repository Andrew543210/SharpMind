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
            await EnsureUserAsync(userManager, "mentor1@gmail.com", "mentor1", "Mentor", "One", "Mentor123!", AppRoles.Mentor),
            await EnsureUserAsync(userManager, "mentor2@gmail.com", "mentor2", "Mentor", "Two", "Mentor123!", AppRoles.Mentor),
            await EnsureUserAsync(userManager, "mentor3@gmail.com", "mentor3", "Mentor", "Three", "Mentor123!", AppRoles.Mentor),
            await EnsureUserAsync(userManager, "mentor4@gmail.com", "mentor4", "Mentor", "Four", "Mentor123!", AppRoles.Mentor)
        };

        _ = admin;
        var devOpsMentor = await EnsureUserAsync(
            userManager,
            "devops_mentor@gmail.com",
            "devops_mentor",
            "DevOps",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var javaMentor = await EnsureUserAsync(
            userManager,
            "java_mentor@gmail.com",
            "java_mentor",
            "Java",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var frontendMentor = await EnsureUserAsync(
            userManager,
            "frontend_mentor@gmail.com",
            "frontend_mentor",
            "Frontend",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var cppMentor = await EnsureUserAsync(
            userManager,
            "cpp_mentor@gmail.com",
            "cpp_mentor",
            "Cpp",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var goMentor = await EnsureUserAsync(
            userManager,
            "go_mentor@gmail.com",
            "go_mentor",
            "Go",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var reactMentor = await EnsureUserAsync(
            userManager,
            "react_mentor@gmail.com",
            "react_mentor",
            "React",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var angularMentor = await EnsureUserAsync(
            userManager,
            "angular_mentor@gmail.com",
            "angular_mentor",
            "Angular",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var devOps = await EnsureCourseShellAsync(
            dbContext,
            devOpsMentor.Id,
            ["DevOps з нуля", "DevOps z nulia", "DevOps from zero to pro"],
            "DevOps from zero to pro",
            "CI/CD, containers, cloud basics and monitoring from scratch.",
            CourseTopic.DevOps,
            CourseLevel.Serednii);
        await RebuildSimpleCourseContentAsync(dbContext, devOps);

        var csharp = await EnsureCourseShellAsync(
            dbContext,
            mentors[0].Id,
            ["C# Advanced"],
            "C# Advanced",
            "Advanced patterns, async, performance and architecture in C#.",
            CourseTopic.Programming,
            CourseLevel.Prosunutyi);
        await RebuildCSharpAdvancedAsync(dbContext, csharp);

        var jsFullStack = await EnsureCourseShellAsync(
            dbContext,
            mentors[1].Id,
            ["JavaScript Full-Stack"],
            "JavaScript Full-Stack",
            "Modern JavaScript from backend services to interactive frontend.",
            CourseTopic.WebDevelopment,
            CourseLevel.Serednii);
        await RebuildJavaScriptFullStackAsync(dbContext, jsFullStack);

        var python = await EnsureCourseShellAsync(
            dbContext,
            mentors[2].Id,
            ["Python for Data Science"],
            "Python for Data Science",
            "Data wrangling, visualization and ML essentials with Python.",
            CourseTopic.DataScience,
            CourseLevel.Serednii);
        await RebuildPythonForDataScienceAsync(dbContext, python);

        var java = await EnsureCourseShellAsync(
            dbContext,
            javaMentor.Id,
            ["Java Fundamentals"],
            "Java Fundamentals",
            "Базовий курс Java: синтаксис, типи даних, ООП, масиви, умови та цикли.",
            CourseTopic.Programming,
            CourseLevel.Pochatkovyi);
        await RebuildJavaFundamentalsAsync(dbContext, java);

        var frontend = await EnsureCourseShellAsync(
            dbContext,
            frontendMentor.Id,
            ["Frontend Fundamentals"],
            "Frontend Fundamentals",
            "Базовий курс Frontend: HTML5, CSS3, Flexbox/Grid, JavaScript та DOM.",
            CourseTopic.WebDevelopment,
            CourseLevel.Pochatkovyi);
        await RebuildFrontendFundamentalsAsync(dbContext, frontend);

        var cpp = await EnsureCourseShellAsync(
            dbContext,
            cppMentor.Id,
            ["C++ Fundamentals"],
            "C++ Fundamentals",
            "Базовий курс з мови C++: синтаксис, типи даних, масиви, вказівники та функції.",
            CourseTopic.Programming,
            CourseLevel.Pochatkovyi);
        await RebuildCppFundamentalsAsync(dbContext, cpp);

        var go = await EnsureCourseShellAsync(
            dbContext,
            goMentor.Id,
            ["Go Fundamentals"],
            "Go Fundamentals",
            "Основи Go: змінні, керування потоком, масиви, структури та конкурентність.",
            CourseTopic.Programming,
            CourseLevel.Pochatkovyi);
        await RebuildGoFundamentalsAsync(dbContext, go);

        var react = await EnsureCourseShellAsync(
            dbContext,
            reactMentor.Id,
            ["React Fundamentals"],
            "React Fundamentals",
            "Початковий курс React: JSX, стани, ефекти, маршрутизація та контекст.",
            CourseTopic.WebDevelopment,
            CourseLevel.Pochatkovyi);
        await RebuildReactFundamentalsAsync(dbContext, react);

        var angular = await EnsureCourseShellAsync(
            dbContext,
            angularMentor.Id,
            ["Angular Fundamentals"],
            "Angular Fundamentals",
            "Основи Angular: компоненти, директиви, сервіси, маршрутизація, форми.",
            CourseTopic.WebDevelopment,
            CourseLevel.Pochatkovyi);
        await RebuildAngularFundamentalsAsync(dbContext, angular);

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Course> EnsureCourseShellAsync(
        ApplicationDbContext dbContext,
        string mentorId,
        IEnumerable<string> aliases,
        string title,
        string description,
        CourseTopic topic,
        CourseLevel level)
    {
        var aliasSet = aliases.ToList();
        var course = await dbContext.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Materials)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Test!)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
            .Include(c => c.Modules)
                .ThenInclude(m => m.PracticalTask)
            .Include(c => c.Tests)
                .ThenInclude(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(c => aliasSet.Contains(c.Title));

        if (course is null)
        {
            course = new Course
            {
                Title = title,
                Description = description,
                Topic = topic,
                Level = level,
                Price = GetPriceForLevel(level),
                MentorId = mentorId,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Courses.Add(course);
        }
        else
        {
            course.Title = title;
            course.Description = description;
            course.Topic = topic;
            course.Level = level;
            course.Price = GetPriceForLevel(level);
            course.MentorId = mentorId;
            course.IsPublished = true;
        }

        return course;
    }

    private static decimal GetPriceForLevel(CourseLevel level) => level switch
    {
        CourseLevel.Pochatkovyi => 2000m,
        CourseLevel.Serednii => 6000m,
        CourseLevel.Prosunutyi => 10000m,
        _ => 2000m
    };

    private static async Task RebuildSimpleCourseContentAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        for (var moduleIndex = 1; moduleIndex <= 5; moduleIndex++)
        {
            var module = new CourseModule
            {
                Title = $"Module {moduleIndex}: DevOps from zero to pro block",
                Description = $"Focused learning outcomes for module {moduleIndex} in DevOps from zero to pro.",
                Order = moduleIndex,
                Materials =
                [
                    new Material { Title = $"Material {moduleIndex}.1", Content = "Theory and useful links." },
                    new Material { Title = $"Material {moduleIndex}.2", Content = "Hands-on examples and checks." },
                    new Material { Title = $"Material {moduleIndex}.3", Content = "Extra references and recap." }
                ],
                PracticalTask = new PracticalTask
                {
                    Title = $"Practical task {moduleIndex}",
                    Description = $"Complete a practical implementation task for module {moduleIndex}."
                },
                Test = BuildTest($"Quiz for module {moduleIndex}", BuildPlaceholderQuestions($"DevOps module {moduleIndex}"))
            };
            course.Modules.Add(module);
        }

        course.Tests.Add(BuildFinalTest("Final test for DevOps from zero to pro", BuildFinalPlaceholderQuestions()));
    }

    private static async Task RebuildJavaFundamentalsAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(
            1,
            "Вступ до Java (JVM, JDK, синтаксис)",
            "Базові компоненти Java: JVM, JDK, синтаксис і перша програма.",
            [
                "[Офіційний туторіал Oracle \"Getting Started\"](https://docs.oracle.com/javase/tutorial/getStarted/index.html)",
                "[Java для початківців: JDK vs JRE vs JVM](https://medium.com/@javatechie/jdk-jre-jvm-8c9c8b6b1e4)",
                "[Перша програма Hello World (JavaRush)](https://javarush.com/ua/groups/posts/1716-javathread-pervaya-programma-hello-world)"
            ],
            [
                Q("Що таке JVM?", ("Java Virtual Machine – віртуальна машина, яка виконує байт-код", true), ("Java Version Manager", false), ("Java Variable Memory", false), ("Java Verification Model", false)),
                Q("Яке розширення має скомпільований файл у Java?", (".java", false), (".class", true), (".jar", false), (".exe", false)),
                Q("Що робить метод public static void main(String[] args)?", ("Точка входу у програму", true), ("Оголошує змінну", false), ("Виводить текст на екран", false), ("Завершує програму", false)),
                Q("Яка команда компілює Java-файл?", ("java MyProgram", false), ("javac MyProgram.java", true), ("compile MyProgram", false), ("run MyProgram", false)),
                Q("Який оператор використовується для виведення тексту в консоль?", ("System.out.print()", true), ("Console.write()", false), ("print.ln()", false), ("echo()", false))
            ],
            "Напишіть програму HelloWorld, яка виводить на екран \"Вітаю на курсі Java Fundamentals!\". Код програми надішліть текстом."));

        course.Modules.Add(BuildModule(
            2,
            "Змінні, типи даних, оператори",
            "Примітивні типи, оператори, змінні та константи в Java.",
            [
                "[Примітивні типи даних Java (Oracle)](https://docs.oracle.com/javase/tutorial/java/nutsandbolts/datatypes.html)",
                "[Оператори в Java: арифметичні, логічні](https://www.w3schools.com/java/java_operators.asp)",
                "[Змінні та константи в Java](https://www.baeldung.com/java-variables)"
            ],
            [
                Q("Який примітивний тип у Java найчастіше використовується для цілих чисел (діапазон ≈ ±2 млрд)?", ("int", true), ("short", false), ("long", false), ("byte", false)),
                Q("Що виведе код: int a = 5; int b = 2; System.out.println(a / b);?", ("2", true), ("2.5", false), ("2.0", false), ("Помилка", false)),
                Q("Який оператор перевіряє рівність значень?", ("=", false), ("==", true), ("!=", false), ("===", false)),
                Q("Яке ключове слово використовується для оголошення константи?", ("const", false), ("final", true), ("static", false), ("constant", false)),
                Q("Що таке тип double?", ("Ціле число", false), ("Число з плаваючою комою подвійної точності", true), ("Логічний тип", false), ("Символ", false))
            ],
            "Напишіть програму, яка оголошує дві змінні типу int (наприклад, 10 і 3), обчислює їх суму, різницю, добуток та частку (цілочисельну та залишок). Виведіть результати. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            3,
            "Умовні конструкції та цикли",
            "Умовний оператор if, switch case та цикли for/while/do-while.",
            [
                "[Умовний оператор if-then-else (Oracle)](https://docs.oracle.com/javase/tutorial/java/nutsandbolts/if.html)",
                "[Цикли for, while, do-while](https://www.w3schools.com/java/java_while_loop.asp)",
                "[Switch case в Java](https://www.baeldung.com/java-switch)"
            ],
            [
                Q("Що виведе код? int x = 10; if (x > 5) System.out.print(\"A\"); else if (x > 8) System.out.print(\"B\"); else System.out.print(\"C\");", ("A", true), ("B", false), ("C", false), ("AB", false)),
                Q("Який цикл виконається хоча б один раз, незалежно від умови?", ("for", false), ("while", false), ("do-while", true), ("foreach", false)),
                Q("Скільки разів виконається цикл? for (int i = 0; i < 3; i++) { }", ("2", false), ("3", true), ("4", false), ("1", false)),
                Q("Який оператор використовується для завершення циклу достроково?", ("continue", false), ("break", true), ("return", false), ("exit", false)),
                Q("Що таке оператор switch?", ("Багатоальтернативне розгалуження", true), ("Цикл", false), ("Оголошення змінної", false), ("Виняток", false))
            ],
            "Напишіть програму, яка виводить числа від 1 до 100, але замість чисел, кратних 3, виводить \"Fizz\", а замість кратних 5 – \"Buzz\", а якщо кратне і 3, і 5 – \"FizzBuzz\". (Класична задача FizzBuzz). Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            4,
            "Масиви та рядки",
            "Робота з масивами, рядками, String і StringBuilder.",
            [
                "[Масиви в Java (Oracle)](https://docs.oracle.com/javase/tutorial/java/nutsandbolts/arrays.html)",
                "[Рядки (String) та методи](https://www.w3schools.com/java/java_strings.asp)",
                "[StringBuilder vs String](https://www.baeldung.com/java-string-builder-string-buffer)"
            ],
            [
                Q("Як оголосити масив цілих чисел розміром 5?", ("int[] arr = new int[5];", true), ("int arr[5];", false), ("int arr = new int[5];", false), ("int[5] arr;", false)),
                Q("Який індекс першого елемента масиву?", ("1", false), ("0", true), ("-1", false), ("5", false)),
                Q("Який метод отримує довжину рядка?", ("length()", true), ("length", false), ("getLength()", false), ("size()", false)),
                Q("Який клас є незмінним (immutable)?", ("StringBuilder", false), ("String", true), ("StringBuffer", false), ("char[]", false)),
                Q("Як отримати символ з рядка за індексом?", ("charAt(index)", true), ("getChar(index)", false), ("charAt(index, string)", false), ("substring(index)", false))
            ],
            "Напишіть програму, яка створює масив цілих чисел (наприклад, {1,2,3,4,5}) і виводить суму всіх елементів. Також створіть рядок \"Hello Java\" і виведіть його у верхньому регістрі. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            5,
            "Основи ООП (класи, об'єкти, методи)",
            "Основи ООП: класи, об'єкти, методи, конструктори, інкапсуляція.",
            [
                "[Класи та об'єкти (Oracle)](https://docs.oracle.com/javase/tutorial/java/concepts/)",
                "[Методи та їх перевантаження](https://www.w3schools.com/java/java_methods.asp)",
                "[Конструктори та ключове слово `new`](https://www.baeldung.com/java-constructors)"
            ],
            [
                Q("Що є шаблоном для створення об'єктів?", ("Метод", false), ("Клас", true), ("Пакет", false), ("Інтерфейс", false)),
                Q("Яке ключове слово використовується для створення нового об'єкта?", ("create", false), ("new", true), ("object", false), ("alloc", false)),
                Q("Що таке конструктор?", ("Метод, який має таку ж назву, як клас, і викликається при створенні об'єкта", true), ("Звичайний метод", false), ("Статичне поле", false), ("Інтерфейс", false)),
                Q("Який модифікатор доступу дозволяє використовувати метод лише в межах класу?", ("public", false), ("private", true), ("protected", false), ("default", false)),
                Q("Що таке інкапсуляція?", ("Приховування даних та об'єднання з методами в одному класі", true), ("Спадкування", false), ("Поліморфізм", false), ("Абстракція", false))
            ],
            "Створіть клас Rectangle з полями width та height, конструктором, методами обчислення площі та периметру. У методі main створіть об'єкт і виведіть площу та периметр. Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Фінальний тест для Java Fundamentals",
        [
            Q("Який метод є точкою входу в програму?", ("main(String[] args)", true), ("start()", false), ("run()", false), ("init()", false)),
            Q("Що таке JDK?", ("Java Development Kit", true), ("Java Runtime Environment", false), ("Java Virtual Machine", false), ("Java Debug Kit", false)),
            Q("Який тип даних використовується для символів?", ("char", true), ("string", false), ("int", false), ("byte", false)),
            Q("Який оператор логічне \"І\"?", ("&&", true), ("||", false), ("&", false), ("|", false)),
            Q("Як оголосити метод, який не повертає значення?", ("void", true), ("int", false), ("static", false), ("public", false)),
            Q("Що таке наслідування в ООП?", ("Механізм створення нового класу на основі існуючого", true), ("Приховування даних", false), ("Виконання декількох завдань одночасно", false), ("Перевантаження методів", false)),
            Q("Який цикл перевіряє умову перед виконанням?", ("while", true), ("do-while", false), ("for-each", false), ("loop", false)),
            Q("Що таке пакет (package) у Java?", ("Група класів", true), ("Один клас", false), ("Метод", false), ("Змінна", false)),
            Q("Як отримати довжину масиву arr?", ("arr.length", true), ("arr.length()", false), ("length(arr)", false), ("arr.size()", false)),
            Q("Що виведе код? System.out.println(10 % 3);", ("1", true), ("3", false), ("0", false), ("10", false))
        ]));
    }

    private static async Task RebuildFrontendFundamentalsAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(
            1,
            "HTML5 семантика",
            "Семантичні теги та структура HTML5-сторінки.",
            [
                "[HTML5 Semantic Elements (MDN)](https://developer.mozilla.org/en-US/docs/Glossary/Semantics)",
                "[HTML5 Structural Elements: header, nav, main, article, section, footer](https://www.w3schools.com/html/html5_semantic_elements.asp)",
                "[Основні теги HTML для початківців](https://htmlreference.io/)"
            ],
            [
                Q("Який тег використовується для головного заголовка сторінки?", ("<heading>", false), ("<h1>", true), ("<title>", false), ("<head>", false)),
                Q("Який тег позначає навігаційне меню?", ("<menu>", false), ("<nav>", true), ("<navigation>", false), ("<ul>", false)),
                Q("Де розміщуються метатеги та заголовок сторінки?", ("<header>", false), ("<head>", true), ("<body>", false), ("<html>", false)),
                Q("Який тег створює абзац?", ("<p>", true), ("<para>", false), ("<text>", false), ("<div>", false)),
                Q("Який тег використовується для найважливішого заголовка другого рівня?", ("<h1>", false), ("<h2>", true), ("<h3>", false), ("<title>", false))
            ],
            "Напишіть HTML-код структури простої сторінки із заголовком, навігацією (посилання \"Головна\", \"Про нас\", \"Контакти\"), головним розділом з текстом та підвалом. Використовуйте семантичні теги. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            2,
            "CSS3 основи (селектори, кольори, шрифти)",
            "Базові CSS-селектори, кольори і типографіка.",
            [
                "[CSS селектори (MDN)](https://developer.mozilla.org/en-US/docs/Learn/CSS/Building_blocks/Selectors)",
                "[Кольори в CSS: назви, rgb, hex](https://www.w3schools.com/css/css_colors.asp)",
                "[Властивості шрифтів: font-family, font-size, font-weight](https://css-tricks.com/almanac/properties/f/font/)"
            ],
            [
                Q("Який селектор вибирає всі елементи з класом \"example\"?", ("#example", false), (".example", true), ("example", false), ("*example", false)),
                Q("Який CSS-колір відповідає білому?", ("#FFFFFF", true), ("#000000", false), ("rgb(0,0,0)", false), ("#FF00FF", false)),
                Q("Яка властивість змінює колір тексту?", ("background-color", false), ("color", true), ("text-color", false), ("font-color", false)),
                Q("Як задати розмір шрифту 20 пікселів?", ("font-size: 20px;", true), ("font: 20px;", false), ("text-size: 20px;", false), ("size: 20px;", false)),
                Q("Який селектор вибирає всі елементи сторінки?", ("all", false), ("*", true), ("body", false), ("html", false))
            ],
            "Напишіть CSS-правила, які: встановлюють фон сторінки світло-сірий, для заголовка h1 – синій колір та шрифт Arial розміром 30px, для всіх абзаців – чорний шрифт 16px. Додайте ці правила до HTML з попередньої практики (або окремо). Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            3,
            "Flexbox та Grid",
            "Створення макетів за допомогою Flexbox і CSS Grid.",
            [
                "[A Complete Guide to Flexbox (CSS-Tricks)](https://css-tricks.com/snippets/css/a-guide-to-flexbox/)",
                "[A Complete Guide to Grid (CSS-Tricks)](https://css-tricks.com/snippets/css/complete-guide-grid/)",
                "[Flexbox Froggy (гра для вивчення)](https://flexboxfroggy.com/uk)"
            ],
            [
                Q("Яка властивість Flexbox визначає напрямок головної осі?", ("flex-wrap", false), ("flex-direction", true), ("justify-content", false), ("align-items", false)),
                Q("Що робить justify-content: center?", ("Центрує елементи по головній осі", true), ("Вирівнює по поперечній осі", false), ("Задає відступи", false), ("Встановлює напрямок", false)),
                Q("Яка властивість Grid створює колонки?", ("grid-template-rows", false), ("grid-template-columns", true), ("grid-gap", false), ("grid-area", false)),
                Q("Щоб зробити контейнер Flexbox, потрібно задати:", ("display: flex;", true), ("display: grid;", false), ("display: block;", false), ("display: inline;", false)),
                Q("Як центрувати елементи по вертикалі у Flexbox?", ("align-items: center;", true), ("justify-content: center;", false), ("vertical-align: middle;", false), ("align-content: center;", false))
            ],
            "Створіть HTML/CSS макет з трьох карток (наприклад, інформація про курси), які вирівняні горизонтально за допомогою Flexbox. Картки повинні мати фон, тінь, та текст. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            4,
            "JavaScript (змінні, функції, події)",
            "Вступ до JS: let/const, функції та обробка подій.",
            [
                "[Змінні в JS: var, let, const (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Grammar_and_types#declarations)",
                "[Функції в JavaScript](https://www.w3schools.com/js/js_functions.asp)",
                "[Обробка подій (onclick, addEventListener)](https://javascript.info/introduction-browser-events)"
            ],
            [
                Q("Яке ключове слово використовується для оголошення змінної з блочною областю видимості?", ("var", false), ("let", true), ("const", false), ("global", false)),
                Q("Як оголосити функцію з іменем sayHello?", ("function sayHello() {}", true), ("def sayHello() {}", false), ("sayHello = function() {}", false), ("func sayHello() {}", false)),
                Q("Який метод додає обробник події до елементу?", ("addEvent()", false), ("addEventListener()", true), ("on()", false), ("bindEvent()", false)),
                Q("Що таке const?", ("Змінна, яку не можна перевизначити", true), ("Звичайна змінна", false), ("Константа, що не змінює значення", false), ("Тип даних", false)),
                Q("Як вивести повідомлення в консоль браузера?", ("console.log()", true), ("print()", false), ("alert()", false), ("log.console()", false))
            ],
            "Напишіть HTML-сторінку з кнопкою \"Натисни мене\". При натисканні на кнопку має з'являтися спливаюче вікно (alert) з текстом \"Привіт, світ!\". Код надішліть текстом (HTML+JS)."));

        course.Modules.Add(BuildModule(
            5,
            "DOM маніпуляції",
            "Пошук та зміна елементів DOM, створення і видалення вузлів.",
            [
                "[Вступ до DOM (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model/Introduction)",
                "[Методи пошуку елементів: getElementById, querySelector](https://www.w3schools.com/js/js_htmldom_elements.asp)",
                "[Зміна вмісту та атрибутів](https://javascript.info/modifying-document)"
            ],
            [
                Q("Який метод повертає елемент за id?", ("getElementById()", true), ("querySelector()", false), ("getElementByClass()", false), ("getById()", false)),
                Q("Як змінити текстовий вміст елемента?", ("element.value = \"текст\"", false), ("element.innerHTML = \"текст\"", true), ("element.textContent = \"текст\"", false), ("element.content = \"текст\"", false)),
                Q("Як створити новий елемент div?", ("document.createElement(\"div\")", true), ("document.newElement(\"div\")", false), ("document.add(\"div\")", false), ("new div()", false)),
                Q("Як додати створений елемент до body?", ("body.appendChild(element)", true), ("body.add(element)", false), ("document.appendChild(element)", false), ("element.appendToBody()", false)),
                Q("Який метод видаляє елемент з DOM?", ("removeChild()", false), ("element.remove()", true), ("delete element", false), ("removeElement()", false))
            ],
            "Створіть HTML-сторінку з полем введення (input) та кнопкою \"Додати елемент\". При натисканні на кнопку текст з поля додається як новий пункт списку (<li>). Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Фінальний тест для Frontend Fundamentals",
        [
            Q("HTML – це ...", ("Мова розмітки гіпертексту", true), ("Мова програмування", false), ("Система управління базами даних", false), ("Графічний редактор", false)),
            Q("Який тег використовується для вставки зображення?", ("<img>", true), ("<image>", false), ("<pic>", false), ("<src>", false)),
            Q("Яка CSS-властивість задає фон?", ("background-color", true), ("color", false), ("bgcolor", false), ("background-colour", false)),
            Q("Як обрати елемент з id=\"header\" у CSS?", ("#header", true), (".header", false), ("header", false), ("*header", false)),
            Q("Що таке Flexbox?", ("Модель компонування для створення гнучких макетів", true), ("Графічна бібліотека", false), ("Мова програмування", false), ("База даних", false)),
            Q("Як оголосити змінну в JavaScript?", ("let x = 5;", true), ("var x = 5;", false), ("x = 5;", false), ("int x = 5;", false)),
            Q("Яка подія відбувається при кліку на елемент?", ("onclick", true), ("onmouseover", false), ("onchange", false), ("onsubmit", false)),
            Q("Як отримати доступ до елемента за класом \"test\" у JS?", ("document.querySelector(\".test\")", true), ("document.getElementById(\"test\")", false), ("document.getElementsByTagName(\"test\")", false), ("$(\".test\")", false)),
            Q("Що таке DOM?", ("Об'єктна модель документа, яка представляє структуру HTML", true), ("Мова запитів", false), ("Графічний інтерфейс", false), ("Серверна мова", false)),
            Q("Як змінити текст елемента за допомогою innerHTML?", ("element.innerHTML = \"Новий текст\";", true), ("element.text = \"Новий текст\";", false), ("element.innerHTML(\"Новий текст\");", false), ("element.changeText(\"Новий текст\");", false))
        ]));
    }

    private static async Task RebuildCppFundamentalsAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(
            1,
            "Вступ до C++ (історія, компілятори, Hello World)",
            "Історія мови, компілятори та перша програма на C++.",
            [
                "Офіційний туторіал cppreference \"Getting started\"",
                "Вступ до C++ (learncpp.com)",
                "Компіляція та запуск першої програми"
            ],
            [
                Q("Яке розширення мають файли з вихідним кодом C++?", (".c", false), (".cpp", true), (".cxx", false), (".cc", false)),
                Q("Яка функція є точкою входу в програму C++?", ("start()", false), ("main()", true), ("run()", false), ("begin()", false)),
                Q("Який оператор використовується для виведення тексту в консоль?", ("printf", false), ("cout <<", true), ("print", false), ("console.log", false)),
                Q("Яка бібліотека потрібна для використання cout?", ("<stdio.h>", false), ("<iostream>", true), ("<cstdlib>", false), ("<conio.h>", false)),
                Q("Як скомпілювати програму hello.cpp за допомогою g++?", ("g++ hello.cpp (правильна, створює a.out)", true), ("compile hello.cpp", false), ("build hello.cpp", false), ("run hello.cpp", false))
            ],
            "Напишіть програму на C++, яка виводить \"Hello, C++!\". Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            2,
            "Змінні, типи даних, оператори",
            "Примітивні типи, оператори, змінні та константи в C++.",
            [
                "Типи даних у C++ (cppreference)",
                "Оператори в C++",
                "Змінні та константи"
            ],
            [
                Q("Який тип даних використовується для цілих чисел (зазвичай 4 байти)?", ("char", false), ("int", true), ("float", false), ("double", false)),
                Q("Що виведе cout << 7 / 2;?", ("3", true), ("3.5", false), ("3.0", false), ("Помилка", false)),
                Q("Який оператор порівняння \"дорівнює\"?", ("=", false), ("==", true), ("!=", false), ("<=", false)),
                Q("Як оголосити константу, яку не можна змінити?", ("const int x = 5;", true), ("int const x = 5; (теж вірно, але A простіше)", false), ("constant int x = 5;", false), ("final int x = 5;", false)),
                Q("Який тип даних для чисел з рухомою комою подвійної точності?", ("float", false), ("double", true), ("long double", false), ("decimal", false))
            ],
            "Напишіть програму, яка оголошує змінні a=10, b=3 і обчислює суму, різницю, добуток, частку (цілочисельну) та остачу. Виведіть результати. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            3,
            "Умовні оператори та цикли",
            "Умовні оператори if/else/switch та цикли for/while/do-while.",
            [
                "if, else, switch (learncpp)",
                "Цикли for, while, do-while",
                "Оператори break і continue"
            ],
            [
                Q("Що виведе код?\nint x = 5;\nif (x < 3) cout << \"A\";\nelse if (x > 3) cout << \"B\";\nelse cout << \"C\";", ("A", false), ("B", true), ("C", false), ("Нічого", false)),
                Q("Який цикл виконує тіло хоча б один раз?", ("for", false), ("while", false), ("do-while", true), ("foreach", false)),
                Q("Скільки ітерацій виконає цикл for (int i=0; i<5; i++)?", ("4", false), ("5", true), ("6", false), ("0", false)),
                Q("Який оператор негайно завершує цикл?", ("continue", false), ("break", true), ("return", false), ("exit", false)),
                Q("Що робить switch?", ("вибирає одну з багатьох гілок", true), ("цикл", false), ("оголошення змінної", false), ("функцію", false))
            ],
            "Напишіть програму, яка виводить всі числа від 1 до 50, які діляться на 3. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            4,
            "Масиви та рядки",
            "Масиви та робота з рядками у C++.",
            [
                "Масиви в C++",
                "Рядки (string, c-string)",
                "Базові операції з масивами"
            ],
            [
                Q("Як оголосити масив з 10 цілих чисел?", ("int arr[10];", true), ("arr int[10];", false), ("int[10] arr;", false), ("array<int> arr(10);", false)),
                Q("Який індекс має перший елемент масиву?", ("1", false), ("0", true), ("-1", false), ("перший", false)),
                Q("Як отримати довжину масиву (кількість елементів)?", ("arr.size()", false), ("sizeof(arr)/sizeof(arr[0])", true), ("arr.length()", false), ("len(arr)", false)),
                Q("Яка бібліотека для роботи з рядками std::string?", ("<cstring>", false), ("<string>", true), ("<strlib>", false), ("<strings>", false)),
                Q("Як скопіювати рядок \"Hello\" у змінну str?", ("string str = \"Hello\";", true), ("str = \"Hello\";", false), ("string str(\"Hello\");", false), ("всі варіанти (A і C правильні, але для тесту виберемо A)", false))
            ],
            "Напишіть програму, яка створює масив з п'яти цілих чисел, введених користувачем, і виводить їх у зворотньому порядку. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            5,
            "Вказівники та функції",
            "Вказівники, функції та передача аргументів.",
            [
                "Вказівники (cppreference)",
                "Функції в C++",
                "Передача аргументів за значенням та за посиланням"
            ],
            [
                Q("Що таке вказівник?", ("Змінна, що зберігає адресу пам'яті", true), ("Тип даних", false), ("Функція", false), ("Оператор", false)),
                Q("Як отримати адресу змінної x?", ("*x", false), ("&x", true), ("x&", false), ("адреса(x)", false)),
                Q("Що робить оператор *p (розіменування) ?", ("повертає значення за адресою", true), ("повертає адресу", false), ("змінює тип", false), ("видаляє змінну", false)),
                Q("Як оголосити функцію, яка повертає ціле число і приймає два цілих?", ("int func(int a, int b) {}", true), ("void func(int a, int b) {}", false), ("func(int a, int b): int {}", false), ("int[] func(int a, int b) {}", false)),
                Q("Що таке перевантаження функцій?", ("Функції з однаковим іменем, але різними параметрами", true), ("Дві функції з різними іменами", false), ("Функція всередині функції", false), ("Рекурсія", false))
            ],
            "Напишіть функцію int max(int a, int b), яка повертає більше з двох чисел. Викличте її в main для чисел 15 і 30, виведіть результат. Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Фінальний тест для C++",
        [
            Q("Який заголовний файл потрібен для cout?", ("<iostream>", true), ("<cstdio>", false), ("<conio>", false), ("<ostream>", false)),
            Q("Що таке nullptr?", ("нульовий вказівник", true), ("константа", false), ("функція", false), ("тип даних", false)),
            Q("Який оператор виділяє пам'ять динамічно?", ("new", true), ("malloc", false), ("alloc", false), ("create", false)),
            Q("Як звільнити пам'ять, виділену за допомогою new?", ("delete", true), ("free", false), ("clear", false), ("remove", false)),
            Q("Що таке посилання (reference)?", ("альтернативне ім'я змінної", true), ("вказівник", false), ("функція", false), ("макрос", false)),
            Q("Який тип циклу використовується для перебору масиву (C++11)?", ("for (int x : arr)", true), ("foreach", false), ("each", false), ("for-each", false)),
            Q("Що означає const після метода класу?", ("метод не змінює стан об'єкта", true), ("метод повертає константу", false), ("об'єкт незмінний", false), ("метод не можна викликати", false)),
            Q("Як передати масив у функцію в C++?", ("через вказівник", true), ("за значенням", false), ("за посиланням", false), ("не можна", false)),
            Q("Що таке std::vector?", ("динамічний масив", true), ("статичний масив", false), ("рядок", false), ("список", false)),
            Q("Яка бібліотека для алгоритмів (сортування тощо)?", ("<algorithm>", true), ("<algo>", false), ("<sort>", false), ("<utility>", false))
        ]));
    }

    private static async Task RebuildGoFundamentalsAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(
            1,
            "Вступ до Go (історія, установка, Hello World)",
            "Встановлення Go, запуск першої програми та базові поняття.",
            [
                "Офіційний тур по Go",
                "Встановлення Go",
                "Перша програма на Go"
            ],
            [
                Q("Яке розширення файлів Go?", (".g", false), (".go", true), (".golang", false), (".gol", false)),
                Q("Яка функція є точкою входу?", ("main()", true), ("start()", false), ("run()", false), ("begin()", false)),
                Q("Як вивести текст у консоль?", ("fmt.Println()", true), ("print()", false), ("println()", false), ("console.log()", false)),
                Q("Який пакет потрібен для виведення?", ("\"fmt\"", true), ("\"print\"", false), ("\"io\"", false), ("\"os\"", false)),
                Q("Як скомпілювати програму hello.go?", ("go build hello.go", true), ("go run hello.go", false), ("compile hello.go", false), ("gcc hello.go", false))
            ],
            "Напишіть програму на Go, яка виводить \"Вітаю у світі Go!\". Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            2,
            "Змінні, типи даних, основні конструкції",
            "Змінні, базові типи, константи та ініціалізація.",
            [
                "Змінні в Go",
                "Базові типи даних",
                "Константи та ініціалізація"
            ],
            [
                Q("Як оголосити змінну x типу int зі значенням 5?", ("var x int = 5", true), ("x := 5 (теж вірно, але краще A)", false), ("int x = 5", false), ("x = 5", false)),
                Q("Що виведе код? x := 7 / 2; fmt.Println(x)", ("3", true), ("3.5", false), ("3.0", false), ("помилка", false)),
                Q("Який тип даних для чисел з плаваючою комою (64 біти)?", ("float32", false), ("float64", true), ("float", false), ("double", false)),
                Q("Як оголосити константу?", ("const pi = 3.14", true), ("var pi = 3.14", false), ("let pi = 3.14", false), ("final pi = 3.14", false)),
                Q("Що таке :=?", ("коротке оголошення змінної", true), ("присвоєння", false), ("порівняння", false), ("декларування типу", false))
            ],
            "Напишіть програму, яка обчислює площу прямокутника зі сторонами 10 і 20. Виведіть результат. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            3,
            "Управління потоком (if, for, switch)",
            "Умовні оператори та цикл for в Go.",
            [
                "if та else в Go",
                "Цикл for (єдиний цикл в Go)",
                "Switch"
            ],
            [
                Q("Як записати цикл for, що повторюється 10 разів?", ("for i := 0; i < 10; i++", true), ("for (i=0; i<10; i++)", false), ("for i in 0..9", false), ("range 10", false)),
                Q("Чи є в Go цикл while?", ("Так, while", false), ("Ні, використовується for без умови", true), ("є repeat-until", false), ("є do-while", false)),
                Q("Що виведе?\nx := 3\nif x > 5 { fmt.Print(\"A\") } else { fmt.Print(\"B\") }", ("A", false), ("B", true), ("C", false), ("помилка", false)),
                Q("Як написати безкінечний цикл?", ("for true {}", false), ("for {}", false), ("for ; ; {}", false), ("всі варіанти", true)),
                Q("Що робить break в циклі?", ("завершує цикл", true), ("переходить до наступної ітерації", false), ("завершує програму", false), ("нічого", false))
            ],
            "Напишіть програму, яка виводить всі парні числа від 1 до 20. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            4,
            "Масиви, зрізи (slices), мапи",
            "Масиви, slices та maps у Go.",
            [
                "Масиви в Go",
                "Зрізи (slices)",
                "Мапи (maps)"
            ],
            [
                Q("Як створити масив з трьох цілих чисел [1,2,3]?", ("arr := [3]int{1,2,3}", true), ("arr := []int{1,2,3}", false), ("arr := [1,2,3]", false), ("var arr [3]int = {1,2,3}", false)),
                Q("Що таке зріз (slice)?", ("динамічний масив", true), ("статичний масив", false), ("вказівник", false), ("структура", false)),
                Q("Як додати елемент до зрізу slice?", ("slice.append(5)", false), ("append(slice, 5)", true), ("slice = slice + 5", false), ("slice.push(5)", false)),
                Q("Як створити мапу (map) з рядковими ключами та цілими значеннями?", ("m := make(map[string]int)", true), ("m := map[string]int{}", false), ("обидва варіанти правильні (D)", false), ("тільки A, але давайте виберемо A як основну", false)),
                Q("Як отримати довжину зрізу?", ("len(slice)", true), ("slice.length()", false), ("slice.len", false), ("size(slice)", false))
            ],
            "Напишіть програму, яка створює зріз цілих чисел [10,20,30] і додає до нього число 40. Виведіть отриманий зріз. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            5,
            "Функції, структури, методи",
            "Функції, структури та методи у Go.",
            [
                "Функції в Go",
                "Структури (struct)",
                "Методи на структурах"
            ],
            [
                Q("Як оголосити функцію, що повертає int, з параметром int?", ("func myFunc(x int) int { return x*2 }", true), ("function myFunc(x int) int { return x*2 }", false), ("def myFunc(x int): return x*2", false), ("int myFunc(int x) { return x*2 }", false)),
                Q("Що таке структура (struct)?", ("набір полів", true), ("масив", false), ("інтерфейс", false), ("функція", false)),
                Q("Як визначити метод для структури Person?", ("func (p Person) SayHello()", true), ("func Person.SayHello()", false), ("method SayHello(p Person)", false), ("Person::SayHello", false)),
                Q("Що таке вказівник на структуру?", ("*Person", true), ("&Person", false), ("Person*", false), ("ref Person", false)),
                Q("Яке ключове слово для інтерфейсу?", ("interface", true), ("implements", false), ("class", false), ("struct", false))
            ],
            "Створіть структуру Rectangle з полями width, height типу float64. Додайте метод area(), що обчислює площу. У main створіть об'єкт і виведіть площу. Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Фінальний тест для Go",
        [
            Q("Що таке go routine?", ("легковаговий потік", true), ("функція", false), ("тип даних", false), ("пакет", false)),
            Q("Як запустити go routine?", ("go myFunc()", true), ("routine myFunc()", false), ("go run myFunc()", false), ("start myFunc()", false)),
            Q("Що таке defer?", ("відкладає виконання функції до повернення", true), ("відкладає оголошення", false), ("створює відкладений потік", false), ("видаляє змінну", false)),
            Q("Як обробити помилку в Go (типовий патерн)?", ("перевірка значення помилки", true), ("try-catch", false), ("throw", false), ("exception", false)),
            Q("Що таке nil в Go?", ("нульове значення для вказівників, інтерфейсів", true), ("помилка", false), ("порожній масив", false), ("константа", false)),
            Q("Як створити новий канал (channel)?", ("ch := make(chan int)", true), ("ch := new(chan int)", false), ("ch := chan int{}", false), ("ch := channel(int)", false)),
            Q("Що робить оператор <-?", ("надсилання/отримання в канал", true), ("присвоєння", false), ("порівняння", false), ("розіменування", false)),
            Q("Як об'єднати два рядки?", ("str1 + str2", true), ("str1.concat(str2)", false), ("str1.append(str2)", false), ("join(str1, str2)", false)),
            Q("Що таке package main?", ("виконуваний пакет", true), ("бібліотека", false), ("тестовий пакет", false), ("пакет для main функції", false)),
            Q("Яка команда для форматування коду?", ("go fmt", true), ("go format", false), ("go lint", false), ("go tidy", false))
        ]));
    }

    private static async Task RebuildReactFundamentalsAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(
            1,
            "Вступ до React (JSX, компоненти, props)",
            "Основи React, JSX та передача props.",
            [
                "React офіційна документація – Головна концепція",
                "JSX в деталях (React)",
                "Компоненти та пропси"
            ],
            [
                Q("Що таке React?", ("бібліотека для побудови інтерфейсів", true), ("фреймворк", false), ("мова програмування", false), ("база даних", false)),
                Q("Який синтаксис дозволяє писати HTML в JavaScript?", ("JSX", true), ("HTML-in-JS", false), ("XML", false), ("TypeScript", false)),
                Q("Як створити функціональний компонент?", ("function MyComponent() { return <div>Hi</div> }", true), ("class MyComponent extends React.Component {}", false), ("component MyComponent() {}", false), ("createComponent(MyComponent)", false)),
                Q("Як передати пропс name зі значенням \"John\"?", ("<Component name=\"John\" />", true), ("<Component name={John} />", false), ("<Component name=John />", false), ("<Component {name:\"John\"} />", false)),
                Q("Що таке props?", ("властивості компонента, тільки для читання", true), ("стан компонента", false), ("методи компонента", false), ("події", false))
            ],
            "Створіть функціональний компонент Greeting, що приймає проп name і виводить <h1>Привіт, {name}!</h1>. Викличте його з name=\"Анна\". Код надішліть текстом (JSX)."));

        course.Modules.Add(BuildModule(
            2,
            "Стан (useState) та обробка подій",
            "Стан компонентів та обробка подій у React.",
            [
                "Стан компонента: useState",
                "Обробка подій у React",
                "Стан та життєвий цикл"
            ],
            [
                Q("Який хук використовується для створення стану?", ("useState", true), ("useEffect", false), ("useRef", false), ("useContext", false)),
                Q("Як оновити стан count?", ("setCount(newValue)", true), ("count = newValue", false), ("this.state.count = newValue", false), ("updateCount(newValue)", false)),
                Q("Яка подія використовується для обробки кліку?", ("onClick", true), ("onPress", false), ("onMouseClick", false), ("click", false)),
                Q("Що буде, якщо викликати useState поза функціональним компонентом?", ("помилка", true), ("нічого", false), ("створиться глобальний стан", false), ("попередження", false)),
                Q("Як зберігати значення між рендерами без оновлення компонента?", ("useRef", true), ("useMemo", false), ("useCallback", false), ("useState", false))
            ],
            "Створіть компонент Counter з кнопкою, при натисканні якої лічильник збільшується на 1. Відобразіть поточне значення. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            3,
            "useEffect та побічні ефекти",
            "Побічні ефекти у React та useEffect.",
            [
                "useEffect (React документація)",
                "Завантаження даних у useEffect",
                "Чистка ефектів"
            ],
            [
                Q("Для чого використовується useEffect?", ("виконання побічних ефектів", true), ("створення стану", false), ("рендеринг", false), ("передача пропсів", false)),
                Q("Як зробити useEffect, який запускається один раз при монтуванні?", ("useEffect(() => {}, [])", true), ("useEffect(() => {}, [state])", false), ("useEffect(() => {}, null)", false), ("useEffect(() => {}, true)", false)),
                Q("Як запускати ефект при зміні певної змінної?", ("вказати залежності в масиві", true), ("викликати ефект вручну", false), ("не можна", false), ("використовувати useLayoutEffect", false)),
                Q("Що таке cleanup функція в useEffect?", ("функція, яка виконується при розмонтуванні", true), ("функція, яка очищає стан", false), ("функція, яка запускається до ефекту", false), ("функція, яка оновлює DOM", false)),
                Q("Який хук використовується для синхронізації з зовнішніми даними?", ("useEffect", true), ("useSync", false), ("useExternal", false), ("useData", false))
            ],
            "Створіть компонент, який при монтуванні завантажує дані з публічного API (наприклад, jsonplaceholder.typicode.com/posts/1) і відображає заголовок поста. Використайте fetch. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            4,
            "Робота зі списками та ключі, форми",
            "Рендеринг списків, ключі та форми у React.",
            [
                "Рендеринг списків",
                "Ключі в React",
                "Форми та контрольовані компоненти"
            ],
            [
                Q("Як відрендерити масив елементів в JSX?", ("map", true), ("forEach", false), ("filter", false), ("reduce", false)),
                Q("Для чого потрібен атрибут key в списках?", ("ідентифікація елементів для оптимізації", true), ("унікальний ключ для порядку", false), ("стилізація", false), ("доступ до елемента", false)),
                Q("Що таке контрольований компонент форми?", ("стан компонента керує значенням поля", true), ("поле керує станом", false), ("DOM елемент керує значенням", false), ("форма не має стану", false)),
                Q("Як отримати значення з інпуту в контрольованому компоненті?", ("через стан (state)", true), ("через ref", false), ("через event.target.value безпосередньо", false), ("через document.getElementById", false)),
                Q("Що робить e.preventDefault() в обробнику форми?", ("запобігає перезавантаженню сторінки", true), ("запобігає відправці форми", false), ("запобігає запуску події", false), ("очищає форму", false))
            ],
            "Створіть компонент TodoList, який має поле вводу та кнопку \"Додати\". При кліку додає новий елемент до списку (використовуйте useState для масиву). Відобразіть список. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            5,
            "Маршрутизація (React Router) та контекст",
            "Маршрутизація та контекст у React.",
            [
                "React Router v6 основи",
                "Контекст в React (useContext)",
                "Навігація між сторінками"
            ],
            [
                Q("Який компонент використовується для визначення маршрутів?", ("Routes, Route", true), ("Router", false), ("Switch", false), ("Path", false)),
                Q("Як створити посилання для переходу без перезавантаження?", ("<Link to=\"/about\">", true), ("<a href=\"/about\">", false), ("<Nav to=\"/about\">", false), ("<Href to=\"/about\">", false)),
                Q("Що таке Context?", ("спосіб передачі даних через дерево компонентів без пропсів", true), ("глобальний стан", false), ("бібліотека маршрутизації", false), ("хук стану", false)),
                Q("Який хук використовується для доступу до контексту?", ("useContext", true), ("useConsumer", false), ("useProvider", false), ("useStore", false)),
                Q("Який хук дозволяє отримати параметри URL?", ("useParams", true), ("useRouteParams", false), ("useLocation", false), ("useQuery", false))
            ],
            "Створіть два компоненти: Home і About. Налаштуйте маршрутизацію (React Router) для них. Додайте навігацію за допомогою Link. Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Фінальний тест для React",
        [
            Q("Що таке JSX?", ("синтаксичне розширення JavaScript, що дозволяє писати HTML", true), ("мова програмування", false), ("бібліотека", false), ("фреймворк", false)),
            Q("Як передати функцію обробки кліку в компонент?", ("onClick={handleClick}", true), ("onClick=\"handleClick\"", false), ("onclick={handleClick}", false), ("onClick={handleClick()}", false)),
            Q("Що таке Virtual DOM?", ("легковагове представлення реального DOM", true), ("база даних", false), ("ще одна бібліотека", false), ("браузерний API", false)),
            Q("Як іменуються файли компонентів зазвичай?", ("PascalCase", true), ("camelCase", false), ("kebab-case", false), ("snake_case", false)),
            Q("Що робить useMemo?", ("мемоізує значення", true), ("мемоізує функцію", false), ("створює реф", false), ("оновлює стан", false)),
            Q("Що робить React.memo?", ("мемоізує компонент, запобігає зайвому рендеру", true), ("мемоізує пропси", false), ("прискорює рендер", false), ("робить компонент чистим", false)),
            Q("Як прийнято робити запити до API в React (без бібліотек)?", ("useEffect + fetch", true), ("useState + fetch", false), ("useRef + fetch", false), ("компонент-контейнер", false)),
            Q("Що таке \"підйом стану\" (lifting state up)?", ("переміщення стану до найближчого спільного предка", true), ("створення глобального стану", false), ("видалення стану", false), ("використання контексту", false)),
            Q("Яке розширення мають файли TypeScript для React?", (".tsx", true), (".ts", false), (".jsx", false), (".react", false)),
            Q("Який хук дозволяє отримати доступ до DOM-елемента?", ("useRef", true), ("useDOM", false), ("useElement", false), ("useCallback", false))
        ]));
    }

    private static async Task RebuildAngularFundamentalsAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(
            1,
            "Вступ до Angular (компоненти, модулі, CLI)",
            "Основи Angular, компоненти та CLI.",
            [
                "Офіційна документація Angular – Вступ",
                "Angular CLI – встановлення та створення проєкту",
                "Компоненти та шаблони"
            ],
            [
                Q("Яка команда створює новий проєкт Angular?", ("ng new project-name", true), ("angular create project-name", false), ("create-react-app project-name", false), ("ng init", false)),
                Q("Що таке компонент в Angular?", ("клас з контролером та шаблоном", true), ("функція", false), ("сервіс", false), ("маршрут", false)),
                Q("Який декоратор позначає клас як компонент?", ("@Component", true), ("@NgModule", false), ("@Injectable", false), ("@Directive", false)),
                Q("Який файл є кореневим модулем?", ("app.module.ts", true), ("main.ts", false), ("index.html", false), ("app.component.ts", false)),
                Q("Як запустити проєкт Angular для розробки?", ("ng serve", true), ("ng build", false), ("ng start", false), ("angular serve", false))
            ],
            "Створіть компонент HelloComponent з шаблоном, який виводить \"Hello Angular\". (Синтаксис TypeScript, не потрібен повний проєкт – достатньо коду компонента). Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            2,
            "Прив'язка даних та директиви",
            "Інтерполяція, директиви та двостороння прив'язка.",
            [
                "Прив'язка даних (інтерполяція, property, event)",
                "Вбудовані директиви: ngIf, ngFor, ngSwitch",
                "Двостороння прив'язка (ngModel)"
            ],
            [
                Q("Який синтаксис інтерполяції в Angular?", ("{{ }})", true), ("{ }", false), ("[ ]", false), ("( )", false)),
                Q("Яка директива використовується для умовного рендерингу?", ("*ngIf", true), ("*ngFor", false), ("*ngSwitch", false), ("*ngShow", false)),
                Q("Як зв'язати властивість src зі змінною imageUrl?", ("[src]=\"imageUrl\"", true), ("src=\"{{imageUrl}}\"", false), ("bind-src=\"imageUrl\"", false), ("src={imageUrl}", false)),
                Q("Як обробити подію кліку?", ("(click)=\"onClick()\"", true), ("on-click=\"onClick()\"", false), ("[click]=\"onClick()\"", false), ("click=\"onClick()\"", false)),
                Q("Що таке двостороння прив'язка?", ("[(ngModel)]", true), ("[ngModel]", false), ("(ngModel)", false), ("[()]", false))
            ],
            "Створіть компонент, який має масив імен [\"Іван\",\"Марія\",\"Олег\"] та відображає їх у список <ul> за допомогою *ngFor. Код компонента та шаблону надішліть текстом."));

        course.Modules.Add(BuildModule(
            3,
            "Сервіси та Dependency Injection",
            "Сервіси, DI та HttpClient в Angular.",
            [
                "Сервіси в Angular",
                "Dependency Injection (DI)",
                "HttpClient для запитів"
            ],
            [
                Q("Який декоратор робить клас сервісом?", ("@Injectable", true), ("@Service", false), ("@Component", false), ("@NgModule", false)),
                Q("Де потрібно зареєструвати сервіс, щоб він був доступний у всьому додатку?", ("у providedIn: 'root'", true), ("в масиві providers компонента", false), ("в масиві imports", false), ("в масиві exports", false)),
                Q("Як використовувати HTTP-клієнт в Angular?", ("import HttpClientModule, inject HttpClient", true), ("використовувати fetch", false), ("axios", false), ("jQuery.ajax", false)),
                Q("Що таке Dependency Injection?", ("механізм передачі залежностей", true), ("створення об'єктів вручну", false), ("бібліотека", false), ("патерн проектування", false)),
                Q("Як отримати доступ до сервісу в компоненті?", ("constructor(private myService: MyService) {}", true), ("myService = new MyService()", false), ("inject(MyService)", false), ("ServiceLocator.get(MyService)", false))
            ],
            "Створіть сервіс DataService, який має метод getUsers(), що повертає об'єкт Promise (або Observable) з масивом користувачів (захардкодьте). Впорскуйте цей сервіс в компонент і виведіть список. Код надішліть текстом."));

        course.Modules.Add(BuildModule(
            4,
            "Маршрутизація (Router)",
            "Налаштування маршрутів та навігації в Angular.",
            [
                "Angular Router – основи",
                "RouterLink, RouterOutlet",
                "Гварди (Guard) для захисту маршрутів"
            ],
            [
                Q("Який компонент вказує, де відображати маршрути?", ("<router-outlet>", true), ("<route-outlet>", false), ("<router-view>", false), ("<outlet>", false)),
                Q("Як створити посилання для переходу на маршрут /about?", ("<a routerLink=\"/about\">", true), ("<a href=\"/about\">", false), ("<link to=\"/about\">", false), ("<router-link to=\"/about\">", false)),
                Q("Де визначаються маршрути в Angular?", ("в масиві routes (AppRoutingModule)", true), ("в main.ts", false), ("в компоненті", false), ("в сервісі", false)),
                Q("Як отримати параметр з URL (наприклад, id: /user/5)?", ("ActivatedRoute params", true), ("Router.params", false), ("Route.snapshot", false), ("Location.params", false)),
                Q("Що таке гвард (Guard)?", ("захист маршруту (перевірка доступу)", true), ("стиль", false), ("директивa", false), ("пайп", false))
            ],
            "Налаштуйте два маршрути: home та contact. Створіть відповідні компоненти. Додайте навігацію за допомогою routerLink. Код маршрутів та шаблону навігації надішліть текстом."));

        course.Modules.Add(BuildModule(
            5,
            "Форми та валідація (Reactive Forms)",
            "Reactive Forms, валідація та шаблони помилок.",
            [
                "Reactive Forms в Angular",
                "Вбудовані валідатори",
                "Створення власних валідаторів"
            ],
            [
                Q("Який модуль потрібно імпортувати для реактивних форм?", ("ReactiveFormsModule", true), ("FormsModule", false), ("FormModule", false), ("NgForm", false)),
                Q("Як створити FormGroup в компоненті?", ("this.form = new FormGroup({})", true), ("this.form = new Form()", false), ("@ViewChild('form')", false), ("createForm()", false)),
                Q("Як прив'язати FormGroup до HTML-форми?", ("[formGroup]=\"form\"", true), ("formGroup=\"form\"", false), ("[(formGroup)]=\"form\"", false), ("bind-formGroup=\"form\"", false)),
                Q("Який валідатор використовується для обов'язкового поля?", ("Validators.required", true), ("Validators.require", false), ("Validators.mandatory", false), ("Validators.notEmpty", false)),
                Q("Як отримати доступ до помилок валідації в шаблоні?", ("form.get('field').hasError('required')", true), ("form.errors.required", false), ("field.errors", false), ("form.invalid", false))
            ],
            "Створіть реактивну форму з полями \"Ім'я\" (обов'язкове) та \"Email\" (обов'язкове, формат email). Додайте відображення помилок під полями. Код компонента та шаблону надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Фінальний тест для Angular",
        [
            Q("Яка основна мова програмування в Angular?", ("TypeScript", true), ("JavaScript", false), ("Dart", false), ("Python", false)),
            Q("Що таке модуль (NgModule)?", ("контейнер для компонентів, директив, сервісів", true), ("компонент", false), ("сервіс", false), ("функція", false)),
            Q("Що таке декоратор?", ("спеціальний синтаксис, який додає метадані", true), ("функція", false), ("клас", false), ("інтерфейс", false)),
            Q("Який хук життєвого циклу викликається першим?", ("ngOnInit", true), ("ngOnChanges", false), ("ngAfterViewInit", false), ("constructor", false)),
            Q("Як передати дані від батьківського компонента до дочірнього?", ("@Input()", true), ("@Output()", false), ("@ViewChild", false), ("сервіс", false)),
            Q("Як випромінити подію з дочірнього компонента до батьківського?", ("@Output() + EventEmitter", true), ("@Input()", false), ("@ViewChild", false), ("сервіс", false)),
            Q("Що таке пайп (pipe)?", ("перетворення даних у шаблоні", true), ("сервіс", false), ("компонент", false), ("директивa", false)),
            Q("Який пайп використовується для форматування дати?", ("date", true), ("datetime", false), ("formatDate", false), ("datepipe", false)),
            Q("Що таке Angular CLI?", ("інтерфейс командного рядка для створення та управління проєктом", true), ("бібліотека", false), ("мова", false), ("фреймворк", false)),
            Q("Яка команда створює компонент MyComponent в Angular?", ("ng generate component MyComponent", true), ("ng new MyComponent", false), ("create component MyComponent", false), ("ng make component MyComponent", false))
        ]));
    }

    private static async Task RebuildCSharpAdvancedAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(1, "Advanced OOP and SOLID", "Поглиблені принципи ООП та SOLID у C#.",
            [
                "https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/",
                "https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures",
                "https://code-maze.com/solid-principles/"
            ],
            [
                Q("Що означає принцип S у SOLID?", ("Single Responsibility Principle", true), ("Safe Object Lifetime In Design", false), ("Structured Object Layering in Design", false), ("System Operation Logic Isolation Design", false)),
                Q("Який модифікатор забороняє наслідування класу?", ("sealed", true), ("private", false), ("readonly", false), ("internal", false)),
                Q("Що таке поліморфізм?", ("Можливість викликати перевизначену поведінку через базовий тип", true), ("Зберігання даних в колекції", false), ("Створення статичних класів", false), ("Робота з потоками", false)),
                Q("Який доступ дозволяє використання лише в межах збірки?", ("internal", true), ("public", false), ("protected", false), ("private", false)),
                Q("Що робить ключове слово virtual?", ("Дозволяє перевизначити метод у нащадку", true), ("Забороняє перевизначення", false), ("Робить поле константою", false), ("Створює анонімний тип", false))
            ],
            "Створіть ієрархію класів Shape -> Rectangle/Circle з перевизначенням методу Area(). Додайте приклад поліморфного виклику та надішліть код текстом."));

        course.Modules.Add(BuildModule(2, "LINQ and Collections", "Поглиблена робота з LINQ, проєкціями та групуванням.",
            [
                "https://learn.microsoft.com/dotnet/csharp/linq/",
                "https://learn.microsoft.com/dotnet/api/system.linq.enumerable",
                "https://www.tutorialsteacher.com/linq"
            ],
            [
                Q("Який метод LINQ використовується для фільтрації?", ("Where()", true), ("SelectMany()", false), ("OrderBy()", false), ("GroupBy()", false)),
                Q("Що повертає Select()?", ("Проєкцію елементів", true), ("Лише перший елемент", false), ("Суму значень", false), ("Кількість елементів", false)),
                Q("Який метод обчислює агреговане значення?", ("Aggregate()", true), ("Skip()", false), ("Take()", false), ("Contains()", false)),
                Q("Що робить ToList()?", ("Матеріалізує послідовність у список", true), ("Сортує за зростанням", false), ("Видаляє дублікати", false), ("Повертає останній елемент", false)),
                Q("Який метод LINQ групує елементи?", ("GroupBy()", true), ("Join()", false), ("Zip()", false), ("Reverse()", false))
            ],
            "Є список замовлень. За допомогою LINQ згрупуйте їх за клієнтом і порахуйте суму по кожному клієнту. Код надішліть текстом."));

        course.Modules.Add(BuildModule(3, "Async/Await and Tasks", "Асинхронне програмування в .NET та робота із Task.",
            [
                "https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/",
                "https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap",
                "https://code-maze.com/csharp-async-vs-sync/"
            ],
            [
                Q("Що повертає async-метод без результату?", ("Task", true), ("void", false), ("Thread", false), ("IEnumerable", false)),
                Q("Коли доречно використовувати async void?", ("Переважно лише для event handlers", true), ("Для всіх сервісних методів", false), ("Для репозиторіїв", false), ("Для контролерів API", false)),
                Q("Що робить await?", ("Неблокуюче очікує завершення Task", true), ("Блокує потік до завершення", false), ("Створює новий потік", false), ("Кешує результат", false)),
                Q("Який виняток часто виникає при блокуванні .Result у UI/ASP.NET?", ("Deadlock", true), ("StackOverflowException", false), ("DivideByZeroException", false), ("NullReferenceException", false)),
                Q("Що таке Task.WhenAll?", ("Очікування завершення кількох задач", true), ("Скасування всіх задач", false), ("Послідовний запуск задач", false), ("Повторний запуск невдалих задач", false))
            ],
            "Реалізуйте метод, який паралельно викликає 3 асинхронні операції через Task.WhenAll і повертає сумарний результат. Код надішліть текстом."));

        course.Modules.Add(BuildModule(4, "Dependency Injection and Architecture", "DI-контейнер, життєві цикли сервісів та архітектурні шари.",
            [
                "https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection",
                "https://learn.microsoft.com/dotnet/core/extensions/dependency-injection",
                "https://www.milanjovanovic.tech/blog/dependency-injection-lifetimes"
            ],
            [
                Q("Який lifetime створює один екземпляр на HTTP-запит?", ("Scoped", true), ("Singleton", false), ("Transient", false), ("Static", false)),
                Q("Що таке DI?", ("Передача залежностей ззовні замість створення всередині", true), ("Патерн для SQL-запитів", false), ("Система логування", false), ("Механізм кешування", false)),
                Q("Який lifetime створюється один раз на весь додаток?", ("Singleton", true), ("Scoped", false), ("Transient", false), ("Request", false)),
                Q("Що робить інтерфейс у DI-контексті?", ("Дає абстракцію і спрощує заміну реалізації", true), ("Автоматично генерує БД", false), ("Забороняє тестування", false), ("Підвищує зв'язність", false)),
                Q("Де в ASP.NET Core реєструють сервіси?", ("У Program.cs через builder.Services", true), ("У контролері", false), ("У View", false), ("У appsettings.json", false))
            ],
            "Створіть сервіс IMessageService з двома реалізаціями та підключіть одну через DI. Покажіть виклик із контролера. Код надішліть текстом."));

        course.Modules.Add(BuildModule(5, "Performance and Memory", "Оптимізація продуктивності, робота з пам'яттю, профілювання.",
            [
                "https://learn.microsoft.com/dotnet/core/diagnostics/",
                "https://learn.microsoft.com/dotnet/csharp/advanced-topics/performance/",
                "https://learn.microsoft.com/dotnet/standard/garbage-collection/fundamentals"
            ],
            [
                Q("Який інструмент підходить для пошуку hot path у .NET?", ("dotnet-trace", true), ("dotnet new", false), ("dotnet publish", false), ("dotnet user-secrets", false)),
                Q("Що зменшує алокації рядків у циклі?", ("StringBuilder", true), ("String.Concat в кожній ітерації", false), ("dynamic", false), ("reflection", false)),
                Q("Що таке boxing?", ("Перетворення value type в object", true), ("Стиснення колекції", false), ("Видалення сміття GC", false), ("Оптимізація JIT", false)),
                Q("Що робить Span<T>?", ("Дозволяє працювати з пам'яттю без додаткових алокацій", true), ("Створює новий масив", false), ("Виконує SQL-запит", false), ("Гарантує thread safety", false)),
                Q("Який режим компіляції зазвичай швидший у виконанні?", ("Release", true), ("Debug", false), ("Design", false), ("Test", false))
            ],
            "Оптимізуйте обробку великого тексту: замініть конкатенацію рядків у циклі на StringBuilder і покажіть різницю в підході. Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Final test for C# Advanced", BuildFinalPlaceholderQuestions()));
    }

    private static async Task RebuildJavaScriptFullStackAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(1, "JavaScript Core", "Синтаксис, області видимості, функції, замикання.",
            [
                "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide",
                "https://javascript.info/closure",
                "https://www.freecodecamp.org/news/javascript-closures-explained-with-example/"
            ],
            BuildPlaceholderQuestions("JavaScript core"),
            "Реалізуйте функцію-лічильник через closure, яка збільшує значення при кожному виклику. Код надішліть текстом."));

        course.Modules.Add(BuildModule(2, "Frontend Architecture", "Компонентний підхід, стан, роутинг, форми.",
            [
                "https://react.dev/learn",
                "https://redux.js.org/introduction/getting-started",
                "https://vuejs.org/guide/introduction.html"
            ],
            BuildPlaceholderQuestions("Frontend architecture"),
            "Побудуйте форму реєстрації з валідацією полів на клієнті. Код надішліть текстом."));

        course.Modules.Add(BuildModule(3, "Backend with Node.js", "Express, REST API, middleware, валідація.",
            [
                "https://expressjs.com/",
                "https://nodejs.dev/en/learn/",
                "https://www.freecodecamp.org/news/build-a-restful-api-using-node-express-and-mongodb/"
            ],
            BuildPlaceholderQuestions("Node backend"),
            "Створіть REST endpoint GET /api/courses, який повертає список курсів у JSON. Код надішліть текстом."));

        course.Modules.Add(BuildModule(4, "Databases and Auth", "Підключення БД, JWT, безпека авторизації.",
            [
                "https://www.mongodb.com/docs/manual/",
                "https://jwt.io/introduction",
                "https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html"
            ],
            BuildPlaceholderQuestions("Databases and auth"),
            "Додайте JWT-авторизацію для захищеного endpoint і приклад перевірки токена. Код надішліть текстом."));

        course.Modules.Add(BuildModule(5, "Testing and Deployment", "Тести, CI/CD, контейнеризація та деплой.",
            [
                "https://jestjs.io/docs/getting-started",
                "https://docs.github.com/actions",
                "https://docs.docker.com/get-started/"
            ],
            BuildPlaceholderQuestions("Testing and deployment"),
            "Напишіть один unit-тест для сервісу й опишіть кроки деплою в Docker. Код/інструкцію надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Final test for JavaScript Full-Stack", BuildFinalPlaceholderQuestions()));
    }

    private static async Task RebuildPythonForDataScienceAsync(ApplicationDbContext dbContext, Course course)
    {
        await PurgeCourseContentAsync(dbContext, course.Id);

        course.Modules.Add(BuildModule(1, "Python Basics for DS", "Типи даних, функції, структури, робота з файлами.",
            [
                "https://docs.python.org/3/tutorial/",
                "https://realpython.com/python-basics/",
                "https://www.w3schools.com/python/"
            ],
            BuildPlaceholderQuestions("Python basics"),
            "Напишіть скрипт, який читає CSV і виводить базову статистику по числовому стовпцю. Код надішліть текстом."));

        course.Modules.Add(BuildModule(2, "NumPy and Pandas", "Масиви, DataFrame, очищення та трансформації даних.",
            [
                "https://numpy.org/doc/stable/user/quickstart.html",
                "https://pandas.pydata.org/docs/user_guide/10min.html",
                "https://www.kaggle.com/learn/pandas"
            ],
            BuildPlaceholderQuestions("NumPy and Pandas"),
            "Завантажте таблицю в Pandas, приберіть пропуски та порахуйте середнє значення по групах. Код надішліть текстом."));

        course.Modules.Add(BuildModule(3, "Data Visualization", "Візуалізація даних через Matplotlib/Seaborn.",
            [
                "https://matplotlib.org/stable/users/getting_started/",
                "https://seaborn.pydata.org/tutorial.html",
                "https://plotly.com/python/"
            ],
            BuildPlaceholderQuestions("Data visualization"),
            "Побудуйте стовпчикову діаграму та гістограму для одного датасету. Код надішліть текстом."));

        course.Modules.Add(BuildModule(4, "Machine Learning Basics", "Навчання з учителем, метрики, train/test split.",
            [
                "https://scikit-learn.org/stable/getting_started.html",
                "https://developers.google.com/machine-learning/crash-course",
                "https://www.kaggle.com/learn/intro-to-machine-learning"
            ],
            BuildPlaceholderQuestions("ML basics"),
            "Навчіть просту модель класифікації в scikit-learn і виведіть accuracy. Код надішліть текстом."));

        course.Modules.Add(BuildModule(5, "Model Evaluation and Deployment", "Крос-валідація, пайплайни, сервіси для моделей.",
            [
                "https://scikit-learn.org/stable/modules/cross_validation.html",
                "https://mlflow.org/docs/latest/index.html",
                "https://fastapi.tiangolo.com/"
            ],
            BuildPlaceholderQuestions("Model evaluation and deployment"),
            "Створіть пайплайн передобробки + моделі та оцініть її через cross-validation. Код надішліть текстом."));

        course.Tests.Add(BuildFinalTest("Final test for Python for Data Science", BuildFinalPlaceholderQuestions()));
    }

    private static async Task PurgeCourseContentAsync(ApplicationDbContext dbContext, int courseId)
    {
        await dbContext.Materials.Where(m => m.Module!.CourseId == courseId).ExecuteDeleteAsync();
        await dbContext.AnswerOptions.Where(a => a.Question!.Test!.Module!.CourseId == courseId || (a.Question!.Test!.CourseId == courseId)).ExecuteDeleteAsync();
        await dbContext.Questions.Where(q => q.Test!.Module!.CourseId == courseId || (q.Test!.CourseId == courseId)).ExecuteDeleteAsync();
        await dbContext.Tests.Where(t => t.Module!.CourseId == courseId || (t.CourseId == courseId)).ExecuteDeleteAsync();
        await dbContext.PracticalSubmissions.Where(s => s.Task!.Module!.CourseId == courseId).ExecuteDeleteAsync();
        await dbContext.PracticalTasks.Where(t => t.Module!.CourseId == courseId).ExecuteDeleteAsync();
        await dbContext.Modules.Where(m => m.CourseId == courseId).ExecuteDeleteAsync();
    }

    private static CourseModule BuildModule(
        int order,
        string title,
        string description,
        IEnumerable<string> materials,
        IEnumerable<QuestionSeed> questions,
        string practicalText)
    {
        var module = new CourseModule
        {
            Order = order,
            Title = $"Модуль {order}: {title}",
            Description = description,
            PracticalTask = new PracticalTask
            {
                Title = $"Практичне завдання модуля {order}",
                Description = practicalText
            },
            Test = BuildTest($"Тест модуля {order}", questions)
        };

        var index = 1;
        foreach (var material in materials)
        {
            module.Materials.Add(new Material
            {
                Title = $"Матеріал {order}.{index}",
                Content = material
            });
            index++;
        }

        return module;
    }

    private static Test BuildFinalTest(string title, IEnumerable<QuestionSeed> questions)
    {
        var test = BuildTest(title, questions);
        test.IsFinal = true;
        return test;
    }

    private static Test BuildTest(string title, IEnumerable<QuestionSeed> questions)
    {
        var test = new Test { Title = title };
        foreach (var question in questions)
        {
            var entity = new Question { Text = question.Text };
            foreach (var option in question.Options)
            {
                entity.AnswerOptions.Add(new AnswerOption
                {
                    Text = option.Text,
                    IsCorrect = option.IsCorrect
                });
            }
            test.Questions.Add(entity);
        }

        return test;
    }

    private static IEnumerable<QuestionSeed> BuildPlaceholderQuestions(string prefix) =>
    [
        Q($"{prefix}: question 1", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q($"{prefix}: question 2", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q($"{prefix}: question 3", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q($"{prefix}: question 4", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q($"{prefix}: question 5", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false))
    ];

    private static IEnumerable<QuestionSeed> BuildFinalPlaceholderQuestions() =>
    [
        Q("Final question 1", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 2", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 3", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 4", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 5", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 6", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 7", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 8", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 9", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false)),
        Q("Final question 10", ("Option 1", true), ("Option 2", false), ("Option 3", false), ("Option 4", false))
    ];

    private static QuestionSeed Q(string text, params (string Text, bool IsCorrect)[] options) =>
        new(text, options.Select(o => new OptionSeed(o.Text, o.IsCorrect)).ToList());

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string username,
        string firstName,
        string lastName,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email)
                   ?? await userManager.FindByNameAsync(username);
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
        else
        {
            if (user.Email != email)
            {
                var emailResult = await userManager.SetEmailAsync(user, email);
                if (!emailResult.Succeeded)
                {
                    var errors = string.Join("; ", emailResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Unable to update email for {email}: {errors}");
                }
            }

            if (user.UserName != username)
            {
                var nameResult = await userManager.SetUserNameAsync(user, username);
                if (!nameResult.Succeeded)
                {
                    var errors = string.Join("; ", nameResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Unable to update username for {email}: {errors}");
                }
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to update user {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private sealed record OptionSeed(string Text, bool IsCorrect);
    private sealed record QuestionSeed(string Text, List<OptionSeed> Options);
}

