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
	public class XmppComponentTests
	{
		private ConsoleEventSink sink = null;
		protected AutoResetEvent clientConnected = new AutoResetEvent(false);
		protected AutoResetEvent clientError = new AutoResetEvent(false);
		protected AutoResetEvent clientOffline = new AutoResetEvent(false);
		protected AutoResetEvent componentConnected = new AutoResetEvent(false);
		protected AutoResetEvent componentError = new AutoResetEvent(false);
		protected AutoResetEvent componentOffline = new AutoResetEvent(false);
		protected XmppClient client;
		protected XmppComponent component;
		protected Exception clientEx = null;
		protected Exception componentEx = null;

		public XmppComponentTests()
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

		[TestInitialize]
		public virtual void Setup()
		{
			this.clientConnected.Reset();
			this.clientError.Reset();
			this.clientOffline.Reset();

			this.componentConnected.Reset();
			this.componentError.Reset();
			this.componentOffline.Reset();
			
			this.clientEx = null;
			this.componentEx = null;

			this.client = new XmppClient("localhost", 5222, "testuser", "testpass", "en")
			{
				TrustServer = true,
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};
			//this.client.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));

			this.client.OnConnectionError += new XmppExceptionEventHandler(Client_OnConnectionError);
			this.client.OnError += new XmppExceptionEventHandler(Client_OnError);
			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);

			this.client.SetPresence(Availability.Chat, string.Empty, new KeyValuePair<string, string>("en", "Live and well"));
			this.client.Connect();

			this.component = new XmppComponent("localhost", 5275, "provisioning.peterwaher-hp14", "provisioning", "collaboration", "provisioning", "Provisioning service")
			{
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};

			this.component.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));

			this.component.OnConnectionError += new XmppExceptionEventHandler(Component_OnConnectionError);
			this.component.OnError += new XmppExceptionEventHandler(Component_OnError);
			this.component.OnStateChanged += new StateChangedEventHandler(Component_OnStateChanged);

			this.WaitConnected(10000);
		}

		private void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.clientEx = null;
					this.clientConnected.Set();
					break;

				case XmppState.Error:
					this.clientError.Set();
					break;

				case XmppState.Offline:
					this.clientOffline.Set();
					break;
			}
		}

		private void Component_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.componentConnected.Set();
					break;

				case XmppState.Error:
					this.componentError.Set();
					break;

				case XmppState.Offline:
					this.componentOffline.Set();
					break;
			}
		}

		void Client_OnError(object Sender, Exception Exception)
		{
			this.clientEx = Exception;
		}

		void Component_OnError(object Sender, Exception Exception)
		{
			this.componentEx = Exception;
		}

		void Client_OnConnectionError(object Sender, Exception Exception)
		{
			this.clientEx = Exception;
		}

		void Component_OnConnectionError(object Sender, Exception Exception)
		{
			this.componentEx = Exception;
		}

		private int WaitClient(int Timeout)
		{
			return WaitHandle.WaitAny(new WaitHandle[] { this.clientConnected, this.clientError, this.clientOffline }, Timeout);
		}

		private int WaitComponent(int Timeout)
		{
			return WaitHandle.WaitAny(new WaitHandle[] { this.componentConnected, this.componentError, this.componentOffline }, Timeout);
		}

		private void WaitClientConnected(int Timeout)
		{
			this.AssertWaitConnected(this.WaitClient(Timeout));
		}

		private void WaitComponentConnected(int Timeout)
		{
			this.AssertWaitConnected(this.WaitComponent(Timeout));
		}

		private void WaitConnected(int Timeout)
		{
			this.WaitClientConnected(Timeout);
			this.WaitComponentConnected(Timeout);
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

		[TestCleanup]
		public virtual void TearDown()
		{
			if (this.client != null)
				this.client.Dispose();

			if (this.component != null)
				this.component.Dispose();

			if (this.clientEx != null)
				throw new TargetInvocationException(this.clientEx);

			if (this.componentEx != null)
				throw new TargetInvocationException(this.componentEx);
		}

		[TestMethod]
		public void Test_01_Connect()
		{
		}
	}
}
