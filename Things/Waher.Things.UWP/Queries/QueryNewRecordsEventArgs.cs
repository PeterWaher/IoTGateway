using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Base class for all query-related table events.
	/// </summary>
	public class QueryNewRecordsEventArgs : QueryTableEventArgs
	{
		private Record[] records;

		/// <summary>
		/// Base class for all query-related table events.
		/// </summary>
		/// <param name="Query">Query.</param>
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
