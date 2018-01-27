using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Windows.Devices.Enumeration;
using Waher.Events;

namespace Waher.Things.Arduino
{
	internal class UsbState : IDisposable
	{
		public string Name = string.Empty;
		public UsbSerial SerialPort = null;
		public RemoteDevice Device = null;
		public Dictionary<string, Pin> Pins = null;
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

		internal void Device_DeviceReady()
		{
			this.Ready = true;
			Log.Informational("Device ready.", this.Name);
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
			if (this.Pins == null)
				return null;

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
			if (this.Pins != null)
			{
				lock (this.Pins)
				{
					if (!this.Pins.ContainsKey(PinNr))
						this.Pins[PinNr] = Pin;
				}
			}
		}

		internal void RemovePin(string PinNr, Pin Pin)
		{
			if (this.Pins != null)
			{
				lock (this.Pins)
				{
					if (this.Pins.TryGetValue(PinNr, out Pin Pin2) && Pin2 == Pin)
						this.Pins.Remove(PinNr);
				}
			}
		}
	}
}
