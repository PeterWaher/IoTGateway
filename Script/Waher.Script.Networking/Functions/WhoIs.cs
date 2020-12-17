using System;
using System.Net;
using Waher.Networking.WHOIS;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Networking.Functions
{
	/// <summary>
	/// Makes a WHOIS query regarding an IP address.
	/// </summary>
	public class WhoIs : FunctionOneScalarVariable
	{
		/// <summary>
		/// Makes a WHOIS query regarding an IP address.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public WhoIs(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "whois";

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			if (IPAddress.TryParse(Argument, out IPAddress IP))
				return new StringValue(WhoIsClient.Query(IP).Result);
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			if (Argument.AssociatedObjectValue is IPAddress IP ||
				IPAddress.TryParse(Argument.AssociatedObjectValue?.ToString() ?? string.Empty, out IP))
			{
				return new StringValue(WhoIsClient.Query(IP).Result);
			}
			else
				throw new ScriptRuntimeException("Not an IP address.", this);
		}
	}
}
