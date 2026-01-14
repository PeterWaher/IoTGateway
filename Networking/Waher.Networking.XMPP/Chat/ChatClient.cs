using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Chat.ChatStates;
using Waher.Networking.XMPP.Chat.Corrections;
using Waher.Networking.XMPP.Chat.Markers;
using Waher.Networking.XMPP.Chat.Receipts;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Chat
{
	/// <summary>
	/// Chat-focused client that encapsulates user messaging behaviors,
	/// delivery receipts (XEP-0184) https://xmpp.org/extensions/xep-0184.html
	/// displayed markers (XEP-0333) https://xmpp.org/extensions/xep-0333.html,
	/// message corrections (XEP-0308) https://xmpp.org/extensions/xep-0308.html
	/// It registers message handlers on the underlying <see cref="XmppClient"/>, raises chat-specific events,
	/// and forwards content messages to default message routing when appropriate.
	/// </summary>
	public class ChatClient : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/chatstates
		/// </summary>
		public const string NamespaceChatStates = "http://jabber.org/protocol/chatstates";
		private const string NamespaceDelayedDelivery = "urn:xmpp:delay";
		private const string NamespaceDelayedDeliveryLegacy = "jabber:x:delay";

		private readonly Dictionary<string, ChatStateNegotiationState> chatStateNegotiation = new Dictionary<string, ChatStateNegotiationState>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, ChatState> lastStandaloneState = new Dictionary<string, ChatState>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, ChatState> lastStatePerContact = new Dictionary<string, ChatState>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, string> currentThreadIdPerContact = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> endedThreads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, bool> chatStateCapabilitiesByNodeVersion = new Dictionary<string, bool>(StringComparer.Ordinal);
		private readonly HashSet<string> pendingChatStateCapabilityRequests = new HashSet<string>(StringComparer.Ordinal);
		private readonly object synchObject = new object();
		private bool enableChatStateNotifications = false;
		private bool enableDeliveryReceipts = false;
		private bool enableChatMarkers = false;
		private bool enableMessageCorrection = false;
		private bool autoRespondToReceiptRequests = false;
		private bool autoSendDisplayedMarkers = false;

		/// <summary>
		/// Chat-focused client that encapsulates user messaging behaviors,
		/// delivery receipts (XEP-0184) https://xmpp.org/extensions/xep-0184.html
		/// displayed markers (XEP-0333) https://xmpp.org/extensions/xep-0333.html,
		/// message corrections (XEP-0308) https://xmpp.org/extensions/xep-0308.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public ChatClient(XmppClient Client)
			: base(Client)
		{
			this.client.RegisterMessageHandler("active", NamespaceChatStates, this.ChatStateHandler, true);
			this.client.RegisterMessageHandler("composing", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.RegisterMessageHandler("paused", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.RegisterMessageHandler("inactive", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.RegisterMessageHandler("gone", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.RegisterMessageHandler("request", DeliveryReceipts.NamespaceDeliveryReceipts, this.ReceiptRequestHandler, false);
			this.client.RegisterMessageHandler("received", DeliveryReceipts.NamespaceDeliveryReceipts, this.ReceiptReceivedHandler, false);
			this.client.RegisterMessageHandler("markable", ChatMarkers.NamespaceChatMarkers, this.ChatMarkerHandler, false);
			this.client.RegisterMessageHandler("displayed", ChatMarkers.NamespaceChatMarkers, this.ChatMarkerHandler, false);
			this.client.RegisterMessageHandler("replace", MessageCorrections.NamespaceMessageCorrection, this.MessageCorrectionHandler, false);

			this.client.OnPresence += this.Client_OnPresence;
			this.client.OnMessageReceived += this.Client_OnMessageReceived;
			this.client.OnStateChanged += this.Client_OnStateChanged;
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this.client.UnregisterMessageHandler("active", NamespaceChatStates, this.ChatStateHandler, true);
			this.client.UnregisterMessageHandler("composing", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.UnregisterMessageHandler("paused", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.UnregisterMessageHandler("inactive", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.UnregisterMessageHandler("gone", NamespaceChatStates, this.ChatStateHandler, false);
			this.client.UnregisterMessageHandler("request", DeliveryReceipts.NamespaceDeliveryReceipts, this.ReceiptRequestHandler, false);
			this.client.UnregisterMessageHandler("received", DeliveryReceipts.NamespaceDeliveryReceipts, this.ReceiptReceivedHandler, false);
			this.client.UnregisterMessageHandler("markable", ChatMarkers.NamespaceChatMarkers, this.ChatMarkerHandler, false);
			this.client.UnregisterMessageHandler("displayed", ChatMarkers.NamespaceChatMarkers, this.ChatMarkerHandler, false);
			this.client.UnregisterMessageHandler("replace", MessageCorrections.NamespaceMessageCorrection, this.MessageCorrectionHandler, false);

			this.client.OnPresence -= this.Client_OnPresence;
			this.client.OnMessageReceived -= this.Client_OnMessageReceived;
			this.client.OnStateChanged -= this.Client_OnStateChanged;

			if (this.enableChatStateNotifications)
				this.client.UnregisterFeature(NamespaceChatStates);
			if (this.enableDeliveryReceipts)
				this.client.UnregisterFeature(DeliveryReceipts.NamespaceDeliveryReceipts);
			if (this.enableChatMarkers)
				this.client.UnregisterFeature(ChatMarkers.NamespaceChatMarkers);
			if (this.enableMessageCorrection)
				this.client.UnregisterFeature(MessageCorrections.NamespaceMessageCorrection);

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0085", "XEP-0184", "XEP-0333", "XEP-0308" };

		/// <summary>
		/// If XEP-0085 chat state notifications are enabled for this client instance. Default is false.
		/// </summary>
		public bool EnableChatStateNotifications
		{
			get
			{
				lock (this.synchObject)
				{
					return this.enableChatStateNotifications;
				}
			}
			set
			{
				bool Changed;

				lock (this.synchObject)
				{
					Changed = this.enableChatStateNotifications != value;
					this.enableChatStateNotifications = value;
				}

				if (!Changed)
					return;

				if (value)
					this.client.RegisterFeature(NamespaceChatStates);
				else
					this.client.UnregisterFeature(NamespaceChatStates);
			}
		}

		/// <summary>
		/// If XEP-0184 message delivery receipts are enabled. Default is false.
		/// </summary>
		public bool EnableDeliveryReceipts
		{
			get
			{
				lock (this.synchObject)
				{
					return this.enableDeliveryReceipts;
				}
			}
			set
			{
				bool Changed;

				lock (this.synchObject)
				{
					Changed = this.enableDeliveryReceipts != value;
					this.enableDeliveryReceipts = value;
				}

				if (!Changed)
					return;

				if (value)
					this.client.RegisterFeature(DeliveryReceipts.NamespaceDeliveryReceipts);
				else
					this.client.UnregisterFeature(DeliveryReceipts.NamespaceDeliveryReceipts);
			}
		}

		/// <summary>
		/// If XEP-0333 displayed markers are enabled. Default is false.
		/// </summary>
		public bool EnableChatMarkers
		{
			get
			{
				lock (this.synchObject)
				{
					return this.enableChatMarkers;
				}
			}
			set
			{
				bool Changed;

				lock (this.synchObject)
				{
					Changed = this.enableChatMarkers != value;
					this.enableChatMarkers = value;
				}

				if (!Changed)
					return;

				if (value)
					this.client.RegisterFeature(ChatMarkers.NamespaceChatMarkers);
				else
					this.client.UnregisterFeature(ChatMarkers.NamespaceChatMarkers);
			}
		}

		/// <summary>
		/// If XEP-0308 message correction is enabled. Default is false.
		/// </summary>
		public bool EnableMessageCorrection
		{
			get
			{
				lock (this.synchObject)
				{
					return this.enableMessageCorrection;
				}
			}
			set
			{
				bool Changed;

				lock (this.synchObject)
				{
					Changed = this.enableMessageCorrection != value;
					this.enableMessageCorrection = value;
				}

				if (!Changed)
					return;

				if (value)
					this.client.RegisterFeature(MessageCorrections.NamespaceMessageCorrection);
				else
					this.client.UnregisterFeature(MessageCorrections.NamespaceMessageCorrection);
			}
		}

		/// <summary>
		/// If receipt requests should be auto-acknowledged. Default is false.
		/// </summary>
		public bool AutoRespondToReceiptRequests
		{
			get
			{
				lock (this.synchObject)
				{
					return this.autoRespondToReceiptRequests;
				}
			}
			set
			{
				lock (this.synchObject)
				{
					this.autoRespondToReceiptRequests = value;
				}
			}
		}

		/// <summary>
		/// If displayed markers should be sent automatically. Default is false.
		/// </summary>
		public bool AutoSendDisplayedMarkers
		{
			get
			{
				lock (this.synchObject)
				{
					return this.autoSendDisplayedMarkers;
				}
			}
			set
			{
				lock (this.synchObject)
				{
					this.autoSendDisplayedMarkers = value;
				}
			}
		}

		/// <summary>
		/// Raised when chat state support has been determined for a contact.
		/// </summary>
		public event ChatStateSupportDeterminedEventHandler OnChatStateSupportDetermined = null;

		/// <summary>
		/// Unified chat state changed event.
		/// </summary>
		public event EventHandlerAsync<ChatStateEventArgs> OnChatStateChanged = null;

		/// <summary>
		/// Raised when composing chat state received.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnChatComposing = null;

		/// <summary>
		/// Raised when active chat state received.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnChatActive = null;

		/// <summary>
		/// Raised when paused chat state received.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnChatPaused = null;

		/// <summary>
		/// Raised when inactive chat state received.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnChatInactive = null;

		/// <summary>
		/// Raised when gone chat state received.
		/// </summary>
		public event EventHandlerAsync<MessageEventArgs> OnChatGone = null;

		/// <summary>
		/// Raised when a delivery receipt request is received.
		/// </summary>
		public event EventHandlerAsync<DeliveryReceiptEventArgs> OnReceiptRequest = null;

		/// <summary>
		/// Raised when a delivery receipt is received.
		/// </summary>
		public event EventHandlerAsync<DeliveryReceiptEventArgs> OnReceiptReceived = null;

		/// <summary>
		/// Raised when a chat marker is received.
		/// </summary>
		public event EventHandlerAsync<ChatMarkerEventArgs> OnChatMarker = null;

		/// <summary>
		/// Raised when a message correction is received.
		/// </summary>
		public event EventHandlerAsync<MessageCorrectionEventArgs> OnMessageCorrected = null;

		/// <summary>
		/// Returns if chat state notifications are known to be supported for the bare JID.
		/// </summary>
		/// <param name="BareJid">Bare JID.</param>
		/// <returns>If chat states are supported.</returns>
		public bool IsChatStateSupported(string BareJid)
		{
			if (string.IsNullOrEmpty(BareJid))
				return false;

			lock (this.synchObject)
			{
				return this.chatStateNegotiation.TryGetValue(BareJid, out ChatStateNegotiationState state) && state == ChatStateNegotiationState.Supported;
			}
		}

		/// <summary>
		/// Sends a chat-state notification stanza. Obeys <see cref="EnableChatStateNotifications"/> and negotiation state.
		/// </summary>
		/// <param name="To">Destination bare or full JID.</param>
		/// <param name="State">Chat state to send.</param>
		/// <param name="ThreadId">Thread identifier.</param>
		/// <param name="ParentThreadId">Optional parent thread identifier.</param>
		public Task SendChatState(string To, ChatState State, string ThreadId = "", string ParentThreadId = "")
		{
			return this.SendStandaloneChatState(To, State, MessageType.Chat, ThreadId, ParentThreadId);
		}

		/// <summary>
		/// Sends a standalone chat-state notification, enforcing negotiation, duplicate suppression, and thread rules.
		/// Standalone <active/> notifications are suppressed per XEP-0085 guidance.
		/// </summary>
		/// <param name="To">Destination bare or full JID.</param>
		/// <param name="State">Chat state to send.</param>
		/// <param name="Type">Message type (chat or groupchat).</param>
		/// <param name="ThreadId">Thread identifier.</param>
		/// <param name="ParentThreadId">Optional parent thread identifier.</param>
		public Task SendStandaloneChatState(string To, ChatState State, MessageType Type = MessageType.Chat, string ThreadId = "", string ParentThreadId = "")
		{
			if (string.IsNullOrEmpty(To))
				return Task.CompletedTask;

			if (Type != MessageType.Chat && Type != MessageType.GroupChat)
				throw new ArgumentOutOfRangeException(nameof(Type), "Chat state notifications are only valid for chat or groupchat messages.");

			if (!this.EnableChatStateNotifications)
				return Task.CompletedTask;

			if (State == ChatState.Active)
				return Task.CompletedTask;

			if (Type == MessageType.GroupChat && State == ChatState.Gone)
				return Task.CompletedTask;

			string BareJid = XmppClient.GetBareJID(To);
			if (string.IsNullOrEmpty(BareJid))
				BareJid = To;

			string ResolvedThreadId = ThreadId;
			bool WillSend;

			lock (this.synchObject)
			{
				ChatStateNegotiationState NegotiationState = this.chatStateNegotiation.TryGetValue(BareJid, out ChatStateNegotiationState Tmp) ? Tmp : ChatStateNegotiationState.Unknown;
				WillSend = NegotiationState != ChatStateNegotiationState.NotSupported;

				if (WillSend && string.IsNullOrEmpty(ResolvedThreadId) && this.currentThreadIdPerContact.TryGetValue(BareJid, out string CurrentThread))
					ResolvedThreadId = CurrentThread;

				string StandaloneKey = ComposeStandaloneStateKey(BareJid, ResolvedThreadId);
				if (WillSend && this.lastStandaloneState.TryGetValue(StandaloneKey, out ChatState LastState) && LastState == State)
					WillSend = false;

				if (WillSend && State == ChatState.Gone && Type != MessageType.GroupChat && !string.IsNullOrEmpty(ResolvedThreadId))
				{
					string Key = ComposeThreadKey(BareJid, ResolvedThreadId);
					if (this.endedThreads.Contains(Key))
						WillSend = false;
					else
					{
						this.endedThreads.Add(Key);
						this.currentThreadIdPerContact.Remove(BareJid);
					}
				}

				if (WillSend)
					this.lastStandaloneState[StandaloneKey] = State;
			}

			if (!WillSend)
				return Task.CompletedTask;

			string LocalName = GetLocalName(State);
			string Xml = "<" + LocalName + " xmlns='" + NamespaceChatStates + "'/>";

			return this.client.SendMessage(QoSLevel.Unacknowledged, Type, string.Empty, To, Xml, string.Empty, string.Empty, string.Empty,
				ResolvedThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a chat content message ensuring chat-state negotiation rules are applied.
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Body">Body text.</param>
		/// <param name="Subject">Optional subject.</param>
		/// <param name="Language">Language code.</param>
		/// <param name="ThreadId">Thread identifier.</param>
		/// <param name="ParentThreadId">Optional parent thread identifier.</param>
		public Task SendChatContentMessage(string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId)
		{
			return this.SendChatContentMessageInternal(MessageType.Chat, To, Body, Subject, Language, ThreadId, ParentThreadId, string.Empty, string.Empty);
		}

		/// <summary>
		/// Sends a chat content message with extra payload XML.
		/// </summary>
		/// <param name="To">Destination address.</param>
		/// <param name="Body">Body text.</param>
		/// <param name="Subject">Optional subject.</param>
		/// <param name="Language">Language code.</param>
		/// <param name="ThreadId">Thread identifier.</param>
		/// <param name="ParentThreadId">Optional parent thread identifier.</param>
		/// <param name="CustomXml">Custom XML payload.</param>
		/// <param name="MessageId">Message stanza id.</param>
		public Task SendChatContentMessage(string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId, string CustomXml, string MessageId)
		{
			return this.SendChatContentMessageInternal(MessageType.Chat, To, Body, Subject, Language, ThreadId, ParentThreadId, CustomXml, MessageId);
		}

		private Task SendChatContentMessageInternal(MessageType Type, string To, string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId, string CustomXml, string MessageId)
		{
			bool HasSubject = !string.IsNullOrEmpty(Subject);
			bool HasBody = !string.IsNullOrEmpty(Body);
			bool IsContentMessage = HasBody || HasSubject;
			string BareJid = string.Empty;

			if (!string.IsNullOrEmpty(To))
			{
				BareJid = XmppClient.GetBareJID(To);
				if (string.IsNullOrEmpty(BareJid))
					BareJid = To;
			}

			if (Type == MessageType.Chat && !string.IsNullOrEmpty(BareJid))
				ThreadId = this.EnsureOutgoingThreadId(BareJid, ThreadId, IsContentMessage);

			string ResolvedCustomXml = CustomXml ?? string.Empty;
			bool ShouldEmbedActive = false;

			if (Type == MessageType.Chat && this.EnableChatStateNotifications && !string.IsNullOrEmpty(BareJid) && IsContentMessage)
			{
				ChatStateNegotiationState NegotiationState;
				lock (this.synchObject)
				{
					NegotiationState = this.chatStateNegotiation.TryGetValue(BareJid, out ChatStateNegotiationState tmp) ? tmp : ChatStateNegotiationState.Unknown;
				}

				if (NegotiationState != ChatStateNegotiationState.NotSupported)
				{
					ShouldEmbedActive = true;

					if (NegotiationState == ChatStateNegotiationState.Unknown)
						this.TryUpdateNegotiationState(BareJid, ChatStateNegotiationState.Requested, out _);
				}
			}

			if (ShouldEmbedActive)
				ResolvedCustomXml += "<" + GetLocalName(ChatState.Active) + " xmlns='" + NamespaceChatStates + "'/>";

			return this.client.SendMessage(QoSLevel.Unacknowledged, Type, MessageId ?? string.Empty, To, ResolvedCustomXml, Body, Subject, Language,
				ThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a composing chat-state notification.
		/// </summary>
		public Task SendComposing(string To, string ThreadId = "", string ParentThreadId = "") => this.SendStandaloneChatState(To, ChatState.Composing, MessageType.Chat, ThreadId, ParentThreadId);
		/// <summary>
		/// Sends an active chat-state notification. Standalone <active/> is suppressed; use content messages to include it.
		/// </summary>
		public Task SendActive(string To, string ThreadId = "", string ParentThreadId = "") => this.SendStandaloneChatState(To, ChatState.Active, MessageType.Chat, ThreadId, ParentThreadId);
		/// <summary>
		/// Sends a paused chat-state notification.
		/// </summary>
		public Task SendPaused(string To, string ThreadId = "", string ParentThreadId = "") => this.SendStandaloneChatState(To, ChatState.Paused, MessageType.Chat, ThreadId, ParentThreadId);

		/// <summary>
		/// Sends an inactive chat-state notification.
		/// </summary>
		public Task SendInactive(string To, string ThreadId = "", string ParentThreadId = "") => this.SendStandaloneChatState(To, ChatState.Inactive, MessageType.Chat, ThreadId, ParentThreadId);

		/// <summary>
		/// Sends a gone chat-state notification.
		/// </summary>
		public Task SendGone(string To, string ThreadId = "", string ParentThreadId = "") => this.SendStandaloneChatState(To, ChatState.Gone, MessageType.Chat, ThreadId, ParentThreadId);

		/// <summary>
		/// Sends a simple chat message.
		/// </summary>
		public Task SendChatMessage(string To, string Body) =>
			this.SendChatContentMessage(To, Body, string.Empty, string.Empty, string.Empty, string.Empty);

		/// <summary>
		/// Sends a simple chat message.
		/// </summary>
		public Task SendChatMessage(string To, string Body, string Subject) =>
			this.SendChatContentMessage(To, Body, Subject, string.Empty, string.Empty, string.Empty);

		/// <summary>
		/// Sends a simple chat message.
		/// </summary>
		public Task SendChatMessage(string To, string Body, string Subject, string Language) =>
			this.SendChatContentMessage(To, Body, Subject, Language, string.Empty, string.Empty);

		/// <summary>
		/// Sends a simple chat message.
		/// </summary>
		public Task SendChatMessage(string To, string Body, string Subject, string Language, string ThreadId) =>
			this.SendChatContentMessage(To, Body, Subject, Language, ThreadId, string.Empty);

		/// <summary>
		/// Sends a simple chat message.
		/// </summary>
		public Task SendChatMessage(string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId) =>
			this.SendChatContentMessage(To, Body, Subject, Language, ThreadId, ParentThreadId);

		/// <summary>
		/// Sends a chat message requesting a delivery receipt (XEP-0184).
		/// </summary>
		public Task SendChatMessageWithReceiptRequest(string To, string Body, string MessageId) =>
			this.SendChatMessageWithReceiptRequest(To, Body, string.Empty, string.Empty, string.Empty, string.Empty, MessageId);

		/// <summary>
		/// Sends a chat message requesting a delivery receipt (XEP-0184).
		/// </summary>
		public Task SendChatMessageWithReceiptRequest(string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId, string MessageId)
		{
			if (!this.EnableDeliveryReceipts || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(MessageId))
				return Task.CompletedTask;

			string Xml = "<request xmlns='" + DeliveryReceipts.NamespaceDeliveryReceipts + "'/>";
			return this.SendChatContentMessageInternal(MessageType.Chat, To, Body, Subject, Language, ThreadId, ParentThreadId, Xml, MessageId);
		}

		/// <summary>
		/// Sends a standalone receipt request (XEP-0184).
		/// </summary>
		public Task SendReceiptRequest(string To, string MessageId, MessageType Type = MessageType.Chat, string ThreadId = "", string ParentThreadId = "")
		{
			if (!this.EnableDeliveryReceipts || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(MessageId))
				return Task.CompletedTask;

			if (Type != MessageType.Chat)
				return Task.CompletedTask;

			string Xml = "<request xmlns='" + DeliveryReceipts.NamespaceDeliveryReceipts + "'/>";
			return this.client.SendMessage(QoSLevel.Unacknowledged, Type, MessageId, To, Xml, string.Empty, string.Empty, string.Empty,
				ThreadId, ParentThreadId, null, null);
		}

		/// <summary>
		/// Sends a delivery receipt (XEP-0184).
		/// </summary>
		public Task SendReceiptReceived(string To, string MessageId, MessageType Type = MessageType.Chat)
		{
			if (!this.EnableDeliveryReceipts || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(MessageId))
				return Task.CompletedTask;

			if (Type != MessageType.Chat)
				return Task.CompletedTask;

			string Xml = "<received xmlns='" + DeliveryReceipts.NamespaceDeliveryReceipts + "' id='" + XML.Encode(MessageId) + "'/>";
			string AckId = this.client.NextId();

			return this.client.SendMessage(QoSLevel.Unacknowledged, Type, AckId, To, Xml, string.Empty, string.Empty, string.Empty,
				string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a chat message marking it as markable (XEP-0333).
		/// </summary>
		public Task SendChatMessageMarkable(string To, string Body, string MessageId) =>
			this.SendChatMessageMarkable(To, Body, string.Empty, string.Empty, string.Empty, string.Empty, MessageId);

		/// <summary>
		/// Sends a chat message marking it as markable (XEP-0333).
		/// </summary>
		public Task SendChatMessageMarkable(string To, string Body, string Subject, string Language, string ThreadId, string ParentThreadId, string MessageId)
		{
			if (!this.EnableChatMarkers || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(MessageId))
				return Task.CompletedTask;

			string Xml = "<markable xmlns='" + ChatMarkers.NamespaceChatMarkers + "'/>";
			return this.SendChatContentMessageInternal(MessageType.Chat, To, Body, Subject, Language, ThreadId, ParentThreadId, Xml, MessageId);
		}

		/// <summary>
		/// Sends a standalone markable marker (XEP-0333).
		/// </summary>
		public Task SendMarkable(string To, string MessageId, MessageType Type = MessageType.Chat)
		{
			if (!this.EnableChatMarkers || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(MessageId))
				return Task.CompletedTask;

			if (Type != MessageType.Chat)
				return Task.CompletedTask;

			string Xml = "<markable xmlns='" + ChatMarkers.NamespaceChatMarkers + "'/>";
			return this.client.SendMessage(QoSLevel.Unacknowledged, Type, MessageId, To, Xml, string.Empty, string.Empty, string.Empty,
				string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a displayed marker (XEP-0333).
		/// </summary>
		public Task SendDisplayedMarker(string To, string MessageId, MessageType Type = MessageType.Chat)
		{
			if (!this.EnableChatMarkers || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(MessageId))
				return Task.CompletedTask;

			if (Type != MessageType.Chat)
				return Task.CompletedTask;

			string Xml = "<displayed xmlns='" + ChatMarkers.NamespaceChatMarkers + "' id='" + XML.Encode(MessageId) + "'/>";
			string MarkerId = this.client.NextId();

			return this.client.SendMessage(QoSLevel.Unacknowledged, Type, MarkerId, To, Xml, string.Empty, string.Empty, string.Empty,
				string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Sends a message correction (XEP-0308).
		/// </summary>
		public Task SendMessageCorrection(string To, string ReplaceId, string NewBody, string NewSubject, string Language, string ThreadId, string ParentThreadId = "")
		{
			if (!this.EnableMessageCorrection || string.IsNullOrEmpty(To) || string.IsNullOrEmpty(ReplaceId))
				return Task.CompletedTask;

			if (string.IsNullOrEmpty(NewBody) && string.IsNullOrEmpty(NewSubject))
				return Task.CompletedTask;

			string NewId = this.client.NextId();
			string Xml = "<replace xmlns='" + MessageCorrections.NamespaceMessageCorrection + "' id='" + XML.Encode(ReplaceId) + "'/>";

			return this.SendChatContentMessageInternal(MessageType.Chat, To, NewBody, NewSubject, Language, ThreadId, ParentThreadId, Xml, NewId);
		}

		private Task Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			this.EvaluateChatStateCapabilities(e);
			return Task.CompletedTask;
		}

		private Task Client_OnMessageReceived(object Sender, MessageEventArgs e)
		{
			return this.EvaluateIncomingChatStateSupport(e);
		}

		private Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Offline:
				case XmppState.Error:
					this.ResetState();
					break;
			}

			return Task.CompletedTask;
		}

		private void ResetState()
		{
			lock (this.synchObject)
			{
				this.chatStateNegotiation.Clear();
				this.lastStandaloneState.Clear();
				this.lastStatePerContact.Clear();
				this.currentThreadIdPerContact.Clear();
				this.endedThreads.Clear();
				this.pendingChatStateCapabilityRequests.Clear();
			}
		}

		private async Task ChatStateHandler(object Sender, MessageEventArgs e)
		{
			if (!(e.Content is XmlElement Element) || Element.NamespaceURI != NamespaceChatStates)
				return;

			ChatState State = GetChatStateFromLocalName(Element.LocalName);
			if (e.Type == MessageType.GroupChat && State == ChatState.Gone)
			{
				if (!string.IsNullOrEmpty(e.Body) || !string.IsNullOrEmpty(e.Subject))
					await this.client.RaiseMessageEventAsync(e);
				return;
			}

			string BareJid = XmppClient.GetBareJID(e.From);
			ChatState PreviousState = ChatState.Active;

			if (!string.IsNullOrEmpty(BareJid))
			{
				if (this.TryUpdateNegotiationState(BareJid, ChatStateNegotiationState.Supported, out ChatStateNegotiationState PreviousNegotiation) &&
					PreviousNegotiation != ChatStateNegotiationState.Supported)
				{
					await this.RaiseChatStateSupportDeterminedAsync(BareJid, true);
				}

				PreviousState = this.UpdateLastStateForContact(BareJid, State);
				this.HandleThreadTrackingForIncomingState(BareJid, State, e);
			}

			await this.RaiseChatStateEvents(State, PreviousState, e);

			if (!string.IsNullOrEmpty(e.Body) || !string.IsNullOrEmpty(e.Subject))
				await this.client.RaiseMessageEventAsync(e);
		}

		private async Task ReceiptRequestHandler(object Sender, MessageEventArgs e)
		{
			if (!(e.Content is XmlElement Element) || Element.NamespaceURI != DeliveryReceipts.NamespaceDeliveryReceipts || Element.LocalName != "request")
				return;

			string Id = e.Id ?? string.Empty;
			EventHandlerAsync<DeliveryReceiptEventArgs> Handler = this.OnReceiptRequest;
			if (!(Handler is null))
				await Handler.Raise(this, new DeliveryReceiptEventArgs(e, Id, true));

			bool AutoRespond;
			lock (this.synchObject)
			{
				AutoRespond = this.enableDeliveryReceipts && this.autoRespondToReceiptRequests;
			}

			if (AutoRespond && e.Type == MessageType.Chat && !string.IsNullOrEmpty(Id) && !HasDelayedDelivery(e))
				await this.SendReceiptReceived(e.From, Id, MessageType.Chat);

			if (!string.IsNullOrEmpty(e.Body) || !string.IsNullOrEmpty(e.Subject))
				await this.client.RaiseMessageEventAsync(e);
		}

		private async Task ReceiptReceivedHandler(object Sender, MessageEventArgs e)
		{
			if (!(e.Content is XmlElement Element) || Element.NamespaceURI != DeliveryReceipts.NamespaceDeliveryReceipts || Element.LocalName != "received")
				return;

			string Id = Element.GetAttribute("id");
			if (string.IsNullOrEmpty(Id))
				return;

			EventHandlerAsync<DeliveryReceiptEventArgs> Handler = this.OnReceiptReceived;
			if (!(Handler is null))
				await Handler.Raise(this, new DeliveryReceiptEventArgs(e, Id, false));

			if (!string.IsNullOrEmpty(e.Body) || !string.IsNullOrEmpty(e.Subject))
				await this.client.RaiseMessageEventAsync(e);
		}

		private async Task ChatMarkerHandler(object Sender, MessageEventArgs e)
		{
			if (!(e.Content is XmlElement Element) || Element.NamespaceURI != ChatMarkers.NamespaceChatMarkers)
				return;

			ChatMarkerType MarkerType;
			string Id = string.Empty;

			switch (Element.LocalName)
			{
				case "markable":
					MarkerType = ChatMarkerType.Markable;
					Id = e.Id ?? string.Empty;
					break;

				case "displayed":
					MarkerType = ChatMarkerType.Displayed;
					Id = Element.GetAttribute("id");
					if (string.IsNullOrEmpty(Id))
						return;
					break;

				default:
					return;
			}

			EventHandlerAsync<ChatMarkerEventArgs> Handler = this.OnChatMarker;
			if (!(Handler is null))
				await Handler.Raise(this, new ChatMarkerEventArgs(e, MarkerType, Id));

			bool AutoSend;
			lock (this.synchObject)
			{
				AutoSend = this.enableChatMarkers && this.autoSendDisplayedMarkers;
			}

			if (MarkerType == ChatMarkerType.Markable && AutoSend && e.Type == MessageType.Chat && !string.IsNullOrEmpty(e.Id) && !HasDelayedDelivery(e))
				await this.SendDisplayedMarker(e.From, e.Id, MessageType.Chat);

			if (!string.IsNullOrEmpty(e.Body) || !string.IsNullOrEmpty(e.Subject))
				await this.client.RaiseMessageEventAsync(e);
		}

		private async Task MessageCorrectionHandler(object Sender, MessageEventArgs e)
		{
			if (!(e.Content is XmlElement Element) || Element.NamespaceURI != MessageCorrections.NamespaceMessageCorrection || Element.LocalName != "replace")
				return;

			string ReplaceId = Element.GetAttribute("id");
			if (string.IsNullOrEmpty(ReplaceId))
				return;

			EventHandlerAsync<MessageCorrectionEventArgs> Handler = this.OnMessageCorrected;
			if (!(Handler is null))
				await Handler.Raise(this, new MessageCorrectionEventArgs(e, ReplaceId, e.Body, e.Subject));

			if (!string.IsNullOrEmpty(e.Body) || !string.IsNullOrEmpty(e.Subject))
				await this.client.RaiseMessageEventAsync(e);
		}

		private bool TryUpdateNegotiationState(string BareJid, ChatStateNegotiationState NewState, out ChatStateNegotiationState PreviousState)
		{
			if (string.IsNullOrEmpty(BareJid))
			{
				PreviousState = ChatStateNegotiationState.Unknown;
				return false;
			}

			lock (this.synchObject)
			{
				if (!this.chatStateNegotiation.TryGetValue(BareJid, out PreviousState))
					PreviousState = ChatStateNegotiationState.Unknown;

				if (PreviousState == NewState)
					return false;

				if (PreviousState == ChatStateNegotiationState.NotSupported && NewState == ChatStateNegotiationState.Requested)
					return false;

				this.chatStateNegotiation[BareJid] = NewState;
				return true;
			}
		}

		private ChatState UpdateLastStateForContact(string BareJid, ChatState NewState)
		{
			lock (this.synchObject)
			{
				if (!this.lastStatePerContact.TryGetValue(BareJid, out ChatState Previous))
					Previous = ChatState.Active;

				this.lastStatePerContact[BareJid] = NewState;
				return Previous;
			}
		}

		private void HandleThreadTrackingForIncomingState(string BareJid, ChatState State, MessageEventArgs e)
		{
			lock (this.synchObject)
			{
				if (!string.IsNullOrEmpty(e.ThreadID))
					this.currentThreadIdPerContact[BareJid] = e.ThreadID;

				if (State == ChatState.Gone && e.Type != MessageType.GroupChat && !string.IsNullOrEmpty(e.ThreadID))
				{
					string Key = ComposeThreadKey(BareJid, e.ThreadID);
					this.endedThreads.Add(Key);

					if (this.currentThreadIdPerContact.TryGetValue(BareJid, out string current) && current == e.ThreadID)
						this.currentThreadIdPerContact.Remove(BareJid);
				}
			}
		}

		private async Task RaiseChatStateEvents(ChatState State, ChatState PreviousState, MessageEventArgs e)
		{
			this.Information("OnChatStateChanged()");
			EventHandlerAsync<ChatStateEventArgs> Changed = this.OnChatStateChanged;
			if (!(Changed is null))
				await Changed.Raise(this, new ChatStateEventArgs(e, State, PreviousState));

			EventHandlerAsync<MessageEventArgs> Specific = State switch
			{
				ChatState.Composing => this.OnChatComposing,
				ChatState.Active => this.OnChatActive,
				ChatState.Paused => this.OnChatPaused,
				ChatState.Inactive => this.OnChatInactive,
				ChatState.Gone => this.OnChatGone,
				_ => null
			};

			if (!(Specific is null))
				await Specific.Raise(this, e);
		}

		private async Task RaiseChatStateSupportDeterminedAsync(string BareJid, bool Supported)
		{
			ChatStateSupportDeterminedEventHandler Callback = this.OnChatStateSupportDetermined;
			if (!(Callback is null))
			{
				try
				{
					Task Task = Callback(BareJid, Supported);
					if (!(Task is null))
						await Task.ConfigureAwait(false);
				}
				catch (Exception Ex)
				{
					this.Exception(Ex);
				}
			}
		}

		private static string ComposeThreadKey(string BareJid, string ThreadId)
		{
			return BareJid + "|" + ThreadId;
		}

		private static string ComposeStandaloneStateKey(string BareJid, string ThreadId)
		{
			if (string.IsNullOrEmpty(ThreadId))
				return BareJid;

			return ComposeThreadKey(BareJid, ThreadId);
		}

		private static ChatState GetChatStateFromLocalName(string LocalName)
		{
			return LocalName switch
			{
				"composing" => ChatState.Composing,
				"paused" => ChatState.Paused,
				"inactive" => ChatState.Inactive,
				"gone" => ChatState.Gone,
				_ => ChatState.Active
			};
		}

		private static string GetLocalName(ChatState State)
		{
			return State switch
			{
				ChatState.Composing => "composing",
				ChatState.Paused => "paused",
				ChatState.Inactive => "inactive",
				ChatState.Gone => "gone",
				_ => "active"
			};
		}

		private void EvaluateChatStateCapabilities(PresenceEventArgs e)
		{
			if (!this.EnableChatStateNotifications || e.Type != PresenceType.Available)
				return;

			string BareJid = e.FromBareJID;
			if (string.IsNullOrEmpty(BareJid))
				return;

			string Node = e.EntityCapabilityNode;
			string Version = e.EntityCapabilityVersion;
			if (string.IsNullOrEmpty(Node) || string.IsNullOrEmpty(Version))
				return;

			string Key = Node + "#" + Version;
			bool Supported;

			lock (this.synchObject)
			{
				if (this.chatStateCapabilitiesByNodeVersion.TryGetValue(Key, out Supported))
				{
					_ = this.ApplyCapabilityNegotiationAsync(BareJid, Supported);
					return;
				}

				if (!this.pendingChatStateCapabilityRequests.Add(Key))
					return;
			}

			string DiscoNode = Node + "#" + Version;

			try
			{
				_ = this.client.SendServiceDiscoveryRequest(e.From, DiscoNode, async (sender, args) =>
				{
					bool Supports = false;

					if (args.Ok)
						Supports = args.HasFeature(NamespaceChatStates);

					lock (this.synchObject)
					{
						this.chatStateCapabilitiesByNodeVersion[Key] = Supports;
						this.pendingChatStateCapabilityRequests.Remove(Key);
					}

					await this.ApplyCapabilityNegotiationAsync(BareJid, Supports);
				}, null);
			}
			catch (Exception Ex)
			{
				lock (this.synchObject)
				{
					this.pendingChatStateCapabilityRequests.Remove(Key);
				}

				this.Exception(Ex);
			}
		}

		private async Task ApplyCapabilityNegotiationAsync(string BareJid, bool Supported)
		{
			if (string.IsNullOrEmpty(BareJid))
				return;

			if (Supported)
			{
				if (this.TryUpdateNegotiationState(BareJid, ChatStateNegotiationState.Supported, out ChatStateNegotiationState Previous) &&
					Previous != ChatStateNegotiationState.Supported)
				{
					await this.RaiseChatStateSupportDeterminedAsync(BareJid, true);
				}
			}
			else
			{
				bool Changed = false;

				lock (this.synchObject)
				{
					if (!this.chatStateNegotiation.TryGetValue(BareJid, out ChatStateNegotiationState Previous) || Previous != ChatStateNegotiationState.NotSupported)
					{
						this.chatStateNegotiation[BareJid] = ChatStateNegotiationState.NotSupported;
						Changed = true;
					}
				}

				if (Changed)
					await this.RaiseChatStateSupportDeterminedAsync(BareJid, false);
			}
		}

		private async Task EvaluateIncomingChatStateSupport(MessageEventArgs e)
		{
			if (e is null)
				return;

			string BareJid = XmppClient.GetBareJID(e.From);
			if (string.IsNullOrEmpty(BareJid))
				BareJid = e.From;
			if (string.IsNullOrEmpty(BareJid))
				return;

			if (!string.IsNullOrEmpty(e.ThreadID) && (e.Type == MessageType.Chat || e.Type == MessageType.GroupChat))
			{
				lock (this.synchObject)
				{
					this.currentThreadIdPerContact[BareJid] = e.ThreadID;
				}
			}

			if (!this.EnableChatStateNotifications || e.Type != MessageType.Chat)
				return;

			bool HasChatState = false;
			foreach (XmlNode Node in e.Message.ChildNodes)
			{
				if (Node is XmlElement Element && Element.NamespaceURI == NamespaceChatStates)
				{
					HasChatState = true;
					break;
				}
			}

			if (HasChatState)
			{
				if (this.TryUpdateNegotiationState(BareJid, ChatStateNegotiationState.Supported, out ChatStateNegotiationState Previous) &&
					Previous != ChatStateNegotiationState.Supported)
				{
					await this.RaiseChatStateSupportDeterminedAsync(BareJid, true);
				}
			}
			else
			{
				bool Changed = false;
				lock (this.synchObject)
				{
					if (this.chatStateNegotiation.TryGetValue(BareJid, out ChatStateNegotiationState Previous) &&
						Previous == ChatStateNegotiationState.Requested)
					{
						this.chatStateNegotiation[BareJid] = ChatStateNegotiationState.NotSupported;
						Changed = true;
					}
				}

				if (Changed)
					await this.RaiseChatStateSupportDeterminedAsync(BareJid, false);
			}
		}

		private static bool HasDelayedDelivery(MessageEventArgs e)
		{
			if (e?.Message is null)
				return false;

			foreach (XmlNode Node in e.Message.ChildNodes)
			{
				if (Node is XmlElement Element)
				{
					if (Element.LocalName == "delay" && Element.NamespaceURI == NamespaceDelayedDelivery)
						return true;

					if (Element.LocalName == "x" && Element.NamespaceURI == NamespaceDelayedDeliveryLegacy)
						return true;
				}
			}

			return false;
		}

		private string EnsureOutgoingThreadId(string BareJid, string RequestedThreadId, bool IsContentMessage)
		{
			if (string.IsNullOrEmpty(BareJid))
				return RequestedThreadId ?? string.Empty;

			lock (this.synchObject)
			{
				string threadId = RequestedThreadId;

				if (!string.IsNullOrEmpty(threadId))
				{
					this.currentThreadIdPerContact[BareJid] = threadId;
					return threadId;
				}

				if (!IsContentMessage)
					return string.Empty;

				if (this.currentThreadIdPerContact.TryGetValue(BareJid, out string Current) &&
					!this.endedThreads.Contains(ComposeThreadKey(BareJid, Current)))
				{
					return Current;
				}

				string NewThread;
				do
				{
					NewThread = Guid.NewGuid().ToString("N");
				}
				while (this.endedThreads.Contains(ComposeThreadKey(BareJid, NewThread)));

				this.currentThreadIdPerContact[BareJid] = NewThread;
				return NewThread;
			}
		}
	}
}
