using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tizen;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeartWatch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : CirclePage
    {
        private HeartRateMonitorModel heart;
        private HttpClient http;

        public MainPage()
        {
            InitializeComponent();

            http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(0.5)
            };

            heart = new HeartRateMonitorModel();
            heart.HeartRateMonitorDataChanged += Heart_HeartRateMonitorDataChanged;
            heart.HeartRateSensorNotSupported += Heart_HeartRateSensorNotSupported;
        }

        private void Heart_HeartRateSensorNotSupported(object sender, EventArgs e)
        {
            // close the application
            Log.Warn(App.LOG_TAG, "Heartrate sensor not supported. Exiting..");
            Tizen.Applications.Application.Current.Exit();
        }

        private void Heart_HeartRateMonitorDataChanged(object sender, EventArgs e)
        {
            var h = heart.GetHeartRate();
            hlabel.Text = $"HB: {h}";
            Send(h);
        }

        private void Send(int heartRate)
        {
            Task.Run(async () =>
            {
                try
                {
                    await http.PostAsync("http://DanielsComputer:8880", new StringContent(heartRate.ToString()));
                }
                catch (Exception ex)
                {
                    Log.Warn(App.LOG_TAG, $"Error while sending request: {ex.Message}");
                }
            });
        }

        private async void OnActionButtonClicked(object sender, EventArgs e)
        {
            if (!heart.IsInitialized)
            {
                if (!await heart.CheckPrivileges())
                    return;

                heart.Init();

                // stop the measurement when the application goes background
                //MessagingCenter.Subscribe<App>(this, "sleep", (s) => { if (heart.IsMeasuring) { Stop(); } });
            }

            if (heart.IsMeasuring)
                Stop();
            else
                Start();
        }

        private void Start()
        {
            //Tizen.System.Power.RequestLock(Tizen.System.PowerLock.Cpu, (int)TimeSpan.FromDays(1).TotalMilliseconds);
            heart.StartHeartRateMonitor();
            actionButton.Text = "STOP";
        }

        private void Stop()
        {
            //Tizen.System.Power.ReleaseLock(Tizen.System.PowerLock.Cpu);
            heart.StopHeartRateMonitor();
            actionButton.Text = "START";
        }
    }
}