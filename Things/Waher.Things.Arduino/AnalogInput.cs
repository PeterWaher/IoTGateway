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
using Waher.Things.DisplayableParameters;
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
				this.mode = value;
				this.Initialize();
			}
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 16, "Analog Input");
		}

		public override void Initialize()
		{
			RemoteDevice Device = this.Device;

			if (Device != null)
			{
				switch (Mode)
				{
					case AnalogInputPinMode.Input:
						Device.pinMode(this.PinNrStr, PinMode.ANALOG);
						break;
				}

				this.initialized = true;
			}
			else
				this.initialized = false;
		}

		public void StartReadout(ISensorReadout Request)
		{
			try
			{
				RemoteDevice Device = this.Device;
				if (Device == null)
					throw new Exception("Device not ready.");
					
				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (!this.initialized)
					this.Initialize();

				if (Request.IsIncluded(FieldType.Status))
				{
					Fields.Add(new EnumField(this, Now, "Drive Mode", Device.getPinMode(this.PinNr), FieldType.Status, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 15));
				}

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

		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Module), 19, "Mode"), this.mode.ToString()));

			return Result;
		}
	}
}
