using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report object attribute.
	/// </summary>
	public class ReportObjectAttribute : ReportAttribute<object>
	{
		/// <summary>
		/// Report object attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportObjectAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Object representation</param>
		/// <returns>Parsed value.</returns>
		public override object ParseValue(string s)
		{
			return s;
		}
	}
}
