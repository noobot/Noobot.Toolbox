using System.Collections.Generic;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Toolbox.Plugins;

namespace Noobot.Toolbox.Middleware
{
    public class PingMiddleware : MiddlewareBase
    {
        private readonly PingPlugin _pingPlugin;

        public PingMiddleware(IMiddleware next, PingPlugin pingPlugin) : base(next)
        {
            _pingPlugin = pingPlugin;

            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("ping stop", "stop pinging me"),
                    Description = "Stops sending you pings",
                    EvaluatorFunc = StopPingingHandler
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("ping list"),
                    Description = "Lists all of the people currently being pinged",
                    EvaluatorFunc = ListPingHandler
                },
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("ping me"),
                    Description = "Sends you a ping about every second",
                    EvaluatorFunc = PingHandler
                },
            };
        }

        private IEnumerable<ResponseMessage> PingHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            yield return message.ReplyToChannel($"Ok, I will start pinging @{message.Username}");
            _pingPlugin.StartPingingUser(message.UserId);
        }

        private IEnumerable<ResponseMessage> StopPingingHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (_pingPlugin.StopPingingUser(message.UserId))
            {
                yield return message.ReplyToChannel($"Ok, I will stop pinging @{message.Username}");
            }
            else
            {
                yield return message.ReplyToChannel($"BUT I AM NOT PINGING @{message.Username}");
            }
        }

        private IEnumerable<ResponseMessage> ListPingHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            string[] users = _pingPlugin.ListPingedUsers();

            yield return message.ReplyDirectlyToUser("I am currently pinging:");
            yield return message.ReplyDirectlyToUser(">>>" + string.Join("\n", users));
        }
    }
}