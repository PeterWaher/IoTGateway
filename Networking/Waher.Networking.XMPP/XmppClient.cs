﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Certificates;
#else
using System.Security.Cryptography.X509Certificates;
#endif
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Authentication;
using Waher.Networking.XMPP.AuthenticationErrors;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Networking.XMPP.StreamErrors;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Networking.XMPP.SoftwareVersion;
using Waher.Networking.XMPP.Search;
using Waher.Runtime.Inventory;
using Waher.Security;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Manages an XMPP client connection. Implements XMPP, as defined in
	/// https://tools.ietf.org/html/rfc6120
	/// https://tools.ietf.org/html/rfc6121
	/// https://tools.ietf.org/html/rfc6122
	/// 
	/// Extensions supported directly by client object:
	/// 
	/// XEP-0030: Service Discovery: http://xmpp.org/extensions/xep-0030.html
	/// XEP-0055: Jabber Search: http://xmpp.org/extensions/xep-0055.html
	/// XEP-0077: In-band Registration: http://xmpp.org/extensions/xep-0077.html
	/// XEP-0092: Software Version: http://xmpp.org/extensions/xep-0092.html
	/// XEP-0115: Entity Capabilities: http://xmpp.org/extensions/xep-0115.html
	/// XEP-0124: Bidirectional-streams Over Synchronous HTTP (BOSH): https://xmpp.org/extensions/xep-0124.html
	/// XEP-0128: Service Discovery Extensions: http://xmpp.org/extensions/xep-0128.html
	/// XEP-0199: XMPP Ping: http://xmpp.org/extensions/xep-0199.html
	/// XEP-0206: XMPP Over BOSH: https://xmpp.org/extensions/xep-0206.html
	/// 
	/// Quality of Service: http://xmpp.org/extensions/inbox/qos.html
	/// </summary>
	public class XmppClient : CommunicationLayer, IDisposableAsync, IHostReference
	{
		/// <summary>
		/// http://etherx.jabber.org/streams
		/// </summary>
		public const string NamespaceStream = "http://etherx.jabber.org/streams";

		/// <summary>
		/// jabber:client
		/// </summary>
		public const string NamespaceClient = "jabber:client";

		/// <summary>
		/// urn:ietf:params:xml:ns:xmpp-streams
		/// </summary>
		public const string NamespaceXmppStreams = "urn:ietf:params:xml:ns:xmpp-streams";

		/// <summary>
		/// urn:ietf:params:xml:ns:xmpp-stanzas
		/// </summary>
		public const string NamespaceXmppStanzas = "urn:ietf:params:xml:ns:xmpp-stanzas";

		/// <summary>
		/// urn:ietf:params:xml:ns:xmpp-sasl
		/// </summary>
		public const string NamespaceXmppSasl = "urn:ietf:params:xml:ns:xmpp-sasl";

		/// <summary>
		/// jabber:iq:register
		/// </summary>
		public const string NamespaceRegister = "jabber:iq:register";

		/// <summary>
		/// jabber:x:data
		/// </summary>
		public const string NamespaceData = "jabber:x:data";

		/// <summary>
		/// http://jabber.org/protocol/xdata-validate
		/// </summary>
		public const string NamespaceDataValidate = "http://jabber.org/protocol/xdata-validate";

		/// <summary>
		/// http://jabber.org/protocol/xdata-layout
		/// </summary>
		public const string NamespaceDataLayout = "http://jabber.org/protocol/xdata-layout";

		/// <summary>
		/// jabber:iq:roster
		/// </summary>
		public const string NamespaceRoster = "jabber:iq:roster";

		/// <summary>
		/// urn:xmpp:xdata:dynamic
		/// </summary>
		public const string NamespaceDynamicForms = "urn:xmpp:xdata:dynamic";

		/// <summary>
		/// http://jabber.org/protocol/disco#info
		/// </summary>
		public const string NamespaceServiceDiscoveryInfo = "http://jabber.org/protocol/disco#info";

		/// <summary>
		/// http://jabber.org/protocol/disco#items
		/// </summary>
		public const string NamespaceServiceDiscoveryItems = "http://jabber.org/protocol/disco#items";

		/// <summary>
		/// jabber:iq:version
		/// </summary>
		public const string NamespaceSoftwareVersion = "jabber:iq:version";

		/// <summary>
		/// jabber:iq:search
		/// </summary>
		public const string NamespaceSearch = "jabber:iq:search";

		/// <summary>
		/// urn:ieee:iot:qos:1.0
		/// </summary>
		public const string NamespaceQualityOfServiceIeeeV1 = "urn:ieee:iot:qos:1.0";

		/// <summary>
		/// urn:nf:iot:qos:1.0
		/// </summary>
		public const string NamespaceQualityOfServiceNeuroFoundationV1 = "urn:nf:iot:qos:1.0";

		/// <summary>
		/// Current namespace for Quality of Service.
		/// </summary>
		public const string NamespaceQualityOfServiceCurrent = NamespaceQualityOfServiceNeuroFoundationV1;

		/// <summary>
		/// urn:xmpp:ping
		/// </summary>
		public const string NamespacePing = "urn:xmpp:ping";

		/// <summary>
		/// http://jabber.org/protocol/caps
		/// </summary>
		public const string NamespaceEntityCapabilities = "http://jabber.org/protocol/caps";

		/// <summary>
		/// urn:xmpp:receipts
		/// </summary>
		public const string NamespaceMessageDeliveryReceipts = "urn:xmpp:receipts";

		/// <summary>
		/// jabber:iq:private (XEP-0049)
		/// </summary>
		public const string NamespacePrivateXmlStorage = "jabber:iq:private";

		/// <summary>
		/// http://waher.se/Schema/QL.xsd
		/// </summary>
		public const string NamespaceQuickLogin = "http://waher.se/Schema/QL.xsd";

		/// <summary>
		/// http://waher.se/Schema/AlternativeNames.xsd
		/// </summary>
		public const string AlternativesNamespace = "http://waher.se/Schema/Alternatives.xsd";

		/// <summary>
		/// Regular expression for Full JIDs
		/// </summary>
		public static readonly Regex FullJidRegEx = new Regex("^(?:([^@/<>'\\\"\\s]+)@)([^@/<>'\\\"\\s]+)(?:/([^<>'\\\"\\s]*))?$", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Regular expression for Bare JIDs
		/// </summary>
		public static readonly Regex BareJidRegEx = new Regex("^(?:([^@/<>'\\\"\\s]+)@)([^@/<>'\\\"\\s]+)$", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Regular expression for Domain JIDs
		/// </summary>
		public static readonly Regex DomainJidRegEx = new Regex("^(?:([^@/<>'\\\"\\s]+))$", RegexOptions.Singleline | RegexOptions.Compiled);

		private readonly static RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private static Type[] alternativeBindingMechanisms = null;

		private const int KeepAliveTimeSeconds = 30;
		private const int MaxFragmentSize = 40000000;

		private readonly Dictionary<string, bool> authenticationMechanisms = new Dictionary<string, bool>();
		private readonly Dictionary<string, bool> compressionMethods = new Dictionary<string, bool>();
		private readonly Dictionary<uint, PendingRequest> pendingRequestsBySeqNr = new Dictionary<uint, PendingRequest>();
		private readonly SortedDictionary<DateTime, PendingRequest> pendingRequestsByTimeout = new SortedDictionary<DateTime, PendingRequest>();
		private readonly Dictionary<string, EventHandlerAsync<IqEventArgs>> iqGetHandlers = new Dictionary<string, EventHandlerAsync<IqEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<IqEventArgs>> iqSetHandlers = new Dictionary<string, EventHandlerAsync<IqEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<MessageEventArgs>> messageHandlers = new Dictionary<string, EventHandlerAsync<MessageEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<MessageFormEventArgs>> messageFormHandlers = new Dictionary<string, EventHandlerAsync<MessageFormEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<PresenceEventArgs>> presenceHandlers = new Dictionary<string, EventHandlerAsync<PresenceEventArgs>>();
		private readonly Dictionary<string, MessageEventArgs> receivedMessages = new Dictionary<string, MessageEventArgs>();
		private readonly SortedDictionary<string, bool> clientFeatures = new SortedDictionary<string, bool>();
		private ServiceDiscoveryEventArgs serverFeatures = null;
		private ServiceItemsDiscoveryEventArgs serverComponents = null;
		private readonly SortedDictionary<string, DataForm> extendedServiceDiscoveryInformation = new SortedDictionary<string, DataForm>();
		private readonly Dictionary<string, RosterItem> roster = new Dictionary<string, RosterItem>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Dictionary<string, PresenceEventArgs> subscriptionRequests = new Dictionary<string, PresenceEventArgs>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Dictionary<string, int> pendingAssuredMessagesPerSource = new Dictionary<string, int>();
		private readonly Dictionary<string, object> tags = new Dictionary<string, object>();
		private readonly List<IXmppExtension> extensions = new List<IXmppExtension>();
		private readonly Dictionary<string, string> services = new Dictionary<string, string>();
		private readonly IqResponses responses = new IqResponses(TimeSpan.FromMinutes(1));
		private AuthenticationMethod authenticationMethod = null;
#if !WINDOWS_UWP
		private readonly X509Certificate clientCertificate = null;
#endif
		private TextTcpClient client;
		private Timer secondTimer = null;
		private DateTime nextPing = DateTime.MaxValue;
		private readonly UTF8Encoding encoding = new UTF8Encoding(false, false);
		private readonly StringBuilder fragment = new StringBuilder();
		private int fragmentLength = 0;
		private XmppState state;
		private readonly Random gen = new Random();
		private readonly object synchObject = new object();
		private Availability currentAvailability = Availability.Online;
		private KeyValuePair<string, string>[] customPresenceStatus = Array.Empty<KeyValuePair<string, string>>();
		private ITextTransportLayer textTransportLayer = null;
		private HashFunction entityHashFunction = HashFunction.SHA256;
		private string entityNode = "https://github.com/PeterWaher/IoTGateway";
		private string clientName;
		private string clientVersion;
		private string clientOS;
		private string host;
		private readonly string language;
		private string domain;
		private string bareJid;
		private string fullJid;
		private string resource = string.Empty;
		private string userName;
		private string password;
		private string passwordHash;
		private string passwordHashMethod;
		private string streamId;
		private string streamHeader;
		private string streamFooter;
		private string formSignatureKey;
		private string formSignatureSecret;
		private string entityCapabilitiesVersion = null;
		private double version;
		private uint seqnr = 0;
		private readonly int port;
		private int keepAliveSeconds = KeepAliveTimeSeconds;
		private int inputState = 0;
		private int inputDepth = 0;
		private int defaultRetryTimeout = 15000;
		private int defaultNrRetries = 0;
		private int defaultMaxRetryTimeout = int.MaxValue;
		private int maxAssuredMessagesPendingFromSource = 5;
		private int maxAssuredMessagesPendingTotal = 100;
		private int nrAssuredMessagesPending = 0;
		private bool defaultDropOff = true;
		private bool trustServer = false;
		private bool canRegister = false;
		private bool createSession = false;
		private bool hasRegistered = false;
		private bool hasRoster = false;
		private bool presenceSent = false;
		private bool requestRosterOnStartup = true;
		private bool allowedToRegister = false;
		private bool allowCramMD5 = true;
		private bool allowDigestMD5 = true;
		private bool allowScramSHA1 = true;
		private bool allowScramSHA256 = true;
		private bool allowPlain = false;
		private bool allowQuickLogin = false;
		private readonly bool sendHeartbeats = true;
		private bool supportsPing = true;
		private bool pingResponse = true;
		private bool allowEncryption = true;
		private bool sendFromAddress = false;
		private bool? checkConnection = null;
		private bool openBracketReceived = false;
		private bool monitorContactResourcesAlive = true;
		private bool upgradeToTls = false;
		private bool legacyTls = false;
		private bool performingQuickLogin = false;
		private bool disposed = false;

#if WINDOWS_UWP
		/// <summary>
		/// Manages an XMPP client connection over a traditional binary socket connection. 
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(string Host, int Port, string UserName, string Password, string Language, Assembly AppAssembly,
			params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.host = this.domain = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = Password;
			this.passwordHash = string.Empty;
			this.passwordHashMethod = string.Empty;
			this.language = Language;
			this.state = XmppState.Offline;

			this.Init(AppAssembly);
		}

		/// <summary>
		/// Manages an XMPP client connection over a traditional binary socket connection. 
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="PasswordHash">Password hash.</param>
		/// <param name="PasswordHashMethod">Password hash method.</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(string Host, int Port, string UserName, string PasswordHash, string PasswordHashMethod, string Language,
			Assembly AppAssembly, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.host = this.domain = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = string.IsNullOrEmpty(PasswordHashMethod) ? PasswordHash : string.Empty;
			this.passwordHash = string.IsNullOrEmpty(PasswordHashMethod) ? string.Empty : PasswordHash;
			this.passwordHashMethod = PasswordHashMethod;
			this.language = Language;
			this.state = XmppState.Offline;
			this.Init(AppAssembly);
		}

		/// <summary>
		/// Manages an XMPP client connection. Connection information is defined in
		/// <paramref name="Credentials"/>.
		/// </summary>
		/// <param name="Credentials">Client credentials.</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(XmppCredentials Credentials, string Language, Assembly AppAssembly, params ISniffer[] Sniffers)
			: base(true, Sniffers)
#else
		/// <summary>
		/// Manages an XMPP client connection over a traditional binary socket connection. 
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(string Host, int Port, string UserName, string Password, string Language, Assembly AppAssembly,
			params ISniffer[] Sniffers)
			: this(Host, Port, UserName, Password, Language, AppAssembly, (X509Certificate)null, Sniffers)
		{
		}

		/// <summary>
		/// Manages an XMPP client connection over a traditional binary socket connection. 
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="ClientCertificate">Optional client certificate.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(string Host, int Port, string UserName, string Password, string Language, Assembly AppAssembly,
			X509Certificate ClientCertificate, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.host = this.domain = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = Password;
			this.passwordHash = string.Empty;
			this.passwordHashMethod = string.Empty;
			this.language = Language;
			this.state = XmppState.Offline;
			this.clientCertificate = ClientCertificate;

			this.Init(AppAssembly);
		}

		/// <summary>
		/// Manages an XMPP client connection over a traditional binary socket connection. 
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="PasswordHash">Password hash.</param>
		/// <param name="PasswordHashMethod">Password hash method.</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(string Host, int Port, string UserName, string PasswordHash, string PasswordHashMethod, string Language,
			Assembly AppAssembly, params ISniffer[] Sniffers)
			: this(Host, Port, UserName, PasswordHash, PasswordHashMethod, Language, AppAssembly, null, Sniffers)
		{
		}

		/// <summary>
		/// Manages an XMPP client connection over a traditional binary socket connection. 
		/// </summary>
		/// <param name="Host">Host name or IP address of XMPP server.</param>
		/// <param name="Port">Port to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="PasswordHash">Password hash.</param>
		/// <param name="PasswordHashMethod">Password hash method.</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="ClientCertificate">Optional client certificate.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(string Host, int Port, string UserName, string PasswordHash, string PasswordHashMethod, string Language, Assembly AppAssembly,
			X509Certificate ClientCertificate, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.host = this.domain = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = string.IsNullOrEmpty(PasswordHashMethod) ? PasswordHash : string.Empty;
			this.passwordHash = string.IsNullOrEmpty(PasswordHashMethod) ? string.Empty : PasswordHash;
			this.passwordHashMethod = PasswordHashMethod;
			this.language = Language;
			this.state = XmppState.Offline;
			this.clientCertificate = ClientCertificate;
			this.Init(AppAssembly);
		}

		/// <summary>
		/// Manages an XMPP client connection. Connection information is defined in
		/// <paramref name="Credentials"/>.
		/// </summary>
		/// <param name="Credentials">Client credentials.</param>
		/// <param name="Language">Language Code, according to RFC 5646.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public XmppClient(XmppCredentials Credentials, string Language, Assembly AppAssembly, params ISniffer[] Sniffers)
			: base(true, Sniffers)
