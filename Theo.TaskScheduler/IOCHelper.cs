/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler
* FileName			: IOCHelper.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 14:03:18
* --------------------------------------------------
*/
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Theo.Business;

namespace Theo.TaskScheduler
{
    ///<summary>
    /// IOCHelper
    ///</summary>
    public static class IOCHelper
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 注入逻辑
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection InjectDependencies(this IServiceCollection services)
        {
            services.AddScoped<IProviderDemo, ProviderDemo>();
            ServiceProvider = services.BuildServiceProvider();
            return services;
        }

        public static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }
        public static object GetService(Type t)
        {
            return ServiceProvider.GetService(t);
        }
    }
}