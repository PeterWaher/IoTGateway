using System;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report DateTime attribute.
	/// </summary>
	public class ReportDateTimeAttribute : ReportAttribute<DateTime>
	{
		/// <summary>
		/// Report DateTime attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportDateTimeAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">DateTime representation</param>
		/// <returns>Parsed value.</returns>
		public override DateTime ParseValue(string s)
		{
			if (XML.TryParse(s, out DateTime Value))
				return Value;
			else if (CommonTypes.TryParseRfc822(s, out DateTimeOffset Value2))
				return Value2.DateTime;
			else if (DateTime.TryParse(s, out Value))
				return Value;
			else
				throw new ArgumentException("Invalid DateTime value: " + s, nameof(s));
		}
	}
}
