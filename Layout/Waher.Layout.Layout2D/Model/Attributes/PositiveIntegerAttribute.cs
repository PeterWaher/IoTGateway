using System;
using System.Xml;
using Waher.Content;

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
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public PositiveIntegerAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
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

	}
}
