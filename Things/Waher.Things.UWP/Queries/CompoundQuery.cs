using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Class handling the reception of data from a query on multiple nodes.
	/// </summary>
	public class CompoundQuery : Query
	{
		private IEnumerable<Query> queries;

		/// <summary>
		/// Class handling the reception of data from a query on multiple nodes.
		/// </summary>
		/// <param name="CommandId">Command ID</param>
		/// <param name="QueryId">Query ID</param>
		/// <param name="State">State object.</param>
		/// <param name="Language">Language of query.</param>
		/// <param name="Queries">Queries</param>
		public CompoundQuery(string CommandId, string QueryId, object State, Language Language, IEnumerable<Query> Queries)
			: base(CommandId, QueryId, State, Language, null)
		{
			this.queries = Queries;

			foreach (Query Query in this.queries)
			{
				Query.OnAborted += Query_OnAborted;
				Query.OnBeginSection += Query_OnBeginSection;
				Query.OnDone += Query_OnDone;
				Query.OnEndSection += Query_OnEndSection;
				Query.OnMessage += Query_OnMessage;
				Query.OnNewObject += Query_OnNewObject;
				Query.OnNewRecords += Query_OnNewRecords;
				Query.OnNewTable += Query_OnNewTable;
				Query.OnStarted += Query_OnStarted;
				Query.OnStatus += Query_OnStatus;
				Query.OnTableDone += Query_OnTableDone;
				Query.OnTitle += Query_OnTitle;
			}
		}

		private void Query_OnTitle(object Sender, QueryTitleEventArgs e)
		{
			this.SetTitle(e);
		}

		private void Query_OnTableDone(object Sender, QueryTableEventArgs e)
		{
			this.TableDone(e);
		}

		private void Query_OnStatus(object Sender, QueryStatusEventArgs e)
		{
			this.SetStatus(e);
		}

		private void Query_OnStarted(object Sender, QueryEventArgs e)
		{
			this.Start(e);
		}

		private void Query_OnNewTable(object Sender, QueryNewTableEventArgs e)
		{
			this.NewTable(e);
		}

		private void Query_OnNewRecords(object Sender, QueryNewRecordsEventArgs e)
		{
			this.NewRecords(e);
		}

		private void Query_OnNewObject(object Sender, QueryObjectEventArgs e)
		{
			this.NewObject(e);
		}

		private void Query_OnMessage(object Sender, QueryMessageEventArgs e)
		{
			this.LogMessage(e);
		}

		private void Query_OnEndSection(object Sender, QueryEventArgs e)
		{
			this.EndSection(e);
		}

		private void Query_OnDone(object Sender, QueryEventArgs e)
		{
			this.Done(e);
		}

		private void Query_OnBeginSection(object Sender, QueryTitleEventArgs e)
		{
			this.BeginSection(e);
		}

		private void Query_OnAborted(object Sender, QueryEventArgs e)
		{
			this.Abort(e);
		}

		/// <summary>
		/// Queries
		/// </summary>
		public IEnumerable<Query> Queries
		{
			get { return this.queries; }
		}

		/// <summary>
		/// Aborts the query.
		/// </summary>
		public override void Abort()
		{
			base.Abort();

			foreach (Query Query in this.queries)
				Query.Abort();
		}
	}
}
