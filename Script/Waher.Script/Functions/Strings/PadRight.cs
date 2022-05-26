using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// PadRight(s,N)
	/// </summary>
	public class PadRight : FunctionTwoScalarVariables
	{
		/// <summary>
		/// PadRight(s,N)
		/// </summary>
		/// <param name="String">String.</param>
		/// <param name="N">Expected width.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PadRight(ScriptNode String, ScriptNode N, int Start, int Length, Expression Expression)
			: base(String, N, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "PadRight"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "s", "N" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">String.</param>
		/// <param name="Argument2">Delimiter</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			string s = Argument1.AssociatedObjectValue?.ToString() ?? string.Empty;
			int N = (int)(Expression.ToDouble(Argument2.AssociatedObjectValue) + 0.5);

			return new StringValue(s.PadRight(N));
		}

	}
}
