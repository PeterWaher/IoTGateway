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
		private static Dictionary<string, UsbState> serialPorts = new Dictionary<string, UsbState>();
		private string portName = string.Empty;
		private UsbState state = null;

		public UsbConnectedDevice()
			: base()
		{
		}

		internal static UsbState GetState(string Name, DeviceInformation DeviceInfo)
		{
			UsbState State;

			lock (serialPorts)
			{
				if (serialPorts.TryGetValue(Name, out State))
					return State;

				if (DeviceInfo != null)
				{
					State = new UsbState()
					{
						SerialPort = new UsbSerial(DeviceInfo)
					};
					
					State.SerialPort.ConnectionEstablished += State.SerialPort_ConnectionEstablished;

					State.Device = new RemoteDevice(State.SerialPort);
					State.Device.DeviceReady += State.Device_DeviceReady;
					State.Device.AnalogPinUpdated += State.Device_AnalogPinUpdated;
					State.Device.DigitalPinUpdated += State.Device_DigitalPinUpdated;
					State.Device.DeviceConnectionFailed += State.Device_DeviceConnectionFailed;
					State.Device.DeviceConnectionLost += State.Device_DeviceConnectionLost;

					serialPorts[Name] = State;
				}
				else
					return null;
			}

			State.SerialPort.begin(57600, SerialConfig.SERIAL_8N1);

			return State;
		}

		internal static RemoteDevice GetDevice(string Name)
		{
			lock (serialPorts)
			{
				if (serialPorts.TryGetValue(Name, out UsbState State))
					return State.Device;
				else
					return null;
			}
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

		internal UsbState UsbState
		{
			get { return this.state; }
		}

		private void Init()
		{
			if (!string.IsNullOrEmpty(this.portName))
			{
				DeviceInformation DeviceInfo = Module.GetDeviceInformation(this.portName);
				if (DeviceInfo != null)
					this.state = GetState(this.portName, DeviceInfo);
			}
		}

		public RemoteDevice Device
		{
			get
			{
				if (this.state != null)
					return this.state.Device;
				else
					return null;
			}
		}

		public override Task AddAsync(INode Child)
		{
			Task Result = base.AddAsync(Child);

			if (this.state != null && Child is Pin Pin)
				this.state.AddPin(Pin.PinNrStr, Pin);

			return Result;
		}

		public override async Task<bool> RemoveAsync(INode Child)
		{
			bool Result = await base.RemoveAsync(Child);

			if (Result && this.state != null && Child is Pin Pin)
				this.state.RemovePin(Pin.PinNrStr, Pin);

			return Result;
		}

		protected override void SortChildrenAfterLoadLocked(List<MeteringNode> Children)
		{
			base.SortChildrenAfterLoadLocked(Children);

			if (this.state != null && Children != null)
			{
				Dictionary<string, Pin> Pins = new Dictionary<string, Pin>();

				foreach (INode Node in Children)
				{
					if (Node is Pin Pin)
						Pins[Pin.PinNrStr] = Pin;
				}

				this.state.Pins = Pins;
			}
		}

		public override Task DestroyAsync()
		{
			lock (serialPorts)
			{
				if (serialPorts.TryGetValue(this.portName, out UsbState State) && State == this.state)
				{
					serialPorts.Remove(this.portName);
					State.Dispose();
					this.state = null;
				}
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

