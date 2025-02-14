using System;
using System.Xml;
using Waher.Content;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Boolean attribute.
	/// </summary>
	public class ReportBooleanAttribute : ReportAttribute<bool>
	{
		/// <summary>
		/// Report Boolean attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportBooleanAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Boolean representation</param>
		/// <returns>Parsed value.</returns>
		public override bool ParseValue(string s)
		{
			if (CommonTypes.TryParse(s, out bool b))
				return b;
			else
				throw new ArgumentException("Invalid Boolean value: " + s, nameof(s));
		}
	}
}
