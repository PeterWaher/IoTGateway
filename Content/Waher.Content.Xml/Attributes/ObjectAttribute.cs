using System.Xml;
using Waher.Script;

namespace Waher.Content.Xml.Attributes
{
	/// <summary>
	/// Objecting point (Object) attribute
	/// </summary>
	public class ObjectAttribute : Attribute<object>
	{
		/// <summary>
		/// Objecting point (Object) attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public ObjectAttribute(string AttributeName, object Value)
			: base(AttributeName, Value)
		{
		}

		/// <summary>
		/// Objecting point (Object) attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public ObjectAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// Objecting point (Object) attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public ObjectAttribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to convert script result to a value of type <see cref="object"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvert(object Result, out object Value)
		{
			Value = Result;
			return true;
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out object Value)
		{
			Value = StringValue;
			return true;
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(object Value)
		{
			return Value?.ToString();
		}
	}
}
