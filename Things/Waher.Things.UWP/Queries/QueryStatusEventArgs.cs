using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Event arguments for query title events.
	/// </summary>
	public class QueryStatusEventArgs : QueryEventArgs
	{
		private string status;

		/// <summary>
		/// Event arguments for query title events.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Status">Status message.</param>
		public QueryStatusEventArgs(Query Query, string Status)
			: base(Query)
		{
			this.status = Status;
		}

		/// <summary>
		/// Status message.
		/// </summary>
		public string Status
		{
			get { return this.status; }
		}
	}
}
