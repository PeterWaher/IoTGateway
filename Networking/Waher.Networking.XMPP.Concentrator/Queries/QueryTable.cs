using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Represents a table in a query result report.
	/// </summary>
	public class QueryTable : QueryItem
	{
		private readonly Table tableDefinition;
		private readonly List<Record> records = new List<Record>();
		private Record[] recordsFixed = null;
		private bool isDone = false;

		/// <summary>
		/// Represents a table in a query result report.
		/// </summary>
		/// <param name="Parent">Parent item.</param>
		/// <param name="Table">Table definition</param>
		public QueryTable(QueryItem Parent, Table Table)
			: base(Parent)
		{
			this.tableDefinition = Table;
		}

		/// <summary>
		/// Table definitions.
		/// </summary>
		public Table TableDefinition => this.tableDefinition;

		/// <summary>
		/// Records
		/// </summary>
		public Record[] Records
		{
			get
			{
				Record[] Result = this.recordsFixed;
				if (Result != null)
					return Result;

				lock (this.records)
				{
					Result = this.recordsFixed = this.records.ToArray();
				}

				return Result;
			}
		}

		/// <summary>
		/// Adds records to the table.
		/// </summary>
		/// <param name="Records">Records</param>
		public void Add(Record[] Records)
		{
			lock (this.records)
			{
				this.records.AddRange(Records);
				this.recordsFixed = null;
			}
		}

		/// <summary>
		/// If the table is completed.
		/// </summary>
		public bool IsDone => this.isDone;

		/// <summary>
		/// Completes the table.
		/// </summary>
		public void Done()
		{
			this.isDone = true;
		}
	}
}
