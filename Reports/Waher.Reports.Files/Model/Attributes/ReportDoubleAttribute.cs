using System;
using System.Xml;
using Waher.Content;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Double attribute.
	/// </summary>
	public class ReportDoubleAttribute : ReportAttribute<double>
	{
		/// <summary>
		/// Report Double attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportDoubleAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Double representation</param>
		/// <returns>Parsed value.</returns>
		public override double ParseValue(string s)
		{
			if (CommonTypes.TryParse(s, out double Value))
				return Value;
			else
				throw new ArgumentException("Invalid double value: " + s, nameof(s));
		}
	}
}