#endif
		{
			this.host = this.domain = Credentials.Host;
			this.port = Credentials.Port;
			this.userName = Credentials.Account;

			if (!string.IsNullOrEmpty(Credentials.UriEndpoint))
			{
				Uri URI = new Uri(Credentials.UriEndpoint);

				if (alternativeBindingMechanisms is null)
				{
					alternativeBindingMechanisms = Types.GetTypesImplementingInterface(typeof(IAlternativeTransport));
					Types.OnInvalidated += this.Types_OnInvalidated;
				}

				IAlternativeTransport Best = Types.FindBest<IAlternativeTransport, Uri>(URI, alternativeBindingMechanisms);

				if (!(Best is null))
				{
					IAlternativeTransport AlternativeTransport = Best.Instantiate(URI, this, new XmppBindingInterface(this));
					this.textTransportLayer = AlternativeTransport;
					this.textTransportLayer.OnReceived += this.TextTransportLayer_OnReceived_NoSniff;
					this.sendHeartbeats = !AlternativeTransport.HandlesHeartbeats;
				}
				else
					throw new ArgumentException("No alternative binding mechanism found that servers the endpoint URI provided.", nameof(Credentials));
			}

			if (string.IsNullOrEmpty(Credentials.PasswordType))
			{
				this.password = Credentials.Password;
				this.passwordHash = string.Empty;
				this.passwordHashMethod = string.Empty;
			}
			else
			{
				this.password = string.Empty;
				this.passwordHash = Credentials.Password;
				this.passwordHashMethod = Credentials.PasswordType;
			}

			this.language = Language;
			this.state = XmppState.Offline;
#if !WINDOWS_UWP
			this.clientCertificate = Credentials.ClientCertificate;
#endif
			this.Init(AppAssembly);

			this.allowCramMD5 = Credentials.AllowCramMD5;
			this.allowDigestMD5 = Credentials.AllowDigestMD5;
			this.allowPlain = Credentials.AllowPlain;
			this.allowScramSHA1 = Credentials.AllowScramSHA1;
			this.allowScramSHA256 = Credentials.AllowScramSHA256;
			this.allowEncryption = Credentials.AllowEncryption;
			this.requestRosterOnStartup = Credentials.RequestRosterOnStartup;
			this.trustServer = Credentials.TrustServer;

			if (Credentials.AllowRegistration)
				this.AllowRegistration(Credentials.FormSignatureKey, Credentials.FormSignatureSecret);
		}

		private void Types_OnInvalidated(object Sender, EventArgs e)
		{
			alternativeBindingMechanisms = Types.GetTypesImplementingInterface(typeof(IAlternativeTransport));
		}

		private void Init(Assembly Assembly)
		{
			AssemblyName Name = Assembly.GetName();
			string Title = string.Empty;
			string Product = string.Empty;
			string AssemblyName = Name.Name;

			foreach (object Attribute in Assembly.GetCustomAttributes())
			{
				if (Attribute is AssemblyTitleAttribute AssemblyTitleAttribute)
					Title = AssemblyTitleAttribute.Title;
				else if (Attribute is AssemblyProductAttribute AssemblyProductAttribute)
					Product = AssemblyProductAttribute.Product;
			}

			if (!string.IsNullOrEmpty(Title))
				this.clientName = Title;
			else if (!string.IsNullOrEmpty(Product))
				this.clientName = Product;
			else
				this.clientName = AssemblyName;

			this.clientVersion = Name.Version.ToString();
			this.bareJid = this.fullJid = this.userName + "@" + this.Domain;

			/* Alternative for UWP:
			string DeviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
			string DeviceFamilyVersion = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion;

			ulong Version = ulong.Parse(DeviceFamilyVersion);
			ulong Major = (Version & 0xFFFF000000000000L) >> 48;
			ulong Minor = (Version & 0x0000FFFF00000000L) >> 32;
			ulong Build = (Version & 0x00000000FFFF0000L) >> 16;
			ulong Revision = (Version & 0x000000000000FFFFL);

			this.clientOS = DeviceFamily + " " + Major.ToString() + "." + Minor.ToString() + "." + Build.ToString() + "." + Revision.ToString();*/

#if NETFW
			this.clientOS = System.Environment.OSVersion.ToString();
#else
			this.clientOS = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
#endif

			this.RegisterDefaultHandlers();
		}

		/// <summary>
		/// Manages an XMPP client connection. Transport layer is implemented in
		/// <paramref name="TextTransporLayer"/>.
		/// </summary>
		/// <param name="TextTransporLayer">Text transport layer to send and receive XMPP fragments on. The transport layer
		/// MUST ALREADY be connected and at least the stream element processed, if applicable. The transport layer is responsible 
		/// for authenticating incoming requests. Text packets received MUST be complete XML fragments.</param>
		/// <param name="State">XMPP state.</param>
		/// <param name="StreamHeader">Stream header start tag.</param>
		/// <param name="StreamFooter">Stream footer end tag.</param>
		/// <param name="BareJid">Bare JID of connection.</param>
		/// <param name="AppAssembly">Application assembly.</param>
		/// <param name="Sniffers">Sniffers</param>
		public XmppClient(ITextTransportLayer TextTransporLayer, XmppState State, string StreamHeader, string StreamFooter, string BareJid,
			Assembly AppAssembly, params ISniffer[] Sniffers)
			: base(true, Sniffers)
		{
			this.textTransportLayer = TextTransporLayer;
			this.Init(AppAssembly);

			this.state = State;
			this.pingResponse = true;
			this.streamHeader = StreamHeader;
			this.streamFooter = StreamFooter;
			this.bareJid = this.fullJid = BareJid;
			this.ResetState(false, true);

			this.textTransportLayer.OnReceived += this.TextTransportLayer_OnReceived;
			this.textTransportLayer.OnSent += this.TextTransportLayer_OnSent;
		}

		private Task<bool> TextTransportLayer_OnSent(object _, string Packet)
		{
			this.TransmitText(Packet);
			return Task.FromResult(true);
		}

		private Task<bool> TextTransportLayer_OnReceived(object _, string Packet)
		{
			if (this.openBracketReceived)
			{
				this.openBracketReceived = false;
				this.ReceiveText("<" + Packet);
			}
			else if (Packet == "<")
				this.openBracketReceived = true;
			else
				this.ReceiveText(Packet);

			return this.ProcessFragment(Packet);
		}

		private async Task<bool> TextTransportLayer_OnReceived_NoSniff(object _, string Packet)
		{
			if (Packet.StartsWith("</"))
			{
				await this.ToError();
				return false;
			}
			else
				return await this.ProcessFragment(Packet);
		}

		/// <summary>
		/// Connects the client.
		/// </summary>
		public Task Connect()
		{
			return this.Connect(this.host);
		}

		/// <summary>
		/// Connects the client.
		/// </summary>
		/// <param name="Domain">Domain name, if different from the host name provided in the constructor.</param>
		public async Task Connect(string Domain)
		{
			try
			{
				if (this.disposed)
					throw new ObjectDisposedException("XMPP Client has been disposed.");

				await this.DisposeClient(false);

				this.domain = Domain;
				this.bareJid = this.fullJid = this.userName + "@" + Domain;

				if (!this.checkConnection.HasValue)
					this.checkConnection = true;

				this.openBracketReceived = false;

				await this.SetState(XmppState.Connecting);
				this.pingResponse = true;
				this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);
				this.serverFeatures = null;
				this.serverComponents = null;
				this.fragmentLength = 0;
				this.fragment.Clear();
				this.upgradeToTls = false;
				this.performingQuickLogin = false;

				lock (this.synchObject)
				{
					this.services.Clear();
				}

				if (this.textTransportLayer is null)
				{
					this.client = new TextTcpClient(this.encoding, true);
					this.client.OnReceived += this.OnReceived;
					this.client.OnSent += this.OnSent;
					this.client.OnError += this.Error;
					this.client.OnDisconnected += this.Client_OnDisconnected;
					this.client.OnPaused += this.Client_OnPaused;

					if (await this.client.ConnectAsync(this.host, this.port, this.legacyTls))
					{
						if (this.legacyTls)
						{
							await this.SetState(XmppState.StartingEncryption);
#if WINDOWS_UWP
							await this.client.UpgradeToTlsAsClient(SocketProtectionLevel.Tls12, this.trustServer);
#else
							await this.client.UpgradeToTlsAsClient(this.clientCertificate, Crypto.SecureTls, this.trustServer, "xmpp-client");
#endif
							this.upgradeToTls = false;
							this.client.Continue();
						}

						await this.SetState(XmppState.StreamNegotiation);

						await this.BeginWrite("<?xml version='1.0' encoding='utf-8'?><stream:stream to='" + XML.Encode(this.domain) + "' version='1.0' xml:lang='" +
							XML.Encode(this.language) + "' xmlns='" + NamespaceClient + "' xmlns:stream='" +
							NamespaceStream + "'>", null, null);
					}
					else
					{
						await this.ConnectionError(new Exception("Unable to connect to " + this.host + ":" + this.port.ToString()));
						return;
					}
				}
				else if (this.textTransportLayer is AlternativeTransport AlternativeTransport)
				{
					await this.SetState(XmppState.StreamNegotiation);
					AlternativeTransport.CreateSession();
				}

				this.ResetState(false, true);
			}
			catch (Exception ex)
			{
				await this.ConnectionError(ex);
			}
		}

		private async Task Client_OnDisconnected(object Sender, EventArgs e)
		{
			this.Information("Disconnected.");
			if (this.state != XmppState.Error)
				await this.SetState(XmppState.Offline);
		}

		private Task<bool> OnSent(object _, string Text)
		{
			this.TransmitText(Text);
			return Task.FromResult(true);
		}

		private Task<bool> OnReceived(object _, string Text)
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

			return this.ParseIncoming(Text);
		}

		private void RegisterDefaultHandlers()
		{
			this.RegisterIqSetHandler("query", NamespaceRoster, this.RosterPushHandler, true);
			this.RegisterIqGetHandler("query", NamespaceServiceDiscoveryInfo, this.ServiceDiscoveryRequestHandler, true);
			this.RegisterIqGetHandler("query", NamespaceSoftwareVersion, this.SoftwareVersionRequestHandler, true);
			this.RegisterIqGetHandler("ping", NamespacePing, this.PingRequestHandler, true);

			#region Neuro-Foundation V1

			this.RegisterIqSetHandler("acknowledged", NamespaceQualityOfServiceNeuroFoundationV1, this.AcknowledgedQoSMessageHandler, true);
			this.RegisterIqSetHandler("assured", NamespaceQualityOfServiceNeuroFoundationV1, this.AssuredQoSMessageHandler, false);
			this.RegisterIqSetHandler("deliver", NamespaceQualityOfServiceNeuroFoundationV1, this.DeliverQoSMessageHandler, false);

			#endregion

			#region IEEE V1

			this.RegisterIqSetHandler("acknowledged", NamespaceQualityOfServiceIeeeV1, this.AcknowledgedQoSMessageHandler, true);
			this.RegisterIqSetHandler("assured", NamespaceQualityOfServiceIeeeV1, this.AssuredQoSMessageHandler, false);
			this.RegisterIqSetHandler("deliver", NamespaceQualityOfServiceIeeeV1, this.DeliverQoSMessageHandler, false);

			#endregion

			this.RegisterMessageHandler("updated", NamespaceDynamicForms, this.DynamicFormUpdatedHandler, true);

			this.clientFeatures[NamespaceMessageDeliveryReceipts] = true;
			this.clientFeatures["urn:xmpp:xdata:signature:oauth1"] = true;
			this.clientFeatures["http://jabber.org/protocols/xdata-validate"] = true;
			this.clientFeatures[NamespaceData] = true;
			this.clientFeatures[NamespaceEntityCapabilities] = true;

			this.entityCapabilitiesVersion = null;
		}

		private void ResetState(bool Authenticated, bool ExpectStream)
		{
			if (ExpectStream)
			{
				this.inputState = 0;
				this.inputDepth = 0;
				this.performingQuickLogin = false;
				this.canRegister = false;
				this.presenceSent = false;

				if (!Authenticated)
				{
					this.authenticationMethod = null;
					this.authenticationMechanisms.Clear();
				}

				this.compressionMethods.Clear();
			}
			else
			{
				this.inputState = 5;
				this.inputDepth = 1;
			}

			lock (this.synchObject)
			{
				this.pendingRequestsBySeqNr.Clear();
				this.pendingRequestsByTimeout.Clear();
			}

			this.responses.Clear();
		}

		internal async Task ConnectionError(Exception ex)
		{
			await this.SetState(XmppState.Error);

			await this.OnConnectionError.Raise(this, ex);
			await this.Error(this, ex);

			this.inputState = -1;
			await this.DisposeClient(false);
		}

		private async Task Error(object _, Exception Exception)
		{
			await this.SetState(XmppState.Error);

			Exception = Log.UnnestException(Exception);

			if (Exception is AggregateException ex)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					await this.Error(this, ex2);
			}
			else
			{
				this.Exception(Exception);
				await this.OnError.Raise(this, Exception);
			}
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
		/// Underlying text transport layer, if such was provided to create the XMPP client.
		/// </summary>
		public ITextTransportLayer TextTransportLayer => this.textTransportLayer;

		internal string StreamHeader
		{
			get => this.streamHeader;
			set => this.streamHeader = value;
		}

		internal string StreamFooter
		{
			get => this.streamFooter;
			set => this.streamFooter = value;
		}

		internal DateTime NextPing
		{
			get => this.nextPing;
			set => this.nextPing = value;
		}

		/// <summary>
		/// If server should be trusted, regardless if the operating system could validate its certificate or not.
		/// </summary>
		public bool TrustServer
		{
			get => this.trustServer;
			set => this.trustServer = value;
		}

		/// <summary>
		/// Legacy TLS means TLS negotiation is done directly after connection.
		/// By default, this is false, and TLS is negotiated using STARTTLS.
		/// </summary>
		public bool LegacyTls
		{
			get => this.legacyTls;
			set => this.legacyTls = value;
		}

		/// <summary>
		/// Certificate used by the server.
		/// </summary>
#if WINDOWS_UWP
		public Certificate ServerCertificate
#else
		public X509Certificate ServerCertificate
