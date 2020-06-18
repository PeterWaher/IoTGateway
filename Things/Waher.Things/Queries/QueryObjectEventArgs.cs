using System;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Base class for all query-related table events.
	/// </summary>
	public class QueryObjectEventArgs : QueryEventArgs
	{
		private readonly object obj;

		/// <summary>
		/// Base class for all query-related table events.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Object">Object</param>
		public QueryObjectEventArgs(Query Query, object Object)
			: base(Query)
		{
			this.obj = Object;
		}

		/// <summary>
		/// Object.
		/// </summary>
		public object Object
		{
			get { return this.obj; }
		}
	}
}
