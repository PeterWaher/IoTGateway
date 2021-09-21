using System;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
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
		private readonly Column[] columns;

		/// <summary>
		/// Table records reported.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		/// <param name="Records">Records</param>
		/// <param name="Columns">Column definitions</param>
		public ReportTableRecords(string TableId, Record[] Records, Column[] Columns)
		{
			this.tableId = TableId;
			this.records = Records;
			this.columns = Columns;
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

					int Column = 0;

					foreach (object Item in Record.Elements)
					{
						if (Item is null)
							Output.WriteElementString("Null", string.Empty);
						else if (Item is string s)
							Output.WriteElementString("String", s);
						else if (Item is double d)
						{
							if (Column >= (this.columns?.Length ?? 0) || !this.columns[Column].NrDecimals.HasValue)
								Output.WriteElementString("Double", CommonTypes.Encode(d));
							else
								Output.WriteElementString("Double", CommonTypes.Encode(d, this.columns[Column].NrDecimals.Value));
						}
						else if (Item is decimal dec)
						{
							if (Column >= (this.columns?.Length ?? 0) || !this.columns[Column].NrDecimals.HasValue)
								Output.WriteElementString("Decimal", CommonTypes.Encode(dec));
							else
								Output.WriteElementString("Double", CommonTypes.Encode(dec, this.columns[Column].NrDecimals.Value));
						}
						else if (Item is float f)
						{
							if (Column >= (this.columns?.Length ?? 0) || !this.columns[Column].NrDecimals.HasValue)
								Output.WriteElementString("Single", CommonTypes.Encode(f));
							else
								Output.WriteElementString("Double", CommonTypes.Encode(f, this.columns[Column].NrDecimals.Value));
						}
						else if (Item is bool b)
							Output.WriteElementString("Boolean", CommonTypes.Encode(b));
						else if (Item is DateTime DT)
						{
							if (DT.TimeOfDay == TimeSpan.Zero)
								Output.WriteElementString("Date", XML.Encode(DT, true));
							else
								Output.WriteElementString("DateTime", XML.Encode(DT));
						}
						else if (Item is TimeSpan TS)
							Output.WriteElementString("TimeSpan", TS.ToString());
						else if (Item is byte UI8)
							Output.WriteElementString("UI8", UI8.ToString());
						else if (Item is ushort UI16)
							Output.WriteElementString("UI16", UI16.ToString());
						else if (Item is uint UI32)
							Output.WriteElementString("UI32", UI32.ToString());
						else if (Item is ulong UI64)
							Output.WriteElementString("UI64", UI64.ToString());
						else if (Item is sbyte I8)
							Output.WriteElementString("I8", I8.ToString());
						else if (Item is short I16)
							Output.WriteElementString("I16", I16.ToString());
						else if (Item is int I32)
							Output.WriteElementString("I32", I32.ToString());
						else if (Item is long I64)
							Output.WriteElementString("I64", I64.ToString());
						else
							Output.WriteElementString("String", Item.ToString());

						Column++;
					}

					Output.WriteEndElement();
				}
			}

			Output.WriteEndElement();
		}
	}
}
