#define LineListener

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Threading;
using Waher.Networking.XMPP.StreamErrors;
using Waher.Networking.XMPP.Authentication;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Connection error event handler delegate.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate void XmppExceptionEventHandler(XmppClient Sender, Exception Exception);

	/// <summary>
	/// Event handler delegate for state change events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="NewState">New state reported.</param>
	public delegate void StateChangedEventHandler(XmppClient Sender, XmppState NewState);

	/// <summary>
	/// Delegate for IQ result callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void IqResultEventHandler(XmppClient Sender, IqResultEventArgs e);

	/// <summary>
	/// Manages an XMPP client connection. Implements XMPP, as defined in
	/// https://tools.ietf.org/html/rfc6120
	/// https://tools.ietf.org/html/rfc6121
	/// https://tools.ietf.org/html/rfc6122
	/// 
	/// Extensions supported directly by client object:
	/// 
	/// XEP-0077: In-band registration: http://xmpp.org/extensions/xep-0077.html
	/// </summary>
	public class XmppClient : IDisposable
	{
		private const int BufferSize = 16384;
		private const int KeepAliveTimeSeconds = 30;

		private X509CertificateCollection clientCertificates = new X509CertificateCollection();
		private LinkedList<KeyValuePair<byte[], EventHandler>> outputQueue = new LinkedList<KeyValuePair<byte[], EventHandler>>();
		private Dictionary<string, bool> authenticationMechanisms = new Dictionary<string, bool>();
		private Dictionary<string, bool> compressionMethods = new Dictionary<string, bool>();
		private Dictionary<uint, KeyValuePair<IqResultEventHandler, object>> callbackMethods = new Dictionary<uint, KeyValuePair<IqResultEventHandler, object>>();
		private byte[] buffer = new byte[BufferSize];
		private AuthenticationMethod authenticationMethod = null;
		private TcpClient client = null;
		private Stream stream = null;
		private Timer secondTimer = null;
		private DateTime nextPing = DateTime.MinValue;
		private UTF8Encoding encoding = new UTF8Encoding(false, false);
		private StringBuilder fragment = new StringBuilder();
		private XmppState state;
		private object synchObject = new object();
		private string host;
		private string language;
		private string domain;
		private string baseJid;
		private string fullJid;
		private string userName;
		private string password;
		private string streamId;
		private string streamHeader;
		private string streamFooter;
		private double version;
		private int port;
		private int keepAliveSeconds;
		private int inputState = 0;
		private int inputDepth = 0;
		private uint seqnr = 0;
		private bool trustServer = false;
		private bool isWriting = false;
		private bool canRegister = false;
		private bool hasRegistered = false;

		/// <summary>
		/// Manages an XMPP client connection. Implements XMPP, as defined in
		/// https://tools.ietf.org/html/rfc6120
		/// https://tools.ietf.org/html/rfc6121
		/// https://tools.ietf.org/html/rfc6122
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="Tls">If TLS is used to encrypt communication.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="ClientCertificates">Any client certificates.</param>
		public XmppClient(string Host, int Port, string UserName, string Password, string Language, params X509Certificate[] ClientCertificates)
		{
			this.host = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = Password;
			this.language = Language;
			this.state = XmppState.Connecting;
			this.clientCertificates.AddRange(ClientCertificates);
			this.client = new TcpClient();
			this.client.BeginConnect(Host, Port, this.ConnectCallback, null);
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				this.client.EndConnect(ar);
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}

			this.stream = new NetworkStream(this.client.Client, false);

			this.State = XmppState.StreamNegotiation;
			this.baseJid = this.userName + "@" + this.host;
			this.BeginWrite("<?xml version='1.0'?><stream:stream from='" + XmlEncode(this.baseJid) + "' to='" + XmlEncode(this.host) +
				"' version='1.0' xml:lang='" + XmlEncode(this.language) + "' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>", null);

			this.ResetState();
			this.BeginRead();
		}

		private void ResetState()
		{
			this.inputState = 0;
			this.inputDepth = 0;
			this.authenticationMethod = null;
			this.canRegister = false;

			this.authenticationMechanisms.Clear();
			this.compressionMethods.Clear();
			this.callbackMethods.Clear();
		}

		private void ConnectionError(Exception ex)
		{
			XmppExceptionEventHandler h = this.OnConnectionError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
#if LineListener
					Console.Out.WriteLine("Ex: " + ex2.Message);
#endif
				}
			}

			this.Error(ex);

			this.inputState = -1;
			if (this.stream != null)
			{
				this.stream.Close();
				this.stream = null;

				this.client.Close();
				this.client = null;
			}

			this.State = XmppState.Error;
		}

		private void Error(Exception ex)
		{
			XmppExceptionEventHandler h = this.OnError;
			if (h != null)
			{
				try
				{
					h(this, ex);
				}
				catch (Exception ex2)
				{
#if LineListener
					Console.Out.WriteLine("Ex: " + ex2.Message);
#endif
				}
			}
		}

		/// <summary>
		/// Event raised when a connection to a broker could not be made.
		/// </summary>
		public event XmppExceptionEventHandler OnConnectionError = null;

		/// <summary>
		/// Event raised when an error was encountered.
		/// </summary>
		public event XmppExceptionEventHandler OnError = null;

		/// <summary>
		/// Host or IP address of XMPP server.
		/// </summary>
		public string Host
		{
			get { return this.host; }
		}

		/// <summary>
		/// Port number to connect to.
		/// </summary>
		public int Port
		{
			get { return this.port; }
		}

		/// <summary>
		/// If server should be trusted, regardless if the operating system could validate its certificate or not.
		/// </summary>
		public bool TrustServer
		{
			get { return this.trustServer; }
			set { this.trustServer = value; }
		}

		/// <summary>
		/// Current state of connection.
		/// </summary>
		public XmppState State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					StateChangedEventHandler h = this.OnStateChanged;
					if (h != null)
					{
						try
						{
							h(this, value);
						}
						catch (Exception ex)
						{
#if LineListener
							Console.Out.WriteLine("Ex: " + ex.Message);
#endif
						}
					}
				}
			}
		}

		/// <summary>
		/// Event raised whenever the internal state of the connection changes.
		/// </summary>
		public event StateChangedEventHandler OnStateChanged = null;

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public void Dispose()
		{
			if (this.state == XmppState.Connected)
				this.BeginWrite(this.streamFooter, this.CleanUp);
			else
				this.CleanUp(this, new EventArgs());
		}

		private void CleanUp(object Sender, EventArgs e)
		{
			this.State = XmppState.Offline;

			if (this.outputQueue != null)
			{
				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
				}
			}

			/*if (this.contentCache != null)
			{
				lock (this.contentCache)
				{
					this.contentCache.Clear();
				}
			}*/

			if (this.secondTimer != null)
			{
				this.secondTimer.Dispose();
				this.secondTimer = null;
			}

			if (this.stream != null)
			{
				this.stream.Dispose();
				this.stream = null;
			}

			if (this.client != null)
			{
				this.client.Close();
				this.client = null;
			}
		}

		private void BeginWrite(string Xml, EventHandler Callback)
		{
#if LineListener
			Console.Out.WriteLine("Tx: " + Xml);
#endif
			byte[] Packet = this.encoding.GetBytes(Xml);

			lock (this.outputQueue)
			{
				if (this.isWriting)
					this.outputQueue.AddLast(new KeyValuePair<byte[], EventHandler>(Packet, Callback));
				else
					this.DoBeginWriteLocked(Packet, Callback);
			}
		}

		private void DoBeginWriteLocked(byte[] Packet, EventHandler Callback)
		{
			this.stream.BeginWrite(Packet, 0, Packet.Length, this.EndWrite, Callback);
			this.isWriting = true;
		}

		private void EndWrite(IAsyncResult ar)
		{
			if (this.stream == null)
				return;

			try
			{
				this.stream.EndWrite(ar);
				this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);

				EventHandler h = (EventHandler)ar.AsyncState;
				if (h != null)
				{
					try
					{
						h(this, new EventArgs());
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
						Debug.WriteLine(ex.StackTrace);
					}
				}

				lock (this.outputQueue)
				{
					LinkedListNode<KeyValuePair<byte[], EventHandler>> Next = this.outputQueue.First;

					if (Next == null)
						this.isWriting = false;
					else
					{
						this.outputQueue.RemoveFirst();
						this.DoBeginWriteLocked(Next.Value.Key, Next.Value.Value);
					}
				}
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);

				lock (this.outputQueue)
				{
					this.outputQueue.Clear();
					this.isWriting = false;
				}
			}
		}

		private void BeginRead()
		{
			this.stream.BeginRead(this.buffer, 0, BufferSize, this.EndRead, null);
		}

		private void EndRead(IAsyncResult ar)
		{
			string s;
			int NrRead;

			if (this.stream == null)
				return;

			try
			{
				NrRead = this.stream.EndRead(ar);
				if (NrRead > 0)
				{
					s = this.encoding.GetString(this.buffer, 0, NrRead);
#if LineListener
					Console.Out.WriteLine("Rx: " + s);
#endif
					if (this.ParseIncoming(s))
						this.stream.BeginRead(this.buffer, 0, BufferSize, this.EndRead, null);
				}
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return;
			}
		}

		private bool ParseIncoming(string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0:		// Waiting for <?
						if (ch == '<')
						{
							this.fragment.Append(ch);
							this.inputState++;
						}
						else if (ch > ' ')
						{
							this.inputState = -1;
							if (this.stream != null)
							{
								this.stream.Close();
								this.stream = null;

								this.client.Close();
								this.client = null;
							}
							this.State = XmppState.Error;
							return false;
						}
						break;

					case 1:		// Waiting for ? or >
						this.fragment.Append(ch);
						if (ch == '?')
							this.inputState++;
						else if (ch == '>')
						{
							this.inputState = 5;
							this.inputDepth = 1;
							this.ProcessStream(this.fragment.ToString());
							this.fragment.Clear();
						}
						break;

					case 2:		// Waiting for ?>
						this.fragment.Append(ch);
						if (ch == '>')
							this.inputState++;
						break;

					case 3:		// Waiting for <stream
						this.fragment.Append(ch);
						if (ch == '<')
							this.inputState++;
						else if (ch > ' ')
						{
							this.inputState = -1;
							if (this.stream != null)
							{
								this.stream.Close();
								this.stream = null;

								this.client.Close();
								this.client = null;
							}
							this.State = XmppState.Error;
							return false;
						}
						break;

					case 4:		// Waiting for >
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputState++;
							this.inputDepth = 1;
							this.ProcessStream(this.fragment.ToString());
							this.fragment.Clear();
						}
						break;

					case 5:	// Waiting for <
						if (ch == '<')
						{
							this.fragment.Append(ch);
							this.inputState++;
						}

						else if (this.inputDepth > 1)
							this.fragment.Append(ch);
						else if (ch > ' ')
						{
							this.inputState = -1;
							if (this.stream != null)
							{
								this.stream.Close();
								this.stream = null;

								this.client.Close();
								this.client = null;
							}
							this.State = XmppState.Error;
							return false;
						}
						break;

					case 6:	// Second character in tag
						this.fragment.Append(ch);
						if (ch == '/')
							this.inputState++;
						else
							this.inputState += 2;
						break;

					case 7:	// Waiting for end of closing tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 1)
							{
								this.inputState = -1;
								if (this.stream != null)
								{
									this.stream.Close();
									this.stream = null;

									this.client.Close();
									this.client = null;
								}
								this.State = XmppState.Offline;
								return false;
							}
							else
							{
								if (this.inputDepth == 1)
								{
									if (!this.ProcessFragment(this.fragment.ToString()))
										Result = false;

									this.fragment.Clear();
								}

								this.inputState = 5;
							}
						}
						break;

					case 8:	// Wait for end of start tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 5;
						}
						else if (ch == '/')
							this.inputState++;
						break;

					case 9:	// Check for end of childless tag.
						this.fragment.Append(ch);
						if (ch == '>')
						{
							if (this.inputDepth == 1)
							{
								if (!this.ProcessFragment(this.fragment.ToString()))
									Result = false;

								this.fragment.Clear();
							}

							this.inputState = 5;
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

		/// <summary>
		/// Encodes a string for use in XML.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>XML-encoded string.</returns>
		public static string XmlEncode(string s)
		{
			if (s.IndexOfAny(specialCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		/// <summary>
		/// Decodes a string used in XML.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>XML-decoded string.</returns>
		public static string XmlDecode(string s)
		{
			if (s.IndexOf('&') < 0)
				return s;

			return s.
				Replace("&apos;", "'").
				Replace("&qout;", "\"").
				Replace("&lt;", "<").
				Replace("&gt;", ">").
				Replace("&amp;", "&");
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <returns>Value of attribute, if found, or the empty string, if not found.</returns>
		public static string XmlAttribute(XmlElement E, string Name)
		{
			if (E.HasAttribute(Name))
				return E.GetAttribute(Name);
			else
				return string.Empty;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static int XmlAttribute(XmlElement E, string Name, int DefaultValue)
		{
			int Result;

			if (E.HasAttribute(Name))
			{
				if (int.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static double XmlAttribute(XmlElement E, string Name, double DefaultValue)
		{
			double Result;

			if (E.HasAttribute(Name))
			{
				if (double.TryParse(E.GetAttribute(Name).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		private static readonly char[] specialCharacters = new char[] { '<', '>', '&', '"', '\'' };

		private void ProcessStream(string Xml)
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

				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Xml + this.streamFooter);

				if (Doc.DocumentElement.LocalName != "stream")
					throw new XmppException("Invalid stream.", Doc.DocumentElement);

				XmlElement Stream = Doc.DocumentElement;

				this.version = XmlAttribute(Stream, "version", 0.0);
				this.streamId = XmlAttribute(Stream, "id");
				this.domain = XmlAttribute(Stream, "from");
				this.baseJid = this.userName + "@" + this.domain;

				if (this.version < 1.0)
					throw new XmppException("Version not supported.", Stream);
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
			}
		}

		private bool ProcessFragment(string Xml)
		{
			XmlDocument Doc;
			XmlElement E;

			try
			{
				Doc = new XmlDocument();
				Doc.LoadXml(this.streamHeader + Xml + this.streamFooter);

				foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null)
						continue;

					switch (E.LocalName)
					{
						case "iq":
							string Type = XmlAttribute(E, "type");
							string Id = XmlAttribute(E, "id");
							string To = XmlAttribute(E, "to");
							string From = XmlAttribute(E, "from");
							switch (Type)
							{
								case "get":
									// TODO
									break;

								case "set":
									// TODO
									break;

								case "result":
								case "error":
									uint SeqNr;
									IqResultEventHandler Callback;
									object State;
									KeyValuePair<IqResultEventHandler, object> Rec;
									bool Ok = (Type == "result");

									if (uint.TryParse(Id, out SeqNr))
									{
										lock (this.synchObject)
										{
											if (this.callbackMethods.TryGetValue(SeqNr, out Rec))
											{
												Callback = Rec.Key;
												State = Rec.Value;

												this.callbackMethods.Remove(SeqNr);
											}
											else
											{
												Callback = null;
												State = null;
											}
										}

										if (Callback != null)
										{
											try
											{
												Callback(this, new IqResultEventArgs(E, Id, To, From, Ok, State));
											}
											catch (Exception ex)
											{
												Debug.WriteLine(ex.Message);
												Debug.WriteLine(ex.StackTrace);
											}
										}
									}
									break;
							}
							break;

						case "message":
							// TODO
							break;

						case "presence":
							// TODO
							break;

						case "features":
							if (E.FirstChild == null)
								this.State = XmppState.Connected;
							else
							{
								foreach (XmlNode N2 in E.ChildNodes)
								{
									switch (N2.LocalName)
									{
										case "starttls":
											this.BeginWrite("<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>", null);
											return true;

										case "mechanisms":
											foreach (XmlNode N3 in N2.ChildNodes)
											{
												if (N3.LocalName == "mechanism")
													this.authenticationMechanisms[N3.InnerText.Trim().ToUpper()] = true;
											}
											break;

										case "compression":
											foreach (XmlNode N3 in N2.ChildNodes)
											{
												if (N3.LocalName == "method")
													this.compressionMethods[N3.InnerText.Trim().ToUpper()] = true;
											}
											break;

										case "auth":
											if (this.authenticationMethod == null)
											{
												if (this.authenticationMechanisms.ContainsKey("SCRAM-SHA-1"))
												{
													string Nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray(), Base64FormattingOptions.None);
													string s = "n,,n=" + this.userName + ",r=" + Nonce;
													byte[] Data = System.Text.Encoding.UTF8.GetBytes(s);

													this.State = XmppState.Authenticating;
													this.authenticationMethod = new ScramSha1(Nonce);
													this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='SCRAM-SHA-1'>" +
														Convert.ToBase64String(Data) + "</auth>", null);
												}
												else if (this.authenticationMechanisms.ContainsKey("DIGEST-MD5"))
												{
													this.State = XmppState.Authenticating;
													this.authenticationMethod = new DigestMd5();
													this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='DIGEST-MD5'/>", null);
												}
												else if (this.authenticationMechanisms.ContainsKey("CRAM-MD5"))
												{
													this.State = XmppState.Authenticating;
													this.authenticationMethod = new CramMd5();
													this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='CRAM-MD5'/>", null);
												}
												else if (this.authenticationMechanisms.ContainsKey("PLAIN"))
													throw new XmppException("PLAIN authentication method not allowed.");
												else if (this.authenticationMechanisms.ContainsKey("ANONYMOUS"))
													throw new XmppException("ANONYMOUS authentication method not allowed.");
												else
													throw new XmppException("No allowed authentication method supported.");
											}
											break;

										case "register":
											this.canRegister = true;
											break;

										default:
											// TODO
											break;
									}
								}
							}
							break;

						case "proceed":
							this.State = XmppState.StartingEncryption;

							SslStream SslStream = new SslStream(this.stream, true, this.ValidateCertificate);
							this.stream = SslStream;

							SslStream.BeginAuthenticateAsClient(this.host, this.clientCertificates, SslProtocols.Tls, true, this.EndAuthenticateAsClient, null);
							return false;

						case "failure":
							if (this.authenticationMethod != null)
							{
								if (this.canRegister && !this.hasRegistered)
								{
									this.IqGet(this.domain, "<query xmlns='jabber:iq:register'/>", this.RegistrationFormReceived, null);
									break;
								}
								else if (E.FirstChild == null)
									throw new XmppException("Unable to authenticate user.", E);
								else
									throw this.GetXmppExceptionObject(E);
							}
							else
							{
								if (E.FirstChild == null)
									throw new XmppException("Unable to start TLS negotiation.", E);
								else
									throw this.GetXmppExceptionObject(E);
							}

						case "challenge":
							if (this.authenticationMethod == null)
								throw new XmppException("No authentication method selected.", E);
							else
							{
								string Response = this.authenticationMethod.Challenge(E.InnerText, this);
								this.BeginWrite("<response xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>" + Response + "</response>", null);
							}
							break;

						case "error":	// Stream errors.
							throw this.GetXmppExceptionObject(E);

						default:
							// TODO
							break;
					}
				}
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
				return false;
			}

			return true;
		}

		private Exception GetXmppExceptionObject(XmlElement E)
		{
			string Msg = string.Empty;

			foreach (XmlNode N2 in E.ChildNodes)
			{
				if (N2.LocalName == "text")
					Msg = N2.InnerText.Trim();
			}

			foreach (XmlNode N2 in E.ChildNodes)
			{
				switch (N2.LocalName)
				{
					case "bad-format": return new BadFormatException(Msg, E);
					case "bad-namespace-prefix": return new BadNamespacePrefixException(Msg, E);
					case "conflict": return new ConflictException(Msg, E);
					case "connection-timeout": return new ConnectionTimeoutException(Msg, E);
					case "host-gone": return new HostGoneException(Msg, E);
					case "host-unknown": return new HostUnknownException(Msg, E);
					case "improper-addressing": return new ImproperAddressingException(Msg, E);
					case "internal-server-error": return new InternalServerErrorException(Msg, E);
					case "invalid-from": return new InvalidFromException(Msg, E);
					case "invalid-namespace": return new InvalidNamespaceException(Msg, E);
					case "invalid-xml": return new InvalidXmlException(Msg, E);
					case "not-authorized": return new NotAuthorizedException(Msg, E);
					case "not-well-formed": return new NotWellFormedException(Msg, E);
					case "policy-violation": return new PolicyViolationException(Msg, E);
					case "remote-connection-failed": return new RemoteConnectionFailedException(Msg, E);
					case "reset": return new ResetException(Msg, E);
					case "resource-constraint": return new ResourceConstraintException(Msg, E);
					case "restricted-xml": return new RestrictedXmlException(Msg, E);
					case "see-other-host": return new SeeOtherHostException(Msg, E);
					case "system-shutdown": return new SystemShutdownException(Msg, E);
					case "undefined-condition": return new UndefinedConditionException(Msg, E);
					case "unsupported-encoding": return new UnsupportedEncodingException(Msg, E);
					case "unsupported-feature": return new UnsupportedFeatureException(Msg, E);
					case "unsupported-stanza-type": return new UnsupportedStanzaTypeException(Msg, E);
					case "unsupported-version": return new UnsupportedVersionException(Msg, E);
					default: return new XmppException("Unrecognized stream error return newed.", E);
				}
			}

			return new XmppException("Unspecified error returned.", E);
		}

		private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors != SslPolicyErrors.None)
				return this.trustServer;

			return true;
		}

		private void EndAuthenticateAsClient(IAsyncResult ar)
		{
			try
			{
				if (this.stream != null)
				{
					((SslStream)this.stream).EndAuthenticateAsClient(ar);

					this.BeginWrite("<?xml version='1.0'?><stream:stream from='" + XmlEncode(this.baseJid) + "' to='" + XmlEncode(this.host) +
						"' version='1.0' xml:lang='" + XmlEncode(this.language) + "' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>", null);

					this.ResetState();
					this.BeginRead();
				}
			}
			catch (Exception ex)
			{
				this.ConnectionError(ex);
			}
		}

		internal string UserName
		{
			get { return this.userName; }
		}

		internal string Password
		{
			get { return this.password; }
		}

		/// <summary>
		/// Current Domain.
		/// </summary>
		public string Domain
		{
			get { return this.domain; }
		}

		/// <summary>
		/// Base JID
		/// </summary>
		public string BaseJid
		{
			get { return this.baseJid; }
		}

		/// <summary>
		/// Full JID.
		/// </summary>
		public string FullJid
		{
			get { return this.fullJid; }
		}

		private void RegistrationFormReceived(XmppClient Sender, IqResultEventArgs e)
		{
		}

		/// <summary>
		/// Performs an IQ Get request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns></returns>
		public uint IqGet(string To, string Xml, IqResultEventHandler Callback, object State)
		{
			return this.Iq(To, Xml, "get", Callback, State);
		}

		private uint Iq(string To, string Xml, string Type, IqResultEventHandler Callback, object State)
		{
			uint SeqNr;

			lock (this.synchObject)
			{
				SeqNr = this.seqnr++;
				this.callbackMethods[SeqNr] = new KeyValuePair<IqResultEventHandler, object>(Callback, State);
			}

			this.BeginWrite("<iq type='" + Type + "' id='" + SeqNr.ToString() + "' to='" + XmlEncode(To) + "'>" + Xml + "</iq>", null);

			return SeqNr;
		}

	}
}
