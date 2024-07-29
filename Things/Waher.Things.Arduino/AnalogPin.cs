using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.Arduino
{
	/// <summary>
	/// TODO
	/// </summary>
	public abstract class AnalogPin : Pin
	{
		/// <summary>
		/// TODO
		/// </summary>
		public AnalogPin()
			: base()
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string PinNrStr => "A" + this.PinNr.ToString();

		/// <summary>
		/// TODO
		/// </summary>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("Pin", await Language.GetStringAsync(typeof(Module), 18, "Pin"), this.PinNrStr));

			return Result;
		}
	}
}
