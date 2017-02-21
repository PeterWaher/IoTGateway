using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Base class for all query-related events.
	/// </summary>
	public class QueryEventArgs : EventArgs
	{
		private Query query;

		/// <summary>
		/// Base class for all query-related events.
		/// </summary>
		/// <param name="Query">Query.</param>
		public QueryEventArgs(Query Query)
		{
			this.query = Query;
		}

		/// <summary>
		/// Query originating the event.
		/// </summary>
		public Query Query
		{
			get { return this.query; }
		}
	}
}
