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
	public class XmppQoSMessagingTests : CommunicationTests
	{
		[TestMethod]
		public void QoS_Test_01_Unacknowledged_Service()
		{
			this.QoSTest(QoSLevel.Unacknowledged);
		}

		[TestMethod]
		public void QoS_Test_02_Acknowledged_Service()
		{
			this.QoSTest(QoSLevel.Acknowledged);
		}

		[TestMethod]
		public void QoS_Test_03_Assured_Service()
		{
			this.QoSTest(QoSLevel.Assured);
		}

		private void QoSTest(QoSLevel Level)
		{
			this.ConnectClients();

			ManualResetEvent Received = new ManualResetEvent(false);
			ManualResetEvent Delivered = new ManualResetEvent(false);

			this.client2.OnNormalMessage += (sender, e) => Received.Set();

			this.client1.SendMessage(Level, MessageType.Normal, this.client2.FullJID, string.Empty, "Hello", string.Empty, "en",
				string.Empty, string.Empty, (sender, e) => Delivered.Set(), null);

			Assert.IsTrue(Delivered.WaitOne(10000), "Message not delivered properly.");
			Assert.IsTrue(Received.WaitOne(10000), "Message not received properly.");
		}

		[TestMethod]
		public void QoS_Test_04_Timeout()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			IqResultEventArgs e2 = null;

			this.ConnectClients();

			this.client2.RegisterIqGetHandler("test", "test", (sender, e) =>
			{
				// Do nothing. Do not return result or error.
			}, false);

			this.client1.SendIqGet(this.client2.FullJID, "<test:test xmlns:test='test'/>", (sender, e) =>
			{
				e2 = e;
				Done.Set();

			}, null, 1000, 3, true, int.MaxValue);

			Assert.IsTrue(Done.WaitOne(20000), "Retry function not working properly.");
			Assert.IsFalse(e2.Ok, "Request not properly cancelled.");
		}
	}
}
