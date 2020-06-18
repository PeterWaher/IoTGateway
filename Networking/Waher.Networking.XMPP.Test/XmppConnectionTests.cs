using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppConnectionTests
	{
		protected static ConsoleEventSink sink = null;
		protected AutoResetEvent connected = new AutoResetEvent(false);
		protected AutoResetEvent error = new AutoResetEvent(false);
		protected AutoResetEvent offline = new AutoResetEvent(false);
		protected XmppClient client;
		protected Exception ex = null;

		public XmppConnectionTests()
		{
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			sink = new ConsoleEventSink();
			Log.Register(sink);
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (sink != null)
			{
				Log.Unregister(sink);
				sink.Dispose();
				sink = null;
			}
		}

		[TestInitialize]
		public void Setup()
		{
			this.connected.Reset();
			this.error.Reset();
			this.offline.Reset();

			this.ex = null;

			this.client = new XmppClient(this.GetCredentials(), "en", typeof(CommunicationTests).Assembly)
			{
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};

			this.client.Add(new TextWriterSniffer(Console.Out, BinaryPresentationMethod.ByteCount));

			this.client.OnConnectionError += this.Client_OnConnectionError;
			this.client.OnError += this.Client_OnError;
			this.client.OnStateChanged += this.Client_OnStateChanged;
			this.client.Connect();
		}

		public virtual XmppCredentials GetCredentials()
		{
			return new XmppCredentials()
			{
				Host = "kode.im",
				Port = 5222,
				Account = "xmppclient.test01",
				Password = "testpassword"
			};
		}

		private Task Client_OnStateChanged(object Sender, XmppState NewState)
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

			return Task.CompletedTask;
		}

		Task Client_OnError(object Sender, Exception Exception)
		{
			this.ex = Exception;
			return Task.CompletedTask;
		}

		Task Client_OnConnectionError(object Sender, Exception Exception)
		{
			this.ex = Exception;
			return Task.CompletedTask;
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
		public void Connection_Test_01_Connect_AutoCreate()
		{
			this.client.AllowRegistration();
			this.WaitConnected(10000);
		}

		[TestMethod]
		public void Connection_Test_02_Connect()
		{
			this.WaitConnected(10000);
		}

		[TestMethod]
		public void Connection_Test_03_Disconnect()
		{
			this.WaitConnected(10000);

			this.client.Dispose();
			this.WaitOffline(10000);
			this.ex = null;
		}

		[TestMethod]
		public void Connection_Test_04_NotAuthorized()
		{
			this.Connection_Test_03_Disconnect();

			this.client = new XmppClient("kode.im", 5222, "xmppclient.test01", "abc", "en", typeof(CommunicationTests).Assembly);
			this.client.OnConnectionError += this.Client_OnConnectionError;
			this.client.OnError += this.Client_OnError;
			this.client.OnStateChanged += this.Client_OnStateChanged;
			this.client.Connect();

			this.WaitError(10000);
		}

		[TestMethod]
		public void Connection_Test_05_Reconnect()
		{
			this.WaitConnected(10000);

			this.client.HardOffline();
			this.WaitOffline(10000);

			this.client.Reconnect();
			this.WaitConnected(10000);
		}

		[TestMethod]
		[Ignore("Feature not supported on server.")]
		public void Connection_Test_06_ChangePassword()
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
