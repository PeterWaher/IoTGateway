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
		private static Dictionary<string, DeviceInformation> usbSerialPorts = new Dictionary<string, DeviceInformation>();

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

				foreach (DeviceInformation DeviceInfo in Devices)
				{
					if (!DeviceInfo.IsEnabled)
						continue;

					if (!DeviceInfo.Name.StartsWith("Arduino"))
						continue;

					lock (usbSerialPorts)
					{
						usbSerialPorts[DeviceInfo.Name] = DeviceInfo;
					}

					bool Found = false;

					foreach (INode Node in await MeteringTopology.Root.ChildNodes)
					{
						if (Node is UsbConnectedDevice Port)
						{
							if (Port.PortName == DeviceInfo.Name)
							{
								Found = true;
								break;
							}
						}
					}

					if (Found)
						continue;

					UsbConnectedDevice Port2 = new UsbConnectedDevice()
					{
						NodeId = DeviceInfo.Name,
						PortName = DeviceInfo.Name
					};

					await MeteringTopology.Root.AddAsync(Port2);

					Log.Informational("Device added to metering topology.", DeviceInfo.Name);
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

		public static DeviceInformation GetDeviceInformation(string DeviceName)
		{
			lock (usbSerialPorts)
			{
				if (usbSerialPorts.TryGetValue(DeviceName, out DeviceInformation Result))
					return Result;
				else
					return null;
			}
		}

		public void Stop()
		{
			lock (usbSerialPorts)
			{
				usbSerialPorts.Clear();
			}
		}
	}
}
