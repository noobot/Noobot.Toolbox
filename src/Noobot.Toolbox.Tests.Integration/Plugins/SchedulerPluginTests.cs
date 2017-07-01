using System;
using System.IO;
using System.Threading;
using Noobot.Core;
using Noobot.Core.Configuration;
using Noobot.Core.DependencyResolution;
using Noobot.Core.MessagingPipeline.Response;
using Noobot.Toolbox.Middleware;
using Noobot.Toolbox.Plugins;
using Noobot.Toolbox.Plugins.Scheduling;
using NUnit.Framework;

namespace Noobot.Toolbox.Tests.Integration.Plugins
{
    [Explicit]
    [TestFixture]
    public class SchedulerPluginTests
    {
        private INoobotContainer _container;
        private INoobotCore _noobotCore;

        [SetUp]
        public void Setup()
        {
            File.Delete(Path.Combine(Environment.CurrentDirectory, "data/schedules.json"));

            var containerFactory = new ContainerFactory(new SchedulerConfig(), new ConfigReader(), NoobotWrapper.GetLogger());
            _container = containerFactory.CreateContainer();

            _noobotCore = _container.GetNoobotCore();
            _noobotCore.Connect().Wait(TimeSpan.FromMinutes(1));
        }

        [TearDown]
        public void TearDown()
        {
            _noobotCore.Disconnect();
        }

        [Test]
        public void should_do_a_thing()
        {
            // given
            SchedulePlugin schedulePlugin = _container.GetPlugin<SchedulePlugin>();
            Guid guid = Guid.NewGuid();
            const string cronSchedule = "*/20 * * * * ?"; // every 5 seconds

            // when
            schedulePlugin.AddSchedule(new ScheduleEntry
            {
                Guid = guid,
                CronSchedule = cronSchedule,
                Channel = _noobotCore.GetChannelId("#general"),
                ChannelType = ResponseType.Channel,
                Command = $"@{_noobotCore.GetBotUserName()} joke",
                UserId = _noobotCore.GetUserIdForUsername("simon"),
                UserName = "simon"
            });

            // then
            Thread.Sleep(TimeSpan.FromMinutes(1));

            // when
            schedulePlugin.DeleteSchedule(guid);

            // then
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }

        private class SchedulerConfig : ConfigurationBase
        {
            public SchedulerConfig()
            {
                UseMiddleware<ScheduleMiddleware>();
                UseMiddleware<JokeMiddleware>();

                UsePlugin<JsonStoragePlugin>();
                UsePlugin<SchedulePlugin>();
            }
        }
    }
}