using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.PeerToPeer;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.P2P;
using Waher.Runtime.Console;
using Waher.Script.Operators.Comparisons;

namespace Waher.Networking.XMPP.Test
{
	public abstract class CommunicationTests
	{
		private static ConsoleEventSink sink = null;
		private static XmlFileSniffer xmlSniffer1 = null;
		private static XmlFileSniffer xmlSniffer2 = null;
		protected ManualResetEvent connected1 = new(false);
		protected ManualResetEvent error1 = new(false);
		protected ManualResetEvent offline1 = new(false);
		protected ManualResetEvent connected2 = new(false);
		protected ManualResetEvent error2 = new(false);
		protected ManualResetEvent offline2 = new(false);
		protected XmppClient client01;
		protected XmppClient client02;
		protected XmppClient client1;
		protected XmppClient client2;
		protected XmppServerlessMessaging serverless1;
		protected XmppServerlessMessaging serverless2;
		protected Exception ex1 = null;
		protected Exception ex2 = null;

		public CommunicationTests()
		{
		}

		public static void SetupSnifferAndLog()
		{
			sink = new ConsoleEventSink();
			Log.Register(sink);

			if (xmlSniffer1 is null)
			{
				File.Delete("XMPP1.xml");
				xmlSniffer1 = new XmlFileSniffer("XMPP1.xml",
						@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
						int.MaxValue, BinaryPresentationMethod.Base64);
			}

			if (xmlSniffer2 is null)
			{
				File.Delete("XMPP2.xml");
				xmlSniffer2 = new XmlFileSniffer("XMPP2.xml",
						@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
						int.MaxValue, BinaryPresentationMethod.Base64);
			}
		}

		public static async Task DisposeSnifferAndLog()
		{
			if (xmlSniffer1 is not null)
			{
				await xmlSniffer1.DisposeAsync();
				xmlSniffer1 = null;
			}

			if (xmlSniffer2 is not null)
			{
				await xmlSniffer2.DisposeAsync();
				xmlSniffer2 = null;
			}

			if (sink is not null)
			{
				Log.Unregister(sink);
				await sink.DisposeAsync();
				sink = null;
			}
		}

		public virtual async Task ConnectClients(int SecurityStrength, bool P2p)
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

			this.client1.SetTag("ShowE2E", true);
			this.client1.Add(new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine));
			this.client1.Add(xmlSniffer1);
			this.client1.OnConnectionError += this.Client_OnConnectionError1;
			this.client1.OnError += this.Client_OnError1;
			this.client1.OnStateChanged += this.Client_OnStateChanged1;
			this.client1.Information("Starting test, client 1...");

			this.PrepareClient1(this.client1, SecurityStrength, P2p);

			await this.client1.SetPresence(Availability.Chat, new KeyValuePair<string, string>("en", "Live and well"));
			await this.client1.Connect();

			this.WaitConnected1(5000);

			if (P2p)
			{
				this.serverless1 = new XmppServerlessMessaging("Client1", this.client1.FullJID,
					5001, 8001, this.client1.Sniffers);

				TaskCompletionSource<bool> Serverless1Ready = new();

				this.serverless1.Network.OnStateChange += (sender, NewState) =>
				{
					this.client1.Information("Serveless 1 state: " + NewState.ToString());

					switch (NewState)
					{
						case PeerToPeerNetworkState.Ready:
							Serverless1Ready.TrySetResult(true);
							break;

						case PeerToPeerNetworkState.Error:
						case PeerToPeerNetworkState.Closed:
							Serverless1Ready.TrySetResult(false);
							break;
					}

					return Task.CompletedTask;
				};

				_ = Task.Delay(10000).ContinueWith(_ => Serverless1Ready.TrySetResult(false));

				if (!await Serverless1Ready.Task)
					throw new Exception("Unable to establish a P2P network for client 1.");
			}
			else
				this.serverless1 = null;

			this.client2 = new XmppClient(this.GetCredentials2(), "en", typeof(CommunicationTests).Assembly)
			{
				DefaultNrRetries = 2,
				DefaultRetryTimeout = 1000,
				DefaultMaxRetryTimeout = 5000,
				DefaultDropOff = true
			};

			this.client2.SetTag("ShowE2E", true);
			this.client2.Add(xmlSniffer2);
			this.client2.OnConnectionError += this.Client_OnConnectionError2;
			this.client2.OnError += this.Client_OnError2;
			this.client2.OnStateChanged += this.Client_OnStateChanged2;
			this.client2.Information("Starting test, client 2...");

			this.PrepareClient2(this.client2, SecurityStrength, P2p);

			await this.client2.SetPresence(Availability.Chat, new KeyValuePair<string, string>("en", "Ready to chat."));
			await this.client2.Connect();

			this.WaitConnected2(5000);

