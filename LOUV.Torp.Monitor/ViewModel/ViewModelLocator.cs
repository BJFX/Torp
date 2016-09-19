﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOUV.Torp.Monitor.ViewModel
{
    public class ViewModelLocator
    {
        public MainFrameViewModel _mainFrameViewModel;
        public HomePageViewModel _homwPageViewModel;
        public LiveCaptureViewModel _LiveCaptureViewModel;
        public GlobalSettingViewModel _GlobalSettingViewModel;
        /// <summary>
        /// Gets the MainFrame ViewModel
        /// </summary>
        public MainFrameViewModel MainFrameViewModel
        {
            get
            {
                // Creates the MainFrame ViewModel
                if (_mainFrameViewModel == null)
                {
                    _mainFrameViewModel = new MainFrameViewModel();
                    _mainFrameViewModel.Initialize();
                }
                return _mainFrameViewModel;
            }
        }
        public HomePageViewModel HomePageViewModel
        {
            get
            {
                // Creates the MainFrame ViewModel
                if (_homwPageViewModel == null)
                {
                    _homwPageViewModel = new HomePageViewModel();
                    _homwPageViewModel.Initialize();
                }
                return _homwPageViewModel;
            }
        }
        public LiveCaptureViewModel LiveCaptureViewModel
        {
            get
            {
                // Creates the MainFrame ViewModel
                if (_LiveCaptureViewModel == null)
                {
                    _LiveCaptureViewModel = new LiveCaptureViewModel();
                    _LiveCaptureViewModel.Initialize();
                }
                return _LiveCaptureViewModel;
            }
        }
        public GlobalSettingViewModel GlobalSettingViewModel
        {
            get
            {
                // Creates the MainFrame ViewModel
                if (_GlobalSettingViewModel == null)
                {
                    _GlobalSettingViewModel = new GlobalSettingViewModel();
                    _GlobalSettingViewModel.Initialize();
                }
                return _GlobalSettingViewModel;
            }
        }
    }
}
