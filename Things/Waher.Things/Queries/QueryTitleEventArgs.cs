using System;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Event arguments for query title events.
	/// </summary>
	public class QueryTitleEventArgs : QueryEventArgs
	{
		private readonly string title;

		/// <summary>
		/// Event arguments for query title events.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Title">Title.</param>
		public QueryTitleEventArgs(Query Query, string Title)
			: base(Query)
		{
			this.title = Title;
		}

		/// <summary>
		/// Title.
		/// </summary>
		public string Title
		{
			get { return this.title; }
		}
	}
}
