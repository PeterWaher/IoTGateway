using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;

namespace Waher.Things.Arduino
{
	public abstract class AnalogPin : Pin
	{
		public AnalogPin()
			: base()
		{
		}

		public override string PinNrStr => "A" + this.PinNr.ToString();

		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Pin", await Language.GetStringAsync(typeof(Module), 18, "Pin"), this.PinNrStr));

			return Result;
		}
	}
}
