using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.BaseType;
using LOUV.Torp.MonProtocol;

namespace LOUV.Torp.Monitor.Events
{
    public class RefreshBuoyInfoEvent
    {
        public int _index = 0;//0,1,2,3

        public RefreshBuoyInfoEvent(int index)
        {
            _index = index;
        }
    }
    //public class RefreshBuoyRangeInfoEvent
    //{
    //    public LiteRange _infoBuoy;
    //    public int _index = 0;//0,1,2,3

    //    public RefreshBuoyRangeInfoEvent(LiteRange info, int index)
    //    {
    //        _infoBuoy = info;
    //        _index = index;
    //    }
    //}
    //public class RefreshBuoyTeleRangeEvent
    //{
    //    public TeleRange _infoBuoy;
    //    public int _index = 0;//0,1,2,3

    //    public RefreshBuoyTeleRangeEvent(TeleRange info, int index)
    //    {
    //        _infoBuoy = info;
    //        _index = index;
    //    }
    //}
}
