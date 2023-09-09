using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking;
using Waher.Networking.Sniffers;

namespace Waher.Events.Socket
{
	/// <summary>
	/// Receives events from a socket.
	/// </summary>
	public class SocketEventRecipient : IDisposable
	{
		/// <summary>
		/// http://waher.se/Schema/EventOutput.xsd
		/// </summary>
		public const string LogNamespace = "http://waher.se/Schema/EventOutput.xsd";

		private readonly Dictionary<Guid, StringBuilder> fragments = new Dictionary<Guid, StringBuilder>();
		private readonly bool logIncoming;
		private BinaryTcpServer server;
		private bool disposed = false;
		private int inputState = 0;
		private int inputDepth = 0;

		/// <summary>
		/// Receives events from a socket.
		/// </summary>
		private SocketEventRecipient(int Port, X509Certificate Certificate, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(Port, TimeSpan.FromSeconds(10), Certificate, Sniffers);
			this.logIncoming = LogIncomingEvents;
		}

		/// <summary>
		/// Creates and opens a socket-based event recipient.
		/// </summary>
		/// <param name="Port">Port to open.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/>.</param>
		/// <param name="Sniffers">Any sniffers to output communucation to.</param>
		/// <returns>Event recipient.</returns>
		public static Task<SocketEventRecipient> Create(int Port, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			return Create(Port, null, LogIncomingEvents, Sniffers);
		}

		/// <summary>
		/// Creates and opens a socket-based event recipient.
		/// </summary>
		/// <param name="Port">Port to open.</param>
		/// <param name="Certificate">Certificate to use for TLS encryption.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/>.</param>
		/// <param name="Sniffers">Any sniffers to output communucation to.</param>
		/// <returns>Event recipient.</returns>
		public static async Task<SocketEventRecipient> Create(int Port, X509Certificate Certificate, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			SocketEventRecipient Result = null;

			try
			{
				Result = new SocketEventRecipient(Port, Certificate, LogIncomingEvents, Sniffers);

				Result.server.OnAccept += Result.Server_OnAccept;
				Result.server.OnClientConnected += Result.Server_OnClientConnected;
				Result.server.OnClientDisconnected += Result.Server_OnClientDisconnected;
				Result.server.OnDataReceived += Result.Server_OnDataReceived;

				await Result.server.Open();
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return Result;
		}

		private async Task Server_OnAccept(object Sender, ServerConnectionAcceptEventArgs e)
		{
			EventHandlerAsync<ServerConnectionAcceptEventArgs> h = this.OnAccept;

			if (!(h is null))
			{
				try
				{
					await h(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					e.Accept = false;
				}
			}
		}

		public event EventHandlerAsync<ServerConnectionAcceptEventArgs> OnAccept;

		private Task Server_OnClientConnected(object Sender, ServerConnectionEventArgs e)
		{
			lock (this.fragments)
			{
				this.fragments.Remove(e.Id);
			}

			return Task.CompletedTask;
		}

		private Task Server_OnClientDisconnected(object Sender, ServerConnectionEventArgs e)
		{
			lock (this.fragments)
			{
				this.fragments.Remove(e.Id);
			}

			return Task.CompletedTask;
		}

		private async Task Server_OnDataReceived(object Sender, ServerConnectionDataEventArgs e)
		{
			StringBuilder Fragment;

			lock (this.fragments)
			{
				if (!this.fragments.TryGetValue(e.Id, out Fragment))
				{
					Fragment = new StringBuilder();
					this.fragments[e.Id] = Fragment;
				}
			}

			string s = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.Count);
			bool Continue;

			try
			{
				Continue = await this.ParseIncoming(Fragment, s);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				Continue = false;
			}

			if (!Continue)
				e.CloseConnection();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.server?.Dispose();
				this.server = null;
			}
		}

		private async Task<bool> ParseIncoming(StringBuilder Fragment, string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0: // Waiting for <
						if (ch == '<')
						{
							Fragment.Append(ch);
							this.inputState++;
						}
						else if (this.inputDepth > 0)
							Fragment.Append(ch);
						else if (ch > ' ')
							return false;
						break;

					case 1: // Second character in tag
						Fragment.Append(ch);
						if (ch == '/')
							this.inputState++;
						else
							this.inputState += 2;
						break;

					case 2: // Waiting for end of closing tag
						Fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 0)
								return false;
							else
							{
								if (this.inputDepth == 0)
								{
									if (!await this.ProcessFragment(Fragment.ToString()))
										Result = false;

									Fragment.Clear();
								}

								if (this.inputState > 0)
									this.inputState = 0;
							}
						}
						break;

