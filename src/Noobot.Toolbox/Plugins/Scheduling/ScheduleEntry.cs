using System;
using Noobot.Core.MessagingPipeline.Response;

namespace Noobot.Toolbox.Plugins.Scheduling
{
    public class ScheduleEntry
    {
        public Guid Guid { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastRun { get; set; }
        public string CronSchedule { get; set; }
        public string Command { get; set; }
        public string Channel { get; set; }
        public ResponseType ChannelType { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public override string ToString()
        {
            return $"Running command `'{Command}'` every `'{CronSchedule}'`. Created on `'{Created}'`. Last run on `'{LastRun}'`. Guid `'{Guid}'`";
        }
    }
}