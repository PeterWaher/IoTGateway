using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// LangMatches(Language,Pattern)
	/// </summary>
	public class LangMatches : FunctionTwoScalarVariables
	{
		/// <summary>
		/// LangMatches(Language,Pattern)
		/// </summary>
		/// <param name="Value">Value Argument.</param>
		/// <param name="Type">Type Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LangMatches(ScriptNode Value, ScriptNode Type, int Start, int Length, Expression Expression)
			: base(Value, Type, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(LangMatches);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Language", "Pattern" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			if (string.IsNullOrEmpty(Argument1))
				return BooleanValue.False;

			if (Argument2 == "*")
				return BooleanValue.True;

			if (Argument2.IndexOf('-') >= 0)
				return string.Compare(Argument1, Argument2, true) == 0 ? BooleanValue.True : BooleanValue.False;

			int i = Argument1.IndexOf('-');
			if (i >= 0)
				Argument1 = Argument1.Substring(0, i);

			return string.Compare(Argument1, Argument2, true) == 0 ? BooleanValue.True : BooleanValue.False;
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
