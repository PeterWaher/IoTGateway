using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Delegate for query event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query event arguments.</param>
	public delegate Task QueryEventHandler(object Sender, QueryEventArgs e);

	/// <summary>
	/// Delegate for query table event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query table event arguments.</param>
	public delegate Task QueryTableEventHandler(object Sender, QueryTableEventArgs e);

	/// <summary>
	/// Delegate for query new table event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query new table event arguments.</param>
	public delegate Task QueryNewTableEventHandler(object Sender, QueryNewTableEventArgs e);

	/// <summary>
	/// Delegate for query new records event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query new records event arguments.</param>
	public delegate Task QueryNewRecordsEventHandler(object Sender, QueryNewRecordsEventArgs e);

	/// <summary>
	/// Delegate for query object event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query object arguments.</param>
	public delegate Task QueryObjectEventHandler(object Sender, QueryObjectEventArgs e);

	/// <summary>
	/// Delegate for query message event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query message arguments.</param>
	public delegate Task QueryMessageEventHandler(object Sender, QueryMessageEventArgs e);

	/// <summary>
	/// Delegate for query title event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query title arguments.</param>
	public delegate Task QueryTitleEventHandler(object Sender, QueryTitleEventArgs e);

	/// <summary>
	/// Delegate for query status event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Query status arguments.</param>
	public delegate Task QueryStatusEventHandler(object Sender, QueryStatusEventArgs e);

	/// <summary>
	/// Class handling the reception of data from a query.
	/// </summary>
	public class Query
	{
		private readonly INode nodeReference;
		private readonly Language language;
		private readonly string commandId;
		private readonly string queryId;
		private readonly object state;
		private bool isAborted = false;
		private bool isDone = false;
		private int seqNr = 0;

		/// <summary>
		/// Class handling the reception of data from a query.
		/// </summary>
		/// <param name="CommandId">Command ID</param>
		/// <param name="QueryId">Query ID</param>
		/// <param name="State">State object.</param>
		/// <param name="Language">Language of query.</param>
		/// <param name="NodeReference">Node reference.</param>
		public Query(string CommandId, string QueryId, object State, Language Language, INode NodeReference)
		{
			this.nodeReference = NodeReference;
			this.commandId = CommandId;
			this.queryId = QueryId;
			this.state = State;
			this.language = Language;
		}

		/// <summary>
		/// Command ID
		/// </summary>
		public string CommandID
		{
			get { return this.commandId; }
		}

		/// <summary>
		/// Query ID
		/// </summary>
		public string QueryID
		{
			get { return this.queryId; }
		}

		/// <summary>
		/// State object.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}

		/// <summary>
		/// Language of query.
		/// </summary>
		public Language Language
		{
			get { return this.language; }
		}

		/// <summary>
		/// Node reference.
		/// </summary>
		public INode NodeReference
		{
			get { return this.nodeReference; }
		}

		/// <summary>
		/// If the query is aborted.
		/// </summary>
		public bool IsAborted
		{
			get { return this.isAborted; }
		}

		/// <summary>
		/// If the query is done.
		/// </summary>
		public bool IsDone
		{
			get { return this.isDone; }
		}

		/// <summary>
		/// Curernt sequence number counter.
		/// </summary>
		public int SequenceNumber
		{
			get { return this.seqNr; }
		}

		/// <summary>
		/// Gets the next sequence number.
		/// </summary>
		/// <returns>Next sequence number.</returns>
		public int NextSequenceNumber()
		{
			return ++this.seqNr;
		}

		/// <summary>
		/// Aborts the query.
		/// </summary>
		public virtual async Task Abort()
		{
			await this.Raise(this.OnAborted, new QueryEventArgs(this));
			this.isAborted = true;
		}

		internal Task Abort(QueryEventArgs e)
		{
			return this.Raise(this.OnAborted, e);
		}

		private async Task Raise(QueryEventHandler Callback, QueryEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when the query has been aborted.
		/// </summary>
		public event QueryEventHandler OnAborted = null;

		/// <summary>
		/// Starts query execution.
		/// </summary>
		public Task Start()
		{
			return this.Raise(this.OnStarted, new QueryEventArgs(this));
		}

		internal Task Start(QueryEventArgs e)
		{
			return this.Raise(this.OnStarted, e);
		}

		/// <summary>
		/// Event raised when the query has been aborted.
		/// </summary>
		public event QueryEventHandler OnStarted = null;

		/// <summary>
		/// Query execution completed.
		/// </summary>
		public async Task Done()
		{
			await this.Raise(this.OnDone, new QueryEventArgs(this));
			this.isDone = true;
		}

		internal Task Done(QueryEventArgs e)
		{
			return this.Raise(this.OnDone, e);
		}

		/// <summary>
		/// Event raised when query has been completed.
		/// </summary>
		public event QueryEventHandler OnDone = null;

		/// <summary>
		/// Defines a new table in the query output.
		/// </summary>
		/// <param name="TableId">ID of table.</param>
		/// <param name="TableName">Localized name of table.</param>
		/// <param name="Columns">Columns.</param>
		public Task NewTable(string TableId, string TableName, params Column[] Columns)
		{
			return this.Raise(this.OnNewTable, new QueryNewTableEventArgs(this, TableId, TableName, Columns));
		}

		internal Task NewTable(QueryNewTableEventArgs e)
		{
			return this.Raise(this.OnNewTable, e);
		}

		private async Task Raise(QueryNewTableEventHandler Callback, QueryNewTableEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a new table has been created.
		/// </summary>
		public event QueryNewTableEventHandler OnNewTable = null;

		/// <summary>
		/// Reports a new set of records in a table.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		/// <param name="Records">New records.</param>
		public Task NewRecords(string TableId, params Record[] Records)
		{
			return this.Raise(this.OnNewRecords, new QueryNewRecordsEventArgs(this, TableId, Records));
		}

		internal Task NewRecords(QueryNewRecordsEventArgs e)
		{
			return this.Raise(this.OnNewRecords, e);
		}

		private async Task Raise(QueryNewRecordsEventHandler Callback, QueryNewRecordsEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when new records are reported for a table.
		/// </summary>
		public event QueryNewRecordsEventHandler OnNewRecords = null;

		/// <summary>
		/// Reports a table as being complete.
		/// </summary>
		/// <param name="TableId">ID of table.</param>
		public Task TableDone(string TableId)
		{
			return this.Raise(this.OnTableDone, new QueryTableEventArgs(this, TableId));
		}

		internal Task TableDone(QueryTableEventArgs e)
		{
			return this.Raise(this.OnTableDone, e);
		}

		private async Task Raise(QueryTableEventHandler Callback, QueryTableEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a table is completed.
		/// </summary>
		public event QueryTableEventHandler OnTableDone = null;

		/// <summary>
		/// Reports a new object.
		/// </summary>
		/// <param name="Object">Object</param>
		public Task NewObject(object Object)
		{
			return this.Raise(this.OnNewObject, new QueryObjectEventArgs(this, Object));
		}

		internal Task NewObject(QueryObjectEventArgs e)
		{
			return this.Raise(this.OnNewObject, e);
		}

		private async Task Raise(QueryObjectEventHandler Callback, QueryObjectEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when new records are reported for a table.
		/// </summary>
		public event QueryObjectEventHandler OnNewObject = null;

		/// <summary>
		/// Logs a query message.
		/// </summary>
		/// <param name="Type">Event type.</param>
		/// <param name="Level">Event level.</param>
		/// <param name="Body">Event message body.</param>
		public Task LogMessage(QueryEventType Type, QueryEventLevel Level, string Body)
		{
			return this.Raise(this.OnMessage, new QueryMessageEventArgs(this, Type, Level, Body));
		}

		internal Task LogMessage(QueryMessageEventArgs e)
		{
			return this.Raise(this.OnMessage, e);
		}

		private async Task Raise(QueryMessageEventHandler Callback, QueryMessageEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a new message has been received.
		/// </summary>
		public event QueryMessageEventHandler OnMessage = null;

		/// <summary>
		/// Sets the title of the report.
		/// </summary>
		/// <param name="Title">Title.</param>
		public Task SetTitle(string Title)
		{
			return this.Raise(this.OnTitle, new QueryTitleEventArgs(this, Title));
		}

		internal Task SetTitle(QueryTitleEventArgs e)
		{
			return this.Raise(this.OnTitle, e);
		}

		private async Task Raise(QueryTitleEventHandler Callback, QueryTitleEventArgs e)
		{
			if (!this.isAborted && !this.isDone && Callback != null)
			{
				try
				{
					await Callback(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when the report title has been set.
		/// </summary>
		public event QueryTitleEventHandler OnTitle = null;

		/// <summary>
		/// Sets the current status of the query execution.
		/// </summary>
		/// <param name="Status">Status message.</param>
		public Task SetStatus(string Status)
		{
			return this.Raise(this.OnStatus, new QueryStatusEventArgs(this, Status));
		}

		internal Task SetStatus(QueryStatusEventArgs e)
		{
			return this.Raise(this.OnStatus, e);
		}

		private async Task Raise(QueryStatusEventHandler h, QueryStatusEventArgs e)
		{
			if (!this.isAborted && !this.isDone && h != null)
			{
				try
				{
					await h(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when the current status changes.
		/// </summary>
		public event QueryStatusEventHandler OnStatus = null;

		/// <summary>
		/// Begins a new section. Sections can be nested.
		/// Each call to <see cref="BeginSection(string)"/> must be followed by a call to <see cref="EndSection()"/>.
		/// </summary>
		/// <param name="Header">Section Title.</param>
		public Task BeginSection(string Header)
		{
			return this.Raise(this.OnBeginSection, new QueryTitleEventArgs(this, Header));
		}

		internal Task BeginSection(QueryTitleEventArgs e)
		{
			return this.Raise(this.OnBeginSection, e);
		}

		/// <summary>
		/// Event raised when a new section is created.
		/// </summary>
		public event QueryTitleEventHandler OnBeginSection = null;

		/// <summary>
		/// Ends a section.
		/// Each call to <see cref="BeginSection(string)"/> must be followed by a call to <see cref="EndSection()"/>.
		/// </summary>
		public Task EndSection()
		{
			return this.Raise(this.OnEndSection, new QueryEventArgs(this));
		}

		internal Task EndSection(QueryEventArgs e)
		{
			return this.Raise(this.OnEndSection, e);
		}

		/// <summary>
		/// Event raised when a section is closed.
		/// </summary>
		public event QueryEventHandler OnEndSection = null;
	}
}
