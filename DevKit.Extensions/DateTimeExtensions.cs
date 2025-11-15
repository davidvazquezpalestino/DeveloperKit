namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para trabajar con fechas y horas.</summary>
public static class DateTimeExtensions
{
    /// <summary>Calcula la diferencia entre dos fechas en la unidad especificada.</summary>
    public static int DateDiff(DateInterval interval, DateTime startDate, DateTime endDate)
    {
        switch (interval)
        {
            case DateInterval.Year:
                return endDate.Year - startDate.Year;

            case DateInterval.Month:
                return (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            case DateInterval.Day:
                return (endDate - startDate).Days;

            case DateInterval.Hour:
                return (int)(endDate - startDate).TotalHours;

            case DateInterval.Minute:
                return (int)(endDate - startDate).TotalMinutes;

            case DateInterval.Second:
                return (int)(endDate - startDate).TotalSeconds;

            default:
                return 0;
        }
    }
    /// <summary>Obtiene el último día del mes para la fecha especificada.</summary>
    public static DateTime EndOfMonth(DateTime date) =>
        new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    /// <summary>Determina si una cadena representa una fecha válida.</summary>
    public static bool IsValidDate(string input) => DateTime.TryParse(input, out _);

    /// <summary>Intenta convertir la representación de cadena de una fecha a su equivalente DateTime.</summary>
    public static DateTime? ParseDate(string input, string[] formats = null)
    {
        if (formats == null)
        {
            formats = ["yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyyMMdd"];
        }

        foreach (string format in formats)
        {
            if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
        }

        return null; // o lanzar excepción si prefieres control estricto
    }
    /// <summary>Obtiene el número de semana ISO 8601 para la fecha especificada.</summary>
    public static int GetIsoWeek(DateTime date) =>
        CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

    /// <summary>Convierte una fecha a un DateTime con el desplazamiento UTC especificado.</summary>
    public static DateTime ToDateOffset(DateTime date, int offsetMinutes) =>
        new DateTimeOffset(date, TimeSpan.FromMinutes(offsetMinutes)).UtcDateTime;

    /// <summary>Crea una nueva instancia de DateTime a partir de los componentes de fecha especificados.</summary>
    public static DateTime DateFromParts(int year, int month, int day) => new DateTime(year, month, day);

    /// <summary>Obtiene el nombre del día de la semana para la fecha especificada.</summary>
    public static string GetDayName(DateTime date) => date.ToString("dddd", CultureInfo.CurrentCulture);

    /// <summary>Obtiene el nombre del mes para la fecha especificada.</summary>
    public static string GetMonthName(DateTime date) => date.ToString("MMMM", CultureInfo.CurrentCulture);
}