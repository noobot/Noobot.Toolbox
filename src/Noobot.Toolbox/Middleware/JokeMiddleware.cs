using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Noobot.Core.MessagingPipeline.Middleware;
using Noobot.Core.MessagingPipeline.Middleware.ValidHandles;
using Noobot.Core.MessagingPipeline.Request;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Core.Plugins.StandardPlugins;

namespace Noobot.Toolbox.Middleware
{
    public class JokeMiddleware : MiddlewareBase
    {
        private readonly StatsPlugin _statsPlugin;

        public JokeMiddleware(IMiddleware next, StatsPlugin statsPlugin) : base(next)
        {
            _statsPlugin = statsPlugin;
            HandlerMappings = new[]
            {
                new HandlerMapping
                {
                    ValidHandles = ExactMatchHandle.For("joke", "tell me a joke"),
                    Description = "Tells a random joke",
                    EvaluatorFunc = JokeHandler
                }
            };
        }

        private async IAsyncEnumerable<ResponseMessage> JokeHandler(IncomingMessage message, IValidHandle matchedHandle)
        {
            yield return message.IndicateTypingOnChannel();

            var jokeResponse = await (new Random().Next(0, 100) < 80
                ? GetChuckNorrisJoke()
                : GetMommaJoke());

            _statsPlugin.IncrementState("Jokes:Told");
            var jokeString = $"{{ {jokeResponse.SelectToken("$..joke").Parent} }}";
            var joke = JsonConvert.DeserializeObject<JokeContainer>(jokeString);

            yield return message.ReplyToChannel(joke.Joke);
        }

        private static async Task<JObject> GetChuckNorrisJoke()
        {
            return await "http://api.icndb.com"
                .AppendPathSegment("/jokes/random")
                .GetJsonAsync<JObject>();
        }

        private static async Task<JObject> GetMommaJoke()
        {
            return await "http://api.yomomma.info"
                .GetJsonAsync<JObject>();
        }

        private class JokeContainer
        {
            [JsonProperty("joke", Required = Required.Always)]
            public string Joke { get; set; }
        }
    }
}
