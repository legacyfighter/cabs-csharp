using NodaTime;

namespace LegacyFighter.Cabs.Service;

public static class MonthOfYearExtensions
{
  public static LocalDate AtEndOfMonth(this YearMonth yearMonth)
  {
    var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(yearMonth.Year, yearMonth.Month);
    var mstLastDayOfCurrentMonth = new LocalDate(
      yearMonth.Year, yearMonth.Month, daysInMonth);
    return mstLastDayOfCurrentMonth;
  }
}