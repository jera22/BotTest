using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BotTest.Dialogs
{
    public class BotServices : IBotServices
    {
        public QnAMakerDialog QnA { get; set; }
        public ComputerVisionClient client { get; set; }

        public BotServices(IConfiguration configuration)
        {
            QnA = new QnAMakerDialog(configuration["QnAKnowledgebaseId"],
                 configuration["QnAEndpointKey"],
                 configuration["QnAEndpointHostName"]
            );

            client = Authenticate(configuration["CognitiveEndpoint"], configuration["CognitiveAPIKey"]);
        }



        private ComputerVisionClient Authenticate(string endpoint, string subscriptionKey)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };
            return client;
        }

        public async Task<string> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {

            // Creating a list that defines the features to be extracted from the image. 
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };
            Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
            Console.WriteLine();

            // Analyze the URL image 
            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);


            // Display categories the image is divided into.
            Console.WriteLine("Categories:");

            string categoriesString = "";
            if (results.Categories?.Count > 1)
            {
                foreach (var category in results.Categories)
                {
                    categoriesString += category.Name + " with confidence " + category.Score + Environment.NewLine;
                }
            }
            else
            {
                categoriesString = "Sorry I couldn't find any matching categories 🤦‍♂️";
            }


            return categoriesString;

        }

        public string GetIntent(ITurnContext<IMessageActivity> turnContext)
        {
            return turnContext.Activity.Attachments?.Count > 0 ? "computerVision" : "qna";
        }
    }
}