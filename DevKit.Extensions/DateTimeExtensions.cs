namespace DevKit.Extensions;

/// <summary>Proporciona métodos de extensión para trabajar con fechas y horas.</summary>
public static class DateTimeExtensions
{
    /// <summary>Calcula la diferencia entre dos fechas en la unidad especificada.</summary>
    /// <param name="startDate">La fecha de inicio.</param>
    /// <param name="interval">La unidad de tiempo (año, mes, día, etc.).</param>
    /// <param name="endDate">La fecha de fin.</param>
    /// <returns>La diferencia en la unidad especificada.</returns>
    public static int DateDiff(this DateTime startDate, DateInterval interval, DateTime endDate)
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
    /// <param name="date">La fecha de referencia.</param>
    /// <returns>Una nueva instancia de <see cref="DateTime"/> que representa el último día del mes.</returns>
    public static DateTime EndOfMonth(this DateTime date) =>
        new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    /// <summary>Determina si una cadena representa una fecha válida.</summary>
    /// <param name="input">La cadena a verificar.</param>
    /// <returns>Verdadero si es una fecha válida; de lo contrario, falso.</returns>
    public static bool IsValidDate(this string input) => DateTime.TryParse(input, out _);

    /// <summary>Intenta convertir la representación de cadena de una fecha a su equivalente DateTime.</summary>
    /// <param name="input">La cadena a convertir.</param>
    /// <param name="formats">Formatos opcionales a intentar.</param>
    /// <returns>El <see cref="DateTime"/> resultante o null si no se pudo convertir.</returns>
    public static DateTime? ParseDate(this string input, string[] formats = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

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

        return null;
    }

    /// <summary>Obtiene el número de semana ISO 8601 para la fecha especificada.</summary>
    /// <param name="date">La fecha de referencia.</param>
    /// <returns>El número de semana ISO.</returns>
    public static int GetIsoWeek(this DateTime date) =>
        CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

    /// <summary>Convierte una fecha a un DateTime con el desplazamiento UTC especificado.</summary>
    /// <param name="date">La fecha de referencia.</param>
    /// <param name="offsetMinutes">El desplazamiento en minutos.</param>
    /// <returns>La fecha convertida a UTC con el desplazamiento aplicado.</returns>
    public static DateTime ToDateOffset(this DateTime date, int offsetMinutes) =>
        new DateTimeOffset(date, TimeSpan.FromMinutes(offsetMinutes)).UtcDateTime;

    /// <summary>Crea una nueva instancia de DateTime a partir de los componentes de fecha especificados.</summary>
    /// <param name="year">Año.</param>
    /// <param name="month">Mes.</param>
    /// <param name="day">Día.</param>
    /// <returns>Una nueva instancia de <see cref="DateTime"/>.</returns>
    public static DateTime DateFromParts(int year, int month, int day) => new DateTime(year, month, day);

    /// <summary>Obtiene el nombre del día de la semana para la fecha especificada.</summary>
    /// <param name="date">La fecha de referencia.</param>
    /// <returns>El nombre del día de la semana.</returns>
    public static string GetDayName(this DateTime date) => date.ToString("dddd", CultureInfo.CurrentCulture);

    /// <summary>Obtiene el nombre del mes para la fecha especificada.</summary>
    /// <param name="date">La fecha de referencia.</param>
    /// <returns>El nombre del mes.</returns>
    public static string GetMonthName(this DateTime date) => date.ToString("MMMM", CultureInfo.CurrentCulture);
}