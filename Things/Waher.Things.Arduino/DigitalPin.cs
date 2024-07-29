using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maker.RemoteWiring;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.Arduino
{
	/// <summary>
	/// TODO
	/// </summary>
	public abstract class DigitalPin : Pin
	{
		/// <summary>
		/// TODO
		/// </summary>
		public DigitalPin()
			: base()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string PinNrStr => this.PinNr.ToString();

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void Pin_ValueChanged(PinState NewState);

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new Int32Parameter("Pin", await Language.GetStringAsync(typeof(Module), 18, "Pin"), this.PinNr));

			return Result;
		}
	}
}
