using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
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
            hlabel.Text = $"{h}";
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
            // crashes
            //Tizen.System.Power.RequestLock(Tizen.System.PowerLock.DisplayDim, 0);
            //Tizen.System.Power.RequestLock(Tizen.System.PowerLock.DisplayDim, (int)TimeSpan.FromHours(4).TotalMilliseconds);

            int ret = DevicePowerRequestLock(1, 0); // type : CPU:0, DisplayNormal:1, DisplayDim:2
            Log.Debug(App.LOG_TAG, $"PowerLock: {ret}");

            heart.StartHeartRateMonitor();
            actionButton.Text = "STOP";
        }

        private void Stop()
        {
            // crashes
            //Tizen.System.Power.ReleaseLock(Tizen.System.PowerLock.DisplayDim);

            int ret = DevicePowerReleaseLock(1);    // type : CPU:0, DisplayNormal:1, DisplayDim:2
            Log.Debug(App.LOG_TAG, $"PowerReleaseLock: {ret}");

            heart.StopHeartRateMonitor();
            actionButton.Text = "START";
        }

        [DllImport("libcapi-system-device.so.0", EntryPoint = "device_power_request_lock", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DevicePowerRequestLock(int type, int timeout_ms);

        [DllImport("libcapi-system-device.so.0", EntryPoint = "device_power_release_lock", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DevicePowerReleaseLock(int type);
    }
}