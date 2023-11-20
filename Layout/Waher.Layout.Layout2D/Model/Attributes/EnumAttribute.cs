using System;
using System.Xml;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Enumeration attribute
	/// </summary>
	public class EnumAttribute<TEnum> : Attribute<TEnum>
		where TEnum : struct
	{
		/// <summary>
		/// Enumeration attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public EnumAttribute(string AttributeName, TEnum Value, Layout2DDocument Document)
			: base(AttributeName, Value, Document)
		{
		}

		/// <summary>
		/// Enumeration attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public EnumAttribute(XmlElement E, string AttributeName, Layout2DDocument Document)
			: base(E, AttributeName, true, Document)
		{
		}

		/// <summary>
		/// Enumeration attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public EnumAttribute(string AttributeName, Expression Expression, Layout2DDocument Document)
			: base(AttributeName, Expression, Document)
		{
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out TEnum Value)
		{
			return Enum.TryParse<TEnum>(StringValue, out Value);
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(TEnum Value)
		{
			return Value.ToString();
		}

		/// <summary>
		/// Copies the attribute object if undefined, or defined by an expression.
		/// Returns a reference to itself, if preset (set by a constant value).
		/// </summary>
		/// <param name="ForDocument">Document that will host the new attribute.</param>
		/// <returns>Attribute reference.</returns>
		public EnumAttribute<TEnum> CopyIfNotPreset(Layout2DDocument ForDocument)
		{
			if (this.HasPresetValue)
				return this;
			else
				return new EnumAttribute<TEnum>(this.Name, this.Expression, ForDocument);
		}

	}
}
