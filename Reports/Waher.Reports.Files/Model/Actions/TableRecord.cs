using System.Collections.Generic;
using System.Xml;
using Waher.Reports.Model.Attributes;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a column in a table.
	/// </summary>
	public class TableRecord
	{
		private readonly ReportStringAttribute[] elements;

		/// <summary>
		/// Defines a column in a table.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public TableRecord(XmlElement Xml)
		{
			List<ReportStringAttribute> Elements = new List<ReportStringAttribute>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Element")
					Elements.Add(new ReportStringAttribute(E, null));
			}

			this.elements = Elements.ToArray();
		}

	}
}
