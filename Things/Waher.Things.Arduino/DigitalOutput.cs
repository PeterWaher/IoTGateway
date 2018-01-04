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
	public class DigitalOutput : DigitalPin, ISensor
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

		public void StartReadout(ISensorReadout Request)
		{
			try
			{
				RemoteDevice Device = this.Device;
				if (Device == null)
					throw new Exception("Device not ready.");

				if (!this.initialized)
				{
					Device.pinMode(this.PinNr, PinMode.OUTPUT);
					this.initialized = true;
				}

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (Request.IsIncluded(FieldType.Momentary))
				{
					Fields.Add(new BooleanField(this, Now, "Value", Device.digitalRead(this.PinNr) == PinState.HIGH, FieldType.Momentary, FieldQoS.AutomaticReadout,
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
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		public override void Pin_ValueChanged(PinState NewState)
		{
			Gateway.NewMomentaryValues(this, new BooleanField(this, DateTime.Now, "Value", NewState == PinState.HIGH, FieldType.Momentary, FieldQoS.AutomaticReadout,
				typeof(Module).Namespace, 13));
		}
	}
}
