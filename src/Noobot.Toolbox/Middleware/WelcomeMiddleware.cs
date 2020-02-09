﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Core.Plugins.StandardPlugins;

namespace Noobot.Toolbox.Middleware
{
    public class WelcomeMiddleware : MiddlewareBase
    {
        private readonly StatsPlugin _statsPlugin;

        public WelcomeMiddleware(IMiddleware next, StatsPlugin statsPlugin) : base(next)
        {
            _statsPlugin = statsPlugin;
            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("hi", "hey", "hello", "wuzzup"),
                    Description = "Try saying hi and see what happens",
                    EvaluatorFunc = WelcomeHandler
                }
            };
        }

        private async IAsyncEnumerable<ResponseMessage> WelcomeHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            _statsPlugin.IncrementState("Hello");

            yield return message.ReplyToChannel($"Hey @{message.Username}, how you doing?");

            await Task.Delay(TimeSpan.FromSeconds(5));

            yield return message.ReplyDirectlyToUser("I know where you live...");
        }
    }
}