using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	public abstract class CommunicationTests
	{
		private ConsoleEventSink sink = null;
		protected AutoResetEvent connected1 = new AutoResetEvent(false);
		protected AutoResetEvent error1 = new AutoResetEvent(false);
		protected AutoResetEvent offline1 = new AutoResetEvent(false);
		protected AutoResetEvent connected2 = new AutoResetEvent(false);
		protected AutoResetEvent error2 = new AutoResetEvent(false);
		protected AutoResetEvent offline2 = new AutoResetEvent(false);
		protected XmppClient client1;
		protected XmppClient client2;
		protected Exception ex1 = null;
		protected Exception ex2 = null;

		public CommunicationTests()
		{
		}

		[TestFixtureSetUp]
		public virtual void TestFixtureSetUp()
		{
			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);
		}

		[TestFixtureTearDown]
		public virtual void TestFixtureTearDown()
		{
			if (this.sink != null)
			{
				Log.Unregister(this.sink);
				this.sink.Dispose();
				this.sink = null;
			}
		}

		[SetUp]
		public virtual void Setup()
		{
			this.connected1.Reset();
			this.error1.Reset();
			this.offline1.Reset();

			this.connected2.Reset();
			this.error2.Reset();
			this.offline2.Reset();
			
			this.ex1 = null;
			this.ex2 = null;

			//this.client1 = new XmppClient("tigase.im", 5222, "xmppclient.test01", "testpassword", "en");
			//this.client1.AllowPlain = true;
			//this.client1.TrustServer = true;
			//this.client1 = new XmppClient("jabber.se", 5222, "xmppclient.test01", "testpassword", "en");
			//this.client1 = new XmppClient("kode.im", 5222, "xmppclient.test01", "testpassword", "en");
			this.client1 = new XmppClient("draugr.de", 5222, "xmppclient.test01", "testpassword", "en");
			this.client1.AllowRegistration();
			this.client1.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));
			this.client1.DefaultNrRetries = 2;
			this.client1.DefaultRetryTimeout = 1000;
			this.client1.DefaultMaxRetryTimeout = 5000;
			this.client1.DefaultDropOff = true;
			this.client1.OnConnectionError += new XmppExceptionEventHandler(client_OnConnectionError1);
			this.client1.OnError += new XmppExceptionEventHandler(client_OnError1);
			this.client1.OnStateChanged += new StateChangedEventHandler(client_OnStateChanged1);
			this.client1.SetPresence(Availability.Chat, string.Empty, new KeyValuePair<string, string>("en", "Live and well"));
			this.client1.Connect();

			//this.client2 = new XmppClient("tigase.im", 5222, "xmppclient.test02", "testpassword", "en");
			//this.client2.AllowPlain = true;
			//this.client2.TrustServer = true;
			//this.client2 = new XmppClient("jabber.se", 5222, "xmppclient.test02", "testpassword", "en");
			this.client2 = new XmppClient("kode.im", 5222, "xmppclient.test02", "testpassword", "en");
			this.client2.AllowRegistration();
			//this.client2.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));
			this.client2.DefaultNrRetries = 2;
			this.client2.DefaultRetryTimeout = 1000;
			this.client2.DefaultMaxRetryTimeout = 5000;
			this.client2.DefaultDropOff = true;
			this.client2.OnConnectionError += new XmppExceptionEventHandler(client_OnConnectionError2);
			this.client2.OnError += new XmppExceptionEventHandler(client_OnError2);
			this.client2.OnStateChanged += new StateChangedEventHandler(client_OnStateChanged2);
			this.client2.SetPresence(Availability.Chat, string.Empty, new KeyValuePair<string, string>("en", "Ready to chat."));
			this.client2.Connect();

			this.WaitConnected(10000);
		}

		private void client_OnStateChanged1(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.connected1.Set();
					break;

				case XmppState.Error:
					this.error1.Set();
					break;

				case XmppState.Offline:
					this.offline1.Set();
					break;
			}
		}

		private void client_OnStateChanged2(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.connected2.Set();
					break;

				case XmppState.Error:
					this.error2.Set();
					break;

				case XmppState.Offline:
					this.offline2.Set();
					break;
			}
		}

		void client_OnError1(object Sender, Exception Exception)
		{
			this.ex1 = Exception;
		}

		void client_OnError2(object Sender, Exception Exception)
		{
			this.ex2 = Exception;
		}

		void client_OnConnectionError1(object Sender, Exception Exception)
		{
			this.ex1 = Exception;
		}

		void client_OnConnectionError2(object Sender, Exception Exception)
		{
			this.ex2 = Exception;
		}

		private int Wait1(int Timeout)
		{
			return WaitHandle.WaitAny(new WaitHandle[] { this.connected1, this.error1, this.offline1 }, Timeout);
		}

		private int Wait2(int Timeout)
		{
			return WaitHandle.WaitAny(new WaitHandle[] { this.connected2, this.error2, this.offline2 }, Timeout);
		}

		private void WaitConnected1(int Timeout)
		{
			this.AssertWaitConnected(this.Wait1(Timeout));
		}

		private void WaitConnected2(int Timeout)
		{
			this.AssertWaitConnected(this.Wait2(Timeout));
		}

		private void WaitConnected(int Timeout)
		{
			this.WaitConnected1(Timeout);
			this.WaitConnected2(Timeout);
		}

		private void AssertWaitConnected(int Event)
		{
			switch (Event)
			{
				case -1:
					Assert.Fail("Unable to connect. Timeout occurred.");
					break;

				case 1:
					Assert.Fail("Unable to connect. Error occurred.");
					break;

				case 2:
					Assert.Fail("Unable to connect. Client turned offline.");
					break;
			}
		}

		[TearDown]
		public virtual void TearDown()
		{
			if (this.client1 != null)
				this.client1.Dispose();

			if (this.client2 != null)
				this.client2.Dispose();

			if (this.ex1 != null)
				throw new TargetInvocationException(this.ex1);

			if (this.ex2 != null)
				throw new TargetInvocationException(this.ex2);
		}
	}
}
