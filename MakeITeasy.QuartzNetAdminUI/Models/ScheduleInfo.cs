using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeITeasy.QuartzNetAdminUI.Models
{
    public class ScheduleInfo
    {
        public List<ScheduleGroupInfo> Groups { get; set; } = new List<ScheduleGroupInfo>();
    }
}
