using System;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Base class for all query-related table events.
	/// </summary>
	public class QueryNewRecordsEventArgs : QueryTableEventArgs
	{
		private readonly Record[] records;

		/// <summary>
		/// Base class for all query-related table events.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Records">Records</param>
		public QueryNewRecordsEventArgs(Query Query, string ObjectId, params Record[] Records)
			: base(Query, ObjectId)
		{
			this.records = Records;
		}

		/// <summary>
		/// Records
		/// </summary>
		public Record[] Records
		{
			get { return this.records; }
		}
	}
}
