using System;
using System.Numerics;
using Waher.Content;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL.Functions.TypeConversion
{
	/// <summary>
	/// Converts a value to a DateTime literal.
	/// </summary>
	public class ToDateTimeLiteral : SemanticConversionFunction
	{
		/// <summary>
		/// Converts a value to a DateTime literal.
		/// </summary>
		public ToDateTimeLiteral()
			: base()
		{
		}

		/// <summary>
		/// Converts a value to a DateTime literal.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToDateTimeLiteral(ScriptNode Argument, int Start, int Length, Expression Expression)
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
			return new ToDateTimeLiteral(Argument, Start, Length, Expression);
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => DateTimeLiteral.TypeUri;

		/// <summary>
		/// Converts an object to the desired type.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Converted value.</returns>
		public override IElement Convert(object Value)
		{
			if (Value is DateTime d)
				return new DateTimeLiteral(d);
			else if (Value is DateTimeOffset DTO)
				return new DateTimeLiteral(DTO.DateTime);
			else if (Value is string s)
			{
				if (XML.TryParse(s, out d))
					return new DateTimeLiteral(d);
			}
		
			d = System.Convert.ToDateTime(Value);
			return new DateTimeLiteral(d);
		}
	}
}
