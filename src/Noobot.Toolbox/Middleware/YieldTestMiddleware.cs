using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;

namespace Noobot.Toolbox.Middleware
{
    public class YieldTestMiddleware : MiddlewareBase
    {
        public YieldTestMiddleware(IMiddleware next) : base(next)
        {
            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("yield test"),
                    EvaluatorFunc = YieldTest,
                    Description = "Just tests delayed messages"
                }
            };
        }

        private async IAsyncEnumerable<ResponseMessage> YieldTest(IncomingMessage incomingMessage, IValidHandle matchedHandle)
        {
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine("Sending message");
                yield return new ResponseMessage { Channel = incomingMessage.Channel, Text = "Waiting " + i };
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}