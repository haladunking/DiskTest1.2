using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskTest
{
    public delegate void NotifyEventHandler(object sender);
    public abstract class Blog_DiskSetting
    {
        public NotifyEventHandler NotifyEvent;
        public int Percent_Now { get; set; }
    }
}
