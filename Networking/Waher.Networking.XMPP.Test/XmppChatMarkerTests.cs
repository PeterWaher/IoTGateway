using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Chat.Markers;
using Waher.Networking.XMPP.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppChatMarkerTests : CommunicationTests
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
		public async Task ChatMarkers_Test_01_MarkableInbound()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableChatMarkers = true;

			using ManualResetEvent markerReceived = new ManualResetEvent(false);
			string markerId = string.Empty;
			ChatMarkerType markerType = ChatMarkerType.Markable;

			EventHandlerAsync<ChatMarkerEventArgs> markerHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID)
				{
					markerType = e.MarkerType;
					markerId = e.Id;
					markerReceived.Set();
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatMarker += markerHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat' id='m1'>" +
					"<body>Markable</body>" +
					"<markable xmlns='urn:xmpp:chat-markers:0'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(markerReceived.WaitOne(1000), "Markable marker not received.");
				Assert.AreEqual(ChatMarkerType.Markable, markerType, "Marker type mismatch.");
				Assert.AreEqual("m1", markerId, "Marker id mismatch.");
			}
			finally
			{
				chatClient2.OnChatMarker -= markerHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatMarkers_Test_02_DisplayedInbound()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableChatMarkers = true;

			using ManualResetEvent markerReceived = new ManualResetEvent(false);
			string markerId = string.Empty;
			ChatMarkerType markerType = ChatMarkerType.Markable;

			EventHandlerAsync<ChatMarkerEventArgs> markerHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID)
				{
					markerType = e.MarkerType;
					markerId = e.Id;
					markerReceived.Set();
				}

				return Task.CompletedTask;
			};

			chatClient2.OnChatMarker += markerHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat'>" +
					"<displayed xmlns='urn:xmpp:chat-markers:0' id='orig1'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(markerReceived.WaitOne(1000), "Displayed marker not received.");
				Assert.AreEqual(ChatMarkerType.Displayed, markerType, "Marker type mismatch.");
				Assert.AreEqual("orig1", markerId, "Marker id mismatch.");
			}
			finally
			{
				chatClient2.OnChatMarker -= markerHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatMarkers_Test_03_GroupChatDoesNotAutoDisplay()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableChatMarkers = true;
			chatClient2.AutoSendDisplayedMarkers = true;

			using ManualResetEvent markerReceived = new ManualResetEvent(false);

			EventHandlerAsync<ChatMarkerEventArgs> markerHandler = (sender, e) =>
			{
				if (e.Message.FromBareJID == this.client1.BareJID)
					markerReceived.Set();

				return Task.CompletedTask;
			};

			InMemorySniffer sniffer = new InMemorySniffer(500, "Marker-Groupchat-Sniffer");
			this.client2.Add(sniffer);

			chatClient2.OnChatMarker += markerHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='groupchat' id='gm1'>" +
					"<body>Group markable</body>" +
					"<markable xmlns='urn:xmpp:chat-markers:0'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(markerReceived.WaitOne(1000), "Groupchat markable marker not received.");

				bool displayedSent = false;
				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is SnifferTxText tx &&
						tx.Text.Contains("<displayed") &&
						tx.Text.Contains("urn:xmpp:chat-markers:0"))
					{
						displayedSent = true;
						break;
					}
				}

				Assert.IsFalse(displayedSent, "Displayed marker should not be auto-sent for groupchat.");
			}
			finally
			{
				chatClient2.OnChatMarker -= markerHandler;
				this.client2.Remove(sniffer);
				sniffer.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatMarkers_Test_04_OutboundMarkersSent()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			chatClient1.EnableChatMarkers = true;

			InMemorySniffer sniffer = new InMemorySniffer(500, "Marker-Outbound-Sniffer");
			this.client1.Add(sniffer);

			try
			{
				await chatClient1.SendMarkable(this.client2.BareJID, "mk1");
				await chatClient1.SendDisplayedMarker(this.client2.BareJID, "mk1");
				await Task.Delay(250);

				bool markableSent = false;
				bool displayedSent = false;

				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is SnifferTxText tx && tx.Text.Contains("urn:xmpp:chat-markers:0"))
					{
						if (tx.Text.Contains("<markable") && tx.Text.Contains("id='mk1'"))
							markableSent = true;
						else if (tx.Text.Contains("<displayed") && tx.Text.Contains("id='mk1'"))
							displayedSent = true;
					}
				}

				Assert.IsTrue(markableSent, "Markable marker not sent.");
				Assert.IsTrue(displayedSent, "Displayed marker not sent.");
			}
			finally
			{
				this.client1.Remove(sniffer);
				sniffer.Dispose();
				chatClient1.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatMarkers_Test_05_MarkerOnlyDoesNotTriggerChatMessage()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableChatMarkers = true;

			using ManualResetEvent markerReceived = new ManualResetEvent(false);
			using ManualResetEvent chatMessageReceived = new ManualResetEvent(false);

			EventHandlerAsync<ChatMarkerEventArgs> markerHandler = (sender, e) =>
			{
				markerReceived.Set();
				return Task.CompletedTask;
			};

			EventHandlerAsync<MessageEventArgs> chatHandler = (sender, e) =>
			{
				chatMessageReceived.Set();
				return Task.CompletedTask;
			};

			chatClient2.OnChatMarker += markerHandler;
			this.client2.OnChatMessage += chatHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat' id='m2'>" +
					"<markable xmlns='urn:xmpp:chat-markers:0'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(markerReceived.WaitOne(1000), "Marker event was not raised.");
				Assert.IsFalse(chatMessageReceived.WaitOne(500), "Marker-only stanza should not trigger chat message handlers.");
			}
			finally
			{
				chatClient2.OnChatMarker -= markerHandler;
				this.client2.OnChatMessage -= chatHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task ChatMarkers_Test_06_DisplayedDoesNotAutoSend()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableChatMarkers = true;
			chatClient2.AutoSendDisplayedMarkers = true;

			using ManualResetEvent markerReceived = new ManualResetEvent(false);

			EventHandlerAsync<ChatMarkerEventArgs> markerHandler = (sender, e) =>
			{
				if (e.MarkerType == ChatMarkerType.Displayed)
					markerReceived.Set();

				return Task.CompletedTask;
			};

			InMemorySniffer sniffer = new InMemorySniffer(500, "Marker-Displayed-Sniffer");
			this.client2.Add(sniffer);

			chatClient2.OnChatMarker += markerHandler;

			try
			{
				int baseline = sniffer.ToArray().Length;

				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat'>" +
					"<displayed xmlns='urn:xmpp:chat-markers:0' id='orig2'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(markerReceived.WaitOne(1000), "Displayed marker not received.");

				SnifferEvent[] events = sniffer.ToArray();
				bool displayedSent = false;

				for (int i = baseline; i < events.Length; i++)
				{
					if (events[i] is SnifferTxText tx &&
						tx.Text.Contains("<displayed") &&
						tx.Text.Contains("urn:xmpp:chat-markers:0"))
					{
						displayedSent = true;
						break;
					}
				}

				Assert.IsFalse(displayedSent, "Displayed marker should not be auto-sent in response to displayed.");
			}
			finally
			{
				chatClient2.OnChatMarker -= markerHandler;
				this.client2.Remove(sniffer);
				sniffer.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}
	}
}
