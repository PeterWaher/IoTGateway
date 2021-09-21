using System;
using System.Xml;
using Waher.Script;
using Waher.Things.Queries;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Table records reported.
	/// </summary>
	public class ReportTableRecords : ReportElement
	{
		private readonly string tableId;
		private readonly Record[] records;

		/// <summary>
		/// Table records reported.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		/// <param name="Records">Records</param>
		public ReportTableRecords(string TableId, Record[] Records)
		{
			this.tableId = TableId;
			this.records = Records;
		}

		/// <summary>
		/// Table ID
		/// </summary>
		public string TableId => this.tableId;

		/// <summary>
		/// Records
		/// </summary>
		public Record[] Records => this.records;

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteStartElement("Records");
			Output.WriteAttributeString("tableId", this.tableId);

			if (!(this.records is null))
			{
				foreach (Record Record in this.records)
				{
					Output.WriteStartElement("Record");

					foreach (object Item in Record.Elements)
					{
						if (!(Item is null))
							Output.WriteElementString("Item", Expression.ToString(Item));
					}

					Output.WriteEndElement();
				}
			}

			Output.WriteEndElement();
		}
	}
}
