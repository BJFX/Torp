using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.Monitor.Events;
using TinyMetroWpfLibrary.ViewModel;
using TinyMetroWpfLibrary.EventAggregation;

namespace LOUV.Torp.Monitor.ViewModel
{
    public class HomePageViewModel : ViewModelBase, IHandleMessage<ShowAboutSlide>
    {
        public override void Initialize()
        {
            
        }

        public override void InitializePage(object extraData)
        {
            AboutVisibility = false;
        }
        public bool AboutVisibility
        {
            get { return GetPropertyValue(() => AboutVisibility); }
            set { SetPropertyValue(() => AboutVisibility, value); }
        }

        public void Handle(ShowAboutSlide message)
        {
            AboutVisibility = true;
        }
    }
}
