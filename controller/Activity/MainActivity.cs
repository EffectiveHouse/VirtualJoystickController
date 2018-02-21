using System;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Bluetooth;
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Support.Design.Widget;

using GR.Net.Maroulis.Library;

namespace controller
{
    [Activity(Label = "EDURON", Name = "controller.MainActivity", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize,
        Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.Landscape, Icon = "@drawable/Logo")]
    public class MainActivity : AppCompatActivity
    {
        private BluetoothAdapter mobileAdapter = null;
        private static int REQUEST_ENABLE_BLUETOOTH = 99;
        private static int NO_ADAPTER = 11;
        private static int NO_PARIED_DEVICE = 12;
        private static int CAN_NOT_FIND_TARGET = 13;
        private static int DEVICE_STATE;
        private static string deviceName = "EDURON";

        GattServices gattServices;
        EasySplashScreen config;
        public static View view = null;

        private void PopupSnackBar(int ResourceId)
        {
            if (MainActivity.view != null)
            {
                Snackbar
                    .Make(MainActivity.view, ResourceId, Snackbar.LengthIndefinite)
                    .SetAction("OK", (v) => { })
                    .Show();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                config = new EasySplashScreen(this)
                    .WithFullScreen()
                    .WithBackgroundColor(Color.ParseColor("#ffffff"))
                    .WithLogo(Resource.Drawable.SplashStart_)
                    .WithHeaderText("Welcome Controller")
                    .WithFooterText("Copyright 2018")
                    .WithBeforeLogoText("DROGEN dev")
                    .WithAfterLogoText("Dual Shork & Android Controller");
                //.WithTargetActivity(Java.Lang.Class.FromType(typeof(controller.ControllerActivity)))

                //Set Text Color
                config.HeaderTextView.SetTextColor(Color.Black);
                config.FooterTextView.SetTextColor(Color.Black);
                config.BeforeLogoTextView.SetTextColor(Color.Black);
                config.AfterLogoTextView.SetTextColor(Color.Black);

                //Create View
                view = config.Create();

                //Set Content View
                SetContentView(view);
            }
            catch (Exception) { }

            //Bluetooth action
            mobileAdapter = BluetoothAdapter.DefaultAdapter;

            if (mobileAdapter == null)
            {
                //Device does not support Bluetooth
                DEVICE_STATE = NO_ADAPTER;
            }

            if (!mobileAdapter.IsEnabled)
            {
                // Bluetooth should be turn on
                Intent enableIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableIntent, REQUEST_ENABLE_BLUETOOTH);
            }
            else
            {
                var pairedDeivces = mobileAdapter.BondedDevices;
                int deviceCount = 0;

                if (pairedDeivces.Count > 0)
                {
                    foreach (var device in pairedDeivces)
                    {
                        if (device.Name == deviceName)
                        {
                            gattServices = new GattServices();
                            gattServices.ConnectGatt(device);
                            break;
                        }
                        else
                        {
                            deviceCount++;
                        }
                    }

                    if (pairedDeivces.Count == deviceCount)
                        DEVICE_STATE = CAN_NOT_FIND_TARGET;
                }
                else
                {
                    // No paired device
                    DEVICE_STATE = NO_PARIED_DEVICE;
                }
            }

            if (DEVICE_STATE == NO_ADAPTER)
            {
                PopupSnackBar(Resource.String.no_adapter);
            }
            else if (DEVICE_STATE == CAN_NOT_FIND_TARGET)
            {
                PopupSnackBar(Resource.String.cannot_find_target);
            }
            else if (DEVICE_STATE == NO_PARIED_DEVICE)
            {
                PopupSnackBar(Resource.String.no_paired_device);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == REQUEST_ENABLE_BLUETOOTH) && (resultCode == Result.Ok))
            {
                mobileAdapter = BluetoothAdapter.DefaultAdapter;

                var pairedDeivces = mobileAdapter.BondedDevices;
                int deviceCount = 0;

                if (pairedDeivces.Count > 0)
                {
                    foreach (var device in pairedDeivces)
                    {
                        if (device.Name == deviceName)
                        {
                            //start GattService
                            gattServices = new GattServices();
                            gattServices.ConnectGatt(device);
                            break;
                        }
                        else
                        {
                            deviceCount++;
                        }
                    }

                    if (pairedDeivces.Count == deviceCount)
                        PopupSnackBar(Resource.String.cannot_find_target);
                }
                else
                {
                    //No paired device
                    //Snackbar first
                    PopupSnackBar(Resource.String.no_paired_device);
                }
            }
        }
    }
}



