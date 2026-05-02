using SharpMind.Models;

namespace SharpMind.Services;

public class CertificateService
{
    public string GenerateCertificateContent(Certificate certificate)
    {
        var content = $@"
═══════════════════════════════════════════════════════════════════════════════
                         СЕРТИФІКАТ ПРО ЗАВЕРШЕННЯ КУРСУ
═══════════════════════════════════════════════════════════════════════════════

                            Платформа: {certificate.PlatformName}
                     Номер сертифіката: {certificate.UniqueNumber}

Цим підтверджується, що

                              {certificate.FullName}

успішно завершив(ла) курс:

                           {certificate.CourseName}

Ментор курсу: {certificate.MentorName ?? "Невідомо"}
Результат фінального тесту: {certificate.FinalTestScore:F1}%
Дата видачі: {certificate.IssueDate:dd MMMM yyyy} року

═══════════════════════════════════════════════════════════════════════════════
Цей сертифікат видано як свідчення успішного завершення навчальної програми.
═══════════════════════════════════════════════════════════════════════════════
";
        return content;
    }
}

