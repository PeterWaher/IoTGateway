using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Int16 attribute.
	/// </summary>
	public class ReportInt16Attribute : ReportAttribute<short>
	{
		/// <summary>
		/// Report Int16 attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportInt16Attribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Int16 representation</param>
		/// <returns>Parsed value.</returns>
		public override short ParseValue(string s)
		{
			if (short.TryParse(s, out short Value))
				return Value;
			else
				throw new ArgumentException("Invalid short value: " + s, nameof(s));
		}
	}
}
