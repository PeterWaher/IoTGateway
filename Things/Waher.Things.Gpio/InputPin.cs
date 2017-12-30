using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Runtime.Language;
using Waher.Things.SensorData;

namespace Waher.Things.Gpio
{
	public class InputPin : Pin, ISensor
	{
		private GpioPin pin = null;

		public InputPin()
			: base()
		{
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 5, "Input Pin");
		}

		public void StartReadout(ISensorReadout Request)
		{
			try
			{
				if (this.pin == null)
				{
					if (!this.Controller.TryOpenPin(this.PinNr, GpioSharingMode.SharedReadOnly, out this.pin, out GpioOpenStatus Status))
					{
						string Id = Status.ToString();
						string s = this.GetStatusMessage(Status);

						this.LogErrorAsync(Id, s);

						Request.ReportErrors(true, new ThingError(this, s));
						return;
					}

					this.pin.ValueChanged += Pin_ValueChanged;
				}

				List<Field> Fields = new List<Field>();
				DateTime Now = DateTime.Now;

				if (Request.IsIncluded(FieldType.Momentary))
					Fields.Add(new BooleanField(this, Now, "Value", this.pin.Read() == GpioPinValue.High, FieldType.Momentary, FieldQoS.AutomaticReadout));

				if (Request.IsIncluded(FieldType.Identity))
					Fields.Add(new Int32Field(this, Now, "Pin Number", this.PinNr, FieldType.Identity, FieldQoS.AutomaticReadout));

				if (Request.IsIncluded(FieldType.Status))
				{
					Fields.Add(new TimeField(this, Now, "Debounce Timeout", this.pin.DebounceTimeout, FieldType.Status, FieldQoS.AutomaticReadout));
					Fields.Add(new EnumField(this, Now, "Sharing Mode", this.pin.SharingMode, FieldType.Status, FieldQoS.AutomaticReadout));
					Fields.Add(new EnumField(this, Now, "Drive Mode", this.pin.GetDriveMode(), FieldType.Status, FieldQoS.AutomaticReadout));
				}

				//this.pin.SetDriveMode();
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

		private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
		}

		public override Task DestroyAsync()
		{
			if (this.pin != null)
			{
				this.pin.Dispose();
				this.pin = null;
			}

			return base.DestroyAsync();
		}
	}
}
