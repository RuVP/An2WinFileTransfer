using System;
using System.Collections.Generic;
using System.Linq;
using MediaDevices;

namespace An2WinFileTransfer.Services
{
    public class DeviceService
    {
        private readonly IEnumerable<MediaDevice> _mediaDevices;

        public DeviceService()
        {
            _mediaDevices = MediaDevice.GetDevices();
        }

        public IEnumerable<string> GetConnectedDeviceNames()
        {
            return _mediaDevices.Select(d => d.FriendlyName);
        }

        public MediaDevice ConnectToDevice(string deviceName)
        {
            var device = _mediaDevices.FirstOrDefault(d => d.FriendlyName == deviceName);

            if (device == null)
            {
                throw new InvalidOperationException($"Device '{deviceName}' not found.");
            }

            if (!device.IsConnected)
            {
                device.Connect();
            }

            return device;
        }

        public void DisconnectDevice(MediaDevice device)
        {
            if (device == null) return;

            if (device.IsConnected)
            {
                device.Disconnect();
            }
        }

    }
}
