using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Gets information about a tab, the URI used, query parameters, session ID and 
	/// session variables. If tab is not found, `null` is returned.
	/// </summary>
	public class GetTabInformation : FunctionOneScalarVariable
	{
		/// <summary>
		/// Gets information about a tab, the URI used, query parameters, session ID and 
		/// session variables. If tab is not found, `null` is returned.
		/// </summary>
		/// <param name="TabID">Tab ID.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GetTabInformation(ScriptNode TabID, int Start, int Length, Expression Expression)
			: base(TabID, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(GetTabInformation);

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public override string DefaultVariableName => "TabID";

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			ClientEvents.TabInformation Info = ClientEvents.GetTabIDInformation(Argument);
			if (Info is null)
				return ObjectValue.Null;
			else
				return new ObjectValue(Info);
		}
	}
}
