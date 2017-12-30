using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Windows.Devices.Enumeration;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Arduino
{
	public class UsbSerialPort : MeteringNode
	{
		private UsbSerial serialPort = null;
		private RemoteDevice device = null;
		private string portName = string.Empty;

		public UsbSerialPort()
			: base()
		{
		}

		[Page(2, "Port")]
		[Header(3, "Name:")]
		[ToolTip(4, "Name of USB serial port.")]
		[DefaultValueStringEmpty]
		public string PortName
		{
			get { return this.portName; }
			set
			{
				if (this.portName != value)
				{
					this.portName = value;
					this.Init();
				}
			}
		}

		private void Init()
		{
			if (this.device != null)
			{
				this.device.Dispose();
				this.device = null;
			}

			if (this.serialPort != null)
			{
				this.serialPort.Dispose();
				this.serialPort = null;
			}

			if (!string.IsNullOrEmpty(this.portName))
			{
				DeviceInformation DeviceInfo = Module.GetDeviceInformation(this.portName);
				if (DeviceInfo != null)
				{
					this.serialPort = new UsbSerial(DeviceInfo);
					this.serialPort.ConnectionEstablished += SerialPort_ConnectionEstablished;

					this.device = new RemoteDevice(this.serialPort);
					this.device.DeviceReady += Device_DeviceReady;
					this.device.AnalogPinUpdated += Device_AnalogPinUpdated;
					this.device.DigitalPinUpdated += Device_DigitalPinUpdated;
					this.device.DeviceConnectionFailed += Device_DeviceConnectionFailed;
					this.device.DeviceConnectionLost += Device_DeviceConnectionLost;

					this.serialPort.begin(57600, SerialConfig.SERIAL_8N1);
				}
			}
		}

		private void Device_DeviceConnectionLost(string message)
		{
			Log.Error("Device connection lost.", this.portName);
		}

		private void Device_DeviceConnectionFailed(string message)
		{
			Log.Error("Device connection failed.", this.portName);
		}

		private void Device_DigitalPinUpdated(byte pin, PinState state)
		{
			// TODO
		}

		private void Device_AnalogPinUpdated(string pin, ushort value)
		{
			// TODO
		}

		private void Device_DeviceReady()
		{
			Log.Informational("Device ready.", this.portName);
		}

		private void SerialPort_ConnectionEstablished()
		{
			Log.Informational("Connection established.", this.portName);
		}

		public override Task DestroyAsync()
		{
			if (this.device != null)
			{
				this.device.Dispose();
				this.device = null;
			}

			if (this.serialPort != null)
			{
				this.serialPort.Dispose();
				this.serialPort = null;
			}

			return base.DestroyAsync();
		}

		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is Root);
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 1, "USB Serial Port");
		}
	}
}
