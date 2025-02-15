using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Class handling the reception of data from a query on multiple nodes.
	/// </summary>
	public class CompoundQuery : Query
	{
		private readonly IEnumerable<Query> queries;

		/// <summary>
		/// Class handling the reception of data from a query on multiple nodes.
		/// </summary>
		/// <param name="CommandId">Command ID</param>
		/// <param name="QueryId">Query ID</param>
		/// <param name="State">State object.</param>
		/// <param name="Language">Language of query.</param>
		/// <param name="Queries">Queries</param>
		/// <param name="Origin">Origin of request to execute the query.</param>
		public CompoundQuery(string CommandId, string QueryId, object State, Language Language, 
			IEnumerable<Query> Queries, RequestOrigin Origin)
			: base(CommandId, QueryId, State, Language, null, Origin)
		{
			this.queries = Queries;

			foreach (Query Query in this.queries)
			{
				Query.OnAborted += this.Query_OnAborted;
				Query.OnBeginSection += this.Query_OnBeginSection;
				Query.OnDone += this.Query_OnDone;
				Query.OnEndSection += this.Query_OnEndSection;
				Query.OnMessage += this.Query_OnMessage;
				Query.OnNewObject += this.Query_OnNewObject;
				Query.OnNewRecords += this.Query_OnNewRecords;
				Query.OnNewTable += this.Query_OnNewTable;
				Query.OnStarted += this.Query_OnStarted;
				Query.OnStatus += this.Query_OnStatus;
				Query.OnTableDone += this.Query_OnTableDone;
				Query.OnTitle += this.Query_OnTitle;
			}
		}

		private Task Query_OnTitle(object Sender, QueryTitleEventArgs e)
		{
			return this.SetTitle(e);
		}

		private Task Query_OnTableDone(object Sender, QueryTableEventArgs e)
		{
			return this.TableDone(e);
		}

		private Task Query_OnStatus(object Sender, QueryStatusEventArgs e)
		{
			return this.SetStatus(e);
		}

		private Task Query_OnStarted(object Sender, QueryEventArgs e)
		{
			return this.Start(e);
		}

		private Task Query_OnNewTable(object Sender, QueryNewTableEventArgs e)
		{
			return this.NewTable(e);
		}

		private Task Query_OnNewRecords(object Sender, QueryNewRecordsEventArgs e)
		{
			return this.NewRecords(e);
		}

		private Task Query_OnNewObject(object Sender, QueryObjectEventArgs e)
		{
			return this.NewObject(e);
		}

		private Task Query_OnMessage(object Sender, QueryMessageEventArgs e)
		{
			return this.LogMessage(e);
		}

		private Task Query_OnEndSection(object Sender, QueryEventArgs e)
		{
			return this.EndSection(e);
		}

		private Task Query_OnDone(object Sender, QueryEventArgs e)
		{
			return this.Done(e);
		}

		private Task Query_OnBeginSection(object Sender, QueryTitleEventArgs e)
		{
			return this.BeginSection(e);
		}

		private Task Query_OnAborted(object Sender, QueryEventArgs e)
		{
			return this.Abort(e);
		}

		/// <summary>
		/// Queries
		/// </summary>
		public IEnumerable<Query> Queries => this.queries;

		/// <summary>
		/// Aborts the query.
		/// </summary>
		public override async Task Abort()
		{
			await base.Abort();

			foreach (Query Query in this.queries)
				await Query.Abort();
		}
	}
}
