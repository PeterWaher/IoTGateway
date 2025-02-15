using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Int64 attribute.
	/// </summary>
	public class ReportInt64Attribute : ReportAttribute<long>
	{
		/// <summary>
		/// Report Int64 attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportInt64Attribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Int64 representation</param>
		/// <returns>Parsed value.</returns>
		public override long ParseValue(string s)
		{
			if (long.TryParse(s, out long Value))
				return Value;
			else
				throw new ArgumentException("Invalid long value: " + s, nameof(s));
		}
	}
}
