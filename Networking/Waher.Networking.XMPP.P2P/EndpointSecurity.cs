using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.PeerToPeer;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Security;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P
{
	/// <summary>
	/// Class managing end-to-end encryption.
	/// </summary>
	public class EndpointSecurity : IEndToEndEncryption
	{
		/// <summary>
		/// urn:ieee:iot:e2e:1.0
		/// </summary>
		public const string IoTHarmonizationE2E = "urn:ieee:iot:e2e:1.0";

		/// <summary>
		/// urn:ieee:iot:p2p:1.0
		/// </summary>
		public const string IoTHarmonizationP2P = "urn:ieee:iot:p2p:1.0";

		private readonly Dictionary<string, E2eEndpoint[]> contacts;
		private XmppClient client;
		private readonly XmppServerlessMessaging serverlessMessaging;
		private RSA rsa = null;
		private RSA rsaOld = null;
		internal NistP192 p192 = null;
		internal NistP192 p192Old = null;
		internal NistP224 p224 = null;
		internal NistP224 p224Old = null;
		internal NistP256 p256 = null;
		internal NistP256 p256Old = null;
		internal NistP384 p384 = null;
		internal NistP384 p384Old = null;
		internal NistP521 p521 = null;
		internal NistP521 p521Old = null;
		private readonly UTF8Encoding encoding = new UTF8Encoding(false, false);
		private readonly object synchObject = new object();
		private readonly int rsaKeySize;
		private string rsaModulus;
		private string rsaExponent;
		private readonly int securityStrength;

		/// <summary>
		/// Class managing end-to-end encryption.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="SecurityStrength">Desired security strength.</param>
		public EndpointSecurity(XmppClient Client, int SecurityStrength)
			: this(Client, null, SecurityStrength)
		{
		}

		/// <summary>
		/// Class managing end-to-end encryption.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ServerlessMessaging">Reference to serverless messaging object.</param>
		/// <param name="SecurityStrength">Desired security strength.</param>
		public EndpointSecurity(XmppClient Client, XmppServerlessMessaging ServerlessMessaging, int SecurityStrength)
			: base()
		{
			this.securityStrength = SecurityStrength;
			this.client = Client;
			this.serverlessMessaging = ServerlessMessaging;
			this.contacts = new Dictionary<string, E2eEndpoint[]>(StringComparer.CurrentCultureIgnoreCase);

			if (SecurityStrength <= 80)
				this.rsaKeySize = 1024;
			else if (SecurityStrength <= 112)
				this.rsaKeySize = 2048;
			else if (SecurityStrength <= 128)
				this.rsaKeySize = 3072;
			else if (SecurityStrength <= 192)
				this.rsaKeySize = 7680;
			else if (SecurityStrength <= 256)
				this.rsaKeySize = 15360;
			else
				throw new ArgumentException("Key strength too high.", nameof(SecurityStrength));

			this.GenerateNewKey();

			if (this.client != null)
			{
				this.RegisterHandlers(this.client);

				this.client.OnStateChanged += Client_OnStateChanged;
				this.client.OnPresence += Client_OnPresence;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			if (this.client != null)
			{
				this.client.OnStateChanged -= Client_OnStateChanged;
				this.client.OnPresence -= Client_OnPresence;

				this.UnregisterHandlers(this.client);
				this.client = null;
			}

			this.rsaOld?.Dispose();
			this.rsaOld = null;

			if (this.rsa != null)
			{
				this.rsa.Dispose();
				this.rsa = null;

				lock (this.contacts)
				{
					foreach (E2eEndpoint[] Endpoints in this.contacts.Values)
					{
						foreach (E2eEndpoint Endpoint in Endpoints)
							Endpoint.Dispose();
					}

					this.contacts.Clear();
				}
			}
		}

		private void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			if (NewState == XmppState.RequestingSession)
				this.GenerateNewKey();
		}

		/// <summary>
		/// Generates new local keys.
		/// </summary>
		public void GenerateNewKey()
		{
			lock (this.synchObject)
			{
				this.rsaOld?.Dispose();
				this.rsaOld = this.rsa;
				this.rsa = RSA.Create();
				this.rsa.KeySize = this.rsaKeySize;

				RSAParameters P = this.rsa.ExportParameters(false);
				this.rsaModulus = Convert.ToBase64String(P.Modulus);
				this.rsaExponent = Convert.ToBase64String(P.Exponent);

				this.p192Old = this.p192;
				this.p192 = new NistP192();

				this.p224Old = this.p224;
				this.p224 = new NistP224();

				this.p256Old = this.p256;
				this.p256 = new NistP256();

				this.p384Old = this.p384;
				this.p384 = new NistP384();

				this.p521Old = this.p521;
				this.p521 = new NistP521();
			}
		}

		/// <summary>
		/// Registers XMPP stanza handlers
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public virtual void RegisterHandlers(XmppClient Client)
		{
			Client?.RegisterMessageHandler("aes", EndpointSecurity.IoTHarmonizationE2E, this.AesMessageHandler, false);
			Client?.RegisterIqGetHandler("aes", EndpointSecurity.IoTHarmonizationE2E, this.AesIqGetHandler, false);
			Client?.RegisterIqSetHandler("aes", EndpointSecurity.IoTHarmonizationE2E, this.AesIqSetHandler, false);
			Client?.RegisterIqSetHandler("synchE2e", EndpointSecurity.IoTHarmonizationE2E, this.SynchE2eHandler, false);
		}

		/// <summary>
		/// Unregisters XMPP stanza handlers
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public virtual void UnregisterHandlers(XmppClient Client)
		{
			Client?.UnregisterMessageHandler("aes", EndpointSecurity.IoTHarmonizationE2E, this.AesMessageHandler, false);
			Client?.UnregisterIqGetHandler("aes", EndpointSecurity.IoTHarmonizationE2E, this.AesIqGetHandler, false);
			Client?.UnregisterIqSetHandler("aes", EndpointSecurity.IoTHarmonizationE2E, this.AesIqSetHandler, false);
			Client?.UnregisterIqSetHandler("synchE2e", EndpointSecurity.IoTHarmonizationE2E, this.SynchE2eHandler, false);
		}

		/// <summary>
		/// Parses a set of E2E keys from XML.
		/// </summary>
		/// <param name="E2E">E2E element.</param>
		/// <param name="LocalEndpoint">Local endpoint security object, if available.</param>
		/// <returns>List of E2E keys.</returns>
		public static List<E2eEndpoint> ParseE2eKeys(XmlElement E2E, EndpointSecurity LocalEndpoint)
		{
			List<E2eEndpoint> Endpoints = null;

			foreach (XmlNode N in E2E.ChildNodes)
			{
				if (N is XmlElement E)
				{
					E2eEndpoint Endpoint = ParseE2eKey(E, LocalEndpoint);

					if (Endpoint != null)
					{
						if (Endpoints == null)
							Endpoints = new List<E2eEndpoint>();

						Endpoints.Add(Endpoint);
					}
					break;
				}
			}

			return Endpoints;
		}

		/// <summary>
		/// Parses a single E2E key from XML.
		/// </summary>
		/// <param name="E">E2E element.</param>
		/// <param name="LocalEndpoint">Local endpoint security object, if available.</param>
		/// <returns>E2E keys, if recognized, or null if not.</returns>
		public static E2eEndpoint ParseE2eKey(XmlElement E, EndpointSecurity LocalEndpoint)
		{
			if (E.NamespaceURI == IoTHarmonizationE2E)
			{
				switch (E.LocalName)
				{
					case "rsa":
						int? KeySize = null;
						byte[] Modulus = null;
						byte[] Exponent = null;

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "size":
									if (int.TryParse(Attr.Value, out int i))
										KeySize = i;
									else
										return null;
									break;

								case "mod":
									Modulus = Convert.FromBase64String(Attr.Value);
									break;

								case "exp":
									Exponent = Convert.FromBase64String(Attr.Value);
									break;
							}
						}

						if (KeySize.HasValue && Modulus != null && Exponent != null)
							return new RsaAes(KeySize.Value, Modulus, Exponent, LocalEndpoint);
						break;

					case "p192":
					case "p224":
					case "p256":
					case "p384":
					case "p521":
						byte[] X = null;
						byte[] Y = null;

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "x":
									X = Convert.FromBase64String(Attr.Value);
									break;

								case "y":
									Y = Convert.FromBase64String(Attr.Value);
									break;
							}
						}

						if (X != null && Y != null)
						{
							switch (E.LocalName)
							{
								case "p192": return new NistP192Aes(X, Y, LocalEndpoint);
								case "p224": return new NistP224Aes(X, Y, LocalEndpoint);
								case "p256": return new NistP256Aes(X, Y, LocalEndpoint);
								case "p384": return new NistP384Aes(X, Y, LocalEndpoint);
								case "p521": return new NistP521Aes(X, Y, LocalEndpoint);
							}
						}
						break;
				}
			}

			return null;
		}

		/// <summary>
		/// Adds E2E information about a peer.
		/// </summary>
		/// <param name="FullJID">Full JID of peer.</param>
		/// <param name="E2E">E2E information.</param>
		/// <returns>If information was found and added.</returns>
		public bool AddPeerPkiInfo(string FullJID, XmlElement E2E)
		{
			try
			{
				List<E2eEndpoint> Endpoints = null;
				E2eEndpoint[] OldEndpoints = null;
				int i;

				if (E2E != null)
					Endpoints = ParseE2eKeys(E2E, this);

				if (Endpoints == null)
				{
					this.RemovePeerPkiInfo(FullJID);
					return false;
				}

				Endpoints.Sort((e1, e2) => e2.SecurityStrength - e1.SecurityStrength);

				i = 0;
				int j, c = Endpoints.Count;

				for (j = 1; j < c; j++)
				{
					if (Endpoints[j].SecurityStrength >= this.securityStrength && !(Endpoints[j] is RsaAes))
						i = j;
				}

				if (i != 0)
				{
					E2eEndpoint Temp = Endpoints[i];
					Endpoints.RemoveAt(i);
					Endpoints.Insert(0, Temp);
				}

				lock (this.contacts)
				{
					if (!this.contacts.TryGetValue(FullJID, out OldEndpoints))
						OldEndpoints = null;

					this.contacts[FullJID] = Endpoints.ToArray();
				}

				if (OldEndpoints != null)
				{
					foreach (E2eEndpoint Endpoint in OldEndpoints)
						Endpoint.Dispose();
				}

				return true;
			}
			catch (Exception)
			{
				this.RemovePeerPkiInfo(FullJID);
				return false;
			}
		}

		/// <summary>
		/// Removes E2E information about a peer.
		/// </summary>
		/// <param name="FullJID">Full JID of peer.</param>
		/// <returns>If E2E information was found and removed.</returns>
		public bool RemovePeerPkiInfo(string FullJID)
		{
			E2eEndpoint[] List;

			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(FullJID, out List))
					return false;
				else
					this.contacts.Remove(FullJID);
			}

			foreach (E2eEndpoint Endpoint in List)
				Endpoint.Dispose();

			return true;
		}

		/// <summary>
		/// If infomation is available for a given endpoint.
		/// </summary>
		/// <param name="FullJid">Full JID of endpoint.</param>
		/// <returns>If E2E information is available.</returns>
		public bool ContainsKey(string FullJid)
		{
			lock (this.contacts)
			{
				return this.contacts.ContainsKey(FullJid);
			}
		}

		/// <summary>
		/// Gets available E2E options for a given endpoint.
		/// </summary>
		/// <param name="FullJid">Full JID of endpoint.</param>
		/// <returns>Available E2E options for endpoint.</returns>
		public E2eEndpoint[] GetE2eEndpoints(string FullJid)
		{
			lock (this.contacts)
			{
				if (this.contacts.TryGetValue(FullJid, out E2eEndpoint[] Endpoints))
					return Endpoints;
				else
					return new E2eEndpoint[0];
			}
		}

		/// <summary>
		/// Encrypts binary data for transmission to an endpoint.
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data</param>
		/// <returns>Encrypted data, or null if no E2E information is found for endpoint.</returns>
		public virtual byte[] Encrypt(string Id, string Type, string From, string To, byte[] Data)
		{
			E2eEndpoint[] Endpoints;

			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(To, out Endpoints))
					return null;
			}

			return Endpoints[0].Encrypt(Id, Type, From, To, Data);
		}

		/// <summary>
		/// Decrypts binary data from an endpoint.
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data</param>
		/// <returns>Decrypted data, or null if no E2E information is found for endpoint.</returns>
		public virtual byte[] Decrypt(string Id, string Type, string From, string To, byte[] Data)
		{
			E2eEndpoint[] Endpoints;

			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(From, out Endpoints))
					return null;
			}

			return Endpoints[0].Decrypt(Id, Type, From, To, Data);
		}

		/// <summary>
		/// Encrypts XML data for transmission to an endpoint.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="DataXml">XML data</param>
		/// <param name="Xml">Output</param>
		/// <returns>If E2E information was available and encryption was possible.</returns>
		public virtual bool Encrypt(XmppClient Client, string Id, string Type, string From, string To, string DataXml, StringBuilder Xml)
		{
			E2eEndpoint[] Endpoints;

			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(To, out Endpoints))
					return false;
			}

			byte[] Data = this.encoding.GetBytes(DataXml);
			bool Result = Endpoints[0].Encrypt(Id, Type, From, To, Data, Xml);

			if (Client.HasSniffers && Client.TryGetTag("ShowE2E", out object Obj) && Obj is bool && (bool)Obj)
				Client.Information(DataXml);

			return Result;
		}

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Signature</returns>
		public byte[] SignRsa(byte[] Data)
		{
			lock (this.synchObject)
			{
				return this.rsa.SignData(Data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
			}
		}

		/// <summary>
		/// Decrypts a key using the local private RSA key.
		/// </summary>
		/// <param name="Key">Encrypted key</param>
		/// <returns>Decrypted key</returns>
		public byte[] DecryptRsa(byte[] Key)
		{
			lock (this.synchObject)
			{
				return this.rsa.Decrypt(Key, RSAEncryptionPadding.OaepSHA256);
			}
		}

		/// <summary>
		/// Decrypts a key using the previous local private RSA key.
		/// </summary>
		/// <param name="Key">Encrypted key</param>
		/// <returns>Decrypted key</returns>
		public byte[] DecryptOldRsa(byte[] Key)
		{
			lock (this.synchObject)
			{
				return this.rsaOld.Decrypt(Key, RSAEncryptionPadding.OaepSHA256);
			}
		}

		private void AesMessageHandler(object Sender, MessageEventArgs e)
		{
			XmppClient Client = Sender as XmppClient;
			string Xml = this.Decrypt(Client, e.Id, e.Message.GetAttribute("type"), e.From, e.To, e.Content);
			if (Xml == null)
			{
				Client.Error("Unable to decrypt or verify message.");
				return;
			}

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);

			MessageEventArgs e2 = new MessageEventArgs(Client, Doc.DocumentElement)
			{
				From = e.From,
				To = e.To,
				Id = e.Id,
				E2eEncryption = this
			};

			Client.ProcessMessage(e2);
		}

		/// <summary>
		/// Decrypts XML data from an endpoint.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="E2eElement">Encrypted XML data</param>
		/// <returns>Decrypted XML.</returns>
		public virtual string Decrypt(XmppClient Client, string Id, string Type, string From, string To, XmlElement E2eElement)
		{
			E2eEndpoint[] Endpoints;

			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(From, out Endpoints))
					return null;
			}

			foreach (E2eEndpoint Endpoint in Endpoints)
			{
				if (Endpoint.CanDecrypt(E2eElement))
				{
					string Xml = Endpoint.Decrypt(Id, Type, From, To, E2eElement);

					if (Client.HasSniffers && Client.TryGetTag("ShowE2E", out object Obj) && Obj is bool && (bool)Obj)
						Client.Information(Xml);

					return Xml;
				}
			}

			return null;
		}

		/// <summary>
		/// Response handler for E2E encrypted iq stanzas
		/// </summary>
		/// <param name="Sender">Sender of event</param>
		/// <param name="e">Event arguments</param>
		protected virtual void IqResult(object Sender, IqResultEventArgs e)
		{
			XmppClient Client = Sender as XmppClient;
			XmlElement E = e.FirstElement;
			object[] P = (object[])e.State;
			IqResultEventHandler Callback = (IqResultEventHandler)P[0];
			object State = P[1];

			if (E != null && E.LocalName == "aes" && E.NamespaceURI == IoTHarmonizationE2E)
			{
				if (Callback != null)
				{
					string Content = this.Decrypt(Client, e.Id, e.Response.GetAttribute("type"), e.From, e.To, E);
					if (Content == null)
					{
						Client.Error("Unable to decrypt or verify response.");
						return;
					}

					StringBuilder Xml = new StringBuilder();

					Xml.Append("<iq id='");
					Xml.Append(e.Id);
					Xml.Append("' from='");
					Xml.Append(XML.Encode(e.From));
					Xml.Append("' to='");
					Xml.Append(XML.Encode(e.To));

					if (e.Ok)
						Xml.Append("' type='result'>");
					else
						Xml.Append("' type='error'>");

					Xml.Append(Content);
					Xml.Append("</iq>");

					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(Xml.ToString());

					IqResultEventArgs e2 = new IqResultEventArgs(Doc.DocumentElement, e.Id, e.To, e.From, e.Ok, State);
					Callback(Sender, e2);
				}
			}
			else if (!e.Ok && this.IsForbidden(e.ErrorElement))
			{
				E2ETransmission E2ETransmission = (E2ETransmission)P[2];
				string Id = (string)P[3];
				string To = (string)P[4];
				string Xml = (string)P[5];
				string Type = (string)P[6];
				int RetryTimeout = (int)P[7];
				int NrRetries = (int)P[8];
				bool DropOff = (bool)P[9];
				int MaxRetryTimeout = (int)P[10];
				bool PkiSynchronized = (bool)P[11];

				if (PkiSynchronized)
				{
					if (Callback != null)
					{
						e.State = State;
						Callback(Sender, e);
					}
				}
				else
				{
					this.SynchronizeE2e(To, (Sender2, e2) =>
					{
						if (e2.Ok)
						{
							this.SendIq(Client, E2ETransmission, Id, To, Xml, Type, Callback, State,
								RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, true);
						}
						else
						{
							e.State = State;
							Callback(Sender, e);
						}
					});
				};
			}
			else
			{
				if (Callback != null)
				{
					e.State = State;
					Callback(Sender, e);
				}
			}
		}

		private bool IsForbidden(XmlElement E)
		{
			if (E == null)
				return false;

			XmlElement E2;

			foreach (XmlNode N in E.ChildNodes)
			{
				E2 = N as XmlElement;
				if (E2 == null)
					continue;

				if (E2.LocalName == "forbidden" && E2.NamespaceURI == XmppClient.NamespaceXmppStanzas)
					return true;
			}

			return false;
		}

		private string EmbedIq(IqEventArgs e, string Type, string Content)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<iq id='");
			Xml.Append(e.Id);
			Xml.Append("' from='");
			Xml.Append(XML.Encode(e.From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(e.To));
			Xml.Append("' type='");
			Xml.Append(Type);
			Xml.Append("'>");
			Xml.Append(Content);
			Xml.Append("</iq>");

			return Xml.ToString();
		}

		private void AesIqGetHandler(object Sender, IqEventArgs e)
		{
			XmppClient Client = Sender as XmppClient;
			string Content = this.Decrypt(Client, e.Id, e.IQ.GetAttribute("type"), e.From, e.To, e.Query);
			if (Content == null)
			{
				Client.Error("Unable to decrypt or verify request.");
				e.IqError(new ForbiddenException("Unable to decrypt or verify message.", e.IQ));
				return;
			}

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(this.EmbedIq(e, "get", Content));

			IqEventArgs e2 = new IqEventArgs(Client, this, Doc.DocumentElement, e.Id, e.To, e.From);
			Client.ProcessIqGet(e2);
		}

		private void AesIqSetHandler(object Sender, IqEventArgs e)
		{
			XmppClient Client = Sender as XmppClient;
			string Content = this.Decrypt(Client, e.Id, e.IQ.GetAttribute("type"), e.From, e.To, e.Query);
			if (Content == null)
			{
				Client.Error("Unable to decrypt or verify request.");
				e.IqError(new ForbiddenException("Unable to decrypt or verify message.", e.IQ));
				return;
			}

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(this.EmbedIq(e, "set", Content));

			IqEventArgs e2 = new IqEventArgs(Client, this, Doc.DocumentElement, e.Id, e.To, e.From);
			Client.ProcessIqSet(e2);
		}

		/// <summary>
		/// Sends an XMPP message to an endpoint.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="QoS">Quality of Service options</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Message body</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		/// <param name="DeliveryCallback">Method to call when message has been delivered.</param>
		/// <param name="State">State object to pass on to <paramref name="DeliveryCallback"/>.</param>
		public virtual void SendMessage(XmppClient Client, E2ETransmission E2ETransmission, QoSLevel QoS,
			MessageType Type, string Id, string To, string CustomXml, string Body, string Subject,
			string Language, string ThreadId, string ParentThreadId, DeliveryEventHandler DeliveryCallback,
			object State)
		{
			if (string.IsNullOrEmpty(Id))
				Id = Client.NextId();

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<message");

			switch (Type)
			{
				case MessageType.Chat:
					Xml.Append(" type='chat'");
					break;

				case MessageType.Error:
					Xml.Append(" type='error'");
					break;

				case MessageType.GroupChat:
					Xml.Append(" type='groupchat'");
					break;

				case MessageType.Headline:
					Xml.Append(" type='headline'");
					break;
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

			Xml.Append("<body>");
			Xml.Append(XML.Encode(Body));
			Xml.Append("</body>");

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
				Xml.Append(CustomXml);

			Xml.Append("</message>");

			string MessageXml = Xml.ToString();
			StringBuilder Encrypted = new StringBuilder();

			if (this.Encrypt(Client, Id, string.Empty, this.client.FullJID, To, MessageXml, Encrypted))
			{
				MessageXml = Encrypted.ToString();

				Client.SendMessage(QoS, MessageType.Normal, Id, To, MessageXml, string.Empty,
					string.Empty, string.Empty, string.Empty, string.Empty, DeliveryCallback, State);
			}
			else if (XmppClient.GetBareJID(To) == To)
			{
				RosterItem Item = Client.GetRosterItem(To);
				bool Found = false;

				if (Item != null)
				{
					foreach (PresenceEventArgs e in Item.Resources)
					{
						Encrypted.Clear();

						if (this.Encrypt(Client, Id, string.Empty, this.client.FullJID, e.From, MessageXml, Encrypted))
						{
							Client.SendMessage(QoS, MessageType.Normal, Id, e.From, Encrypted.ToString(), string.Empty,
								string.Empty, string.Empty, string.Empty, string.Empty, DeliveryCallback, State);

							Found = true;
						}
					}
				}

				if (!Found)
				{
					Client.SendMessage(QoS, Type, Id, To, CustomXml, Body, Subject, Language,
						ThreadId, ParentThreadId, DeliveryCallback, State);
				}
			}
			else
			{
				Client.SendMessage(QoS, Type, Id, To, CustomXml, Body, Subject, Language,
					ThreadId, ParentThreadId, DeliveryCallback, State);
			}
		}

		/// <summary>
		/// Sends an IQ Get stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <returns>ID of IQ stanza.</returns>
		public uint SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			IqResultEventHandler Callback, object State)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "get", Callback, State, Client.DefaultRetryTimeout,
				Client.DefaultNrRetries, Client.DefaultDropOff, Client.DefaultMaxRetryTimeout, false);
		}

		/// <summary>
		/// Sends an IQ Get stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza.</returns>
		public uint SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, false,
				RetryTimeout, false);
		}

		/// <summary>
		/// Sends an IQ Get stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza.</returns>
		public uint SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "get", Callback, State, RetryTimeout,
				NrRetries, DropOff, MaxRetryTimeout, false);
		}

		/// <summary>
		/// Sends an IQ Set stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <returns>ID of IQ stanza.</returns>
		public uint SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			IqResultEventHandler Callback, object State)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State,
				Client.DefaultRetryTimeout, Client.DefaultNrRetries, Client.DefaultDropOff,
				Client.DefaultMaxRetryTimeout, false);
		}

		/// <summary>
		/// Sends an IQ Set stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza.</returns>
		public uint SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State, RetryTimeout,
				NrRetries, false, RetryTimeout, false);
		}

		/// <summary>
		/// Sends an IQ Set stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza.</returns>
		public uint SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State, RetryTimeout,
				NrRetries, DropOff, MaxRetryTimeout, false);
		}

		/// <summary>
		/// Sends an IQ Result stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		public void SendIqResult(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml)
		{
			this.SendIq(Client, E2ETransmission, Id, To, Xml, "result", null, null, 0, 0, false, 0, false);
		}

		/// <summary>
		/// Sends an IQ Error stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		public void SendIqError(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml)
		{
			this.SendIq(Client, E2ETransmission, Id, To, Xml, "error", null, null, 0, 0, false, 0, false);
		}

		/// <summary>
		/// Sends an IQ Error stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="ex">Exception object</param>
		public void SendIqError(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To,
			Exception ex)
		{
			StanzaExceptionException ex2 = ex as StanzaExceptionException;
			this.SendIqError(Client, E2ETransmission, Id, To, Client.ExceptionToXmppXml(ex));
		}

		/// <summary>
		/// Sends an IQ stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <param name="PkiSynchronized">If E2E information has been synchronized. If not, and a forbidden response is returned,
		/// E2E information is first synchronized, and the operation retried, before conceding failure.</param>
		/// <returns>ID of IQ stanza, if none provided in <paramref name="Id"/>.</returns>
		protected uint SendIq(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml,
			string Type, IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout, bool PkiSynchronized)
		{
			if (string.IsNullOrEmpty(Id))
				Id = Client.NextId();

			StringBuilder Encrypted = new StringBuilder();

			if (this.Encrypt(Client, Id, Type, this.client.FullJID, To, Xml, Encrypted))
			{
				string XmlEnc = Encrypted.ToString();

				return Client.SendIq(Id, To, XmlEnc, Type, this.IqResult,
					new object[] { Callback, State, E2ETransmission, Id, To, Xml, Type, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, PkiSynchronized },
					RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
			}
			else
				return Client.SendIq(Id, To, Xml, Type, Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
		}

		/// <summary>
		/// Appends E2E information to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public void AppendE2eInfo(StringBuilder Xml)
		{
			PointOnCurve P;

			Xml.Append("<e2e xmlns='");
			Xml.Append(IoTHarmonizationE2E);
			Xml.Append("'><p521 x='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork((P = this.p521.PublicKey).X)));
			Xml.Append("' y='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(P.Y)));
			Xml.Append("'/><p384 x='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork((P = this.p384.PublicKey).X)));
			Xml.Append("' y='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(P.Y)));
			Xml.Append("'/><p256 x='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork((P = this.p256.PublicKey).X)));
			Xml.Append("' y='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(P.Y)));
			Xml.Append("'/><p224 x='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork((P = this.p224.PublicKey).X)));
			Xml.Append("' y='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(P.Y)));
			Xml.Append("'/><p192 x='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork((P = this.p192.PublicKey).X)));
			Xml.Append("' y='");
			Xml.Append(Convert.ToBase64String(EcAes256.ToNetwork(P.Y)));
			Xml.Append("'/><rsa size='");
			Xml.Append(this.rsaKeySize.ToString());
			Xml.Append("' mod='");
			Xml.Append(this.rsaModulus);
			Xml.Append("' exp='");
			Xml.Append(this.rsaExponent);
			Xml.Append("'/></e2e>");
		}

		/// <summary>
		/// Synchronizes End-to-End Encryption and Peer-to-Peer connectivity parameters with a remote entity.
		/// </summary>
		/// <param name="FullJID">Full JID of remote entity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		public void SynchronizeE2e(string FullJID, IqResultEventHandler Callback)
		{
			this.client.SendIqSet(FullJID, this.GetE2eXml(), (Sender, e) =>
			{
				if (e.Ok && e.FirstElement != null)
					this.ParseE2e(e.FirstElement, FullJID);

				if (Callback != null)
				{
					try
					{
						Callback(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, null);
		}

		private string GetE2eXml()
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<synchE2e xmlns='");
			Xml.Append(IoTHarmonizationE2E);
			Xml.Append("'>");

			this.AppendE2eInfo(Xml);
			this.serverlessMessaging?.AppendP2pInfo(Xml);

			Xml.Append("</synchE2e>");

			return Xml.ToString();
		}

		private void ParseE2e(XmlElement E, string RemoteFullJID)
		{
			XmlElement E2E = null;
			XmlElement P2P = null;

			if (E != null && E.LocalName == "synchE2e" && E.NamespaceURI == IoTHarmonizationE2E)
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2)
					{
						switch (E2.LocalName)
						{
							case "e2e":
								if (E2.NamespaceURI == IoTHarmonizationE2E)
									E2E = E2;
								break;

							case "p2p":
								if (E2.NamespaceURI == IoTHarmonizationP2P)
									P2P = E2;
								break;
						}
					}
				}
			}

			bool HasE2E = this.AddPeerPkiInfo(RemoteFullJID, E2E);
			bool HasP2P = this.serverlessMessaging?.AddPeerAddressInfo(RemoteFullJID, P2P) ?? false;

			this.PeerUpdated?.Invoke(this, new PeerSynchronizedEventArgs(RemoteFullJID, HasE2E, HasP2P));
		}

		private void SynchE2eHandler(object Sender, IqEventArgs e)
		{
			RosterItem Item;

			if (e.FromBareJid != this.client.BareJID &&
				((Item = this.client.GetRosterItem(e.FromBareJid)) == null ||
				Item.State == SubscriptionState.None ||
				Item.State == SubscriptionState.Remove ||
				Item.State == SubscriptionState.Unknown))
			{
				throw new Networking.XMPP.StanzaErrors.ForbiddenException("Access denied.", e.IQ);
			}

			this.ParseE2e(e.Query, e.From);
			e.IqResult(this.GetE2eXml());
		}

		private void Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			switch (e.Type)
			{
				case PresenceType.Available:
					XmlElement E2E = null;
					XmlElement P2P = null;

					foreach (XmlNode N in e.Presence.ChildNodes)
					{
						if (N is XmlElement E)
						{
							if (E.NamespaceURI == IoTHarmonizationE2E)
							{
								switch (E.LocalName)
								{
									case "e2e":
										E2E = E;
										break;

									case "p2p":
										P2P = E;
										break;
								}
							}
						}
					}

					bool HasE2E = this.AddPeerPkiInfo(e.From, E2E);
					bool HasP2P = this.serverlessMessaging?.AddPeerAddressInfo(e.From, P2P) ?? false;

					this.PeerAvailable?.Invoke(this, new AvailableEventArgs(e, HasE2E, HasP2P));
					break;

				case PresenceType.Unavailable:
					this.PeerUnavailable?.Invoke(this, e);
					break;
			}
		}

		/// <summary>
		/// Event raised whenever a peer has become available.
		/// </summary>
		public event AvailableEventHandler PeerAvailable = null;

		/// <summary>
		/// Event raised whenever a peer has become unavailable.
		/// </summary>
		public event PresenceEventHandler PeerUnavailable = null;

		/// <summary>
		/// Event raised whenever information about a peer has been updated.
		/// </summary>
		public event PeerSynchronizedEventHandler PeerUpdated = null;

	}
}
