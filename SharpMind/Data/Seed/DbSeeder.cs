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
        var devOpsMentor = await EnsureUserAsync(
            userManager,
            "devops_mentor@courses.com",
            "devops_mentor",
            "DevOps",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var javaMentor = await EnsureUserAsync(
            userManager,
            "java_mentor@courses.com",
            "java_mentor",
            "Java",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var frontendMentor = await EnsureUserAsync(
            userManager,
            "frontend_mentor@courses.com",
            "frontend_mentor",
            "Frontend",
            "Mentor",
            "Mentor123!",
            AppRoles.Mentor);

        var devOps = await EnsureCourseShellAsync(
            dbContext,
            devOpsMentor.Id,
            ["DevOps з нуля", "DevOps z nulia", "DevOps from zero to pro"],
            "DevOps from zero to pro",
            "CI/CD, containers, cloud basics and monitoring from scratch.",
            CourseTopic.DevOps);
        await RebuildSimpleCourseContentAsync(dbContext, devOps);

        var csharp = await EnsureCourseShellAsync(
            dbContext,
            mentors[0].Id,
            ["C# Advanced"],
            "C# Advanced",
            "Advanced patterns, async, performance and architecture in C#.",
            CourseTopic.Programming);
        await RebuildCSharpAdvancedAsync(dbContext, csharp);

        var jsFullStack = await EnsureCourseShellAsync(
            dbContext,
            mentors[1].Id,
            ["JavaScript Full-Stack"],
            "JavaScript Full-Stack",
            "Modern JavaScript from backend services to interactive frontend.",
            CourseTopic.WebDevelopment);
        await RebuildJavaScriptFullStackAsync(dbContext, jsFullStack);

        var python = await EnsureCourseShellAsync(
            dbContext,
            mentors[2].Id,
            ["Python for Data Science"],
            "Python for Data Science",
            "Data wrangling, visualization and ML essentials with Python.",
            CourseTopic.DataScience);
        await RebuildPythonForDataScienceAsync(dbContext, python);

        var java = await EnsureCourseShellAsync(
            dbContext,
            javaMentor.Id,
            ["Java Fundamentals"],
            "Java Fundamentals",
            "Базовий курс Java: синтаксис, типи даних, ООП, масиви, умови та цикли.",
            CourseTopic.Programming);
        await RebuildJavaFundamentalsAsync(dbContext, java);

        var frontend = await EnsureCourseShellAsync(
            dbContext,
            frontendMentor.Id,
            ["Frontend Fundamentals"],
            "Frontend Fundamentals",
            "Базовий курс Frontend: HTML5, CSS3, Flexbox/Grid, JavaScript та DOM.",
            CourseTopic.WebDevelopment);
        await RebuildFrontendFundamentalsAsync(dbContext, frontend);

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Course> EnsureCourseShellAsync(
        ApplicationDbContext dbContext,
        string mentorId,
        IEnumerable<string> aliases,
        string title,
        string description,
        CourseTopic topic)
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
                Level = CourseLevel.Pochatkovyi,
                Price = 0,
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
            course.Level = CourseLevel.Pochatkovyi;
            course.Price = 0;
            course.MentorId = mentorId;
            course.IsPublished = true;
        }

        return course;
    }

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

    private sealed record OptionSeed(string Text, bool IsCorrect);
    private sealed record QuestionSeed(string Text, List<OptionSeed> Options);
}

