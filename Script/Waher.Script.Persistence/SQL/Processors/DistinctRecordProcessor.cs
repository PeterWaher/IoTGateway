using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that generates a tabular record set of distinct records.
	/// </summary>
	public class DistinctRecordProcessor : RecordProcessor
	{
		private readonly Dictionary<Record, bool> reported = new Dictionary<Record, bool>();

		/// <summary>
		/// Processor that generates a tabular record set of distinct records.
		/// </summary>
		public DistinctRecordProcessor(ScriptNode[] Columns, Variables Variables)
			: base(Columns, Variables)
		{
		}

		/// <summary>
		/// Processes a record.
		/// </summary>
		/// <param name="Record">New record.</param>
		protected override void ProcessRecord(IElement[] Record)
		{
			Record Rec = new Record(Record);
			if (!this.reported.ContainsKey(Rec))
			{
				this.reported[Rec] = true;
				base.ProcessRecord(Record);
			}
		}
	}
}
