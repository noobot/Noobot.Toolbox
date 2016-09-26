using Noobot.Core.DependencyResolution;
using Quartz;
using Quartz.Spi;

namespace Noobot.Toolbox.Middleware.Scheduling
{
    public class StructuremapJobFactory : IJobFactory
    {
        private readonly INoobotContainer _noobotContainer;

        public StructuremapJobFactory(INoobotContainer noobotContainer)
        {
            _noobotContainer = noobotContainer;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            throw new System.NotImplementedException();
        }

        public void ReturnJob(IJob job)
        {
            throw new System.NotImplementedException();
        }
    }
}