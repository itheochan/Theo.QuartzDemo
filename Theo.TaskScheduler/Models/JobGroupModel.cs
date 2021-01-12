/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Models
* FileName			: JobGroupModel.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 11:02:03
* --------------------------------------------------
*/
using System.Xml.Serialization;

namespace Theo.TaskScheduler.Models
{
    ///<summary>
    /// 计划任务job组模型
    ///</summary>
    public class JobGroupModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// 禁用
        /// </summary>
        [XmlAttribute("disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the job list.
        /// </summary>
        [XmlElement("job")]
        public JobModel[] JobList { get; set; }
    }
}
