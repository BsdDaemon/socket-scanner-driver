using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SocketScanner;

namespace QuickStart
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : ScannerActivity
    {
        private TextView _displayText;

        protected override void OnBarcodeScanned(object sender, EventArgs e)
        {
            ViewText(Scanner.LastScan);
        }

        protected override void OnScannerConnected(object sender, EventArgs e)
        {
            ViewText("Scanner is connected!");
        }

        protected override void OnScannerDisconnected(object sender, EventArgs e)
        {
            ViewText("Scanner is not connected.");
        }
        protected override void HardwareError(object sender, EventArgs e) { }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            _displayText = FindViewById<TextView>(Resource.Id.textView1);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            Scanner.SendScannerDisable(!Scanner.Disabled);
            if (Scanner.Disabled)
            {
                ShowSnackbar(sender, "Scanner is disabled.");
                Scanner.SendResponse(Middleware.Responses.Failure);
            }
            else
            {
                ShowSnackbar(sender, "Scanner is enabled.");
                Scanner.SendResponse(Middleware.Responses.Success);
            }
        }

        private void ShowSnackbar(object sender, string message)
        {
            View view = (View)sender;
            Snackbar.Make(view, message, Snackbar.LengthLong).Show();
        }

        private void ViewText(string value)
        {
            RunOnUiThread(() =>
            {
                _displayText.SetText(value, TextView.BufferType.Normal);
            });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

