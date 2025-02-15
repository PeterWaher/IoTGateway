using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Int8 attribute.
	/// </summary>
	public class ReportInt8Attribute : ReportAttribute<sbyte>
	{
		/// <summary>
		/// Report Int8 attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportInt8Attribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Int8 representation</param>
		/// <returns>Parsed value.</returns>
		public override sbyte ParseValue(string s)
		{
			if (sbyte.TryParse(s, out sbyte Value))
				return Value;
			else
				throw new ArgumentException("Invalid sbyte value: " + s, nameof(s));
		}
	}
}
