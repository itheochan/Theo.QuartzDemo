/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Models
* FileName			: JobModel.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 11:00:44
* --------------------------------------------------
*/
using System.Xml.Serialization;

namespace Theo.TaskScheduler.Models
{
    ///<summary>
    /// 计划任务job模型
    ///</summary>
    public class JobModel
    {
        [XmlIgnore]
        public string Group { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the cron.
        /// </summary>
        [XmlAttribute("cron")]
        public string Cron { get; set; }

        /// <summary>
        /// Gets or sets the dllName
        /// </summary>
        [XmlAttribute("dllName")]
        public string DllName { get; set; }

        /// <summary>
        /// Gets or sets the className
        /// </summary>
        [XmlAttribute("className")]
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the methodName
        /// </summary>
        [XmlAttribute("methodName")]
        public string MethodName { get; set; }

        /// <summary>
        /// 是否允许并发调度
        /// </summary>
        [XmlAttribute("allowConcurrent")]
        public string AllowConcurrent { get; set; }

        /// <summary>
        /// 是否允许并发调度
        /// </summary>
        /// <returns></returns>
        public bool AllowConcurrentExecution()
        {
            if (bool.TryParse(AllowConcurrent, out bool result))
            {
                return result;
            }
            return false;
        }

        /// <summary>
        /// 禁用
        /// </summary>
        [XmlAttribute("disabled")]
        public bool Disabled { get; set; }
    }
}