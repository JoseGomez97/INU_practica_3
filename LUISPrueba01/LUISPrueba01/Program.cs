using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace LUISPrueba01
{
    class Program
    {
        private static string predictionKey = "2469683b617541958b2caca858d22580";

        // Endpoint URL example value = "https://YOUR-RESOURCE-NAME.api.cognitive.microsoft.com"
        private static string predictionEndpoint = "https://inuluisapp-authoring.cognitiveservices.azure.com/";

        // App Id example value = "df67dcdb-c37d-46af-88e1-8b97951ca1c2"
        private static string appId = "dd949566-f531-4281-acb4-15383fd70a16";
        static LUISRuntimeClient CreateClient()
        {
            var credentials = new ApiKeyServiceClientCredentials(predictionKey);
            var luisClient = new LUISRuntimeClient(credentials, new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = predictionEndpoint
            };

            return luisClient;

        }
        static async Task<PredictionResponse> GetPredictionAsync()
        {

            // Get client 
            using (var luisClient = CreateClient())
            {

                var requestOptions = new PredictionRequestOptions
                {
                    DatetimeReference = DateTime.Parse("2019-01-01"),
                    PreferExternalEntities = true
                };

                var predictionRequest = new PredictionRequest
                {
                    Query = "mostrar hora",
                    Options = requestOptions
                };

                // get prediction
                return await luisClient.Prediction.GetSlotPredictionAsync(
                    Guid.Parse(appId),
                    slotName: "production",
                    predictionRequest,
                    verbose: true,
                    showAllIntents: true,
                    log: true);
            }
        }
        static void Main(string[] args)
        {
            // Get prediction
            var predictionResult = GetPredictionAsync().Result;

            var prediction = predictionResult.Prediction;

            // Display query
            Console.WriteLine("Query:'{0}'", predictionResult.Query);
            Console.WriteLine("TopIntent :'{0}' ", prediction.TopIntent);

            foreach (var i in prediction.Intents)
            {
                Console.WriteLine(string.Format("{0}:{1}", i.Key, i.Value.Score));
            }

            foreach (var e in prediction.Entities)
            {
                Console.WriteLine(string.Format(" Entities Key= {0} Value= {1}", e.Key, e.Value));
            }

            Console.Write("done");
            Console.ReadKey();

        }
    }
}
