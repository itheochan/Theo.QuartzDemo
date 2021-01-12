/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Quartz
* FileName			: QuartzWorker.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 14:48:04
* --------------------------------------------------
*/
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Theo.TaskScheduler.Models;

namespace Theo.TaskScheduler.Quartz
{
    /// <summary>
    /// 后台服务
    /// </summary>
    public class QuartzWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QuartzWorker> _logger;
        private readonly StdSchedulerFactory _schedulerFactory;
        private PhysicalFileProvider _FileProvider;
        private readonly string _ConfigFile;
        private IScheduler Scheduler;
        private CancellationToken Token;

        public QuartzWorker(ILogger<QuartzWorker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _schedulerFactory = new StdSchedulerFactory();
            string configFile = _configuration["Quartz:Config"];
            if (!string.IsNullOrWhiteSpace(configFile) && !configFile.Contains(':'))
                configFile = Path.Combine(AppContext.BaseDirectory, _configuration["Quartz:Config"]);
            _ConfigFile = configFile;
            _logger.LogInformation($"{nameof(QuartzWorker)}.Ctor...configFile:{_ConfigFile}");
        }

        /// <summary>
        /// 停止Service
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(QuartzWorker)}.StopAsync...");
            if (Scheduler != null && !Scheduler.IsShutdown)
            {
                _ = Scheduler.Shutdown(cancellationToken);
                _FileProvider.Dispose();
                _FileProvider = null;
            }
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// 启动时候的
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Scheduler execute start");

                if (Scheduler == null)
                {
                    Scheduler = await this._schedulerFactory.GetScheduler(stoppingToken);
                }
                if (Scheduler.IsStarted)
                {
                    _logger.LogInformation("Scheduler is started...");
                    return;
                }
                Token = stoppingToken;
                // 监控文件变化
                if (string.Equals(_configuration["Quartz:Watch"], "true", StringComparison.OrdinalIgnoreCase))
                {
                    var fInfo = new FileInfo(_ConfigFile);
                    if (!fInfo.Exists)
                    {
                        _logger.LogWarning($"当前目录：{AppContext.BaseDirectory}");
                        throw new FileNotFoundException("缺失Quartz任务调度配置文件", Path.GetFullPath(_ConfigFile));
                    }
                    _logger.LogInformation("监控Quartz任务调度配置文件：" + fInfo.FullName);
                    if (_FileProvider != null)
                    {
                        _FileProvider.Dispose();
                        _FileProvider = null;
                    }
                    _FileProvider = new PhysicalFileProvider(fInfo.DirectoryName);
                    _ = ChangeToken.OnChange(() => _FileProvider.Watch(fInfo.Name),
                        async () => await ReloadJobs(stoppingToken, LoadConfig<ScheduleModel>(_FileProvider, fInfo.Name)));
                }
                var config = LoadConfig<ScheduleModel>(_ConfigFile);
                await ReloadJobs(stoppingToken, config);

                _logger.LogInformation("Scheduler execute end");
            }
            catch (Exception ex)
            {
                _logger.LogError("exception when ExecuteAsync ::" + ex.ToString());
            }
        }

        /// <summary>
        /// 重新加载调度任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <param name="configFile">配置文件路径，支持.xml和.json</param>
        /// <returns></returns>
        private async Task ReloadJobs(CancellationToken stoppingToken, string configFile)
        {
            _logger.LogInformation("ReloadJobs, configFile:" + configFile);
            /// 获取Quartz配置，支持.xml和.json
            ScheduleModel scheduleConfig = LoadConfig<ScheduleModel>(configFile);
            if (scheduleConfig == null || scheduleConfig.GroupList?.Any(g => g.JobList?.Length > 0) != true)
            {
                _logger.LogInformation("GroupList is null");
                return;
            }

            foreach (var group in scheduleConfig.GroupList)
            {
                if (group == null || group.JobList == null || group.JobList.Length == 0)
                {
                    _logger.LogInformation($"Group or JobList is null: {group?.Name}");
                    continue;
                }

                foreach (var jobInfo in group.JobList)
                {
                    _logger.LogInformation($"load job {group.Name}-{jobInfo.Name}");
                    bool disabled = group.Disabled || jobInfo.Disabled;
                    jobInfo.Group = group.Name;
                    JobKey jobKey = new JobKey(jobInfo.Name, jobInfo.Group);
                    var job = await Scheduler.GetJobDetail(jobKey, stoppingToken);
                    if (job == null)
                    {
                        if (disabled) { continue; }
                        job = CreateJobDetail(jobInfo, jobKey);
                        var trigger = CreateTrigger(jobInfo);
                        var offset = await Scheduler.ScheduleJob(job, trigger, stoppingToken);
                        _logger.LogInformation($"Group [{jobInfo.Group}], Job [{jobInfo.Name}]-" +
                            $"[{(jobInfo.AllowConcurrentExecution() ? "Allow" : "Disallow") + "ConcurrentExecution"}]" +
                            $" is added to scheduler, next execution: {offset.ToLocalTime()}");
                    }
                    else
                    {
                        if (disabled)
                        {
                            _ = Scheduler.DeleteJob(jobKey, stoppingToken);
                            _logger.LogWarning($"Group [{jobInfo.Group}], Job [{jobInfo.Name}] is set to disabled, " +
                                $"now remove it.");
                        }
                        else
                        {
                            _logger.LogInformation($"Group [{jobInfo.Group}], Job [{jobInfo.Name}] already exists, " +
                                $"so does not perform it, and directly skip");
                        }
                    }
                }
            }

            _ = Scheduler.Start(stoppingToken);
        }

        /// <summary>
        /// 重新加载调度任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <param name="scheduleConfig">作业调度配置</param>
        /// <returns></returns>
        private async Task ReloadJobs(CancellationToken stoppingToken, ScheduleModel scheduleConfig)
        {
            if (scheduleConfig == null || scheduleConfig.GroupList?.Any(g => g.JobList?.Length > 0) != true)
            {
                _logger.LogInformation("GroupList is null");
                return;
            }

            foreach (var group in scheduleConfig.GroupList)
            {
                if (group == null || group.JobList == null || group.JobList.Length == 0)
                {
                    _logger.LogInformation($"Group or JobList is null: {group?.Name}");
                    continue;
                }

                foreach (var jobInfo in group.JobList)
                {
                    _logger.LogInformation($"load job {group.Name}-{jobInfo.Name}");
                    bool disabled = group.Disabled || jobInfo.Disabled;
                    jobInfo.Group = group.Name;
                    JobKey jobKey = new JobKey(jobInfo.Name, jobInfo.Group);
                    var job = await Scheduler.GetJobDetail(jobKey, stoppingToken);
                    if (job == null)
                    {
                        if (disabled) { continue; }
                        job = CreateJobDetail(jobInfo, jobKey);
                        var trigger = CreateTrigger(jobInfo);
                        var offset = await Scheduler.ScheduleJob(job, trigger, stoppingToken);
                        _logger.LogInformation($"Group [{jobInfo.Group}], Job [{jobInfo.Name}]-" +
                            $"[{(jobInfo.AllowConcurrentExecution() ? "Allow" : "Disallow") + "ConcurrentExecution"}]" +
                            $" is added to scheduler, next execution: {offset.ToLocalTime()}");
                    }
                    else
                    {
                        if (disabled)
                        {
                            _ = Scheduler.DeleteJob(jobKey, stoppingToken);
                            _logger.LogWarning($"Group [{jobInfo.Group}], Job [{jobInfo.Name}] is set to disabled, " +
                                $"now remove it.");
                        }
                        else
                        {
                            var newTrigger = CreateTrigger(jobInfo);
                            var trigger = await Scheduler.GetTrigger(newTrigger.Key);
                            if (trigger is CronTriggerImpl && (trigger as CronTriggerImpl).CronExpressionString != jobInfo.Cron)
                            {
                                await Scheduler.RescheduleJob(newTrigger.Key, newTrigger, stoppingToken);
                                _logger.LogInformation($"Group [{jobInfo.Group}], Job [{jobInfo.Name}] already exists, update it`s cron");
                            }
                            else
                            {
                                _logger.LogInformation($"Group [{jobInfo.Group}], Job [{jobInfo.Name}] already exists, skiped ");
                            }
                        }
                    }
                }
            }

            _ = Scheduler.Start(stoppingToken);
        }

        /// <summary>
        /// 创建调度任务
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        private static IJobDetail CreateJobDetail(JobModel jobInfo, JobKey jobKey)
        {
            var job = (jobInfo.AllowConcurrentExecution() ? JobBuilder.Create<ConcurrentJob>() : JobBuilder.Create<BlockedJob>())
                        .WithIdentity(jobKey)
                        .UsingJobData("group", jobInfo.Group)
                        .UsingJobData("name", jobInfo.Name)
                        .UsingJobData("dllName", jobInfo.DllName)
                        .UsingJobData("className", jobInfo.ClassName)
                        .UsingJobData("methodName", jobInfo.MethodName)
                        .Build();
            return job;
        }

        /// <summary>
        /// 创建计时器
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        private static ITrigger CreateTrigger(JobModel jobInfo)
        {
            var trigger = TriggerBuilder.Create()
                            .WithIdentity(jobInfo.Name, jobInfo.Group)
                            .WithCronSchedule(jobInfo.Cron)
                            .StartNow()
                            .Build();
            return trigger;
        }

        /// <summary>
        /// 创建计时器
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        private static ITrigger UpdateTrigger(JobModel jobInfo)
        {

            var trigger = TriggerBuilder.Create()
                            .WithIdentity(jobInfo.Name, jobInfo.Group)
                            .WithCronSchedule(jobInfo.Cron)
                            .StartNow()
                            .Build();
            return trigger;
        }

        /// <summary>
        /// 解析配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private static T LoadConfig<T>(string fullName)
        {
            var fInfo = new FileInfo(fullName);
            if (!fInfo.Exists)
            {
                throw new FileNotFoundException("反序列化配置文件时产生异常，配置文件不存在", fullName);
            }
            using FileStream fs = fInfo.OpenRead();
            return DeserializeConfig<T>(fs, fullName);
        }

        /// <summary>
        /// 需要使用 IFileProvider.GetFileInfo(fileName).CreateReadStream() 方法读取文件内容，
        /// 因为FileInfo.OpenRead()会发生文件占用异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileProvider"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static T LoadConfig<T>(IFileProvider fileProvider, string fileName)
        {
            using Stream stream = fileProvider.GetFileInfo(fileName).CreateReadStream();
            return DeserializeConfig<T>(stream, fileName);
        }

        /// <summary>
        /// 反序列化配置文件内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static T DeserializeConfig<T>(Stream stream, string fileName)
        {
            using StreamReader sr = new StreamReader(stream);
            if (fileName.ToLower().EndsWith(".xml"))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(sr);
            }
            else if (fileName.ToLower().EndsWith(".json"))
            {
                string config = sr.ReadToEnd();
                return JsonSerializer.Deserialize<T>(config);
            }
            else
            {
                throw new FileLoadException("反序列化配置文件时产生异常：不支持该文件类型", fileName);
            }
        }
    }
}