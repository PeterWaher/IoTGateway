using Waher.Content.Semantic.Model;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// isIRI(x), isURI(x)
	/// </summary>
	public class IsUri : FunctionOneSemanticVariable
	{
		/// <summary>
		/// isIRI(x), isURI(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public IsUri(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(IsUri);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "IsIri" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(ISemanticElement Argument, Variables Variables)
		{
			return Argument is UriNode ? BooleanValue.True : BooleanValue.False;
		}
	}
}
