using System;
using System.Xml;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Positive integer attribute
	/// </summary>
	public class PositiveIntegerAttribute : Attribute<int>
	{
		/// <summary>
		/// Positive integer attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public PositiveIntegerAttribute(string AttributeName, int Value)
			: base(AttributeName, Value)
		{
		}

		/// <summary>
		/// Positive integer attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public PositiveIntegerAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// Positive integer attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public PositiveIntegerAttribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out int Value)
		{
			if (int.TryParse(StringValue, out Value))
				return Value > 0;
			else
				return false;
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(int Value)
		{
			return Value.ToString();
		}

		/// <summary>
		/// Copies the attribute object if undefined, or defined by an expression.
		/// Returns a reference to itself, if preset (set by a constant value).
		/// </summary>
		/// <returns>Attribute reference.</returns>
		public PositiveIntegerAttribute CopyIfNotPreset()
		{
			if (this.HasPresetValue)
				return this;
			else
				return new PositiveIntegerAttribute(this.Name, this.Expression);
		}

	}
}
