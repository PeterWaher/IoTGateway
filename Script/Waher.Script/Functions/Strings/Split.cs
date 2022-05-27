using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Split(String,Substring)
	/// </summary>
	public class Split : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Split(String,Substring)
		/// </summary>
		/// <param name="String">String.</param>
		/// <param name="Substring">Delimiter</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Split(ScriptNode String, ScriptNode Substring, int Start, int Length, Expression Expression)
			: base(String, Substring, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Split);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "String", "Substring" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">String.</param>
		/// <param name="Argument2">Delimiter</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			return new ObjectVector(Argument1.Split(new string[] { Argument2 }, System.StringSplitOptions.None));
		}
	}
}
