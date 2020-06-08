using System;

using Android.OS;
using Android.Support.V7.App;

namespace SocketScanner
{
    public abstract class ScannerActivity : AppCompatActivity
    {
        protected Middleware Scanner;

        protected abstract void OnBarcodeScanned(object sender, EventArgs e);
        protected abstract void OnScannerConnected(object sender, EventArgs e);
        protected abstract void OnScannerDisconnected(object sender, EventArgs e);
        protected abstract void HardwareError(object sender, EventArgs e);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SendBroadcast(Middleware.CaptureService());
        }

        protected override void OnResume()
        {
            base.OnResume();

            Scanner = new Middleware();
            Scanner.BarcodeScanned += OnBarcodeScanned;
            Scanner.HardwareConnected += OnScannerConnected;
            Scanner.HardwareNoConnection += OnScannerDisconnected;
            Scanner.Exception += HardwareError;

            SendBroadcast(Middleware.CaptureService());
        }

        protected override void OnPause()
        {
            base.OnPause();

            Scanner.Dispose();
        }
    }
}