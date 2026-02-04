using System.Xml;
using Waher.Script;

namespace Waher.Content.Xml.Attributes
{
	/// <summary>
	/// Doubleing point (Double) attribute
	/// </summary>
	public class DoubleAttribute : Attribute<double>
	{
		/// <summary>
		/// Doubleing point (Double) attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public DoubleAttribute(string AttributeName, double Value)
			: base(AttributeName, Value)
		{
		}

		/// <summary>
		/// Doubleing point (Double) attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public DoubleAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// Doubleing point (Double) attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public DoubleAttribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to convert script result to a value of type <see cref="double"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvert(object Result, out double Value)
		{
			if (Result is double d)
			{
				Value = (double)d;
				return true;
			}
			else
				return base.TryConvert(Result, out Value);
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out double Value)
		{
			return CommonTypes.TryParse(StringValue, out Value);
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(double Value)
		{
			return CommonTypes.Encode(Value);
		}
	}
}
