using System;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report Int32 attribute.
	/// </summary>
	public class ReportInt32Attribute : ReportAttribute<int>
	{
		/// <summary>
		/// Report Int32 attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportInt32Attribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Int32 representation</param>
		/// <returns>Parsed value.</returns>
		public override int ParseValue(string s)
		{
			if (int.TryParse(s, out int Value))
				return Value;
			else
				throw new ArgumentException("Invalid int value: " + s, nameof(s));
		}
	}
}
