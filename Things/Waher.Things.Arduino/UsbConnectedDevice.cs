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
	public class UsbConnectedDevice : MeteringNode
	{
		private Dictionary<string, Pin> pins = null;
		private UsbSerial serialPort = null;
		private RemoteDevice device = null;
		private string portName = string.Empty;
		private bool ready = false;

		public UsbConnectedDevice()
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

		public RemoteDevice Device
		{
			get
			{
				if (this.ready)
					return this.device;
				else
					return null;
			}
		}

		private void Device_DeviceConnectionLost(string message)
		{
			this.ready = false;
			Log.Error("Device connection lost.", this.portName);    // TODO: Retry
		}

		private void Device_DeviceConnectionFailed(string message)
		{
			this.ready = false;
			Log.Error("Device connection failed.", this.portName);  // TODO: Retry, after delay
		}

		private async void Device_DigitalPinUpdated(byte pin, PinState state)
		{
			try
			{
				Pin Pin = await this.GetPin(pin.ToString());

				if (Pin != null && Pin is DigitalPin DigitalPin)
					DigitalPin.Pin_ValueChanged(state);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Device_AnalogPinUpdated(string pin, ushort value)
		{
			try
			{
				Pin Pin = await this.GetPin(pin);

				if (Pin != null && Pin is AnalogInput AnalogInput)
					AnalogInput.Pin_ValueChanged(value);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		public async Task<Pin> GetPin(string PinNr)
		{
			if (this.pins == null)
			{
				Dictionary<string, Pin> Pins = new Dictionary<string, Pin>();

				foreach (INode Node in await this.ChildNodes)
				{
					if (Node is Pin Pin)
						Pins[Pin.PinNrStr] = Pin;
				}

				this.pins = Pins;
			}

			lock (this.pins)
			{
				if (this.pins.TryGetValue(PinNr, out Pin Pin))
					return Pin;
				else
					return null;
			}
		}

		public override Task AddAsync(INode Child)
		{
			Task Result = base.AddAsync(Child);

			if (Child is Pin Pin)
				this.AddPin(Pin.PinNrStr, Pin);

			return Result;
		}

		public override async Task<bool> RemoveAsync(INode Child)
		{
			bool Result = await base.RemoveAsync(Child);

			if (Result && Child is Pin Pin)
				this.RemovePin(Pin.PinNrStr, Pin);

			return Result;
		}

		public void AddPin(string PinNr, Pin Pin)
		{
			if (this.pins != null)
			{
				lock (this.pins)
				{
					if (!this.pins.ContainsKey(PinNr))
						this.pins[PinNr] = Pin;
				}
			}
		}

		public void RemovePin(string PinNr, Pin Pin)
		{
			if (this.pins != null)
			{
				lock (this.pins)
				{
					if (this.pins.TryGetValue(PinNr, out Pin Pin2) && Pin2 == Pin)
						this.pins.Remove(PinNr);
				}
			}
		}

		private void Device_DeviceReady()
		{
			this.ready = true;
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
			return Task.FromResult<bool>(Child is Pin);
		}

		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(Parent is Root);
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 1, "USB Connected Device");
		}
	}
}

