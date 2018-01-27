using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Things;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Arduino
{
	public class Module : IModule
	{
		private static Dictionary<string, UsbState> serialPorts = new Dictionary<string, UsbState>();

		public Module()
		{
		}

		public WaitHandle Start()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			this.Init(Done);
			return Done;
		}

		private async void Init(ManualResetEvent Done)
		{
			try
			{
				DeviceInformationCollection Devices = await UsbSerial.listAvailableDevicesAsync();
				UsbState State;

				foreach (DeviceInformation DeviceInfo in Devices)
				{
					if (!DeviceInfo.IsEnabled)
						continue;

					lock (serialPorts)
					{
						State = new UsbState()
						{
							Name = DeviceInfo.Name,
							DeviceInformation = DeviceInfo,
							SerialPort = new UsbSerial(DeviceInfo)
						};

						State.SerialPort.ConnectionEstablished += State.SerialPort_ConnectionEstablished;

						State.Device = new RemoteDevice(State.SerialPort);
						State.Device.DeviceReady += State.Device_DeviceReady;
						State.Device.AnalogPinUpdated += State.Device_AnalogPinUpdated;
						State.Device.DigitalPinUpdated += State.Device_DigitalPinUpdated;
						State.Device.DeviceConnectionFailed += State.Device_DeviceConnectionFailed;
						State.Device.DeviceConnectionLost += State.Device_DeviceConnectionLost;

						serialPorts[DeviceInfo.Name] = State;
					}

					State.SerialPort.begin(57600, SerialConfig.SERIAL_8N1);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				Done.Set();
			}
		}

		public static DeviceInformation GetDeviceInformation(string Name)
		{
			return GetState(Name)?.DeviceInformation;
		}

		internal static RemoteDevice GetDevice(string Name)
		{
			return GetState(Name)?.Device;
		}

		internal static UsbState GetState(string Name)
		{
			lock (serialPorts)
			{
				if (serialPorts.TryGetValue(Name, out UsbState State))
					return State;
				else
					return null;
			}
		}

		public void Stop()
		{
			UsbState[] States;

			lock (serialPorts)
			{
				States = new UsbState[serialPorts.Count];
				serialPorts.Values.CopyTo(States, 0);
				serialPorts.Clear();
			}

			foreach (UsbState State in States)
				State.Dispose();
		}
	}
}
