using System.Collections.Generic;
using Android.Util;
using Android.Views;

namespace controller
{
    class JoystickController : Java.Lang.Object
    {
        public int currentDeviceID = -1;
        private const string TAG = "DUAL_SHOCK_TAG";
        public List<int> connectedDevices = new List<int>();
        
        public double rightStickX;
        public double rightStickY;
        public double leftStickX;
        public double leftStickY;

        private double JOYSTICK_INPUT_MIN = -0.8;
        private double JOYSTICK_INPUT_MAX = 0.8;

        private VirtualController LeftPad, RightPad;

        public JoystickController(VirtualController LeftPad, VirtualController RightPad)
        {
            this.LeftPad    = LeftPad;
            this.RightPad   = RightPad;
        }

        public void CheckGameControllers(int[] getDevicesID)
        {
            Log.Debug(TAG, "CheckGameControllers");
            int[] deviceList = getDevicesID;
            foreach (int deviceId in deviceList)
            {
                InputDevice dev = InputDevice.GetDevice(deviceId);
                int sources = (int)dev.Sources;

                if (((sources & (int)InputSourceType.Gamepad)  == (int)InputSourceType.Gamepad) ||
                    ((sources & (int)InputSourceType.Joystick) == (int)InputSourceType.Joystick))
                {
                    if (!connectedDevices.Contains(deviceId))
                    {
                        connectedDevices.Add(deviceId);
                        if (currentDeviceID == -1)
                        {
                            currentDeviceID = deviceId;
                        }
                    }
                }
            }
        }

        public void KeyMapping(MotionEvent e)
        {
            this.leftStickX     = VirtualController.ConversionStickData(e.GetAxisValue(AxesMapping.AXIS_X), JOYSTICK_INPUT_MIN, JOYSTICK_INPUT_MAX, LeftPad.STICK_X_MIN, LeftPad.STICK_X_MAX);
            this.leftStickY     = VirtualController.ConversionStickData(e.GetAxisValue(AxesMapping.AXIS_Y), JOYSTICK_INPUT_MIN, JOYSTICK_INPUT_MAX, LeftPad.STICK_Y_MIN, LeftPad.STICK_Y_MAX);
            this.rightStickX    = VirtualController.ConversionStickData(e.GetAxisValue(AxesMapping.AXIS_RX), JOYSTICK_INPUT_MIN, JOYSTICK_INPUT_MAX, RightPad.STICK_X_MIN, RightPad.STICK_X_MAX);
            this.rightStickY    = VirtualController.ConversionStickData(e.GetAxisValue(AxesMapping.AXIS_RY), JOYSTICK_INPUT_MIN, JOYSTICK_INPUT_MAX, RightPad.STICK_Y_MIN, RightPad.STICK_Y_MAX);
        }

        public static bool IsGamepad(InputDevice device)
        {
            if ((device.Sources & InputSourceType.Gamepad)       == InputSourceType.Gamepad ||
                (device.Sources & InputSourceType.ClassJoystick) == InputSourceType.Joystick)
            {
                return true;
            }
            return false;
        }
    }
}