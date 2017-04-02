using System;
using System.Windows;

namespace LOUV.Torp.Monitor
{
    public static class Splasher
    {
        /// <summary>
        /// 
        /// </summary>
        private static Window mSplash;

        private static int ExitNo = 0;//0,正常，其他自定义
        /// <summary>
        /// Get or set the splash screen window
        /// </summary>
        public static Window Splash
        {
            get
            {
                return mSplash;
            }
            set
            {
                mSplash = value;
            }
        }

        /// <summary>
        /// Show splash screen
        /// </summary>
        public static void ShowSplash()
        {
            if (mSplash != null)
            {
                mSplash.Show();
            }
        }
        /// <summary>
        /// Close splash screen
        /// </summary>
        public static void CloseSplash(int exit=0)
        {
            if (mSplash != null)
            {
                mSplash.Hide();

                
            }
            ExitNo = exit;
        }
    }
}
