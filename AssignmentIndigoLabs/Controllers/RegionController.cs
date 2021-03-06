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
        private readonly List<string> _regionList = new(){ "LJ", "CE", "KR", "NM", "KK", "KP", "MB", "MS", "NG", "PO", "SG", "ZA" };
        public CovidData _covidData = new();

        private DateOnly ConvertToDateOnly(int date) 
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
        private bool CompareDates(DateOnly firstDate, DateOnly secondDate)
        {
            if (firstDate >= secondDate)
                return true;
            return false;
        }
        private bool CheckValues(string? region, int from, int to)
        {
            if (from != 0 && to != 0 && to < from)
                return false;
            if (region != null && !_regionList.Contains(region.ToUpper()))
                return false;
            return true;
        }

        private Boolean CheckApiAuthentication(IHeaderDictionary? header) 
        {
            if (header == null || !header.ContainsKey("AuthenticationPassword") || !header["AuthenticationPassword"].Equals("IndigoLabs"))
                return false;
            return true;
        }

        private DateOnly FindFirstDay(List<DateOnly> week, Dictionary<DateOnly, Dictionary<string, int>> lastWeekData)
        {
            foreach (var day in week)
            {
                if (lastWeekData.ContainsKey(day))
                    return day;
            }
            throw new Exception("Missing data for the past week");
        }
        private DateOnly FindLastDay(List<DateOnly> week, Dictionary<DateOnly, Dictionary<string, int>> lastWeekData)
        {
            week.Reverse();
            foreach (var day in week)
            {
                if (lastWeekData.ContainsKey(day))
                    return day;
            }
            throw new Exception("Missing data for the past week");
        }
        private List<DateOnly> GetLastWeekDates() 
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
        private Dictionary<DateOnly, Dictionary<string, int>> GetLastWeekData(Dictionary<DateOnly, Dictionary<string, int>> allData) 
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
            foreach (var region in _regionList)
            {
                var activeCasesEnd = lastWeekData[firstDay]["region."+region.ToLower()+".cases.confirmed.todate"];
                var activeCasesStart = lastWeekData[lastDay]["region." + region.ToLower() + ".cases.confirmed.todate"];

                var avgActiveCasesPerDay = (activeCasesEnd - activeCasesStart) / 7;
                LastWeekResults today = new(region, avgActiveCasesPerDay);
                lastWeekResultsList.Add(today);
            }
            return lastWeekResultsList.OrderByDescending(t => t.AvgCases).ToList();
        }

        private List<CasesResults> GetCaseResultData(DateOnly from, DateOnly to) 
        {
            DateOnly checkUp;
            if (_covidData?.AllData != null)
            {
                List<CasesResults> casesResultsList = new();
                foreach (var key in _covidData.AllData.Keys)
                {
                    if (!from.Equals(checkUp) && !CompareDates(key, from))
                        continue;
                    else if (!to.Equals(checkUp) && !CompareDates(to, key))
                        continue;
                    foreach (var region in _regionList)
                    {
                        CasesResults caseResult = new(key.ToString("yyyy-MM-dd"), region, _covidData.AllData[key]["region." + region.ToLower() + ".cases.active"], _covidData.AllData[key]["region." + region.ToLower() + ".vaccinated.1st.todate"],
                             _covidData.AllData[key]["region." + region.ToLower() + ".vaccinated.2nd.todate"], _covidData.AllData[key]["region." + region.ToLower() + ".deceased.todate"]);
                        casesResultsList.Add(caseResult);
                    }
                }

                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }
        private List<CasesResults> GetCaseResultData(string region, DateOnly from, DateOnly to)
        {
            if (_covidData?.AllData != null)
            {
                DateOnly checkUp;
                List<CasesResults> casesResultsList = new();
                foreach (var key in _covidData.AllData.Keys)
                {
                    if (!from.Equals(checkUp) && !CompareDates(key, from))
                        continue;
                    else if (!to.Equals(checkUp) && !CompareDates(to, key))
                        continue;
                    CasesResults caseResult = new(key.ToString("yyyy-MM-dd"), region, _covidData.AllData[key]["region." + region.ToLower() + ".cases.active"], _covidData.AllData[key]["region." + region.ToLower() + ".vaccinated.1st.todate"],
                             _covidData.AllData[key]["region." + region.ToLower() + ".vaccinated.2nd.todate"], _covidData.AllData[key]["region." + region.ToLower() + ".deceased.todate"]);
                    casesResultsList.Add(caseResult);
                }

                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }
        private List<CasesResults> FormatCasesResults(string? region, DateOnly from, DateOnly to) 
        {
            if (_covidData?.AllData != null) 
            {
                List<CasesResults> casesResultsList;
                if (region == null)
                {
                    casesResultsList = GetCaseResultData(from, to);
                }
                else
                {
                    casesResultsList = GetCaseResultData(region, from, to);
                }
                return casesResultsList;
            }
            else
                throw new Exception("Missing Covid data");
        }


        [HttpGet("cases")]
        public ActionResult<List<CasesResults>> Get(string? region, int from, int to)
        {
            var request = Request;
            var headers = request.Headers;
            if (!CheckApiAuthentication(headers))
                return Unauthorized();
            if (CheckValues(region, from, to))
            {

                if (_covidData.AllData == null)
                    Task.Run(() => _covidData.FillDataAsync()).Wait();
                if (_covidData?.AllData != null)
                {
                    DateOnly fromDate = ConvertToDateOnly(from);
                    DateOnly toDate = ConvertToDateOnly(to);

                    List<CasesResults> results = FormatCasesResults(region, fromDate, toDate);
                    return results;

                }
                else
                    return StatusCode(500);
            }
            else
                return BadRequest();
        }

        [HttpGet("lastweek")]
        public ActionResult<List<LastWeekResults>> GetLastWeek()
        {
            var request = Request;
            var headers = request.Headers;
            if (!CheckApiAuthentication(headers))
                return Unauthorized();

            if (_covidData.AllData == null)
                Task.Run(() => _covidData.FillDataAsync()).Wait();
            if (_covidData.AllData != null)
            {
                var lastWeekData = GetLastWeekData(_covidData.AllData);
                var formatedWeekResults = FormatLastWeekData(lastWeekData);
                return formatedWeekResults;
            }
            else
                return StatusCode(500);

        }

        
    }
}
