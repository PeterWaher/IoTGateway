using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;
using Waher.Networking.XMPP.Chat;
using Waher.Networking.XMPP.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppDeliveryReceiptTests : CommunicationTests
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
		public async Task DeliveryReceipts_Test_01_AutoAckRequest()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableDeliveryReceipts = true;
			chatClient2.EnableDeliveryReceipts = true;
			chatClient2.AutoRespondToReceiptRequests = true;

			using ManualResetEvent receiptReceived = new ManualResetEvent(false);

			string messageId = this.client1.NextId();

			EventHandlerAsync<DeliveryReceiptEventArgs> receivedHandler = (sender, e) =>
			{
				if (e.IsReceived && e.Id == messageId && e.Message.FromBareJID == this.client2.BareJID)
					receiptReceived.Set();

				return Task.CompletedTask;
			};

			chatClient1.OnReceiptReceived += receivedHandler;

			try
			{
				await chatClient1.SendChatMessageWithReceiptRequest(this.client2.BareJID, "Receipt please.", messageId);
				Assert.IsTrue(receiptReceived.WaitOne(10000), "Receipt response was not received.");
			}
			finally
			{
				chatClient1.OnReceiptReceived -= receivedHandler;
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task DeliveryReceipts_Test_02_NoAckWithoutId()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableDeliveryReceipts = true;
			chatClient2.AutoRespondToReceiptRequests = true;

			using ManualResetEvent requestReceived = new ManualResetEvent(false);

			EventHandlerAsync<DeliveryReceiptEventArgs> requestHandler = (sender, e) =>
			{
				if (e.IsRequest)
					requestReceived.Set();

				return Task.CompletedTask;
			};

			InMemorySniffer sniffer = new InMemorySniffer(500, "Receipt-Ack-Sniffer");
			this.client2.Add(sniffer);

			chatClient2.OnReceiptRequest += requestHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat'>" +
					"<body>No id.</body>" +
					"<request xmlns='urn:xmpp:receipts'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(requestReceived.WaitOne(1000), "Receipt request event was not raised.");

				bool ackSent = false;
				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is SnifferTxText tx &&
						tx.Text.Contains("<received") &&
						tx.Text.Contains("urn:xmpp:receipts"))
					{
						ackSent = true;
						break;
					}
				}

				Assert.IsFalse(ackSent, "Auto-ack should not be sent without a stanza id.");
			}
			finally
			{
				chatClient2.OnReceiptRequest -= requestHandler;
				this.client2.Remove(sniffer);
				sniffer.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task DeliveryReceipts_Test_03_NoAckForDelayedMessage()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableDeliveryReceipts = true;
			chatClient2.AutoRespondToReceiptRequests = true;

			using ManualResetEvent requestReceived = new ManualResetEvent(false);

			EventHandlerAsync<DeliveryReceiptEventArgs> requestHandler = (sender, e) =>
			{
				if (e.IsRequest)
					requestReceived.Set();

				return Task.CompletedTask;
			};

			InMemorySniffer sniffer = new InMemorySniffer(500, "Receipt-Delay-Sniffer");
			this.client2.Add(sniffer);

			chatClient2.OnReceiptRequest += requestHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat' id='d1'>" +
					"<body>Delayed.</body>" +
					"<delay xmlns='urn:xmpp:delay' stamp='2020-01-01T00:00:00Z'/>" +
					"<request xmlns='urn:xmpp:receipts'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(requestReceived.WaitOne(1000), "Receipt request event was not raised.");

				bool ackSent = false;
				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is SnifferTxText tx &&
						tx.Text.Contains("<received") &&
						tx.Text.Contains("urn:xmpp:receipts"))
					{
						ackSent = true;
						break;
					}
				}

				Assert.IsFalse(ackSent, "Auto-ack should not be sent for delayed messages.");
			}
			finally
			{
				chatClient2.OnReceiptRequest -= requestHandler;
				this.client2.Remove(sniffer);
				sniffer.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task DeliveryReceipts_Test_04_AckIsReceiptOnly()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			ChatClient chatClient2 = new ChatClient(this.client2);

			chatClient1.EnableDeliveryReceipts = true;
			chatClient2.EnableDeliveryReceipts = true;
			chatClient2.AutoRespondToReceiptRequests = true;

			InMemorySniffer sniffer = new InMemorySniffer(500, "Receipt-Only-Sniffer");
			this.client2.Add(sniffer);

			using ManualResetEvent receiptReceived = new ManualResetEvent(false);

			string messageId = this.client1.NextId();

			EventHandlerAsync<DeliveryReceiptEventArgs> receivedHandler = (sender, e) =>
			{
				if (e.IsReceived && e.Id == messageId)
					receiptReceived.Set();

				return Task.CompletedTask;
			};

			chatClient1.OnReceiptReceived += receivedHandler;

			try
			{
				await chatClient1.SendChatMessageWithReceiptRequest(this.client2.BareJID, "Receipt only ack.", messageId);
				Assert.IsTrue(receiptReceived.WaitOne(10000), "Receipt response was not received.");

				bool receiptOnly = false;
				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is not SnifferTxText tx || !tx.Text.Contains("<message"))
						continue;

					try
					{
						XmlDocument doc = new XmlDocument();
						doc.LoadXml(tx.Text);

						if (doc.DocumentElement is null || doc.DocumentElement.LocalName != "message")
							continue;

						bool hasReceived = false;
						bool hasRequest = false;
						bool hasBody = false;
						bool hasSubject = false;

						foreach (XmlNode node in doc.DocumentElement.ChildNodes)
						{
							if (node is not XmlElement element)
								continue;

							if (element.LocalName == "received" && element.NamespaceURI == "urn:xmpp:receipts")
								hasReceived = true;
							else if (element.LocalName == "request" && element.NamespaceURI == "urn:xmpp:receipts")
								hasRequest = true;
							else if (element.LocalName == "body")
								hasBody = true;
							else if (element.LocalName == "subject")
								hasSubject = true;
						}

						if (hasReceived)
							receiptOnly = !hasRequest && !hasBody && !hasSubject;
					}
					catch (XmlException)
					{
						// Ignore non-stanza output
					}
				}

				Assert.IsTrue(receiptOnly, "Receipt ack should not include request, body, or subject.");
			}
			finally
			{
				chatClient1.OnReceiptReceived -= receivedHandler;
				this.client2.Remove(sniffer);
				sniffer.Dispose();
				chatClient1.Dispose();
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task DeliveryReceipts_Test_05_ReceiptOnlyDoesNotTriggerChatMessage()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableDeliveryReceipts = true;

			using ManualResetEvent receiptReceived = new ManualResetEvent(false);
			using ManualResetEvent chatMessageReceived = new ManualResetEvent(false);

			EventHandlerAsync<DeliveryReceiptEventArgs> receivedHandler = (sender, e) =>
			{
				if (e.IsReceived && e.Id == "r1")
					receiptReceived.Set();

				return Task.CompletedTask;
			};

			EventHandlerAsync<MessageEventArgs> chatHandler = (sender, e) =>
			{
				chatMessageReceived.Set();
				return Task.CompletedTask;
			};

			chatClient2.OnReceiptReceived += receivedHandler;
			this.client2.OnChatMessage += chatHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat'>" +
					"<received xmlns='urn:xmpp:receipts' id='r1'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(receiptReceived.WaitOne(1000), "Receipt event was not raised.");
				Assert.IsFalse(chatMessageReceived.WaitOne(500), "Receipt-only stanza should not trigger chat message handlers.");
			}
			finally
			{
				chatClient2.OnReceiptReceived -= receivedHandler;
				this.client2.OnChatMessage -= chatHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}
	}
}
