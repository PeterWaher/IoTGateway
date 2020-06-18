using System;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Represents a table in a query result.
	/// </summary>
    public class Table
    {
		private readonly string tableId;
		private readonly string name;
		private readonly Column[] columns;

		/// <summary>
		/// Represents a table in a query result.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		/// <param name="Name">Table name</param>
		/// <param name="Columns">Columns</param>
		public Table(string TableId, string Name, Column[] Columns)
		{
			this.tableId = TableId;
			this.name = Name;
			this.columns = Columns;
		}

		/// <summary>
		/// Table ID
		/// </summary>
		public string TableId => this.tableId;

		/// <summary>
		/// Table name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Columns
		/// </summary>
		public Column[] Columns => this.columns;
	}
}
