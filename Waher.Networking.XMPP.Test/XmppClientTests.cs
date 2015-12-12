using System;
using System.Reflection;
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
		private Exception ex = null;

		public XmppClientTests()
		{
		}

		[SetUp]
		public void Setup()
		{
			this.ex = null;
			this.client = new XmppClient("thingk.me", 5222, "xmppclient.test", "testpassword", "en");
			this.client.OnConnectionError += new XmppExceptionEventHandler(client_OnConnectionError);
			this.client.OnError += new XmppExceptionEventHandler(client_OnError);
			this.client.OnStateChanged += new StateChangedEventHandler(client_OnStateChanged);
		}

		private void client_OnStateChanged(XmppClient Sender, XmppState NewState)
		{
			Console.Out.WriteLine(NewState.ToString());
		}

		void client_OnError(XmppClient Sender, Exception Exception)
		{
			this.ex = Exception;
		}

		void client_OnConnectionError(XmppClient Sender, Exception Exception)
		{
			this.ex = Exception;
		}

		[TearDown]
		public void TearDown()
		{
			this.client.Dispose();

			if (this.ex != null)
				throw new TargetInvocationException(this.ex);
		}

		[Test]
		public void Test_01_Connect()
		{
			System.Threading.Thread.Sleep(10000);
		}
	}
}
