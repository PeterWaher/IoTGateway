using System.Xml;
using Waher.Script;

namespace Waher.Content.Xml.Attributes
{
	/// <summary>
	/// 64-bit integer attribute
	/// </summary>
	public class Int64Attribute : Attribute<long>
	{
		/// <summary>
		/// 64-bit integer attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public Int64Attribute(string AttributeName, long Value)
			: base(AttributeName, Value)
		{
		}

		/// <summary>
		/// 64-bit integer attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public Int64Attribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// 64-bit integer attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public Int64Attribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to convert script result to a value of type <see cref="long"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvert(object Result, out long Value)
		{
			if (Result is double d)
			{
				Value = (long)d;
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
		public override bool TryParse(string StringValue, out long Value)
		{
			return long.TryParse(StringValue, out Value);
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(long Value)
		{
			return Value.ToString();
		}
	}
}
