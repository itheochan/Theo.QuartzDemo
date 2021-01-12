/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Quartz
* FileName			: QuartzJob.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 11:21:24
* --------------------------------------------------
*/
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Theo.TaskScheduler.Quartz
{
    ///<summary>
    /// QuartzJob
    ///</summary>
    public class QuartzJob : IJob
    {
        private ILogger _logger;
        private IServiceProvider _serviceProvider;

        public QuartzJob() { }
        public QuartzJob(ILogger<QuartzJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            //_logger.LogInformation(GetType().Name + " .Ctor");
        }

        protected void Init(ILogger<QuartzJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            //_logger.LogInformation(GetType().Name + " .Init");
        }

        public virtual Task Execute(IJobExecutionContext context)
        {
            var map = context.JobDetail.JobDataMap;
            var group = map["group"].ToString();
            var jobName = map["name"].ToString();
            var dllName = map["dllName"].ToString();
            var className = map["className"].ToString();
            var methodName = map["methodName"].ToString();

            _logger.LogDebug($"{nameof(Execute)} group:{group}, jobName:{jobName}");

            try
            {
                var type = Assembly.Load(dllName).GetType(className);

                if ((type.IsClass && !type.IsAbstract) || type.IsInterface)
                {
                    var obj = _serviceProvider.GetService(type);
                    //_logger.LogInformation($"{className}:{obj}");
                    MethodInfo method = type.GetMethod(methodName);
                    if (type.IsInterface || (type.IsClass && !method.IsAbstract))
                    {
                        //object ret = method.Invoke(obj, null); //调用实例方法
                        var paramList = method.GetParameters();
                        if (paramList != null && paramList.Length == 1 && paramList[0].ParameterType == typeof(string))
                        {
                            method.Invoke(obj, new object[] { jobName });
                        }
                        else
                            method.Invoke(obj, null); //调用实例方法
                        //_logger.LogInformation("Execute; Result: " + JsonSerializer.Serialize(ret));
                    }
                }
                else
                {
                    _logger.LogWarning("Execute; failed: " + className + " isn't a class or an abstract class..");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Execute; Excepetion: " + ex.ToString());
            }
            return Task.CompletedTask;
        }
    }
}