using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Arduino
{
	public class DigitalOutput : DigitalPin, ISensor, IActuator
	{
		private bool initialized = false;

		public DigitalOutput()
			: base()
		{
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Module), 17, "Digital Output");
		}

		public override void Initialize()
		{
			RemoteDevice Device = this.Device;

			if (Device != null)
			{
				Device.pinMode(this.PinNr, PinMode.OUTPUT);
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
				if (Device is null)
					throw new Exception("Device not ready.");

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (!this.initialized)
					this.Initialize();

				if (Request.IsIncluded(FieldType.Momentary))
				{
					Fields.Add(new BooleanField(this, Now, "Value", Device.digitalRead(this.PinNr) == PinState.HIGH, FieldType.Momentary, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 13));
				}
				
				if (Request.IsIncluded(FieldType.Identity))
				{
					Fields.Add(new Int32Field(this, Now, "Pin Number", this.PinNr, FieldType.Identity, FieldQoS.AutomaticReadout,
						typeof(Module).Namespace, 14));

					this.AddIdentityReadout(Fields, Now);
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

		public override void Pin_ValueChanged(PinState NewState)
		{
			this.NewMomentaryValues(new BooleanField(this, DateTime.Now, "Value", NewState == PinState.HIGH, FieldType.Momentary, FieldQoS.AutomaticReadout,
				typeof(Module).Namespace, 13));
		}

		public ControlParameter[] GetControlParameters()
		{
			RemoteDevice Device = this.Device;
			if (Device is null)
				return new ControlParameter[0];

			return new ControlParameter[]
			{
				new BooleanControlParameter("Output", "Actuator", "Output:", "Digital output.",
					(Node) => Device.digitalRead(this.PinNr) == PinState.HIGH,
					(Node, Value) =>
					{
						try
						{
							Device.digitalWrite(this.PinNr, Value ? PinState.HIGH : PinState.LOW);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					})
			};
		}

		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Mode", await Language.GetStringAsync(typeof(Module), 19, "Mode"), PinMode.OUTPUT.ToString()));

			return Result;
		}
	}
}
