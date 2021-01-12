/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Quartz
* FileName			: BlockedJob.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 14:00:55
* --------------------------------------------------
*/
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace Theo.TaskScheduler.Quartz
{
    ///<summary>
    /// 阻塞模式job，
    /// 上次执行尚未结束，则不调用
    /// </summary>
    [DisallowConcurrentExecution]
    public class BlockedJob : QuartzJob
    {
        public BlockedJob()
        {
            base.Init(IOCHelper.GetService<ILogger<BlockedJob>>(), IOCHelper.ServiceProvider);
        }
        public override Task Execute(IJobExecutionContext context)
        {
            return base.Execute(context);
        }
    }
}