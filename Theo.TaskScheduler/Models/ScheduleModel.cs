/*
* --------------------------------------------------
* Copyright(C)		: 
* Namespace			: Theo.TaskScheduler.Models
* FileName			: ScheduleModel.cs
* CLR Version		: 4.0.30319.42000
* Author			: Theo
* CreateTime		: 2021/1/11 11:03:24
* --------------------------------------------------
*/
using System.Xml.Serialization;

namespace Theo.TaskScheduler.Models
{
    /// <summary>
    /// 任务调度模型
    /// </summary>
    [XmlRoot("schedulers", IsNullable = false)]
    public class ScheduleModel
    {
        [XmlElement("group")]
        public JobGroupModel[] GroupList { get; set; }
    }
}