			if (P2p)
			{
				this.serverless2 = new XmppServerlessMessaging("Client2", this.client2.FullJID,
					5002, 8002, this.client2.Sniffers);

				TaskCompletionSource<bool> Serverless2Ready = new();

				this.serverless2.Network.OnStateChange += (sender, NewState) =>
				{
					this.client2.Information("Serveless 2 state: " + NewState.ToString());

					switch (NewState)
					{
						case PeerToPeerNetworkState.Ready:
							Serverless2Ready.TrySetResult(true);
							break;

						case PeerToPeerNetworkState.Error:
						case PeerToPeerNetworkState.Closed:
							Serverless2Ready.TrySetResult(false);
							break;
					}

					return Task.CompletedTask;
				};

				_ = Task.Delay(10000).ContinueWith(_ => Serverless2Ready.TrySetResult(false));

				if (!await Serverless2Ready.Task)
					throw new Exception("Unable to establish a P2P network for client 2.");

				PeerConnectionEventArgs e = await this.serverless1.GetPeerConnectionAsync(this.client2.FullJID);
				if (e.Client is null)
					throw new Exception("Client 1 could not connect to client 2 in serverless mode.");

				this.client01 = this.client1;
				this.client1 = e.Client;

				e = await this.serverless2.GetPeerConnectionAsync(this.client1.FullJID);
				if (e.Client is null)
					throw new Exception("Client 2 could not connect to client 1 in serverless mode.");

				this.client02 = this.client2;
				this.client2 = e.Client;
			}
			else
				this.serverless2 = null;
		}

		public virtual void PrepareClient1(XmppClient Client, int SecurityStrength, bool P2p)
		{
		}

		public virtual void PrepareClient2(XmppClient Client, int SecurityStrength, bool P2p)
		{
		}

		public virtual XmppCredentials GetCredentials1()
		{
			return new XmppCredentials()
			{
				Host = "waher.se",
				TrustServer = false,
				//Host = "localhost",
				//TrustServer = true,
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
				TrustServer = false,
				//Host = "localhost",
				//TrustServer = true,
				Port = 5222,
				Account = "xmppclient.test02",
				Password = "testpassword"
			};
		}

		private Task Client_OnStateChanged1(object Sender, XmppState NewState)
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

			return Task.CompletedTask;
		}

		private Task Client_OnStateChanged2(object Sender, XmppState NewState)
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

			return Task.CompletedTask;
		}

		Task Client_OnError1(object Sender, Exception Exception)
		{
			this.ex1 = Exception;
			return Task.CompletedTask;
		}

		Task Client_OnError2(object Sender, Exception Exception)
		{
			this.ex2 = Exception;
			return Task.CompletedTask;
		}

		Task Client_OnConnectionError1(object Sender, Exception Exception)
		{
			this.ex1 = Exception;
			return Task.CompletedTask;
		}

		Task Client_OnConnectionError2(object Sender, Exception Exception)
		{
			this.ex2 = Exception;
			return Task.CompletedTask;
		}

		private int Wait1(int Timeout)
		{
			return WaitHandle.WaitAny([this.connected1, this.error1, this.offline1], Timeout);
		}

		private int Wait2(int Timeout)
		{
			return WaitHandle.WaitAny([this.connected2, this.error2, this.offline2], Timeout);
		}

		private void WaitConnected1(int Timeout)
		{
			AssertWaitConnected(this.Wait1(Timeout));
			Thread.Sleep(100);  // Wait for presence to be processed by server.
		}

		private void WaitConnected2(int Timeout)
		{
			AssertWaitConnected(this.Wait2(Timeout));
			Thread.Sleep(100);  // Wait for presence to be processed by server.
		}

		private static void AssertWaitConnected(int Event)
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

		public virtual async Task DisposeClients()
		{
			await ConsoleOut.FlushAsync();

			if (this.client01 is not null)
			{
				this.client01.Information("Stopping test, client 01...");
				await this.client01.OfflineAndDisposeAsync(false);
				this.client01 = null;
			}

			if (this.serverless1 is not null)
			{
				await this.serverless1.DisposeAsync();
				this.serverless1 = null;
			}

			if (this.client1 is not null)
			{
				this.client1.Information("Stopping test, client 1...");
				await this.client1.OfflineAndDisposeAsync(false);
				this.client1 = null;
			}

			if (this.client02 is not null)
			{
				this.client02.Information("Stopping test, client 02...");
				await this.client02.OfflineAndDisposeAsync(false);
				this.client02 = null;
			}

			if (this.serverless2 is not null)
			{
				await this.serverless2.DisposeAsync();
				this.serverless2 = null;
			}

			if (this.client2 is not null)
			{
				this.client2.Information("Stopping test, client 2...");
				await this.client2.OfflineAndDisposeAsync(false);
				this.client2 = null;
			}

			if (this.ex1 is not null)
				throw new TargetInvocationException(this.ex1);

			if (this.ex2 is not null)
				throw new TargetInvocationException(this.ex2);
		}
	}
}
