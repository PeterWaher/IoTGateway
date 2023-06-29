using System.Numerics;
using Waher.Content;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Converts a value to a single literal.
	/// </summary>
	public class ToSingleLiteral : SemanticConversionFunction
	{
		/// <summary>
		/// Converts a value to a single literal.
		/// </summary>
		public ToSingleLiteral()
			: base()
		{
		}

		/// <summary>
		/// Converts a value to a single literal.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToSingleLiteral(ScriptNode Argument, int Start, int Length, Expression Expression)
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
			return new ToSingleLiteral(Argument, Start, Length, Expression);
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => SingleLiteral.TypeUri;

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Converted value.</returns>
		public override IElement Convert(object Value)
		{
			if (Value is float d)
				return new SingleLiteral(d);
			else if (Value is bool b)
				return new SingleLiteral(b ? 1 : 0);
			else if (Value is string s)
			{
				if (CommonTypes.TryParse(s, out d))
					return new SingleLiteral(d);
			}
			else if (Value is BigInteger i)
				return new SingleLiteral((float)i);
			else if (Value is Complex z)
			{
				if (z.Imaginary == 0)
					return new SingleLiteral((float)z.Real);
			}
		
			d = System.Convert.ToSingle(Value);
			return new SingleLiteral(d);
		}
	}
}
