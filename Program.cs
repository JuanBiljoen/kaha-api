//In this task I firstly added  error handling to make it easier to debug the code. I also added comments to make readibility easier. I refactored the GetAllCountries function by removing the if statement, it was unnecessary. The bug  in the Random Country function was the >= 0, so it was searching for countries in the northern hemisphere. I added the sunset/sunrise functionality and I also added the bonus task of calculating the distance between the random country and the Kaha offices by using the Haversine formula + adding the Typewriter style in the console (Google is your friend). Additionally I added a unit test for the API call which is the core of this application. 

using Newtonsoft.Json;

namespace KahaAPI
{
    public class Program
    {
        //added for better testablity
        private readonly HttpClient _httpClient;

        public Program(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        static async Task Main(string[] args)
        {
            await TypeWriteLine("Welcome to the KAHA Travel Bot", delay: 50);
            await TypeWriteLine("Fetching list of countries...", delay: 50);

            // Call the method to fetch the list of countries
            List<Country>? countries = await GetAllCountries();

            // Display the list of countries
            if (countries != null && countries.Count > 0)
            {
                foreach (var country in countries)
                {
                    // You can uncomment this line to display each country's name and capital.
                    // await TypeWriteLine($"Country: {country.Name}, Capital: {country.Capital}", delay: 20);
                }
                await TypeWriteLine($"Countries fetched", delay: 20);

                // Get a random country in the southern hemisphere
                Country? randomSouthernHemisphereCountry = GetRandomSouthernHemisphereCountry(countries);

                if (randomSouthernHemisphereCountry != null)
                {
                    await TypeWriteLine($"Random Southern Hemisphere Country: {randomSouthernHemisphereCountry.Name}", delay: 20);

                    // Get sunrise and sunset times for tomorrow in the capital city
                    await GetSunriseAndSunsetTimes(randomSouthernHemisphereCountry.Capital);

                    // Output interesting country summary
                    await OutputCountrySummary(randomSouthernHemisphereCountry);

                    // Coordinates of KAHA offices
                    double kahaLatitude = -33.9759679;
                    double kahaLongitude = 18.4566283;

                    // Get the latitude and longitude of the random capital city
                    double capitalLatitude = randomSouthernHemisphereCountry.Latlng?[0] ?? 0.0;
                    double capitalLongitude = randomSouthernHemisphereCountry.Latlng?[1] ?? 0.0;

                    // Calculate the distance between the capital city and KAHA offices
                    double distanceInKm = CalculateDistance(capitalLatitude, capitalLongitude, kahaLatitude, kahaLongitude);

                    Console.ForegroundColor = ConsoleColor.Blue;
                    await TypeWriteLine($"Distance to KAHA offices: {distanceInKm:F2} kilometers", delay: 20);
                    Console.ResetColor(); // Reset the color to the default
                }
                else
                {
                    await TypeWriteLine("No countries in the southern hemisphere found.", delay: 20);
                }
            }
            else
            {
                await TypeWriteLine("No countries found.", delay: 20);
            }
        }

        public static async Task<List<Country>?> GetAllCountries()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Specify the API URL
                    var apiUrl = "https://restcountries.com/v2/all";

                    // Send an HTTP GET request to the API
                    var response = await httpClient.GetStringAsync(apiUrl);

                    // Deserialize the JSON response into a list of Country objects
                    List<Country>? countries = JsonConvert.DeserializeObject<List<Country>>(response);

                    return countries;
                }
                catch (Exception ex)
                {
                    await TypeWriteLine($"Error fetching countries: {ex.Message}", delay: 20);
                    return null; // Return null in case of an error
                }
            }
        }

        static Country? GetRandomSouthernHemisphereCountry(List<Country> countries)
        {
            var random = new Random();

            // Filter countries in the southern hemisphere (latitude < 0)
            var southernHemisphereCountries = countries.FindAll(country => country.Latlng != null && country.Latlng.Count >= 2 && country.Latlng[0] < 0);

            if (southernHemisphereCountries.Count > 0)
            {
                // Get a random index
                int randomIndex = random.Next(0, southernHemisphereCountries.Count);

                // Return the random country in the southern hemisphere
                return southernHemisphereCountries[randomIndex];
            }

            return null;
        }

        static async Task GetSunriseAndSunsetTimes(string capitalCity)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    // Specify the API URL
                    var apiUrl = $"https://api.sunrise-sunset.org/json?city={capitalCity}&country=&date=tomorrow";

                    // Send an HTTP GET request to the Sunrise-Sunset API
                    var response = await httpClient.GetStringAsync(apiUrl);

                    // Deserialize the JSON response into a SunriseSunsetResponse object
                    SunriseSunsetResponse? sunriseSunsetResponse = JsonConvert.DeserializeObject<SunriseSunsetResponse>(response);

                    if (sunriseSunsetResponse != null && sunriseSunsetResponse.Status == "OK")
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        await TypeWriteLine("Sunrise and Sunset Times for Tomorrow:", delay: 20);
                        Console.ResetColor(); // Reset the color to the default

                        await TypeWriteLine($"Sunrise: {sunriseSunsetResponse.Results.Sunrise}", delay: 20);
                        await TypeWriteLine($"Sunset: {sunriseSunsetResponse.Results.Sunset}", delay: 20);
                    }
                    else
                    {
                        await TypeWriteLine("Error fetching sunrise and sunset times.", delay: 20);
                    }
                }
                catch (Exception ex)
                {
                    await TypeWriteLine($"Error fetching sunrise and sunset times: {ex.Message}", delay: 20);
                }
            }
        }

        static async Task OutputCountrySummary(Country country)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            await TypeWriteLine("Country Summary:", delay: 20);
            Console.ResetColor(); // Reset the color to the default
            await TypeWriteLine($"Capital: {country.Capital}", delay: 20);
            await TypeWriteLine($"Total Official Languages: {country.Languages.Count}", delay: 20);

            if (country.Currencies.Count > 0)
            {
                foreach (var currency in country.Currencies)
                {
                    await TypeWriteLine($"Currency Used: {currency.Name} ({currency.Symbol})", delay: 20);
                }
            }
            else
            {
                await TypeWriteLine("No currency information available.", delay: 20);
            }
        }

        // Function to implement Typewriter style in console
        static async Task TypeWriteLine(string text, int delay)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                await Task.Delay(delay);
            }
            Console.WriteLine();
        }

        // Function to calculate distance using the Haversine formula 
        static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371; // Radius of the Earth in kilometers

            // Convert latitude and longitude from degrees to radians
            lat1 = ToRadians(lat1);
            lon1 = ToRadians(lon1);
            lat2 = ToRadians(lat2);
            lon2 = ToRadians(lon2);

            // Haversine formula
            double dlon = lon2 - lon1;
            double dlat = lat2 - lat1;
            double a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = earthRadius * c;

            return distance;
        }

        static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public class Country
        {
            public string Name { get; set; } = "";
            public string Capital { get; set; } = "";
            public List<float>? Latlng { get; set; } = new List<float>();
            public List<Language> Languages { get; set; } = new List<Language>();
            public List<Currency> Currencies { get; set; } = new List<Currency>();
        }

        class SunriseSunsetResponse
        {
            public string Status { get; set; } = "";
            public Results Results { get; set; } = new Results();
        }

        class Results
        {
            public string Sunrise { get; set; } = "";
            public string Sunset { get; set; } = "";
        }

        public class Language
        {
            public string Name { get; set; } = "";
        }

        public class Currency
        {
            public string Name { get; set; } = "";
            public string Symbol { get; set; } = "";
        }
    }

}