using System;
using System.Collections.Generic;
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
			this.columns = null;
		}

		/// <summary>
		/// Table records reported.
		/// </summary>
		/// <param name="Xml">XML Definition.</param>
		/// <param name="ColumnsByTableId">Available column definitions</param>
		public ReportTableRecords(XmlElement Xml, Dictionary<string, Column[]> ColumnsByTableId)
		{
			this.tableId = XML.Attribute(Xml, "tableId");
		
			ColumnsByTableId.TryGetValue(this.tableId, out this.columns);

			List<Record> Records = new List<Record>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Record")
				{
					List<object> Items = new List<object>();

					foreach (XmlNode N2 in E.ChildNodes)
					{
						if (N2 is XmlElement E2)
						{
							switch (E2.LocalName)
							{
								case "String":
								default:
									Items.Add(E2.InnerText);
									break;

								case "Double":
									if (CommonTypes.TryParse(E2.InnerText, out double d))
										Items.Add(d);
									else
										Items.Add(E2.InnerText);
									break;

								case "Decimal":
									if (CommonTypes.TryParse(E2.InnerText, out decimal dec))
										Items.Add(dec);
									else
										Items.Add(E2.InnerText);
									break;

								case "Single":
									if (CommonTypes.TryParse(E2.InnerText, out float f))
										Items.Add(f);
									else
										Items.Add(E2.InnerText);
									break;

								case "Boolean":
									if (CommonTypes.TryParse(E2.InnerText, out bool b))
										Items.Add(b);
									else
										Items.Add(E2.InnerText);
									break;

								case "Date":
								case "DateTime":
									if (XML.TryParse(E2.InnerText, out DateTime TP))
										Items.Add(TP);
									else
										Items.Add(E2.InnerText);
									break;

								case "TimeSpan":
									if (TimeSpan.TryParse(E2.InnerText, out TimeSpan TS))
										Items.Add(TS);
									else
										Items.Add(E2.InnerText);
									break;

								case "UI8":
									if (byte.TryParse(E2.InnerText, out byte ui8))
										Items.Add(ui8);
									else
										Items.Add(E2.InnerText);
									break;

								case "UI16":
									if (ushort.TryParse(E2.InnerText, out ushort ui16))
										Items.Add(ui16);
									else
										Items.Add(E2.InnerText);
									break;

								case "UI32":
									if (uint.TryParse(E2.InnerText, out uint ui32))
										Items.Add(ui32);
									else
										Items.Add(E2.InnerText);
									break;

								case "UI64":
									if (ulong.TryParse(E2.InnerText, out ulong ui64))
										Items.Add(ui64);
									else
										Items.Add(E2.InnerText);
									break;

								case "I8":
									if (sbyte.TryParse(E2.InnerText, out sbyte i8))
										Items.Add(i8);
									else
										Items.Add(E2.InnerText);
									break;

								case "I16":
									if (short.TryParse(E2.InnerText, out short i16))
										Items.Add(i16);
									else
										Items.Add(E2.InnerText);
									break;

								case "I32":
									if (int.TryParse(E2.InnerText, out int i32))
										Items.Add(i32);
									else
										Items.Add(E2.InnerText);
									break;

								case "I64":
									if (long.TryParse(E2.InnerText, out long i64))
										Items.Add(i64);
									else
										Items.Add(E2.InnerText);
									break;

								case "Null":
									Items.Add(null);
									break;
							}
						}
					}

					Records.Add(new Record(Items.ToArray()));
				}
			}

			this.records = Records.ToArray();
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
