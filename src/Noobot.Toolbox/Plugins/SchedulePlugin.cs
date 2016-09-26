using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Noobot.Core;
using Noobot.Core.Logging;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Core.Plugins;
using Noobot.Core.Plugins.StandardPlugins;
using Quartz;
using Quartz.Impl;
using SlackConnector.Models;

namespace Noobot.Toolbox.Plugins
{
    public class SchedulePlugin : IPlugin
    {
        private string FileName { get; } = "schedules";
        private readonly JsonStoragePlugin _storagePlugin;
        private readonly INoobotCore _noobotCore;
        private readonly StatsPlugin _statsPlugin;
        private readonly ILog _log;
        private readonly object _lock = new object();
        private readonly List<ScheduleEntry> _schedules = new List<ScheduleEntry>();

        public SchedulePlugin(JsonStoragePlugin storagePlugin, INoobotCore noobotCore, StatsPlugin statsPlugin, ILog log)
        {
            _storagePlugin = storagePlugin;
            _noobotCore = noobotCore;
            _statsPlugin = statsPlugin;
            _log = log;
        }

        public void Start()
        {
            lock (_lock)
            {
                ScheduleEntry[] schedules = _storagePlugin.ReadFile<ScheduleEntry>(FileName);
                _schedules.AddRange(schedules);

            }
        }

        public void Stop()
        {
            Save();
        }

        public void AddSchedule(ScheduleEntry schedule)
        {
            lock (_lock)
            {
                ExecuteSchedule(schedule);
                _schedules.Add(schedule);
            }

            Save();
        }

        public ScheduleEntry[] ListSchedulesForChannel(string channel)
        {
            lock (_lock)
            {
                ScheduleEntry[] schedules = _schedules
                                                .Where(x => x.Channel == channel)
                                                .OrderBy(x => x.LastRun)
                                                .ThenBy(x => x.Command)
                                                .ToArray();
                return schedules;
            }
        }

        public ScheduleEntry[] ListAllSchedules()
        {
            lock (_lock)
            {
                ScheduleEntry[] schedules = _schedules
                                                .OrderBy(x => x.LastRun)
                                                .ThenBy(x => x.Command)
                                                .ToArray();
                return schedules;
            }
        }

        public void DeleteSchedules(ScheduleEntry[] scheduleEntries)
        {
            lock (_lock)
            {
                foreach (ScheduleEntry scheduleEntry in scheduleEntries)
                {
                    _schedules.Remove(scheduleEntry);
                }
            }

            Save();
        }

        public void DeleteSchedule(ScheduleEntry scheduleEntry)
        {
            DeleteSchedules(new[] { scheduleEntry });
        }

        private void ExecuteSchedule(ScheduleEntry schedule)
        {
            var job = JobBuilder.Create();
            job.UsingJobData("guid", schedule.Guid.ToString());
           // job.


            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger3", "group1")
                .WithCronSchedule(schedule.CronSchedule)
                .ForJob(job.Build())
                .Build();

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            //scheduler.JobFactory = new 
            //scheduler.
        }

        // private void RunSchedules(object sender, ElapsedEventArgs e)
        //{
        //     lock (_lock)
        //     {
        //         _statsPlugin.RecordStat("Schedules:LastRun", DateTime.Now.ToString("G"));
        //         _statsPlugin.RecordStat("Schedules:IsCurrentlyNight", IsCurrentlyNight().ToString());

        //         foreach (var schedule in _schedules)
        //         {
        //             ExecuteSchedule(schedule);
        //         }
        //     }

        //     Save();
        // }

        //private void ExecuteSchedule(ScheduleEntry schedule)
        //{
        //    if (ShouldRunSchedule(schedule))
        //    {
        //        _log.Log($"Running schedule: {schedule}");

        //        SlackChatHubType channelType = schedule.ChannelType == ResponseType.Channel
        //            ? SlackChatHubType.Channel
        //            : SlackChatHubType.DM;

        //        var slackMessage = new SlackMessage
        //        {
        //            Text = schedule.Command,
        //            User = new SlackUser { Id = schedule.UserId, Name = schedule.UserName },
        //            ChatHub = new SlackChatHub { Id = schedule.Channel, Type = channelType },
        //        };

        //        _noobotCore.MessageReceived(slackMessage);
        //        schedule.LastRun = DateTime.Now;
        //    }
        //}

        //private static bool ShouldRunSchedule(ScheduleEntry schedule)
        //{
        //    bool shouldRun = false;
        //    if (!schedule.LastRun.HasValue)
        //    {
        //        shouldRun = true;
        //    }
        //    else if (schedule.LastRun + schedule.RunEvery < DateTime.Now)
        //    {
        //        shouldRun = true;
        //    }

        //    if (shouldRun & schedule.RunOnlyAtNight)
        //    {
        //        shouldRun = IsCurrentlyNight();
        //    }

        //    return shouldRun;
        //}

        private void Save()
        {
            lock (_lock)
            {
                _storagePlugin.SaveFile(FileName, _schedules.ToArray());
                _statsPlugin.RecordStat("Schedules:Active", _schedules.Count);
            }
        }

        public class ScheduleEntry
        {
            public Guid Guid { get; set; }
            public DateTime? LastRun { get; set; }
            public string CronSchedule { get; set; }
            public string Command { get; set; }
            public string Channel { get; set; }
            public ResponseType ChannelType { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }

            public override string ToString()
            {
                return $"Running command `'{Command}'` every `'{CronSchedule}'`. Last run at `'{LastRun}'`. Guid {Guid}";
            }
        }
    }
}
