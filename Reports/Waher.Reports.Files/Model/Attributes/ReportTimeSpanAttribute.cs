using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report TimeSpan attribute.
	/// </summary>
	public class ReportTimeSpanAttribute : ReportAttribute<TimeSpan>
	{
		/// <summary>
		/// Report TimeSpan attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportTimeSpanAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">TimeSpan representation</param>
		/// <returns>Parsed value.</returns>
		public override TimeSpan ParseValue(string s)
		{
			if (TimeSpan.TryParse(s, out TimeSpan Value))
				return Value;
			else
				throw new ArgumentException("Invalid TimeSpan value: " + s, nameof(s));
		}
	}
}
