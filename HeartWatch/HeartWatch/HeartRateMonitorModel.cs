using System;
using System.Threading.Tasks;
using Tizen;
using Tizen.Security;
using Tizen.Sensor;
using HRM = Tizen.Sensor.HeartRateMonitor;

namespace HeartWatch
{
    /// <summary>
    /// HeartRateMonitorModel class.
    /// Provides methods that allow the application to use the Tizen Sensor API.
    /// Implements IHeartRateMonitorModel interface to be available
    /// from portable part of the application source code.
    /// </summary>
    public class HeartRateMonitorModel
    {
        #region fields

        /// <summary>
        /// An instance of the HeartRateMonitor class provided by the Tizen Sensor API.
        /// </summary>
        private HRM heartRateMonitor;

        /// <summary>
        /// Number representing value of the current heart rate.
        /// </summary>
        private int currentHeartRate;

        /// <summary>
        /// The check privileges task.
        /// </summary>
        private TaskCompletionSource<bool> checkPrivilegesTask;

        #endregion

        #region properties

        /// <summary>
        /// HeartRateMonitorDataChanged event.
        /// Notifies UI about heart rate value update.
        /// </summary>
        public event EventHandler HeartRateMonitorDataChanged;

        /// <summary>
        /// HeartRateSensorNotSupported event.
        /// Notifies application about lack of heart rate sensor.
        /// </summary>
        public event EventHandler HeartRateSensorNotSupported;

        /// <summary>
        /// Healthinfo privilege key.
        /// </summary>
        private const string HEALTHINFO_PRIVILEGE = "http://tizen.org/privilege/healthinfo";


        public bool IsInitialized { get; private set; }
        public bool IsMeasuring { get; private set; }
        #endregion

        #region methods

        /// <summary>
        /// Initializes HeartRateMonitorModel class.
        /// Invokes HeartRateSensorNotSupported event if heart rate sensor is not supported.
        /// </summary>
        public void Init()
        {
            try
            {
                heartRateMonitor = new HRM
                {
                    Interval = 1000
                };

                heartRateMonitor.DataUpdated += OnDataUpdated;
                IsInitialized = true;
            }
            catch (Exception)
            {
                HeartRateSensorNotSupported?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Returns current heart rate value provided by the Tizen Sensor API.
        /// </summary>
        /// <returns>Current heart rate value provided by the Tizen Sensor API.</returns>
        public int GetHeartRate()
        {
            return currentHeartRate;
        }

        /// <summary>
        /// Starts notification about changes of heart rate value.
        /// </summary>
        public void StartHeartRateMonitor()
        {
            heartRateMonitor.Start();
            IsMeasuring = true;
            Log.Debug(App.LOG_TAG, "Start monitoring");
        }

        /// <summary>
        /// Stops notification about changes of heart rate value.
        /// </summary>
        public void StopHeartRateMonitor()
        {
            heartRateMonitor.Stop();
            IsMeasuring = false;
            Log.Debug(App.LOG_TAG, "Stop monitoring");
        }

        /// <summary>
        /// Handles "DataUpdated" event of the HeartRateMonitor object provided by the Tizen Sensor API.
        /// Saves current heart rate value in the _currentHeartRate field.
        /// Invokes "HeartRateMonitorDataChanged" event.
        /// </summary>
        /// <param name="sender">Object firing the event.</param>
        /// <param name="e">An instance of the HeartRateMonitorDataUpdatedEventArgs class providing detailed information about the event.</param>
        private void OnDataUpdated(object sender, HeartRateMonitorDataUpdatedEventArgs e)
        {
            Log.Debug(App.LOG_TAG, $"Rate:{e.HeartRate}");
            currentHeartRate = e.HeartRate;
            HeartRateMonitorDataChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Handles privilege request response from the privacy privilege manager.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="requestResponseEventArgs">Event arguments.</param>
        private void PrivilegeManagerOnResponseFetched(object sender,
            RequestResponseEventArgs requestResponseEventArgs)
        {
            if (requestResponseEventArgs.cause == CallCause.Answer)
            {
                checkPrivilegesTask.SetResult(requestResponseEventArgs.result == RequestResult.AllowForever);
            }
            else
            {
                Log.Error(App.LOG_TAG, "Error occurred during requesting permission");
                checkPrivilegesTask.SetResult(false);
            }
        }

        /// <summary>
        /// Returns true if all required privileges are granted, false otherwise.
        /// </summary>
        /// <returns>Task with check result.</returns>
        public async Task<bool> CheckPrivileges()
        {
            CheckResult result = PrivacyPrivilegeManager.CheckPermission(HEALTHINFO_PRIVILEGE);

            switch (result)
            {
                case CheckResult.Allow:
                    return true;
                case CheckResult.Deny:
                    return false;
                case CheckResult.Ask:
                    PrivacyPrivilegeManager.ResponseContext context = null;
                    PrivacyPrivilegeManager.GetResponseContext(HEALTHINFO_PRIVILEGE)
                        .TryGetTarget(out context);

                    if (context == null)
                    {
                        Log.Error(App.LOG_TAG, "Unable to get privilege response context");
                        return false;
                    }

                    checkPrivilegesTask = new TaskCompletionSource<bool>();

                    context.ResponseFetched += PrivilegeManagerOnResponseFetched;

                    PrivacyPrivilegeManager.RequestPermission(HEALTHINFO_PRIVILEGE);
                    return await checkPrivilegesTask.Task;
                default:
                    return false;
            }
        }

        #endregion
    }
}
