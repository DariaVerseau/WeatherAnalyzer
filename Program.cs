using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherAnalyzer
{
    public class WeatherData
    {
        public string City { get; set; } = "";
        public double Temperature { get; set; }
        public string Country { get; set; } = "";
    }

    public class WttrResponse
    {
        [JsonPropertyName("current_condition")]
        public List<CurrentCondition>? CurrentCondition { get; set; }

        [JsonPropertyName("nearest_area")]
        public List<NearestArea>? NearestArea { get; set; }
    }

    public class CurrentCondition
    {
        [JsonPropertyName("temp_C")]
        public string? TempC { get; set; }
    }

    public class NearestArea
    {
        [JsonPropertyName("country")]
        public List<AreaValue>? Country { get; set; }
    }

    public class AreaValue
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    class Program
    {
        private static readonly HttpClient client = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            var http = new HttpClient(handler);
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            return http;
        }

        static async Task Main()
        {
            var cities = LoadCities("Cities.txt");
            if (cities.Count == 0)
            {
                Console.WriteLine("[ERROR] No cities loaded.");
                return;
            }

            var results = new List<WeatherData>();
            foreach (var city in cities)
            {
                var w = await GetWeatherAsync(city);
                if (w != null)
                    results.Add(w);
            }

            if (results.Count == 0)
            {
                Console.WriteLine("No data received.");
                return;
            }

            PrintPerCity(results);
            PrintByCountry(results);
        }

        static List<string> LoadCities(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[ERROR] File not found: {path}");
                return new List<string>();
            }

            return File.ReadAllLines(path)
                .Select(l => l.Trim().TrimStart('\uFEFF'))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();
        }

        static async Task<WeatherData?> GetWeatherAsync(string city)
        {
            try
            {
                string encoded = Uri.EscapeDataString(city);
                string url = $"https://wttr.in/{encoded}?format=j1";

                string json = await client.GetStringAsync(url);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<WttrResponse>(json, options);

                if (data?.CurrentCondition == null || data.CurrentCondition.Count == 0)
                    return null;

                if (!double.TryParse(data.CurrentCondition[0].TempC, out double temp))
                    return null;

                string country = "Unknown";
                if (data.NearestArea != null && data.NearestArea.Count > 0 &&
                    data.NearestArea[0].Country != null && data.NearestArea[0].Country!.Count > 0)
                {
                    country = data.NearestArea[0].Country![0].Value ?? "Unknown";
                }

                return new WeatherData
                {
                    City = city,
                    Temperature = temp,
                    Country = country
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] {city}: {e.Message}");
                return null;
            }
        }

        static void PrintPerCity(List<WeatherData> data)
        {
            Console.WriteLine("--- Weather by City ---");
            foreach (var w in data)
            {
                string sign = w.Temperature >= 0 ? "+" : "";
                Console.WriteLine($"{w.City}, {w.Country} {sign}{Math.Round(w.Temperature)} °C");
            }
        }

        static void PrintByCountry(List<WeatherData> data)
        {
            Console.WriteLine("\n--- Statistics by Country ---");
            var grouped = data.GroupBy(d => d.Country);

            foreach (var group in grouped)
            {
                var temps = group.Select(d => d.Temperature).ToList();
                int count = temps.Count;
                double avg = Math.Round(temps.Average());
                double mn = Math.Round(temps.Min());
                double mx = Math.Round(temps.Max());

                string avgS = avg >= 0 ? "+" : "";
                string mnS = mn >= 0 ? "+" : "";
                string mxS = mx >= 0 ? "+" : "";

                Console.WriteLine($"{group.Key} - {count} cities, avg: {avgS}{avg} °C, min: {mnS}{mn} °C, max: {mxS}{mx} °C");
            }
        }
    }
}