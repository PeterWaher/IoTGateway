using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// Subject(x)
	/// </summary>
	public class Subject : FunctionOneSemanticVariable
	{
		/// <summary>
		/// Subject(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Subject(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Subject);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(ISemanticElement Argument, Variables Variables)
		{
			if (Argument is ISemanticTriple Triple)
				return Triple.Subject;
			else
				throw new ScriptRuntimeException("Semantic triple expected.", this);
		}
	}
}
