using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class XmppClientTests
	{
		private XmppClient client;

		public XmppClientTests()
		{
		}

		[SetUp]
		public void Setup()
		{
			this.client = new XmppClient("thingk.me", 5222, "xmppclient.test", "testpassword", "en");
		}

		[TearDown]
		public void TearDown()
		{
			this.client.Dispose();
		}

		[Test]
		public void Test_01_Connect()
		{
			System.Threading.Thread.Sleep(1000);
		}
	}
}
