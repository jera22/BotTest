using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace BotTest.Dialogs

{
    public interface IBotServices
    {
        public QnAMakerDialog QnA { get; set; }
        ComputerVisionClient client { get; set; }

        /// <summary>
        /// Analyzes the image using ComputerVision
        /// </summary>
        /// <param name="client">ComputerVisionClient</param>
        /// <param name="imageUrl">Url of an Image</param>
        /// <returns>string</returns>
        public Task<string> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl);

       

        /// <summary>
        /// Gets intent for current turn contex
        /// </summary>
        /// <param name="turnContext"></param>
        /// <returns>string</returns>
        string GetIntent(ITurnContext<IMessageActivity> turnContext);
    }
}