using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.IoTGateway;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Things.Arduino
{
	public enum AnalogInputPinMode
	{
		Input
	}

	public class AnalogInput : AnalogPin, ISensor
	{
		private AnalogInputPinMode mode = AnalogInputPinMode.Input;
		private bool initialized = false;

		public AnalogInput()
			: base()
		{
		}

		[Page(5, "Arduino")]
		[Header(9, "Mode:")]
		[ToolTip(10, "Select drive mode of pin.")]
		[DefaultValue(AnalogInputPinMode.Input)]
		[Option(AnalogInputPinMode.Input, 11, "Input")]
		public AnalogInputPinMode Mode
		{
			get { return this.mode; }
			set
			{
				this.SetDriveMode(value);
				this.mode = value;
			}
		}

		private void SetDriveMode(AnalogInputPinMode Mode)
		{
			RemoteDevice Device = this.Device;

			if (Device == null)
				this.initialized = false;
			else
			{
				switch (Mode)
				{
					case AnalogInputPinMode.Input:
						Device.pinMode(this.PinNr, PinMode.ANALOG);
						break;
				}

				this.initialized = true;
			}
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 16, "Analog Input");
		}

		public void StartReadout(ISensorReadout Request)
		{
			try
			{
				RemoteDevice Device = this.Device;
				if (Device == null)
					throw new Exception("Device not ready.");

				if (!this.initialized)
					this.SetDriveMode(this.mode);
					
				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (Request.IsIncluded(FieldType.Momentary))
				{
					Fields.Add(new Int32Field(this, Now, "Value", Device.analogRead(this.PinNrStr), FieldType.Momentary, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 13));
				}
				
				if (Request.IsIncluded(FieldType.Identity))
				{
					Fields.Add(new Int32Field(this, Now, "Pin Number", this.PinNr, FieldType.Identity, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 14));
				}

				if (Request.IsIncluded(FieldType.Status))
				{
					Fields.Add(new EnumField(this, Now, "Drive Mode", Device.getPinMode(this.PinNr), FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 15));
				}

				Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		public void Pin_ValueChanged(ushort Value)
		{
			Gateway.NewMomentaryValues(this, new Int32Field(this, DateTime.Now, "Value", Value, FieldType.Momentary, FieldQoS.AutomaticReadout,
				typeof(Module).Namespace, 13));
		}
	}
}
