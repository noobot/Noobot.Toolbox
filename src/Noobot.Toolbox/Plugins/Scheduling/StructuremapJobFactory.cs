using Noobot.Core.DependencyResolution;
using Quartz;
using Quartz.Spi;

namespace Noobot.Toolbox.Plugins.Scheduling
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
            return _noobotContainer.GetStructuremapContainer().GetInstance(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        { }
    }
}