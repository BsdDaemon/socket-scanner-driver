using System;
using System.Threading.Tasks;
using SocketMobile.Capture;

using Android.Content;

namespace SocketScanner
{
    public class Middleware : IDisposable
    {
        private readonly CaptureHelper _driver;
        private CaptureHelperDevice _device;
        private string _buffer;
        private bool _disabled;

        protected bool Disposed { get; private set; }

        internal const string CAPTURE_SERVICE_ACTION = "com.socketmobile.capture.START_SERVICE";
        internal const string CAPTURE_SERVICE_PACKAGE = "com.socketmobile.companion";
        internal const string CAPTURE_SERVICE_START = "com.socketmobile.capture.StartService";

        public event EventHandler BarcodeScanned;
        public event EventHandler HardwareConnected;
        public event EventHandler HardwareNoConnection;
        public event EventHandler Exception;

        public enum Responses
        {
            None = 0,
            Success = 1,
            Failure = 2
        }

        public string LastScan => _buffer;
        public bool Connected => _device?.HasOwnership ?? false;
        public bool Disabled => _disabled;

        public bool RapidScanning = false;


        public static Intent CaptureService()
        {
            //Register with companion app
            Intent intent = new Intent(CAPTURE_SERVICE_ACTION);
            intent.SetComponent(new ComponentName(CAPTURE_SERVICE_PACKAGE, CAPTURE_SERVICE_START));
            intent.AddFlags(ActivityFlags.ReceiverForeground);
            return intent;
        }

        public Middleware()
        {
            //Initialize SDK main class and hook in events.
            _driver = new CaptureHelper
            {
                DoNotUseWebSocket = true
            };
            _driver.DecodedData += DecodedData;
            _driver.DeviceArrival += DeviceHandshake;
            _driver.DeviceOwnershipChange += DeviceNoConnection;
            _driver.DeviceRemoval += DeviceNoConnection;
            _driver.Errors += DeviceThrewError;

            //Connect to hardware
            Handshake();
        }

        public void Dispose()
        {
            _driver.CloseAsync();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SendResponse(Responses tone)
        {
            int beep;
            int led;
            int rumble;

            if (_device == null)
            {
                throw new System.Exception("SocketMobile is not connected.");
            }

            switch (tone)
            {
                case Responses.None:
                    beep = ICaptureProperty.Values.DataConfirmation.kBeepNone;
                    led = ICaptureProperty.Values.DataConfirmation.kLedNone;
                    rumble = ICaptureProperty.Values.DataConfirmation.kRumbleNone;
                    break;
                case Responses.Success:
                    beep = ICaptureProperty.Values.DataConfirmation.kBeepGood;
                    led = ICaptureProperty.Values.DataConfirmation.kLedGreen;
                    rumble = ICaptureProperty.Values.DataConfirmation.kRumbleGood;
                    break;
                case Responses.Failure:
                    beep = ICaptureProperty.Values.DataConfirmation.kBeepBad;
                    led = ICaptureProperty.Values.DataConfirmation.kLedRed;
                    rumble = ICaptureProperty.Values.DataConfirmation.kRumbleBad;
                    break;
                default:
                    throw new System.Exception("Response is not a valid response.");
            }

            _device.SetDataConfirmationAsync(beep, led, rumble);
            if (RapidScanning) _device.SetTriggerStartAsync();
        }

        public void SendScannerScan(bool on = true)
        {
            //Control the scanners laser LEDs
            if (on)
            {
                _device.SetTriggerStartAsync();
            }
            else
            {
                _device.SetTriggerStopAsync();
            }
        }

        public void SendScannerDisable(bool disabled = true)
        {
            //Enable and disable scanning, keep state of scanner
            if (disabled)
            {
                _device.SetTriggerDisableAsync();
            }
            else
            {
                _device.SetTriggerEnableAsync();
            }
            _disabled = disabled;
        }

        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }

        internal async void Handshake()
        {
            int tries = 0;
            while (await RegisterTask() == SktErrors.ESKT_UNABLEOPENDEVICE)
            {
                tries++;
                if (tries >= 50)
                {
                    //Throw hook HardwareNoConnection and die
                    HardwareNoConnection?.Invoke(this, null);
                    break;
                }
                await Task.Delay(500);
            }
        }

        private async Task<long> RegisterTask()
        {
            //Sends plaintext data, AndroidManifest.xml needs android:usesCleartextTraffic="true"
            long result = await _driver.OpenAsync("android:com.socketmobile.simplecapturedemo",
                "bb57d8e1-f911-47ba-b510-693be162686a",
                "MC4CFQC76uXj3J36NLgYLaZP7YevE/A4pgIVAPqOydqV4fv4Gh5v01DJGbaSbY61");

            return result;
        }

        private void DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            //Remove delay issues, run this first
            SendResponse(Responses.None);

            //Decouple DecodedData
            _buffer = e.DecodedData.DataToUTF8String;

            BarcodeScanned?.Invoke(this, e);
        }

        private void DeviceHandshake(object sender, CaptureHelper.DeviceArgs e)
        {
            //Decouple CaptureDevice
            _device = e.CaptureDevice;

            //Keep device disabled if it is restarted
            if (Disabled) _device.SetTriggerDisableAsync();

            HardwareConnected?.Invoke(this, e);
        }

        private void DeviceNoConnection(object sender, CaptureHelper.DeviceArgs e)
        {
            //Decouple CaptureDevice
            if (e.CaptureDevice.HasOwnership) return;
            _device = null;

            HardwareNoConnection?.Invoke(this, e);
        }

        private void DeviceThrewError(object sender, EventArgs e)
        {
            Exception?.Invoke(this, e);
        }

        ~Middleware()
        {
            Dispose(false);
        }
    }
}