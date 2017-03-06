using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using LOUV.Torp.Monitor.Events;
using TinyMetroWpfLibrary.ViewModel;
using TinyMetroWpfLibrary.EventAggregation;
using LOUV.Torp.Monitor.Core;
using LOUV.Torp.BaseType;

namespace LOUV.Torp.Monitor.ViewModel
{
    public class HomePageViewModel : ViewModelBase, IHandleMessage<ShowAboutSlide>, 
        IHandleMessage<RefreshBuoyInfoEvent>,
        IHandleMessage<SwitchMapModeEvent>
    {
        public override void Initialize()
        {
            ObjTarget = new Target();
            Buoy1 = new Buoy(1);
            Buoy2 = new Buoy(2);
            Buoy3 = new Buoy(3);
            Buoy4 = new Buoy(4);
        }

        public override void InitializePage(object extraData)
        {
            AboutVisibility = false;
            MapMode = 0;
        }

        private void RefreshTarget(Target target)
        {
            ObjTarget = target;
        }
        private void RefreshBuoy(int index,Buoy buoy)
        {
            switch (index)
            {
                case 0:
                    Buoy1 = buoy;
                    break;
                case 1:
                    Buoy2 = buoy;
                    break;
                case 2:
                    Buoy3 = buoy;
                    break;
                case 3:
                    Buoy4 = buoy;
                    break;
                default:
                    break;
            }
        }
        public bool AboutVisibility
        {
            get { return GetPropertyValue(() => AboutVisibility); }
            set { SetPropertyValue(() => AboutVisibility, value); }
        }
        public int MapMode//0:2D,1:3D
        {
            get { return GetPropertyValue(() => MapMode); }
            set { SetPropertyValue(() => MapMode, value); }
        }

        public Target ObjTarget
        {
            get { return GetPropertyValue(() => ObjTarget); }
            set { SetPropertyValue(() => ObjTarget, value); }
        }
        public Buoy Buoy1
        {
            get { return GetPropertyValue(() => Buoy1); }
            set { SetPropertyValue(() => Buoy1, value); }
        }
        public Buoy Buoy2
        {
            get { return GetPropertyValue(() => Buoy2); }
            set { SetPropertyValue(() => Buoy2, value); }
        }
        public Buoy Buoy3
        {
            get { return GetPropertyValue(() => Buoy3); }
            set { SetPropertyValue(() => Buoy3, value); }
        }
        public Buoy Buoy4
        {
            get { return GetPropertyValue(() => Buoy4); }
            set { SetPropertyValue(() => Buoy4, value); }
        }
        public void Handle(ShowAboutSlide message)
        {
            AboutVisibility = true;
        }

        public void Handle(RefreshBuoyInfoEvent message)
        {    
            if(UnitCore.Instance.Buoy.ContainsKey(message._index))
            {
                RefreshBuoy(message._index, (Buoy)UnitCore.Instance.Buoy[message._index]);
            }
        }

        public void Handle(SwitchMapModeEvent message)
        {
            if (MapMode == 0)
                MapMode = 1;
            else
            {
                MapMode = 0;
            }
        }
    }
}
