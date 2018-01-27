using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Windows.Devices.Enumeration;
using Waher.Events;
using Waher.Things.Metering;

namespace Waher.Things.Arduino
{
	internal class UsbState : IDisposable
	{
		public Dictionary<string, Pin> Pins = new Dictionary<string, Pin>();
		public DeviceInformation DeviceInformation = null;
		public UsbSerial SerialPort = null;
		public RemoteDevice Device = null;
		public string Name = string.Empty;
		public bool Ready = false;

		internal void Device_DeviceConnectionLost(string message)
		{
			this.Ready = false;
			Log.Error("Device connection lost.", this.Name);    // TODO: Retry
		}

		internal void Device_DeviceConnectionFailed(string message)
		{
			this.Ready = false;
			Log.Error("Device connection failed.", this.Name);  // TODO: Retry, after delay
		}

		internal async void Device_DeviceReady()
		{
			try
			{
				this.Ready = true;
				Log.Informational("Device ready.", this.Name);

				UsbConnectedDevice Port = null;
				bool Found = false;

				foreach (INode Node in await MeteringTopology.Root.ChildNodes)
				{
					Port = Node as UsbConnectedDevice;
					if (Port != null && Port.PortName == this.Name)
					{
						Found = true;
						break;
					}
				}

				if (!Found)
				{
					Port = new UsbConnectedDevice()
					{
						NodeId = this.Name,
						PortName = this.Name
					};

					await MeteringTopology.Root.AddAsync(Port);

					Log.Informational("Device added to metering topology.", this.Name);
				}

				foreach (MeteringNode Child in await Port.ChildNodes)
				{
					if (Child is Pin Pin)
					{
						lock (this.Pins)
						{
							this.Pins[Pin.PinNrStr] = Pin;
						}

						Pin.Initialize();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal void SerialPort_ConnectionEstablished()
		{
			Log.Informational("Connection established.", this.Name);
		}

		internal void Device_DigitalPinUpdated(byte pin, PinState state)
		{
			try
			{
				Pin Pin = this.GetPin(pin.ToString());

				if (Pin != null && Pin is DigitalPin DigitalPin)
					DigitalPin.Pin_ValueChanged(state);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal void Device_AnalogPinUpdated(string pin, ushort value)
		{
			try
			{
				Pin Pin = this.GetPin(pin);

				if (Pin != null && Pin is AnalogInput AnalogInput)
					AnalogInput.Pin_ValueChanged(value);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal Pin GetPin(string PinNr)
		{
			lock (this.Pins)
			{
				if (this.Pins.TryGetValue(PinNr, out Pin Pin))
					return Pin;
				else
					return null;
			}
		}

		public void Dispose()
		{
			if (this.Device != null)
			{
				this.Device.Dispose();
				this.Device = null;
			}

			if (this.SerialPort != null)
			{
				this.SerialPort.Dispose();
				this.SerialPort = null;
			}
		}

		internal void AddPin(string PinNr, Pin Pin)
		{
			lock (this.Pins)
			{
				if (!this.Pins.ContainsKey(PinNr))
					this.Pins[PinNr] = Pin;
			}

			if (this.Ready)
				Pin.Initialize();
		}

		internal void RemovePin(string PinNr, Pin Pin)
		{
			lock (this.Pins)
			{
				if (this.Pins.TryGetValue(PinNr, out Pin Pin2) && Pin2 == Pin)
					this.Pins.Remove(PinNr);
			}
		}
	}
}
