using LOUV.Torp.BaseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOUV.Torp.Monitor.Events
{
    public class RefreshTargetEvent
    {
        public Target TargetPos { get; set; }
        public RefreshTargetEvent(Target pos)
        {
            TargetPos = pos;
        }
    }
}
