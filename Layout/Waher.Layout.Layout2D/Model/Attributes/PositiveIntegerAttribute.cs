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
		/// <param name="Document">Document hosting the attribute.</param>
		public PositiveIntegerAttribute(string AttributeName, int Value, Layout2DDocument Document)
			: base(AttributeName, Value, Document)
		{
		}

		/// <summary>
		/// Positive integer attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public PositiveIntegerAttribute(XmlElement E, string AttributeName, Layout2DDocument Document)
			: base(E, AttributeName, true, Document)
		{
		}

		/// <summary>
		/// Positive integer attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Document">Document hosting the attribute.</param>
		public PositiveIntegerAttribute(string AttributeName, Expression Expression, Layout2DDocument Document)
			: base(AttributeName, Expression, Document)
		{
		}

		/// <summary>
		/// Tries to convert script result to a value of type <see cref="float"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvert(object Result, out int Value)
		{
			if (Result is double d)
			{
				Value = (int)(d + 0.5);
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
		/// <param name="ForDocument">Document that will host the new attribute.</param>
		/// <returns>Attribute reference.</returns>
		public PositiveIntegerAttribute CopyIfNotPreset(Layout2DDocument ForDocument)
		{
			if (this.HasPresetValue)
				return this;
			else
				return new PositiveIntegerAttribute(this.Name, this.Expression, ForDocument);
		}

	}
}
