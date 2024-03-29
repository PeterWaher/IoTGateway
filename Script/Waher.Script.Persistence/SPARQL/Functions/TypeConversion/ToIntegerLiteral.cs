﻿using System.Numerics;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Converts a value to a integer literal.
	/// </summary>
	public class ToIntegerLiteral : SemanticConversionFunction
	{
		/// <summary>
		/// Converts a value to a integer literal.
		/// </summary>
		public ToIntegerLiteral()
			: base()
		{
		}

		/// <summary>
		/// Converts a value to a integer literal.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToIntegerLiteral(ScriptNode Argument, int Start, int Length, Expression Expression)
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
			return new ToIntegerLiteral(Argument, Start, Length, Expression);
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => IntegerLiteral.TypeUri;

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Converted value.</returns>
		public override IElement Convert(object Value)
		{
			if (Value is BigInteger i)
				return new IntegerLiteral(i);
			else if (Value is double d)
				return new IntegerLiteral((BigInteger)d);
			else if (Value is bool b)
				return new IntegerLiteral(b ? 1 : 0);
			else if (Value is string s)
			{
				if (BigInteger.TryParse(s, out i))
					return new IntegerLiteral(i);
			}
			else if (Value is Complex z)
			{
				if (z.Imaginary == 0)
					return new IntegerLiteral((BigInteger)z.Real);
			}
			
			long l = System.Convert.ToInt64(Value);
			return new IntegerLiteral(l);
		}
	}
}
