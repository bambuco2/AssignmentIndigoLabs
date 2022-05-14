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

        private static bool CheckDay(int day) { return day<=31&&day>=1; }
        private static bool CheckMonth(int month) { return month<=12&&month>=1; } 
        private static bool CheckYear(int year) { return year<=2022&&year>=2020; }
        private static bool CheckDate(int date) 
        {
            if (date == 0)
                return true;
            string currentDate = date.ToString();
            if ((currentDate.Length == 8 && CheckDay(int.Parse(""+ currentDate[6]+ currentDate[7])) && 
                CheckMonth(int.Parse(""+ currentDate[4]+ currentDate[5])) && CheckYear(int.Parse(""+ currentDate[0]+ currentDate[1]+ currentDate[2]+ currentDate[3]))) 
                )
            {
                return true;
            }
            return false;
        }

        private static void CheckApiAuthentication(IHeaderDictionary? header) 
        {
            if (header == null || !header.ContainsKey("AuthenticationPassword") || !header["AuthenticationPassword"].Equals("IndigoLabs"))
                throw new Exception("API Authentication failed");
        }

        private bool CheckValues(string? Region, int From, int To) 
        {
            if((Region == null || regionList.Contains(Region.ToUpper())) && CheckDate(From) && CheckDate(To) && CompareDates(To, From))
                return true;
            return false;
        }

        private static List<string> GetLastWeekDates() 
        {
            List<string> week = new();
            DateTime today = DateTime.Today;
            DateTime startingDay = DateTime.Today;
            startingDay = startingDay.AddDays(-1);
            week.Add(startingDay.ToString("yyyy-MM-dd"));

            while (startingDay.DayOfWeek != today.DayOfWeek)
            {
                startingDay = startingDay.AddDays(-1);
                week.Add(startingDay.ToString("yyyy-MM-dd"));
            }
            return week;
        }

        private static Dictionary<string, Dictionary<string, int>> GetLastWeekData(Dictionary<string, Dictionary<string, int>> allData) 
        {
            Dictionary<string, Dictionary<string, int>> data = new();
            var week = GetLastWeekDates();
            foreach(var day in week) 
            {
                if(allData.ContainsKey(day))
                    data[day] = allData[day];
            }

            return data;
        }

        private static string FindFirstDay(List<string> week, Dictionary<string, Dictionary<string, int>> lastWeekData) 
        {
            foreach(var day in week) 
            {
                if(lastWeekData.ContainsKey(day))
                    return day;
            }
            throw new Exception("Missing data for the past week");
        }
        private static string FindLastDay(List<string> week, Dictionary<string, Dictionary<string, int>> lastWeekData) 
        {
            week.Reverse();
            foreach (var day in week)
            {
                if (lastWeekData.ContainsKey(day))
                    return day;
            }
            throw new Exception("Missing data for the past week");
        }
        private List<LastWeekResults> FormatLastWeekData(Dictionary<string, Dictionary<string, int>>  lastWeekData) 
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

        private static bool CompareDates(int date1, string date2) 
        {
            var firstDate = DateOnly.ParseExact(date1.ToString(), "yyyyMMdd");
            var secondDate = DateOnly.Parse(date2);
            if(firstDate >= secondDate) 
                return true;
            return false;
        }
        private static bool CompareDates(string date1, int date2) 
        {
            var firstDate = DateOnly.Parse(date1);
            var secondDate = DateOnly.ParseExact(date2.ToString(), "yyyyMMdd");
            if (firstDate >= secondDate)
                return true;
            return false;
        }
        private static bool CompareDates(int date1, int date2) 
        {
            if (date1 == 0 || date2 == 0)
                return true;
            var firstDate = DateOnly.ParseExact(date1.ToString(), "yyyyMMdd");
            var secondDate = DateOnly.ParseExact(date2.ToString(), "yyyyMMdd");
            if (firstDate >= secondDate)
                return true;
            return false;
        }

        private List<CasesResults> GetCaseResultData(int From, int To) 
        {
            if (covidData?.allData != null)
            {
                List<CasesResults> casesResultsList = new();
                foreach (var key in covidData.allData.Keys)
                {
                    if (From != 0 && !CompareDates(key, From))
                        continue;
                    else if (To != 0 && !CompareDates(To, key))
                        continue;
                    foreach (var region in regionList)
                    {
                        CasesResults caseResult = new(key, region, covidData.allData[key]["region." + region.ToLower() + ".cases.active"], covidData.allData[key]["region." + region.ToLower() + ".vaccinated.1st.todate"],
                             covidData.allData[key]["region." + region.ToLower() + ".vaccinated.2nd.todate"], covidData.allData[key]["region." + region.ToLower() + ".deceased.todate"]);
                        casesResultsList.Add(caseResult);
                    }
                }

                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }
        private List<CasesResults> GetCaseResultData(string Region, int From, int To)
        {
            if (covidData?.allData != null)
            {
                List<CasesResults> casesResultsList = new();
                foreach (var key in covidData.allData.Keys)
                {
                    if (From != 0 && !CompareDates(key, From))
                        continue;
                    else if (To != 0 && !CompareDates(To, key))
                        continue;
                    CasesResults caseResult = new(key, Region, covidData.allData[key]["region." + Region.ToLower() + ".cases.active"], covidData.allData[key]["region." + Region.ToLower() + ".vaccinated.1st.todate"],
                             covidData.allData[key]["region." + Region.ToLower() + ".vaccinated.2nd.todate"], covidData.allData[key]["region." + Region.ToLower() + ".deceased.todate"]);
                    casesResultsList.Add(caseResult);
                }

                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }

        private List<CasesResults> FormatCasesResults(string? Region, int From, int To) 
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

            if (covidData.allData == null)
                Task.Run(() => covidData.FillDataAsync()).Wait();
            if (covidData?.allData != null)
            {
                if (CheckValues(Region, From, To))
                {
                    List<CasesResults> results = FormatCasesResults(Region, From, To);
                    return results;
                }
                else
                {
                    throw new Exception("Wrong parameters");
                }
            }
            else
                throw new Exception("Missing Covid data");
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
