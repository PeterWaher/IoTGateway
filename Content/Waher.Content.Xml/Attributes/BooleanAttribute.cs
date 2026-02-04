using System.Xml;
using Waher.Script;

namespace Waher.Content.Xml.Attributes
{
	/// <summary>
	/// Boolean attribute
	/// </summary>
	public class BooleanAttribute : Attribute<bool>
	{
		/// <summary>
		/// Boolean attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public BooleanAttribute(string AttributeName, bool Value)
			: base(AttributeName, Value)
		{
		}

		/// <summary>
		/// Boolean attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public BooleanAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// Boolean attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public BooleanAttribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out bool Value)
		{
			return CommonTypes.TryParse(StringValue, out Value);
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(bool Value)
		{
			return CommonTypes.Encode(Value);
		}
	}
}
