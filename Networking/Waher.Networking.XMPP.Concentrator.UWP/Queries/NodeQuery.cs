using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Client-side Node Query object. It collects the results of the query.
	/// </summary>
	public class NodeQuery : IDisposable
	{
		private readonly string queryId;
		private readonly string to;
		private readonly string nodeID;
		private readonly string sourceID;
		private readonly string partition;
		private readonly string command;
		private readonly string language;
		private readonly string serviceToken;
		private readonly string deviceToken;
		private readonly string userToken;
		private readonly ConcentratorClient client;
		private bool isDone = false;
		private int seqNr = 0;
		private string title = string.Empty;
		private readonly List<QueryItem> result = new List<QueryItem>();
		private QueryItem[] resultFixed = null;
		private readonly Dictionary<string, QueryTable> tables = new Dictionary<string, QueryTable>();
		private QuerySection currentSection = null;
		private LinkedList<KeyValuePair<int, MessageEventArgs>> queuedMessages = null;

		/// <summary>
		/// Client-side Node Query object. It collects the results of the query.
		/// </summary>
		/// <param name="Client">Concentrator client.</param>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		public NodeQuery(ConcentratorClient Client, string To, string NodeID, string SourceID, string Partition, string Command,
			string Language, string ServiceToken, string DeviceToken, string UserToken)
		{
			this.queryId = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.client = Client;
			this.to = To;
			this.nodeID = NodeID;
			this.sourceID = SourceID;
			this.partition = Partition;
			this.command = Command;
			this.language = Language;
			this.serviceToken = ServiceToken;
			this.deviceToken = DeviceToken;
			this.userToken = UserToken;
		}

		/// <summary>
		/// Query ID
		/// </summary>
		public string QueryId => this.queryId;

		/// <summary>
		/// Address of concentrator server.
		/// </summary>
		public string To => this.to;

		/// <summary>
		/// Node ID
		/// </summary>
		public string NodeID => this.nodeID;

		/// <summary>
		/// Optional Source ID
		/// </summary>
		public string SourceID => this.sourceID;

		/// <summary>
		/// Optional Partition
		/// </summary>
		public string Partition => this.partition;

		/// <summary>
		/// Command for which to get parameters.
		/// </summary>
		public string Command => this.command;

		/// <summary>
		/// Code of desired language.
		/// </summary>
		public string Language => this.language;

		/// <summary>
		/// Optional Service token.
		/// </summary>
		public string ServiceToken => this.serviceToken;

		/// <summary>
		/// Optional Device token.
		/// </summary>
		public string DeviceToken => this.deviceToken;

		/// <summary>
		/// Optional User token.
		/// </summary>
		public string UserToken => this.userToken;

		/// <summary>
		/// Concentrator client managing the query.
		/// </summary>
		public ConcentratorClient Client => this.client;

		/// <summary>
		/// If the query has been completed.
		/// </summary>
		public bool IsDone => this.isDone;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.isDone)
			{
				this.isDone = true;

				try
				{
					this.client.AbortQuery(this.to, this.nodeID, this.sourceID, this.partition, this.command, this.queryId,
						this.language, this.serviceToken, this.deviceToken, this.userToken, null, null);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		private void Invoke(NodeQueryEventHandler h, MessageEventArgs e)
		{
			try
			{
				h?.Invoke(this, new NodeQueryEventArgs(this, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal void SetTitle(string Title, MessageEventArgs e)
		{
			this.title = Title;
			this.Invoke(this.NewTitle, e);
		}

		/// <summary>
		/// Title os response report.
		/// </summary>
		public string Title => this.title;

		/// <summary>
		/// Raised when the result report gets a new title.
		/// </summary>
		public event NodeQueryEventHandler NewTitle = null;

		internal void ReportStarted(MessageEventArgs e)
		{
			this.Invoke(this.Started, e);
		}

		/// <summary>
		/// Raised when the result report has started.
		/// </summary>
		public event NodeQueryEventHandler Started = null;

		internal void ReportDone(MessageEventArgs e)
		{
			this.isDone = true;
			this.Invoke(this.Done, e);
		}

		/// <summary>
		/// Raised when the result report is completed.
		/// </summary>
		public event NodeQueryEventHandler Done = null;

		internal void ReportAborted(MessageEventArgs e)
		{
			this.Invoke(this.Aborted, e);
		}

		/// <summary>
		/// Raised when the result report is aborted.
		/// </summary>
		public event NodeQueryEventHandler Aborted = null;

		internal void NewTable(Table Table, MessageEventArgs e)
		{
			QueryTable Table2;

			lock (this.result)
			{
				if (this.currentSection != null)
				{
					Table2 = new QueryTable(this.currentSection, Table);

					this.currentSection.Add(Table2);
				}
				else
				{
					Table2 = new QueryTable(null, Table);

					this.result.Add(Table2);
					this.resultFixed = null;
				}
			}

			lock (this.tables)
			{
				this.tables[Table.TableId] = Table2;
			}

			this.Invoke(this.TableAdded, Table2, e);
		}

		/// <summary>
		/// Event raised when a new table has been added to the report.
		/// </summary>
		public event NodeQueryTableEventHandler TableAdded = null;

		private void Invoke(NodeQueryTableEventHandler Callback, QueryTable Table, MessageEventArgs e)
		{
			try
			{
				Callback?.Invoke(this, new NodeQueryTableEventArgs(Table, this, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal void NewRecords(string TableId, Record[] Records, MessageEventArgs e)
		{
			QueryTable Table;

			lock (this.tables)
			{
				if (!this.tables.TryGetValue(TableId, out Table))
					return;
			}

			Table.Add(Records);

			this.Invoke(this.TableUpdated, Table, Records, e);
		}

		private void Invoke(NodeQueryTableUpdatedEventHandler h, QueryTable Table, Record[] NewRecords, MessageEventArgs e)
		{
			try
			{
				h?.Invoke(this, new NodeQueryTableUpdatedEventArgs(Table, this, NewRecords, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when records has been added to a table.
		/// </summary>
		public event NodeQueryTableUpdatedEventHandler TableUpdated = null;

		internal void TableDone(string TableId, MessageEventArgs e)
		{
			QueryTable Table;

			lock (this.tables)
			{
				if (!this.tables.TryGetValue(TableId, out Table))
					return;
			}

			Table.Done();

			this.Invoke(this.TableCompleted, Table, e);
		}

		/// <summary>
		/// Event raised when a table has been completed.
		/// </summary>
		public event NodeQueryTableEventHandler TableCompleted = null;

		internal void NewObject(object Object, MessageEventArgs e)
		{
			QueryObject Obj;

			lock (this.result)
			{
				if (this.currentSection != null)
				{
					Obj = new QueryObject(this.currentSection, Object);

					this.currentSection.Add(Obj);
				}
				else
				{
					Obj = new QueryObject(null, Object);

					this.result.Add(Obj);
					this.resultFixed = null;
				}
			}

			this.Invoke(this.ObjectAdded, Obj, e);
		}

		/// <summary>
		/// Raised when an object has been added to the report.
		/// </summary>
		public event NodeQueryObjectEventHandler ObjectAdded = null;

		private void Invoke(NodeQueryObjectEventHandler Callback, QueryObject Object, MessageEventArgs e)
		{
			try
			{
				Callback?.Invoke(this, new NodeQueryObjectEventArgs(Object, this, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal void BeginSection(string Header, MessageEventArgs e)
		{
			QuerySection Section;

			lock (this.result)
			{
				if (this.currentSection != null)
				{
					Section = new QuerySection(this.currentSection, Header);

					this.currentSection.Add(Section);
				}
				else
				{
					Section = new QuerySection(null, Header);

					this.result.Add(Section);
					this.resultFixed = null;
				}

				this.currentSection = Section;
			}

			this.Invoke(this.SectionAdded, Section, e);
		}

		private void Invoke(NodeQuerySectionEventHandler Callback, QuerySection Section, MessageEventArgs e)
		{
			try
			{
				Callback?.Invoke(this, new NodeQuerySectionEventArgs(Section, this, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a new section has been added to the result set.
		/// </summary>
		public event NodeQuerySectionEventHandler SectionAdded = null;

		internal void EndSection(MessageEventArgs e)
		{
			QuerySection Section;

			lock (this.result)
			{
				if (this.currentSection == null)
					return;

				Section = this.currentSection;
				this.currentSection = this.currentSection.Parent as QuerySection;
			}

			this.Invoke(this.SectionCompleted, Section, e);
		}

		/// <summary>
		/// Event raised when a section has been compelted in the result set.
		/// </summary>
		public event NodeQuerySectionEventHandler SectionCompleted = null;

		internal void StatusMessage(string Message, MessageEventArgs e)
		{
			try
			{
				this.StatusMessageReceived?.Invoke(this, new NodeQueryStatusMessageEventArgs(Message, this, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Raised when a new status message has been received.
		/// </summary>
		public event NodeQueryStatusMessageEventHandler StatusMessageReceived = null;

		internal void QueryMessage(QueryEventType Type, QueryEventLevel Level, string Message, MessageEventArgs e)
		{
			try
			{
				this.EventMessageReceived?.Invoke(this, new NodeQueryEventMessageEventArgs(Type, Level, Message, this, e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Raised when a new event message has been received.
		/// </summary>
		public event NodeQueryEventMessageEventHandler EventMessageReceived = null;

		/// <summary>
		/// Result set
		/// </summary>
		public QueryItem[] Result
		{
			get
			{
				QueryItem[] Result = this.resultFixed;
				if (Result != null)
					return Result;

				lock (this.result)
				{
					Result = this.resultFixed = this.result.ToArray();
				}

				return Result;
			}
		}

		internal void Queue(int SequenceNr, MessageEventArgs e)
		{
			if (this.queuedMessages == null)
			{
				this.queuedMessages = new LinkedList<KeyValuePair<int, MessageEventArgs>>();
				this.queuedMessages.AddLast(new KeyValuePair<int, MessageEventArgs>(SequenceNr, e));
			}
			else
			{
				LinkedListNode<KeyValuePair<int, MessageEventArgs>> Loop = this.queuedMessages.First;

				while (Loop != null)
				{
					if (SequenceNr < Loop.Value.Key)
					{
						this.queuedMessages.AddBefore(Loop, new KeyValuePair<int, MessageEventArgs>(SequenceNr, e));
						Loop = null;
					}
					else if (Loop.Next == null)
					{
						this.queuedMessages.AddAfter(Loop, new KeyValuePair<int, MessageEventArgs>(SequenceNr, e));
						Loop = null;
					}
					else
						Loop = Loop.Next;
				}
			}
		}

		internal bool HasQueued
		{
			get { return this.queuedMessages != null; }
		}

		internal int SequenceNr
		{
			get { return this.seqNr; }
		}

		internal void NextSequenceNr()
		{
			this.seqNr++;
		}

		internal MessageEventArgs PopQueued(int ExpectedSequenceNr)
		{
			KeyValuePair<int, MessageEventArgs> P;

			if (this.queuedMessages != null && this.queuedMessages.First != null && (P = this.queuedMessages.First.Value).Key == ExpectedSequenceNr)
			{
				this.queuedMessages.RemoveFirst();

				if (this.queuedMessages.First == null)
					this.queuedMessages = null;

				return P.Value;
			}
			else
				return null;
		}

	}
}
