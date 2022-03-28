using System;
using System.Net.Http;
using RestSharp;
using RestSharp.Authenticators;
using System.Text;

using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Google_Geocode_Test
{
    class Program
    {
        private static readonly string m_apiKey = "";
        private static readonly string m_reservedCharacters = "!*'();:@&=+$,/?%#[]";
        static void Main(string[] args)
        {
            SearchAddress();
            Console.WriteLine("Press Enter to restart or type \"exit\" to stop");
            string line = Console.ReadLine();
            while (line != "exit")
            {
                Console.Clear();
                SearchAddress();
                Console.WriteLine("Press Enter to restart or type \"exit\" to stop");
                line = Console.ReadLine()?.ToLower();

            }
        }

        public static void SearchAddress()
        {
            var country = "";
            var address = "";

            Console.WriteLine("Choose one option:\n1.\tAutoComplete\n2.\tGeolocation (Only to be used with full address)\n");
            var option = Console.ReadLine();

            while (option != null && (option != "1" && option != "2"))
            {
                Console.Clear();
                Console.WriteLine("Choose one option:\n1.\tAutoComplete\n2.\tGeolocation (Only to be used with full address)\n");
                option = Console.ReadLine();
            }


            Console.WriteLine("Please type in the ISO country id you want to look up:\n");
            country = Console.ReadLine()?.ToLower();
            Console.WriteLine($"\n");


            Console.WriteLine("Please type in the address you want to look up:\n");
            address = Console.ReadLine()?.ToLower();
            Console.WriteLine($"\n");

            try
            {
                switch (option)
                {
                    case "1":
                        DoAutoCompleteSearch(UrlEncode(address), UrlEncode(country));
                        break;
                    case "2":
                        DoGeoLocactionSearch(UrlEncode(address), UrlEncode(country), false);
                        break;
                    default:
                        Console.WriteLine($"You what?!");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }

        public static void DoGeoLocactionSearch(string address, string country, bool doPlaceID)
        {
            var results = GetResponseGeoLocation(address, country, doPlaceID);

            foreach (var result in results.results)
            {
                if (result.geometry.location_type == "APPROXIMATE")
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"WARNING: This location is an approximate");
                    Console.ResetColor();
                }

                Console.WriteLine($"Address:\t\t{result.formatted_address}\n" +
                                  $"Place ID:\t\t{result.place_id}\n" +
                                  $"Latitude:\t\t{result.geometry.location.lat}\n" +
                                  $"Longitude:\t\t{result.geometry.location.lng}\n" +
                                  $"Google maps url:\t{result.google_maps_url}");

            }
            Console.WriteLine($"\n");
        }

        public static void DoAutoCompleteSearch(string address, string country)
        {
            var jsonAutoCompleteResult = GetResponseAutoComplete(UrlEncode(address), UrlEncode(country));

            if (jsonAutoCompleteResult.predictions.Length == 1)
            {
                Console.WriteLine($"AutoComplete came back with 1 result. Getting more information...\n");
                DoGeoLocactionSearch(UrlEncode(jsonAutoCompleteResult.predictions[0].place_id), UrlEncode(country), true);
            }
            else
            {
                Console.WriteLine($"AutoComplete came back with multiple results:\n");
                int numberOfResults = 1;
                foreach (var result in jsonAutoCompleteResult.predictions)
                {
                    Console.WriteLine($"{numberOfResults}.\t {result.description}");
                    numberOfResults++;
                }
                Console.WriteLine($"For which result do you want more information?\n");

                var resultOption = Console.ReadLine();
                int intOption = 0;
                while (resultOption != null && (!int.TryParse(resultOption, out intOption)))
                {
                    Console.WriteLine("That's not an valid option, try again...\n");
                    resultOption = Console.ReadLine();
                }

                if (intOption > jsonAutoCompleteResult.predictions.Length)
                {
                    Console.WriteLine("That's not an valid option, try again...\n");
                    resultOption = Console.ReadLine();
                    while (resultOption != null && (!int.TryParse(resultOption, out intOption)))
                    {
                        Console.WriteLine("That's not an valid option, try again...\n");
                        resultOption = Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine($"Getting more information for option {intOption}\n");
                    DoGeoLocactionSearch(UrlEncode(jsonAutoCompleteResult.predictions[intOption - 1].place_id), UrlEncode(country), true);

                }

            }
            Console.WriteLine($"\n");
        }


        public static JsonAutoComplete GetResponseAutoComplete(string search, string country)
        {
            return GetContentAutoComplete($"https://maps.googleapis.com/maps/api/place/autocomplete/json?components=country:{country}&input={search}&key={m_apiKey}").Result;
        }

        public static JsonGeoLocation GetResponseGeoLocation(string search, string country, bool doPlaceID)
        {
            if (doPlaceID)
                return GetContentGeoLocation($"https://maps.googleapis.com/maps/api/geocode/json?place_id={search}&key={m_apiKey}").Result;
            return GetContentGeoLocation($"https://maps.googleapis.com/maps/api/geocode/json?address={search}=country:{country}&key={m_apiKey}").Result;
        }



        public static async Task<JsonGeoLocation> GetContentGeoLocation(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = httpClient.SendAsync(request);
                    var responseString = await response.Result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JsonGeoLocation>(responseString);
                    ;
                }
            }
        }

        public static async Task<JsonAutoComplete> GetContentAutoComplete(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = httpClient.SendAsync(request);
                    var responseString = await response.Result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<JsonAutoComplete>(responseString);
                    ;
                }
            }
        }
        public static string UrlEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            var sb = new StringBuilder();

            foreach (char @char in value)
            {
                if (m_reservedCharacters.IndexOf(@char) == -1)
                    sb.Append(@char);
                else
                    sb.AppendFormat("%{0:X2}", (int)@char);
            }
            return sb.ToString();
        }

    }
}
