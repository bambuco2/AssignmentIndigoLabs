using AssignmentIndigoLabs.Classes;
using AssignmentIndigoLabs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AssignmentIndigoLabs.Controllers
{
    [Route("api/region/")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly List<string> regionList = new(){ "LJ", "CE", "KR", "NM", "KK", "KP", "MB", "MS", "NG", "PO", "SG", "ZA" };
        public CovidData covidData = new();

        private static DateOnly ConvertToDateOnly(int date) 
        {
            DateOnly parsedDate;
            if (date != 0)
            {
                try
                {
                    parsedDate = DateOnly.ParseExact(date.ToString(), "yyyyMMdd");
                }
                catch
                {
                    throw new Exception("Wrong date parameter");
                }
            }
            return parsedDate;
        }
        private static bool CompareDates(DateOnly firstDate, DateOnly secondDate)
        {
            if (firstDate >= secondDate)
                return true;
            return false;
        }
        private bool CheckValues(string? Region, int From, int To)
        {
            if (From != 0 && To != 0 && To < From)
                return false;
            if (Region != null && !regionList.Contains(Region.ToUpper()))
                return false;
            return true;
        }

        private static void CheckApiAuthentication(IHeaderDictionary? header) 
        {
            if (header == null || !header.ContainsKey("AuthenticationPassword") || !header["AuthenticationPassword"].Equals("IndigoLabs"))
                throw new Exception("API Authentication failed");
        }

        private static DateOnly FindFirstDay(List<DateOnly> week, Dictionary<DateOnly, Dictionary<string, int>> lastWeekData)
        {
            foreach (var day in week)
            {
                if (lastWeekData.ContainsKey(day))
                    return day;
            }
            throw new Exception("Missing data for the past week");
        }
        private static DateOnly FindLastDay(List<DateOnly> week, Dictionary<DateOnly, Dictionary<string, int>> lastWeekData)
        {
            week.Reverse();
            foreach (var day in week)
            {
                if (lastWeekData.ContainsKey(day))
                    return day;
            }
            throw new Exception("Missing data for the past week");
        }
        private static List<DateOnly> GetLastWeekDates() 
        {
            List<DateOnly> week = new();
            DateTime today = DateTime.Today;
            DateTime startingDay = DateTime.Today;
            startingDay = startingDay.AddDays(-1);
            week.Add(DateOnly.FromDateTime(startingDay));

            while (startingDay.DayOfWeek != today.DayOfWeek)
            {
                startingDay = startingDay.AddDays(-1);
                week.Add(DateOnly.FromDateTime(startingDay));
            }
            return week;
        }
        private static Dictionary<DateOnly, Dictionary<string, int>> GetLastWeekData(Dictionary<DateOnly, Dictionary<string, int>> allData) 
        {
            Dictionary<DateOnly, Dictionary<string, int>> data = new();
            var week = GetLastWeekDates();
            foreach(var day in week) 
            {
                if(allData.ContainsKey(day))
                    data[day] = allData[day];
            }

            return data;
        }
        private List<LastWeekResults> FormatLastWeekData(Dictionary<DateOnly, Dictionary<string, int>>  lastWeekData) 
        {
            List<LastWeekResults> lastWeekResultsList = new();
            var week = GetLastWeekDates();
            var firstDay = FindFirstDay(week, lastWeekData);
            var lastDay = FindLastDay(week, lastWeekData);
            foreach (var region in regionList)
            {
                var activeCasesEnd = lastWeekData[firstDay]["region."+region.ToLower()+".cases.confirmed.todate"];
                var activeCasesStart = lastWeekData[lastDay]["region." + region.ToLower() + ".cases.confirmed.todate"];

                var avgActiveCasesPerDay = (activeCasesEnd - activeCasesStart) / 7;
                LastWeekResults today = new(region, avgActiveCasesPerDay);
                lastWeekResultsList.Add(today);
            }
            return lastWeekResultsList.OrderByDescending(t => t.AvgCases).ToList();
        }

        private List<CasesResults> GetCaseResultData(DateOnly From, DateOnly To) 
        {
            DateOnly checkUp;
            if (covidData?.allData != null)
            {
                List<CasesResults> casesResultsList = new();
                foreach (var key in covidData.allData.Keys)
                {
                    if (!From.Equals(checkUp) && !CompareDates(key, From))
                        continue;
                    else if (!To.Equals(checkUp) && !CompareDates(To, key))
                        continue;
                    foreach (var region in regionList)
                    {
                        CasesResults caseResult = new(key.ToString("yyyy-MM-dd"), region, covidData.allData[key]["region." + region.ToLower() + ".cases.active"], covidData.allData[key]["region." + region.ToLower() + ".vaccinated.1st.todate"],
                             covidData.allData[key]["region." + region.ToLower() + ".vaccinated.2nd.todate"], covidData.allData[key]["region." + region.ToLower() + ".deceased.todate"]);
                        casesResultsList.Add(caseResult);
                    }
                }

                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }
        private List<CasesResults> GetCaseResultData(string Region, DateOnly From, DateOnly To)
        {
            if (covidData?.allData != null)
            {
                DateOnly checkUp;
                List<CasesResults> casesResultsList = new();
                foreach (var key in covidData.allData.Keys)
                {
                    if (!From.Equals(checkUp) && !CompareDates(key, From))
                        continue;
                    else if (!To.Equals(checkUp) && !CompareDates(To, key))
                        continue;
                    CasesResults caseResult = new(key.ToString("yyyy-MM-dd"), Region, covidData.allData[key]["region." + Region.ToLower() + ".cases.active"], covidData.allData[key]["region." + Region.ToLower() + ".vaccinated.1st.todate"],
                             covidData.allData[key]["region." + Region.ToLower() + ".vaccinated.2nd.todate"], covidData.allData[key]["region." + Region.ToLower() + ".deceased.todate"]);
                    casesResultsList.Add(caseResult);
                }

                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }
        private List<CasesResults> FormatCasesResults(string? Region, DateOnly From, DateOnly To) 
        {
            if (covidData?.allData != null) 
            {
                List<CasesResults> casesResultsList;
                if (Region == null)
                {
                    casesResultsList = GetCaseResultData(From, To);
                }
                else
                {
                    casesResultsList = GetCaseResultData(Region, From, To);
                }
                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }


        [HttpGet("cases")]
        public List<CasesResults> Get(string? Region, int From=0, int To=0)
        {
            var request = Request;
            var headers = request.Headers;
            CheckApiAuthentication(headers);
            if (CheckValues(Region, From, To))
            {

                if (covidData.allData == null)
                    Task.Run(() => covidData.FillDataAsync()).Wait();
                if (covidData?.allData != null)
                {
                    DateOnly fromDate = ConvertToDateOnly(From);
                    DateOnly toDate = ConvertToDateOnly(To);

                    List<CasesResults> results = FormatCasesResults(Region, fromDate, toDate);
                    return results;

                }
                else
                    throw new Exception("Missing Covid data");
            }
            else
                throw new Exception("Wrong date or region parameter");
        }

        [HttpGet("lastweek")]
        public List<LastWeekResults> GetLastWeek()
        {
            var request = Request;
            var headers = request.Headers;
            CheckApiAuthentication(headers);

            if (covidData.allData == null)
                Task.Run(() => covidData.FillDataAsync()).Wait();
            if (covidData.allData != null)
            {
                var lastWeekData = GetLastWeekData(covidData.allData);
                var formatedWeekResults = FormatLastWeekData(lastWeekData);
                return formatedWeekResults;
            }
            else
                throw new Exception("Missing Covid data");

        }
    }
}
