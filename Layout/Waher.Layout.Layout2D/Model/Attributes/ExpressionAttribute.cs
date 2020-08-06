using System;
using System.Xml;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Event attribute
	/// </summary>
	public class ExpressionAttribute : Attribute<Expression>
	{
		/// <summary>
		/// Event attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public ExpressionAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, false)
		{
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out Expression Value)
		{
			try
			{
				Value = new Expression(StringValue);
				return true;
			}
			catch (Exception)
			{
				Value = null;
				return false;
			}
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(Expression Value)
		{
			return Value.Script;
		}

	}
}
