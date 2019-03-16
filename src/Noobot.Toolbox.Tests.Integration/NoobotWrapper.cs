using Microsoft.Extensions.Logging;

namespace Noobot.Toolbox.Tests.Integration
{
    public static class NoobotWrapper
    {
        public static ILogger GetLogger()
        {
            return new ConsoleLogger();
        }
    }
}
