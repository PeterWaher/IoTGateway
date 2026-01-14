using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using Waher.Networking.XMPP.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;
using Waher.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Chat.ChatStates;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppChatStateTests : CommunicationTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			await DisposeSnifferAndLog();
		}

		[TestMethod]
		public async Task ChatStates_Test_01_ImplicitNegotiation()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			Assert.IsFalse(chatClient1.EnableChatStateNotifications, "Client 1 should disable chat state notifications by default.");
			Assert.IsFalse(chatClient2.EnableChatStateNotifications, "Client 2 should disable chat state notifications by default.");

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent client2Active = new ManualResetEvent(false);
			using ManualResetEvent client1Active = new ManualResetEvent(false);
			using ManualResetEvent client1Supported = new ManualResetEvent(false);
			using ManualResetEvent client2Supported = new ManualResetEvent(false);

			ChatStateSupportDeterminedEventHandler client1SupportHandler = (bareJid, supported) =>
			{
				if (supported && bareJid == this.client2.BareJID)
					client1Supported.Set();

				return Task.CompletedTask;
			};

			ChatStateSupportDeterminedEventHandler client2SupportHandler = (bareJid, supported) =>
			{
				if (supported && bareJid == this.client1.BareJID)
					client2Supported.Set();

				return Task.CompletedTask;
			};

			int responseSent = 0;

			EventHandlerAsync<ChatStateEventArgs> client2StateHandler = async (sender, e) =>
			{
				if (e.State == ChatState.Active && e.Message.FromBareJID == this.client1.BareJID)
				{
					client2Active.Set();

					if (Interlocked.Exchange(ref responseSent, 1) == 0)
						await chatClient2.SendChatContentMessage(this.client1.BareJID, "Hello back!", string.Empty, string.Empty, string.Empty, string.Empty);
				}
			};

			EventHandlerAsync<ChatStateEventArgs> client1StateHandler = (sender, e) =>
			{
				if (e.State == ChatState.Active && e.Message.FromBareJID == this.client2.BareJID)
					client1Active.Set();

				return Task.CompletedTask;
			};

			chatClient1.OnChatStateSupportDetermined += client1SupportHandler;
			chatClient2.OnChatStateSupportDetermined += client2SupportHandler;
			chatClient2.OnChatStateChanged += client2StateHandler;
			chatClient1.OnChatStateChanged += client1StateHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Hello there!", string.Empty, string.Empty, string.Empty, string.Empty);

				Assert.IsTrue(client2Active.WaitOne(10000), "Client 2 did not observe the active chat state.");
				Assert.IsTrue(client2Supported.WaitOne(10000), "Client 2 did not resolve chat state support.");
				Assert.IsTrue(client1Supported.WaitOne(10000), "Client 1 did not resolve chat state support.");
				Assert.IsTrue(client1Active.WaitOne(10000), "Client 1 did not observe the active chat state reply.");
			}
			finally
			{
				chatClient1.OnChatStateSupportDetermined -= client1SupportHandler;
				chatClient2.OnChatStateSupportDetermined -= client2SupportHandler;
				chatClient2.OnChatStateChanged -= client2StateHandler;
				chatClient1.OnChatStateChanged -= client1StateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_02_NegotiationFailure()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = false;

			using ManualResetEvent notSupported = new ManualResetEvent(false);
			using ManualResetEvent stateReceived = new ManualResetEvent(false);

			int responseSent = 0;

			ChatStateSupportDeterminedEventHandler client1SupportHandler = (bareJid, supported) =>
			{
				if (bareJid == this.client2.BareJID && !supported)
					notSupported.Set();

				return Task.CompletedTask;
			};

			EventHandlerAsync<ChatStateEventArgs> client2NegotiationHandler = async (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID &&
					e.State == ChatState.Active &&
					Interlocked.Exchange(ref responseSent, 1) == 0)
				{
					await this.client2.SendMessage(MessageType.Chat, this.client1.BareJID, string.Empty, "Notifications disabled.", string.Empty, string.Empty, string.Empty, string.Empty);
				}
			};

			EventHandlerAsync<ChatStateEventArgs> client2StateHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID)
					stateReceived.Set();

				return Task.CompletedTask;
			};

			chatClient1.OnChatStateSupportDetermined += client1SupportHandler;
			chatClient2.OnChatStateChanged += client2NegotiationHandler;
			chatClient2.OnChatStateChanged += client2StateHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Testing negotiation.", string.Empty, string.Empty, string.Empty, string.Empty);

				Assert.IsTrue(notSupported.WaitOne(10000), "Client 1 did not detect lack of chat state support.");
				Assert.IsFalse(chatClient1.IsChatStateSupported(this.client2.BareJID), "Client 1 should mark contact as not supporting chat states.");

				stateReceived.Reset();
				await chatClient1.SendChatState(this.client2.BareJID, ChatState.Composing);

				Assert.IsFalse(stateReceived.WaitOne(1000), "Standalone state should be suppressed after negotiation failure.");
			}
			finally
			{
				chatClient1.OnChatStateSupportDetermined -= client1SupportHandler;
				chatClient2.OnChatStateChanged -= client2NegotiationHandler;
				chatClient2.OnChatStateChanged -= client2StateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_03_ThreadRollsAfterGone()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent firstMessage = new ManualResetEvent(false);
			using ManualResetEvent goneReceived = new ManualResetEvent(false);
			using ManualResetEvent duplicateGoneReceived = new ManualResetEvent(false);
			using ManualResetEvent secondMessage = new ManualResetEvent(false);

			string initialThreadId = string.Empty;
			string newThreadId = string.Empty;
			int messageCount = 0;
			int goneCount = 0;

			EventHandlerAsync<ChatStateEventArgs> stateHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID != this.client1.BareJID)
					return Task.CompletedTask;

				if (e.State == ChatState.Gone)
				{
					if (Interlocked.Increment(ref goneCount) == 1)
						goneReceived.Set();
					else
						duplicateGoneReceived.Set();
				}
				else if (e.State == ChatState.Active)
				{
					MessageEventArgs msg = e.Message;
					int count = Interlocked.Increment(ref messageCount);

					if (count == 1)
					{
						initialThreadId = msg.ThreadID;
						firstMessage.Set();
					}
					else if (count == 2)
					{
						newThreadId = msg.ThreadID;
						secondMessage.Set();
					}
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatStateChanged += stateHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Start thread", string.Empty, string.Empty, "initial-thread", string.Empty);

				Assert.IsTrue(firstMessage.WaitOne(10000), "Initial thread message not received.");
				Assert.AreEqual("initial-thread", initialThreadId, "Initial thread identifier mismatch.");

				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Gone, MessageType.Chat, "initial-thread");
				Assert.IsTrue(goneReceived.WaitOne(10000), "Gone state was not received.");

				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Gone, MessageType.Chat, "initial-thread");
				Assert.IsFalse(duplicateGoneReceived.WaitOne(1000), "Gone state should be one-shot per thread.");

				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Follow-up message", string.Empty, string.Empty, string.Empty, string.Empty);

				Assert.IsTrue(secondMessage.WaitOne(10000), "Follow-up message not received.");
				Assert.IsFalse(string.IsNullOrEmpty(newThreadId), "New message should generate a thread identifier.");
				Assert.AreNotEqual(initialThreadId, newThreadId, "Thread identifier should change after gone state.");
			}
			finally
			{
				chatClient2.OnChatStateChanged -= stateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_04_GroupChatGoneSuppressed()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent stateReceived = new ManualResetEvent(false);

			EventHandlerAsync<ChatStateEventArgs> stateHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID && e.State == ChatState.Gone)
					stateReceived.Set();

				return Task.CompletedTask;
			};

			chatClient2.OnChatStateChanged += stateHandler;

			try
			{
				Task send = chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Gone, MessageType.GroupChat);

				Assert.IsNotNull(send);
				Assert.IsTrue(send.IsCompleted, "Groupchat gone should be suppressed immediately.");
				Assert.IsFalse(stateReceived.WaitOne(1000), "Gone state must not be delivered in groupchat context.");
			}
			finally
			{
				chatClient2.OnChatStateChanged -= stateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_05_IncomingGroupChatGoneIgnored()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent goneReceived = new ManualResetEvent(false);

			EventHandlerAsync<ChatStateEventArgs> stateHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID && e.State == ChatState.Gone)
					goneReceived.Set();

				return Task.CompletedTask;
			};

			chatClient2.OnChatStateChanged += stateHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='groupchat'>" +
					"<gone xmlns='http://jabber.org/protocol/chatstates'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsFalse(goneReceived.WaitOne(1000), "Incoming groupchat gone should be ignored.");
			}
			finally
			{
				chatClient2.OnChatStateChanged -= stateHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_06_DuplicateStandaloneSuppression()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent composingReceived = new ManualResetEvent(false);
			using ManualResetEvent pausedReceived = new ManualResetEvent(false);
			using ManualResetEvent inactiveReceived = new ManualResetEvent(false);
			using ManualResetEvent goneReceived = new ManualResetEvent(false);

			int composingCount = 0;
			int pausedCount = 0;
			int inactiveCount = 0;
			int goneCount = 0;

			EventHandlerAsync<ChatStateEventArgs> stateHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID != this.client1.BareJID)
					return Task.CompletedTask;

				switch (e.State)
				{
					case ChatState.Composing:
						if (Interlocked.Increment(ref composingCount) == 1)
							composingReceived.Set();
						break;

					case ChatState.Paused:
						if (Interlocked.Increment(ref pausedCount) == 1)
							pausedReceived.Set();
						break;

					case ChatState.Inactive:
						if (Interlocked.Increment(ref inactiveCount) == 1)
							inactiveReceived.Set();
						break;

					case ChatState.Gone:
						if (Interlocked.Increment(ref goneCount) == 1)
							goneReceived.Set();
						break;
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatStateChanged += stateHandler;

			try
			{
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Composing, MessageType.Chat, "dup-thread-composing");
				Assert.IsTrue(composingReceived.WaitOne(10000), "Composing state not received.");
				composingReceived.Reset();
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Composing, MessageType.Chat, "dup-thread-composing");
				Assert.IsFalse(composingReceived.WaitOne(1000), "Duplicate composing state should be suppressed.");

				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Paused, MessageType.Chat, "dup-thread-paused");
				Assert.IsTrue(pausedReceived.WaitOne(10000), "Paused state not received.");
				pausedReceived.Reset();
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Paused, MessageType.Chat, "dup-thread-paused");
				Assert.IsFalse(pausedReceived.WaitOne(1000), "Duplicate paused state should be suppressed.");

				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Inactive, MessageType.Chat, "dup-thread-inactive");
				Assert.IsTrue(inactiveReceived.WaitOne(10000), "Inactive state not received.");
				inactiveReceived.Reset();
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Inactive, MessageType.Chat, "dup-thread-inactive");
				Assert.IsFalse(inactiveReceived.WaitOne(1000), "Duplicate inactive state should be suppressed.");

				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Gone, MessageType.Chat, "dup-thread-gone");
				Assert.IsTrue(goneReceived.WaitOne(10000), "Gone state not received.");
				goneReceived.Reset();
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Gone, MessageType.Chat, "dup-thread-gone");
				Assert.IsFalse(goneReceived.WaitOne(1000), "Duplicate gone state should be suppressed.");
			}
			finally
			{
				chatClient2.OnChatStateChanged -= stateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_07_ThreadPropagationOnStandalone()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent composingReceived = new ManualResetEvent(false);
			string composingThreadId = string.Empty;

			EventHandlerAsync<MessageEventArgs> composingHandler = (sender, e) =>
			{
				if (e.FromBareJID == this.client1.BareJID)
				{
					composingThreadId = e.ThreadID;
					composingReceived.Set();
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatComposing += composingHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Thread seed", string.Empty, string.Empty, "thread-propagation", string.Empty);
				await chatClient1.SendComposing(this.client2.BareJID);

				Assert.IsTrue(composingReceived.WaitOne(10000), "Composing state not received.");
				Assert.AreEqual("thread-propagation", composingThreadId, "Standalone chat state should reuse the current thread.");
			}
			finally
			{
				chatClient2.OnChatComposing -= composingHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_08_ActiveEmbeddedOnlyInContentMessages()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent activeReceived = new ManualResetEvent(false);
			int activeCount = 0;

			EventHandlerAsync<ChatStateEventArgs> stateHandler = (sender, e) =>
			{
				if (e.State == ChatState.Active && e.Message.FromBareJID == this.client1.BareJID)
				{
					Interlocked.Increment(ref activeCount);
					activeReceived.Set();
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatStateChanged += stateHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, string.Empty, "Subject-only", string.Empty, string.Empty, string.Empty);
				Assert.IsTrue(activeReceived.WaitOne(10000), "Active state should be embedded when subject is present.");

				activeReceived.Reset();
				int activeBefore = activeCount;
				await chatClient1.SendChatContentMessage(this.client2.BareJID, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
				Assert.IsFalse(activeReceived.WaitOne(1000), "Active state should not be embedded for empty content.");
				Assert.AreEqual(activeBefore, activeCount, "Active state should not be emitted for empty content.");
			}
			finally
			{
				chatClient2.OnChatStateChanged -= stateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_09_NonChatStateMessageDoesNotMarkNotSupported()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent notSupported = new ManualResetEvent(false);
			bool messageSent = false;

			ChatStateSupportDeterminedEventHandler supportHandler = (bareJid, supported) =>
			{
				if (messageSent && bareJid == this.client2.BareJID && !supported)
					notSupported.Set();

				return Task.CompletedTask;
			};

			chatClient1.OnChatStateSupportDetermined += supportHandler;

			try
			{
				messageSent = true;
				await this.client2.SendMessage(MessageType.Chat, this.client1.BareJID, string.Empty, "Plain message", string.Empty, string.Empty, string.Empty, string.Empty);
				Assert.IsFalse(notSupported.WaitOne(1000), "Non-chatstate message should not mark contact as not supported unless requested.");
			}
			finally
			{
				chatClient1.OnChatStateSupportDetermined -= supportHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_10_EnableDisableTogglesEmbedding()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent activeReceived = new ManualResetEvent(false);
			int activeCount = 0;

			EventHandlerAsync<ChatStateEventArgs> stateHandler = (sender, e) =>
			{
				if (e.State == ChatState.Active && e.Message.FromBareJID == this.client1.BareJID)
				{
					Interlocked.Increment(ref activeCount);
					activeReceived.Set();
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatStateChanged += stateHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Active on", string.Empty, string.Empty, string.Empty, string.Empty);
				Assert.IsTrue(activeReceived.WaitOne(10000), "Active state should be embedded when enabled.");

				chatClient1.EnableChatStateNotifications = false;
				activeReceived.Reset();
				int before = activeCount;
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Active off", string.Empty, string.Empty, string.Empty, string.Empty);
				Assert.IsFalse(activeReceived.WaitOne(1000), "Active state should not be embedded when disabled.");
				Assert.AreEqual(before, activeCount, "Active state should not be emitted when disabled.");

				chatClient1.EnableChatStateNotifications = true;
				activeReceived.Reset();
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Active on again", string.Empty, string.Empty, string.Empty, string.Empty);
				Assert.IsTrue(activeReceived.WaitOne(10000), "Active state should be embedded again after re-enable.");
			}
			finally
			{
				chatClient2.OnChatStateChanged -= stateHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_11_GroupChatComposingIsSent()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			chatClient1.EnableChatStateNotifications = true;

			InMemorySniffer sniffer = new InMemorySniffer(500, "ChatState-Send-Sniffer");
			this.client1.Add(sniffer);

			try
			{
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Composing, MessageType.GroupChat);
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Paused, MessageType.GroupChat);
				await chatClient1.SendStandaloneChatState(this.client2.BareJID, ChatState.Inactive, MessageType.GroupChat);
				await Task.Delay(250);

				bool foundComposing = false;
				bool foundPaused = false;
				bool foundInactive = false;
				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is SnifferTxText tx &&
						tx.Text.Contains("type='groupchat'") &&
						tx.Text.Contains("http://jabber.org/protocol/chatstates"))
					{
						if (tx.Text.Contains("<composing"))
							foundComposing = true;
						else if (tx.Text.Contains("<paused"))
							foundPaused = true;
						else if (tx.Text.Contains("<inactive"))
							foundInactive = true;
					}
				}

				Assert.IsTrue(foundComposing, "Groupchat composing state should be sent.");
				Assert.IsTrue(foundPaused, "Groupchat paused state should be sent.");
				Assert.IsTrue(foundInactive, "Groupchat inactive state should be sent.");
			}
			finally
			{
				this.client1.Remove(sniffer);
				await sniffer.DisposeAsync();
				chatClient1.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatStates_Test_12_ResetStateClearsNegotiationAndThreadTracking()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableChatStateNotifications = true;
			chatClient2.EnableChatStateNotifications = true;

			using ManualResetEvent supportedReceived = new ManualResetEvent(false);

			ChatStateSupportDeterminedEventHandler supportHandler = (bareJid, supported) =>
			{
				if (supported && bareJid == this.client2.BareJID)
					supportedReceived.Set();

				return Task.CompletedTask;
			};

			chatClient1.OnChatStateSupportDetermined += supportHandler;

			try
			{
				await chatClient1.SendChatContentMessage(this.client2.BareJID, "Seed thread", string.Empty, string.Empty, "reset-thread", string.Empty);
				await chatClient2.SendComposing(this.client1.BareJID);

				Assert.IsTrue(supportedReceived.WaitOne(10000), "Chat state support not detected before reset.");

				int negotiationCountBefore = GetPrivateCollectionCount(chatClient1, "chatStateNegotiation");
				int threadCountBefore = GetPrivateCollectionCount(chatClient1, "currentThreadIdPerContact");

				Assert.IsTrue(negotiationCountBefore > 0, "Negotiation state should be tracked before reset.");
				Assert.IsTrue(threadCountBefore > 0, "Thread tracking should be populated before reset.");

				MethodInfo stateChanged = typeof(ChatClient).GetMethod("Client_OnStateChanged", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.IsNotNull(stateChanged, "Client_OnStateChanged handler not found.");

				Task stateTask = (Task)stateChanged.Invoke(chatClient1, new object[] { this.client1, XmppState.Offline });
				if (stateTask != null)
					await stateTask;

				int negotiationCountAfter = GetPrivateCollectionCount(chatClient1, "chatStateNegotiation");
				int threadCountAfter = GetPrivateCollectionCount(chatClient1, "currentThreadIdPerContact");

				Assert.AreEqual(0, negotiationCountAfter, "Negotiation state should be cleared on offline.");
				Assert.AreEqual(0, threadCountAfter, "Thread tracking should be cleared on offline.");
			}
			finally
			{
				chatClient1.OnChatStateSupportDetermined -= supportHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}

			static int GetPrivateCollectionCount(ChatClient client, string fieldName)
			{
				FieldInfo field = typeof(ChatClient).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.IsNotNull(field, "Field not found: " + fieldName);

				object value = field.GetValue(client);
				PropertyInfo countProperty = value?.GetType().GetProperty("Count");
				Assert.IsNotNull(countProperty, "Count property not found: " + fieldName);

				return (int)countProperty.GetValue(value);
			}
		}
	}
}