#endif
			=> this.client.RemoteCertificate;

		/// <summary>
		/// If the server certificate is valid.
		/// </summary>
		public bool ServerCertificateValid => this.client.RemoteCertificateValid;

		/// <summary>
		/// Name of the client in the XMPP network.
		/// </summary>
		public string ClientName => this.clientName;

		/// <summary>
		/// Version of the client in the XMPP network.
		/// </summary>
		public string ClientVersion => this.clientVersion;

		/// <summary>
		/// OS of the client in the XMPP network.
		/// </summary>
		public string ClientOS => this.clientOS;

		/// <summary>
		/// Language of the client in the XMPP network.
		/// </summary>
		public string Language => this.language;

		/// <summary>
		/// Monitors contact resources to see they are alive.
		/// </summary>
		public bool MonitorContactResourcesAlive
		{
			get => this.monitorContactResourcesAlive;
			set => this.monitorContactResourcesAlive = value;
		}

		/// <summary>
		/// Last availability set by the client when setting presence.
		/// </summary>
		public Availability LastSetPresenceAvailability => this.currentAvailability;

		/// <summary>
		/// Last custom status set by the client when setting presence.
		/// </summary>
		public KeyValuePair<string, string>[] LastSetPresenceCustomStatus => this.customPresenceStatus;

		/// <summary>
		/// If the connection should be regularly checked, and automatic reconnection attempts should be made.
		/// This feature is turned on by default, when connecting the client for the first time. If set to false,
		/// it must be set to true again, for ping and other connection checks to be made regularly.
		/// </summary>
		public bool? CheckConnection
		{
			get => this.checkConnection;
			set => this.checkConnection = value;
		}

		/// <summary>
		/// Current state of connection.
		/// </summary>
		public XmppState State => this.state;

		/// <summary>
		/// Sets the current state of the connection.
		/// </summary>
		/// <param name="NewState"></param>
		internal async Task SetState(XmppState NewState)
		{
			if (this.state != NewState)
			{
				this.state = NewState;
				this.Information("State changed to " + NewState.ToString());
				await this.RaiseOnStateChanged(NewState);

				if (NewState == XmppState.Offline || NewState == XmppState.Error)
				{
					RosterItem[] Roster = this.Roster;
					PresenceEventArgs[] Resources;
					List<PresenceEventArgs> ToUnavail = null;

					foreach (RosterItem Item in Roster)
					{
						Resources = Item.UnavailAllResources();
						if (Resources is null)
							continue;

						if (ToUnavail is null)
							ToUnavail = new List<PresenceEventArgs>();

						ToUnavail.AddRange(Resources);
					}

					if (!(ToUnavail is null))
					{
						foreach (PresenceEventArgs e in ToUnavail)
							await this.Unavail(e);
					}
				}
			}
		}

		private Task RaiseOnStateChanged(XmppState State)
		{
			return this.OnStateChanged.Raise(this, State);
		}

		internal Task Unavail(PresenceEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence from='");
			Xml.Append(XML.Encode(e.From));
			Xml.Append("' to='");
			Xml.Append(XML.Encode(this.fullJid));
			Xml.Append("' type='unavailable'/>");

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml.ToString());

			return this.ProcessPresence(new PresenceEventArgs(this, Doc.DocumentElement));
		}

		/// <summary>
		/// Event raised whenever the internal state of the connection changes.
		/// </summary>
		public event EventHandlerAsync<XmppState> OnStateChanged = null;

		/// <summary>
		/// Waits for one of the states in <paramref name="States"/> to occur.
		/// </summary>
		/// <param name="Timeout">Time to wait, in milliseconds.</param>
		/// <param name="States">States to wait for.</param>
		/// <returns>Index into <paramref name="States"/> corresponding to the
		/// first state match occurring. If -1 is returned, none of the states
		/// provided occurred within the given time.</returns>
		public async Task<int> WaitStateAsync(int Timeout, params XmppState[] States)
		{
			if (States is null || States.Length == 0)
				return -1;

			TaskCompletionSource<int> T = new TaskCompletionSource<int>();
			int Result;

			Task StateEventHandler(object Sender, XmppState NewState)
			{
				int i = Array.IndexOf(States, NewState);
				if (i >= 0)
					T.TrySetResult(i);

				return Task.CompletedTask;
			}

			this.OnStateChanged += StateEventHandler;
			try
			{
				Result = Array.IndexOf(States, this.state);
				if (Result < 0)
				{
					Task _ = Task.Delay(Timeout).ContinueWith((T2) => T.TrySetResult(-1));
					Result = await T.Task;
				}
			}
			finally
			{
				this.OnStateChanged -= StateEventHandler;
			}

			return Result;
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		[Obsolete("Use the DisposeAsync() or OfflineAndDisposeAsync() method.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Sends an offline presence, and then disposes the object by calling
		/// <see cref="DisposeAsync"/>.
		/// </summary>
		public Task OfflineAndDisposeAsync()
		{
			return this.OfflineAndDisposeAsync(true);
		}

		/// <summary>
		/// Sends an offline presence (if online), and then disposes the object by calling
		/// <see cref="DisposeAsync"/>.
		/// </summary>
		/// <param name="DisposeSniffers">If attached sniffers should be disposed.</param>
		public async Task OfflineAndDisposeAsync(bool DisposeSniffers)
		{
			if (this.state == XmppState.Connected)
			{
				TaskCompletionSource<bool> OfflineSent = new TaskCompletionSource<bool>();

				await this.SetPresence(Availability.Offline, (Sender, e) =>
				{
					OfflineSent.TrySetResult(true);
					return Task.CompletedTask;
				}, null);

				Task _ = Task.Delay(1000).ContinueWith((_2) =>
				{
					OfflineSent.TrySetResult(false);
					return Task.CompletedTask;
				});

				await OfflineSent.Task;
			}

			await this.RemoveRange(this.Sniffers, true);
			await this.DisposeAsync();
		}

		/// <summary>
		/// Closes the connection and disposes of all resources.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				if (this.checkConnection.HasValue && this.checkConnection.Value)
					this.checkConnection = null;

				if (this.state == XmppState.Connected || this.state == XmppState.FetchingRoster || this.state == XmppState.SettingPresence)
				{
					try
					{
						await this.BeginWrite(this.streamFooter, this.CleanUp, null);
					}
					catch (Exception)
					{
						await this.CleanUp(this, EventArgs.Empty);
					}
				}
				else
					await this.CleanUp(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// If the client has been disposed.
		/// </summary>
		public bool Disposed => this.disposed;

		/// <summary>
		/// Closes the connection the hard way. This might disrupt stream processing, but can simulate a lost connection. 
		/// To close the connection softly, call the <see cref="Dispose"/> method.
		/// 
		/// Note: After turning the connection hard-offline, you can reconnect to the server calling the <see cref="Reconnect()"/> method.
		/// </summary>
		public Task HardOffline()
		{
			return this.CleanUp(false);
		}

		private Task CleanUp(object Sender, EventArgs e)
		{
			return this.CleanUp(true);
		}

		private async Task CleanUp(bool RaiseEvent)
		{
			if (this.checkConnection.HasValue && this.checkConnection.Value)
				this.checkConnection = null;

			await this.SetState(XmppState.Offline);

			this.authenticationMechanisms?.Clear();
			this.compressionMethods?.Clear();

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

			await this.DisposeClient(RaiseEvent);

			this.responses.Dispose();

			ITextTransportLayer TTL;

			if (!((TTL = this.textTransportLayer) is null))
			{
				this.textTransportLayer = null;
				await TTL.DisposeAsync();
			}
		}

		private async Task DisposeClient(bool RaiseEvent)
		{
			this.client?.DisposeWhenDone();
			this.client = null;

			if (this.textTransportLayer is IAlternativeTransport AlternativeTransport)
				AlternativeTransport.CloseSession();

			if (RaiseEvent)
			{
				await this.OnDisposed.Raise(this, EventArgs.Empty, false);
				this.OnDisposed = null;
			}
		}

		/// <summary>
		/// Event raised when object is disposed.
		/// </summary>
		public event EventHandlerAsync OnDisposed = null;

		/// <summary>
		/// Reconnects a client after an error or if it's offline. Reconnecting, instead of creating a completely new connection,
		/// saves time. It binds to the same resource provided earlier, and avoids fetching the roster.
		/// </summary>
		public async Task Reconnect()
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(XmppClient), "XMPP Client has already been disposed.");

			if (!(this.textTransportLayer is null) && !(this.textTransportLayer is AlternativeTransport))
				throw new Exception("Reconnections must be made in the underlying transport layer.");
			else
			{
				string s = this.fragment.ToString();
				if (!string.IsNullOrEmpty(s))
					this.Warning("Input lost:\r\n\r\n" + s);

				await this.Connect(this.domain);
			}
		}

		/// <summary>
		/// Reconnects a client after an error or if it's offline. Reconnecting, instead of creating a completely new connection,
		/// saves time. It binds to the same resource provided earlier, and avoids fetching the roster.
		/// </summary>
		/// <param name="UserName">New user name.</param>
		/// <param name="Password">New password.</param>
		public Task Reconnect(string UserName, string Password)
		{
			if (this.disposed)
				throw new ObjectDisposedException("XMPP Client has already been disposed.");

			if (!(this.textTransportLayer is null))
				throw new Exception("Reconnections must be made in the underlying transport layer.");

			this.userName = UserName;
			this.password = Password;

			return this.Connect(this.domain);
		}

		/// <summary>
		/// Reconnects a client after an error or if it's offline. Reconnecting, instead of creating a completely new connection,
		/// saves time. It binds to the same resource provided earlier, and avoids fetching the roster.
		/// </summary>
		/// <param name="UserName">New user name.</param>
		/// <param name="PasswordHash">New password hash.</param>
		/// <param name="PasswordHashMethod">New password hash method.</param>
		public Task Reconnect(string UserName, string PasswordHash, string PasswordHashMethod)
		{
			if (this.disposed)
				throw new ObjectDisposedException("XMPP Client has already been disposed.");

			if (!(this.textTransportLayer is null))
				throw new Exception("Reconnections must be made in the underlying transport layer.");

			this.userName = UserName;
			this.password = string.IsNullOrEmpty(PasswordHashMethod) ? PasswordHash : string.Empty;
			this.passwordHash = string.IsNullOrEmpty(PasswordHashMethod) ? string.Empty : PasswordHash;
			this.passwordHashMethod = PasswordHashMethod;

			return this.Connect(this.domain);
		}

		private async Task BeginWrite(string Xml, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (string.IsNullOrEmpty(Xml))
				await Callback.Raise(this, new DeliveryEventArgs(State, true));
			else
			{
				if (this.textTransportLayer is null)
					this.client?.SendAsync(Xml, Callback, State);
				else
					await this.textTransportLayer.SendAsync(Xml, Callback, State);

				this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);
			}
		}

		private async Task<bool> ParseIncoming(string s)
		{
			bool Result = true;
			string Fragment;

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

							Fragment = this.fragment.ToString();
							this.fragment.Clear();
							this.fragmentLength = 0;

							await this.ProcessStream(Fragment);
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

							Fragment = this.fragment.ToString();
							this.fragment.Clear();
							this.fragmentLength = 0;

							await this.ProcessStream(Fragment);
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
									Fragment = this.fragment.ToString();
									this.fragment.Clear();
									this.fragmentLength = 0;

									if (!await this.ProcessFragment(Fragment))
										Result = false;
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
								Fragment = this.fragment.ToString();
								this.fragment.Clear();
								this.fragmentLength = 0;

								if (!await this.ProcessFragment(Fragment))
									Result = false;
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

			ITextTransportLayer TTL;

			if (!((TTL = this.textTransportLayer) is null))
			{
				this.textTransportLayer = null;
				await TTL.DisposeAsync();
			}

			await this.SetState(XmppState.Error);
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

				this.version = XML.Attribute(Stream, "version", 0.0);
				this.streamId = XML.Attribute(Stream, "id");
				this.domain = XML.Attribute(Stream, "from");
				this.bareJid = this.fullJid = this.userName + "@" + this.domain;

				if (this.version < 1.0)
					throw new XmppException("Version not supported.", Stream);

				await this.SetState(XmppState.StreamOpened);
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
											await this.SendIqResult(Id, From, ResponseXml);
										else
											await this.SendIqError(Id, From, ResponseXml);
									}
									else
									{
										IqEventArgs ie = new IqEventArgs(this, E, Id, To, From);
										if (await this.ValidateSender(E, From, ie.FromBareJid, ie, null))
											await this.ProcessIq(this.iqGetHandlers, ie);
										else
											await ie.IqError(new ForbiddenException("Access denied. Invalid sender.", E));
									}
									break;

								case "set":
									if (this.responses.TryGet(From, Id, out ResponseXml, out Ok))
									{
										if (Ok)
											await this.SendIqResult(Id, From, ResponseXml);
										else
											await this.SendIqError(Id, From, ResponseXml);
									}
									else
									{
										IqEventArgs ie = new IqEventArgs(this, E, Id, To, From);
										if (await this.ValidateSender(E, From, ie.FromBareJid, ie, null))
											await this.ProcessIq(this.iqSetHandlers, new IqEventArgs(this, E, Id, To, From));
										else
											await ie.IqError(new ForbiddenException("Access denied. Invalid sender.", E));
									}
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
							MessageEventArgs me = new MessageEventArgs(this, E);

							if (await this.ValidateSender(E, me.From, me.FromBareJID, null, me))
								this.ProcessMessage(me);
							break;

						case "presence":
							PresenceEventArgs pe = new PresenceEventArgs(this, E);

							if (!this.performingQuickLogin && this.state == XmppState.SettingPresence)
							{
								int i = pe.From.LastIndexOf('/');
								if (i > 0)
								{
									this.resource = pe.From.Substring(i + 1);
									this.state = XmppState.Connected;
									this.performingQuickLogin = false;
								}
							}

							await this.ProcessPresence(pe);
							break;

						case "features":
							if (E.FirstChild is null)
								await this.AdvanceUntilConnected();
							else
							{
								bool StartTls = false;
								bool Auth = false;
								bool Bind = false;
								bool QuickLogin = false;

								this.createSession = false;

								foreach (XmlNode N2 in E.ChildNodes)
								{
									switch (N2.LocalName)
									{
										case "ql":
											QuickLogin = true;
											this.authenticationMethod = null;
											break;

										case "starttls":
											StartTls = true;
											break;

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
											Auth = true;
											break;

										case "register":
											this.canRegister = true;
											break;

										case "bind":
											Bind = true;
											break;

										case "session":
											this.createSession = true;
											break;

										default:
											break;
									}
								}

								if (QuickLogin &&
									this.allowEncryption &&
									this.allowQuickLogin &&
									await this.StartQuickLogin())
								{
									this.upgradeToTls = true;
									this.performingQuickLogin = true;
									return false;
								}
								else if (StartTls && this.allowEncryption)
								{
									await this.BeginWrite("<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>", null, null);
									return true;
								}
								else if (Auth)
								{
									await this.StartAuthentication();
									return true;
								}
								else if (Bind)
								{
									await this.SetState(XmppState.Binding);
									if (string.IsNullOrEmpty(this.resource))
										await this.SendIqSet(string.Empty, "<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'/>", this.BindResult, null);
									else
									{
										await this.SendIqSet(string.Empty, "<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><resource>" +
											XML.Encode(this.resource) + "</resource></bind>", this.BindResult, null);
									}
									return true;
								}
								else if (this.createSession)
								{
									this.createSession = false;
									await this.SetState(XmppState.RequestingSession);
									await this.SendIqSet(string.Empty, "<session xmlns='urn:ietf:params:xml:ns:xmpp-session'/>", this.SessionResult, null);
									return true;
								}
								else if (this.authenticationMechanisms.Count > 0 &&
									(this.state == XmppState.Connecting || this.state == XmppState.StreamNegotiation ||
									this.state == XmppState.StreamOpened || this.state == XmppState.StartingEncryption))
								{
									await this.StartAuthentication();
									return true;
								}
							}
							break;

						case "proceed":
							this.upgradeToTls = true;
							return false;

						case "failure":
							if (!(this.authenticationMethod is null))
							{
								if (this.canRegister && !this.hasRegistered && this.allowedToRegister && !string.IsNullOrEmpty(this.password))
								{
									this.hasRegistered = true;
									await this.SendIqGet(string.Empty, "<query xmlns='" + NamespaceRegister + "'/>", this.RegistrationFormReceived, null);
									break;
								}
								else if (E.FirstChild is null)
									throw new XmppException("Unable to authenticate user.", E);
								else
								{
									if (!string.IsNullOrEmpty(this.password))
									{
										this.passwordHash = string.Empty;
										this.passwordHashMethod = string.Empty;
									}

									throw GetExceptionObject(E);
								}
							}
							else
							{
								if (E.FirstChild is null)
									throw new XmppException("Unable to start TLS negotiation.", E);
								else
									throw GetExceptionObject(E);
							}

						case "challenge":
							if (this.authenticationMethod is null)
								throw new XmppException("No authentication method selected.", E);
							else
							{
								if (this.State != XmppState.Authenticating)
									await this.SetState(XmppState.Authenticating);

								string Response = this.authenticationMethod.Challenge(E.InnerText, this);
								await this.BeginWrite("<response xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>" + Response + "</response>", null, null);
							}
							break;

						case "error":
							XmppException StreamException = GetExceptionObject(E);
							if (StreamException is SeeOtherHostException SeeOtherHostException)
							{
								this.host = SeeOtherHostException.NewHost;
								this.inputState = -1;

								if (!this.disposed)
								{
									this.Information("Reconnecting to " + this.host);
									await this.Connect(this.domain);
								}

								return false;
							}
							else
								throw StreamException;

						case "success":
							if (this.authenticationMethod is null)
								throw new XmppException("No authentication method selected.", E);
							else
							{
								if (this.authenticationMethod.CheckSuccess(E.InnerText, this))
								{
									if (this.performingQuickLogin)
									{
										await this.SetState(XmppState.Binding);

										await this.SetState(XmppState.RequestingSession);
										this.createSession = false;

										this.presenceSent = false;
										await this.AdvanceUntilConnected();
									}
									else
									{
										this.ResetState(true, true);

										await this.BeginWrite("<?xml version='1.0' encoding='utf-8'?><stream:stream to='" + XML.Encode(this.domain) +
											"' version='1.0' xml:lang='" + XML.Encode(this.language) +
											"' xmlns='" + NamespaceClient + "' xmlns:stream='" + NamespaceStream + "'>", null, null);
									}
								}
								else
									throw new XmppException("Server authentication rejected by client.", E);
							}
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

		private async Task<bool> ValidateSender(XmlElement Stanza, string From, string FromBareJid, IqEventArgs IqStanza, MessageEventArgs MessageStanza)
		{
			EventHandlerAsync<ValidateSenderEventArgs> h = this.OnValidateSender;
			if (h is null)
				return true;

			ValidateSenderEventArgs e = new ValidateSenderEventArgs(this, Stanza, From, FromBareJid, IqStanza, MessageStanza);
			await h.Raise(this, e, false);

			if (e.Rejected && !e.Accepted)
			{
				try
				{
					this.Warning("Incoming message rejected.");
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}

				return false;
			}
			else
				return true;
		}

		/// <summary>
		/// Event raised when a stanza has been received. Event handlers can validate the sender of a stanza.
		/// </summary>
		public event EventHandlerAsync<ValidateSenderEventArgs> OnValidateSender = null;

		/// <summary>
		/// Processes an incoming message.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		public async void ProcessMessage(MessageEventArgs e)
		{
			try
			{
				EventHandlerAsync<MessageFormEventArgs> FormHandler = null;
				EventHandlerAsync<MessageEventArgs> MessageHandler = null;
				DataForm Form = null;
				string FormType = null;
				string Key;

				lock (this.synchObject)
				{
					foreach (XmlNode N in e.Message.ChildNodes)
					{
						if (!(N is XmlElement E))
							continue;

						if (FormHandler is null && E.LocalName == "x" && E.NamespaceURI == NamespaceData)
						{
							Form = new DataForm(this, E, this.MessageFormSubmitted, this.MessageFormCancelled, e.From, e.To)
							{
								State = e
							};

							Field FormTypeField = Form["FORM_TYPE"];
							FormType = FormTypeField?.ValueString;

							if (!string.IsNullOrEmpty(FormType) && this.messageFormHandlers.TryGetValue(FormType, out FormHandler))
							{
								e.Content = E;
								break;
							}
						}

						Key = E.LocalName + " " + E.NamespaceURI;
						if (this.messageHandlers.TryGetValue(Key, out MessageHandler))
						{
							e.Content = E;
							break;
						}
						else if (E.LocalName == "request" && E.NamespaceURI == NamespaceMessageDeliveryReceipts && !string.IsNullOrEmpty(e.Id))
						{
							RosterItem Item = this.GetRosterItemLocked(GetBareJID(e.To));
							if (!(Item is null) && (Item.State == SubscriptionState.Both || Item.State == SubscriptionState.From))
							{
								this.SendMessage(MessageType.Normal, e.From, "<received xmlns='" + NamespaceMessageDeliveryReceipts +
									"' id='" + XML.Encode(e.Id) + "'/>", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
							}
						}
						else
							MessageHandler = null;
					}
				}

				if (!(MessageHandler is null))
					this.Information(MessageHandler.GetMethodInfo().Name);
				else if (!(FormHandler is null))
					this.Information(FormHandler.GetMethodInfo().Name);
				else
				{
					switch (e.Type)
					{
						case MessageType.Chat:
							this.Information("OnChatMessage()");
							MessageHandler = this.OnChatMessage;
							break;

						case MessageType.Error:
							this.Information("OnErrorMessage()");
							MessageHandler = this.OnErrorMessage;
							break;

						case MessageType.GroupChat:
							this.Information("OnGroupChatMessage()");
							MessageHandler = this.OnGroupChatMessage;
							break;

						case MessageType.Headline:
							this.Information("OnHeadlineMessage()");
							MessageHandler = this.OnHeadlineMessage;
							break;

						case MessageType.Normal:
						default:
							this.Information("OnNormalMessage()");
							MessageHandler = this.OnNormalMessage;
							break;
					}
				}

				if (!(MessageHandler is null))
				{
					try
					{
						await MessageHandler(this, e);
					}
					catch (Exception ex)
					{
						this.Exception(ex);
					}
				}
				else if (!(FormHandler is null))
				{
					try
					{
						await FormHandler(this, new MessageFormEventArgs(Form, FormType, e));
					}
					catch (Exception ex)
					{
						this.Exception(ex);
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

		private Task MessageFormSubmitted(object _, DataForm Form)
		{
			MessageEventArgs e = (MessageEventArgs)Form.State;
			this.SubmitForm(Form, e.Type, e.ThreadID, e.ParentThreadID);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Submits a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		public Task SubmitForm(DataForm Form)
		{
			return this.SubmitForm(Form, MessageType.Normal, string.Empty, string.Empty);
		}

		/// <summary>
		/// Submits a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		/// <param name="MessageType">Message type to use.</param>
		public Task SubmitForm(DataForm Form, MessageType MessageType)
		{
			return this.SubmitForm(Form, MessageType, string.Empty, string.Empty);
		}

		/// <summary>
		/// Submits a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		/// <param name="MessageType">Message type to use.</param>
		/// <param name="ThreadId">Thread ID</param>
		public Task SubmitForm(DataForm Form, MessageType MessageType, string ThreadId)
		{
			return this.SubmitForm(Form, MessageType, ThreadId, string.Empty);
		}

		/// <summary>
		/// Submits a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		/// <param name="MessageType">Message type to use.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public Task SubmitForm(DataForm Form, MessageType MessageType, string ThreadId, string ParentThreadId)
		{
			StringBuilder Xml = new StringBuilder();

			Form.SerializeSubmit(Xml);

			return this.SendMessage(MessageType, Form.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, ThreadId, ParentThreadId);
		}

		private Task MessageFormCancelled(object _, DataForm Form)
		{
			MessageEventArgs e = (MessageEventArgs)Form.State;
			return this.CancelForm(Form, e.Type, e.ThreadID, e.ParentThreadID);
		}

		/// <summary>
		/// Cancels a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		public Task CancelForm(DataForm Form)
		{
			return this.CancelForm(Form, MessageType.Normal, string.Empty, string.Empty);
		}

		/// <summary>
		/// Cancels a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		/// <param name="MessageType">Message type to use.</param>
		public Task CancelForm(DataForm Form, MessageType MessageType)
		{
			return this.CancelForm(Form, MessageType, string.Empty, string.Empty);
		}

		/// <summary>
		/// Cancels a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		/// <param name="MessageType">Message type to use.</param>
		/// <param name="ThreadId">Thread ID</param>
		public Task CancelForm(DataForm Form, MessageType MessageType, string ThreadId)
		{
			return this.CancelForm(Form, MessageType, ThreadId, string.Empty);
		}

		/// <summary>
		/// Cancels a form back to the sender.
		/// </summary>
		/// <param name="Form">Data Form</param>
		/// <param name="MessageType">Message type to use.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public Task CancelForm(DataForm Form, MessageType MessageType, string ThreadId, string ParentThreadId)
		{
			StringBuilder Xml = new StringBuilder();

			Form.SerializeCancel(Xml);

			return this.SendMessage(MessageType, Form.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, ThreadId, ParentThreadId);
		}

		/// <summary>
		/// Processes an incoming IQ GET stanza.
		/// </summary>
		/// <param name="e">IQ event arguments.</param>
		public Task ProcessIqGet(IqEventArgs e)
		{
			return this.ProcessIq(this.iqGetHandlers, e);
		}

		/// <summary>
		/// Processes an incoming IQ SET stanza.
		/// </summary>
		/// <param name="e">IQ event arguments.</param>
		public Task ProcessIqSet(IqEventArgs e)
		{
			return this.ProcessIq(this.iqSetHandlers, e);
		}

		internal async Task ProcessPresence(PresenceEventArgs e)
		{
			try
			{
				LinkedList<KeyValuePair<EventHandlerAsync<PresenceEventArgs>, XmlElement>> Handlers = null;
				EventHandlerAsync<PresenceEventArgs> Callback;
				RosterItem Item;
				string Key;
				object State = null;
				string Id = e.Id;
				string FromBareJid = e.FromBareJID;

				if (uint.TryParse(Id, out uint SeqNr))
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
							Callback = null;
					}
				}
				else
					Callback = null;

				if (Callback is null)
				{
					lock (this.synchObject)
					{
						foreach (XmlNode N in e.Presence.ChildNodes)
						{
							if (!(N is XmlElement E))
								continue;

							Key = E.LocalName + " " + E.NamespaceURI;
							if (this.presenceHandlers.TryGetValue(Key, out Callback))
							{
								if (Handlers is null)
									Handlers = new LinkedList<KeyValuePair<EventHandlerAsync<PresenceEventArgs>, XmlElement>>();

								Handlers.AddLast(new KeyValuePair<EventHandlerAsync<PresenceEventArgs>, XmlElement>(Callback, E));
							}
						}
					}

					switch (e.Type)
					{
						case PresenceType.Available:
							this.Information("OnPresence()");
							Callback = this.OnPresence;
							e.UpdateLastPresence = true;

							lock (this.synchObject)
							{
								if (this.roster.TryGetValue(e.FromBareJID, out Item))
									Item.PresenceReceived(this, e);
							}
							break;

						case PresenceType.Unavailable:
							this.Information("OnPresence()");
							Callback = this.OnPresence;

							lock (this.synchObject)
							{
								if (this.roster.TryGetValue(e.FromBareJID, out Item))
									Item.PresenceReceived(this, e);
							}
							break;

						case PresenceType.Error:
						case PresenceType.Probe:
						default:
							this.Information("OnPresence()");
							Callback = this.OnPresence;
							break;

						case PresenceType.Subscribe:
							lock (this.synchObject)
							{
								this.subscriptionRequests[e.FromBareJID] = e;
							}

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

					if (!(Handlers is null))
					{
						foreach (KeyValuePair<EventHandlerAsync<PresenceEventArgs>, XmlElement> P in Handlers)
						{
							e.Content = P.Value;
							this.Information(P.Key.GetMethodInfo().Name);

							try
							{
								await P.Key(this, e);
							}
							catch (Exception ex)
							{
								this.Exception(ex);
							}
						}
					}
				}

				e.Content = null;
				e.State = State;

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

		private async Task ProcessIq(Dictionary<string, EventHandlerAsync<IqEventArgs>> Handlers, IqEventArgs e)
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
				{
					string Xml = "<error type='cancel'><feature-not-implemented xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/></error>";

					if (e.UsesE2eEncryption)
						await e.E2eEncryption.SendIqError(this, E2ETransmission.NormalIfNotE2E, e.Id, e.From, Xml);
					else
						await this.SendIqError(e.Id, e.From, Xml);
				}
				else
				{
					try
					{
						await h(this, e);
					}
					catch (Exception ex)
					{
						if (e.UsesE2eEncryption)
							await e.E2eEncryption.SendIqError(this, E2ETransmission.NormalIfNotE2E, e.Id, e.From, this.ExceptionToXmppXml(ex));
						else
							await this.SendIqError(e.Id, e.From, ex);
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

		private void RegisterIqHandler(Dictionary<string, EventHandlerAsync<IqEventArgs>> Handlers, string LocalName, string Namespace, EventHandlerAsync<IqEventArgs> Handler,
			bool PublishNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (Handlers.TryGetValue(Key, out EventHandlerAsync<IqEventArgs> PrevHandler))
				{
					if (PrevHandler.Method == Handler.Method && PrevHandler.Target == Handler.Target)
						return;

					throw new ArgumentException("Handler already registered: " + Namespace + "#" + LocalName, nameof(LocalName));
				}

				Handlers[Key] = Handler;

				if (PublishNamespaceAsClientFeature)
				{
					this.clientFeatures[Namespace] = true;
					this.entityCapabilitiesVersion = null;
				}
			}
		}

		private class SyncIqHandler
		{
			private readonly EventHandlerAsync<IqEventArgs> h;

			public SyncIqHandler(EventHandlerAsync<IqEventArgs> h)
			{
				this.h = h;
			}

			public Task Call(object Sender, IqEventArgs e)
			{
				this.h(Sender, e);
				return Task.CompletedTask;
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

		private bool UnregisterIqHandler(Dictionary<string, EventHandlerAsync<IqEventArgs>> Handlers, string LocalName, string Namespace, EventHandlerAsync<IqEventArgs> Handler,
			bool RemoveNamespaceAsClientFeature)
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
				{
					this.clientFeatures.Remove(Namespace);
					this.entityCapabilitiesVersion = null;
				}
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
				if (this.messageHandlers.TryGetValue(Key, out EventHandlerAsync<MessageEventArgs> PrevHandler))
				{
					if (PrevHandler.Method == Handler.Method && PrevHandler.Target == Handler.Target)
						return;
					
					throw new ArgumentException("Handler already registered: " + Namespace + "#" + LocalName, nameof(LocalName));
				}

				this.messageHandlers[Key] = Handler;

				if (PublishNamespaceAsClientFeature)
				{
					this.clientFeatures[Namespace] = true;
					this.entityCapabilitiesVersion = null;
				}
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
				{
					this.clientFeatures.Remove(Namespace);
					this.entityCapabilitiesVersion = null;
				}
			}

			return true;
		}

		/// <summary>
		/// Registers a Message Form handler.
		/// </summary>
		/// <param name="FormType">Form Type, as defined by the FORM_TYPE field in the form.</param>
		/// <param name="Handler">Handler to process message.</param>
		public void RegisterMessageFormHandler(string FormType, EventHandlerAsync<MessageFormEventArgs> Handler)
		{
			lock (this.synchObject)
			{
				if (this.messageFormHandlers.TryGetValue(FormType, out EventHandlerAsync<MessageFormEventArgs> PrevHandler))
				{
					if (PrevHandler.Method == Handler.Method && PrevHandler.Target == Handler.Target)
						return;

					throw new ArgumentException("Handler already registered: " + FormType, nameof(FormType));
				}

				this.messageFormHandlers[FormType] = Handler;
			}
		}

		/// <summary>
		/// Unregisters a Message handler.
		/// </summary>
		/// <param name="FormType">Form Type, as defined by the FORM_TYPE field in the form.</param>
		/// <param name="Handler">Handler to remove.</param>
		/// <returns>If the handler was found and removed.</returns>
		public bool UnregisterMessageFormHandler(string FormType, EventHandlerAsync<MessageFormEventArgs> Handler)
		{
			lock (this.synchObject)
			{
				if (!this.messageFormHandlers.TryGetValue(FormType, out EventHandlerAsync<MessageFormEventArgs> h))
					return false;

				if (h != Handler)
					return false;

				this.messageFormHandlers.Remove(FormType);
			}

			return true;
		}

		/// <summary>
		/// Registers a Presence handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to process presence.</param>
		/// <param name="PublishNamespaceAsClientFeature">If the namespace should be published as a client feature.</param>
		public void RegisterPresenceHandler(string LocalName, string Namespace, EventHandlerAsync<PresenceEventArgs> Handler, bool PublishNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (this.presenceHandlers.TryGetValue(Key, out EventHandlerAsync<PresenceEventArgs> PrevHandler))
				{
					if (PrevHandler.Method == Handler.Method && PrevHandler.Target == Handler.Target)
						return;

					throw new ArgumentException("Handler already registered: " + Namespace + "#" + LocalName, nameof(LocalName));
				}

				this.presenceHandlers[Key] = Handler;

				if (PublishNamespaceAsClientFeature)
				{
					this.clientFeatures[Namespace] = true;
					this.entityCapabilitiesVersion = null;
				}
			}
		}

		/// <summary>
		/// Unregisters a Presence handler.
		/// </summary>
		/// <param name="LocalName">Local Name</param>
		/// <param name="Namespace">Namespace</param>
		/// <param name="Handler">Handler to remove.</param>
		/// <param name="RemoveNamespaceAsClientFeature">If the namespace should be removed from the lit of client features.</param>
		/// <returns>If the handler was found and removed.</returns>
		public bool UnregisterPresenceHandler(string LocalName, string Namespace, EventHandlerAsync<PresenceEventArgs> Handler, bool RemoveNamespaceAsClientFeature)
		{
			string Key = LocalName + " " + Namespace;

			lock (this.synchObject)
			{
				if (!this.presenceHandlers.TryGetValue(Key, out EventHandlerAsync<PresenceEventArgs> h))
					return false;

				if (h != Handler)
					return false;

				this.presenceHandlers.Remove(Key);

				if (RemoveNamespaceAsClientFeature)
				{
					this.clientFeatures.Remove(Namespace);
					this.entityCapabilitiesVersion = null;
				}
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
				this.entityCapabilitiesVersion = null;
			}
		}

		/// <summary>
		/// Checks if a feature is supported by the client.
		/// </summary>
		/// <param name="Feature">Feature.</param>
		/// <returns>If feature is supported.</returns>
		public bool SupportsFeature(string Feature)
		{
			lock (this.synchObject)
			{
				return this.clientFeatures.ContainsKey(Feature);
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
				this.entityCapabilitiesVersion = null;
				return this.clientFeatures.Remove(Feature);
			}
		}

		/// <summary>
		/// Gets available client features.
		/// </summary>
		/// <returns>Client features.</returns>
		public string[] GetFeatures()
		{
			string[] Result;

			lock (this.synchObject)
			{
				Result = new string[this.clientFeatures.Count];
				this.clientFeatures.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Registers handlers already registered on a prototype client.
		/// </summary>
		/// <param name="Prototype">Prototype, from which to take registrations.</param>
		public void RegisterHandlers(XmppClient Prototype)
		{
			KeyValuePair<string, EventHandlerAsync<IqEventArgs>>[] GetHandlers;
			KeyValuePair<string, EventHandlerAsync<IqEventArgs>>[] SetHandlers;
			KeyValuePair<string, EventHandlerAsync<MessageEventArgs>>[] MessageHandlers;
			KeyValuePair<string, EventHandlerAsync<PresenceEventArgs>>[] PresenceHandlers;
			KeyValuePair<string, bool>[] Features;
			KeyValuePair<string, DataForm>[] Forms;
			int i;

			lock (Prototype.synchObject)
			{
				GetHandlers = new KeyValuePair<string, EventHandlerAsync<IqEventArgs>>[Prototype.iqGetHandlers.Count];
				i = 0;
				foreach (KeyValuePair<string, EventHandlerAsync<IqEventArgs>> P in Prototype.iqGetHandlers)
					GetHandlers[i++] = P;

				SetHandlers = new KeyValuePair<string, EventHandlerAsync<IqEventArgs>>[Prototype.iqSetHandlers.Count];
				i = 0;
				foreach (KeyValuePair<string, EventHandlerAsync<IqEventArgs>> P in Prototype.iqSetHandlers)
					SetHandlers[i++] = P;

				MessageHandlers = new KeyValuePair<string, EventHandlerAsync<MessageEventArgs>>[Prototype.messageHandlers.Count];
				i = 0;
				foreach (KeyValuePair<string, EventHandlerAsync<MessageEventArgs>> P in Prototype.messageHandlers)
					MessageHandlers[i++] = P;

				PresenceHandlers = new KeyValuePair<string, EventHandlerAsync<PresenceEventArgs>>[Prototype.presenceHandlers.Count];
				i = 0;
				foreach (KeyValuePair<string, EventHandlerAsync<PresenceEventArgs>> P in Prototype.presenceHandlers)
					PresenceHandlers[i++] = P;

				Features = new KeyValuePair<string, bool>[Prototype.clientFeatures.Count];
				i = 0;
				foreach (KeyValuePair<string, bool> P in Prototype.clientFeatures)
					Features[i++] = P;

				Forms = new KeyValuePair<string, DataForm>[Prototype.extendedServiceDiscoveryInformation.Count];
				i = 0;
				foreach (KeyValuePair<string, DataForm> P in Prototype.extendedServiceDiscoveryInformation)
					Forms[i++] = P;
			}

			lock (this.synchObject)
			{
				foreach (KeyValuePair<string, EventHandlerAsync<IqEventArgs>> P in GetHandlers)
					this.iqGetHandlers[P.Key] = P.Value;

				foreach (KeyValuePair<string, EventHandlerAsync<IqEventArgs>> P in SetHandlers)
					this.iqSetHandlers[P.Key] = P.Value;

				foreach (KeyValuePair<string, EventHandlerAsync<MessageEventArgs>> P in MessageHandlers)
					this.messageHandlers[P.Key] = P.Value;

				foreach (KeyValuePair<string, EventHandlerAsync<PresenceEventArgs>> P in PresenceHandlers)
					this.presenceHandlers[P.Key] = P.Value;

				foreach (KeyValuePair<string, bool> P in Features)
					this.clientFeatures[P.Key] = P.Value;

				foreach (KeyValuePair<string, DataForm> P in Forms)
					this.extendedServiceDiscoveryInformation[P.Key] = P.Value;

				this.entityCapabilitiesVersion = null;
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

		private async Task<bool> StartQuickLogin()
		{
			if (!(this.authenticationMethod is null))
				return false;

			string Challenge;

			switch (this.passwordHashMethod)
			{
				case "SCRAM-SHA-256":
					if (!this.allowScramSHA256)
						return false;

					string Nonce = Convert.ToBase64String(GetRandomBytes(32));
					string s = "n,,n=" + this.userName + ",r=" + Nonce;
					byte[] Data = Encoding.UTF8.GetBytes(s);

					this.authenticationMethod = new ScramSha256(Nonce);

					Challenge = Convert.ToBase64String(Data);
					break;

				case "SCRAM-SHA-1":
					if (!this.allowScramSHA1)
						return false;

					Nonce = Convert.ToBase64String(GetRandomBytes(20));
					s = "n,,n=" + this.userName + ",r=" + Nonce;
					Data = Encoding.UTF8.GetBytes(s);

					this.authenticationMethod = new ScramSha1(Nonce);

					Challenge = Convert.ToBase64String(Data);
					break;

				default:
					return false;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<ql xmlns='");
			Xml.Append(NamespaceQuickLogin);
			Xml.Append("' m='");
			Xml.Append(this.passwordHashMethod);
			Xml.Append("' c='");
			Xml.Append(XML.Encode(Challenge));

			if (!string.IsNullOrEmpty(this.resource))
			{
				Xml.Append("' r='");
				Xml.Append(XML.Encode(this.resource));
			}

			Xml.Append("'/>");

			await this.SetState(XmppState.StartingEncryption);
			await this.BeginWrite(Xml.ToString(), null, null);

			return true;
		}

		private async Task StartAuthentication()
		{
			if (this.authenticationMethod is null)
			{
				if (this.allowScramSHA256 && this.authenticationMechanisms.ContainsKey("SCRAM-SHA-256") &&
					(string.IsNullOrEmpty(this.passwordHashMethod) || this.passwordHashMethod == "SCRAM-SHA-256"))
				{
					string Nonce = Convert.ToBase64String(GetRandomBytes(32));
					string s = "n,,n=" + this.userName + ",r=" + Nonce;
					byte[] Data = Encoding.UTF8.GetBytes(s);

					await this.SetState(XmppState.Authenticating);
					this.authenticationMethod = new ScramSha256(Nonce);
					await this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='SCRAM-SHA-256'>" +
						Convert.ToBase64String(Data) + "</auth>", null, null);
				}
				else if (this.allowScramSHA1 && this.authenticationMechanisms.ContainsKey("SCRAM-SHA-1") &&
					(string.IsNullOrEmpty(this.passwordHashMethod) || this.passwordHashMethod == "SCRAM-SHA-1"))
				{
					string Nonce = Convert.ToBase64String(GetRandomBytes(20));
					string s = "n,,n=" + this.userName + ",r=" + Nonce;
					byte[] Data = Encoding.UTF8.GetBytes(s);

					await this.SetState(XmppState.Authenticating);
					this.authenticationMethod = new ScramSha1(Nonce);
					await this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='SCRAM-SHA-1'>" +
						Convert.ToBase64String(Data) + "</auth>", null, null);
				}
				else if (this.allowDigestMD5 && this.authenticationMechanisms.ContainsKey("DIGEST-MD5") &&
					(string.IsNullOrEmpty(this.passwordHashMethod) || this.passwordHashMethod == "DIGEST-MD5"))
				{
					await this.SetState(XmppState.Authenticating);
					this.authenticationMethod = new DigestMd5();
					await this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='DIGEST-MD5'/>", null, null);
				}
				else if (this.allowCramMD5 && this.authenticationMechanisms.ContainsKey("CRAM-MD5") &&
					(string.IsNullOrEmpty(this.passwordHashMethod) || this.passwordHashMethod == "CRAM-MD5"))
				{
					await this.SetState(XmppState.Authenticating);
					this.authenticationMethod = new CramMd5();
					await this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='CRAM-MD5'/>", null, null);
				}
				else if (this.allowPlain && this.authenticationMechanisms.ContainsKey("PLAIN") &&
					(string.IsNullOrEmpty(this.passwordHashMethod) || this.passwordHashMethod == "PLAIN"))
				{
					await this.SetState(XmppState.Authenticating);
					this.authenticationMethod = new Plain();

					string Pwd;

					if (string.IsNullOrEmpty(this.passwordHashMethod))
					{
						Pwd = this.password;
						this.passwordHash = Pwd;
						this.passwordHashMethod = "PLAIN";
					}
					else
						Pwd = this.passwordHash;

					await this.BeginWrite("<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='PLAIN'>" +
						Convert.ToBase64String(this.encoding.GetBytes("\x00" + this.userName + "\x00" + Pwd)) +
						"</auth>", null, null);
				}
				//else if (this.authenticationMechanisms.ContainsKey("ANONYMOUS"))
				//	throw new XmppException("ANONYMOUS authentication method not allowed.");
				else
					throw new XmppException("No allowed authentication method supported.");
			}
		}

		private static string GetErrorText(XmlElement E)
		{
			foreach (XmlNode N2 in E.ChildNodes)
			{
				if (N2.LocalName == "text" && (N2.NamespaceURI == NamespaceXmppStanzas || N2.NamespaceURI == NamespaceXmppStreams || N2.NamespaceURI == NamespaceXmppSasl))
					return N2.InnerText.Trim();
			}

			return string.Empty;
		}

		/// <summary>
		/// Gets an XMPP Exception object corresponding to its XML definition.
		/// </summary>
		/// <param name="StanzaElement">XMPP Stanza.</param>
		/// <returns>Exception object.</returns>
		public static XmppException GetExceptionObject(XmlElement StanzaElement)
		{
			string Msg = GetErrorText(StanzaElement);

			foreach (XmlNode N2 in StanzaElement.ChildNodes)
			{
				switch (N2.NamespaceURI)
				{
					case NamespaceXmppStreams:
						switch (N2.LocalName)
						{
							// Stream Exceptions:
							case "bad-format": return new BadFormatException(Msg, StanzaElement);
							case "bad-namespace-prefix": return new BadNamespacePrefixException(Msg, StanzaElement);
							case "conflict": return new StreamErrors.ConflictException(Msg, StanzaElement);
							case "connection-timeout": return new ConnectionTimeoutException(Msg, StanzaElement);
							case "host-gone": return new HostGoneException(Msg, StanzaElement);
							case "host-unknown": return new HostUnknownException(Msg, StanzaElement);
							case "improper-addressing": return new ImproperAddressingException(Msg, StanzaElement);
							case "internal-server-error": return new StreamErrors.InternalServerErrorException(Msg, StanzaElement);
							case "invalid-from": return new InvalidFromException(Msg, StanzaElement);
							case "invalid-namespace": return new InvalidNamespaceException(Msg, StanzaElement);
							case "invalid-xml": return new InvalidXmlException(Msg, StanzaElement);
							case "not-authorized": return new StreamErrors.NotAuthorizedException(Msg, StanzaElement);
							case "not-well-formed": return new NotWellFormedException(Msg, StanzaElement);
							case "policy-violation": return new StreamErrors.PolicyViolationException(Msg, StanzaElement);
							case "remote-connection-failed": return new RemoteConnectionFailedException(Msg, StanzaElement);
							case "reset": return new ResetException(Msg, StanzaElement);
							case "resource-constraint": return new StreamErrors.ResourceConstraintException(Msg, StanzaElement);
							case "restricted-xml": return new RestrictedXmlException(Msg, StanzaElement);
							case "see-other-host": return new SeeOtherHostException(Msg, StanzaElement, N2.InnerText);
							case "system-shutdown": return new SystemShutdownException(Msg, StanzaElement);
							case "undefined-condition": return new StreamErrors.UndefinedConditionException(Msg, StanzaElement);
							case "unsupported-encoding": return new UnsupportedEncodingException(Msg, StanzaElement);
							case "unsupported-feature": return new UnsupportedFeatureException(Msg, StanzaElement);
							case "unsupported-stanza-type": return new UnsupportedStanzaTypeException(Msg, StanzaElement);
							case "unsupported-version": return new UnsupportedVersionException(Msg, StanzaElement);
							case "xml-not-well-formed": return new NotWellFormedException(Msg, StanzaElement);
							default: return new XmppException(string.IsNullOrEmpty(Msg) ? "Unrecognized stream error returned." : Msg, StanzaElement);
						}

					case NamespaceXmppStanzas:
						switch (N2.LocalName)
						{
							case "bad-request": return new BadRequestException(Msg, StanzaElement);
							case "conflict": return new StanzaErrors.ConflictException(Msg, StanzaElement);
							case "feature-not-implemented": return new FeatureNotImplementedException(Msg, StanzaElement);
							case "forbidden": return new ForbiddenException(Msg, StanzaElement);
							case "gone": return new GoneException(Msg, StanzaElement);
							case "internal-server-error": return new StanzaErrors.InternalServerErrorException(Msg, StanzaElement);
							case "item-not-found": return new ItemNotFoundException(Msg, StanzaElement);
							case "jid-malformed": return new JidMalformedException(Msg, StanzaElement);
							case "not-acceptable": return new NotAcceptableException(Msg, StanzaElement);
							case "not-allowed": return new NotAllowedException(Msg, StanzaElement);
							case "not-authorized": return new StanzaErrors.NotAuthorizedException(Msg, StanzaElement);
							case "policy-violation": return new StanzaErrors.PolicyViolationException(Msg, StanzaElement);
							case "recipient-unavailable": return new RecipientUnavailableException(Msg, StanzaElement);
							case "redirect": return new RedirectException(Msg, StanzaElement);
							case "registration-required": return new RegistrationRequiredException(Msg, StanzaElement);
							case "remote-server-not-found": return new RemoteServerNotFoundException(Msg, StanzaElement);
							case "remote-server-timeout": return new RemoteServerTimeoutException(Msg, StanzaElement);
							case "resource-constraint": return new StanzaErrors.ResourceConstraintException(Msg, StanzaElement);
							case "service-unavailable": return new ServiceUnavailableException(Msg, StanzaElement);
							case "subscription-required": return new SubscriptionRequiredException(Msg, StanzaElement);
							case "undefined-condition": return new StanzaErrors.UndefinedConditionException(Msg, StanzaElement);
							case "unexpected-request": return new UnexpectedRequestException(Msg, StanzaElement);
							default: return new XmppException(string.IsNullOrEmpty(Msg) ? "Unrecognized stanza error returned." : Msg, StanzaElement);
						}

					case NamespaceXmppSasl:
						switch (N2.LocalName)
						{
							case "account-disabled": return new AccountDisabledException(Msg, StanzaElement);
							case "credentials-expired": return new CredentialsExpiredException(Msg, StanzaElement);
							case "encryption-required": return new EncryptionRequiredException(Msg, StanzaElement);
							case "incorrect-encoding": return new IncorrectEncodingException(Msg, StanzaElement);
							case "invalid-authzid": return new InvalidAuthzidException(Msg, StanzaElement);
							case "invalid-mechanism": return new InvalidMechanismException(Msg, StanzaElement);
							case "malformed-request": return new MalformedRequestException(Msg, StanzaElement);
							case "mechanism-too-weak": return new MechanismTooWeakException(Msg, StanzaElement);
							case "bad-auth":    // SASL error returned from some XMPP servers. Not listed in RFC6120.
							case "not-authorized": return new AuthenticationErrors.NotAuthorizedException(Msg, StanzaElement);
							case "temporary-auth-failure": return new TemporaryAuthFailureException(Msg, StanzaElement);
							default: return new XmppException(string.IsNullOrEmpty(Msg) ? "Unrecognized SASL error returned." : Msg, StanzaElement);
						}
				}
			}

			return new XmppException(string.IsNullOrEmpty(Msg) ? "Unspecified error returned." : Msg, StanzaElement);
		}

		private async Task Client_OnPaused(object Sender, EventArgs e)
		{
			if (this.upgradeToTls)
			{
				this.upgradeToTls = false;

				try
				{
					await this.SetState(XmppState.StartingEncryption);
#if WINDOWS_UWP
					await this.client.UpgradeToTlsAsClient(SocketProtectionLevel.Tls12, this.trustServer);
#else
					await this.client.UpgradeToTlsAsClient(this.clientCertificate, Crypto.SecureTls, this.trustServer, "xmpp-client");
#endif
					bool SendStream = !this.performingQuickLogin;

					this.ResetState(false, SendStream);
					this.client?.Continue();

					if (SendStream)
					{
						await this.BeginWrite("<?xml version='1.0' encoding='utf-8'?><stream:stream from='" + XML.Encode(this.bareJid) + "' to='" + XML.Encode(this.domain) +
							"' version='1.0' xml:lang='" + XML.Encode(this.language) + "' xmlns='" + NamespaceClient + "' xmlns:stream='" +
							NamespaceStream + "'>", null, null);
					}
				}
				catch (Exception ex)
				{
					await this.ConnectionError(ex);
				}
			}
		}

		/// <summary>
		/// User name.
		/// </summary>
		public string UserName => this.userName;

		internal string Password => this.password;

		/// <summary>
		/// Hash value of password. Depends on method used to authenticate user.
		/// </summary>
		public string PasswordHash
		{
			get => this.passwordHash;
			internal set => this.passwordHash = value;
		}

		/// <summary>
		/// Password hash method.
		/// </summary>
		public string PasswordHashMethod
		{
			get => this.passwordHashMethod;
			internal set => this.passwordHashMethod = value;
		}

		/// <summary>
		/// Current Domain.
		/// </summary>
		public string Domain => this.domain;

		/// <summary>
		/// Current Stream ID
		/// </summary>
		public string StreamId => this.streamId;

		/// <summary>
		/// Bare JID
		/// </summary>
		public string BareJID => this.bareJid;

		/// <summary>
		/// Full JID.
		/// </summary>
		public string FullJID => this.fullJid;

		/// <summary>
		/// Resource part of the <see cref="FullJID"/>. Will be available after successfully binding the connection.
		/// </summary>
		public string Resource => this.resource;

		/// <summary>
		/// If the CRAM-MD5 authentication method is allowed or not. Default is true.
		/// </summary>
		public bool AllowCramMD5
		{
			get => this.allowCramMD5;
			set => this.allowCramMD5 = value;
		}

		/// <summary>
		/// If the DIGEST-MD5 authentication method is allowed or not. Default is true.
		/// </summary>
		public bool AllowDigestMD5
		{
			get => this.allowDigestMD5;
			set => this.allowDigestMD5 = value;
		}

		/// <summary>
		/// If the SCRAM-SHA-1 authentication method is allowed or not. Default is true.
		/// </summary>
		public bool AllowScramSHA1
		{
			get => this.allowScramSHA1;
			set => this.allowScramSHA1 = value;
		}

		/// <summary>
		/// If the SCRAM-SHA-1 authentication method is allowed or not. Default is true.
		/// </summary>
		public bool AllowScramSHA256
		{
			get => this.allowScramSHA256;
			set => this.allowScramSHA256 = value;
		}

		/// <summary>
		/// If the PLAIN authentication method is allowed or not. Default is true.
		/// </summary>
		public bool AllowPlain
		{
			get => this.allowPlain;
			set => this.allowPlain = value;
		}

		/// <summary>
		/// If Quick Login is permitted and supported by the broker.
		/// </summary>
		public bool AllowQuickLogin
		{
			get => this.allowQuickLogin;
			set => this.allowQuickLogin = value;
		}

		/// <summary>
		/// If registration of a new account is allowed. 
		/// Requires a password. Having a password hash is not sufficient.
		/// </summary>
		public void AllowRegistration()
		{
			this.AllowRegistration(string.Empty, string.Empty);
		}

		/// <summary>
		/// If registration of a new account is allowed.
		/// Requires a password. Having a password hash is not sufficient.
		/// </summary>
		/// <param name="FormSignatureKey">Form signature key, if form signatures (XEP-0348) is to be used during registration.</param>
		/// <param name="FormSignatureSecret">Form signature secret, if form signatures (XEP-0348) is to be used during registration.</param>
		public void AllowRegistration(string FormSignatureKey, string FormSignatureSecret)
		{
			this.allowedToRegister = true;
			this.formSignatureKey = FormSignatureKey;
			this.formSignatureSecret = FormSignatureSecret;
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.SendIq(null, To, Xml, "get", Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries)
		{
			return this.SendIq(null, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, false, RetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
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
		public Task<uint> SendIqGet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			return this.SendIq(null, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.SendIq(null, To, Xml, "set", Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries)
		{
			return this.SendIq(null, To, Xml, "set", Callback, State, RetryTimeout, NrRetries, false, RetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
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
		public Task<uint> SendIqSet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			return this.SendIq(null, To, Xml, "set", Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(null, To, Xml, "get", Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(null, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, false, RetryTimeout, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqGet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(null, To, Xml, "get", Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(null, To, Xml, "set", Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State, int RetryTimeout, int NrRetries,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(null, To, Xml, "set", Callback, State, RetryTimeout, NrRetries, false, RetryTimeout, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza</returns>
		public Task<uint> SendIqSet(string To, string Xml, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(null, To, Xml, "set", Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout,
				DeliveryCallback);
		}

		/// <summary>
		/// Returns a response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the response.</param>
		public Task SendIqResult(string Id, string To, string Xml)
		{
			this.responses.Add(To, Id, Xml, true);
			return this.SendIq(Id, To, Xml, "result", null, null, 0, 0, false, 0);
		}

		/// <summary>
		/// Returns an error response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the response.</param>
		public Task SendIqError(string Id, string To, string Xml)
		{
			this.responses.Add(To, Id, Xml, false);
			return this.SendIq(Id, To, Xml, "error", null, null, 0, 0, false, 0);
		}

		/// <summary>
		/// Returns an error response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="To">Destination address</param>
		/// <param name="ex">Internal exception object.</param>
		public async Task SendIqError(string Id, string To, Exception ex)
		{
			await this.SendIqError(Id, To, this.ExceptionToXmppXml(ex));
		}

		/// <summary>
		/// Converts an exception object to an XMPP XML error element.
		/// </summary>
		/// <param name="ex">Exception.</param>
		/// <returns>Error element.</returns>
		public string ExceptionToXmppXml(Exception ex)
		{
			this.Error(ex.Message);

			StringBuilder Xml = new StringBuilder();
			if (ex is StanzaExceptionException ex2)
			{
				Xml.Append("<error type='");
				Xml.Append(ex2.ErrorType);
				Xml.Append("'><");
				Xml.Append(ex2.ErrorStanzaName);
				Xml.Append(" xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
				Xml.Append("<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>");
				Xml.Append(XML.Encode(ex2.Message));
				Xml.Append("</text>");
				Xml.Append("</error>");
			}
			else
			{
				Xml.Append("<error type='cancel'><internal-server-error xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
				Xml.Append("<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>");
				Xml.Append(XML.Encode(ex.Message));
				Xml.Append("</text>");
				Xml.Append("</error>");
			}

			return Xml.ToString();
		}

		/// <summary>
		/// Generates a new id attribute value.
		/// </summary>
		/// <returns>Next id</returns>
		public string NextId()
		{
			uint SeqNr;

			lock (this.synchObject)
			{
				do
				{
					SeqNr = this.seqnr++;
				}
				while (this.pendingRequestsBySeqNr.ContainsKey(SeqNr));
			}

			return SeqNr.ToString();
		}

		/// <summary>
		/// Sends an IQ stanza.
		/// </summary>
		/// <param name="Id">Optional ID attribute of IQ stanza.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Type">Type of IQ stanza to send.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza, if none provided in <paramref name="Id"/>.</returns>
		public Task<uint> SendIq(string Id, string To, string Xml, string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.SendIq(Id, To, Xml, Type, Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout, null);
		}

		/// <summary>
		/// Sends an IQ stanza.
		/// </summary>
		/// <param name="Id">Optional ID attribute of IQ stanza.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Type">Type of IQ stanza to send.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza, if none provided in <paramref name="Id"/>.</returns>
		public Task<uint> SendIq(string Id, string To, string Xml, string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			return this.SendIq(Id, To, Xml, Type, Callback, State, this.defaultRetryTimeout, this.defaultNrRetries, this.defaultDropOff,
				this.defaultMaxRetryTimeout, DeliveryCallback);
		}

		/// <summary>
		/// Sends an IQ stanza.
		/// </summary>
		/// <param name="Id">Optional ID attribute of IQ stanza.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Type">Type of IQ stanza to send.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza, if none provided in <paramref name="Id"/>.</returns>
		public Task<uint> SendIq(string Id, string To, string Xml, string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			return this.SendIq(Id, To, Xml, Type, Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, null);
		}

		/// <summary>
		/// Sends an IQ stanza.
		/// </summary>
		/// <param name="Id">Optional ID attribute of IQ stanza.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Type">Type of IQ stanza to send.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <param name="DeliveryCallback">Optional callback called when request has been sent.</param>
		/// <returns>ID of IQ stanza, if none provided in <paramref name="Id"/>.</returns>
		public async Task<uint> SendIq(string Id, string To, string Xml, string Type, EventHandlerAsync<IqResultEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback)
		{
			PendingRequest PendingRequest = null;
			DateTime TP;
			uint SeqNr;

			if (Type == "get" || Type == "set")
			{
				lock (this.synchObject)
				{
					if (string.IsNullOrEmpty(Id))
					{
						do
						{
							SeqNr = this.seqnr++;
						}
						while (this.pendingRequestsBySeqNr.ContainsKey(SeqNr));

						Id = SeqNr.ToString();
					}
					else
					{
						if (!uint.TryParse(Id, out SeqNr))
							SeqNr = 0;
						else if (this.pendingRequestsBySeqNr.ContainsKey(SeqNr))
							throw new ArgumentException("Pending request with that id already exits.", nameof(Id));
					}

					PendingRequest = new PendingRequest(SeqNr, Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, To);
					TP = PendingRequest.Timeout;

					while (this.pendingRequestsByTimeout.ContainsKey(TP))
						TP = TP.AddTicks(this.gen.Next(1, 10));

					PendingRequest.Timeout = TP;

					this.pendingRequestsBySeqNr[SeqNr] = PendingRequest;
					this.pendingRequestsByTimeout[TP] = PendingRequest;
				}
			}
			else if (!uint.TryParse(Id, out SeqNr))
				SeqNr = 0;

			StringBuilder XmlOutput = new StringBuilder();

			XmlOutput.Append("<iq type='");
			XmlOutput.Append(Type);
			XmlOutput.Append("' id='");
			XmlOutput.Append(Id);

			if (this.sendFromAddress && !string.IsNullOrEmpty(this.fullJid))
			{
				XmlOutput.Append("' from='");
				XmlOutput.Append(XML.Encode(this.fullJid));
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

			await this.BeginWrite(IqXml, DeliveryCallback, State);

			return SeqNr;
		}

		/// <summary>
		/// Performs a synchronous IQ Get request/response operation.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public XmlElement IqGet(string To, string Xml, int Timeout)
		{
			Task<XmlElement> Result = this.IqGetAsync(To, Xml);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous IQ Get request/response operation.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<XmlElement> IqGetAsync(string To, string Xml)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.SendIqGet(To, Xml, (Sender, e) =>
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
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public XmlElement IqSet(string To, string Xml, int Timeout)
		{
			Task<XmlElement> Result = this.IqSetAsync(To, Xml);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous IQ Set request/response operation.
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <returns>Response XML element.</returns>
		/// <exception cref="TimeoutException">If a timeout occurred.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<XmlElement> IqSetAsync(string To, string Xml)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.SendIqSet(To, Xml, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Response);
				else
					Result.TrySetException(e.StanzaError ?? new XmppException("Unable to perform IQ Set."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		private async Task RegistrationFormReceived(object Sender, IqResultEventArgs e)
		{
			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query" && N.NamespaceURI == NamespaceRegister)
					{
						DataForm Form = null;
						string UserName = null;
						string Password = null;

						foreach (XmlNode N2 in N.ChildNodes)
						{
							switch (N2.LocalName)
							{
								case "username":
									UserName = N2.InnerText;
									break;

								case "password":
									Password = N2.InnerText;
									break;

								case "x":
									Form = new DataForm(this, (XmlElement)N2, this.SubmitRegistrationForm, this.CancelRegistrationForm, e.From, e.To)
									{
										State = e
									};

									Field Field = Form["username"];
									if (!(Field is null))
										await Field.SetValue(this.userName);

									Field = Form["password"];
									if (!(Field is null))
										await Field.SetValue(this.password);
									break;
							}
						}

						if (!(Form is null))
						{
							this.Information("OnRegistrationForm()");
							EventHandlerAsync<DataForm> h = this.OnRegistrationForm;
							if (!(h is null))
								await h.Raise(this, Form);
							else
								await Form.Submit();
						}
						else
						{
							StringBuilder Xml = new StringBuilder();

							Xml.Append("<query xmlns='" + NamespaceRegister + "'>");

							if (!(UserName is null))
							{
								Xml.Append("<username>");
								Xml.Append(XML.Encode(this.userName));
								Xml.Append("</username>");
							}

							if (!(Password is null))
							{
								Xml.Append("<password>");
								Xml.Append(XML.Encode(this.password));
								Xml.Append("</password>");
							}

							Xml.Append("</query>");

							await this.SendIqSet(string.Empty, Xml.ToString(), this.RegistrationResultReceived, null);
						}

						return;
					}
				}
			}

			await this.ConnectionError(e.StanzaError ?? new XmppException("Unable to register new account.", e.Response));
		}

		/// <summary>
		/// Event raised when a registration form is shown during automatic account creation during connection.
		/// </summary>
		public event EventHandlerAsync<DataForm> OnRegistrationForm = null;

		private async Task SubmitRegistrationForm(object _, DataForm RegistrationForm)
		{
			IqResultEventArgs e = (IqResultEventArgs)RegistrationForm.State;
			StringBuilder Xml = new StringBuilder();

			if (!string.IsNullOrEmpty(this.formSignatureKey) && !string.IsNullOrEmpty(this.formSignatureSecret))
				await RegistrationForm.Sign(this.formSignatureKey, this.formSignatureSecret);

			Xml.Append("<query xmlns='" + NamespaceRegister + "'>");
			RegistrationForm.SerializeSubmit(Xml);
			Xml.Append("</query>");

			await this.SendIqSet(e.From, Xml.ToString(), this.RegistrationResultReceived, null);
		}

		private Task CancelRegistrationForm(object _, DataForm RegistrationForm)
		{
			IqResultEventArgs e = (IqResultEventArgs)RegistrationForm.State;
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='" + NamespaceRegister + "'>");
			RegistrationForm.SerializeCancel(Xml);
			Xml.Append("</query>");

			return this.SendIqSet(e.From, Xml.ToString(), null, null);
		}

		private Task RegistrationResultReceived(object Sender, IqResultEventArgs e)
		{
			if (e.Ok)
			{
				this.authenticationMethod = null;
				return this.StartAuthentication();
			}
			else
				return this.ConnectionError(e.StanzaError ?? new XmppException("Unable to register new account.", e.Response));
		}

		private Task BindResult(object Sender, IqResultEventArgs e)
		{
			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "bind")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "jid")
							{
								this.fullJid = N2.InnerText.Trim();

								int i = this.fullJid.IndexOf('/');
								if (i > 0)
								{
									this.resource = this.fullJid.Substring(i + 1);

									i = this.resource.IndexOf(' ');
									if (i > 0)
									{
										this.resource = this.resource.Substring(i + 1).TrimStart();
										this.fullJid = this.bareJid + "/" + this.resource;
									}
								}

								return this.AdvanceUntilConnected();
							}
						}
					}
				}
			}

			return this.ConnectionError(e.StanzaError ?? new XmppException("Unable to bind the connection.", e.Response));
		}

		private Task SessionResult(object Sender, IqResultEventArgs e)
		{
			if (e.Ok)
				return this.AdvanceUntilConnected();
			else
				return this.ConnectionError(e.StanzaError ?? new XmppException("Unable to create session.", e.Response));
		}

		/// <summary>
		/// Changes the password of the current user.
		/// </summary>
		/// <param name="NewPassword">New password.</param>
		public Task ChangePassword(string NewPassword)
		{
			return this.ChangePassword(NewPassword, null, null);
		}

		/// <summary>
		/// Changes the password of the current user.
		/// </summary>
		/// <param name="NewPassword">New password.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task ChangePassword(string NewPassword, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='" + NamespaceRegister + "'><username>");
			Xml.Append(XML.Encode(this.userName));
			Xml.Append("</username><password>");
			Xml.Append(XML.Encode(NewPassword));
			Xml.Append("</password></query>");

			return this.SendIqSet(string.Empty, Xml.ToString(), this.ChangePasswordResult, new object[] { NewPassword, true, Callback, State });
		}

		private async Task ChangePasswordResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			string NewPassword = (string)P[0];
			bool FirstAttempt = (bool)P[1];
			EventHandlerAsync<IqResultEventArgs> Callback = (EventHandlerAsync<IqResultEventArgs>)P[2];
			object State = P[3];

			if (e.Ok)
			{
				this.password = NewPassword;

				// TODO: Also update hash and hash method

				this.Information("OnPasswordChanged()");
				await this.OnPasswordChanged.Raise(this, EventArgs.Empty);
			}
			else
			{
				if (e.ErrorType == ErrorType.Modify)
				{
					foreach (XmlNode N in e.Response.ChildNodes)
					{
						if (N.LocalName == "query" && N.NamespaceURI == NamespaceRegister)
						{
							foreach (XmlNode N2 in N.ChildNodes)
							{
								if (N2.LocalName == "x" && N2.NamespaceURI == NamespaceData)
								{
									DataForm Form = new DataForm(this, (XmlElement)N2, this.SubmitChangePasswordForm, this.CancelChangePasswordForm,
										e.From, e.To)
									{
										State = e
									};

									Field Field = Form["username"];
									if (!(Field is null))
										await Field.SetValue(this.userName);

									Field = Form["old_password"];
									if (!(Field is null))
										await Field.SetValue(this.password);

									Field = Form["password"];
									if (!(Field is null))
										await Field.SetValue(NewPassword);

									this.Information("OnChangePasswordForm()");
									EventHandlerAsync<DataForm> h = this.OnChangePasswordForm;
									if (!(h is null))
									{
										await h.Raise(this, Form);
										return;
									}
									else if (FirstAttempt)
									{
										await Form.Submit();
										return;
									}
								}
							}
						}
					}
				}

				await this.Error(this, e.StanzaError);
			}

			e.State = State;
			await Callback.Raise(this, e);
		}

		private async Task SubmitChangePasswordForm(object _, DataForm RegistrationForm)
		{
			IqResultEventArgs e = (IqResultEventArgs)RegistrationForm.State;
			StringBuilder Xml = new StringBuilder();

			if (!string.IsNullOrEmpty(this.formSignatureKey) && !string.IsNullOrEmpty(this.formSignatureSecret))
				await RegistrationForm.Sign(this.formSignatureKey, this.formSignatureSecret);

			Xml.Append("<query xmlns='" + NamespaceRegister + "'>");
			RegistrationForm.SerializeSubmit(Xml);
			Xml.Append("</query>");

			await this.SendIqSet(e.From, Xml.ToString(), this.ChangePasswordResult, e.State);
		}

		private Task CancelChangePasswordForm(object _, DataForm RegistrationForm)
		{
			IqResultEventArgs e = (IqResultEventArgs)RegistrationForm.State;
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='" + NamespaceRegister + "'>");
			RegistrationForm.SerializeCancel(Xml);
			Xml.Append("</query>");

			return this.SendIqSet(e.From, Xml.ToString(), null, null);
		}

		/// <summary>
		/// Event raised when a change password form is shown during password change.
		/// </summary>
		public event EventHandlerAsync<DataForm> OnChangePasswordForm = null;

		/// <summary>
		/// Event raised when password has been changed.
		/// </summary>
		public event EventHandlerAsync OnPasswordChanged = null;

		/// <summary>
		/// If the roster should be automatically fetched on startup or not.
		/// </summary>
		public bool RequestRosterOnStartup
		{
			get => this.requestRosterOnStartup;
			set => this.requestRosterOnStartup = value;
		}

		private async Task AdvanceUntilConnected()
		{
			if (this.createSession)
			{
				this.createSession = false;
				await this.SetState(XmppState.RequestingSession);
				await this.SendIqSet(string.Empty, "<session xmlns='urn:ietf:params:xml:ns:xmpp-session'/>", this.SessionResult, null);
			}
			else if (!this.hasRoster && this.requestRosterOnStartup)
			{
				await this.SetState(XmppState.FetchingRoster);
				await this.SendIqGet(string.Empty, "<query xmlns='" + NamespaceRoster + "'/>", this.RosterResult, null);
			}
			else if (!this.presenceSent)
			{
				EventHandlerAsync h = this.OnConnectionPresence;

				await this.SetState(XmppState.SettingPresence);
				if (h is null)
					await this.SetPresence(this.currentAvailability, this.customPresenceStatus);
				else
				{
					this.presenceSent = true;

					try
					{
						await h(this, EventArgs.Empty);
					}
					catch (Exception ex)
					{
						await this.ConnectionError(ex);
					}

					await this.AdvanceUntilConnected();
				}
			}
			else
			{
				await this.SetState(XmppState.Connected);
				this.supportsPing = true;

				this.secondTimer?.Dispose();
				this.secondTimer = null;

				this.secondTimer = new Timer(this.SecondTimerCallback, null, 1000, 1000);
			}
		}

		/// <summary>
		/// Event that is raised when it is time to send a presence stanza at the end of a connection event.
		/// If this event handler is not provided, a normal online presence stanza is sent.
		/// </summary>
		public event EventHandlerAsync OnConnectionPresence = null;

		private async Task RosterResult(object Client, IqResultEventArgs e)
		{
			RosterItem Item;

			this.hasRoster = true;

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query" && N.NamespaceURI == NamespaceRoster)
					{
						lock (this.synchObject)
						{
							Dictionary<string, RosterItem> OldRoster = new Dictionary<string, RosterItem>();

							foreach (KeyValuePair<string, RosterItem> P in this.roster)
								OldRoster[P.Key] = P.Value;

							this.roster.Clear();

							foreach (XmlNode N2 in N.ChildNodes)
							{
								if (N2.LocalName == "item")
								{
									Item = new RosterItem((XmlElement)N2, OldRoster);
									this.roster[Item.BareJid] = Item;
								}
							}
						}
					}
				}
			}
			else
				await this.Error(this, e.StanzaError ?? new XmppException("Unable to fetch roster.", e.Response));

			await this.AdvanceUntilConnected();
		}

		/// <summary>
		/// Access to the roster in the client.
		/// 
		/// To add or update a roster item, simply set the corresponding property value. To remove a roster item, set it to null.
		/// </summary>
		/// <param name="BareJID">Bare JID of roster item.</param>
		/// <returns>Roster item, if found, or null, if not available.</returns>
		/// <exception cref="ArgumentException">If updating a roster item with an item that doesn't have the same bare JID as <paramref name="BareJID"/>.</exception>
		public RosterItem this[string BareJID]
		{
			get
			{
				return this.GetRosterItem(BareJID);
			}

			set
			{
				if (value is null)
					this.RemoveRosterItem(BareJID, null, null);
				else if (BareJID != value.BareJid)
					throw new ArgumentException("Bare JIDs don't match.", nameof(BareJID));
				else
					this.AddRosterItem(value, null, null);
			}
		}

		/// <summary>
		/// Gets a roster item.
		/// </summary>
		/// <param name="BareJID">Bare JID of roster item.</param>
		/// <returns>Roster item, if found, or null, if not available.</returns>
		public RosterItem GetRosterItem(string BareJID)
		{
			lock (this.synchObject)
			{
				return this.GetRosterItemLocked(BareJID);
			}
		}

		private RosterItem GetRosterItemLocked(string BareJID)
		{
			if (this.roster.TryGetValue(BareJID, out RosterItem RosterItem))
				return RosterItem;
			else
				return null;
		}

		/// <summary>
		/// Adds an item to the roster. If an item with the same Bare JID is found in the roster, that item is updated.
		/// </summary>
		/// <param name="Item">Item to add.</param>
		public Task AddRosterItem(RosterItem Item)
		{
			return this.AddRosterItem(Item, null, null);
		}

		/// <summary>
		/// Adds an item to the roster. If an item with the same Bare JID is found in the roster, that item is updated.
		/// </summary>
		/// <param name="Item">Item to add.</param>
		/// <param name="Callback">Callback method to call, when roster has been updated. Can be null.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task AddRosterItem(RosterItem Item, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			lock (this.synchObject)
			{
				if (this.roster.TryGetValue(this.BareJID, out RosterItem RosterItem))
				{
					Item.PendingSubscription = RosterItem.PendingSubscription;
					Item.State = RosterItem.State;
				}

				this.roster[this.BareJID] = Item;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceRoster);
			Xml.Append("'>");

			Item.Serialize(Xml);

			Xml.Append("</query>");

			return this.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Updates an item in the roster.
		/// </summary>
		/// <param name="BareJID">Bare JID of the roster item.</param>
		/// <param name="Name">New name for the item.</param>
		/// <param name="Groups">Set of groups assigned to the item.</param>
		/// <exception cref="ArgumentException">If there is no roste item available with the corresponding bare JID.</exception>
		public Task UpdateRosterItem(string BareJID, string Name, params string[] Groups)
		{
			return this.UpdateRosterItem(BareJID, Name, Groups, null, null);
		}

		/// <summary>
		/// Updates an item in the roster.
		/// </summary>
		/// <param name="BareJID">Bare JID of the roster item.</param>
		/// <param name="Name">New name for the item.</param>
		/// <param name="Groups">Set of groups assigned to the item.</param>
		/// <param name="Callback">Callback method to call, when roster has been updated. Can be null.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <exception cref="ArgumentException">If there is no roster item available with the corresponding bare JID.</exception>
		public Task UpdateRosterItem(string BareJID, string Name, string[] Groups, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			RosterItem RosterItem;

			lock (this.synchObject)
			{
				if (!this.roster.TryGetValue(BareJID, out RosterItem))
					throw new ArgumentException("A Roster Item with that bare JID was not found.", nameof(BareJID));

				RosterItem.Name = Name;
				RosterItem.Groups = Groups;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceRoster);
			Xml.Append("'>");

			RosterItem.Serialize(Xml);

			Xml.Append("</query>");

			return this.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Removes an item from the roster.
		/// </summary>
		/// <param name="BareJID">Bare JID of the roster item.</param>
		/// <exception cref="ArgumentException">If there is no roster item available with the corresponding bare JID.</exception>
		public Task RemoveRosterItem(string BareJID)
		{
			return this.RemoveRosterItem(BareJID, null, null);
		}

		/// <summary>
		/// Removes an item from the roster.
		/// </summary>
		/// <param name="BareJID">Bare JID of the roster item.</param>
		/// <param name="Callback">Callback method to call, when roster has been updated. Can be null.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <exception cref="ArgumentException">If there is no roster item available with the corresponding bare JID.</exception>
		public Task RemoveRosterItem(string BareJID, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			lock (this.synchObject)
			{
				if (!this.roster.Remove(BareJID))
					throw new ArgumentException("A Roster Item with that bare JID was not found.", nameof(BareJID));
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceRoster);
			Xml.Append("'><item jid='");
			Xml.Append(XML.Encode(BareJID));
			Xml.Append("' subscription='remove'/></query>");

			return this.SendIqSet(string.Empty, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// If the roster has been fetched.
		/// </summary>
		public bool HasRoster => this.hasRoster;

		/// <summary>
		/// Items in the roster.
		/// </summary>
		public RosterItem[] Roster
		{
			get
			{
				RosterItem[] Result;

				lock (this.synchObject)
				{
					Result = new RosterItem[this.roster.Count];
					this.roster.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		/// <summary>
		/// Received presence subscription requests.
		/// </summary>
		public PresenceEventArgs[] SubscriptionRequests
		{
			get
			{
				PresenceEventArgs[] Result;

				lock (this.synchObject)
				{
					Result = new PresenceEventArgs[this.subscriptionRequests.Count];
					this.subscriptionRequests.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		/// <summary>
		/// Gets a presence subscription request
		/// </summary>
		/// <param name="BareJID">Bare JID of sender.</param>
		/// <returns>Subscription request, if found, or null, if not available.</returns>
		public PresenceEventArgs GetSubscriptionRequest(string BareJID)
		{
			lock (this.synchObject)
			{
				if (this.subscriptionRequests.TryGetValue(BareJID, out PresenceEventArgs SubscriptionRequest))
					return SubscriptionRequest;
				else
					return null;
			}
		}

		/// <summary>
		/// Event raised when presence is set. Allows custom presence XML to be inserted into presence stanza.
		/// </summary>
		public event EventHandlerAsync<CustomPresenceEventArgs> CustomPresenceXml = null;

		/// <summary>
		/// Adds custom presence XML to a presence stanza being built.
		/// </summary>
		/// <param name="Availability">Client availability</param>
		/// <param name="Xml">XML of stanza being built.</param>
		public async Task AddCustomPresenceXml(Availability Availability, StringBuilder Xml)
		{
			await this.CustomPresenceXml.Raise(this, new CustomPresenceEventArgs(Availability, Xml), false);

			lock (this.synchObject)
			{
				Xml.Append("<c xmlns='");
				Xml.Append(NamespaceEntityCapabilities);
				Xml.Append("' hash='");

				switch (this.entityHashFunction)
				{
					case HashFunction.MD5:
						Xml.Append("md5");
						break;

					case HashFunction.SHA1:
						Xml.Append("sha-1");
						break;

					case HashFunction.SHA256:
						Xml.Append("sha-256");
						break;

					case HashFunction.SHA384:
						Xml.Append("sha-384");
						break;

					case HashFunction.SHA512:
						Xml.Append("sha-512");
						break;
				}

				Xml.Append("' node='");
				Xml.Append(XML.Encode(this.entityNode));
				Xml.Append("' ver='");
				Xml.Append(XML.Encode(this.EntityCapabilitiesVersion));
				Xml.Append("'/>");
			}
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// Add a <see cref="CustomPresenceXml"/> event handler to add custom presence XML to the stanza.
		/// </summary>
		public Task SetPresence()
		{
			return this.SetPresence(Availability.Online, null, null);
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// Add a <see cref="CustomPresenceXml"/> event handler to add custom presence XML to the stanza.
		/// </summary>
		/// <param name="Availability">Client availability.</param>
		public Task SetPresence(Availability Availability)
		{
			return this.SetPresence(Availability, null, null);
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// Add a <see cref="CustomPresenceXml"/> event handler to add custom presence XML to the stanza.
		/// </summary>
		/// <param name="Availability">Client availability.</param>
		/// <param name="Status">Custom Status message, defined as a set of (language,text) pairs.</param>
		public Task SetPresence(Availability Availability, params KeyValuePair<string, string>[] Status)
		{
			return this.SetPresence(Availability, null, null, Status);
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// Add a <see cref="CustomPresenceXml"/> event handler to add custom presence XML to the stanza.
		/// </summary>
		/// <param name="Availability">Client availability.</param>
		/// <param name="Callback">Method to call when stanza has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Status">Custom Status message, defined as a set of (language,text) pairs.</param>
		public async Task SetPresence(Availability Availability, EventHandlerAsync<DeliveryEventArgs> Callback, 
			object State, params KeyValuePair<string, string>[] Status)
		{
			this.currentAvailability = Availability;
			this.customPresenceStatus = Status;

			if (this.state == XmppState.Connected || this.state == XmppState.SettingPresence)
			{
				StringBuilder Xml = new StringBuilder();

				switch (Availability)
				{
					case Availability.Online:
					default:
						Xml.Append("<presence>");
						break;

					case Availability.Away:
						Xml.Append("<presence><show>away</show>");
						break;

					case Availability.Chat:
						Xml.Append("<presence><show>chat</show>");
						break;

					case Availability.DoNotDisturb:
						Xml.Append("<presence><show>dnd</show>");
						break;

					case Availability.ExtendedAway:
						Xml.Append("<presence><show>xa</show>");
						break;

					case Availability.Offline:
						Xml.Append("<presence type='unavailable'>");
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

				if (Availability != Availability.Offline)
					await this.AddCustomPresenceXml(Availability, Xml);

				Xml.Append("</presence>");

				if (Callback is null)
					Callback = this.PresenceSent;

				await this.BeginWrite(Xml.ToString(), Callback, State);
			}
		}

		/// <summary>
		/// Sets the presence of the connection.
		/// Add a <see cref="CustomPresenceXml"/> event handler to add custom presence XML to the stanza.
		/// </summary>
		/// <param name="Availability">Client availability.</param>
		/// <param name="Status">Custom Status message, defined as a set of (language,text) pairs.</param>
		/// <returns>Task object that finishes when stanza has been sent.</returns>
		public async Task SetPresenceAsync(Availability Availability, params KeyValuePair<string, string>[] Status)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.SetPresence(Availability, (Sender, e) =>
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			}, null, Status);

			await Result.Task;
		}

		private async Task PresenceSent(object Sender, EventArgs e)
		{
			try
			{
				if (!this.presenceSent)
				{
					this.presenceSent = true;
					await this.AdvanceUntilConnected();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Requests subscription of presence information from a contact.
		/// </summary>
		/// <param name="BareJid">Bare JID of contact.</param>
		public Task RequestPresenceSubscription(string BareJid)
		{
			return this.RequestPresenceSubscription(BareJid, string.Empty);
		}

		/// <summary>
		/// Generates custom XML for embedding a nickname, as defined in XEP-0172. Can be used with 
		/// <see cref="RequestPresenceSubscription(string, string)"/> for instance, to inform the recipient of your
		/// nickname, as well as your Bare JID.
		/// </summary>
		/// <param name="NickName">Nickname.</param>
		/// <returns>Custom XML</returns>
		public static string EmbedNickName(string NickName)
		{
			if (string.IsNullOrEmpty(NickName))
				return string.Empty;
			else
				return "<nick xmlns='http://jabber.org/protocol/nick'>" + XML.Encode(NickName) + "</nick>";
		}

		/// <summary>
		/// Requests subscription of presence information from a contact.
		/// </summary>
		/// <param name="BareJid">Bare JID of contact.</param>
		/// <param name="CustomXml">Custom XML to include in the subscription request. Use with <see cref="EmbedNickName(string)"/> to
		/// include a nickname in the presence subscription.</param>
		public Task RequestPresenceSubscription(string BareJid, string CustomXml)
		{
			RosterItem Item = this.GetRosterItem(BareJid);
			if (!(Item is null))
				Item.PendingSubscription = PendingSubscription.Subscribe;

			StringBuilder Xml = new StringBuilder();
			uint SeqNr;

			lock (this.synchObject)
			{
				SeqNr = this.seqnr++;
			}

			Xml.Append("<presence id='");
			Xml.Append(SeqNr.ToString());
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));
			Xml.Append("' type='subscribe'");
			if (string.IsNullOrEmpty(CustomXml))
				Xml.Append("/>");
			else
			{
				if (!XML.IsValidXml(CustomXml, true, true, true, true, false, false))
					throw new ArgumentException("Not valid XML.", nameof(CustomXml));

				Xml.Append(">");
				Xml.Append(CustomXml);
				Xml.Append("</presence>");
			}

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		/// <summary>
		/// Requests unssubscription of presence information from a contact.
		/// </summary>
		/// <param name="BareJid">Bare JID of contact.</param>
		public Task RequestPresenceUnsubscription(string BareJid)
		{
			RosterItem Item = this.GetRosterItem(BareJid);
			if (!(Item is null))
				Item.PendingSubscription = PendingSubscription.Unsubscribe;

			StringBuilder Xml = new StringBuilder();
			uint SeqNr;

			lock (this.synchObject)
			{
				SeqNr = this.seqnr++;
			}

			Xml.Append("<presence id='");
			Xml.Append(SeqNr.ToString());
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));
			Xml.Append("' type='unsubscribe'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		/// <summary>
		/// Requests a previous presence subscription request revoked.
		/// </summary>
		/// <param name="BareJid">Bare JID of contact.</param>
		public Task RequestRevokePresenceSubscription(string BareJid)
		{
			StringBuilder Xml = new StringBuilder();
			uint SeqNr;

			lock (this.synchObject)
			{
				SeqNr = this.seqnr++;
			}

			Xml.Append("<presence id='");
			Xml.Append(SeqNr.ToString());
			Xml.Append("' to='");
			Xml.Append(XML.Encode(BareJid));
			Xml.Append("' type='unsubscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceSubscriptionAccepted(string Id, string BareJid)
		{
			lock (this.synchObject)
			{
				this.subscriptionRequests.Remove(BareJid);
			}

			RosterItem Item = this.GetRosterItem(BareJid);
			if (!(Item is null))
			{
				switch (Item.State)
				{
					case SubscriptionState.None:
						Item.State = SubscriptionState.From;
						break;

					case SubscriptionState.To:
						Item.State = SubscriptionState.Both;
						break;
				}
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='subscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceSubscriptionDeclined(string Id, string BareJid)
		{
			lock (this.synchObject)
			{
				this.subscriptionRequests.Remove(BareJid);
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='unsubscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceUnsubscriptionAccepted(string Id, string BareJid)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence to='");
			Xml.Append(XML.Encode(BareJid));

			if (!string.IsNullOrEmpty(Id))
			{
				Xml.Append("' id='");
				Xml.Append(XML.Encode(Id));
			}

			Xml.Append("' type='unsubscribed'/>");

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		internal Task PresenceUnsubscriptionDeclined(string Id, string BareJid)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence to='");
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
		/// Sends a directed presence stanza to a recipient.
		/// </summary>
		/// <param name="Type">Presence type.</param>
		/// <param name="To">JID of recipient.</param>
		/// <param name="CustomXml">Custom XML to include in the presence stanza.</param>
		public Task SendDirectedPresence(string Type, string To, string CustomXml)
		{
			return this.SendDirectedPresence(Type, To, CustomXml, null, null);
		}

		/// <summary>
		/// Sends a directed presence stanza to a recipient.
		/// </summary>
		/// <param name="Type">Presence type.</param>
		/// <param name="To">JID of recipient.</param>
		/// <param name="CustomXml">Custom XML to include in the presence stanza.</param>
		/// <param name="Callback">Method to call when a response is returned.</param>
		/// <param name="State">State object, to pass on to callback method.</param>
		public Task SendDirectedPresence(string Type, string To, string CustomXml, EventHandlerAsync<PresenceEventArgs> Callback, object State)
		{
			return this.SendDirectedPresence(Type, To, CustomXml, Callback, State,
				this.defaultRetryTimeout, this.defaultNrRetries,
				this.defaultDropOff, this.defaultMaxRetryTimeout);
		}

		/// <summary>
		/// Sends a directed presence stanza to a recipient.
		/// </summary>
		/// <param name="Type">Presence type.</param>
		/// <param name="To">JID of recipient.</param>
		/// <param name="CustomXml">Custom XML to include in the presence stanza.</param>
		/// <param name="Callback">Method to call when a response is returned.</param>
		/// <param name="State">State object, to pass on to callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		public Task SendDirectedPresence(string Type, string To, string CustomXml, EventHandlerAsync<PresenceEventArgs> Callback, object State,
			int RetryTimeout, int NrRetries, bool DropOff, int MaxRetryTimeout)
		{
			PendingRequest PendingRequest;
			DateTime TP;
			uint SeqNr;

			lock (this.synchObject)
			{
				do
				{
					SeqNr = this.seqnr++;
				}
				while (this.pendingRequestsBySeqNr.ContainsKey(SeqNr));

				if (!(Callback is null))
				{
					PendingRequest = new PendingRequest(SeqNr, Callback, State, RetryTimeout, NrRetries, DropOff, MaxRetryTimeout, To);
					TP = PendingRequest.Timeout;

					while (this.pendingRequestsByTimeout.ContainsKey(TP))
						TP = TP.AddTicks(this.gen.Next(1, 10));

					PendingRequest.Timeout = TP;

					this.pendingRequestsBySeqNr[SeqNr] = PendingRequest;
					this.pendingRequestsByTimeout[TP] = PendingRequest;
				}
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<presence id='");
			Xml.Append(SeqNr.ToString());

			if (!string.IsNullOrEmpty(Type))
			{
				Xml.Append("' type='");
				Xml.Append(XML.Encode(Type));
			}

			Xml.Append("' to='");
			Xml.Append(XML.Encode(To));
			Xml.Append("'");
			if (string.IsNullOrEmpty(CustomXml))
				Xml.Append("/>");
			else
			{
				if (!XML.IsValidXml(CustomXml, true, true, true, true, false, false))
					throw new ArgumentException("Not valid XML.", nameof(CustomXml));

				Xml.Append(">");
				Xml.Append(CustomXml);
				Xml.Append("</presence>");
			}

			return this.BeginWrite(Xml.ToString(), null, null);
		}

		/// <summary>
		/// Sends a directed presence stanza to a recipient.
		/// </summary>
		/// <param name="Type">Presence type.</param>
		/// <param name="To">JID of recipient.</param>
		/// <param name="CustomXml">Custom XML to include in the presence stanza.</param>
		public async Task SendDirectedPresenceAsync(string Type, string To, string CustomXml)
		{
			TaskCompletionSource<PresenceEventArgs> Query = new TaskCompletionSource<PresenceEventArgs>();

			await this.SendDirectedPresence(Type, To, CustomXml, (Sender, e) =>
			{
				Query.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			await Query.Task;
		}

		private async Task RosterPushHandler(object Sender, IqEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.From))
				return;

			RosterItem Item = null;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "item" && E.NamespaceURI == NamespaceRoster)
				{
					lock (this.synchObject)
					{
						Item = new RosterItem(E, this.roster);
					}

					break;
				}
			}

			if (Item is null)
				throw new BadRequestException(string.Empty, e.Query);

			EventHandlerAsync<RosterItem> h;

			await this.SendIqResult(e.Id, e.From, string.Empty);

			lock (this.synchObject)
			{
				if (Item.State == SubscriptionState.Remove)
				{
					this.roster.Remove(Item.BareJid);
					this.Information("OnRosterItemRemoved()");
					h = this.OnRosterItemRemoved;
				}
				else
				{
					if (this.roster.TryGetValue(Item.BareJid, out RosterItem Prev))
					{
						if (Item.Equals(Prev))
						{
							h = null;
							this.Information("Roster item identical to previous.");
						}
						else
						{
							this.roster[Item.BareJid] = Item;
							this.Information("OnRosterItemUpdated()");
							h = this.OnRosterItemUpdated;
						}
					}
					else
					{
						this.Information("OnRosterItemAdded()");
						h = this.OnRosterItemAdded;
						this.roster[Item.BareJid] = Item;
					}
				}
			}

			await h.Raise(this, Item);
		}

		/// <summary>
		/// Event raised when an item has been added to the roster.
		/// </summary>
		public event EventHandlerAsync<RosterItem> OnRosterItemAdded = null;

		/// <summary>
		/// Event raised when an item has been updated in the roster.
		/// </summary>
		public event EventHandlerAsync<RosterItem> OnRosterItemUpdated = null;

		/// <summary>
		/// Event raised when an item has been removed from the roster.
		/// </summary>
		public event EventHandlerAsync<RosterItem> OnRosterItemRemoved = null;

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		public Task SendChatMessage(string To, string Body)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, To, string.Empty,
				Body, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		public Task SendChatMessage(string To, string Body, string Subject)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, To, string.Empty,
				Body, Subject, string.Empty, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		public Task SendChatMessage(string To, string Body, string Subject, string Language)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, To, string.Empty,
				Body, Subject, Language, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		public Task SendChatMessage(string To, string Body, string Subject, string Language, string ThreadId)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, To, string.Empty,
				Body, Subject, Language, ThreadId, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="To">Destination address</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public Task SendChatMessage(string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, string.Empty, To, string.Empty,
				Body, Subject, Language, ThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		public Task SendMessage(MessageType Type, string To, string CustomXml, string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId)
		{
			return this.SendMessage(QoSLevel.Unacknowledged, Type, string.Empty, To, CustomXml,
				Body, Subject, Language, ThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a simple chat message
		/// </summary>
		/// <param name="QoS">Quality of Service level of message.</param>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		/// <param name="DeliveryCallback">Callback to call when message has been sent, or failed to be sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public Task SendMessage(QoSLevel QoS, MessageType Type, string To, string CustomXml, string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			return this.SendMessage(QoS, Type, string.Empty, To, CustomXml, Body, Subject, Language, ThreadId,
				ParentThreadId, DeliveryCallback, State);
		}

		/// <summary>
		/// Sends a message
		/// </summary>
		/// <param name="QoS">Quality of Service level of message.</param>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="Id">Message ID</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		/// <param name="DeliveryCallback">Callback to call when message has been sent, or failed to be sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task SendMessage(QoSLevel QoS, MessageType Type, string Id, string To, string CustomXml, string Body, string Subject, string Language, string ThreadId,
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
				if (!string.IsNullOrEmpty(To))
				{
					Xml.Append(" to='");
					Xml.Append(XML.Encode(To));
					Xml.Append('\'');
				}

				if (this.sendFromAddress && !string.IsNullOrEmpty(this.fullJid))
				{
					Xml.Append(" from='");
					Xml.Append(XML.Encode(this.fullJid));
					Xml.Append('\'');
				}
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
					await this.BeginWrite(MessageXml, async (Sender, e) => await this.CallDeliveryCallback(DeliveryCallback, State, true), State);
					break;

				case QoSLevel.Acknowledged:
					Xml.Clear();
					Xml.Append("<qos:acknowledged xmlns:qos='");
					Xml.Append(NamespaceQualityOfServiceCurrent);
					Xml.Append("'>");
					Xml.Append(MessageXml);
					Xml.Append("</qos:acknowledged>");

					await this.SendIqSet(To, Xml.ToString(), async (Sender, e) => await this.CallDeliveryCallback(DeliveryCallback, State, e.Ok), null,
						15000, int.MaxValue, true, 3600000);
					break;

				case QoSLevel.Assured:
					string MsgId = Hashes.BinaryToString(GetRandomBytes(16));

					Xml.Clear();
					Xml.Append("<qos:assured xmlns:qos='");
					Xml.Append(NamespaceQualityOfServiceCurrent);
					Xml.Append("' msgId='");
					Xml.Append(MsgId);
					Xml.Append("'>");
					Xml.Append(MessageXml);
					Xml.Append("</qos:assured>");

					await this.SendIqSet(To, Xml.ToString(), this.AssuredDeliveryStep, new object[] { DeliveryCallback, State, MsgId },
						15000, int.MaxValue, true, 3600000);
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
							Xml.Append(NamespaceQualityOfServiceCurrent);
							Xml.Append("' msgId='");
							Xml.Append(MsgId);
							Xml.Append("'/>");

							await this.SendIqSet(e.From, Xml.ToString(),
								async (sender, e2) => await this.CallDeliveryCallback(DeliveryCallback, State, e2.Ok),
								null, 15000, int.MaxValue, true, 3600000);
							return;
						}
					}
				}
			}

			await this.CallDeliveryCallback(DeliveryCallback, State, false);
		}

		private Task CallDeliveryCallback(EventHandlerAsync<DeliveryEventArgs> Callback, object State, bool Ok)
		{
			if (Callback is null)
				return Task.CompletedTask;  // Do not call Raise. null Callbacks are common, and should not result in a warning in sniffers.
			else
				return Callback.Raise(this, new DeliveryEventArgs(State, Ok));
		}

		private async Task DynamicFormUpdatedHandler(object Sender, MessageEventArgs e)
		{
			DataForm Form = null;
			string SessionVariable = XML.Attribute(e.Content, "sessionVariable");
			string Language = XML.Attribute(e.Content, "xml:lang");

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N.LocalName == "x")
				{
					Form = new DataForm(this, (XmlElement)N, null, null, e.From, e.To);
					break;
				}
			}

			if (!(Form is null))
				await this.OnDynamicFormUpdated.Raise(this, new DynamicDataFormEventArgs(Form, SessionVariable, Language, e));
		}

		/// <summary>
		/// Event raised when a dynamic for has been updated. Dynamic forms have to be joined to the previous form 
		/// using the <see cref="DataForm.Join"/> method on the old form. The old form is identified using
		/// <see cref="DynamicDataFormEventArgs.SessionVariable"/>.
		/// </summary>
		public event EventHandlerAsync<DynamicDataFormEventArgs> OnDynamicFormUpdated = null;

		private Task ServiceDiscoveryRequestHandler(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceServiceDiscoveryInfo);

			if (e.Query.HasAttribute("node"))
			{
				Xml.Append("' node='");
				Xml.Append(this.entityNode);
				Xml.Append('#');
				Xml.Append(this.EntityCapabilitiesVersion);
			}

			Xml.Append("'><identity category='client' type='pc' name='");
			Xml.Append(XML.Encode(this.clientName));

			if (!string.IsNullOrEmpty(this.language))
			{
				Xml.Append("' xml:lang='");
				Xml.Append(XML.Encode(this.language));
			}

			Xml.Append("'/>");

			lock (this.synchObject)
			{
				foreach (string Feature in this.clientFeatures.Keys)
				{
					Xml.Append("<feature var='");
					Xml.Append(XML.Encode(Feature));
					Xml.Append("'/>");
				}

				foreach (DataForm Form in this.extendedServiceDiscoveryInformation.Values)
					Form.SerializeResult(Xml);
			}

			Xml.Append("</query>");

			return e.IqResult(Xml.ToString());
		}

		/// <summary>
		/// Returns the Entity Capabilities Version string, as defined in
		/// XEP-0115: Entity Capabilities http://xmpp.org/extensions/xep-0115.html
		/// </summary>
		public string EntityCapabilitiesVersion
		{
			get
			{
				lock (this.synchObject)
				{
					if (string.IsNullOrEmpty(this.entityCapabilitiesVersion))
					{
						StringBuilder S = new StringBuilder();

						S.Append("client/pc/");
						S.Append(this.language);
						S.Append('/');
						S.Append(this.clientName);
						S.Append('<');

						foreach (string Feature in this.clientFeatures.Keys)
						{
							S.Append(Feature);
							S.Append('<');
						}

						foreach (KeyValuePair<string, DataForm> P in this.extendedServiceDiscoveryInformation)
						{
							S.Append(P.Key);
							S.Append('<');

							SortedDictionary<string, string[]> Fields = new SortedDictionary<string, string[]>();

							foreach (Field Field in P.Value.Fields)
								Fields[Field.Var] = Field.ValueStrings;

							foreach (KeyValuePair<string, string[]> P2 in Fields)
							{
								S.Append(P2.Key);
								S.Append('<');

								foreach (string P3 in P2.Value)
								{
									S.Append(P3);
									S.Append('<');
								}
							}
						}

						byte[] Hash = Hashes.ComputeHash(this.entityHashFunction, Encoding.UTF8.GetBytes(S.ToString()));
#if WINDOWS_UWP
						this.entityCapabilitiesVersion = Convert.ToBase64String(Hash);
#else
						this.entityCapabilitiesVersion = Convert.ToBase64String(Hash);
#endif
					}

					return this.entityCapabilitiesVersion;
				}
			}
		}

		/// <summary>
		/// The hash function to use when reporting entity capabilities. Default value is 
		/// <see cref="HashFunction.SHA256"/>.
		/// </summary>
		public HashFunction EntityHashFunction
		{
			get => this.entityHashFunction;
			set
			{
				if (this.entityHashFunction != value)
				{
					lock (this.synchObject)
					{
						this.entityHashFunction = value;
						this.entityCapabilitiesVersion = null;
					}
				}
			}
		}

		/// <summary>
		/// Entity node. This parameter is reported when the client reports its entity capabilities.
		/// It should be a URL pointing to the product connecting to the XMPP network.
		/// </summary>
		public string EntityNode
		{
			get => this.entityNode;
			set => this.entityNode = value;
		}

		/// <summary>
		/// Adds extended service discovery information to the client. Such information is defined in data forms.
		/// Each form must contain a hidden FORM_TYPE field defining the contents of the information.
		/// This information can then be seen by other clients using service discovery requests, as defined in:
		/// 
		/// XEP-0030: Service Discovery: http://xmpp.org/extensions/xep-0030.html
		/// XEP-0068: Field Standardization for Data Forms: http://xmpp.org/extensions/xep-0068.html
		/// XEP-0128: Service Discovery Extensions: http://xmpp.org/extensions/xep-0128.html
		/// </summary>
		/// <param name="Information">Extended service discovery information.</param>
		public void AddExtendedServiceDiscoveryInformation(DataForm Information)
		{
			Field FormTypeField = Information["FORM_TYPE"] ?? throw new ArgumentException("Extended Service Discovery Information forms must contain a FORM_TYPE field.", nameof(Information));
			string FormType = FormTypeField.ValueString;

			lock (this.synchObject)
			{
				this.extendedServiceDiscoveryInformation[FormType] = Information;
				this.entityCapabilitiesVersion = null;
			}
		}

		/// <summary>
		/// Removes extended service discovery information, previously added by calls to
		/// <see cref="AddExtendedServiceDiscoveryInformation"/>, from the client.
		/// </summary>
		/// <param name="FormType">FORM_TYPE of the extended discovery information to remove.</param>
		/// <returns>If such information was found, and removed.</returns>
		public bool RemoveExtendedServiceDiscoveryInformation(string FormType)
		{
			lock (this.synchObject)
			{
				this.entityCapabilitiesVersion = null;
				return this.extendedServiceDiscoveryInformation.Remove(FormType);
			}
		}

		/// <summary>
		/// Sends a service discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendServiceDiscoveryRequest(string To, EventHandlerAsync<ServiceDiscoveryEventArgs> Callback, object State)
		{
			return this.SendServiceDiscoveryRequest(null, To, string.Empty, Callback, State);
		}

		/// <summary>
		/// Sends a service discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendServiceDiscoveryRequest(string To, string Node, EventHandlerAsync<ServiceDiscoveryEventArgs> Callback, object State)
		{
			return this.SendServiceDiscoveryRequest(null, To, Node, Callback, State);
		}

		/// <summary>
		/// Sends a service discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendServiceDiscoveryRequest(IEndToEndEncryption E2eEncryption, string To,
			EventHandlerAsync<ServiceDiscoveryEventArgs> Callback, object State)
		{
			return this.SendServiceDiscoveryRequest(E2eEncryption, To, string.Empty, Callback, State);
		}

		/// <summary>
		/// Sends a service discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task SendServiceDiscoveryRequest(IEndToEndEncryption E2eEncryption, string To, string Node,
			EventHandlerAsync<ServiceDiscoveryEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();
			bool CacheResponse = string.IsNullOrEmpty(Node) && (string.IsNullOrEmpty(To) || To == this.domain);

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceServiceDiscoveryInfo);

			if (string.IsNullOrEmpty(Node))
			{
				if (CacheResponse && !(this.serverFeatures is null))
				{
					ServiceDiscoveryEventArgs e2 = new ServiceDiscoveryEventArgs(this.serverFeatures, this.serverFeatures.Identities,
						this.serverFeatures.Features, this.serverFeatures.ExtendedInformation)
					{
						State = State
					};

					await Callback.Raise(this, e2);

					return;
				}
			}
			else
			{
				Xml.Append("' node='");
				Xml.Append(XML.Encode(Node));
			}

			Xml.Append("'/>");

			if (!(E2eEncryption is null))
			{
				await E2eEncryption.SendIqGet(this, E2ETransmission.NormalIfNotE2E, To, Xml.ToString(),
					this.ServiceDiscoveryResponse, new object[] { Callback, State, CacheResponse });
			}
			else
				await this.SendIqGet(To, Xml.ToString(), this.ServiceDiscoveryResponse, new object[] { Callback, State, CacheResponse });
		}

		private async Task ServiceDiscoveryResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<ServiceDiscoveryEventArgs> Callback = (EventHandlerAsync<ServiceDiscoveryEventArgs>)P[0];
			object State2 = P[1];
			bool CacheResponse = (bool)P[2];
			Dictionary<string, bool> Features = new Dictionary<string, bool>();
			Dictionary<string, DataForm> ExtendedInformation = new Dictionary<string, DataForm>();
			List<Identity> Identities = new List<Identity>();

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							switch (N2.LocalName)
							{
								case "identity":
									Identities.Add(new Identity((XmlElement)N2));
									break;

								case "feature":
									Features[XML.Attribute((XmlElement)N2, "var")] = true;
									break;

								case "x":
									DataForm Form = new DataForms.DataForm(this, (XmlElement)N2, null, null, e.From, e.To);
									Field FormType = Form["FORM_TYPE"];
									if (FormType is null)
										break;

									ExtendedInformation[FormType.ValueString] = Form;
									break;
							}
						}
					}
				}
			}

			ServiceDiscoveryEventArgs e2 = new ServiceDiscoveryEventArgs(e, Identities.ToArray(), Features, ExtendedInformation)
			{
				State = State2
			};

			if (e.Ok && CacheResponse)
				this.serverFeatures = e2;

			await Callback.Raise(this, e2);
		}

		/// <summary>
		/// Performs a synchronous service discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public ServiceDiscoveryEventArgs ServiceDiscovery(string To, int Timeout)
		{
			return this.ServiceDiscovery(null, To, string.Empty, Timeout);
		}

		/// <summary>
		/// Performs a synchronous service discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public ServiceDiscoveryEventArgs ServiceDiscovery(string To, string Node, int Timeout)
		{
			return this.ServiceDiscovery(null, To, Node, Timeout);
		}

		/// <summary>
		/// Performs a synchronous service discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public ServiceDiscoveryEventArgs ServiceDiscovery(IEndToEndEncryption E2eEncryption, string To, int Timeout)
		{
			return this.ServiceDiscovery(E2eEncryption, To, string.Empty, Timeout);
		}

		/// <summary>
		/// Performs a synchronous service discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public ServiceDiscoveryEventArgs ServiceDiscovery(IEndToEndEncryption E2eEncryption, string To, string Node, int Timeout)
		{
			Task<ServiceDiscoveryEventArgs> Result = this.ServiceDiscoveryAsync(E2eEncryption, To, Node);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous service discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public Task<ServiceDiscoveryEventArgs> ServiceDiscoveryAsync(string To)
		{
			return this.ServiceDiscoveryAsync(null, To, string.Empty);
		}

		/// <summary>
		/// Performs an asynchronous service discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public Task<ServiceDiscoveryEventArgs> ServiceDiscoveryAsync(string To, string Node)
		{
			return this.ServiceDiscoveryAsync(null, To, Node);
		}

		/// <summary>
		/// Performs an asynchronous service discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public Task<ServiceDiscoveryEventArgs> ServiceDiscoveryAsync(IEndToEndEncryption E2eEncryption, string To)
		{
			return this.ServiceDiscoveryAsync(E2eEncryption, To, string.Empty);
		}

		/// <summary>
		/// Performs an asynchronous service discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<ServiceDiscoveryEventArgs> ServiceDiscoveryAsync(IEndToEndEncryption E2eEncryption, string To, string Node)
		{
			TaskCompletionSource<ServiceDiscoveryEventArgs> Result = new TaskCompletionSource<ServiceDiscoveryEventArgs>();

			await this.SendServiceDiscoveryRequest(E2eEncryption, To, Node, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Sends a service items discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendServiceItemsDiscoveryRequest(string To, EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback, object State)
		{
			return this.SendServiceItemsDiscoveryRequest(null, To, string.Empty, Callback, State);
		}

		/// <summary>
		/// Sends a service items discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendServiceItemsDiscoveryRequest(string To, string Node, EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback, object State)
		{
			return this.SendServiceItemsDiscoveryRequest(null, To, Node, Callback, State);
		}

		/// <summary>
		/// Sends a service items discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendServiceItemsDiscoveryRequest(IEndToEndEncryption E2eEncryption, string To, EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback, object State)
		{
			return this.SendServiceItemsDiscoveryRequest(E2eEncryption, To, string.Empty, Callback, State);
		}

		/// <summary>
		/// Sends a service items discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task SendServiceItemsDiscoveryRequest(IEndToEndEncryption E2eEncryption, string To, string Node,
			EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();
			bool CacheResponse = string.IsNullOrEmpty(Node) && (string.IsNullOrEmpty(To) || To == this.domain);

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceServiceDiscoveryItems);

			if (string.IsNullOrEmpty(Node))
			{
				if (CacheResponse && !(this.serverComponents is null))
				{
					ServiceItemsDiscoveryEventArgs e2 = new ServiceItemsDiscoveryEventArgs(this.serverComponents, this.serverComponents.Items)
					{
						State = State
					};

					await Callback.Raise(this, e2);

					return;
				}
			}
			else
			{
				Xml.Append("' node='");
				Xml.Append(XML.Encode(Node));
			}

			Xml.Append("'/>");

			if (!(E2eEncryption is null))
			{
				await E2eEncryption.SendIqGet(this, E2ETransmission.NormalIfNotE2E, To, Xml.ToString(),
					this.ServiceItemsDiscoveryResponse, new object[] { Callback, State, CacheResponse });
			}
			else
			{
				await this.SendIqGet(To, Xml.ToString(), this.ServiceItemsDiscoveryResponse,
					new object[] { Callback, State, CacheResponse });
			}
		}

		private async Task ServiceItemsDiscoveryResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<ServiceItemsDiscoveryEventArgs> Callback = (EventHandlerAsync<ServiceItemsDiscoveryEventArgs>)P[0];
			object State = P[1];
			bool CacheResponse = (bool)P[2];
			List<Item> Items = new List<Item>();

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "item")
								Items.Add(new Item((XmlElement)N2));
						}
					}
				}
			}

			ServiceItemsDiscoveryEventArgs e2 = new ServiceItemsDiscoveryEventArgs(e, Items.ToArray())
			{
				State = State
			};

			if (CacheResponse && e.Ok)
				this.serverComponents = e2;

			await Callback.Raise(this, e2);
		}

		/// <summary>
		/// Performs a synchronous service items discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public ServiceItemsDiscoveryEventArgs ServiceItemsDiscovery(string To, int Timeout)
		{
			return this.ServiceItemsDiscovery(null, To, string.Empty, Timeout);
		}

		/// <summary>
		/// Performs a synchronous service items discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public ServiceItemsDiscoveryEventArgs ServiceItemsDiscovery(string To, string Node, int Timeout)
		{
			return this.ServiceItemsDiscovery(null, To, Node, Timeout);
		}

		/// <summary>
		/// Performs a synchronous service items discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public ServiceItemsDiscoveryEventArgs ServiceItemsDiscovery(IEndToEndEncryption E2eEncryption, string To, int Timeout)
		{
			return this.ServiceItemsDiscovery(E2eEncryption, To, string.Empty, Timeout);
		}

		/// <summary>
		/// Performs a synchronous service items discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public ServiceItemsDiscoveryEventArgs ServiceItemsDiscovery(IEndToEndEncryption E2eEncryption, string To,
			string Node, int Timeout)
		{
			Task<ServiceItemsDiscoveryEventArgs> Result = this.ServiceItemsDiscoveryAsync(E2eEncryption, To, Node);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous service items discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public Task<ServiceItemsDiscoveryEventArgs> ServiceItemsDiscoveryAsync(string To)
		{
			return this.ServiceItemsDiscoveryAsync(null, To, string.Empty);
		}

		/// <summary>
		/// Performs an asynchronous service items discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public Task<ServiceItemsDiscoveryEventArgs> ServiceItemsDiscoveryAsync(IEndToEndEncryption E2eEncryption, string To)
		{
			return this.ServiceItemsDiscoveryAsync(E2eEncryption, To, string.Empty);
		}

		/// <summary>
		/// Performs an asynchronous service items discovery request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public Task<ServiceItemsDiscoveryEventArgs> ServiceItemsDiscoveryAsync(string To, string Node)
		{
			return this.ServiceItemsDiscoveryAsync(null, To, Node);
		}

		/// <summary>
		/// Performs an asynchronous service items discovery request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Node">Optional node.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <exception cref="XmppException">If an IQ error is returned.</exception>
		public async Task<ServiceItemsDiscoveryEventArgs> ServiceItemsDiscoveryAsync(IEndToEndEncryption E2eEncryption, string To,
			string Node)
		{
			TaskCompletionSource<ServiceItemsDiscoveryEventArgs> Result = new TaskCompletionSource<ServiceItemsDiscoveryEventArgs>();

			await this.SendServiceItemsDiscoveryRequest(E2eEncryption, To, Node, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		private Task SoftwareVersionRequestHandler(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceSoftwareVersion);
			Xml.Append("'><name>");
			Xml.Append(XML.Encode(this.clientName));
			Xml.Append("</name><version>");
			Xml.Append(XML.Encode(this.clientVersion));
			Xml.Append("</version><os>");
			Xml.Append(XML.Encode(this.clientOS));
			Xml.Append("</os></query>");

			return e.IqResult(Xml.ToString());
		}

		/// <summary>
		/// Sends a software version request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendSoftwareVersionRequest(string To, EventHandlerAsync<SoftwareVersionEventArgs> Callback, object State)
		{
			return this.SendSoftwareVersionRequest(null, To, Callback, State);
		}

		/// <summary>
		/// Sends a software version request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendSoftwareVersionRequest(IEndToEndEncryption E2eEncryption, string To, EventHandlerAsync<SoftwareVersionEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceSoftwareVersion);
			Xml.Append("'/>");

			if (!(E2eEncryption is null))
			{
				return E2eEncryption.SendIqGet(this, E2ETransmission.NormalIfNotE2E, To, Xml.ToString(),
					this.SoftwareVersionResponse, new object[] { Callback, State });
			}
			else
				return this.SendIqGet(To, Xml.ToString(), this.SoftwareVersionResponse, new object[] { Callback, State });
		}

		private async Task SoftwareVersionResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<SoftwareVersionEventArgs> Callback = (EventHandlerAsync<SoftwareVersionEventArgs>)P[0];
			object State = P[1];
			List<Item> Items = new List<Item>();

			string Name = string.Empty;
			string Version = string.Empty;
			string OS = string.Empty;

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							switch (N2.LocalName)
							{
								case "name":
									Name = N2.InnerText;
									break;

								case "version":
									Version = N2.InnerText;
									break;

								case "os":
									OS = N2.InnerText;
									break;
							}
						}

						break;
					}
				}
			}

			SoftwareVersionEventArgs e2 = new SoftwareVersionEventArgs(e, Name, Version, OS)
			{
				State = State
			};

			await Callback.Raise(this, e2);
		}

		/// <summary>
		/// Performs a synchronous software version request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <returns>Version information.</returns>
		public SoftwareVersionEventArgs SoftwareVersion(string To, int Timeout)
		{
			return this.SoftwareVersion(null, To, Timeout);
		}

		/// <summary>
		/// Performs a synchronous software version request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <returns>Version information.</returns>
		public SoftwareVersionEventArgs SoftwareVersion(IEndToEndEncryption E2eEncryption, string To, int Timeout)
		{
			Task<SoftwareVersionEventArgs> Result = this.SoftwareVersionAsync(E2eEncryption, To);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous software version request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <returns>Version information.</returns>
		public Task<SoftwareVersionEventArgs> SoftwareVersionAsync(string To)
		{
			return this.SoftwareVersionAsync(null, To);
		}

		/// <summary>
		/// Performs an asynchronous software version request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <returns>Version information.</returns>
		public async Task<SoftwareVersionEventArgs> SoftwareVersionAsync(IEndToEndEncryption E2eEncryption, string To)
		{
			TaskCompletionSource<SoftwareVersionEventArgs> Result = new TaskCompletionSource<SoftwareVersionEventArgs>();

			await this.SendSoftwareVersionRequest(E2eEncryption, To, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Sends a search form request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="FormCallback">Method to call when search form response or error is returned.</param>
		/// <param name="ResultCallback">Method to call when search result is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendSearchFormRequest(string To, EventHandlerAsync<SearchFormEventArgs> FormCallback,
			EventHandlerAsync<SearchResultEventArgs> ResultCallback, object State)
		{
			return this.SendSearchFormRequest(null, To, FormCallback, ResultCallback, State);
		}

		/// <summary>
		/// Sends a search form request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="FormCallback">Method to call when search form response or error is returned.</param>
		/// <param name="ResultCallback">Method to call when search result is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendSearchFormRequest(IEndToEndEncryption E2eEncryption, string To,
			EventHandlerAsync<SearchFormEventArgs> FormCallback, EventHandlerAsync<SearchResultEventArgs> ResultCallback,
			object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespaceSearch);
			Xml.Append("'/>");

			if (!(E2eEncryption is null))
			{
				return E2eEncryption.SendIqGet(this, E2ETransmission.NormalIfNotE2E, To, Xml.ToString(),
					this.SearchFormResponse, new object[] { FormCallback, ResultCallback, State });
			}
			else
				return this.SendIqGet(To, Xml.ToString(), this.SearchFormResponse, new object[] { FormCallback, ResultCallback, State });
		}

		private async Task SearchFormResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<SearchFormEventArgs> FormCallback = (EventHandlerAsync<SearchFormEventArgs>)P[0];
			EventHandlerAsync<SearchResultEventArgs> ResultCallback = (EventHandlerAsync<SearchResultEventArgs>)P[1];
			object State = P[2];
			List<Item> Items = new List<Item>();

			if (!(FormCallback is null))
			{
				DataForm SearchForm = null;
				string Instructions = null;
				string First = null;
				string Last = null;
				string Nick = null;
				string EMail = null;
				bool SupportsForms = false;

				if (e.Ok)
				{
					foreach (XmlNode N in e.Response.ChildNodes)
					{
						if (N.LocalName == "query")
						{
							foreach (XmlNode N2 in N.ChildNodes)
							{
								switch (N2.LocalName)
								{
									case "instructions":
										Instructions = N2.InnerText;
										break;

									case "first":
										First = N2.InnerText;
										break;

									case "last":
										Last = N2.InnerText;
										break;

									case "nick":
										Nick = N2.InnerText;
										break;

									case "email":
										EMail = N2.InnerText;
										break;

									case "x":
										SearchForm = new DataForm(this, (XmlElement)N2, this.SubmitSearchDataForm, this.CancelSearchDataForm, e.From, e.To);
										SupportsForms = true;
										break;
								}
							}

							if (SearchForm is null)
							{
								List<Field> Fields = new List<Field>();
								string Tooltip = "Use asterisks (*) to do wildcard searches.";

								if (!(First is null))
								{
									Fields.Add(new TextSingleField(null, "first", "First name:", false, new string[] { First }, null,
										Tooltip, new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
								}

								if (!(Last is null))
								{
									Fields.Add(new TextSingleField(null, "last", "Last name:", false, new string[] { Last }, null,
										Tooltip, new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
								}

								if (!(Nick is null))
								{
									Fields.Add(new TextSingleField(null, "nick", "Nick name:", false, new string[] { Nick }, null,
										Tooltip, new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
								}

								if (!(EMail is null))
								{
									Fields.Add(new TextSingleField(null, "email", "e-Mail:", false, new string[] { EMail }, null,
										Tooltip, new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
								}

								SearchForm = new DataForm(this, this.SubmitSearchDataForm, this.CancelSearchDataForm, e.From, e.To, Fields.ToArray());

								if (!string.IsNullOrEmpty(Instructions))
									SearchForm.Instructions = new string[] { Instructions };
							}
						}
					}
				}

				SearchFormEventArgs e2 = new SearchFormEventArgs(this, e, Instructions, First, Last, Nick, EMail, SearchForm, SupportsForms)
				{
					State = State
				};

				if (!(SearchForm is null))
					SearchForm.State = new object[] { ResultCallback, State, e2 };

				await FormCallback.Raise(this, e2);
			}
		}

		private Task SubmitSearchDataForm(object _, DataForm Form)
		{
			object[] P = (object[])Form.State;
			EventHandlerAsync<SearchResultEventArgs> ResultCallback = (EventHandlerAsync<SearchResultEventArgs>)P[0];
			object State = P[1];
			SearchFormEventArgs e = (SearchFormEventArgs)P[2];

			e.SendSearchRequest(ResultCallback, State);

			return Task.CompletedTask;
		}

		private Task CancelSearchDataForm(object _, DataForm Form)
		{
			// Do nothing.

			return Task.CompletedTask;
		}

		/// <summary>
		/// Performs a synchronous search form request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public SearchFormEventArgs SearchForm(string To, int Timeout)
		{
			return this.SearchForm(null, To, Timeout);
		}

		/// <summary>
		/// Performs a synchronous search form request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public SearchFormEventArgs SearchForm(IEndToEndEncryption E2eEncryption, string To, int Timeout)
		{
			Task<SearchFormEventArgs> Result = this.SearchFormAsync(E2eEncryption, To);

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs an asynchronous search form request
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public Task<SearchFormEventArgs> SearchFormAsync(string To)
		{
			return this.SearchFormAsync(null, To);
		}

		/// <summary>
		/// Performs an asynchronous search form request
		/// </summary>
		/// <param name="E2eEncryption">Optional End-to-end encryption interface. If end-to-end encryption
		/// cannot be established, the request is sent normally.</param>
		/// <param name="To">Destination address.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public async Task<SearchFormEventArgs> SearchFormAsync(IEndToEndEncryption E2eEncryption, string To)
		{
			TaskCompletionSource<SearchFormEventArgs> Result = new TaskCompletionSource<SearchFormEventArgs>();

			await this.SendSearchFormRequest(E2eEncryption, To, (Sender, e) =>
			{
				Result.TrySetResult(e);
				return Task.CompletedTask;
			}, null, null);

			return await Result.Task;
		}

		internal static string Concat(params string[] Rows)
		{
			if (Rows is null)
				return string.Empty;

			StringBuilder sb = null;

			foreach (string s in Rows)
			{
				if (sb is null)
					sb = new StringBuilder(s);
				else
				{
					sb.AppendLine();
					sb.Append(s);
				}
			}

			if (sb is null)
				return string.Empty;
			else
				return sb.ToString();
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

					await this.SendIqResult(e.Id, e.From, string.Empty);
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
		/// 0 = no retries.
		/// </summary>
		public int DefaultNrRetries
		{
			get => this.defaultNrRetries;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value cannot be negative.", nameof(this.DefaultNrRetries));

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

		private Task AssuredQoSMessageHandler(object Sender, IqEventArgs e)
		{
			string FromBareJid = GetBareJID(e.From);
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
								" pending assured messages.", GetBareJID(e.To), GetBareJID(e.From), "ResourceConstraint",
								new KeyValuePair<string, object>("Variable", "NrAssuredMessagesPending"),
								new KeyValuePair<string, object>("Limit", (double)this.maxAssuredMessagesPendingTotal),
								new KeyValuePair<string, object>("Unit", string.Empty));

							throw new StanzaErrors.ResourceConstraintException(string.Empty, e.Query);
						}

						if (!this.roster.ContainsKey(FromBareJid))
						{
							Log.Notice("Rejected incoming assured message. Sender not in roster.", GetBareJID(e.To), GetBareJID(e.From), "NotAllowed",
								new KeyValuePair<string, object>("Variable", "NrAssuredMessagesPending"));

							throw new NotAllowedException(string.Empty, e.Query);
						}

						if (this.pendingAssuredMessagesPerSource.TryGetValue(FromBareJid, out int i))
						{
							if (i >= this.maxAssuredMessagesPendingFromSource)
							{
								Log.Warning("Rejected incoming assured message. Unable to manage more than " + this.maxAssuredMessagesPendingFromSource.ToString() +
									" pending assured messages from each sender.", GetBareJID(e.To), GetBareJID(e.From), "ResourceConstraint",
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

					return this.SendIqResult(e.Id, e.From, "<received msgId='" + XML.Encode(MsgId) + "'/>");
				}
			}

			throw new BadRequestException(string.Empty, e.Query);
		}

		/// <summary>
		/// Gets the Bare JID from a JID, which may be a Full JID.
		/// </summary>
		/// <param name="JID">JID</param>
		/// <returns>Bare JID</returns>
		public static string GetBareJID(string JID)
		{
			int i = JID.IndexOf('/');
			if (i > 0)
				return JID.Substring(0, i);
			else
				return JID;
		}

		/// <summary>
		/// Gets the resource part of a full JID. If no resource part is available, the empty string is returned.
		/// </summary>
		/// <param name="JID">Full JID</param>
		/// <returns>Resource part, or empty string if no resource part.</returns>
		public static string GetResource(string JID)
		{
			int i = JID.IndexOf('/');
			if (i < 0)
				return string.Empty;
			else
				return JID.Substring(i + 1);
		}

		/// <summary>
		/// Gets the domain part of a JID.
		/// </summary>
		/// <param name="JID">Any JID</param>
		/// <returns>Domain part.</returns>
		public static string GetDomain(string JID)
		{
			int i = JID.IndexOf('/');
			if (i >= 0)
				JID = JID.Substring(0, i);

			i = JID.IndexOf('@');
			if (i >= 0)
				JID = JID.Substring(i + 1);

			return JID;
		}

		/// <summary>
		/// Gets the account part of a JID.
		/// </summary>
		/// <param name="JID">Any JID</param>
		/// <returns>Account part.</returns>
		public static string GetAccount(string JID)
		{
			int i = JID.IndexOf('@');
			if (i >= 0)
				return JID.Substring(0, i);
			else
				return string.Empty;
		}

		private async Task DeliverQoSMessageHandler(object Sender, IqEventArgs e)
		{
			MessageEventArgs e2;
			string MsgId = XML.Attribute(e.Query, "msgId");
			string From = GetBareJID(e.From);
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

			await this.SendIqResult(e.Id, e.From, string.Empty);

			if (!(e2 is null))
				this.ProcessMessage(e2);
		}

		private async void SecondTimerCallback(object State)
		{
			try
			{
				if (!this.checkConnection.HasValue ||
					!this.checkConnection.Value ||
					this.disposed ||
					NetworkingModule.Stopping)
				{
					return;
				}

				if (DateTime.Now >= this.nextPing)
				{
					this.nextPing = DateTime.Now.AddMilliseconds(this.keepAliveSeconds * 500);

					if (this.state == XmppState.Connected)
					{
						if (this.sendHeartbeats)
						{
							try
							{
								if (this.supportsPing)
								{
									if (this.pingResponse)
									{
										this.pingResponse = false;
										await this.SendPing(string.Empty, this.PingResult, null);
									}
									else
									{
										try
										{
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
								this.Exception(ex);

								if (!NetworkingModule.Stopping)
									await this.Reconnect();
							}
						}
					}
					else
					{
						try
						{
							this.nextPing = DateTime.Now.AddSeconds(this.keepAliveSeconds);
							this.Warning("Reconnecting.");

							if (!NetworkingModule.Stopping)
								await this.Reconnect();
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
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
							if (!this.pendingRequestsByTimeout.Remove(Request.Timeout))
							{
								this.pendingRequestsBySeqNr.Remove(Request.SeqNr);
								continue;   // Already processed
							}
							else if (Retry = Request.CanRetry())
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

								Xml.Append("<iq xmlns='" + NamespaceClient + "' type='error' from='");
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

			if (!e.Ok && !this.disposed && !NetworkingModule.Stopping)
			{
				if (e.StanzaError is RecipientUnavailableException)
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
				else
					this.supportsPing = false;
			}
		}

		/// <summary>
		/// Sends an XMPP ping request.
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendPing(string To, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<ping xmlns='");
			Xml.Append(NamespacePing);
			Xml.Append("'/>");

			return this.SendIqGet(To, Xml.ToString(), Callback, State, 15000, 0);
		}

		private Task PingRequestHandler(object Sender, IqEventArgs e)
		{
			return e.IqResult(string.Empty);
		}

		/// <summary>
		/// If encryption is allowed. By default, this is true. If you, for some reason, want to disable encryption, for instance if you want
		/// to be able to sniff the communication for test purposes, set this property to false.
		/// </summary>
		public bool AllowEncryption
		{
			get => this.allowEncryption;
			set => this.allowEncryption = value;
		}

		/// <summary>
		/// Tries to get a tag from the client. Tags can be used to attached application specific objects to the client.
		/// </summary>
		/// <param name="TagName">Name of tag.</param>
		/// <param name="Tag">Tag value.</param>
		/// <returns>If a tag was found with the corresponding name.</returns>
		public bool TryGetTag(string TagName, out object Tag)
		{
			lock (this.synchObject)
			{
				if (this.tags.TryGetValue(TagName, out Tag))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Removes a tag from the client.
		/// </summary>
		/// <param name="TagName">Name of tag.</param>
		/// <returns>If a tag was found with the corresponding name, and removed.</returns>
		public bool RemoveTag(string TagName)
		{
			lock (this.synchObject)
			{
				return this.tags.Remove(TagName);
			}
		}

		/// <summary>
		/// Checks if a tag with a given name is defined.
		/// </summary>
		/// <param name="TagName">Name of tag.</param>
		/// <returns>If a tag with the corresponding name is found.</returns>
		public bool ContainsTag(string TagName)
		{
			lock (this.synchObject)
			{
				return this.tags.ContainsKey(TagName);
			}
		}

		/// <summary>
		/// Sets a tag value.
		/// </summary>
		/// <param name="TagName">Name of tag.</param>
		/// <param name="Tag">Tag value.</param>
		public void SetTag(string TagName, object Tag)
		{
			lock (this.synchObject)
			{
				this.tags[TagName] = Tag;
			}
		}

		/// <summary>
		/// If the from address attribute should be send on stanzas. Default is false. Set this to true, if using the client in
		/// serverless messaging.
		/// </summary>
		public bool SendFromAddress
		{
			get => this.sendFromAddress;
			set => this.sendFromAddress = value;
		}

		/// <summary>
		/// If new accounts can be registered.
		/// </summary>
		public bool CanRegister => this.canRegister;

		/// <summary>
		/// Registers an extension with the client.
		/// </summary>
		/// <param name="Extension">Extension</param>
		public void RegisterExtension(IXmppExtension Extension)
		{
			lock (this.synchObject)
			{
				if (!this.extensions.Contains(Extension))
					this.extensions.Add(Extension);
			}
		}

		/// <summary>
		/// Unregisters an extension on the client.
		/// </summary>
		/// <param name="Extension">Extension</param>
		/// <returns>If the extension was found and unregistered.</returns>
		public bool UnregisterExtension(IXmppExtension Extension)
		{
			lock (this.synchObject)
			{
				return this.extensions.Remove(Extension);
			}
		}

		/// <summary>
		/// Registered extensions.
		/// </summary>
		public IXmppExtension[] Extensions
		{
			get
			{
				lock (this.synchObject)
				{
					return this.extensions.ToArray();
				}
			}
		}

		/// <summary>
		/// Tries to get a registered extension of a specific type from the client.
		/// </summary>
		/// <param name="Type">Extension type.</param>
		/// <param name="Extension">Registered extension, if found.</param>
		/// <returns>If a registered extension of the desired type was found.</returns>
		public bool TryGetExtension(Type Type, out IXmppExtension Extension)
		{
			lock (this.synchObject)
			{
				foreach (IXmppExtension Extension2 in this.extensions)
				{
					if (Extension2.GetType() == Type)
					{
						Extension = Extension2;
						return true;
					}
				}
			}

			Extension = null;

			return false;
		}

		/// <summary>
		/// Tries to get a registered extension of a specific type from the client.
		/// </summary>
		/// <typeparam name="T">Extension type.</typeparam>
		/// <param name="Extension">Registered extension, if found.</param>
		/// <returns>If a registered extension of the desired type was found.</returns>
		public bool TryGetExtension<T>(out T Extension)
			where T : IXmppExtension
		{
			lock (this.synchObject)
			{
				foreach (IXmppExtension Extension2 in this.extensions)
				{
					if (Extension2 is T Result)
					{
						Extension = Result;
						return true;
					}
				}
			}

			Extension = default;

			return false;
		}

		/// <summary>
		/// Gets an array of random bytes.
		/// </summary>
		/// <param name="NrBytes">Number of random bytes to get.</param>
		/// <returns>Random bytes.</returns>
		public static byte[] GetRandomBytes(int NrBytes)
		{
			byte[] Result = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Result);
			}

			return Result;
		}

		private const double maxD = ((double)(1UL << 53));

		/// <summary>
		/// Gets a random value in the range [0,1).
		/// </summary>
		/// <returns>Random value in the range [0,1).</returns>
		public static double GetRandomDouble()
		{
			byte[] Bin = new byte[8];

			lock (rnd)
			{
				rnd.GetBytes(Bin);
			}

			ulong l = BitConverter.ToUInt64(Bin, 0);
			l >>= 11;

			return l / maxD;
		}

		/// <summary>
		/// Gets a random integer value in the interval [<paramref name="Min"/>,<paramref name="Max"/>].
		/// </summary>
		/// <param name="Min">Smallest value in interval.</param>
		/// <param name="Max">Largest value in interval.</param>
		/// <returns>Random number.</returns>
		public static int GetRandomValue(int Min, int Max)
		{
			if (Min > Max)
			{
				int Temp = Min;
				Min = Max;
				Max = Temp;
			}

			uint Diff = (uint)(Max - Min);
			return (int)(GetRandomDouble() * (Diff + 1)) + Min;
		}

		#region XEP-0049: Private XML Storage

		/// <summary>
		/// Gets an XML element from the Private XML Storage for the current account.
		/// </summary>
		/// <param name="LocalName">Local name of XML element.</param>
		/// <param name="Namespace">Namespace of XML element.</param>
		public async Task<XmlElement> GetPrivateXmlElementAsync(string LocalName, string Namespace)
		{
			TaskCompletionSource<XmlElement> Result = new TaskCompletionSource<XmlElement>();

			await this.GetPrivateXmlElement(LocalName, Namespace, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Element);
				else
					Result.TrySetException(e.StanzaError ?? new XmppException("Unable to get private XML element."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets an XML element from the Private XML Storage for the current account.
		/// </summary>
		/// <param name="LocalName">Local name of XML element.</param>
		/// <param name="Namespace">Namespace of XML element.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task GetPrivateXmlElement(string LocalName, string Namespace, EventHandlerAsync<PrivateXmlEventArgs> Callback, object State)
		{
			if (XML.Encode(LocalName) != LocalName)
				throw new ArgumentException("Invalid local name.", nameof(LocalName));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespacePrivateXmlStorage);
			Xml.Append("'><");
			Xml.Append(LocalName);
			Xml.Append(" xmlns='");
			Xml.Append(XML.Encode(Namespace));
			Xml.Append("'/></query>");

			return this.SendIqGet(string.Empty, Xml.ToString(), async (Sender, e) =>
			{
				XmlElement Result = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null))
				{
					foreach (XmlNode N in e.FirstElement.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == LocalName && E2.NamespaceURI == Namespace)
						{
							Result = E2;
							break;
						}
					}
				}

				if (Result is null)
					e.Ok = false;

				await Callback.Raise(this, new PrivateXmlEventArgs(Result, e));

			}, State);
		}

		/// <summary>
		/// Sets an XML element in the Private XML Storage for the current account.
		/// </summary>
		/// <param name="ElementXml">XML element to store.</param>
		public Task SetPrivateXmlElementAsync(string ElementXml)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(ElementXml);

			return this.SetPrivateXmlElementAsync(Doc.DocumentElement);
		}

		/// <summary>
		/// Sets an XML element in the Private XML Storage for the current account.
		/// </summary>
		/// <param name="Element">XML element to store.</param>
		public async Task SetPrivateXmlElementAsync(XmlElement Element)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			await this.SetPrivateXmlElement(Element, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new XmppException("Unable to set private XML element."));

				return Task.CompletedTask;
			}, null);

			await Result.Task;
		}

		/// <summary>
		/// Sets an XML element in the Private XML Storage for the current account.
		/// </summary>
		/// <param name="ElementXml">XML element to store.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task SetPrivateXmlElement(string ElementXml, EventHandlerAsync<PrivateXmlEventArgs> Callback, object State)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(ElementXml);

			return this.SetPrivateXmlElement(Doc.DocumentElement, Callback, State);
		}

		/// <summary>
		/// Sets an XML element in the Private XML Storage for the current account.
		/// </summary>
		/// <param name="Element">XML element to store.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to <paramref name="Callback"/>.</param>
		public Task SetPrivateXmlElement(XmlElement Element, EventHandlerAsync<PrivateXmlEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(NamespacePrivateXmlStorage);
			Xml.Append("'>");
			Xml.Append(Element.OuterXml);
			Xml.Append("</query>");

			return this.SendIqSet(string.Empty, Xml.ToString(), async (Sender, e) =>
			{
				await Callback.Raise(this, new PrivateXmlEventArgs(Element, e));

			}, State);
		}

		#endregion

		#region Finding components

		/// <summary>
		/// Finds a component having a specific feature, servicing a JID.
		/// </summary>
		/// <param name="Jid">JID</param>
		/// <param name="Feature">Requested feature.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void FindComponent(string Jid, string Feature, EventHandlerAsync<ServiceEventArgs> Callback, object State)
		{
			this.FindComponent(Jid, new string[] { Feature }, Callback, State);
		}

		/// <summary>
		/// Finds a component having a specific feature, servicing a JID.
		/// </summary>
		/// <param name="Jid">JID</param>
		/// <param name="Features">Requested features.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async void FindComponent(string Jid, string[] Features, EventHandlerAsync<ServiceEventArgs> Callback, object State)
		{
			string Service;
			string Feature;

			try
			{
				KeyValuePair<string, string> P = await this.FindComponentAsync(Jid, Features);
				Service = P.Key;
				Feature = P.Value;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Service = null;
				Feature = null;
			}

			await Callback.Raise(this, new ServiceEventArgs(Service, Feature, State));
		}

		/// <summary>
		/// Finds a component having a specific feature, servicing a JID.
		/// </summary>
		/// <param name="Jid">JID</param>
		/// <param name="Feature">Requested feature.</param>
		/// <returns>Component JID, if found, or null if not.</returns>
		public async Task<string> FindComponentAsync(string Jid, string Feature)
		{
			KeyValuePair<string, string> P = await this.FindComponentAsync(Jid, new string[] { Feature });
			return P.Key;
		}

		/// <summary>
		/// Finds a component having one of a specific set of features, servicing a JID.
		/// </summary>
		/// <param name="Jid">JID</param>
		/// <param name="Features">Requested set of features.</param>
		/// <returns>Component JID and corresponding namespace, if found, or null if not.</returns>
		public async Task<KeyValuePair<string, string>> FindComponentAsync(string Jid, params string[] Features)
		{
			string Key;

			lock (this.synchObject)
			{
				foreach (string Feature in Features)
				{
					Key = Jid + " " + Feature;

					if (this.services.TryGetValue(Key, out string Service))
						return new KeyValuePair<string, string>(Service, Feature);
				}
			}

			string Domain = GetDomain(Jid);

			ServiceDiscoveryEventArgs e = await this.ServiceDiscoveryAsync(Domain);

			foreach (string Feature in Features)
			{
				if (e.HasFeature(Feature))
				{
					Key = Jid + " " + Feature;

					lock (this.synchObject)
					{
						this.services[Key] = Domain;
					}

					return new KeyValuePair<string, string>(Domain, Feature);
				}
			}

			ServiceItemsDiscoveryEventArgs e2 = await this.ServiceItemsDiscoveryAsync(Domain);

			foreach (Item Component in e2.Items)
			{
				e = await this.ServiceDiscoveryAsync(Component.JID);

				foreach (string Feature in Features)
				{
					if (e.HasFeature(Feature))
					{
						Key = Jid + " " + Feature;

						lock (this.synchObject)
						{
							this.services[Key] = Component.JID;
						}

						return new KeyValuePair<string, string>(Component.JID, Feature);
					}
				}
			}

			return new KeyValuePair<string, string>(null, null);
		}

		#endregion
	}
}
