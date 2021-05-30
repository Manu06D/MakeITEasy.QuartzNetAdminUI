using System;

namespace MakeITeasy.QuartzNetAdminUI.Models
{
    public class TriggerInfo
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Description { get; internal set; }
        public DateTimeOffset LastExecution { get; internal set; }
        public DateTimeOffset NextExecution { get; internal set; }
        public TimeSpan? Interval { get; internal set; }
        public Status Status { get; internal set; }
    }
}