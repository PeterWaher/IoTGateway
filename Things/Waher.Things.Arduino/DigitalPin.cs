using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Persistence.Attributes;
using Microsoft.Maker.RemoteWiring;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;

namespace Waher.Things.Arduino
{
	public abstract class DigitalPin : Pin
	{
		public DigitalPin()
			: base()
		{
		}

		public override string PinNrStr => this.PinNr.ToString();

		public abstract void Pin_ValueChanged(PinState NewState);
	}
}
