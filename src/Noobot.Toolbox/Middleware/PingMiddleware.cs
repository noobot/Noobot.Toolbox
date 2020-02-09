using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private async IAsyncEnumerable<ResponseMessage> PingHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            yield return await Task.FromResult(message.ReplyToChannel($"Ok, I will start pinging @{message.Username}"));
            _pingPlugin.StartPingingUser(message.UserId);
        }

        private async IAsyncEnumerable<ResponseMessage> StopPingingHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            if (_pingPlugin.StopPingingUser(message.UserId))
            {
                yield return await Task.FromResult(message.ReplyToChannel($"Ok, I will stop pinging @{message.Username}"));
            }
            else
            {
                yield return message.ReplyToChannel($"BUT I AM NOT PINGING @{message.Username}");
            }
        }

        private async IAsyncEnumerable<ResponseMessage> ListPingHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            var users = _pingPlugin.ListPingedUsers();

            if (users.Any())
            {
                yield return await Task.FromResult(message.ReplyDirectlyToUser("I am currently pinging:"));
                yield return message.ReplyDirectlyToUser(">>>" + string.Join("\n", users));
            }
            else
            {
                yield return message.ReplyDirectlyToUser("I am not currently pinging anyone.");
            }
        }
    }
}