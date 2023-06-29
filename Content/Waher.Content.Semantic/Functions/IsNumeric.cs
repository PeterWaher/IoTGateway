using Waher.Content.Semantic.Model;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// IsNumeric(x)
	/// </summary>
	public class IsNumeric : FunctionOneSemanticVariable
	{
		/// <summary>
		/// IsNumeric(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public IsNumeric(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(IsNumeric);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(ISemanticElement Argument, Variables Variables)
		{
			return Argument is SemanticNumericLiteral ? BooleanValue.True : BooleanValue.False;
		}
	}
}
