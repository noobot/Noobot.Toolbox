﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;

namespace Noobot.Toolbox.Middleware
{
    public class AutoResponderMiddleware : MiddlewareBase
    {
        public AutoResponderMiddleware(IMiddleware next) : base(next)
        {
            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = new IValidHandle[] { new AlwaysMatchHandle() },
                    Description = "Annoys the heck out of everyone",
                    EvaluatorFunc = AutoResponseHandler,
                    MessageShouldTargetBot = false,
                    ShouldContinueProcessing = true,
                    VisibleInHelp = false
                }
            };
        }

        private async IAsyncEnumerable<ResponseMessage> AutoResponseHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            yield return await Task.FromResult(message.ReplyDirectlyToUser(message.FullText));
        }
    }
}