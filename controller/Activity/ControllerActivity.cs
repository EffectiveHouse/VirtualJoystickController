using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Runtime;
using Android.Content.PM;
using Android.Content;
using Android.Hardware.Input;

namespace controller
{
    [Activity(Label = "Virtual Controller Stick", ScreenOrientation = ScreenOrientation.Landscape, Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
         ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class ControllerActivity : Activity, InputManager.IInputDeviceListener
    {       
        private const short ARM_ON = 2000;
        private const short ARM_OFF = 1000;
        private const short FLIGHT_MODE_ANGLE = 1000;
        private const short FLIGHT_MODE_POSHOLD = 2000;
        private const int STICK_OUTPUT_MIN = 1000;
        private const int STICK_OUTPUT_MID = 1500;
        private const int STICK_OUTPUT_MAX = 2000;
        private const string LEFT_PAD_NAME = "LeftStickPad";
        private const string RIGHT_PAD_NAME = "RightStickPad";

        private int stickModeID;
        private int flightModeID;
        //Controller data : @, T, A, E, R, Aux1, Aux2, Aux3, Aux4, \r
        public short[] controllerData = new short[8];       
        private static int[] buttonMap = new int[ButtonMapping.size];

        private Switch modeSwitch, flightSwitch;
        private Spinner stickModeSpinner, flightModeSpinner;
        private InputManager inputManager;
        private ToggleButton armButton;
        private RelativeLayout layoutJoystickLeft, layoutJoystickRight;

        VirtualController LeftStickPad, RightStickPad;
        JoystickController JoystickPad;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VirtualController);
            //Get Mode Value 
            ISharedPreferences pref = GetSharedPreferences("pref", FileCreationMode.Private);
            stickModeID = pref.GetInt("stick_mode", 0);  //(Key, Defualt Value)
            flightModeID = pref.GetInt("flight_mode", 0);//(Key, Defualt Value)

            for (int i = 0; i < controllerData.Length; i++)
            {
                //T A E R 1500 , Aux 1000
                if(i < 4) controllerData[i] = STICK_OUTPUT_MID;
                else      controllerData[i] = STICK_OUTPUT_MIN;
            }

            for (int i = 0; i < buttonMap.Length; i++)
                buttonMap[i] = 0;

            armButton           = this.FindViewById<ToggleButton>(Resource.Id.armButton);
            modeSwitch          = this.FindViewById<Switch>(Resource.Id.modeSwitch);
            flightSwitch        = this.FindViewById<Switch>(Resource.Id.flightSwitch);
            stickModeSpinner    = this.FindViewById<Spinner>(Resource.Id.spinnerStickMode);
            flightModeSpinner   = this.FindViewById<Spinner>(Resource.Id.spinnerFlightMode);
            layoutJoystickLeft  = this.FindViewById<RelativeLayout>(Resource.Id.layout_joystick1);
            layoutJoystickRight = this.FindViewById<RelativeLayout>(Resource.Id.layout_joystick2);

            stickModeSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Spinner_ItemSelected);
            var adapter1 = ArrayAdapter.CreateFromResource(this, Resource.Array.stick_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter1.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            stickModeSpinner.Adapter = adapter1;

            flightModeSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Spinner_ItemSelected);
            var adapter2 = ArrayAdapter.CreateFromResource(this, Resource.Array.flight_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            flightModeSpinner.Adapter = adapter2;

            stickModeSpinner.SetSelection(stickModeID);
            flightModeSpinner.SetSelection(flightModeID);
            GetFlightMode();

            RightStickPad   = new VirtualController(ApplicationContext, layoutJoystickRight, Resource.Drawable.image_button);
            LeftStickPad    = new VirtualController(ApplicationContext, layoutJoystickLeft, Resource.Drawable.image_button);
            JoystickPad     = new JoystickController(LeftStickPad, RightStickPad);
            inputManager    = (InputManager)GetSystemService(Context.InputService);

            layoutJoystickRight.SetOnTouchListener(new OnTouchListenerAnonymousInnerClass(this, RightStickPad));
            layoutJoystickLeft.SetOnTouchListener(new OnTouchListenerAnonymousInnerClass(this, LeftStickPad));
            armButton.Click += ArmButtonClick;

            JoystickPad.CheckGameControllers(inputManager.GetInputDeviceIds());
        }

        private void ArmButtonClick(object sender, System.EventArgs e)
        {
            if (armButton.Checked)
            {
                controllerData[4] = ARM_ON;

                ISharedPreferences pref         = GetSharedPreferences("pref", FileCreationMode.Private);
                ISharedPreferencesEditor editor = pref.Edit();

                editor.PutInt("stick_mode", (int)stickModeSpinner.SelectedItemId);
                editor.PutInt("flight_mode", (int)flightModeSpinner.SelectedItemId);
                editor.Commit();

                InitStickPosition();

                modeSwitch.Visibility = ViewStates.Invisible;
                flightSwitch.Visibility = ViewStates.Invisible;

                flightModeSpinner.Enabled = false;
                stickModeSpinner.Enabled = false;               
            }
            else
            {
                controllerData[4] = ARM_OFF;

                LeftStickPad.DrawPosition(LeftStickPad.STICK_X_CENTER, LeftStickPad.STICK_Y_MAX);
                RightStickPad.DrawPosition(RightStickPad.STICK_X_CENTER, RightStickPad.STICK_Y_MAX);

                modeSwitch.Visibility = ViewStates.Visible;
                flightSwitch.Visibility = ViewStates.Visible;

                flightModeSpinner.Enabled = true;
                stickModeSpinner.Enabled = true;

                LeftStickPad.Remove();
                RightStickPad.Remove();
            }

            SetChannelData();
        }

        public void SetChannelData()
        {
            for (int i = 0; i < GattServices.channelData.Length; i++)
                GattServices.channelData[i] = controllerData[i];
        }

        public void GetFlightMode()
        {
            if (flightModeSpinner.SelectedItem.ToString() == "ANGLE MODE")
            {
                controllerData[0] = STICK_OUTPUT_MIN;
                controllerData[5] = FLIGHT_MODE_ANGLE;
            }
            else if (flightModeSpinner.SelectedItem.ToString() == "POSITION HOLD MODE")
            {
                controllerData[5] = FLIGHT_MODE_POSHOLD;
            }

            SetChannelData();
        }

        public void GetStickData()
        {
            if (stickModeSpinner.SelectedItemId == 0)
            {//Stick Mode 1
                controllerData[0] = (short)VirtualController.ConversionStickData(-RightStickPad.Y, -RightStickPad.STICK_Y_MAX, -RightStickPad.STICK_Y_MIN, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);   //T
                controllerData[1] = (short)VirtualController.ConversionStickData(RightStickPad.X, RightStickPad.STICK_X_MIN, RightStickPad.STICK_X_MAX, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);      //A
                controllerData[2] = (short)VirtualController.ConversionStickData(LeftStickPad.Y, LeftStickPad.STICK_Y_MIN, LeftStickPad.STICK_Y_MAX, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);      //E
                controllerData[3] = (short)VirtualController.ConversionStickData(-LeftStickPad.X, -LeftStickPad.STICK_X_MAX, -LeftStickPad.STICK_X_MIN, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);   //R
            }
            else if (stickModeSpinner.SelectedItemId == 1)
            {//Stick Mode 2
                controllerData[0] = (short)VirtualController.ConversionStickData(-LeftStickPad.Y, -LeftStickPad.STICK_Y_MAX, -LeftStickPad.STICK_Y_MIN, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);   //T
                controllerData[1] = (short)VirtualController.ConversionStickData(RightStickPad.X, LeftStickPad.STICK_X_MIN, LeftStickPad.STICK_X_MAX, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);      //A
                controllerData[2] = (short)VirtualController.ConversionStickData(RightStickPad.Y, RightStickPad.STICK_Y_MIN, RightStickPad.STICK_Y_MAX, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);      //E
                controllerData[3] = (short)VirtualController.ConversionStickData(-LeftStickPad.X, -LeftStickPad.STICK_X_MAX, -LeftStickPad.STICK_X_MIN, STICK_OUTPUT_MIN, STICK_OUTPUT_MAX);   //R
            }

            SetChannelData();
        }

        public void InitStickPosition()
        {
            if (flightModeSpinner.SelectedItem.ToString() == "ANGLE MODE")
            {
                controllerData[5] = FLIGHT_MODE_ANGLE;

                if (stickModeSpinner.SelectedItemId == 0)
                { //Stick Mode 1
                    LeftStickPad.DrawPosition(LeftStickPad.STICK_X_CENTER, LeftStickPad.STICK_Y_CENTER);
                    RightStickPad.DrawPosition(RightStickPad.STICK_X_CENTER, (float)RightStickPad.Y);
                }
                else if (stickModeSpinner.SelectedItemId == 1)
                { //Stick Mode 2
                    LeftStickPad.DrawPosition(LeftStickPad.STICK_X_CENTER, (float)LeftStickPad.Y);
                    RightStickPad.DrawPosition(RightStickPad.STICK_X_CENTER, RightStickPad.STICK_Y_CENTER);
                }
            }
            else if (flightModeSpinner.SelectedItem.ToString() == "POSITION HOLD MODE")
            {
                controllerData[5] = FLIGHT_MODE_POSHOLD;
                LeftStickPad.DrawPosition(LeftStickPad.STICK_X_CENTER, LeftStickPad.STICK_Y_CENTER);
                RightStickPad.DrawPosition(RightStickPad.STICK_X_CENTER, RightStickPad.STICK_Y_CENTER);
            }

            GetStickData();
        }

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string toast = string.Format("SELECT: {0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(this, toast, ToastLength.Long).Show();
            GetFlightMode();
        }

        protected override void OnStop()
        {
            base.OnStop();
            //Set Mode Value
            ISharedPreferences pref = GetSharedPreferences("pref", FileCreationMode.Private);
            ISharedPreferencesEditor editor = pref.Edit();

            editor.PutInt("stick_mode", (int)stickModeSpinner.SelectedItemId);
            editor.PutInt("flight_mode", (int)flightModeSpinner.SelectedItemId);
            editor.Commit();
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            //Joystick Stick Event Method
            JoystickPad.KeyMapping(e);

            if (armButton.Checked)
            {
                LeftStickPad.DrawPosition(JoystickPad.leftStickX, JoystickPad.leftStickY);
                RightStickPad.DrawPosition(JoystickPad.rightStickX, JoystickPad.rightStickY);
            }
            else
            {
                LeftStickPad.SetPosition(JoystickPad.leftStickX, JoystickPad.leftStickY);
                RightStickPad.SetPosition(JoystickPad.rightStickX, JoystickPad.rightStickY);
            }

            GetStickData();

            return base.OnGenericMotionEvent(e);
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            //Joystick Key Event Method
            InputDevice device = e.Device;
            if (device != null && device.Id == JoystickPad.currentDeviceID)
            {
                if (JoystickController.IsGamepad(device))
                {
                    int index = ButtonMapping.OrdinalValue(keyCode);
                    if (index >= 0)
                    {
                        buttonMap[index] = 1;
                    }
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            //Joystick Key Event Method
            InputDevice device = e.Device;
            if (device != null && device.Id == JoystickPad.currentDeviceID)
            {
                int index = ButtonMapping.OrdinalValue(keyCode);
                if (index >= 0)
                {
                    buttonMap[index] = 0;
                }
                return true;
            }
            return base.OnKeyUp(keyCode, e);
        }

        public void OnInputDeviceAdded(int deviceId)
        {
            if (!JoystickPad.connectedDevices.Contains(deviceId))
            {
                JoystickPad.connectedDevices.Add(deviceId);
            }
            if (JoystickPad.currentDeviceID == -1)
            {
                JoystickPad.currentDeviceID = deviceId;
                InputDevice dev = InputDevice.GetDevice(JoystickPad.currentDeviceID);
                if (dev != null)
                {
                }
            }
        }

        public void OnInputDeviceChanged(int deviceId)
        {
            JoystickPad.CheckGameControllers(inputManager.GetInputDeviceIds());
        }

        public void OnInputDeviceRemoved(int deviceId)
        {
            JoystickPad.connectedDevices.Remove(deviceId);
            if (JoystickPad.currentDeviceID == deviceId)
                JoystickPad.currentDeviceID = -1;

            if (JoystickPad.connectedDevices.Count == 0)
            {
            }
            else
            {
                JoystickPad.currentDeviceID = JoystickPad.connectedDevices[0];
                InputDevice dev = InputDevice.GetDevice(JoystickPad.currentDeviceID);
            }
        }

        private class OnTouchListenerAnonymousInnerClass : Java.Lang.Object, Android.Views.View.IOnTouchListener
        {
            private readonly ControllerActivity outerInstance;
            private readonly VirtualController virtualController;
            //private TextView text1;
            private bool touchState;

            public OnTouchListenerAnonymousInnerClass(ControllerActivity outerInstance, VirtualController virtualController)
            {
                this.outerInstance = outerInstance;
                this.virtualController = virtualController;
                touchState = true;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                //Touch Event Method
                if (outerInstance.controllerData[4] == ARM_ON)
                {
                    virtualController.DrawStick(e);
                    if (e.Action == MotionEventActions.Down)
                    {
                        if (virtualController.touchState)
                        {
                            outerInstance.GetStickData();
                            touchState = true;
                        }
                    }
                    else if (e.Action == MotionEventActions.Move && touchState)
                    {
                        outerInstance.GetStickData();
                    }
                    else if (e.Action == MotionEventActions.Up)
                    {
                        outerInstance.InitStickPosition();
                        virtualController.SetLastStickData((float)virtualController.X, (float)virtualController.Y);
                        touchState = false;
                    }
                }
                else
                {//ARM OFF

                }
                return true;
            }
        }
    }
}


