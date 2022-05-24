using System.Net;

namespace AssignmentIndigoLabs.Models
{
    public class CovidData
    {
        public Dictionary<DateOnly, Dictionary<string, int>>? AllData;

        private async Task GetCSVAsync(string url)
        {
            var myClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            var response = await myClient.GetAsync(url);
            var streamResponse = await response.Content.ReadAsStreamAsync();

            StreamReader sr = new(streamResponse);
            string? headers = sr.ReadLine();
            if (headers != null)
            {
                var header = headers.Split(",");
                Dictionary<DateOnly, Dictionary<string, int>> data = new();

                while (!sr.EndOfStream)
                {
                    Dictionary<string, int> subData = new();
                    string? results = sr.ReadLine();
                    if (results != null)
                    {
                        var result = results.Split(",");
                        for (int i = 1; i < result.Length; i++)
                        {
                            if (result[i] == "")
                                subData.Add(header[i], 0);
                            else
                                subData.Add(header[i], int.Parse(result[i]));
                        }
                        data.Add(DateOnly.ParseExact(result[0].Replace("-", ""), "yyyyMMdd"), subData);
                    }
                }
                sr.Close();

                AllData = data;
            }
            else
                throw new Exception("URL to covid data not working or corrupted data");
        }
        public async Task FillDataAsync() 
        {
            AllData = new();
            await GetCSVAsync("https://raw.githubusercontent.com/sledilnik/data/master/csv/region-cases.csv");
        }
    }
}