					case 3: // Wait for end of start tag
						Fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 0;
						}
						else if (ch == '/')
							this.inputState++;
						break;

					case 4: // Check for end of childless tag.
						Fragment.Append(ch);
						if (ch == '>')
						{
							if (this.inputDepth == 0)
							{
								if (!await this.ProcessFragment(Fragment.ToString()))
									Result = false;

								Fragment.Clear();
							}

							if (this.inputState != 0)
								this.inputState = 0;
						}
						else
							this.inputState--;
						break;

					default:
						break;
				}
			}

			return Result;
		}

		private async Task<bool> ProcessFragment(string Xml)
		{
			try
			{
				XmlDocument Doc = new XmlDocument();

				Doc.LoadXml(Xml);

				if (TryParseEventXml(Doc.DocumentElement, out Event Event))
				{
					if (this.logIncoming)
						Log.Event(Event);

					EventEventHandler h = this.EventReceived;

					if (!(h is null))
						await h(this, new EventEventArgs(Event));
				}
				else
				{
					CustomFragmentEventHandler h = this.CustomFragmentReceived;

					if (!(h is null))
						await h(this, new CustomFragmentEventArgs(Doc));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return true;
		}

		/// <summary>
		/// Event raised when an event has been received.
		/// </summary>
		public event EventEventHandler EventReceived;

		/// <summary>
		/// Event raised when a custom XML fragment has been received.
		/// </summary>
		public event CustomFragmentEventHandler CustomFragmentReceived;

		/// <summary>
		/// Parses an event encoded into an XML fragment
		/// </summary>
		/// <param name="Xml">XML Element</param>
		/// <param name="Parsed">Parsed event, if detected, null if not an event.</param>
		/// <returns>If able to parse an event.</returns>
		public static bool TryParseEventXml(XmlElement Xml, out Event Parsed)
		{
			if (Xml is null || Xml.NamespaceURI != LogNamespace)
			{
				Parsed = null;
				return false;
			}

			DateTime Timestamp = XML.Attribute(Xml, "timestamp", DateTime.Now);
			EventLevel Level = XML.Attribute(Xml, "level", EventLevel.Minor);
			string EventId = XML.Attribute(Xml, "id");
			string Object = XML.Attribute(Xml, "object");
			string Actor = XML.Attribute(Xml, "actor");
			string Module = XML.Attribute(Xml, "module");
			string Facility = XML.Attribute(Xml, "facility");
			string StackTrace = null;
			string Message = null;
			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>();

			foreach (XmlNode N2 in Xml.ChildNodes)
			{
				if (!(N2 is XmlElement E2))
					continue;

				switch (E2.LocalName)
				{
					case "Message":
						Message = ReadRows(E2);
						break;

					case "StackTrace":
						StackTrace = ReadRows(E2);
						break;

					case "Tag":
						string Key = XML.Attribute(E2, "key");
						string Value = XML.Attribute(E2, "value");

						if (bool.TryParse(Value, out bool b))
							Tags.Add(new KeyValuePair<string, object>(Key, b));
						else if (int.TryParse(Value, out int i))
							Tags.Add(new KeyValuePair<string, object>(Key, i));
						else if (long.TryParse(Value, out long l))
							Tags.Add(new KeyValuePair<string, object>(Key, l));
						else if (double.TryParse(Value, out double d))
							Tags.Add(new KeyValuePair<string, object>(Key, d));
						else
							Tags.Add(new KeyValuePair<string, object>(Key, Value));
						break;
				}
			}

			if (!Enum.TryParse(Xml.LocalName, out EventType EventType))
			{
				Parsed = null;
				return false;
			}

			Parsed = new Event(Timestamp, EventType, Message ?? string.Empty, Object, Actor,
				EventId, Level, Facility, Module, StackTrace, Tags.ToArray());

			return true;
		}

		private static string ReadRows(XmlElement E)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				if (E2.LocalName == "Row")
				{
					if (First)
						First = false;
					else
						sb.AppendLine();

					sb.Append(E2.InnerText);
				}
			}

			return sb.ToString();
		}

	}
}
