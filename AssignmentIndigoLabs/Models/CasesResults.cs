namespace AssignmentIndigoLabs.Models
{
    public class CasesResults
    {
        public string Date { get; set; }
        public string? Region { get; set; }
        public int ActiveCasesPerDay { get; set; }
        public int NumberOfVaccinatedFirst { get; set; }
        public int NumberOfVaccinatedSecond { get; set; }
        public int DeceasedToDate { get; set; }

        public CasesResults(string date, string? region, int activeCasesPerDay, int numberOfVaccinatedFirst, int numberOfVaccinatedSecond, int deceasedToDate)
        {
            Date = date;
            Region = region?.ToUpper();
            ActiveCasesPerDay = activeCasesPerDay;
            NumberOfVaccinatedFirst = numberOfVaccinatedFirst;
            NumberOfVaccinatedSecond = numberOfVaccinatedSecond;
            DeceasedToDate = deceasedToDate;
        }
    }
}
