namespace WorkforceManager.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Nager.Date;

    public static class HolidaysService
    {
        public static async Task<int> GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            var year = DateTime.Now.Year;
            var currentYearHolidays = await GetHolidaysByYear(year);
            var daysOffBetween = 0;

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                if (currentYearHolidays.Any(x => x.Date == date.Date) || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    daysOffBetween += 1;
                }
            }

            var paidDaysUsed = (endDate.Date - startDate.Date).Days - daysOffBetween;

            return await Task.Run(() => paidDaysUsed);
        }

        private static async Task<ICollection<DateTime>> GetHolidaysByYear(int year)
        {
            var publicHolidaysInBulgaria = DateSystem.GetPublicHolidays(year, "BG").Select(d => d.Date.Date);
            var publicHolidaysInMacedonia = DateSystem.GetPublicHolidays(year, "MK").Select(d => d.Date.Date);
            var allHolidaysDates = publicHolidaysInBulgaria.Union(publicHolidaysInMacedonia).ToList();

            return await Task.Run(() => allHolidaysDates);
        }
    }
}
