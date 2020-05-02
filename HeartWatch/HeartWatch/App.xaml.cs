using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeartWatch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public const string LOG_TAG = "HeartWatch";

        public App()
        {
            InitializeComponent();

            MainPage = new HeartWatch.MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            //MessagingCenter.Send(this, "sleep");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
