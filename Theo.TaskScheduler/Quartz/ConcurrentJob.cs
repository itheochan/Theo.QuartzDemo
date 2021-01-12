/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Quartz
* FileName			: ConcurrentJob.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 14:47:24
* --------------------------------------------------
*/
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace Theo.TaskScheduler.Quartz
{
    /// <summary>
    /// 可并发执行job，不管上次执行结果，到时间就重新调用执行
    /// </summary>
    public class ConcurrentJob : QuartzJob
    {
        #region Fields
        public ConcurrentJob()
        {
            base.Init(IOCHelper.GetService<ILogger<BlockedJob>>(), IOCHelper.ServiceProvider);
        }
        #endregion Fields

        #region Methods
        public override Task Execute(IJobExecutionContext context)
        {
            return base.Execute(context);
        }
        #endregion Methods
    }
}