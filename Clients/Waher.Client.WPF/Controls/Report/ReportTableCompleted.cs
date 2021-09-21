using System;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Completion of a table.
	/// </summary>
	public class ReportTableCompleted : ReportElement
	{
		private readonly string tableId;

		/// <summary>
		/// Completion of a table.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		public ReportTableCompleted(string TableId)
		{
			this.tableId = TableId;
		}

		/// <summary>
		/// Completion of a table.
		/// </summary>
		/// <param name="Xml">XML Definition.</param>
		public ReportTableCompleted(XmlElement Xml)
		{
			this.tableId = XML.Attribute(Xml, "tableId");
		}

		/// <summary>
		/// Table ID
		/// </summary>
		public string TableId => this.tableId;

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteStartElement("TableEnd");
			Output.WriteAttributeString("tableId", this.tableId);
			Output.WriteEndElement();
		}
	}
}
