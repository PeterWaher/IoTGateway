using System.Numerics;
using Waher.Content;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Converts a value to a boolean literal.
	/// </summary>
	public class ToBooleanLiteral : SemanticConversionFunction
	{
		/// <summary>
		/// Converts a value to a boolean literal.
		/// </summary>
		public ToBooleanLiteral()
			: base()
		{
		}

		/// <summary>
		/// Converts a value to a boolean literal.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToBooleanLiteral(ScriptNode Argument, int Start, int Length, Expression Expression)
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
			return new ToBooleanLiteral(Argument, Start, Length, Expression);
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => BooleanLiteral.TypeUri;

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Converted value.</returns>
		public override IElement Convert(object Value)
		{
			if (Value is bool b)
				return new BooleanLiteral(b);
			else if (Value is string s)
			{
				if (CommonTypes.TryParse(s, out b))
					return new BooleanLiteral(b);
			}
			else if (Value is double d)
				return new BooleanLiteral(d != 0);
			else if (Value is Complex z)
				return new BooleanLiteral(z != Complex.Zero);
		
			b = System.Convert.ToBoolean(Value);
			return new BooleanLiteral(b);
		}
	}
}
