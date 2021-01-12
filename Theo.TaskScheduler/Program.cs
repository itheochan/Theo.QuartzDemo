using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Theo.TaskScheduler.Quartz;

namespace Theo.TaskScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)//ָ��Ӧ�ó����Ŀ¼
                .UseWindowsService()   //֧��Windows����  ����ƽ̨�Զ�����
                .UseSystemd()          //֧��Linux�ػ����̣�����ƽ̨�Զ�����
                .ConfigureLogging(conf => conf.AddLog4Net("Configs\\log4net.config"))
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    var env = hostContext.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        config.AddJsonFile("appsettings.Development.json");
                    }
                    else
                    {
                        config.AddJsonFile("appsettings.json");
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IOCHelper.InjectDependencies(services);
                    services.AddHostedService<QuartzWorker>();
                });
        public static void LogMessage(string message)
        {
            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "applog.txt"));
            if (!file.Exists) using (var tmp = file.Create()) { }
            using FileStream fs = File.Open(file.FullName, FileMode.Append);
            using StreamWriter fw = new StreamWriter(fs);
            fw.WriteLine($"\n[{DateTimeOffset.Now}]\t{message}");
            fw.Flush();
        }
    }
}
