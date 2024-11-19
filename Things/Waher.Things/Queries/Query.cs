using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;

namespace Waher.Things.Queries
{
    /// <summary>
    /// Class handling the reception of data from a query.
    /// </summary>
    public class Query : IObservableLayer
	{
		private readonly INode nodeReference;
		private readonly Language language;
		private readonly string commandId;
		private readonly string queryId;
		private readonly object state;
		private bool isStarted = false;
		private bool isAborted = false;
		private bool isDone = false;
		private int seqNr = 0;
		private int nrSectionsBegun = 0;
		private int nrSectionsEnded = 0;
		private int nrTablesStarted = 0;
		private int nrTablesCompleted = 0;
		private int nrObjectsReported = 0;
		private int nrMessagesReported = 0;
		private int nrRecordsReported = 0;
		private bool hasTitle = false;

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
		public string CommandID => this.commandId;

		/// <summary>
		/// Query ID
		/// </summary>
		public string QueryID => this.queryId;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;

		/// <summary>
		/// Language of query.
		/// </summary>
		public Language Language => this.language;

		/// <summary>
		/// Node reference.
		/// </summary>
		public INode NodeReference => this.nodeReference;

		/// <summary>
		/// If the query has been started.
		/// </summary>
		public bool IsStarted => this.isStarted;

		/// <summary>
		/// If the query is aborted.
		/// </summary>
		public bool IsAborted => this.isAborted;

		/// <summary>
		/// If the query is done.
		/// </summary>
		public bool IsDone => this.isDone;

		/// <summary>
		/// Curernt sequence number counter.
		/// </summary>
		public int SequenceNumber => this.seqNr;

		/// <summary>
		/// Number of sectios begun.
		/// </summary>
		public int NrSectionsBegun => this.nrSectionsBegun;

		/// <summary>
		/// Number of sectios ended.
		/// </summary>
		public int NrSectionsEnded => this.nrSectionsEnded;

		/// <summary>
		/// Number of tables started.
		/// </summary>
		public int NrTablesStarted => this.nrTablesStarted;

		/// <summary>
		/// Number of tables compeleted.
		/// </summary>
		public int NrTablesCompleted => this.nrTablesCompleted;

		/// <summary>
		/// Number of objects reported.
		/// </summary>
		public int NrObjectsReported => this.nrObjectsReported;

		/// <summary>
		/// Number of messages reported.
		/// </summary>
		public int NrMessagesReported => this.nrMessagesReported;

		/// <summary>
		/// Number of records reported.
		/// </summary>
		public int NrRecordsReported => this.nrRecordsReported;

		/// <summary>
		/// Number of items (total) reported.
		/// </summary>
		public int NrTotalItemsReported
		{
			get
			{
				int Result = this.nrSectionsBegun;
				Result += this.nrSectionsEnded;
				Result += this.NrTablesStarted;
				Result += this.nrTablesCompleted;
				Result += this.nrObjectsReported;
				Result += this.nrMessagesReported;
				Result += this.nrRecordsReported;
				return Result;
			}
		}

		/// <summary>
		/// If a title has been set
		/// </summary>
		public bool HasTitle => this.hasTitle;

		/// <summary>
		/// If anything has been reported.
		/// </summary>
		public bool HasReported => this.NrTotalItemsReported > 0;

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
			if (!this.isAborted)
			{
				this.isAborted = true;
				await this.Raise(this.OnAborted, new QueryEventArgs(this), false);
			}
		}

		internal Task Abort(QueryEventArgs e)
		{
			if (!this.isAborted)
			{
				this.isAborted = true;
				return this.Raise(this.OnAborted, e, false);
			}
			else
				return Task.CompletedTask;
		}

