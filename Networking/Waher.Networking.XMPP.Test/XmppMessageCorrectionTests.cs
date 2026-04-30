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
	public class XmppMessageCorrectionTests : CommunicationTests
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
		public async Task MessageCorrection_Test_01_InboundReplace()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableMessageCorrection = true;

			using ManualResetEvent correctionReceived = new ManualResetEvent(false);
			string replaceId = string.Empty;
			string newBody = string.Empty;
			string newSubject = string.Empty;

			EventHandlerAsync<MessageCorrectionEventArgs> correctionHandler = (sender, e) =>
			{
				replaceId = e.ReplaceId;
				newBody = e.NewBody;
				newSubject = e.NewSubject;
				correctionReceived.Set();
				return Task.CompletedTask;
			};

			chatClient2.OnMessageCorrected += correctionHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat' id='c1'>" +
					"<subject>New subject</subject>" +
					"<body>New body</body>" +
					"<replace xmlns='urn:xmpp:message-correct:0' id='orig1'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(correctionReceived.WaitOne(1000), "Correction event not received.");
				Assert.AreEqual("orig1", replaceId, "Replace id mismatch.");
				Assert.AreEqual("New body", newBody, "Body mismatch.");
				Assert.AreEqual("New subject", newSubject, "Subject mismatch.");
			}
			finally
			{
				chatClient2.OnMessageCorrected -= correctionHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task MessageCorrection_Test_02_OutboundReplaceHasNewId()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			chatClient1.EnableMessageCorrection = true;

			InMemorySniffer sniffer = new InMemorySniffer(500, "Correction-Outbound-Sniffer");
			this.client1.Add(sniffer);

			try
			{
				await chatClient1.SendMessageCorrection(this.client2.BareJID, "orig42", "Corrected body", "Corrected subject", string.Empty, string.Empty);
				await Task.Delay(250);

				bool replaceFound = false;
				bool messageIdFound = false;
				bool hasOldMessageId = false;

				foreach (SnifferEvent evt in sniffer.ToArray())
				{
					if (evt is not SnifferTxText tx)
						continue;

					if (!tx.Text.Contains("<message"))
						continue;

					try
					{
						XmlDocument doc = new XmlDocument();
						doc.LoadXml(tx.Text);

						if (doc.DocumentElement is null || doc.DocumentElement.LocalName != "message")
							continue;

						string stanzaId = doc.DocumentElement.GetAttribute("id");
						if (!string.IsNullOrEmpty(stanzaId))
						{
							messageIdFound = true;
							if (stanzaId == "orig42")
								hasOldMessageId = true;
						}

						foreach (XmlNode node in doc.DocumentElement.ChildNodes)
						{
							if (node is XmlElement element &&
								element.LocalName == "replace" &&
								element.NamespaceURI == "urn:xmpp:message-correct:0" &&
								element.GetAttribute("id") == "orig42")
							{
								replaceFound = true;
								break;
							}
						}
					}
					catch (XmlException)
					{
						// Ignore non-stanza output
					}
				}

				Assert.IsTrue(replaceFound, "Correction replace element not sent.");
				Assert.IsTrue(messageIdFound, "Correction message should have a new stanza id.");
				Assert.IsFalse(hasOldMessageId, "Correction message id should not match the replaced id.");
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
		public async Task MessageCorrection_Test_03_NoSendForEmptyCorrection()
		{
			await this.ConnectClients();

			ChatClient chatClient1 = new ChatClient(this.client1);
			chatClient1.EnableMessageCorrection = true;

			InMemorySniffer sniffer = new InMemorySniffer(500, "Correction-Empty-Sniffer");
			this.client1.Add(sniffer);

			try
			{
				int baseline = sniffer.ToArray().Length;
				await chatClient1.SendMessageCorrection(this.client2.BareJID, "orig-empty", string.Empty, string.Empty, string.Empty, string.Empty);
				await Task.Delay(250);

				SnifferEvent[] events = sniffer.ToArray();
				bool replaceSent = false;

				for (int i = baseline; i < events.Length; i++)
				{
					if (events[i] is SnifferTxText tx &&
						tx.Text.Contains("<replace") &&
						tx.Text.Contains("urn:xmpp:message-correct:0"))
					{
						replaceSent = true;
						break;
					}
				}

				Assert.IsFalse(replaceSent, "Empty correction should not send a replace stanza.");
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
		public async Task MessageCorrection_Test_04_ReplaceOnlyDoesNotTriggerChatMessage()
		{
			await this.ConnectClients();

			ChatClient chatClient2 = new ChatClient(this.client2);
			chatClient2.EnableMessageCorrection = true;

			using ManualResetEvent correctionReceived = new ManualResetEvent(false);
			using ManualResetEvent chatMessageReceived = new ManualResetEvent(false);

			EventHandlerAsync<MessageCorrectionEventArgs> correctionHandler = (sender, e) =>
			{
				if (e.ReplaceId == "orig55")
					correctionReceived.Set();

				return Task.CompletedTask;
			};

			EventHandlerAsync<MessageEventArgs> chatHandler = (sender, e) =>
			{
				chatMessageReceived.Set();
				return Task.CompletedTask;
			};

			chatClient2.OnMessageCorrected += correctionHandler;
			this.client2.OnChatMessage += chatHandler;

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(
					"<message from='" + this.client1.FullJID + "' to='" + this.client2.FullJID + "' type='chat' id='c2'>" +
					"<replace xmlns='urn:xmpp:message-correct:0' id='orig55'/>" +
					"</message>");

				this.client2.ProcessMessage(new MessageEventArgs(this.client2, doc.DocumentElement));

				Assert.IsTrue(correctionReceived.WaitOne(1000), "Correction event was not raised.");
				Assert.IsFalse(chatMessageReceived.WaitOne(500), "Replace-only stanza should not trigger chat message handlers.");
			}
			finally
			{
				chatClient2.OnMessageCorrected -= correctionHandler;
				this.client2.OnChatMessage -= chatHandler;
				chatClient2.Dispose();
				await this.DisposeClients();
			}
		}
	}
}
