using System.Collections.Generic;

namespace MakeITeasy.QuartzNetAdminUI.Models
{
    public class ScheduleGroupInfo
    {
        public string Name { get; set; }

        public List<TriggerInfo> Jobs { get; set; } = new List<TriggerInfo>();
    }
}