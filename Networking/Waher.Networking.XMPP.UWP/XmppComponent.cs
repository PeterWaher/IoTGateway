﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Networking.XMPP.StreamErrors;
using Waher.Runtime.Cache;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Events;
using Waher.Security;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Delegate for event raised to get roster items for the component.
	/// </summary>
	/// <param name="BareJid">Bare JID</param>
	/// <returns>Corresponding roster item, if found, or null, if not found.</returns>
	public delegate RosterItem GetRosterItemEventHandler(string BareJid);

	/// <summary>
	/// Manages an XMPP component connection, as defined in XEP-0114:
	/// http://xmpp.org/extensions/xep-0114.html
	/// </summary>
	public class XmppComponent : CommunicationLayer, IDisposableAsync, IHostReference
	{
		private const int KeepAliveTimeSeconds = 30;
		private const int MaxFragmentSize = 40000000;

		private readonly LinkedList<KeyValuePair<string, EventHandler>> outputQueue = new LinkedList<KeyValuePair<string, EventHandler>>();
		private readonly Dictionary<uint, PendingRequest> pendingRequestsBySeqNr = new Dictionary<uint, PendingRequest>();
		private readonly SortedDictionary<DateTime, PendingRequest> pendingRequestsByTimeout = new SortedDictionary<DateTime, PendingRequest>();
		private readonly Dictionary<string, EventHandlerAsync<IqEventArgs>> iqGetHandlers = new Dictionary<string, EventHandlerAsync<IqEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<IqEventArgs>> iqSetHandlers = new Dictionary<string, EventHandlerAsync<IqEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<MessageEventArgs>> messageHandlers = new Dictionary<string, EventHandlerAsync<MessageEventArgs>>();
		private readonly Dictionary<string, MessageEventArgs> receivedMessages = new Dictionary<string, MessageEventArgs>();
		private readonly Dictionary<string, bool> clientFeatures = new Dictionary<string, bool>();
		private readonly Dictionary<string, int> pendingAssuredMessagesPerSource = new Dictionary<string, int>();
		private readonly IqResponses responses = new IqResponses(TimeSpan.FromMinutes(1));
		private Cache<string, uint> pendingPresenceRequests;
		private TextTcpClient client = null;
		private Timer secondTimer = null;
		private DateTime nextPing = DateTime.MinValue;
		private readonly UTF8Encoding encoding = new UTF8Encoding(false, false);
		private readonly StringBuilder fragment = new StringBuilder();
		private int fragmentLength = 0;
		private XmppState state;
		private readonly Random gen = new Random();
		private readonly object synchObject = new object();
		private readonly string identityCategory;
		private readonly string identityType;
		private readonly string identityName;
		private string host;
		private readonly string componentSubDomain;
		private readonly string sharedSecret;
		private string streamId;
		private string streamHeader;
		private string streamFooter;
		private uint seqnr = 0;
		private readonly int port;
		private int keepAliveSeconds = KeepAliveTimeSeconds;
		private int inputState = 0;
		private int inputDepth = 0;
		private int defaultRetryTimeout = 15000;
		private int defaultNrRetries = 5;
		private int defaultMaxRetryTimeout = int.MaxValue;
		private int maxAssuredMessagesPendingFromSource = 5;
		private int maxAssuredMessagesPendingTotal = 100;
		private int nrAssuredMessagesPending = 0;
		private bool defaultDropOff = true;
		private bool supportsPing = true;
		private bool pingResponse = true;
		private bool openBracketReceived = false;

		/// <summary>
		/// Manages an XMPP component connection, as defined in XEP-0114:
		/// http://xmpp.org/extensions/xep-0114.html
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="ComponentSubDomain">Component sub-domain.</param>
		/// <param name="SharedSecret">Shared secret for the component.</param>
		/// <param name="IdentityCategory">Identity category, as defined in XEP-0030.</param>
		/// <param name="IdentityType">Identity type, as defined in XEP-0030.</param>
		/// <param name="IdentityName">Identity name, as defined in XEP-0030.</param>
		/// <param name="Sniffers">Sniffers</param>
		public XmppComponent(string Host, int Port, string ComponentSubDomain, string SharedSecret,
			string IdentityCategory, string IdentityType, string IdentityName, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.identityCategory = IdentityCategory;
			this.identityType = IdentityType;
			this.identityName = IdentityName;
			this.host = Host;
			this.port = Port;
			this.componentSubDomain = ComponentSubDomain;
			this.sharedSecret = SharedSecret;
			this.state = XmppState.Offline;

			this.pendingPresenceRequests = new Cache<string, uint>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), true);
			this.pendingPresenceRequests.Removed += this.PendingPresenceRequest_Removed;

			this.RegisterDefaultHandlers();
			this.Connect();
		}

		private async void Connect()
		{
			this.State = XmppState.Connecting;
			this.pingResponse = true;
			this.openBracketReceived = false;

			try
			{
				this.client = new TextTcpClient(this.encoding, true);
				this.client.OnReceived += this.OnReceived;
				this.client.OnSent += this.OnSent;
				this.client.OnError += this.Error;
				this.client.OnDisconnected += this.Client_OnDisconnected;

				if (await this.client.ConnectAsync(this.host, this.port))
				{
					this.State = XmppState.StreamNegotiation;

					await this.BeginWrite("<?xml version='1.0' encoding='utf-8'?><stream:stream to='" + XML.Encode(this.componentSubDomain) +
						"' xmlns='jabber:component:accept' xmlns:stream='" + XmppClient.NamespaceStream + "'>", null, null);

					this.ResetState();
				}
				else
				{
					await this.ConnectionError(new System.Exception("Unable to connect to " + this.host + ":" + this.port.ToString()));
					return;
				}
			}
			catch (Exception ex)
			{
				await this.ConnectionError(ex);
			}
		}

		private void RegisterDefaultHandlers()
		{
			this.RegisterIqGetHandler("query", XmppClient.NamespaceServiceDiscoveryInfo, this.ServiceDiscoveryRequestHandler, true);
			this.RegisterIqGetHandler("ping", XmppClient.NamespacePing, this.PingRequestHandler, true);

			#region Neuro-Foundation V1

			this.RegisterIqSetHandler("acknowledged", XmppClient.NamespaceQualityOfServiceNeuroFoundationV1, this.AcknowledgedQoSMessageHandler, true);
			this.RegisterIqSetHandler("assured", XmppClient.NamespaceQualityOfServiceNeuroFoundationV1, this.AssuredQoSMessageHandler, false);
			this.RegisterIqSetHandler("deliver", XmppClient.NamespaceQualityOfServiceNeuroFoundationV1, this.DeliverQoSMessageHandler, false);

			#endregion

			#region IEEE V1

			this.RegisterIqSetHandler("acknowledged", XmppClient.NamespaceQualityOfServiceIeeeV1, this.AcknowledgedQoSMessageHandler, true);
			this.RegisterIqSetHandler("assured", XmppClient.NamespaceQualityOfServiceIeeeV1, this.AssuredQoSMessageHandler, false);
			this.RegisterIqSetHandler("deliver", XmppClient.NamespaceQualityOfServiceIeeeV1, this.DeliverQoSMessageHandler, false);

			#endregion
		}

		private void ResetState()
		{
			this.inputState = 0;
			this.inputDepth = 0;

			this.pendingRequestsBySeqNr.Clear();
			this.pendingRequestsByTimeout.Clear();

			this.responses.Clear();
		}

		private async Task ConnectionError(Exception ex)
		{
			await this.OnConnectionError.Raise(this, ex, false);
			await this.Error(this, ex);

			this.inputState = -1;
			await this.DisposeClient();
			this.State = XmppState.Error;
		}

		private Task Client_OnDisconnected(object Sender, EventArgs e)
		{
			if (this.HasSniffers)
			{
				try
				{
					this.Information("Disconnected.");
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			if (this.state != XmppState.Error)
				this.State = XmppState.Offline;

			return Task.CompletedTask;
		}

		private async Task Error(object _, Exception ex)
		{
			this.State = XmppState.Error;

			this.Exception(ex);
			await this.OnError.Raise(this, ex);
		}

		private Task<bool> OnSent(object _, string Text)
		{
			this.TransmitText(Text);
			return Task.FromResult(true);
		}

		private async Task<bool> OnReceived(object _, string Text)
		{
			if (this.openBracketReceived)
			{
				this.openBracketReceived = false;
				this.ReceiveText("<" + Text);
			}
			else if (Text == "<")
				this.openBracketReceived = true;
			else
				this.ReceiveText(Text);

			return await this.ParseIncoming(Text);
		}

		/// <summary>
		/// Event raised when a connection to a broker could not be made.
		/// </summary>
		public event EventHandlerAsync<Exception> OnConnectionError = null;

		/// <summary>
		/// Event raised when an error was encountered.
		/// </summary>
		public event EventHandlerAsync<Exception> OnError = null;

		/// <summary>
		/// Host or IP address of XMPP server.
		/// </summary>
		public string Host => this.host;

		/// <summary>
		/// Port number to connect to.
		/// </summary>
		public int Port => this.port;

		/// <summary>
		/// Current state of connection.
		/// </summary>
		public XmppState State
		{
			get => this.state;
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					Task.Run(async () =>
					{
						try
						{
							this.Information("State changed to " + value.ToString());
							await this.RaiseOnStateChanged(value);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});
				}
			}
		}

		private Task RaiseOnStateChanged(XmppState State)
		{
			return this.OnStateChanged.Raise(this, State);
		}

		/// <summary>
		/// Event raised whenever the internal state of the connection changes.
		/// </summary>
		public event EventHandlerAsync<XmppState> OnStateChanged = null;

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		[Obsolete("Use the DisposeAsync() method.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (this.state == XmppState.Connected || this.state == XmppState.FetchingRoster || this.state == XmppState.SettingPresence)
				await this.BeginWrite(this.streamFooter, this.CleanUp, null);
			else
				await this.CleanUp(this, EventArgs.Empty);
		}

		/// <summary>
		/// Closes the connection the hard way. This might disrupt stream processing, but can simulate a lost connection. To close the connection
		/// softly, call the <see cref="Dispose"/> method.
		/// 
		/// Note: After turning the connection hard-offline, you can reconnect to the server calling the <see cref="Reconnect"/> method.
		/// </summary>
		public Task HardOffline()
		{
			return this.CleanUp(this, EventArgs.Empty);
		}

		private async Task CleanUp(object Sender, EventArgs e)
		{
			this.State = XmppState.Offline;

			this.pendingPresenceRequests?.Dispose();
			this.pendingPresenceRequests = null;

			if (!(this.outputQueue is null))
			{
				lock (this.synchObject)
				{
					this.outputQueue.Clear();
				}
			}

			if (!(this.pendingRequestsBySeqNr is null))
			{
				lock (this.synchObject)
				{
					this.pendingRequestsBySeqNr.Clear();
					this.pendingRequestsByTimeout.Clear();
				}
			}

			this.secondTimer?.Dispose();
			this.secondTimer = null;

			await this.DisposeClient();

			this.responses.Dispose();
		}

		private async Task DisposeClient()
		{
			if (!(this.client is null))
			{
				await this.client.DisposeAsync();
				this.client = null;
			}
		}

		/// <summary>
		/// Reconnects a client after an error or if it's offline. Reconnecting, instead of creating a completely new connection,
		/// saves time. It binds to the same resource provided earlier, and avoids fetching the roster.
		/// </summary>
		public async Task Reconnect()
		{
			await this.DisposeClient();
			this.Connect();
		}

		private async Task BeginWrite(string Xml, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (string.IsNullOrEmpty(Xml))
			{
				if (!(Callback is null))
					await Callback.Raise(this, new DeliveryEventArgs(State, true));
			}
			else
			{
				await this.client.SendAsync(Xml, Callback, State);
				this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);
			}
		}

		private async Task<bool> ParseIncoming(string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0:     // Waiting for first <
						if (ch == '<')
						{
							this.fragment.Append(ch);
							if (++this.fragmentLength > MaxFragmentSize)
							{
								await this.ToError();
								return false;
							}
							else
								this.inputState++;
						}
						else if (ch > ' ')
						{
							await this.ToError();
							return false;
						}
						break;

					case 1:     // Waiting for ? or >
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '?')
							this.inputState++;
						else if (ch == '>')
						{
							this.inputState = 5;
							this.inputDepth = 1;
							await this.ProcessStream(this.fragment.ToString());
							this.fragment.Clear();
							this.fragmentLength = 0;
						}
						break;

					case 2:     // In processing instruction. Waiting for ?>
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState++;
						break;

					case 3:     // Waiting for <stream
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '<')
							this.inputState++;
						else if (ch > ' ')
						{
							await this.ToError();
							return false;
						}
						break;

					case 4:     // Waiting for >
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputState++;
							this.inputDepth = 1;
							await this.ProcessStream(this.fragment.ToString());
							this.fragment.Clear();
							this.fragmentLength = 0;
						}
						break;

					case 5: // Waiting for start element.
						if (ch == '<')
						{
							this.fragment.Append(ch);
							if (++this.fragmentLength > MaxFragmentSize)
							{
								await this.ToError();
								return false;
							}
							else
								this.inputState++;
						}
						else if (this.inputDepth > 1)
						{
							this.fragment.Append(ch);
							if (++this.fragmentLength > MaxFragmentSize)
							{
								await this.ToError();
								return false;
							}
						}
						else if (ch > ' ')
						{
							await this.ToError();
							return false;
						}
						break;

					case 6: // Second character in tag
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '/')
							this.inputState++;
						else if (ch == '!')
							this.inputState = 13;
						else
							this.inputState += 2;
						break;

					case 7: // Waiting for end of closing tag
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 1)
							{
								await this.ToError();
								return false;
							}
							else
							{
								if (this.inputDepth == 1)
								{
									if (!await this.ProcessFragment(this.fragment.ToString()))
										Result = false;

									this.fragment.Clear();
									this.fragmentLength = 0;
								}

								if (this.inputState > 0)
									this.inputState = 5;
							}
						}
						break;

					case 8: // Wait for end of start tag
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState++;
						else if (ch <= ' ')
							this.inputState += 2;
						break;

					case 9: // Check for end of childless tag.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							if (this.inputDepth == 1)
							{
								if (!await this.ProcessFragment(this.fragment.ToString()))
									Result = false;

								this.fragment.Clear();
								this.fragmentLength = 0;
							}

							if (this.inputState != 0)
								this.inputState = 5;
						}
						else
							this.inputState--;
						break;

					case 10:    // Check for attributes.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState--;
						else if (ch == '"')
							this.inputState++;
						else if (ch == '\'')
							this.inputState += 2;
						break;

					case 11:    // Double quote attribute.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '"')
							this.inputState--;
						break;

					case 12:    // Single quote attribute.
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '\'')
							this.inputState -= 2;
						break;

					case 13:    // Third character in start of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						else if (ch == '[')
							this.inputState = 18;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 14:    // Fourth character in start of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 15:    // In comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						break;

					case 16:    // Second character in end of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '-')
							this.inputState++;
						else
							this.inputState--;
						break;

					case 17:    // Third character in end of comment
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState = 5;
						else
							this.inputState -= 2;
						break;

					case 18:    // Fourth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == 'C')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 19:    // Fifth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == 'D')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 20:    // Sixth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == 'A')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 21:    // Seventh character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == 'T')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 22:    // Eighth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == 'A')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 23:    // Ninth character in start of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '[')
							this.inputState++;
						else
						{
							await this.ToError();
							return false;
						}
						break;

					case 24:    // In CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == ']')
							this.inputState++;
						break;

					case 25:    // Second character in end of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == ']')
							this.inputState++;
						else
							this.inputState--;
						break;

					case 26:    // Third character in end of CDATA
						this.fragment.Append(ch);
						if (++this.fragmentLength > MaxFragmentSize)
						{
							await this.ToError();
							return false;
						}
						else if (ch == '>')
							this.inputState = 5;
						else if (ch != ']')
							this.inputState -= 2;
						break;

					default:
						break;
				}
			}

			return Result;
		}

		private async Task ToError()
		{
			this.inputState = -1;

			if (!(this.client is null))
			{
				await this.client.DisposeAsync();
				this.client = null;
			}

			this.State = XmppState.Error;
		}

		private async Task ProcessStream(string Xml)
		{
			try
			{
				int i = Xml.IndexOf("?>");
				if (i >= 0)
					Xml = Xml.Substring(i + 2).TrimStart();

				this.streamHeader = Xml;

				i = Xml.IndexOf(":stream");
				if (i < 0)
					this.streamFooter = "</stream>";
				else
					this.streamFooter = "</" + Xml.Substring(1, i - 1) + ":stream>";

				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(Xml + this.streamFooter);

				if (Doc.DocumentElement.LocalName != "stream")
					throw new XmppException("Invalid stream.", Doc.DocumentElement);

				XmlElement Stream = Doc.DocumentElement;

				this.streamId = XML.Attribute(Stream, "id");
				string From = XML.Attribute(Stream, "from");

				if (From != this.componentSubDomain)
					await this.ConnectionError(new System.Exception("Invalid component address."));
				else
				{
					this.State = XmppState.StreamOpened;

					string s = this.streamId + this.sharedSecret;
					byte[] Data = System.Text.Encoding.UTF8.GetBytes(s);

					await this.BeginWrite("<handshake>" + Hashes.ComputeSHA1HashString(Data) + "</handshake>", null, null);
				}
			}
			catch (Exception ex)
			{
				await this.ConnectionError(ex);
			}
		}

		private async Task<bool> ProcessFragment(string Xml)
		{
			XmlDocument Doc;
			XmlElement E;

			try
			{
				Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(this.streamHeader + Xml + this.streamFooter);

				foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
				{
					E = N as XmlElement;
					if (E is null)
						continue;

					switch (E.LocalName)
					{
						case "iq":
							string Type = XML.Attribute(E, "type");
							string Id = XML.Attribute(E, "id");
							string To = XML.Attribute(E, "to");
							string From = XML.Attribute(E, "from");
							switch (Type)
							{
								case "get":
									if (this.responses.TryGet(From, Id, out string ResponseXml, out bool Ok))
									{
										if (Ok)
											await this.SendIqResult(Id, To, From, ResponseXml);
										else
											await this.SendIqError(Id, To, From, ResponseXml);
									}
									else
										this.ProcessIq(this.iqGetHandlers, new IqEventArgs(this, E, Id, To, From));
									break;

								case "set":
									if (this.responses.TryGet(From, Id, out ResponseXml, out Ok))
									{
										if (Ok)
											await this.SendIqResult(Id, To, From, ResponseXml);
										else
											await this.SendIqError(Id, To, From, ResponseXml);
									}
									else
										this.ProcessIq(this.iqSetHandlers, new IqEventArgs(this, E, Id, To, From));
									break;

								case "result":
								case "error":
									uint SeqNr;
									EventHandlerAsync<IqResultEventArgs> Callback;
									object State;
									PendingRequest Rec;

									Ok = (Type == "result");

									if (uint.TryParse(Id, out SeqNr))
									{
										lock (this.synchObject)
										{
											if (this.pendingRequestsBySeqNr.TryGetValue(SeqNr, out Rec))
											{
												Callback = Rec.IqCallback;
												State = Rec.State;

												this.pendingRequestsBySeqNr.Remove(SeqNr);
												this.pendingRequestsByTimeout.Remove(Rec.Timeout);
											}
											else
											{
												Callback = null;
												State = null;
											}
										}

										await Callback.Raise(this, new IqResultEventArgs(E, Id, To, From, Ok, State));
									}
									break;
							}
							break;

						case "message":
							this.ProcessMessage(new MessageEventArgs(this, E));
							break;

						case "presence":
							this.ProcessPresence(new PresenceEventArgs(this, E));
							break;

						case "error":
							XmppException StreamException = XmppClient.GetExceptionObject(E);
							if (StreamException is SeeOtherHostException SeeOther)
							{
								this.host = SeeOther.NewHost;
								this.inputState = -1;

								this.Information("Reconnecting to " + this.host);

								await this.DisposeClient();
								this.Connect();
								return false;
							}
							else
								throw StreamException;

						case "handshake":
							this.State = XmppState.Connected;
							this.secondTimer = new Timer(this.SecondTimerCallback, null, 1000, 1000);
							break;

						default:
							break;
					}
				}
			}
			catch (Exception ex)
			{
				await this.ConnectionError(ex);
				return false;
			}

			return true;
		}

		private async void ProcessMessage(MessageEventArgs e)
		{
			try
			{
				EventHandlerAsync<MessageEventArgs> h = null;
				string Key;

				lock (this.synchObject)
				{
					foreach (XmlNode N in e.Message.ChildNodes)
					{
						if (!(N is XmlElement E))
							continue;

						Key = E.LocalName + " " + E.NamespaceURI;
						if (this.messageHandlers.TryGetValue(Key, out h))
						{
							e.Content = E;
							break;
						}
						else
							h = null;
					}
				}

				if (!(h is null))
					this.Information(h.GetMethodInfo().Name);
				else
				{
					switch (e.Type)
					{
						case MessageType.Chat:
							this.Information("OnChatMessage()");
							h = this.OnChatMessage;
							break;

						case MessageType.Error:
							this.Information("OnErrorMessage()");
							h = this.OnErrorMessage;
							break;

						case MessageType.GroupChat:
							this.Information("OnGroupChatMessage()");
							h = this.OnGroupChatMessage;
							break;

						case MessageType.Headline:
							this.Information("OnHeadlineMessage()");
							h = this.OnHeadlineMessage;
							break;

						case MessageType.Normal:
						default:
							this.Information("OnNormalMessage()");
							h = this.OnNormalMessage;
							break;
					}
				}

				await h.Raise(this, e);
			}
			catch (Exception ex)
			{
				try
				{
					this.Exception(ex);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		private async void ProcessPresence(PresenceEventArgs e)
		{
			try
			{
				EventHandlerAsync<PresenceEventArgs> Callback;
				object State;
				string Id = e.Id;
				string FromBareJid = e.FromBareJID;

				if (uint.TryParse(Id, out uint SeqNr) ||
					((e.Type == PresenceType.Error || e.From.Contains("/")) && this.pendingPresenceRequests.TryGetValue(FromBareJid, out SeqNr)))
				{
					lock (this.synchObject)
					{
						if (this.pendingRequestsBySeqNr.TryGetValue(SeqNr, out PendingRequest Rec))
						{
							Callback = Rec.PresenceCallback;
							State = Rec.State;

							this.pendingRequestsBySeqNr.Remove(SeqNr);
							this.pendingRequestsByTimeout.Remove(Rec.Timeout);
						}
						else
						{
							Callback = null;
							State = null;
						}
					}

					if (string.IsNullOrEmpty(Id))
						this.pendingPresenceRequests.Remove(FromBareJid);
					else
					{
						if (this.pendingPresenceRequests.TryGetValue(FromBareJid, out uint SeqNr2) && SeqNr == SeqNr2)
							this.pendingPresenceRequests.Remove(FromBareJid);
					}
				}
				else
					Callback = null;

				if (Callback is null)
				{
					switch (e.Type)
					{
						case PresenceType.Available:
							this.Information("OnPresence()");
							Callback = this.OnPresence;
							break;

						case PresenceType.Unavailable:
							this.Information("OnPresence()");
							Callback = this.OnPresence;
							break;

						case PresenceType.Error:
						case PresenceType.Probe:
						default:
							this.Information("OnPresence()");
							Callback = this.OnPresence;
							break;

						case PresenceType.Subscribe:
							this.Information("OnPresenceSubscribe()");
							Callback = this.OnPresenceSubscribe;
							break;

						case PresenceType.Subscribed:
							this.Information("OnPresenceSubscribed()");
							Callback = this.OnPresenceSubscribed;
							break;

						case PresenceType.Unsubscribe:
							this.Information("OnPresenceUnsubscribe()");
							Callback = this.OnPresenceUnsubscribe;
							break;

						case PresenceType.Unsubscribed:
							this.Information("OnPresenceUnsubscribed()");
							Callback = this.OnPresenceUnsubscribed;
							break;
					}
				}

				await Callback.Raise(this, e);
			}
			catch (Exception ex)
			{
				try
				{
					this.Exception(ex);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		private async void ProcessIq(Dictionary<string, EventHandlerAsync<IqEventArgs>> Handlers, IqEventArgs e)
		{
			try
			{
				EventHandlerAsync<IqEventArgs> h = null;
				string Key;

				lock (this.synchObject)
				{
					foreach (XmlNode N in e.IQ.ChildNodes)
					{
						if (!(N is XmlElement E))
							continue;

						Key = E.LocalName + " " + E.NamespaceURI;
						if (Handlers.TryGetValue(Key, out h))
						{
							e.Query = E;
							break;
						}
						else
							h = null;
					}
				}

				if (h is null)
					await this.SendIqError(e.Id, e.To, e.From, "<error type='cancel'><feature-not-implemented xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/></error>");
				else
				{
					try
					{
						await h(this, e);
					}
					catch (Exception ex)
					{
						await this.SendIqError(e.Id, e.To, e.From, ex);
					}
				}
			}
			catch (Exception ex)
			{
				try
				{
					this.Exception(ex);
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Registers an IQ-Get handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to process request.</param>
		/// <param name="PublishNamespaceAsClientFeature">If the namespace should be published as a client feature.</param>
		public void RegisterIqGetHandler(string LocalName, string Namespace, EventHandlerAsync<IqEventArgs> Handler, bool PublishNamespaceAsClientFeature)
		{
			this.RegisterIqHandler(this.iqGetHandlers, LocalName, Namespace, Handler, PublishNamespaceAsClientFeature);
		}

		/// <summary>
		/// Registers an IQ-Set handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to process request.</param>
		/// <param name="PublishNamespaceAsClientFeature">If the namespace should be published as a client feature.</param>
		public void RegisterIqSetHandler(string LocalName, string Namespace, EventHandlerAsync<IqEventArgs> Handler, bool PublishNamespaceAsClientFeature)
		{
			this.RegisterIqHandler(this.iqSetHandlers, LocalName, Namespace, Handler, PublishNamespaceAsClientFeature);
		}

		private void RegisterIqHandler(Dictionary<string, EventHandlerAsync<IqEventArgs>> Handlers, string LocalName, string Namespace,
			EventHandlerAsync<IqEventArgs> Handler, bool PublishNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (Handlers.ContainsKey(Key))
					throw new ArgumentException("Handler already registered: " + Namespace + "#" + LocalName, nameof(LocalName));

				Handlers[Key] = Handler;

				if (PublishNamespaceAsClientFeature)
					this.clientFeatures[Namespace] = true;
			}
		}

		/// <summary>
		/// Unregisters an IQ-Get handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to process request.</param>
		/// <param name="RemoveNamespaceAsClientFeature">If the namespace should be removed from the lit of client features.</param>
		/// <returns>If the handler was found and removed.</returns>
		public bool UnregisterIqGetHandler(string LocalName, string Namespace, EventHandlerAsync<IqEventArgs> Handler, bool RemoveNamespaceAsClientFeature)
		{
			return this.UnregisterIqHandler(this.iqGetHandlers, LocalName, Namespace, Handler, RemoveNamespaceAsClientFeature);
		}

		/// <summary>
		/// Unregisters an IQ-Set handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to process request.</param>
		/// <param name="RemoveNamespaceAsClientFeature">If the namespace should be removed from the lit of client features.</param>
		/// <returns>If the handler was found and removed.</returns>
		public bool UnregisterIqSetHandler(string LocalName, string Namespace, EventHandlerAsync<IqEventArgs> Handler, bool RemoveNamespaceAsClientFeature)
		{
			return this.UnregisterIqHandler(this.iqSetHandlers, LocalName, Namespace, Handler, RemoveNamespaceAsClientFeature);
		}

		private bool UnregisterIqHandler(Dictionary<string, EventHandlerAsync<IqEventArgs>> Handlers, string LocalName, string Namespace,
			EventHandlerAsync<IqEventArgs> Handler, bool RemoveNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (!Handlers.TryGetValue(Key, out EventHandlerAsync<IqEventArgs> h))
					return false;

				if (h != Handler)
					return false;

				Handlers.Remove(Key);

				if (RemoveNamespaceAsClientFeature)
					this.clientFeatures.Remove(Namespace);
			}

			return true;
		}

		/// <summary>
		/// Registers a Message handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to process message.</param>
		/// <param name="PublishNamespaceAsClientFeature">If the namespace should be published as a client feature.</param>
		public void RegisterMessageHandler(string LocalName, string Namespace, EventHandlerAsync<MessageEventArgs> Handler, bool PublishNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (this.messageHandlers.ContainsKey(Key))
					throw new ArgumentException("Handler already registered: " + Namespace + "#" + LocalName, nameof(LocalName));

				this.messageHandlers[Key] = Handler;

				if (PublishNamespaceAsClientFeature)
					this.clientFeatures[Namespace] = true;
			}
		}

		/// <summary>
		/// Unregisters a Message handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to remove.</param>
		/// <param name="RemoveNamespaceAsClientFeature">If the namespace should be removed from the lit of client features.</param>
		/// <returns>If the handler was found and removed.</returns>
		public bool UnregisterMessageHandler(string LocalName, string Namespace, EventHandlerAsync<MessageEventArgs> Handler, bool RemoveNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (!this.messageHandlers.TryGetValue(Key, out EventHandlerAsync<MessageEventArgs> h))
					return false;

				if (h != Handler)
					return false;

				this.messageHandlers.Remove(Key);

				if (RemoveNamespaceAsClientFeature)
					this.clientFeatures.Remove(Namespace);
			}

			return true;
		}

		/// <summary>
		/// Registers a feature on the client.
		/// </summary>
		/// <param name="Feature">Feature to register.</param>
		public void RegisterFeature(string Feature)
		{
			lock (this.synchObject)
			{
				this.clientFeatures[Feature] = true;
			}
		}

		/// <summary>
		/// Unregisters a feature from the client.
		/// </summary>
		/// <param name="Feature">Feature to remove.</param>
		/// <returns>If the feature was found and removed.</returns>
		public bool UnregisterFeature(string Feature)
		{
			lock (this.synchObject)
			{
				return this.clientFeatures.Remove(Feature);
			}
		}

		/// <summary>
		/// Event raised when a presence message has been received from a resource.
		/// </summary>
		public event EventHandlerAsync<PresenceEventArgs> OnPresence = null;

		/// <summary>
		/// Event raised when a resource is requesting to be informed of the current client's presence
		/// </summary>
		public event EventHandlerAsync<PresenceEventArgs> OnPresenceSubscribe = null;

		/// <summary>
		/// Event raised when your presence subscription has been accepted.
		/// </summary>
		public event EventHandlerAsync<PresenceEventArgs> OnPresenceSubscribed = null;

		/// <summary>
		/// Event raised when a resource is requesting to be removed from the current client's presence
		/// </summary>
		public event EventHandlerAsync<PresenceEventArgs> OnPresenceUnsubscribe = null;

		/// <summary>
		/// Event raised when your presence unsubscription has been accepted.
		/// </summary>
		public event EventHandlerAsync<PresenceEventArgs> OnPresenceUnsubscribed = null;

		/// <summary>
		/// Raised when a chat message has been received, that is not handled by a specific message handler.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnChatMessage = null;

		/// <summary>
		/// Raised when an error message has been received, that is not handled by a specific message handler.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnErrorMessage = null;

		/// <summary>
		/// Raised when a group chat message has been received, that is not handled by a specific message handler.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnGroupChatMessage = null;

		/// <summary>
		/// Raised when a headline message has been received, that is not handled by a specific message handler.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnHeadlineMessage = null;

		/// <summary>
		/// Raised when a normal message has been received, that is not handled by a specific message handler.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnNormalMessage = null;

		/// <summary>
		/// Component sub-domain on server.
		/// </summary>
		public string ComponentSubDomain => this.componentSubDomain;

		internal string SharedSecret => this.sharedSecret;

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string From, string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.SendIq(null, From, To, Xml, "get", Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string From, string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries)
		{
			return this.SendIq(null, From, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, false, RetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string From, string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			return this.SendIq(null, From, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string From, string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.SendIq(null, From, To, Xml, "set", Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string From, string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries)
		{
			return this.SendIq(null, From, To, Xml, "set", Callback, State, RetryTimeout, NrRetries, false, RetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string From, string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			return this.SendIq(null, From, To, Xml, "set", Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
		}

		/// <summary>
		/// Returns a response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the response.</param>
		public Task SendIqResult(string Id, string From, string To, string Xml)
		{
			this.responses.Add(To, Id, Xml, true);
			return this.SendIq(Id, From, To, Xml, "result", null, null, 0, 0, false, 0);
		}

		/// <summary>
		/// Returns an error response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the response.</param>
		public Task SendIqError(string Id, string From, string To, string Xml)
		{
			this.responses.Add(To, Id, Xml, false);
			return this.SendIq(Id, From, To, Xml, "error", null, null, 0, 0, false, 0);
		}

		/// <summary>
		/// Returns an error response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="ex">Internal exception object.</param>
		public async Task SendIqError(string Id, string From, string To, Exception ex)
		{
			if (ex is StanzaExceptionException ex2)
			{
				StringBuilder Xml = new StringBuilder();

				this.Exception(ex2);

				Xml.Append("<error type='");
				Xml.Append(ex2.ErrorType);
				Xml.Append("'><");
				Xml.Append(ex2.ErrorStanzaName);
				Xml.Append(" xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
				Xml.Append("<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>");
				Xml.Append(XML.Encode(ex2.Message));
				Xml.Append("</text>");
				Xml.Append("</error>");

				await this.SendIqError(Id, From, To, Xml.ToString());
			}
			else
			{
				StringBuilder Xml = new StringBuilder();

				this.Exception(ex);

				Xml.Append("<error type='cancel'><internal-server-error xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
				Xml.Append("<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>");
				Xml.Append(XML.Encode(ex.Message));
				Xml.Append("</text>");
				Xml.Append("</error>");

				await this.SendIqError(Id, From, To, Xml.ToString());
			}
		}

		private async Task<uint> SendIq(string Id, string From, string To, string Xml, string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			PendingRequest PendingRequest = null;
			DateTime TP;
			uint SeqNr;

			if (string.IsNullOrEmpty(Id))
			{
				lock (this.synchObject)
				{
					do
					{
						SeqNr = this.seqnr++;
					}
					while (this.pendingRequestsBySeqNr.ContainsKey(SeqNr));

					PendingRequest = new PendingRequest(SeqNr, Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, To);
					TP = PendingRequest.Timeout;

					while (this.pendingRequestsByTimeout.ContainsKey(TP))
						TP = TP.AddTicks(this.gen.Next(1, 10));

					PendingRequest.Timeout = TP;

					this.pendingRequestsBySeqNr[SeqNr] = PendingRequest;
					this.pendingRequestsByTimeout[TP] = PendingRequest;

					Id = SeqNr.ToString();
				}
			}
			else
				SeqNr = 0;

			StringBuilder XmlOutput = new StringBuilder();

			XmlOutput.Append("<iq type='");
			XmlOutput.Append(Type);
			XmlOutput.Append("' id='");
			XmlOutput.Append(Id);

			if (!string.IsNullOrEmpty(From))
			{
				XmlOutput.Append("' from='");
				XmlOutput.Append(XML.Encode(From));
			}

			if (!string.IsNullOrEmpty(To))
			{
				XmlOutput.Append("' to='");
				XmlOutput.Append(XML.Encode(To));
			}

			XmlOutput.Append("'>");
			XmlOutput.Append(Xml);
			XmlOutput.Append("</iq>");

			string IqXml = XmlOutput.ToString();
			if (!(PendingRequest is null))
				PendingRequest.Xml = IqXml;

			await this.BeginWrite(IqXml, null, null);

			return SeqNr;
		}

		/// <summary>
		/// Performs a synchronous IQ Get request/response operation.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public XmlElement IqGet(string From, string To, string Xml, int Timeout)
		{
			Task<XmlElement> Result = this.IqGetAsync(From, To, Xml);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous IQ Get request/response operation.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<XmlElement> IqGetAsync(string From, string To, string Xml)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.SendIqGet(From, To, Xml, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Response);
				else
					Result.TrySetException(e.StanzaError ?? new XmppException("Unable to perform IQ Get."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Performs a synchronous IQ Set request/response operation.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public XmlElement IqSet(string From, string To, string Xml, int Timeout)
		{
			Task<XmlElement> Result = this.IqSetAsync(From, To, Xml);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous IQ Set request/response operation.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<XmlElement> IqSetAsync(string From, string To, string Xml)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.SendIqSet(From, To, Xml, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Response);
				else
					Result.TrySetException(e.StanzaError ?? new XmppException("Unable to perform IQ Set."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		public Task SetPresence(string From)
		{
			return this.SetPresence(From, Availability.Online, string.Empty, null);
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="Availability">Client availability.</param>
		public Task SetPresence(string From, Availability Availability)
		{
			return this.SetPresence(From, Availability, string.Empty, null);
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="Availability">Client availability.</param>
		/// <param name="CustomXml">Custom XML.</param>
		public Task SetPresence(string From, Availability Availability, string CustomXml)
		{
			return this.SetPresence(From, Availability, CustomXml, null);
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="Availability">Client availability.</param>
		/// <param name="CustomXml">Custom XML.</param>
		/// <param name="Status">Custom Status message, defined as a set of (language,text) pairs.</param>
		public async Task SetPresence(string From, Availability Availability, string CustomXml, params KeyValuePair<string, string>[] Status)
		{
			if (this.state == XmppState.Connected || this.state == XmppState.SettingPresence)
			{
				StringBuilder Xml = new StringBuilder("<presence");

				if (!string.IsNullOrEmpty(From))
				{
					Xml.Append(" from='");
					Xml.Append(XML.Encode(From));
					Xml.Append("'");
				}

				switch (Availability)
				{
					case Availability.Online:
					default:
						Xml.Append(">");
						break;

					case Availability.Away:
						Xml.Append("><show>away</show>");
						break;

					case Availability.Chat:
						Xml.Append("><show>chat</show>");
						break;

					case Availability.DoNotDisturb:
						Xml.Append("><show>dnd</show>");
						break;

					case Availability.ExtendedAway:
						Xml.Append("><show>xa</show>");
						break;

					case Availability.Offline:
						Xml.Append(" type='unavailable'>");
						break;
				}

				if (!(Status is null))
				{
					foreach (KeyValuePair<string, string> P in Status)
					{
						Xml.Append("<status");

						if (!string.IsNullOrEmpty(P.Key))
						{
							Xml.Append(" xml:lang='");
							Xml.Append(XML.Encode(P.Key));
							Xml.Append("'>");
						}
						else
							Xml.Append('>');

						Xml.Append(XML.Encode(P.Value));
						Xml.Append("</status>");
					}
				}

				if (!string.IsNullOrEmpty(CustomXml))
				{
					if (!XML.IsValidXml(CustomXml, true, true, true, true, false, false))
						throw new ArgumentException("Not valid XML.", nameof(CustomXml));

					Xml.Append(CustomXml);
				}

				Xml.Append("</presence>");

				await this.BeginWrite(Xml.ToString(), null, null);
			}
		}

		/// <summary>
		/// Requests subscription of presence information from a contact.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="BareJid">Bare JID of contact.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object.</param>
		public Task RequestPresenceSubscription(string From, string BareJid, EventHandlerAsync<PresenceEventArgs> Callback, object State)
		{
			return this.RequestPresenceAction(From, BareJid, "subscribe", Callback, State, 0, 0, false, 0, false);
		}

		private async Task RequestPresenceAction(string From, string BareJid, string Type, EventHandlerAsync<PresenceEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout, bool IdMightBeRemoved)
		{
			PendingRequest PendingRequest;
			string Id;

			if (!(Callback is null))
			{
				uint SeqNr = this.GetSeqNr(BareJid, Callback, State, out PendingRequest, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
				Id = SeqNr.ToString();

				if (IdMightBeRemoved)
					this.pendingPresenceRequests.Add(BareJid, SeqNr);
			}
			else
			{
				PendingRequest = null;
				Id = string.Empty;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(Id);
			}

			Xml.Append("' type='");
			Xml.Append(Type);
			Xml.Append("'/>");

			string PresenceXml = Xml.ToString();

			if (!(PendingRequest is null))
				PendingRequest.Xml = PresenceXml;

			await this.BeginWrite(PresenceXml, null, null);
		}

		private async Task PendingPresenceRequest_Removed(object Sender, CacheItemEventArgs<string, uint> e)
		{
			PendingRequest PendingRequest;

			lock (this.synchObject)
			{
				if (!this.pendingRequestsBySeqNr.TryGetValue(e.Value, out PendingRequest))
					return;

				this.pendingRequestsBySeqNr.Remove(e.Value);
				this.pendingRequestsByTimeout.Remove(PendingRequest.Timeout);
			}

			if (e.Reason == RemovedReason.Manual)
				return;

			try
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<presence xmlns='jabber:component:accept' type='error' from='");
				Xml.Append(PendingRequest.To);
				Xml.Append("' id='");
				Xml.Append(PendingRequest.SeqNr.ToString());
				Xml.Append("'><error type='wait'><recipient-unavailable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
				Xml.Append("<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Timeout.</text></error></iq>");

				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(Xml.ToString());

				PresenceEventArgs e2 = new PresenceEventArgs(this, Doc.DocumentElement);

				await PendingRequest.PresenceCallback.Raise(this, e2);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Requests unssubscription of presence information from a contact.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="BareJid">Bare JID of contact.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object.</param>
		public Task RequestPresenceUnsubscription(string From, string BareJid, EventHandlerAsync<PresenceEventArgs> Callback, object State)
		{
			return this.RequestPresenceAction(From, BareJid, "unsubscribe", Callback, State, 0, 0, false, 0, false);
		}

		internal Task PresenceSubscriptionAccepted(string Id, string From, string BareJid)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='subscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceSubscriptionDeclined(string Id, string From, string BareJid)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='unsubscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceUnsubscriptionAccepted(string Id, string From, string BareJid)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='unsubscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceUnsubscriptionDeclined(string Id, string From, string BareJid)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='subscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		public Task SendChatMessage(string From, string To, string Body)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, From, To, string.Empty,
				Body, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		public Task SendChatMessage(string From, string To, string Body, string Subject)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, From, To, string.Empty,
				Body, Subject, string.Empty, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		public Task SendChatMessage(string From, string To, string Body, string Subject, string Language)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, From, To, string.Empty,
				Body, Subject, Language, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		public Task SendChatMessage(string From, string To, string Body, string Subject, string Language, string ThreadId)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, From, To, string.Empty,
				Body, Subject, Language, ThreadId, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public Task SendChatMessage(string From, string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, From, To, string.Empty,
				Body, Subject, Language, ThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public Task SendMessage(MessageType Type, string From, string To, string CustomXml, string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, Type, string.Empty, From, To, CustomXml,
				Body, Subject, Language, ThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="QoS">Quality of Service level of message.</param>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		/// <param name="DeliveryCallback">Callback to call when message has been sent, or failed to be sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task SendMessage(QoSLevel QoS, MessageType Type, string From, string To, string CustomXml, string Body,
			string Subject, string Language, string ThreadId, string ParentThreadId, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			return this.SendMessage(QoS, Type, string.Empty, From, To, CustomXml, Body, Subject, Language, ThreadId, ParentThreadId, DeliveryCallback, State);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="QoS">Quality of Service level of message.</param>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="Id">Message ID</param>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		/// <param name="DeliveryCallback">Callback to call when message has been sent, or failed to be sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task SendMessage(QoSLevel QoS, MessageType Type, string Id, string From, string To,
			string CustomXml, string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<message");

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append(" id='");
				Xml.Append(XML.Encode(Id));
				Xml.Append('\'');
			}

			if (Type != MessageType.Normal)
			{
				Xml.Append(" type='");
				Xml.Append(Type.ToString().ToLower());
				Xml.Append("'");
			}

			if (QoS == QoSLevel.Unacknowledged)
			{
				Xml.Append(" from='");
				Xml.Append(XML.Encode(From));
				Xml.Append("' to='");
				Xml.Append(XML.Encode(To));
				Xml.Append('\'');
			}

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append(" xml:lang='");
				Xml.Append(XML.Encode(Language));
				Xml.Append('\'');
			}

			Xml.Append('>');

			if (!string.IsNullOrEmpty(Subject))
			{
				Xml.Append("<subject>");
				Xml.Append(XML.Encode(Subject));
				Xml.Append("</subject>");
			}

			if (!string.IsNullOrEmpty(Body))
			{
				Xml.Append("<body>");
				Xml.Append(XML.Encode(Body));
				Xml.Append("</body>");
			}

			if (!string.IsNullOrEmpty(ThreadId))
			{
				Xml.Append("<thread");

				if (!string.IsNullOrEmpty(ParentThreadId))
				{
					Xml.Append(" parent='");
					Xml.Append(XML.Encode(ParentThreadId));
					Xml.Append("'");
				}

				Xml.Append(">");
				Xml.Append(XML.Encode(ThreadId));
				Xml.Append("</thread>");
			}

			if (!string.IsNullOrEmpty(CustomXml))
			{
				if (!XML.IsValidXml(CustomXml, true, true, true, true, false, false))
					throw new ArgumentException("Not valid XML.", nameof(CustomXml));

				Xml.Append(CustomXml);
			}

			Xml.Append("</message>");

			string MessageXml = Xml.ToString();

			switch (QoS)
			{
				case QoSLevel.Unacknowledged:
					await this.BeginWrite(MessageXml, async (Sender, e) => await this.DeliveryCallback(DeliveryCallback, State, true), State);
					break;

				case QoSLevel.Acknowledged:
					Xml.Clear();
					Xml.Append("<qos:acknowledged xmlns:qos='");
					Xml.Append(XmppClient.NamespaceQualityOfServiceCurrent);
					Xml.Append("'>");
					Xml.Append(MessageXml);
					Xml.Append("</qos:acknowledged>");

					await this.SendIqSet(From, To, Xml.ToString(), (Sender, e) => this.DeliveryCallback(DeliveryCallback, State, e.Ok), null,
						5000, int.MaxValue, true, 3600000);
					break;

				case QoSLevel.Assured:
					string MsgId = Hashes.BinaryToString(XmppClient.GetRandomBytes(16));

					Xml.Clear();
					Xml.Append("<qos:assured xmlns:qos='");
					Xml.Append(XmppClient.NamespaceQualityOfServiceCurrent);
					Xml.Append("' msgId='");
					Xml.Append(MsgId);
					Xml.Append("'>");
					Xml.Append(MessageXml);
					Xml.Append("</qos:assured>");

					await this.SendIqSet(From, To, Xml.ToString(), this.AssuredDeliveryStep, new object[] { DeliveryCallback, State, MsgId },
						5000, int.MaxValue, true, 3600000);
					break;
			}
		}

		private async Task AssuredDeliveryStep(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback = (EventHandlerAsync<DeliveryEventArgs>)P[0];
			object State = P[1];
			string MsgId = (string)P[2];

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response)
				{
					if (N.LocalName == "received")
					{
						if (MsgId == XML.Attribute((XmlElement)N, "msgId"))
						{
							StringBuilder Xml = new StringBuilder();

							Xml.Append("<qos:deliver xmlns:qos='");
							Xml.Append(XmppClient.NamespaceQualityOfServiceCurrent);
							Xml.Append("' msgId='");
							Xml.Append(MsgId);
							Xml.Append("'/>");

							await this.SendIqSet(e.To, e.From, Xml.ToString(), async (sender, e2) => await this.DeliveryCallback(DeliveryCallback, State, e2.Ok), null,
								5000, int.MaxValue, true, 3600000);
							return;
						}
					}
				}
			}

			await this.DeliveryCallback(DeliveryCallback, State, false);
		}

		private Task DeliveryCallback(EventHandlerAsync<DeliveryEventArgs> Callback, object State, bool Ok)
		{
			if (Callback is null)
				return Task.CompletedTask;  // Do not call Raise. null Callbacks are common, and should not result in a warning in sniffers.
			else
				return Callback.Raise(this, new DeliveryEventArgs(State, Ok));
		}

		/// <summary>
		/// Number of seconds before a network connection risks being closed by the network, if no communication is done over it.
		/// To avoid this, ping messages are sent over the network with an interval of half this value (in seconds).
		/// </summary>
		public int KeepAliveSeconds
		{
			get => this.keepAliveSeconds;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.KeepAliveSeconds));

				this.keepAliveSeconds = value;
			}
		}

		private async Task AcknowledgedQoSMessageHandler(object Sender, IqEventArgs e)
		{
			foreach (XmlNode N in e.Query.ChildNodes)
			{
				if (N.LocalName == "message")
				{
					MessageEventArgs e2 = new MessageEventArgs(this, (XmlElement)N)
					{
						From = e.From,
						To = e.To
					};

					await this.SendIqResult(e.Id, e.To, e.From, string.Empty);
					this.ProcessMessage(e2);

					return;
				}
			}

			throw new BadRequestException(string.Empty, e.Query);
		}

		/// <summary>
		/// Maximum number of pending incoming assured messages received from a single source.
		/// </summary>
		public int MaxAssuredMessagesPendingFromSource
		{
			get => this.maxAssuredMessagesPendingFromSource;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.MaxAssuredMessagesPendingFromSource));

				this.maxAssuredMessagesPendingFromSource = value;
			}
		}

		/// <summary>
		/// Maximum total number of pending incoming assured messages received.
		/// </summary>
		public int MaxAssuredMessagesPendingTotal
		{
			get => this.maxAssuredMessagesPendingTotal;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.MaxAssuredMessagesPendingTotal));

				this.maxAssuredMessagesPendingTotal = value;
			}
		}

		/// <summary>
		/// Default retry timeout, in milliseconds.
		/// This value is used when sending IQ requests wihtout specifying request-specific retry parameter values.
		/// </summary>
		public int DefaultRetryTimeout
		{
			get => this.defaultRetryTimeout;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.DefaultRetryTimeout));

				this.defaultRetryTimeout = value;
			}
		}

		/// <summary>
		/// Default number of retries if results or errors are not returned.
		/// This value is used when sending IQ requests wihtout specifying request-specific retry parameter values.
		/// </summary>
		public int DefaultNrRetries
		{
			get => this.defaultNrRetries;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.DefaultNrRetries));

				this.defaultNrRetries = value;
			}
		}

		/// <summary>
		/// Default maximum retry timeout, in milliseconds.
		/// This value is used when sending IQ requests wihtout specifying request-specific retry parameter values.
		/// </summary>
		public int DefaultMaxRetryTimeout
		{
			get => this.defaultMaxRetryTimeout;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.DefaultMaxRetryTimeout));

				this.defaultMaxRetryTimeout = value;
			}
		}

		/// <summary>
		/// Default Drop-off value. If drop-off is used, the retry timeout is doubled for each retry, up till the maximum retry timeout time.
		/// This value is used when sending IQ requests wihtout specifying request-specific retry parameter values.
		/// </summary>
		public bool DefaultDropOff
		{
			get => this.defaultDropOff;
			set => this.defaultDropOff = value;
		}

		/// <summary>
		/// Tries to get a roster item for a given Bare JID.
		/// </summary>
		/// <param name="BareJid">Bare JID</param>
		/// <param name="Item">Roster item, if found, or null, if not found.</param>
		/// <returns>If a roster item was found.</returns>
		public bool TryGetRosterItem(string BareJid, out RosterItem Item)
		{
			GetRosterItemEventHandler h = this.OnGetRosterItem;
			if (h is null)
			{
				Item = null;
				return false;
			}
			else
			{
				Item = h(BareJid);
				return !(Item is null);
			}
		}

		/// <summary>
		/// Event raised when the component needs a roster item. Results are not cached. For performance reasons, it might be wise
		/// to cache results by the event handler.
		/// </summary>
		public event GetRosterItemEventHandler OnGetRosterItem = null;

		private async Task AssuredQoSMessageHandler(object Sender, IqEventArgs e)
		{
			string FromBareJid = XmppClient.GetBareJID(e.From);
			string MsgId = XML.Attribute(e.Query, "msgId");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				if (N.LocalName == "message")
				{
					MessageEventArgs e2 = new MessageEventArgs(this, (XmlElement)N)
					{
						From = e.From,
						To = e.To
					};

					lock (this.synchObject)
					{
						if (this.nrAssuredMessagesPending >= this.maxAssuredMessagesPendingTotal)
						{
							Log.Warning("Rejected incoming assured message. Unable to manage more than " + this.maxAssuredMessagesPendingTotal.ToString() +
								" pending assured messages.", XmppClient.GetBareJID(e.To), XmppClient.GetBareJID(e.From), "ResourceConstraint",
								new KeyValuePair<string, object>("Variable", "NrAssuredMessagesPending"),
								new KeyValuePair<string, object>("Limit", (double)this.maxAssuredMessagesPendingTotal),
								new KeyValuePair<string, object>("Unit", string.Empty));

							throw new StanzaErrors.ResourceConstraintException(string.Empty, e.Query);
						}

						if (!this.TryGetRosterItem(FromBareJid, out RosterItem Item))
						{
							Log.Notice("Rejected incoming assured message. Sender not in roster.", XmppClient.GetBareJID(e.To), XmppClient.GetBareJID(e.From), "NotAllowed",
								new KeyValuePair<string, object>("Variable", "NrAssuredMessagesPending"));

							throw new NotAllowedException(string.Empty, e.Query);
						}

						if (this.pendingAssuredMessagesPerSource.TryGetValue(FromBareJid, out int i))
						{
							if (i >= this.maxAssuredMessagesPendingFromSource)
							{
								Log.Warning("Rejected incoming assured message. Unable to manage more than " + this.maxAssuredMessagesPendingFromSource.ToString() +
									" pending assured messages from each sender.", XmppClient.GetBareJID(e.To), XmppClient.GetBareJID(e.From), "ResourceConstraint",
									new KeyValuePair<string, object>("Variable", "NrPendingAssuredMessagesPerSource"),
									new KeyValuePair<string, object>("Limit", (double)this.maxAssuredMessagesPendingFromSource),
									new KeyValuePair<string, object>("Unit", string.Empty));

								throw new StanzaErrors.ResourceConstraintException(string.Empty, e.Query);
							}
						}
						else
							i = 0;

						i++;
						this.pendingAssuredMessagesPerSource[FromBareJid] = i;
						this.receivedMessages[FromBareJid + " " + MsgId] = e2;
					}

					await this.SendIqResult(e.Id, e.To, e.From, "<received msgId='" + XML.Encode(MsgId) + "'/>");
					return;
				}
			}

			throw new BadRequestException(string.Empty, e.Query);
		}

		private async Task DeliverQoSMessageHandler(object Sender, IqEventArgs e)
		{
			MessageEventArgs e2;
			string MsgId = XML.Attribute(e.Query, "msgId");
			string From = XmppClient.GetBareJID(e.From);
			string Key = From + " " + MsgId;

			lock (this.synchObject)
			{
				if (this.receivedMessages.TryGetValue(Key, out e2))
				{
					this.receivedMessages.Remove(Key);
					this.nrAssuredMessagesPending--;

					if (this.pendingAssuredMessagesPerSource.TryGetValue(From, out int i))
					{
						i--;
						if (i <= 0)
							this.pendingAssuredMessagesPerSource.Remove(From);
						else
							this.pendingAssuredMessagesPerSource[From] = i;
					}
				}
				else
					e2 = null;
			}

			await this.SendIqResult(e.Id, e.To, e.From, string.Empty);

			if (!(e2 is null))
				this.ProcessMessage(e2);
		}

		private async void SecondTimerCallback(object State)
		{
			try
			{
				if (DateTime.Now >= this.nextPing)
				{
					this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);
					try
					{
						if (this.supportsPing)
						{
							if (this.pingResponse)
							{
								this.pingResponse = false;
								await this.SendPing(this.componentSubDomain, string.Empty, this.PingResult, null);
							}
							else if (!NetworkingModule.Stopping)
							{
								try
								{
									if (this.HasSniffers)
										this.Warning("Reconnecting.");

									await this.Reconnect();
								}
								catch (Exception ex)
								{
									Log.Exception(ex);
								}
							}
						}
						else
							await this.BeginWrite(" ", null, null);
					}
					catch (Exception ex)
					{
						if (this.HasSniffers)
							this.Exception(ex);

						if (!NetworkingModule.Stopping)
							await this.Reconnect();
					}
				}

				List<PendingRequest> Retries = null;
				DateTime Now = DateTime.Now;
				DateTime TP;
				bool Retry;

				lock (this.synchObject)
				{
					foreach (KeyValuePair<DateTime, PendingRequest> P in this.pendingRequestsByTimeout)
					{
						if (P.Key <= Now)
						{
							if (Retries is null)
								Retries = new List<PendingRequest>();

							Retries.Add(P.Value);
						}
						else
							break;
					}
				}

				if (!(Retries is null))
				{
					foreach (PendingRequest Request in Retries)
					{
						lock (this.synchObject)
						{
							this.pendingRequestsByTimeout.Remove(Request.Timeout);

							if (Retry = Request.CanRetry())
							{
								TP = Request.Timeout;

								while (this.pendingRequestsByTimeout.ContainsKey(TP))
									TP = TP.AddTicks(this.gen.Next(1, 10));

								Request.Timeout = TP;

								this.pendingRequestsByTimeout[Request.Timeout] = Request;
							}
							else
								this.pendingRequestsBySeqNr.Remove(Request.SeqNr);
						}

						try
						{
							if (Retry)
								await this.BeginWrite(Request.Xml, null, null);
							else if (!(Request.IqCallback is null))
							{
								StringBuilder Xml = new StringBuilder();

								Xml.Append("<iq xmlns='jabber:component:accept' type='error' from='");
								Xml.Append(Request.To);
								Xml.Append("' id='");
								Xml.Append(Request.SeqNr.ToString());
								Xml.Append("'><error type='wait'><recipient-unavailable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
								Xml.Append("<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Timeout.</text></error></iq>");

								XmlDocument Doc = new XmlDocument()
								{
									PreserveWhitespace = true
								};
								Doc.LoadXml(Xml.ToString());

								IqResultEventArgs e = new IqResultEventArgs(Doc.DocumentElement, Request.SeqNr.ToString(), string.Empty, Request.To, false,
									Request.State);

								await Request.IqCallback.Raise(this, e);
							}
						}
						catch (Exception ex)
						{
							this.Exception(ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task PingResult(object Sender, IqResultEventArgs e)
		{
			this.pingResponse = true;

			if (!e.Ok)
			{
				if (e.StanzaError is RecipientUnavailableException)
				{
					if (!NetworkingModule.Stopping)
					{
						try
						{
							await this.Reconnect();
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}
				else
					this.supportsPing = false;
			}
		}

		/// <summary>
		/// Sends an XMPP ping request.
		/// </summary>
		/// <param name="From">Address of sender.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendPing(string From, string To, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<ping xmlns='");
			Xml.Append(XmppClient.NamespacePing);
			Xml.Append("'/>");

			return this.SendIqGet(From, To, Xml.ToString(), Callback, State, 5000, 0);
		}

		private Task PingRequestHandler(object Sender, IqEventArgs e)
		{
			return e.IqResult(string.Empty);
		}

		private Task ServiceDiscoveryRequestHandler(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(XmppClient.NamespaceServiceDiscoveryInfo);
			Xml.Append("'>");

			// TODO: Discovery on accounts.

			Xml.Append("<identity category='");
			Xml.Append(XML.Encode(this.identityCategory));
			Xml.Append("' type='");
			Xml.Append(XML.Encode(this.identityType));
			Xml.Append("' name='");
			Xml.Append(XML.Encode(this.identityName));
			Xml.Append("'/>");

			lock (this.synchObject)
			{
				foreach (string Feature in this.clientFeatures.Keys)
				{
					Xml.Append("<feature var='");
					Xml.Append(XML.Encode(Feature));
					Xml.Append("'/>");
				}
			}

			Xml.Append("</query>");

			return e.IqResult(Xml.ToString());
		}

		/// <summary>
		/// Identity category, as defined in XEP-0030.
		/// </summary>
		public string IdentityCategory => this.identityCategory;

		/// <summary>
		/// Identity type, as defined in XEP-0030.
		/// </summary>
		public string IdentityType => this.identityType;

		/// <summary>
		/// Identity name, as defined in XEP-0030.
		/// </summary>
		public string IdentityName => this.identityName;

		/// <summary>
		/// Performs a presence probe against a bare JID, to learn its latest presence.
		/// </summary>
		/// <param name="BareJID">Bare JID of entity.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object.</param>
		public Task PresenceProbe(string BareJID, EventHandlerAsync<PresenceEventArgs> Callback, object State)
		{
			return this.RequestPresenceAction(this.componentSubDomain, BareJID, "probe", Callback, State,
				this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff, this.defaultMaxRetryTimeout, true);
		}

		private uint GetSeqNr(string BareJID, EventHandlerAsync<PresenceEventArgs> Callback, object State, out PendingRequest PendingRequest,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			DateTime TP;
			uint SeqNr;

			lock (this.synchObject)
			{
				do
				{
					SeqNr = this.seqnr++;
				}
				while (this.pendingRequestsBySeqNr.ContainsKey(SeqNr));

				PendingRequest = new PendingRequest(SeqNr, Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, BareJID);

				TP = PendingRequest.Timeout;

				while (this.pendingRequestsByTimeout.ContainsKey(TP))
					TP = TP.AddTicks(this.gen.Next(1, 10));

				PendingRequest.Timeout = TP;

				this.pendingRequestsBySeqNr[SeqNr] = PendingRequest;
				this.pendingRequestsByTimeout[TP] = PendingRequest;
			}

			return SeqNr;
		}

		// TODO: Encryption
	}
}
