using System;
using System.IO;
using System.Threading;
using Noobot.Core;
using Noobot.Core.Configuration;
using Noobot.Core.DependencyResolution;
using NUnit.Framework;

namespace Noobot.Toolbox.Tests.Integration
{
    [Explicit]
    [TestFixture]
    public class NoobotDemo
    {
        private INoobotContainer _container;
        private INoobotCore _noobotCore;

        [SetUp]
        public void Setup()
        {
            var containerFactory = new ContainerFactory(new ToolboxConfiguration(), new ConfigReader(), NoobotWrapper.GetLogger());
            _container = containerFactory.CreateContainer();

            _noobotCore = _container.GetNoobotCore();
            _noobotCore.Connect().Wait(TimeSpan.FromMinutes(1));
        }

        [TearDown]
        public void TearDown()
        {
            _noobotCore.Disconnect();
            File.Delete(Path.Combine(Environment.CurrentDirectory, "data/schedules.json"));
        }

        [Test]
        public void should_do_a_thing()
        {
            Thread.Sleep(TimeSpan.FromMinutes(10));
        }
    }
}