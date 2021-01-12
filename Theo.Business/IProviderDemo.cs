using System;

namespace Theo.Business
{
    public interface IProviderDemo
    {

        /// <summary>
        /// 模拟提供发邮件服务
        /// </summary>
        /// <param name="param">参数</param>
        void AutoSendMail(string param);

        /// <summary>
        /// 模拟提供发短信服务
        /// </summary>
        /// <param name="param">参数</param>
        void AutoSendSMS(string param);
    }
}
