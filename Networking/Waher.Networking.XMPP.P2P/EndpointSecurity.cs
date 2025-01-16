using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;

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
		public const string IoTHarmonizationE2EIeeeV1 = "urn:ieee:iot:e2e:1.0";

		/// <summary>
		/// urn:nf:iot:e2e:1.0
		/// </summary>
		public const string IoTHarmonizationE2ENeuroFoundationV1 = "urn:nf:iot:e2e:1.0";

		/// <summary>
		/// Current namespace for End-to-End encryption.
		/// </summary>
		public const string IoTHarmonizationE2ECurrent = IoTHarmonizationE2ENeuroFoundationV1;

		/// <summary>
		/// Namespaces supported for End-to-end encryption.
		/// </summary>
		public static readonly string[] NamespacesIoTHarmonizationE2E = new string[]
		{
			IoTHarmonizationE2ENeuroFoundationV1,
			IoTHarmonizationE2EIeeeV1
		};

		/// <summary>
		/// urn:ieee:iot:p2p:1.0
		/// </summary>
		public const string IoTHarmonizationP2PIeeeV1 = "urn:ieee:iot:p2p:1.0";

		/// <summary>
		/// urn:nf:iot:p2p:1.0
		/// </summary>
		public const string IoTHarmonizationP2PNeuroFoundationV1 = "urn:nf:iot:p2p:1.0";

		/// <summary>
		/// Current namespace for peer-to-peer communication
		/// </summary>
		public const string IoTHarmonizationP2PCurrent = IoTHarmonizationP2PNeuroFoundationV1;

		/// <summary>
		/// Namespaces supported for Peer-to-peer communication.
		/// </summary>
		public static readonly string[] NamespacesIoTHarmonizationP2P = new string[]
		{
			IoTHarmonizationP2PNeuroFoundationV1,
			IoTHarmonizationP2PIeeeV1
		};

		private static Dictionary<string, IE2eEndpoint> endpointTypes = new Dictionary<string, IE2eEndpoint>();
		private static bool initialized = false;
		private static Type[] e2eTypes = null;
		private static bool e2eTypesLocked = false;

		private readonly Dictionary<string, RemoteEndpoints> contacts;
		private XmppClient client;
		private readonly XmppServerlessMessaging serverlessMessaging;
		private Dictionary<string, IE2eEndpoint> oldKeys = null;
		private Dictionary<string, IE2eEndpoint> keys = null;
		private IE2eEndpoint[] oldKeysSorted = null;
		private IE2eEndpoint[] keysSorted = null;
		private readonly Guid instanceId = Guid.NewGuid();
		private readonly UTF8Encoding encoding = new UTF8Encoding(false, false);
		private readonly object synchObject = new object();
		private readonly int securityStrength;
		private readonly bool ephemeralKeys;
		private Aes256 aes = new Aes256();
		private AeadChaCha20Poly1305 acp = new AeadChaCha20Poly1305();
		private ChaCha20 cha = new ChaCha20();

		private class RemoteEndpoints
		{
			public Dictionary<string, IE2eEndpoint> ByFqn;
			public IE2eEndpoint Default;
		}

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
			: this(Client, ServerlessMessaging, SecurityStrength, null)
		{
		}

		/// <summary>
		/// Class managing end-to-end encryption.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="SecurityStrength">Desired security strength.</param>
		/// <param name="LocalEndpoints">Local endpoints to use</param>
		public EndpointSecurity(XmppClient Client, int SecurityStrength, params IE2eEndpoint[] LocalEndpoints)
			: this(Client, null, SecurityStrength, LocalEndpoints)
		{
		}

		/// <summary>
		/// Class managing end-to-end encryption.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ServerlessMessaging">Reference to serverless messaging object.</param>
		/// <param name="SecurityStrength">Desired security strength.</param>
		/// <param name="LocalEndpoints">Local endpoints to use</param>
		public EndpointSecurity(XmppClient Client, XmppServerlessMessaging ServerlessMessaging, int SecurityStrength,
			params IE2eEndpoint[] LocalEndpoints)
			: base()
		{
			this.securityStrength = SecurityStrength;
			this.client = Client;
			this.serverlessMessaging = ServerlessMessaging;
			this.contacts = new Dictionary<string, RemoteEndpoints>(StringComparer.CurrentCultureIgnoreCase);

			if (LocalEndpoints is null)
			{
				LocalEndpoints = CreateEndpoints(SecurityStrength, 0, int.MaxValue);
				this.ephemeralKeys = true;
			}
			else
				this.ephemeralKeys = false;

			this.keys = new Dictionary<string, IE2eEndpoint>();
			this.keysSorted = null;

			foreach (IE2eEndpoint Endpoint in LocalEndpoints)
				this.keys[Endpoint.Namespace + "#" + Endpoint.LocalName] = Endpoint;

			if (!(this.client is null))
			{
				this.RegisterHandlers(this.client);

				this.client.OnStateChanged += this.Client_OnStateChanged;
				this.client.OnPresence += this.Client_OnPresence;
				this.client.CustomPresenceXml += this.Client_CustomPresenceXml;
			}
		}

		/// <summary>
		/// Current keys.
		/// </summary>
		internal IE2eEndpoint[] Keys
		{
			get
			{
				this.keysSorted ??= SortedArray(this.keys);
				return this.keysSorted;
			}
		}

		/// <summary>
		/// Previous keys.
		/// </summary>
		internal IE2eEndpoint[] OldKeys
		{
			get
			{
				this.oldKeysSorted ??= SortedArray(this.oldKeys);
				return this.oldKeysSorted;
			}
		}

		private Task Client_CustomPresenceXml(object Sender, CustomPresenceEventArgs e)
		{
			this.AppendE2eInfo(e.Stanza);
			this.serverlessMessaging?.AppendP2pInfo(e.Stanza);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Creates a set of endpoints within a range of security strengths.
		/// </summary>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <returns>Array of local endpoint keys.</returns>
		public static IE2eEndpoint[] CreateEndpoints(int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength)
		{
			return CreateEndpoints(DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength, null);
		}

		/// <summary>
		/// Creates a set of endpoints within a range of security strengths.
		/// </summary>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFrom">Only return endpoints derived from this type.</param>
		/// <returns>Array of local endpoint keys.</returns>
		public static IE2eEndpoint[] CreateEndpoints(int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type OnlyIfDerivedFrom)
		{
			return CreateEndpoints(DesiredSecurityStrength, MinSecurityStrength, MaxSecurityStrength, OnlyIfDerivedFrom, null);
		}

		/// <summary>
		/// Creates a set of endpoints within a range of security strengths.
		/// </summary>
		/// <param name="DesiredSecurityStrength">Desired security strength.</param>
		/// <param name="MinSecurityStrength">Minimum security strength.</param>
		/// <param name="MaxSecurityStrength">Maximum security strength.</param>
		/// <param name="OnlyIfDerivedFrom">Only return endpoints derived from this type.</param>
		/// <param name="Thread">Optional profiling thread.</param>
		/// <returns>Array of local endpoint keys.</returns>
		public static IE2eEndpoint[] CreateEndpoints(int DesiredSecurityStrength, int MinSecurityStrength, int MaxSecurityStrength,
			Type OnlyIfDerivedFrom, ProfilerThread Thread)
		{
			Thread = Thread?.CreateSubThread("Endpoints", ProfilerThreadType.Sequential);
			try
			{
				Thread?.Start();
				Thread?.NewState("Init");

				List<IE2eEndpoint> Result = new List<IE2eEndpoint>();
				TypeInfo OnlyIfDerivedFromType = OnlyIfDerivedFrom?.GetTypeInfo();
				IEnumerable<IE2eEndpoint> Templates;
				bool CheckHeritance = true;

				lock (endpointTypes)
				{
					if (initialized)
						Templates = endpointTypes.Values;
					else
					{
						Dictionary<string, IE2eEndpoint> E2eTypes = new Dictionary<string, IE2eEndpoint>();
						Dictionary<string, bool> TypeNames = new Dictionary<string, bool>();
						TypeInfo E2eTypeInfo = typeof(IE2eEndpoint).GetTypeInfo();

						foreach (KeyValuePair<string, IE2eEndpoint> P in endpointTypes)
						{
							E2eTypes[P.Key] = P.Value;
							TypeNames[P.Value.GetType().FullName] = true;
						}

						foreach (Type T in e2eTypes ?? Types.GetTypesImplementingInterface(typeof(IE2eEndpoint)))
						{
							if (TypeNames.ContainsKey(T.FullName))
								continue;

							TypeInfo TI = T.GetTypeInfo();
							if (!(e2eTypes is null) && !E2eTypeInfo.IsAssignableFrom(TI))
								continue;

							if (!(OnlyIfDerivedFromType?.IsAssignableFrom(TI) ?? true))
								continue;

							ConstructorInfo CI = Types.GetDefaultConstructor(T);
							if (CI is null)
								continue;

							try
							{
								IE2eEndpoint Endpoint = (IE2eEndpoint)CI.Invoke(Types.NoParameters);
								E2eTypes[Endpoint.Namespace + "#" + Endpoint.LocalName] = Endpoint;
							}
							catch (Exception ex)
							{
								Log.Exception(ex);
								continue;
							}
						}

						endpointTypes = E2eTypes;
						Templates = E2eTypes.Values;

						if (OnlyIfDerivedFromType is null)
							initialized = true;
						else
							CheckHeritance = false;
					}
				}

				foreach (IE2eEndpoint Endpoint in Templates)
				{
					if (CheckHeritance && !(OnlyIfDerivedFromType?.IsAssignableFrom(Endpoint.GetType().GetTypeInfo()) ?? true))
						continue;

					Thread?.NewState(Endpoint.LocalName);

					IE2eEndpoint Endpoint2 = Endpoint.Create(DesiredSecurityStrength);
					int i = Endpoint2.SecurityStrength;
					if (i >= MinSecurityStrength && i <= MaxSecurityStrength)
						Result.Add(Endpoint2);
					else
						Endpoint2.Dispose();
				}

				return Result.ToArray();
			}
			finally
			{
				Thread?.Stop();
			}
		}

		/// <summary>
		/// ID of <see cref="IEndToEndEncryption"/> instance.
		/// </summary>
		public Guid InstanceId => this.instanceId;

		/// <summary>
		/// Sets allowed cipers in endpoint security.
		/// </summary>
		/// <param name="CipherTypes">Allowed cipher types. null=all types allowed.</param>
		/// <param name="Lock">If set of ciphers should be locked.</param>
		public static void SetCiphers(Type[] CipherTypes, bool Lock)
		{
			if (e2eTypesLocked)
				throw new InvalidOperationException("Ciphers locked.");

			e2eTypes = CipherTypes;
			e2eTypesLocked = Lock;
		}

		/// <summary>
		/// Tries to get an existing endpoint, given its qualified name.
		/// </summary>
		/// <param name="LocalName">Local name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Endpoint">Endpoint, or null if not found.</param>
		/// <returns>If an endpoint was found with the given name.</returns>
		public static bool TryGetEndpoint(string LocalName, string Namespace, out IE2eEndpoint Endpoint)
		{
			if (Namespace.StartsWith("urn:ieee:"))
				Namespace = Namespace.Replace("urn:ieee:", "urn:nf:");

			string Key = Namespace + "#" + LocalName;

			if (endpointTypes.TryGetValue(Key, out Endpoint))
				return true;
			else if (initialized || endpointTypes.Count > 0)
				return false;

			CreateEndpoints(128, 0, int.MaxValue);

			return endpointTypes.TryGetValue(Key, out Endpoint);
		}

		/// <summary>
		/// Tries to create a new endpoint, given its qualified name.
		/// </summary>
		/// <param name="LocalName">Local name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Endpoint">Created endpoint, or null if not found.</param>
		/// <returns>If an endpoint was found with the given name, and a new instance was created.</returns>
		public static bool TryCreateEndpoint(string LocalName, string Namespace, out IE2eEndpoint Endpoint)
		{
			if (TryGetEndpoint(LocalName, Namespace, out Endpoint))
			{
				Endpoint = Endpoint.Create(Endpoint.SecurityStrength);
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			if (!(this.client is null))
			{
				this.client.OnStateChanged -= this.Client_OnStateChanged;
				this.client.OnPresence -= this.Client_OnPresence;
				this.client.CustomPresenceXml -= this.Client_CustomPresenceXml;

				this.UnregisterHandlers(this.client);
				this.client = null;
			}

			lock (this.synchObject)
			{
				if (!(this.oldKeys is null))
				{
					foreach (IE2eEndpoint E2e in this.OldKeys)
						E2e.Dispose();
				}

				if (!(this.keys is null))
				{
					foreach (IE2eEndpoint E2e in this.Keys)
						E2e.Dispose();
				}
			}

			lock (this.contacts)
			{
				foreach (RemoteEndpoints Endpoints in this.contacts.Values)
				{
					foreach (IE2eEndpoint Endpoint in Endpoints.ByFqn.Values)
						Endpoint.Dispose();
				}

				this.contacts.Clear();
			}

			this.aes?.Dispose();
			this.aes = null;

			this.acp?.Dispose();
			this.acp = null;

			this.cha?.Dispose();
			this.cha = null;
		}

		private Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			if (NewState == XmppState.RequestingSession && this.ephemeralKeys)
				this.GenerateNewKey();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates new local keys.
		/// </summary>
		public void GenerateNewKey()
		{
			lock (this.synchObject)
			{
				IE2eEndpoint[] Keys = this.OldKeys;

				this.oldKeys = this.keys;
				this.oldKeysSorted = this.keysSorted;

				if (!(Keys is null))
				{
					foreach (IE2eEndpoint E2e in Keys)
						E2e.Dispose();
				}

				Dictionary<string, IE2eEndpoint>  NewKeys = new Dictionary<string, IE2eEndpoint>();

				foreach (KeyValuePair<string, IE2eEndpoint> P in this.keys)
				{
					IE2eEndpoint NewKey = P.Value.Create(this.securityStrength);
					NewKey.Previous = P.Value;
					P.Value.Previous = null;
					NewKeys[P.Key] = NewKey;
				}

				this.keys = NewKeys;
				this.keysSorted = null;
			}
		}

		/// <summary>
		/// Registers XMPP stanza handlers
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public virtual void RegisterHandlers(XmppClient Client)
		{
			#region Neuro-Foundation V1

			Client?.RegisterMessageHandler("aes", IoTHarmonizationE2ENeuroFoundationV1, this.AesMessageHandler, false);
			Client?.RegisterIqGetHandler("aes", IoTHarmonizationE2ENeuroFoundationV1, this.AesIqGetHandler, false);
			Client?.RegisterIqSetHandler("aes", IoTHarmonizationE2ENeuroFoundationV1, this.AesIqSetHandler, false);
			Client?.RegisterMessageHandler("acp", IoTHarmonizationE2ENeuroFoundationV1, this.AcpMessageHandler, false);
			Client?.RegisterIqGetHandler("acp", IoTHarmonizationE2ENeuroFoundationV1, this.AcpIqGetHandler, false);
			Client?.RegisterIqSetHandler("acp", IoTHarmonizationE2ENeuroFoundationV1, this.AcpIqSetHandler, false);
			Client?.RegisterMessageHandler("cha", IoTHarmonizationE2ENeuroFoundationV1, this.ChaMessageHandler, false);
			Client?.RegisterIqGetHandler("cha", IoTHarmonizationE2ENeuroFoundationV1, this.ChaIqGetHandler, false);
			Client?.RegisterIqSetHandler("cha", IoTHarmonizationE2ENeuroFoundationV1, this.ChaIqSetHandler, false);
			Client?.RegisterIqSetHandler("synchE2e", IoTHarmonizationE2ENeuroFoundationV1, this.SynchE2eHandler, false);

			#endregion

			#region IEEE v1

			Client?.RegisterMessageHandler("aes", IoTHarmonizationE2EIeeeV1, this.AesMessageHandler, false);
			Client?.RegisterIqGetHandler("aes", IoTHarmonizationE2EIeeeV1, this.AesIqGetHandler, false);
			Client?.RegisterIqSetHandler("aes", IoTHarmonizationE2EIeeeV1, this.AesIqSetHandler, false);
			Client?.RegisterMessageHandler("acp", IoTHarmonizationE2EIeeeV1, this.AcpMessageHandler, false);
			Client?.RegisterIqGetHandler("acp", IoTHarmonizationE2EIeeeV1, this.AcpIqGetHandler, false);
			Client?.RegisterIqSetHandler("acp", IoTHarmonizationE2EIeeeV1, this.AcpIqSetHandler, false);
			Client?.RegisterMessageHandler("cha", IoTHarmonizationE2EIeeeV1, this.ChaMessageHandler, false);
			Client?.RegisterIqGetHandler("cha", IoTHarmonizationE2EIeeeV1, this.ChaIqGetHandler, false);
			Client?.RegisterIqSetHandler("cha", IoTHarmonizationE2EIeeeV1, this.ChaIqSetHandler, false);
			Client?.RegisterIqSetHandler("synchE2e", IoTHarmonizationE2EIeeeV1, this.SynchE2eHandler, false);

			#endregion
		}

		/// <summary>
		/// Unregisters XMPP stanza handlers
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public virtual void UnregisterHandlers(XmppClient Client)
		{
			#region Neuro-Foundation V1

			Client?.UnregisterMessageHandler("aes", IoTHarmonizationE2ENeuroFoundationV1, this.AesMessageHandler, false);
			Client?.UnregisterIqGetHandler("aes", IoTHarmonizationE2ENeuroFoundationV1, this.AesIqGetHandler, false);
			Client?.UnregisterIqSetHandler("aes", IoTHarmonizationE2ENeuroFoundationV1, this.AesIqSetHandler, false);
			Client?.UnregisterMessageHandler("acp", IoTHarmonizationE2ENeuroFoundationV1, this.AcpMessageHandler, false);
			Client?.UnregisterIqGetHandler("acp", IoTHarmonizationE2ENeuroFoundationV1, this.AcpIqGetHandler, false);
			Client?.UnregisterIqSetHandler("acp", IoTHarmonizationE2ENeuroFoundationV1, this.AcpIqSetHandler, false);
			Client?.UnregisterMessageHandler("cha", IoTHarmonizationE2ENeuroFoundationV1, this.ChaMessageHandler, false);
			Client?.UnregisterIqGetHandler("cha", IoTHarmonizationE2ENeuroFoundationV1, this.ChaIqGetHandler, false);
			Client?.UnregisterIqSetHandler("cha", IoTHarmonizationE2ENeuroFoundationV1, this.ChaIqSetHandler, false);
			Client?.UnregisterIqSetHandler("synchE2e", IoTHarmonizationE2ENeuroFoundationV1, this.SynchE2eHandler, false);

			#endregion

			#region IEEE v1

			Client?.UnregisterMessageHandler("aes", IoTHarmonizationE2EIeeeV1, this.AesMessageHandler, false);
			Client?.UnregisterIqGetHandler("aes", IoTHarmonizationE2EIeeeV1, this.AesIqGetHandler, false);
			Client?.UnregisterIqSetHandler("aes", IoTHarmonizationE2EIeeeV1, this.AesIqSetHandler, false);
			Client?.UnregisterMessageHandler("acp", IoTHarmonizationE2EIeeeV1, this.AcpMessageHandler, false);
			Client?.UnregisterIqGetHandler("acp", IoTHarmonizationE2EIeeeV1, this.AcpIqGetHandler, false);
			Client?.UnregisterIqSetHandler("acp", IoTHarmonizationE2EIeeeV1, this.AcpIqSetHandler, false);
			Client?.UnregisterMessageHandler("cha", IoTHarmonizationE2EIeeeV1, this.ChaMessageHandler, false);
			Client?.UnregisterIqGetHandler("cha", IoTHarmonizationE2EIeeeV1, this.ChaIqGetHandler, false);
			Client?.UnregisterIqSetHandler("cha", IoTHarmonizationE2EIeeeV1, this.ChaIqSetHandler, false);
			Client?.UnregisterIqSetHandler("synchE2e", IoTHarmonizationE2EIeeeV1, this.SynchE2eHandler, false);

			#endregion
		}

		/// <summary>
		/// Parses a set of E2E keys from XML.
		/// </summary>
		/// <param name="E2E">E2E element.</param>
		/// <param name="SecurityStrength">Desired security strength.</param>
		/// <returns>List of E2E keys.</returns>
		public static Dictionary<string, IE2eEndpoint> ParseE2eKeys(XmlElement E2E, int SecurityStrength)
		{
			Dictionary<string, IE2eEndpoint> Endpoints = null;

			foreach (XmlNode N in E2E.ChildNodes)
			{
				if (N is XmlElement E)
				{
					IE2eEndpoint Endpoint = ParseE2eKey(E);

					if (!(Endpoint is null) && Endpoint.SecurityStrength >= SecurityStrength)
					{
						Endpoints ??= new Dictionary<string, IE2eEndpoint>();
						Endpoints[Endpoint.Namespace + "#" + Endpoint.LocalName] = Endpoint;
					}
				}
			}

			return Endpoints;
		}

		/// <summary>
		/// Parses a single E2E key from XML.
		/// </summary>
		/// <param name="E">E2E element.</param>
		/// <returns>E2E keys, if recognized, or null if not.</returns>
		public static IE2eEndpoint ParseE2eKey(XmlElement E)
		{
			if (TryGetEndpoint(E.LocalName, E.NamespaceURI, out IE2eEndpoint Endpoint))
				return Endpoint.Parse(E);
			else
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
				Dictionary<string, IE2eEndpoint> Endpoints = null;

				if (!(E2E is null))
					Endpoints = ParseE2eKeys(E2E, this.securityStrength);

				if (Endpoints is null)
				{
					this.RemovePeerPkiInfo(FullJID);
					return false;
				}

				RemoteEndpoints OldEndpoints;

				lock (this.contacts)
				{
					if (!this.contacts.TryGetValue(FullJID, out OldEndpoints))
						OldEndpoints = null;

					this.contacts[FullJID] = new RemoteEndpoints()
					{
						ByFqn = Endpoints
					};
				}

				if (!(OldEndpoints is null))
				{
					foreach (IE2eEndpoint Endpoint in OldEndpoints.ByFqn.Values)
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
			RemoteEndpoints Keys;

			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(FullJID, out Keys))
					return false;
				else
					this.contacts.Remove(FullJID);
			}

			foreach (IE2eEndpoint Endpoint in Keys.ByFqn.Values)
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
		public IE2eEndpoint[] GetE2eEndpoints(string FullJid)
		{
			lock (this.contacts)
			{
				if (this.contacts.TryGetValue(FullJid, out RemoteEndpoints Endpoints))
					return SortedArray(Endpoints.ByFqn);
			}

			if (string.Compare(FullJid, this.client.BareJID) == 0)
			{
				StringBuilder Xml = new StringBuilder();

				this.AppendE2eInfo(Xml);

				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(Xml.ToString());

				return SortedArray(ParseE2eKeys(Doc.DocumentElement, this.securityStrength) ?? new Dictionary<string, IE2eEndpoint>());
			}

			return new E2eEndpoint[0];
		}

		private static IE2eEndpoint[] SortedArray(Dictionary<string, IE2eEndpoint> Endpoints)
		{
			int c = Endpoints?.Count ?? 0;
			IE2eEndpoint[] Result = new IE2eEndpoint[c];

			if (c > 0)
			{
				Endpoints.Values.CopyTo(Result, 0);

				Array.Sort(Result, (ep1, ep2) =>
				{
					int Diff = (ep2.Safe ? 1 : 0) - (ep1.Safe ? 1 : 0);
					if (Diff != 0)
						return Diff;

					Diff = ep2.SecurityStrength - ep1.SecurityStrength;
					if (Diff != 0)
						return Diff;

					Diff = (ep1.Slow ? 1 : 0) - (ep2.Slow ? 1 : 0);
					if (Diff != 0)
						return Diff;

					Diff = (ep2.SupportsSignatures ? 1 : 0) - (ep1.SupportsSignatures ? 1 : 0);
					if (Diff != 0)
						return Diff;

					Diff = (ep2.SupportsSharedSecrets ? 1 : 0) - (ep1.SupportsSharedSecrets ? 1 : 0);
					if (Diff != 0)
						return Diff;

					return ep2.Score - ep1.Score;
				});
			}

			return Result;
		}

		/// <summary>
		/// Encrypts binary data for transmission to an endpoint.
		/// </summary>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data</param>
		/// <param name="EndpointReference">Endpoint used for encryption.</param>
		/// <returns>Encrypted data, or null if no E2E information is found for endpoint.</returns>
		public virtual Task<byte[]> Encrypt(string Id, string Type, string From, string To, byte[] Data, out IE2eEndpoint EndpointReference)
		{
			IE2eEndpoint RemoteEndpoint = this.FindRemoteEndpoint(To, null);
			if (RemoteEndpoint is null)
			{
				EndpointReference = null;
				return null;
			}

			EndpointReference = this.FindLocalEndpoint(RemoteEndpoint);
			if (EndpointReference is null)
				return null;

			uint Counter = EndpointReference.GetNextCounter();
			byte[] Encrypted = EndpointReference.DefaultSymmetricCipher.Encrypt(Id, Type, From, To, Counter, Data, EndpointReference, RemoteEndpoint);

			return Task.FromResult(Encrypted);
		}

		/// <summary>
		/// Decrypts binary data from an endpoint.
		/// </summary>
		/// <param name="EndpointReference">Endpoint reference.</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="Type">Type attribute</param>
		/// <param name="From">From attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Data">Binary data</param>
		/// <param name="SymmetricCipher">Type of symmetric cipher to use to decrypt content.</param>
		/// <returns>Decrypted data, or null if no E2E information is found for endpoint.</returns>
		public virtual Task<byte[]> Decrypt(string EndpointReference, string Id, string Type, string From, string To, byte[] Data,
			IE2eSymmetricCipher SymmetricCipher)
		{
			IE2eEndpoint RemoteEndpoint = this.FindRemoteEndpoint(From, EndpointReference);
			if (RemoteEndpoint is null)
				return null;

			IE2eEndpoint LocalEndpoint = this.FindLocalEndpoint(RemoteEndpoint);
			if (LocalEndpoint is null)
				return null;

			IE2eSymmetricCipher Cipher = LocalEndpoint.DefaultSymmetricCipher;
			if (!(SymmetricCipher is null) && Cipher.GetType() != SymmetricCipher.GetType())
				Cipher = SymmetricCipher;

			byte[] Decrypted = Cipher.Decrypt(Id, Type, From, To, Data, RemoteEndpoint, LocalEndpoint);

			return Task.FromResult(Decrypted);
		}

		private IE2eEndpoint FindRemoteEndpoint(string RemoteJid, string EndpointReference)
		{
			lock (this.contacts)
			{
				if (!this.contacts.TryGetValue(RemoteJid, out RemoteEndpoints Endpoints))
					return null;

				if (EndpointReference is null)
				{
					if (Endpoints.Default is null)
					{
						IE2eEndpoint[] Ordered = SortedArray(Endpoints.ByFqn);
						IE2eEndpoint LastSafeAndFastAndSignaturesAndSharedSecrets = null;
						IE2eEndpoint LastSafeAndFastAndSignatures = null;
						IE2eEndpoint LastSafeAndFast = null;
						IE2eEndpoint LastSafe = null;

						if (Ordered.Length == 0)
							return null;

						foreach (IE2eEndpoint Endpoint in Ordered)
						{
							if (Endpoint.Safe)
							{
								LastSafe = Endpoint;

								if (!Endpoint.Slow)
								{
									LastSafeAndFast = Endpoint;

									if (Endpoint.SupportsSignatures)
									{
										LastSafeAndFastAndSignatures = Endpoint;

										if (Endpoint.SupportsSharedSecrets)
											LastSafeAndFastAndSignaturesAndSharedSecrets = Endpoint;
									}
								}
							}
						}

						Endpoints.Default = 
							LastSafeAndFastAndSignaturesAndSharedSecrets ??
							LastSafeAndFastAndSignatures ?? 
							LastSafeAndFast ?? LastSafe ?? 
							Ordered[0];
					}

					return Endpoints.Default;
				}

				if (EndpointReference.IndexOf('#') < 0)
					EndpointReference = IoTHarmonizationE2ECurrent + "#" + EndpointReference;
				else
					EndpointReference = EndpointReference.Replace("urn:ieee:", "urn:nf:");

				if (Endpoints.ByFqn.TryGetValue(EndpointReference, out IE2eEndpoint Selected))
					return Selected;
				else
					return null;
			}
		}

		/// <summary>
		/// Encrypts binary data that can be sent to an XMPP client out of band.
		/// </summary>
		/// <param name="Id">ID Attribute.</param>
		/// <param name="Type">Type Attribute.</param>
		/// <param name="From">From attribute.</param>
		/// <param name="To">To attribute.</param>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Encrypted">Encrypted data will be stored here.</param>
		/// <returns>If encryption was possible, a reference to the endpoint performing the encryption, null otherwise.</returns>
		public virtual async Task<IE2eEndpoint> Encrypt(string Id, string Type, string From, string To, Stream Data, Stream Encrypted)
		{
			IE2eEndpoint RemoteEndpoint = this.FindRemoteEndpoint(To, null);
			if (RemoteEndpoint is null)
				return null;

			IE2eEndpoint LocalEndpoint = this.FindLocalEndpoint(RemoteEndpoint);
			if (LocalEndpoint is null)
				return null;

			uint Counter = LocalEndpoint.GetNextCounter();
			await LocalEndpoint.DefaultSymmetricCipher.Encrypt(Id, Type, From, To, Counter, Data, Encrypted, LocalEndpoint, RemoteEndpoint);

			return LocalEndpoint;
		}

		/// <summary>
		/// Decrypts binary data received from an XMPP client out of band.
		/// </summary>
		/// <param name="EndpointReference">Endpoint reference.</param>
		/// <param name="Id">ID Attribute.</param>
		/// <param name="Type">Type Attribute.</param>
		/// <param name="From">From attribute.</param>
		/// <param name="To">To attribute.</param>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="SymmetricCipher">Type of symmetric cipher to use to decrypt content.</param>
		/// <returns>Decrypted data, if decryption was possible from the recipient, or null if not.</returns>
		public virtual async Task<Stream> Decrypt(string EndpointReference, string Id, string Type, string From, string To, Stream Data,
			IE2eSymmetricCipher SymmetricCipher)
		{
			IE2eEndpoint RemoteEndpoint = this.FindRemoteEndpoint(From, EndpointReference);
			if (RemoteEndpoint is null)
				return null;

			IE2eEndpoint LocalEndpoint = this.FindLocalEndpoint(RemoteEndpoint);
			if (LocalEndpoint is null)
				return null;

			IE2eSymmetricCipher Cipher = LocalEndpoint.DefaultSymmetricCipher;
			if (!(SymmetricCipher is null) && Cipher.GetType() != SymmetricCipher.GetType())
				Cipher = SymmetricCipher;

			return await Cipher.Decrypt(Id, Type, From, To, Data, RemoteEndpoint, LocalEndpoint);
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
			bool SniffE2eInfo = Client.HasSniffers && Client.TryGetTag("ShowE2E", out object Obj) && Obj is bool b && b;
			IE2eEndpoint RemoteEndpoint = this.FindRemoteEndpoint(To, null);
			if (RemoteEndpoint is null)
			{
				if (SniffE2eInfo)
					Client.Warning("Remote E2E endpoint not found. Unable to encrypt message.");

				return false;
			}

			IE2eEndpoint LocalEndpoint = this.FindLocalEndpoint(RemoteEndpoint);
			if (LocalEndpoint is null)
			{
				if (SniffE2eInfo)
					Client.Warning("Local E2E endpoint matching remote endpoint not found. Unable to encrypt message.");

				return false;
			}

			byte[] Data = this.encoding.GetBytes(DataXml);
			uint Counter = LocalEndpoint.GetNextCounter();
			bool Result = LocalEndpoint.DefaultSymmetricCipher.Encrypt(Id, Type, From, To, Counter, Data, Xml, LocalEndpoint, RemoteEndpoint);

			if (SniffE2eInfo)
				Client.Information(DataXml);

			return Result;
		}

		/// <summary>
		/// Returns the local endpoint that matches a given remote endpoint.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <returns>Matching key, or null if none found.</returns>
		public IE2eEndpoint FindLocalEndpoint(IE2eEndpoint RemoteEndpoint)
		{
			return this.FindLocalEndpoint(RemoteEndpoint.LocalName, RemoteEndpoint.Namespace);
		}

		/// <summary>
		/// Returns the local endpoint that matches a given key name.
		/// </summary>
		/// <param name="KeyName">Key (algorithm) name.</param>
		/// <returns>Matching key, or null if none found.</returns>
		public IE2eEndpoint FindLocalEndpoint(string KeyName)
		{
			return this.FindLocalEndpoint(KeyName, IoTHarmonizationE2ECurrent);
		}

		/// <summary>
		/// Returns the local endpoint that matches a given key name and namespace.
		/// </summary>
		/// <param name="KeyName">Key (algorithm) name.</param>
		/// <param name="KeyNamespace">Key (algorithm) namespace.</param>
		/// <returns>Matching key, or null if none found.</returns>
		public IE2eEndpoint FindLocalEndpoint(string KeyName, string KeyNamespace)
		{
			if (this.keys?.TryGetValue(KeyNamespace + "#" + KeyName, out IE2eEndpoint Result) ?? false)
				return Result;
			else
				return null;
		}

		/// <summary>
		/// Returns the local endpoint that matches a given type.
		/// </summary>
		/// <param name="KeyType">Type of key to get.</param>
		/// <returns>Key, if found, or null if not.</returns>
		public IE2eEndpoint FindLocalEndpoint(Type KeyType)
		{
			lock (this.synchObject)
			{
				foreach (IE2eEndpoint Endpoint in this.Keys)
				{
					if (Endpoint.GetType() == KeyType)
						return Endpoint;
				}

				if (!typeof(IE2eEndpoint).GetTypeInfo().IsAssignableFrom(KeyType.GetTypeInfo()))
					throw new ArgumentException("Not assignable from " + typeof(IE2eEndpoint).FullName, nameof(KeyType));

				return null;
			}
		}

		/// <summary>
		/// Returns the local endpoint that matches a given public key.
		/// </summary>
		/// <param name="PublicKey">Public key</param>
		/// <returns>Key, if found, or null if not.</returns>
		public IE2eEndpoint FindLocalEndpoint(byte[] PublicKey)
		{
			if (PublicKey is null)
				return null;

			string s = Convert.ToBase64String(PublicKey);

			lock (this.synchObject)
			{
				foreach (IE2eEndpoint Endpoint in this.Keys)
				{
					if (Endpoint.PublicKeyBase64 == s)
						return Endpoint;
				}
			}

			return null;
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
		/// <param name="SymmetricCipher">Type of symmetric cipher to use to decrypt content.</param>
		/// <returns>Decrypted XML, together with an endpoint reference, if successful, null otherwise..</returns>
		public virtual Tuple<string, string> Decrypt(XmppClient Client, string Id, string Type, string From, string To, XmlElement E2eElement,
			IE2eSymmetricCipher SymmetricCipher)
		{
			bool SniffE2eInfo = Client.HasSniffers && Client.TryGetTag("ShowE2E", out object Obj) && Obj is bool b && b;
			string EndpointReference = XML.Attribute(E2eElement, "r");
			IE2eEndpoint RemoteEndpoint = this.FindRemoteEndpoint(From, EndpointReference);
			if (RemoteEndpoint is null)
			{
				if (SniffE2eInfo)
					Client.Error("Remote E2E endpoint not found. Unable to decrypt message.");

				return null;
			}

			IE2eEndpoint LocalEndpoint = this.FindLocalEndpoint(RemoteEndpoint);
			if (LocalEndpoint is null)
			{
				if (SniffE2eInfo)
					Client.Error("Local E2E endpoint matching remote endpoint not found. Unable to decrypt message.");

				return null;
			}

			IE2eSymmetricCipher Cipher = LocalEndpoint.DefaultSymmetricCipher;
			if (!(SymmetricCipher is null) && Cipher.GetType() != SymmetricCipher.GetType())
				Cipher = SymmetricCipher;

			string Xml = Cipher.Decrypt(Id, Type, From, To, E2eElement, RemoteEndpoint, LocalEndpoint);
			if (Xml is null)
				return null;

			if (SniffE2eInfo)
				Client.Information(Xml);

			return new Tuple<string, string>(Xml, EndpointReference);
		}

		private Task AesMessageHandler(object Sender, MessageEventArgs e)
		{
			return this.E2eMessageHandler(Sender, e, this.aes);
		}

		private Task AcpMessageHandler(object Sender, MessageEventArgs e)
		{
			return this.E2eMessageHandler(Sender, e, this.acp);
		}

		private Task ChaMessageHandler(object Sender, MessageEventArgs e)
		{
			return this.E2eMessageHandler(Sender, e, this.cha);
		}

		private Task E2eMessageHandler(object Sender, MessageEventArgs e, IE2eSymmetricCipher Cipher)
		{
			XmppClient Client = Sender as XmppClient;
			Tuple<string, string> T = this.Decrypt(Client, e.Id, e.Message.GetAttribute("type"), e.From, e.To, e.Content, Cipher);
			if (T is null)
				return Task.CompletedTask;

			string Xml = T.Item1;
			string EndpointReference = T.Item2;

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml);

			MessageEventArgs e2 = new MessageEventArgs(Client, Doc.DocumentElement)
			{
				From = e.From,
				To = e.To,
				Id = e.Id,
				E2eEncryption = this,
				E2eReference = EndpointReference
			};

			Client.ProcessMessage(e2);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Tries to get a symmetric cipher from a reference.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Cipher">Symmetric cipher, if found.</param>
		/// <returns>If a symmetric cipher was found with the given name.</returns>
		public virtual bool TryGetSymmetricCipher(string LocalName, string Namespace, out IE2eSymmetricCipher Cipher)
		{
			switch (LocalName)
			{
				case "aes": Cipher = this.aes; return true;
				case "acp": Cipher = this.acp; return true;
				case "cha": Cipher = this.cha; return true;
			}

			Cipher = null;
			return false;
		}

		/// <summary>
		/// Response handler for E2E encrypted iq stanzas
		/// </summary>
		/// <param name="Sender">Sender of event</param>
		/// <param name="e">Event arguments</param>
		protected virtual async Task IqResult(object Sender, IqResultEventArgs e)
		{
			XmppClient Client = Sender as XmppClient;
			XmlElement E = e.FirstElement;
			object[] P = (object[])e.State;
			EventHandlerAsync<IqResultEventArgs> Callback = (EventHandlerAsync<IqResultEventArgs>)P[0];
			object State = P[1];

			if (!this.TryGetSymmetricCipher(E.LocalName, E.NamespaceURI, out IE2eSymmetricCipher Cipher))
				Cipher = null;

			if (!(Cipher is null))
			{
				Tuple<string, string> T = this.Decrypt(Client, e.Id, e.Response.GetAttribute("type"), e.From, e.To, E, Cipher);
				if (T is null)
				{
					Client.Error("Unable to decrypt or verify response.");
					return;
				}

				string Content = T.Item1;
				string EndpointReference = T.Item2;
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<iq xmlns=\"jabber:client\" id=\"");
				Xml.Append(e.Id);
				Xml.Append("\" from=\"");
				Xml.Append(XML.Encode(e.From));
				Xml.Append("\" to=\"");
				Xml.Append(XML.Encode(e.To));

				if (e.Ok)
					Xml.Append("\" type=\"result\">");
				else
					Xml.Append("\" type=\"error\">");

				Xml.Append(Content);
				Xml.Append("</iq>");

				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(Xml.ToString());

				IqResultEventArgs e2 = new IqResultEventArgs(this, EndpointReference, Cipher,
					Doc.DocumentElement, e.Id, e.To, e.From, e.Ok, State);
				await Callback.Raise(Sender, e2);
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
					e.State = State;
					await Callback.Raise(Sender, e);
				}
				else
				{
					await this.SynchronizeE2e(To, async (Sender2, e2) =>
					{
						if (e2.Ok)
						{
							await this.SendIq(Client, E2ETransmission, Id, To, Xml, Type, Callback, State,
								RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, true);
						}
						else
						{
							e.State = State;
							await Callback.Raise(Sender, e);
						}
					});
				};
			}
			else
			{
				e.State = State;
				await Callback.Raise(Sender, e);
			}
		}

		private bool IsForbidden(XmlElement E)
		{
			if (E is null)
				return false;

			XmlElement E2;

			foreach (XmlNode N in E.ChildNodes)
			{
				E2 = N as XmlElement;
				if (E2 is null)
					continue;

				if (E2.LocalName == "forbidden" && E2.NamespaceURI == XmppClient.NamespaceXmppStanzas)
					return true;
			}

			return false;
		}

		private string EmbedIq(IqEventArgs e, string Type, string Content)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<iq xmlns=\"jabber:client\" id=\"");
			Xml.Append(e.Id);
			Xml.Append("\" from=\"");
			Xml.Append(XML.Encode(e.From));
			Xml.Append("\" to=\"");
			Xml.Append(XML.Encode(e.To));
			Xml.Append("\" type=\"");
			Xml.Append(Type);
			Xml.Append("\">");
			Xml.Append(Content);
			Xml.Append("</iq>");

			return Xml.ToString();
		}

		private Task AesIqGetHandler(object Sender, IqEventArgs e)
		{
			return this.E2eIqGetHandler(Sender, e, this.aes);
		}

		private Task AcpIqGetHandler(object Sender, IqEventArgs e)
		{
			return this.E2eIqGetHandler(Sender, e, this.acp);
		}

		private Task ChaIqGetHandler(object Sender, IqEventArgs e)
		{
			return this.E2eIqGetHandler(Sender, e, this.cha);
		}

		private async Task E2eIqGetHandler(object Sender, IqEventArgs e, IE2eSymmetricCipher Cipher)
		{
			XmppClient Client = Sender as XmppClient;
			Tuple<string, string> T = this.Decrypt(Client, e.Id, e.IQ.GetAttribute("type"), e.From, e.To, e.Query, Cipher);
			if (T is null)
			{
				await e.IqError(new ForbiddenException("Unable to decrypt or verify message.", e.IQ));
				return;
			}

			string Content = T.Item1;
			string EndpointReference = T.Item2;

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(this.EmbedIq(e, "get", Content));

			IqEventArgs e2 = new IqEventArgs(Client, this, EndpointReference, Cipher, Doc.DocumentElement, e.Id, e.To, e.From);
			await Client.ProcessIqGet(e2);
		}

		private Task AesIqSetHandler(object Sender, IqEventArgs e)
		{
			return this.E2eIqSetHandler(Sender, e, this.aes);
		}

		private Task AcpIqSetHandler(object Sender, IqEventArgs e)
		{
			return this.E2eIqSetHandler(Sender, e, this.acp);
		}

		private Task ChaIqSetHandler(object Sender, IqEventArgs e)
		{
			return this.E2eIqSetHandler(Sender, e, this.cha);
		}

		private async Task E2eIqSetHandler(object Sender, IqEventArgs e, IE2eSymmetricCipher Cipher)
		{
			XmppClient Client = Sender as XmppClient;
			Tuple<string, string> T = this.Decrypt(Client, e.Id, e.IQ.GetAttribute("type"), e.From, e.To, e.Query, Cipher);
			if (T is null)
			{
				await e.IqError(new ForbiddenException("Unable to decrypt or verify message.", e.IQ));
				return;
			}

			string Content = T.Item1;
			string EndpointReference = T.Item2;

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(this.EmbedIq(e, "set", Content));

			IqEventArgs e2 = new IqEventArgs(Client, this, EndpointReference, Cipher, Doc.DocumentElement, e.Id, e.To, e.From);
			await Client.ProcessIqSet(e2);
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
		public Task SendMessage(XmppClient Client, E2ETransmission E2ETransmission, QoSLevel QoS,
			MessageType Type, string Id, string To, string CustomXml, string Body, string Subject,
			string Language, string ThreadId, string ParentThreadId, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback,
			object State)
		{
			return this.SendMessage(Client, E2ETransmission, QoS, Type, Id, To, CustomXml, Body, Subject,
				Language, ThreadId, ParentThreadId, DeliveryCallback, State, false);
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
		/// <param name="PkiSynchronized">If E2E parameters have been synchronized</param>
		private async Task SendMessage(XmppClient Client, E2ETransmission E2ETransmission, QoSLevel QoS,
			MessageType Type, string Id, string To, string CustomXml, string Body, string Subject,
			string Language, string ThreadId, string ParentThreadId, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback,
			object State, bool PkiSynchronized)
		{
			if (this.client is null)
				throw new ObjectDisposedException("Endpoint security object disposed.");

			if (string.IsNullOrEmpty(Id))
				Id = Client.NextId();

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<message");

			switch (Type)
			{
				case MessageType.Chat:
					Xml.Append(" type=\"chat\"");
					break;

				case MessageType.Error:
					Xml.Append(" type=\"error\"");
					break;

				case MessageType.GroupChat:
					Xml.Append(" type=\"groupchat\"");
					break;

				case MessageType.Headline:
					Xml.Append(" type=\"headline\"");
					break;
			}

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append(" xml:lang=\"");
				Xml.Append(XML.Encode(Language));
				Xml.Append('"');
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
					Xml.Append(" parent=\"");
					Xml.Append(XML.Encode(ParentThreadId));
					Xml.Append('"');
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

				await Client.SendMessage(QoS, MessageType.Normal, Id, To, MessageXml, string.Empty,
					string.Empty, string.Empty, string.Empty, string.Empty, DeliveryCallback, State);

				return;
			}
			else if (XmppClient.GetBareJID(To) == To)
			{
				RosterItem Item = Client.GetRosterItem(To);
				bool Found = false;

				if (!(Item is null))
				{
					foreach (PresenceEventArgs e in Item.Resources)
					{
						Encrypted.Clear();

						if (this.Encrypt(Client, Id, string.Empty, this.client.FullJID, e.From, MessageXml, Encrypted))
						{
							await Client.SendMessage(QoS, MessageType.Normal, Id, e.From, Encrypted.ToString(), string.Empty,
								string.Empty, string.Empty, string.Empty, string.Empty, DeliveryCallback, State);

							Found = true;
						}
					}
				}

				if (Found)
					return;
			}

			if (E2ETransmission == E2ETransmission.IgnoreIfNotE2E)
				return;

			if (!PkiSynchronized)
			{
				await this.SynchronizeE2e(To, async (Sender, e) =>
				{
					if (e.Ok)
					{
						await this.SendMessage(Client, E2ETransmission, QoS, Type, Id, To, CustomXml, Body, Subject,
							Language, ThreadId, ParentThreadId, DeliveryCallback, State, true);
					}
					else if (E2ETransmission == E2ETransmission.NormalIfNotE2E)
					{
						await Client.SendMessage(QoS, Type, Id, To, CustomXml, Body, Subject, Language,
							ThreadId, ParentThreadId, DeliveryCallback, State);
					}
					else if (!(DeliveryCallback is null))   // null Callbacks are common, and should not result in a warning in sniffers.
						await DeliveryCallback.Raise(Sender, new DeliveryEventArgs(State, false));

				}, State);
			}
			else if (E2ETransmission == E2ETransmission.NormalIfNotE2E)
			{
				await Client.SendMessage(QoS, Type, Id, To, CustomXml, Body, Subject, Language,
					ThreadId, ParentThreadId, DeliveryCallback, State);
			}
			else
				throw new InvalidOperationException("End-to-End Encryption not available between " + Client.FullJID + " and " + To + ".");
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
		public Task<uint> SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
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
		public Task<uint> SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries)
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
		public Task<uint> SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
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
		public Task<uint> SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
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
		public Task<uint> SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries)
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
		public Task<uint> SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State, RetryTimeout,
				NrRetries, DropOff, MaxRetryTimeout, false);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza.</returns>
		public Task<uint> SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "get", Callback, State, Client.DefaultRetryTimeout,
				Client.DefaultNrRetries, Client.DefaultDropOff, Client.DefaultMaxRetryTimeout, false, DeliveryCallback);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza.</returns>
		public Task<uint> SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, false,
				RetryTimeout, false, DeliveryCallback);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza.</returns>
		public Task<uint> SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "get", Callback, State, RetryTimeout,
				NrRetries, DropOff, MaxRetryTimeout, false, DeliveryCallback);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza.</returns>
		public Task<uint> SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State,
				Client.DefaultRetryTimeout, Client.DefaultNrRetries, Client.DefaultDropOff,
				Client.DefaultMaxRetryTimeout, false, DeliveryCallback);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza.</returns>
		public Task<uint> SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State, RetryTimeout,
				NrRetries, false, RetryTimeout, false, DeliveryCallback);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza.</returns>
		public Task<uint> SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml,
			EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Client, E2ETransmission, null, To, Xml, "set", Callback, State, RetryTimeout,
				NrRetries, DropOff, MaxRetryTimeout, false, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ Result stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		public Task SendIqResult(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml)
		{
			return this.SendIq(Client, E2ETransmission, Id, To, Xml, "result", null, null, 0, 0, false, 0, false);
		}

		/// <summary>
		/// Sends an IQ Error stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		public Task SendIqError(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml)
		{
			return this.SendIq(Client, E2ETransmission, Id, To, Xml, "error", null, null, 0, 0, false, 0, false);
		}

		/// <summary>
		/// Sends an IQ Error stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="Id">Id attribute</param>
		/// <param name="To">To attribute</param>
		/// <param name="ex">Exception object</param>
		public Task SendIqError(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To,
			Exception ex)
		{
			return this.SendIqError(Client, E2ETransmission, Id, To, Client.ExceptionToXmppXml(ex));
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
		/// <exception cref="ObjectDisposedException">If endpoint security object has been disposed.</exception>
		/// <exception cref="InvalidOperationException">If E2E transmission is asserted, but no E2E-encrypted channel could be established.</exception>
		protected Task<uint> SendIq(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml,
			string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout, bool PkiSynchronized)
		{
			return this.SendIq(Client, E2ETransmission, Id, To, Xml, Type, Callback, State, RetryTimeout, NrRetries, DropOff,
				MaxRetryTimeout, PkiSynchronized, null);
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
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza, if none provided in <paramref name="Id"/>.</returns>
		/// <exception cref="ObjectDisposedException">If endpoint security object has been disposed.</exception>
		/// <exception cref="InvalidOperationException">If E2E transmission is asserted, but no E2E-encrypted channel could be established.</exception>
		protected async Task<uint> SendIq(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml,
			string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries, bool DropOff,
			int MaxRetryTimeout, bool PkiSynchronized, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			if (this.client is null)
				throw new ObjectDisposedException("Endpoint security object disposed.");

			if (string.IsNullOrEmpty(Id))
				Id = Client.NextId();

			StringBuilder Encrypted = new StringBuilder();

			if (this.Encrypt(Client, Id, Type, this.client.FullJID, To, Xml, Encrypted))
			{
				string XmlEnc = Encrypted.ToString();

				return await Client.SendIq(Id, To, XmlEnc, Type, this.IqResult,
					new object[] { Callback, State, E2ETransmission, Id, To, Xml, Type, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, PkiSynchronized },
					RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, DeliveryCallback);
			}

			if (E2ETransmission == E2ETransmission.IgnoreIfNotE2E)
			{
				if (uint.TryParse(Id, out uint SeqNr))
					return SeqNr;
				else
					return 0;
			}

			if (!PkiSynchronized)
			{
				if (!uint.TryParse(Id, out uint SeqNr))
				{
					Id = Client.NextId();
					SeqNr = uint.Parse(Id);
				}

				await this.SynchronizeE2e(To, async (Sender, e) =>
				{
					if (e.Ok)
					{
						await this.SendIq(Client, E2ETransmission, Id, To, Xml, Type, Callback, State,
							RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, true, DeliveryCallback);
					}
					else if (E2ETransmission == E2ETransmission.NormalIfNotE2E)
					{
						await Client.SendIq(Id, To, Xml, Type, Callback, State, RetryTimeout,
							NrRetries, DropOff, MaxRetryTimeout, DeliveryCallback);
					}
					else
					{
						if (!(DeliveryCallback is null))
							await DeliveryCallback.Raise(Sender, new DeliveryEventArgs(Sender, true));

						await Callback.Raise(Sender, e);
					}
				}, State);

				return SeqNr;
			}
			else if (E2ETransmission == E2ETransmission.NormalIfNotE2E)
			{
				return await Client.SendIq(Id, To, Xml, Type, Callback, State, RetryTimeout,
					NrRetries, DropOff, MaxRetryTimeout, DeliveryCallback);
			}
			else
				throw new InvalidOperationException("End-to-End Encryption not available between " + Client.FullJID + " and " + To + ".");
		}

		/// <summary>
		/// Sends an IQ Get stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public XmlElement IqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, int Timeout)
		{
			Task<XmlElement> Result = this.IqGetAsync(Client, E2ETransmission, To, Xml);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Sends an IQ Get stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<XmlElement> IqGetAsync(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.SendIqGet(Client, E2ETransmission, To, Xml, (Sender, e) =>
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
		/// Sends an IQ Set stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public XmlElement IqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, int Timeout)
		{
			Task<XmlElement> Result = this.IqSetAsync(Client, E2ETransmission, To, Xml);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Sends an IQ Set stanza
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="E2ETransmission">End-to-end Encryption options</param>
		/// <param name="To">To attribute</param>
		/// <param name="Xml">Payload XML</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<XmlElement> IqSetAsync(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.SendIqSet(Client, E2ETransmission, To, Xml, (Sender, e) =>
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
		/// Appends E2E information to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public void AppendE2eInfo(StringBuilder Xml)
		{
			lock (this.synchObject)
			{
				Xml.Append("<e2e xmlns=\"");
				Xml.Append(IoTHarmonizationE2ECurrent);
				Xml.Append("\">");

				foreach (IE2eEndpoint E2e in this.Keys)
					E2e.ToXml(Xml, IoTHarmonizationE2ECurrent);

				Xml.Append("</e2e>");
			}
		}

		/// <summary>
		/// Synchronizes End-to-End Encryption and Peer-to-Peer connectivity parameters with a remote entity.
		/// </summary>
		/// <param name="FullJID">Full JID of remote entity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		public Task SynchronizeE2e(string FullJID, EventHandlerAsync<IqResultEventArgs> Callback)
		{
			return this.SynchronizeE2e(FullJID, Callback, null);
		}

		/// <summary>
		/// Synchronizes End-to-End Encryption and Peer-to-Peer connectivity parameters with a remote entity.
		/// </summary>
		/// <param name="FullJID">Full JID of remote entity.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task SynchronizeE2e(string FullJID, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			LinkedList<SynchRec> CallbackList;

			lock (this.synchronizationTasks)
			{
				SynchRec Rec = new SynchRec()
				{
					Callback = Callback,
					State = State
				};

				if (this.synchronizationTasks.TryGetValue(FullJID, out CallbackList))
				{
					CallbackList.AddLast(Rec);
					return;
				}

				CallbackList = new LinkedList<SynchRec>();
				CallbackList.AddLast(Rec);

				this.synchronizationTasks[FullJID] = CallbackList;
			}

			await this.client.SendIqSet(FullJID, this.GetE2eXml(), async (Sender, e) =>
			{
				lock (this.synchronizationTasks)
				{
					if (!this.synchronizationTasks.TryGetValue(FullJID, out CallbackList))
						return;

					this.synchronizationTasks.Remove(FullJID);
				}

				if (e.Ok && !(e.FirstElement is null))
					await this.ParseE2e(e.FirstElement, FullJID);

				foreach (SynchRec Rec in CallbackList)
				{
					e.State = Rec.State;
					await Rec.Callback.Raise(Sender, e);
				}
			}, State);
		}

		private readonly Dictionary<string, LinkedList<SynchRec>> synchronizationTasks = new Dictionary<string, LinkedList<SynchRec>>();

		private class SynchRec
		{
			public EventHandlerAsync<IqResultEventArgs> Callback;
			public object State;
		}

		private string GetE2eXml()
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<synchE2e xmlns=\"");
			Xml.Append(IoTHarmonizationE2ECurrent);
			Xml.Append("\">");

			this.AppendE2eInfo(Xml);
			this.serverlessMessaging?.AppendP2pInfo(Xml);

			Xml.Append("</synchE2e>");

			return Xml.ToString();
		}

		private async Task ParseE2e(XmlElement E, string RemoteFullJID)
		{
			XmlElement E2E = null;
			XmlElement P2P = null;

			if (!(E is null) && E.LocalName == "synchE2e")
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2)
					{
						switch (E2.LocalName)
						{
							case "e2e":
								if (Array.IndexOf(NamespacesIoTHarmonizationE2E, E.NamespaceURI) >= 0)
									E2E = E2;
								break;

							case "p2p":
								if (Array.IndexOf(NamespacesIoTHarmonizationP2P, E.NamespaceURI) >= 0)
									P2P = E2;
								break;
						}
					}
				}
			}

			bool HasE2E = this.AddPeerPkiInfo(RemoteFullJID, E2E);
			bool HasP2P = !(this.serverlessMessaging is null) && await this.serverlessMessaging.AddPeerAddressInfo(RemoteFullJID, P2P);

			await this.PeerUpdated.Raise(this, new PeerSynchronizedEventArgs(RemoteFullJID, HasE2E, HasP2P));
		}

		private async Task SynchE2eHandler(object Sender, IqEventArgs e)
		{
			RosterItem Item;

			if (e.FromBareJid != this.client.BareJID &&
				((Item = this.client.GetRosterItem(e.FromBareJid)) is null ||
				Item.State == SubscriptionState.None ||
				Item.State == SubscriptionState.Remove ||
				Item.State == SubscriptionState.Unknown))
			{
				throw new ForbiddenException("Access denied. Unable to synchronize E2EE.", e.IQ);
			}

			await this.ParseE2e(e.Query, e.From);
			await e.IqResult(this.GetE2eXml());
		}

		private async Task Client_OnPresence(object Sender, PresenceEventArgs e)
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
							switch (E.LocalName)
							{
								case "e2e":
									if (Array.IndexOf(NamespacesIoTHarmonizationE2E, E.NamespaceURI) >= 0)
										E2E = E;
									break;

								case "p2p":
									if (Array.IndexOf(NamespacesIoTHarmonizationP2P, E.NamespaceURI) >= 0)
										P2P = E;
									break;
							}
						}
					}

					bool HasE2E = this.AddPeerPkiInfo(e.From, E2E);
					bool HasP2P = !(this.serverlessMessaging is null) && await this.serverlessMessaging.AddPeerAddressInfo(e.From, P2P);

					await this.PeerAvailable.Raise(this, new AvailableEventArgs(e, HasE2E, HasP2P));
					break;

				case PresenceType.Unavailable:
					await this.PeerUnavailable.Raise(this, e);
					break;
			}
		}

		/// <summary>
		/// Event raised whenever a peer has become available.
		/// </summary>
		public event EventHandlerAsync<AvailableEventArgs> PeerAvailable = null;

		/// <summary>
		/// Event raised whenever a peer has become unavailable.
		/// </summary>
		public event EventHandlerAsync<PresenceEventArgs> PeerUnavailable = null;

		/// <summary>
		/// Event raised whenever information about a peer has been updated.
		/// </summary>
		public event EventHandlerAsync<PeerSynchronizedEventArgs> PeerUpdated = null;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.instanceId.ToString();
		}

	}
}
