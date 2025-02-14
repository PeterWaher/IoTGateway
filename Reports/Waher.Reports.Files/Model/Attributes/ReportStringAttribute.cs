using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report string attribute.
	/// </summary>
	public class ReportStringAttribute : ReportAttribute<string>
	{
		/// <summary>
		/// Report string attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportStringAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">String representation</param>
		/// <returns>Parsed value.</returns>
		public override string ParseValue(string s)
		{
			return s;
		}
	}
}
