using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Byte attribute.
	/// </summary>
	public class ReportByteAttribute : ReportAttribute<decimal>
	{
		/// <summary>
		/// Report Byte attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportByteAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Byte representation</param>
		/// <returns>Parsed value.</returns>
		public override decimal ParseValue(string s)
		{
			if (byte.TryParse(s, out byte Value))
				return Value;
			else
				throw new ArgumentException("Invalid byte value: " + s, nameof(s));
		}
	}
}
