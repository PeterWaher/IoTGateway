using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public abstract class CommunicationTests
	{
		private ConsoleEventSink sink = null;
		protected ManualResetEvent connected1 = new ManualResetEvent(false);
		protected ManualResetEvent error1 = new ManualResetEvent(false);
		protected ManualResetEvent offline1 = new ManualResetEvent(false);
		protected ManualResetEvent connected2 = new ManualResetEvent(false);
		protected ManualResetEvent error2 = new ManualResetEvent(false);
		protected ManualResetEvent offline2 = new ManualResetEvent(false);
		protected XmppClient client1;
		protected XmppClient client2;
		protected Exception ex1 = null;
		protected Exception ex2 = null;

		public CommunicationTests()
		{
		}

		[ClassInitialize]
		public virtual void TestFixtureSetUp()
		{
			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);
		}

		[ClassCleanup]
		public virtual void TestFixtureTearDown()
		{
			if (this.sink != null)
			{
				Log.Unregister(this.sink);
				this.sink.Dispose();
				this.sink = null;
			}
		}

		public virtual void ConnectClients()
		{
			this.connected1.Reset();
			this.error1.Reset();
			this.offline1.Reset();

			this.connected2.Reset();
			this.error2.Reset();
			this.offline2.Reset();

			this.ex1 = null;
			this.ex2 = null;

			this.client1 = new XmppClient(this.GetCredentials1(), "en", typeof(CommunicationTests).Assembly)
			{
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};

            this.PrepareClient1(this.client1);

            this.client1.Add(new TextWriterSniffer(Console.Out, BinaryPresentationMethod.ByteCount));
            this.client1.OnConnectionError += Client_OnConnectionError1;
			this.client1.OnError += Client_OnError1;
			this.client1.OnStateChanged += Client_OnStateChanged1;
			this.client1.SetPresence(Availability.Chat, new KeyValuePair<string, string>("en", "Live and well"));
			this.client1.Connect();

			this.WaitConnected1(5000);

			this.client2 = new XmppClient(this.GetCredentials2(), "en", typeof(CommunicationTests).Assembly)
			{
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};

            this.PrepareClient2(this.client2);

            //this.client2.Add(new TextWriterSniffer(Console.Out, BinaryPresentationMethod.ByteCount));
            this.client2.OnConnectionError += Client_OnConnectionError2;
			this.client2.OnError += Client_OnError2;
			this.client2.OnStateChanged += Client_OnStateChanged2;
			this.client2.SetPresence(Availability.Chat, new KeyValuePair<string, string>("en", "Ready to chat."));
			this.client2.Connect();

			this.WaitConnected2(5000);

            //this.client2.Add(new TextWriterSniffer(Console.Out, BinaryPresentationMethod.ByteCount));
        }

        public virtual void PrepareClient1(XmppClient Client)
        {
        }

        public virtual void PrepareClient2(XmppClient Client)
        {
        }

        public virtual XmppCredentials GetCredentials1()
		{
			return new XmppCredentials()
			{
				Host = "waher.se",
				Port = 5222,
				Account = "xmppclient.test01",
				Password = "testpassword"
			};
		}

		public virtual XmppCredentials GetCredentials2()
		{
			return new XmppCredentials()
			{
				Host = "waher.se",
				Port = 5222,
				Account = "xmppclient.test02",
				Password = "testpassword"
			};
		}

		private void Client_OnStateChanged1(object Sender, XmppState NewState)
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

				case XmppState.Connecting:
					break;
			}
		}

		private void Client_OnStateChanged2(object Sender, XmppState NewState)
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

				case XmppState.Connecting:
					break;
			}
		}

		void Client_OnError1(object Sender, Exception Exception)
		{
			this.ex1 = Exception;
		}

		void Client_OnError2(object Sender, Exception Exception)
		{
			this.ex2 = Exception;
		}

		void Client_OnConnectionError1(object Sender, Exception Exception)
		{
			this.ex1 = Exception;
		}

		void Client_OnConnectionError2(object Sender, Exception Exception)
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
			Thread.Sleep(100);  // Wait for presence to be processed by server.
		}

		private void WaitConnected2(int Timeout)
		{
			this.AssertWaitConnected(this.Wait2(Timeout));
			Thread.Sleep(100);  // Wait for presence to be processed by server.
		}

		private void AssertWaitConnected(int Event)
		{
			switch (Event)
			{
				case -1:
				case WaitHandle.WaitTimeout:
					Assert.Fail("Unable to connect. Timeout occurred.");
					break;

				case 0: // Connected
					break;

				case 1:
					Assert.Fail("Unable to connect. Error occurred.");
					break;

				case 2:
					Assert.Fail("Unable to connect. Client turned offline.");
					break;
			}
		}

		public virtual void DisposeClients()
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
