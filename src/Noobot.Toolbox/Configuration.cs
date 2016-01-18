using Noobot.Core.Configuration;
using Noobot.Toolbox.Pipeline.Middleware;
using Noobot.Toolbox.Plugins;

namespace Noobot.Toolbox
{
    /// <summary>
    /// A standard configuration with all of the middleware & plugins found in the toolbox. Note: This is an example and probably shouldn't be used in production code.
    /// </summary>
    public class Configuration : ConfigurationBase
    {
        public Configuration()
        {
            //UseMiddleware<AutoResponderMiddleware>();
            UseMiddleware<WelcomeMiddleware>();
            UseMiddleware<AdminMiddleware>();
            UseMiddleware<ScheduleMiddleware>();
            UseMiddleware<JokeMiddleware>();
            UseMiddleware<YieldTestMiddleware>();
            UseMiddleware<PingMiddleware>();
            UseMiddleware<FlickrMiddleware>();
            UseMiddleware<CalculatorMiddleware>();
            
            UsePlugin<StoragePlugin>();
            UsePlugin<SchedulePlugin>();
            UsePlugin<AdminPlugin>();
            UsePlugin<PingPlugin>();
        }
    }
}