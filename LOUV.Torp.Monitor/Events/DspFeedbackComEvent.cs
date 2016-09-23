using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.MonP;

namespace LOUV.Torp.Monitor.Events
{
    public class DspFeedbackComEvent
    {
        private ModuleType _type;
        private byte[] _data;
        public DspFeedbackComEvent(ModuleType type, byte[] dt)
        {
            _type = type;
            _data = dt;
        }

        public ModuleType Type
        {
            get { return _type; }
        }

        public byte[] Data
        {
            get { return _data; }
        }
    }
}
