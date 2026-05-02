namespace SharpMind.Services;

public class PaymentService
{
    public (bool isValid, string message) ValidatePayment(string cardNumber, string expiryDate, string cvv)
    {
        // Валідація номера картки (16 цифр, починається з 41 для Visa)
        if (!System.Text.RegularExpressions.Regex.IsMatch(cardNumber, @"^\d{16}$"))
        {
            return (false, "Номер картки має містити 16 цифр");
        }

        if (!cardNumber.StartsWith("41"))
        {
            return (false, "Номер картки має починатися з 41 (Visa)");
        }

        // Валідація терміну дії
        var parts = expiryDate.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year))
        {
            return (false, "Невірний формат терміну дії (МM/РР)");
        }

        if (month < 1 || month > 12)
        {
            return (false, "Місяць повинен бути від 01 до 12");
        }

        var currentYear = DateTime.Now.Year % 100;
        var currentMonth = DateTime.Now.Month;

        // Якщо рік поточний, перевіряємо місяць
        if (year == currentYear && month < currentMonth)
        {
            return (false, "Карта закінчилась");
        }

        if (year < currentYear)
        {
            return (false, "Карта закінчилась");
        }

        // Валідація CVV
        if (!System.Text.RegularExpressions.Regex.IsMatch(cvv, @"^\d{3}$"))
        {
            return (false, "CVV має містити 3 цифри");
        }

        return (true, "Платіж успішний");
    }
}

