using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoonieBear.DeckUnit.ACMP;

namespace LOUV.Torp.Monitor.Events
{
    public class USBLEvent
    {
        public Sysposition Position { get; private set; }

        public USBLEvent(Sysposition pos)
        {
            Position = pos;
        }
    }
}
