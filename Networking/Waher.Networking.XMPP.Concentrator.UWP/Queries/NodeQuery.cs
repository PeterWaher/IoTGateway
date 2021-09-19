using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Client-side Node Query object. It collects the results of the query.
	/// </summary>
	public class NodeQuery : IDisposable
	{
		private readonly object synchObj = new object();
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
		private bool paused = true;
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
		/// If query reception is paused. It can be resumed, by calling <see cref="Resume"/>. The object is paused by default,
		/// and resumed when event handlers have been properly assigned.
		/// </summary>
		public bool Paused
		{
			get
			{
				lock (this.synchObj)
				{
					return this.paused;
				}
			}
		}

		/// <summary>
		/// Resumes a paused query reception.
		/// </summary>
		public void Resume()
		{
			lock (this.synchObj)
			{
				if (this.paused)
				{
					MessageEventArgs e;

					while (!((e = this.queuedMessages?.First?.Value.Value) is null))
					{
						if (this.Process(e, false))
							this.queuedMessages.RemoveFirst();
						else
							break;
					}

					this.paused = false;
				}
			}
		}

		internal bool Process(MessageEventArgs e, bool CanQueue)
		{
			int SequenceNr = XML.Attribute(e.Content, "seqNr", 0);

			int ExpectedSeqNr = this.SequenceNr + 1;
			if (SequenceNr < ExpectedSeqNr)
				return true;

			if (SequenceNr == ExpectedSeqNr && !this.paused)
			{
				this.NextSequenceNr();
				this.ProcessQueryProgress(e);

				if (this.HasQueued)
				{
					ExpectedSeqNr++;

					e = this.PopQueued(ExpectedSeqNr);
					while (!(e is null))
					{
						this.ProcessQueryProgress(e);
						ExpectedSeqNr++;
						e = this.PopQueued(ExpectedSeqNr);
					}
				}

				return true;
			}
			else
			{
				if (CanQueue)
					this.Queue(SequenceNr, e);

				return false;
			}
		}

		private void ProcessQueryProgress(MessageEventArgs e)
		{
			string s, s2;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					try
					{
						switch (E.LocalName)
						{
							case "title":
								s = XML.Attribute(E, "name");
								this.SetTitle(s, e);
								break;

							case "tableDone":
								s = XML.Attribute(E, "tableId");
								this.TableDone(s, e);
								break;

							case "status":
								s = XML.Attribute(E, "message");
								this.StatusMessage(s, e);
								break;

							case "queryStarted":
								this.ReportStarted(e);
								break;

							case "newTable":
								s = XML.Attribute(E, "tableId");
								s2 = XML.Attribute(E, "tableName");

								List<Column> Columns = new List<Column>();

								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "column")
									{
										string ColumnId = XML.Attribute(E2, "columnId");
										string Header = XML.Attribute(E2, "header");
										string SourceID = XML.Attribute(E2, "src");
										string Partition2 = XML.Attribute(E2, "pt");
										SKColor? FgColor = null;
										SKColor? BgColor = null;
										ColumnAlignment? ColumnAlignment = null;
										byte? NrDecimals = null;

										if (E2.HasAttribute("fgColor") && ConcentratorClient.TryParse(E2.GetAttribute("fgColor"), out SKColor Color))
											FgColor = Color;

										if (E2.HasAttribute("bgColor") && ConcentratorClient.TryParse(E2.GetAttribute("bgColor"), out Color))
											BgColor = Color;

										if (E2.HasAttribute("alignment") && Enum.TryParse<ColumnAlignment>(E2.GetAttribute("alignment"), out ColumnAlignment ColumnAlignment2))
											ColumnAlignment = ColumnAlignment2;

										if (E2.HasAttribute("nrDecimals") && byte.TryParse(E2.GetAttribute("nrDecimals"), out byte b))
											NrDecimals = b;

										Columns.Add(new Column(ColumnId, Header, SourceID, Partition2, FgColor, BgColor, ColumnAlignment, NrDecimals));
									}
								}

								this.NewTable(new Table(s, s2, Columns.ToArray()), e);
								break;

							case "newRecords":
								s = XML.Attribute(E, "tableId");

								List<Record> Records = new List<Record>();
								List<object> Record = null;

								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "record")
									{
										if (Record is null)
											Record = new List<object>();
										else
											Record.Clear();

										foreach (XmlNode N3 in E2.ChildNodes)
										{
											if (N3 is XmlElement E3)
											{
												switch (E3.LocalName)
												{
													case "void":
														Record.Add(null);
														break;

													case "boolean":
														if (CommonTypes.TryParse(E3.InnerText, out bool b))
															Record.Add(b);
														else
															Record.Add(null);
														break;

													case "color":
														if (ConcentratorClient.TryParse(E3.InnerText, out SKColor Color))
															Record.Add(Color);
														else
															Record.Add(null);
														break;

													case "date":
													case "dateTime":
														if (XML.TryParse(E3.InnerText, out DateTime TP))
															Record.Add(TP);
														else
															Record.Add(null);
														break;

													case "double":
														if (CommonTypes.TryParse(E3.InnerText, out double d))
															Record.Add(d);
														else
															Record.Add(null);
														break;

													case "duration":
														if (Duration.TryParse(E3.InnerText, out Duration d2))
															Record.Add(d2);
														else
															Record.Add(null);
														break;

													case "int":
														if (int.TryParse(E3.InnerText, out int i))
															Record.Add(i);
														else
															Record.Add(null);
														break;

													case "long":
														if (long.TryParse(E3.InnerText, out long l))
															Record.Add(l);
														else
															Record.Add(null);
														break;

													case "string":
														Record.Add(E3.InnerText);
														break;

													case "time":
														if (TimeSpan.TryParse(E3.InnerText, out TimeSpan TS))
															Record.Add(TS);
														else
															Record.Add(null);
														break;

													case "base64":
														try
														{
															string ContentType = XML.Attribute(E3, "contentType");
															byte[] Bin = Convert.FromBase64String(E3.InnerText);
															object Decoded = InternetContent.Decode(ContentType, Bin, null);

															Record.Add(Decoded);
														}
														catch (Exception ex)
														{
															this.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message, e);
															Record.Add(null);
														}
														break;

													default:
														Record.Add(null);
														break;
												}
											}
										}

										Records.Add(new Record(Record.ToArray()));
									}
								}

								this.NewRecords(s, Records.ToArray(), e);
								break;

							case "newObject":
								try
								{
									string ContentType = XML.Attribute(E, "contentType");
									byte[] Bin = Convert.FromBase64String(E.InnerText);
									object Decoded = InternetContent.Decode(ContentType, Bin, null);

									this.NewObject(Decoded, e);
								}
								catch (Exception ex)
								{
									this.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message, e);
								}
								break;

							case "queryMessage":
								QueryEventType Type = (QueryEventType)XML.Attribute(E, "type", QueryEventType.Information);
								QueryEventLevel Level = (QueryEventLevel)XML.Attribute(E, "level", QueryEventLevel.Minor);

								this.QueryMessage(Type, Level, E.InnerText, e);
								break;

							case "endSection":
								this.EndSection(e);
								break;

							case "queryDone":
								this.ReportDone(e);
								break;

							case "beginSection":
								s = XML.Attribute(E, "header");
								this.BeginSection(s, e);
								break;

							case "queryAborted":
								this.ReportAborted(e);
								break;

							default:
								this.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, "Unrecognized sniffer event received: " + E.OuterXml, e);
								break;
						}
					}
					catch (Exception ex)
					{
						this.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message, e);
					}
				}
			}
		}

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
				if (!(this.currentSection is null))
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
				if (!(this.currentSection is null))
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
				if (!(this.currentSection is null))
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
				if (this.currentSection is null)
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
				if (!(Result is null))
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
			lock (this.synchObj)
			{
				if (this.queuedMessages is null)
				{
					this.queuedMessages = new LinkedList<KeyValuePair<int, MessageEventArgs>>();
					this.queuedMessages.AddLast(new KeyValuePair<int, MessageEventArgs>(SequenceNr, e));
				}
				else
				{
					LinkedListNode<KeyValuePair<int, MessageEventArgs>> Loop = this.queuedMessages.First;

					while (!(Loop is null))
					{
						if (SequenceNr < Loop.Value.Key)
						{
							this.queuedMessages.AddBefore(Loop, new KeyValuePair<int, MessageEventArgs>(SequenceNr, e));
							Loop = null;
						}
						else if (Loop.Next is null)
						{
							this.queuedMessages.AddAfter(Loop, new KeyValuePair<int, MessageEventArgs>(SequenceNr, e));
							Loop = null;
						}
						else
							Loop = Loop.Next;
					}
				}
			}
		}

		internal bool HasQueued
		{
			get 
			{
				lock (this.synchObj)
				{
					return this.queuedMessages != null;
				}
			}
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
			lock (this.synchObj)
			{
				KeyValuePair<int, MessageEventArgs> P;

				if (this.queuedMessages != null && this.queuedMessages.First != null && (P = this.queuedMessages.First.Value).Key == ExpectedSequenceNr)
				{
					this.queuedMessages.RemoveFirst();

					if (this.queuedMessages.First is null)
						this.queuedMessages = null;

					return P.Value;
				}
				else
					return null;
			}
		}

	}
}
