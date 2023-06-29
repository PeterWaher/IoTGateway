using System.Threading.Tasks;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// StrLang(x,Type)
	/// </summary>
	public class StrLang : FunctionTwoScalarVariables
	{
		/// <summary>
		/// StrLang(x,Type)
		/// </summary>
		/// <param name="Value">Value Argument.</param>
		/// <param name="Type">Type Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public StrLang(ScriptNode Value, ScriptNode Type, int Start, int Length, Expression Expression)
			: base(Value, Type, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(StrLang);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Value", "Language" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			return new StringLiteral(Argument1, Argument2);
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
