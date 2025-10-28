using System;
using System.Collections.Generic;
using System.Linq;
using MediaDevices;

namespace An2WinFileTransfer.Services
{
    public class DeviceService
    {
        public IEnumerable<string> GetConnectedDeviceNames()
        {
            return MediaDevice.GetDevices().Select(d => d.FriendlyName);
        }

        public MediaDevice ConnectToDevice(string deviceName)
        {
            var device = MediaDevice.GetDevices()
                .FirstOrDefault(d => d.FriendlyName == deviceName);

            if (device == null)
            {
                throw new InvalidOperationException($"Device '{deviceName}' not found.");
            }

            device.Connect();
            return device;
        }

        public void DisconnectDevice(MediaDevice device)
        {
            if (device?.IsConnected == true)
            {
                device.Disconnect();
            }
        }

    }
}
