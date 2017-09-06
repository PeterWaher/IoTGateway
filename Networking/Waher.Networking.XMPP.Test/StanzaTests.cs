using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class StanzaTests : CommunicationTests
	{
		[TestMethod]
		public void Test_01_ChatMessage()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			this.client2.OnChatMessage += (sender, e) => Done.Set();

			this.client1.SendChatMessage(this.client2.FullJID, "Hello");

			Assert.IsTrue(Done.WaitOne(10000), "Message not delivered properly.");
		}

		[TestMethod]
		public void Test_02_ChatMessageWithSubject()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			this.client2.OnChatMessage += (sender, e) => Done.Set();

			this.client1.SendChatMessage(this.client2.FullJID, "Hello", "Greeting");

			Assert.IsTrue(Done.WaitOne(10000), "Message not delivered properly.");
		}

		[TestMethod]
		public void Test_03_ChatMessageWithSubjectAndLanguage()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			this.client2.OnChatMessage += (sender, e) => Done.Set();

			this.client1.SendChatMessage(this.client2.FullJID, "Hello", "Greeting", "en");

			Assert.IsTrue(Done.WaitOne(10000), "Message not delivered properly.");
		}

		[TestMethod]
		public void Test_04_Presence()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			this.client1.OnPresence += (sender, e) =>
			{
				if (e.From == this.client2.FullJID && e.Availability == Availability.Chat)
					Done.Set();
			};

			this.client2.SetPresence(Availability.Chat, "<hola xmlns='bandola'>abc</hola>");

			Assert.IsTrue(Done.WaitOne(10000), "Presence not delivered properly.");
		}

		[TestMethod]
		public void Test_05_IQ_Get()
		{
			this.client1.RegisterIqGetHandler("query", "test", (sender, e) =>
			{
				e.IqResult("<response xmlns='test'/>");
			}, true);

			this.client2.IqGet(this.client1.FullJID, "<query xmlns='test'/>", 10000);
		}

		[TestMethod]
		public void Test_06_IQ_Set()
		{
			this.client1.RegisterIqSetHandler("query", "test", (sender, e) =>
			{
				e.IqResult("<response xmlns='test'/>");
			}, true);

			this.client2.IqSet(this.client1.FullJID, "<query xmlns='test'/>", 10000);
		}
	}
}
