using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Converts a value to a string literal.
	/// </summary>
	public class ToStringLiteral : SemanticConversionFunction
	{
		/// <summary>
		/// Converts a value to a string literal.
		/// </summary>
		public ToStringLiteral()
			: base()
		{
		}

		/// <summary>
		/// Converts a value to a string literal.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToStringLiteral(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a function node.
		/// </summary>
		/// <param name="Argument">Parsed argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		/// <returns>Function script node.</returns>
		public override ScriptNode CreateFunction(ScriptNode Argument, int Start, int Length, Expression Expression)
		{
			return new ToStringLiteral(Argument, Start, Length, Expression);
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => StringLiteral.TypeUri;

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Converted value.</returns>
		public override IElement Convert(object Value)
		{
			return new StringLiteral(Value?.ToString() ?? string.Empty);
		}
	}
}
