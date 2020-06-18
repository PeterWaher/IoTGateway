using System;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Base class for all query-related table events.
	/// </summary>
	public class QueryNewTableEventArgs : QueryTableEventArgs
	{
		private readonly string tableName;
		private readonly Column[] columns;

		/// <summary>
		/// Base class for all query-related table events.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="TableId">Table ID.</param>
		/// <param name="Columns">Localized table name.</param>
		/// <param name="TableName">Columns.</param>
		public QueryNewTableEventArgs(Query Query, string TableId, string TableName, params Column[] Columns)
			: base(Query, TableId)
		{
			this.tableName = TableName;
			this.columns = Columns;
		}

		/// <summary>
		/// Localized Table Name.
		/// </summary>
		public string TableName
		{
			get { return this.tableName; }
		}

		/// <summary>
		/// Columns
		/// </summary>
		public Column[] Columns
		{
			get { return this.columns; }
		}
	}
}
