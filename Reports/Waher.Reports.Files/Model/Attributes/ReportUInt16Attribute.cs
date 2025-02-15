using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report UInt16 attribute.
	/// </summary>
	public class ReportUInt16Attribute : ReportAttribute<ushort>
	{
		/// <summary>
		/// Report UInt16 attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportUInt16Attribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">UInt16 representation</param>
		/// <returns>Parsed value.</returns>
		public override ushort ParseValue(string s)
		{
			if (ushort.TryParse(s, out ushort Value))
				return Value;
			else
				throw new ArgumentException("Invalid ushort value: " + s, nameof(s));
		}
	}
}
