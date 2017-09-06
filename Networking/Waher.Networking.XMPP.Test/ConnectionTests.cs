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
	public class ConnectionTests
	{
		private ConsoleEventSink sink = null;
		private AutoResetEvent connected = new AutoResetEvent(false);
		private AutoResetEvent error = new AutoResetEvent(false);
		private AutoResetEvent offline = new AutoResetEvent(false);
		private XmppClient client;
		private Exception ex = null;

		public ConnectionTests()
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
		public void Setup()
		{
			this.connected.Reset();
			this.error.Reset();
			this.offline.Reset();

			this.ex = null;

			//this.client = new XmppClient("tigase.im", 5222, "xmppclient.test01", "testpassword", "en");
			//this.client.AllowPlain = true;
			//this.client.TrustServer = true;
			//this.client = new XmppClient("jappix.com", 5222, "xmppclient.test01", "testpassword", "en");
			//this.client = new XmppClient("jabber.de", 5222, "xmppclient.test01", "testpassword", "en");
			//this.client = new XmppClient("jabber.se", 5222, "xmppclient.test01", "testpassword", "en");
			//this.client = new XmppClient("ik.nu", 5222, "xmppclient.test01", "testpassword", "en");
			this.client = new XmppClient("kode.im", 5222, "xmppclient.test01", "testpassword", "en")
			{
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};

			this.client.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount));

			this.client.OnConnectionError += new XmppExceptionEventHandler(Client_OnConnectionError);
			this.client.OnError += new XmppExceptionEventHandler(Client_OnError);
			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);
			this.client.Connect();
		}

		private void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					this.connected.Set();
					break;

				case XmppState.Error:
					this.error.Set();
					break;

				case XmppState.Offline:
					this.offline.Set();
					break;
			}
		}

		void Client_OnError(object Sender, Exception Exception)
		{
			this.ex = Exception;
		}

		void Client_OnConnectionError(object Sender, Exception Exception)
		{
			this.ex = Exception;
		}

		private int Wait(int Timeout)
		{
			return WaitHandle.WaitAny(new WaitHandle[] { this.connected, this.error, this.offline }, Timeout);
		}

		private void WaitConnected(int Timeout)
		{
			switch (this.Wait(Timeout))
			{
				default:
					Assert.Fail("Unable to connect. Timeout occurred.");
					break;

				case 0:
					break;

				case 1:
					Assert.Fail("Unable to connect. Error occurred.");
					break;

				case 2:
					Assert.Fail("Unable to connect. Client turned offline.");
					break;
			}
		}

		private void WaitError(int Timeout)
		{
			switch (this.Wait(Timeout))
			{
				default:
					Assert.Fail("Expected error. Timeout occurred.");
					break;

				case 0:
					Assert.Fail("Expected error. Connection successful.");
					break;

				case 1:
					break;

				case 2:
					Assert.Fail("Expected error. Client turned offline.");
					break;
			}

			this.ex = null;
		}

		private void WaitOffline(int Timeout)
		{
			switch (this.Wait(Timeout))
			{
				default:
					Assert.Fail("Expected offline. Timeout occurred.");
					break;

				case 0:
					Assert.Fail("Expected offline. Connection successful.");
					break;

				case 1:
					Assert.Fail("Expected offline. Error occured.");
					break;

				case 2:
					break;
			}
		}

		[TestCleanup]
		public void TearDown()
		{
			if (this.client != null)
				this.client.Dispose();

			if (this.ex != null)
				throw new TargetInvocationException(this.ex);
		}

		[TestMethod]
		public void Test_01_Connect_AutoCreate()
		{
			this.client.AllowRegistration();
			this.WaitConnected(10000);
		}

		[TestMethod]
		public void Test_02_Connect()
		{
			this.WaitConnected(10000);
		}

		[TestMethod]
		public void Test_03_Disconnect()
		{
			this.WaitConnected(10000);

			this.client.Dispose();
			this.WaitOffline(10000);
			this.ex = null;
		}

		[TestMethod]
		public void Test_04_NotAuthorized()
		{
			this.Test_03_Disconnect();

			this.client = new XmppClient("kode.im", 5222, "xmppclient.test01", "abc", "en");
			this.client.OnConnectionError += new XmppExceptionEventHandler(Client_OnConnectionError);
			this.client.OnError += new XmppExceptionEventHandler(Client_OnError);
			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);
			this.client.Connect();

			this.WaitError(10000);
		}

		[TestMethod]
		public void Test_05_Reconnect()
		{
			this.WaitConnected(10000);

			this.client.HardOffline();
			this.WaitOffline(10000);

			this.client.Reconnect();
			this.WaitConnected(10000);
		}

		[TestMethod]
		[Ignore("Feature not supported on server.")]
		public void Test_06_ChangePassword()
		{
			AutoResetEvent Changed = new AutoResetEvent(false);

			this.WaitConnected(10000);

			this.client.OnPasswordChanged += (sender, e) => Changed.Set();

			this.client.ChangePassword("newtestpassword");
			Assert.IsTrue(Changed.WaitOne(10000), "Unable to change password.");

			this.client.ChangePassword("testpassword");
			Assert.IsTrue(Changed.WaitOne(10000), "Unable to change password back.");
		}
	}
}
