using System.Threading.Tasks;
using Waher.Content.Semantic.Model;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// StrDt(x,Type)
	/// </summary>
	public class StrDt : FunctionTwoScalarVariables
	{
		/// <summary>
		/// StrDt(x,Type)
		/// </summary>
		/// <param name="Value">Value Argument.</param>
		/// <param name="Type">Type Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public StrDt(ScriptNode Value, ScriptNode Type, int Start, int Length, Expression Expression)
			: base(Value, Type, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(StrDt);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Value", "Type" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			return SemanticElements.Parse(Argument1, Argument2, string.Empty);
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(string Argument1, string Argument2, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument1, Argument2, Variables));
		}
	}
}
