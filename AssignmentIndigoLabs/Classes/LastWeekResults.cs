namespace AssignmentIndigoLabs.Classes
{
    public class LastWeekResults
    {
        public string Region { get; set; }
        public int AvgCases { get; set; }

        public LastWeekResults(string region, int avgCases)
        {
            Region = region;
            AvgCases = avgCases;
        }
    }
}
