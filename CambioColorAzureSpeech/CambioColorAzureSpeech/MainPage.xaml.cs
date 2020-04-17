using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Intent;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System.Drawing;
using Newtonsoft.Json.Linq;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace CambioColorAzureSpeech
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool muestraHora = false;
        bool muestraTexto = false;
        private  IntentRecognizer recognizer;
        private Dictionary<String, Windows.UI.Color> colors = new Dictionary<String, Windows.UI.Color>()
        {
            {"verde", Windows.UI.Color.FromArgb(0xFF,0x00,0x80,0x00)} ,
            {"rojo", Windows.UI.Color.FromArgb(0xFF,0xff,0x00,0x00)},
            {"azul", Windows.UI.Color.FromArgb(0xFF,0x00,0x00,0xff)}
        };

        // TODO De momento solo he añadido esto
        private static string predictionKey = "2469683b617541958b2caca858d22580";
        private static string predictionEndpoint = "https://inuluisapp-authoring.cognitiveservices.azure.com/";
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

        static async Task<PredictionResponse> GetPredictionAsync(string text)
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
                    Query = text,
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

        public MainPage()
        {
            this.InitializeComponent();
            EnableMicrophone();
            ConfigureIntentRecognizer();
        }

        public async void MyMessageBox(string mytext)
        {
            var dialog = new MessageDialog(mytext);
            await dialog.ShowAsync();
        }

        private async void EnableMicrophone()
        {
            try
            {
                var mediaCapture = new Windows.Media.Capture.MediaCapture();
                var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
                await mediaCapture.InitializeAsync(settings);
            }
            catch (Exception)
            {
                MyMessageBox("No se pudo inicializar el micrófono");
                
            }

        }

        private async void ConfigureIntentRecognizer()
        {

            var config = SpeechConfig.FromSubscription("e4c237841b7043b9b2c73a432a85416c", "westus");
            config.SpeechRecognitionLanguage = "es-es";
            var stopRecognition = new TaskCompletionSource<int>();
            using (recognizer = new IntentRecognizer(config))
            {
                var model = LanguageUnderstandingModel.FromAppId("ce892100-78a7-48b0-8e09-5dc18b16996d");
                recognizer.AddAllIntents(model, "Id1");

                recognizer.Recognized += async (s, e) =>
                {
                    await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        //recognizedText.Text = e.Result.Text;

                        //var prediction = predictionResult.Prediction;
                        //MyMessageBox(prediction.TopIntent);
                    });

                    try
                    {
                        var predictionResult = GetPredictionAsync(e.Result.Text).Result;
                        var prediction = predictionResult.Prediction;


                        if (prediction.TopIntent == "None")
                        {
                            await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                recognizedText.Text = "No entiendo la acción que me pides.";
                            });
                            muestraHora = false;
                            muestraTexto = false;
                        }
                        else if (prediction.TopIntent == "MuestraHora")
                        {
                            await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                var now = DateTime.Now;
                                //recognizedText.Text = now.ToString("HH:mm");
                                recognizedText.Text = "Son las " + now.Hour + ":" + now.Minute + ".";
                            });
                            muestraHora = true;
                            muestraTexto = false;
                        }
                        else if (prediction.TopIntent == "EscribeTexto")
                        {
                            await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {;
                                recognizedText.Text = "Esto es un texto.";
                            });
                            muestraTexto = true;
                            muestraHora = false;
                        }
                        else if (prediction.TopIntent == "OcultaHora")
                        {
                            await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (muestraHora)
                                {
                                    recognizedText.Text = "Hora oculta.";
                                    muestraHora = false;
                                    muestraTexto = false;
                                }
                            });
                        }
                        else if (prediction.TopIntent == "OcultaTexto")
                        {
                            await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (muestraTexto)
                                {
                                    recognizedText.Text = "Texto ocultado.";
                                    muestraHora = false;
                                    muestraTexto = false;
                                }
                            });
                        }
                        else if (prediction.TopIntent == "CambioColor")
                        {
                            string color = "";
                            await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                //string item = prediction.Entities.First().Value.ToString();
                                color = prediction.Entities.First().Value.ToString().Replace(System.Environment.NewLine, "");
                                color = color.Replace("[", "");
                                color = color.Replace("]", "");
                                color = color.Replace("\"", "");
                                color = color.Replace(" ", "");
                                recognizedText.Text = "Estableciendo fondo de color " + color + ".";
                                muestraHora = false;
                                muestraTexto = false;
                            });
                            try
                            {
                                if (colors.ContainsKey(color))
                                    await rectangle.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        rectangle.Fill = new SolidColorBrush(colors[color]);
                                    });
                            }
                            catch (NullReferenceException) { }
                        }
                    }
                    catch (System.AggregateException)
                    {

                    }

                    /*if (e.Result.Reason == ResultReason.RecognizedIntent)
                    {

                        var jsonResponse = JObject.Parse(e.Result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult));
                        await textJson.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            textJson.Text = jsonResponse.ToString();
                        });
                        var intent = jsonResponse.SelectToken("topScoringIntent.intent").ToString();

                        if (intent.Equals("CambioColor"))
                        {
                            try
                            {
                                string color = jsonResponse.SelectToken("$..entities[?(@.type=='Color')].entity").ToString();
                                if (colors.ContainsKey(color))
                                    await rectangle.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        rectangle.Fill = new SolidColorBrush(colors[color]);
                                    });
                            }
                            catch (NullReferenceException) { }



                        }


                    }
                    else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            recognizedText.Text = e.Result.Text;
                        });
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        await recognizedText.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            recognizedText.Text = "Speech could not be recognized.";
                        });
                    }*/
                };

                recognizer.Canceled += (s, e) =>
                {
                    Console.WriteLine($"CANCELED: Reason={e.Reason}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("\n    Session stopped event.");
                    Console.WriteLine("\nStop recognition.");
                    stopRecognition.TrySetResult(0);
                };

                Console.WriteLine("Say something...");
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                // Waits for completion.
                // Use Task.WaitAny to keep the task rooted.
                Task.WaitAny(new[] { stopRecognition.Task });

                // Stops recognition.
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }

        }

        private async Task Start_Button_Click(object sender, RoutedEventArgs e)
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        }

        private async Task Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }


    }
}
