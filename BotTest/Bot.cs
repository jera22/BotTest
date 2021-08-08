using BotTest.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotTest
{
    public class Bot : ActivityHandler
    {
        private readonly ILogger<Bot> _logger;
        private readonly ConversationState _conversationState;
        private readonly IBotServices _botServices;
        private DialogSet _dialogSet;
        private Dialog Dialog;

        public Bot(ConversationState conversationState, ILogger<Bot> logger, IBotServices botServices)
        {
            _conversationState = conversationState;
            _botServices = botServices;
            _logger = logger;
            var conversationStateAccessors = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _dialogSet = new DialogSet(conversationStateAccessors);

        }


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                //Gives feedback to user that bot is doing something
                await turnContext.SendActivitiesAsync(
                new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 3000 },

                },
                cancellationToken);

                var _dialogContext = await _dialogSet.CreateContextAsync(turnContext);
                Dialog = _botServices.QnA;

                //This check is used for more complex multi-turn QnA answers with hero cards for an answer
                if (_dialogContext.ActiveDialog != null)
                {
                    await _dialogContext.ContinueDialogAsync();
                }
                else
                {
                    Dialog.Id = Guid.NewGuid().ToString();
                }

                //Gets intent depending on the content of the message
                var intent = _botServices.GetIntent(turnContext);

                //Routes the turnContext to correct Processor
                await DispatchToTopIntentAsync(turnContext, intent, _dialogContext, cancellationToken);

                //Saves the state of the conversation
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
           
        }

        /// <summary>
        /// Dispatches the conversation to processor
        /// </summary>
        /// <param name="turnContext">Context of the currnet turn</param>
        /// <param name="dialogContext">Context of dialog</param>
        /// /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, DialogContext dialogContext, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "qna":
                    await ProcessQnAAsync(dialogContext);
                    break;

                case "computerVision":
                    await ProcessImageAsync(turnContext, cancellationToken);
                    break;

                default:
                    _logger.LogInformation($"Dispatch unrecognized intent: {intent}.");
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);
                    break;
            }
        }

        private async Task ProcessImageAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var item in turnContext.Activity.Attachments)
                {
                    if (item.ContentType.Contains("image"))
                    {
                        var img_url = item.ContentUrl;
                        var resultString = await _botServices.AnalyzeImageUrl(_botServices.client, img_url);
                        await turnContext.SendActivityAsync(MessageFactory.Text(resultString), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Some file types are not supported and this is one of them ->" + item.ContentType), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            
        }

        private async Task ProcessQnAAsync(DialogContext _dialogContext)
        {
            try
            {
                if (_dialogSet.Find(Dialog.Id) == null)
                {
                    _dialogSet.Add(Dialog);
                    await _dialogContext.BeginDialogAsync(Dialog.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
