using System;
using System.Collections.Generic;
using System.Linq;
using Noobot.Core.Plugins;
using Noobot.Core.Plugins.StandardPlugins;
using Noobot.Toolbox.Plugins.Scheduling;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;

namespace Noobot.Toolbox.Plugins
{
    public class SchedulePlugin : IPlugin
    {
        private string FileName { get; } = "schedules";
        private readonly StructuremapJobFactory _jobFactory;
        private readonly JsonStoragePlugin _storagePlugin;
        private readonly StatsPlugin _statsPlugin;
        private readonly object _lock = new object();
        private readonly List<ScheduleEntry> _schedules = new List<ScheduleEntry>();
        private IScheduler _scheduler;

        public SchedulePlugin(StructuremapJobFactory jobFactory, JsonStoragePlugin storagePlugin, StatsPlugin statsPlugin)
        {
            _jobFactory = jobFactory;
            _storagePlugin = storagePlugin;
            _statsPlugin = statsPlugin;
        }

        public void Start()
        {
            lock (_lock)
            {
                _scheduler = StdSchedulerFactory.GetDefaultScheduler();
                _scheduler.JobFactory = _jobFactory;

                ScheduleEntry[] schedules = _storagePlugin.ReadFile<ScheduleEntry>(FileName);
                foreach (var schedule in schedules)
                {
                    AddSchedule(schedule);
                }

                _scheduler.Start();
            }
        }

        public void Stop()
        {
            _scheduler.Shutdown(false);
            Save();
        }

        public void AddSchedule(ScheduleEntry schedule)
        {
            if (!CronExpression.IsValidExpression(schedule.CronSchedule))
            {
                _statsPlugin.IncrementState("schedules:invalidcron");
                throw new InvalidConfigurationException("Cron expression is invalid");
            }

            lock (_lock)
            {
                schedule.Created = DateTime.Now;
                ExecuteSchedule(schedule);
                _schedules.Add(schedule);
                _statsPlugin.IncrementState("schedules:added");
            }

            Save();
        }

        public ScheduleEntry[] ListSchedulesForChannel(string channel)
        {
            lock (_lock)
            {
                ScheduleEntry[] schedules = _schedules
                    .Where(x => x.Channel == channel)
                    .OrderBy(x => x.Command)
                    .ToArray();
                return schedules;
            }
        }

        public ScheduleEntry[] ListAllSchedules()
        {
            lock (_lock)
            {
                ScheduleEntry[] schedules = _schedules
                    .OrderBy(x => x.Command)
                    .ToArray();
                return schedules;
            }
        }

        public void DeleteSchedules(Guid[] guids)
        {
            lock (_lock)
            {
                foreach (Guid guid in guids)
                {
                    _scheduler.UnscheduleJob(new TriggerKey(guid.ToString(), "job"));

                    ScheduleEntry toRemove = _schedules.First(x => x.Guid == guid);
                    _schedules.Remove(toRemove);
                    _statsPlugin.IncrementState("schedules:deleted");
                }
            }

            Save();
        }

        public void DeleteSchedule(Guid guid)
        {
            DeleteSchedules(new[] { guid });
        }

        private void ExecuteSchedule(ScheduleEntry schedule)
        {
            IJobDetail job = JobBuilder.Create<ScheduledJob>()
                .UsingJobData("guid", schedule.Guid.ToString())
                .WithIdentity(schedule.Guid.ToString())
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(new TriggerKey(schedule.Guid.ToString(), "job"))
                .WithCronSchedule(schedule.CronSchedule)
                .Build();

            _scheduler.ScheduleJob(job, trigger);
        }

        private void Save()
        {
            lock (_lock)
            {
                _storagePlugin.SaveFile(FileName, _schedules.ToArray());
                _statsPlugin.RecordStat("Schedules:Active", _schedules.Count);
            }
        }
    }
}
