using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace controller
{
    class GattServices : BluetoothGattCallback
    {
        private BluetoothGatt gattEDURON;
        private BluetoothGattService serialService;
        private BluetoothGattCharacteristic writeCharacteristic;
        private static BluetoothDevice targetDevice = null;
        TimerCallback timerDelegate;
        TimerState timerState;
        System.Threading.Timer timer;

        private static UUID SERIAL_UUID = UUID.FromString("0000fff0-0000-1000-8000-00805f9b34fb");
        private static UUID TX_UUID = UUID.FromString("0000fff2-0000-1000-8000-00805f9b34fb");
        private static int CHANNEL_PACKET_SIZE = 18;
        private static int MAX_CHANNEL = 8;
        public static byte[] writeBuffer = new byte[CHANNEL_PACKET_SIZE];
        public static short[] channelData = new short[MAX_CHANNEL];
        private static byte[] byteShort = new byte[2];
        public bool stateCheck = false;

        public class TimerState
        {
            public int counter = 0;
            public System.Threading.Timer tmr;
        }

        public GattServices()
        {
            gattEDURON = null;
            serialService = null;
            writeCharacteristic = null;
        }

        public void ConnectGatt(BluetoothDevice device)
        { 
            if (device != null)
            {
                targetDevice = device; //Save target device for re-connect
                gattEDURON = device.ConnectGatt(Application.Context, false, this);
            }
        }

        private void MakeChannelPacket(byte[] buffer)
        {
            // should be add buffer[0], buffer[17]
            for (int i = 0; i < MAX_CHANNEL; i++)
            {
                byteShort = BitConverter.GetBytes(channelData[i]);
                buffer[2 * i + 1] = byteShort[0];
                buffer[2 * i + 2] = byteShort[1];
            }

            buffer[0] = 0x40; //@
            buffer[17] = 0x0D; //CR  
        }

        private void GattDataWrite(object sendState)
        {
            if (this.stateCheck)
            {
                MakeChannelPacket(writeBuffer);
                writeCharacteristic.SetValue(writeBuffer);
                gattEDURON.WriteCharacteristic(writeCharacteristic);
            }
        }
        
        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            switch (newState)
            {
                case ProfileState.Disconnected:
                    if(targetDevice != null)
                        this.ConnectGatt(targetDevice);
                    break;

                case ProfileState.Connected:
                    Thread.Sleep(400);
                    gatt.DiscoverServices();
                    break;

                case ProfileState.Connecting:
                    break;

                case ProfileState.Disconnecting:
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            //Ready to Write Something!
            serialService = gattEDURON.GetService(SERIAL_UUID);
            writeCharacteristic = serialService.GetCharacteristic(TX_UUID);

            if (serialService != null && writeCharacteristic != null)
            {
                stateCheck = true;
                //Start Timer
                timerState = new TimerState();
                timerDelegate = new TimerCallback(GattDataWrite);
                timer = new System.Threading.Timer(timerDelegate, timerState, 50, 50);
                //Start ControllerActivity
                Intent intent = new Intent(Application.Context, typeof(controller.ControllerActivity));
                Application.Context.StartActivity(intent);
            }
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);
        }
    }
}