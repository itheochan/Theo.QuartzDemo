using Microsoft.Extensions.Logging;
using System;

namespace Theo.Business
{
    ///<summary>
    /// 模拟业务逻辑
    ///</summary>
    public class ProviderDemo : IProviderDemo
    {
        private readonly ILogger<ProviderDemo> _logger;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logger"></param>
        public ProviderDemo(ILogger<ProviderDemo> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 模拟提供发邮件服务
        /// </summary>
        /// <param name="param">参数</param>
        public void AutoSendMail(string param)
        {
            _logger.LogError($"[{DateTimeOffset.Now:HH:mm:ss.fff}]\t{nameof(AutoSendMail)}\tparam:{param}");
        }

        /// <summary>
        /// 模拟提供发短信服务
        /// </summary>
        /// <param name="param">参数</param>
        public void AutoSendSMS(string param)
        {
            _logger.LogInformation($"[{DateTimeOffset.Now:HH:mm:ss.fff}]\t{nameof(AutoSendSMS)}\tparam:{param}");
        }
    }
}