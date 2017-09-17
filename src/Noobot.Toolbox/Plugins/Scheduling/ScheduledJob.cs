using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Noobot.Core;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Core.Plugins.StandardPlugins;
using Quartz;
using SlackConnector.Models;

namespace Noobot.Toolbox.Plugins.Scheduling
{
    public class ScheduledJob : IJob
    {
        private readonly SchedulePlugin _schedulePlugin;
        private readonly INoobotCore _noobotCore;
        private readonly ILog _log;
        private readonly StatsPlugin _statsPlugin;

        public ScheduledJob(SchedulePlugin schedulePlugin, INoobotCore noobotCore, ILog logger, StatsPlugin statsPlugin)
        {
            _schedulePlugin = schedulePlugin;
            _noobotCore = noobotCore;
            _log = logger;
            _statsPlugin = statsPlugin;
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            Guid guid = Guid.Parse(context.JobDetail.JobDataMap["guid"].ToString());
            var schedule = _schedulePlugin.ListAllSchedules().First(x => x.Guid == guid);

            _statsPlugin.IncrementState("schedules:run");
            _log.Info($"Running schedule: {schedule}");

            SlackChatHubType channelType = schedule.ChannelType == ResponseType.Channel
                ? SlackChatHubType.Channel
                : SlackChatHubType.DM;

            var slackMessage = new SlackMessage
            {
                Text = schedule.Command,
                User = new SlackUser { Id = schedule.UserId, Name = schedule.UserName },
                ChatHub = new SlackChatHub { Id = schedule.Channel, Type = channelType },
            };

            _noobotCore.MessageReceived(slackMessage);
            schedule.LastRun = DateTime.Now;

            return Task.CompletedTask;
        }
    }
}