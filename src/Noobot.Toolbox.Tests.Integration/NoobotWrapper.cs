using Common.Logging;
using Common.Logging.Simple;

namespace Noobot.Toolbox.Tests.Integration
{
    public static class NoobotWrapper
    {
        public static ConsoleOutLogger GetLogger()
        {
            return new ConsoleOutLogger("Integration Test", LogLevel.All, true, true, false, "yyyy/MM/dd HH:mm:ss:fff");
        }
    }
}