		private async Task Raise(EventHandlerAsync<QueryEventArgs> Callback, QueryEventArgs e, bool CheckTerminated)
		{
			if (CheckTerminated && (this.isAborted || this.isDone))
				return;

			await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when the query has been aborted.
		/// </summary>
		public event EventHandlerAsync<QueryEventArgs> OnAborted = null;

		/// <summary>
		/// Starts query execution.
		/// </summary>
		public Task Start()
		{
			if (this.isStarted)
				return Task.CompletedTask;

			this.isStarted = true;
			return this.Raise(this.OnStarted, new QueryEventArgs(this), false);
		}

		internal Task Start(QueryEventArgs e)
		{
			if (!this.isStarted)
			{
				this.isStarted = true;
				return this.Raise(this.OnStarted, e, false);
			}
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when the query has been aborted.
		/// </summary>
		public event EventHandlerAsync<QueryEventArgs> OnStarted = null;

		/// <summary>
		/// Query execution completed.
		/// </summary>
		public async Task Done()
		{
			if (!this.isDone)
			{
				this.isDone = true;
				await this.Raise(this.OnDone, new QueryEventArgs(this), false);
			}
		}

		internal Task Done(QueryEventArgs e)
		{
			if (!this.isDone)
			{
				this.isDone = true;
				return this.Raise(this.OnDone, e, false);
			}
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when query has been completed.
		/// </summary>
		public event EventHandlerAsync<QueryEventArgs> OnDone = null;

		/// <summary>
		/// Defines a new table in the query output.
		/// </summary>
		/// <param name="TableId">ID of table.</param>
		/// <param name="TableName">Localized name of table.</param>
		/// <param name="Columns">Columns.</param>
		public Task NewTable(string TableId, string TableName, params Column[] Columns)
		{
			this.nrTablesStarted++;
			return this.Raise(this.OnNewTable, new QueryNewTableEventArgs(this, TableId, TableName, Columns));
		}

		internal Task NewTable(QueryNewTableEventArgs e)
		{
			this.nrTablesStarted++;
			return this.Raise(this.OnNewTable, e);
		}

		private async Task Raise(EventHandlerAsync<QueryNewTableEventArgs> Callback, QueryNewTableEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a new table has been created.
		/// </summary>
		public event EventHandlerAsync<QueryNewTableEventArgs> OnNewTable = null;

		/// <summary>
		/// Reports a new set of records in a table.
		/// </summary>
		/// <param name="TableId">Table ID</param>
		/// <param name="Records">New records.</param>
		public Task NewRecords(string TableId, params Record[] Records)
		{
			this.nrRecordsReported += Records.Length;
			return this.Raise(this.OnNewRecords, new QueryNewRecordsEventArgs(this, TableId, Records));
		}

		internal Task NewRecords(QueryNewRecordsEventArgs e)
		{
			this.nrRecordsReported += e.Records.Length;
			return this.Raise(this.OnNewRecords, e);
		}

		private async Task Raise(EventHandlerAsync<QueryNewRecordsEventArgs> Callback, QueryNewRecordsEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when new records are reported for a table.
		/// </summary>
		public event EventHandlerAsync<QueryNewRecordsEventArgs> OnNewRecords = null;

		/// <summary>
		/// Reports a table as being complete.
		/// </summary>
		/// <param name="TableId">ID of table.</param>
		public Task TableDone(string TableId)
		{
			this.nrTablesCompleted++;
			return this.Raise(this.OnTableDone, new QueryTableEventArgs(this, TableId));
		}

		internal Task TableDone(QueryTableEventArgs e)
		{
			this.nrTablesCompleted++;
			return this.Raise(this.OnTableDone, e);
		}

		private async Task Raise(EventHandlerAsync<QueryTableEventArgs> Callback, QueryTableEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a table is completed.
		/// </summary>
		public event EventHandlerAsync<QueryTableEventArgs> OnTableDone = null;

		/// <summary>
		/// Reports a new object.
		/// </summary>
		/// <param name="Object">Object</param>
		public Task NewObject(object Object)
		{
			this.nrObjectsReported++;
			return this.Raise(this.OnNewObject, new QueryObjectEventArgs(this, Object));
		}

		internal Task NewObject(QueryObjectEventArgs e)
		{
			this.nrObjectsReported++;
			return this.Raise(this.OnNewObject, e);
		}

		private async Task Raise(EventHandlerAsync<QueryObjectEventArgs> Callback, QueryObjectEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when new records are reported for a table.
		/// </summary>
		public event EventHandlerAsync<QueryObjectEventArgs> OnNewObject = null;

		/// <summary>
		/// Logs an Exception as a query message.
		/// </summary>
		/// <param name="Exception">Exception being logged.</param>
		public Task LogMessage(Exception Exception)
		{
			return this.LogMessage(QueryEventLevel.Major, Exception);
		}

		/// <summary>
		/// Logs an Exception as a query message.
		/// </summary>
		/// <param name="Level">Event level.</param>
		/// <param name="Exception">Exception being logged.</param>
		public Task LogMessage(QueryEventLevel Level, Exception Exception)
		{
			Exception = Log.UnnestException(Exception);
			return this.LogMessage(QueryEventType.Exception, Level, Exception.Message);
		}

		/// <summary>
		/// Logs a query message.
		/// </summary>
		/// <param name="Type">Event type.</param>
		/// <param name="Level">Event level.</param>
		/// <param name="Body">Event message body.</param>
		public Task LogMessage(QueryEventType Type, QueryEventLevel Level, string Body)
		{
			this.nrMessagesReported++;
			return this.Raise(this.OnMessage, new QueryMessageEventArgs(this, Type, Level, Body));
		}

		internal Task LogMessage(QueryMessageEventArgs e)
		{
			this.nrMessagesReported++;
			return this.Raise(this.OnMessage, e);
		}

		private async Task Raise(EventHandlerAsync<QueryMessageEventArgs> Callback, QueryMessageEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a new message has been received.
		/// </summary>
		public event EventHandlerAsync<QueryMessageEventArgs> OnMessage = null;

		/// <summary>
		/// Sets the title of the report.
		/// </summary>
		/// <param name="Title">Title.</param>
		public Task SetTitle(string Title)
		{
			this.hasTitle = true;
			return this.Raise(this.OnTitle, new QueryTitleEventArgs(this, Title));
		}

		internal Task SetTitle(QueryTitleEventArgs e)
		{
			this.hasTitle = true;
			return this.Raise(this.OnTitle, e);
		}

		private async Task Raise(EventHandlerAsync<QueryTitleEventArgs> Callback, QueryTitleEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await Callback.Raise(this, e);
		}

		/// <summary>
		/// Event raised when the report title has been set.
		/// </summary>
		public event EventHandlerAsync<QueryTitleEventArgs> OnTitle = null;

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

		private async Task Raise(EventHandlerAsync<QueryStatusEventArgs> h, QueryStatusEventArgs e)
		{
			if (!this.isAborted && !this.isDone)
				await h.Raise(this, e);
		}

		/// <summary>
		/// Event raised when the current status changes.
		/// </summary>
		public event EventHandlerAsync<QueryStatusEventArgs> OnStatus = null;

		/// <summary>
		/// Begins a new section. Sections can be nested.
		/// Each call to <see cref="BeginSection(string)"/> must be followed by a call to <see cref="EndSection()"/>.
		/// </summary>
		/// <param name="Header">Section Title.</param>
		public Task BeginSection(string Header)
		{
			this.nrSectionsBegun++;
			return this.Raise(this.OnBeginSection, new QueryTitleEventArgs(this, Header));
		}

		internal Task BeginSection(QueryTitleEventArgs e)
		{
			this.nrSectionsBegun++;
			return this.Raise(this.OnBeginSection, e);
		}

		/// <summary>
		/// Event raised when a new section is created.
		/// </summary>
		public event EventHandlerAsync<QueryTitleEventArgs> OnBeginSection = null;

		/// <summary>
		/// Ends a section.
		/// Each call to <see cref="BeginSection(string)"/> must be followed by a call to <see cref="EndSection()"/>.
		/// </summary>
		public Task EndSection()
		{
			this.nrSectionsEnded++;
			return this.Raise(this.OnEndSection, new QueryEventArgs(this), true);
		}

		internal Task EndSection(QueryEventArgs e)
		{
			this.nrSectionsEnded++;
			return this.Raise(this.OnEndSection, e, true);
		}

		/// <summary>
		/// Event raised when a section is closed.
		/// </summary>
		public event EventHandlerAsync<QueryEventArgs> OnEndSection = null;

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => false;

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public Task Information(string Comment) => this.LogMessage(QueryEventType.Information, QueryEventLevel.Minor, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public Task Warning(string Warning) => this.LogMessage(QueryEventType.Warning, QueryEventLevel.Minor, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public Task Error(string Error) => this.LogMessage(QueryEventType.Error, QueryEventLevel.Minor, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(Exception Exception) => this.LogMessage(QueryEventType.Exception, QueryEventLevel.Minor, Exception.Message);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(string Exception) => this.LogMessage(QueryEventType.Exception, QueryEventLevel.Minor, Exception);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public Task Information(DateTime Timestamp, string Comment) => this.LogMessage(QueryEventType.Information, QueryEventLevel.Minor, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public Task Warning(DateTime Timestamp, string Warning) => this.LogMessage(QueryEventType.Warning, QueryEventLevel.Minor, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public Task Error(DateTime Timestamp, string Error) => this.LogMessage(QueryEventType.Error, QueryEventLevel.Minor, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public Task Exception(DateTime Timestamp, string Exception) => this.LogMessage(QueryEventType.Exception, QueryEventLevel.Minor, Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public Task Exception(DateTime Timestamp, Exception Exception) => this.LogMessage(QueryEventType.Exception, QueryEventLevel.Minor, Exception.Message);

		#endregion
	}
}
