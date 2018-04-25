using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.P2P.SOCKS5;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSocks5Tests : CommunicationTests
	{
		[TestMethod]
		public void Socks5_Test_01_FindProxy()
		{
			this.ConnectClients();
			ManualResetEvent Done = new ManualResetEvent(false);
			Socks5Proxy Proxy = new Socks5Proxy(this.client1);
			
			Proxy.StartSearch((sender, e) =>
			{
				Done.Set();
			});

			Assert.IsTrue(Done.WaitOne(30000), "Search not complete.");
			Assert.IsTrue(Proxy.HasProxy, "No SOCKS5 proxy found.");

			Console.Out.WriteLine("JID: " + Proxy.JID);
			Console.Out.WriteLine("Port: " + Proxy.Port.ToString());
			Console.Out.WriteLine("Host: " + Proxy.Host);
		}

		[TestMethod]
		public void Socks5_Test_02_ConnectSOCKS5()
		{
			this.ConnectClients();
			ManualResetEvent Error = new ManualResetEvent(false);
			ManualResetEvent Done = new ManualResetEvent(false);
			Socks5Client Client = new Socks5Client("waher.se", 1080, "socks5.waher.se",
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));

			Client.OnStateChange += (sender, e) =>
			{
				switch (Client.State)
				{
					case Socks5State.Authenticated:
						Done.Set();
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						Error.Set();
						break;
				}
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Unable to connect.");
		}

		[TestMethod]
		public void Socks5_Test_03_ConnectStream()
		{
			this.ConnectClients();
			ManualResetEvent Error = new ManualResetEvent(false);
			ManualResetEvent Done = new ManualResetEvent(false);
			Socks5Client Client = new Socks5Client("waher.se", 1080, "socks5.waher.se",
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));

			Client.OnStateChange += (sender, e) =>
			{
				switch (Client.State)
				{
					case Socks5State.Authenticated:
						Client.CONNECT("Stream0001", this.client1.FullJID, this.client2.FullJID);
						break;

					case Socks5State.Connected:
						Done.Set();
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						Error.Set();
						break;
				}
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Unable to connect.");
		}

		[TestMethod]
		public void Socks5_Test_04_ActivateStream()
		{
			this.ConnectClients();
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done1 = new ManualResetEvent(false);
			Socks5Client Client1 = new Socks5Client("waher.se", 1080, "socks5.waher.se",
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));

			Client1.OnStateChange += (sender, e) =>
			{
				switch (Client1.State)
				{
					case Socks5State.Authenticated:
						Client1.CONNECT("Stream0001", this.client1.FullJID, this.client2.FullJID);
						break;

					case Socks5State.Connected:
						Done1.Set();
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						Error1.Set();
						break;
				}
			};

			ManualResetEvent Error2 = new ManualResetEvent(false);
			ManualResetEvent Done2 = new ManualResetEvent(false);
			Socks5Client Client2 = new Socks5Client("waher.se", 1080, "socks5.waher.se",
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));

			Client2.OnStateChange += (sender, e) =>
			{
				switch (Client2.State)
				{
					case Socks5State.Authenticated:
						Client2.CONNECT("Stream0001", this.client1.FullJID, this.client2.FullJID);
						break;

					case Socks5State.Connected:
						Done2.Set();
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						Error2.Set();
						break;
				}
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 10000), "Unable to connect.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000), "Unable to connect.");

			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			Done1.Reset();
			Error1.Reset();

			Done2.Reset();
			Error2.Reset();

			Client1.OnDataReceived += (sender, e) =>
			{
				if (Encoding.UTF8.GetString(e.Data) == "Hello2")
					Done1.Set();
				else
					Error1.Set();
			};

			Client2.OnDataReceived += (sender, e) =>
			{
				if (Encoding.UTF8.GetString(e.Data) == "Hello1")
					Done2.Set();
				else
					Error2.Set();
			};

			this.client1.SendIqSet("socks5.waher.se", "<query xmlns='http://jabber.org/protocol/bytestreams' sid='Stream0001'>" +
				"<activate>" + this.client2.FullJID + "</activate></query>", (sender, e) =>
			{
				if (e.Ok)
				{
					Client1.Send(Encoding.UTF8.GetBytes("Hello1"));
					Client2.Send(Encoding.UTF8.GetBytes("Hello2"));

					Done.Set();
				}
				else
					Error.Set();
			}, null);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Unable to activate stream.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 10000), "Did not receive message.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000), "Did not receive message.");
		}

		[TestMethod]
		public void Socks5_Test_05_InitiateSession()
		{
			this.ConnectClients();
			ManualResetEvent Done1 = new ManualResetEvent(false);
			Socks5Proxy Proxy1 = new Socks5Proxy(this.client1);

			Proxy1.StartSearch((sender, e) =>
			{
				Done1.Set();
			});

			ManualResetEvent Done2 = new ManualResetEvent(false);
			Socks5Proxy Proxy2 = new Socks5Proxy(this.client2);

			Proxy2.StartSearch((sender, e) =>
			{
				Done2.Set();
			});

			Assert.IsTrue(Done1.WaitOne(30000), "Search not complete.");
			Assert.IsTrue(Proxy1.HasProxy, "No SOCKS5 proxy found.");

			Assert.IsTrue(Done2.WaitOne(30000), "Search not complete.");
			Assert.IsTrue(Proxy2.HasProxy, "No SOCKS5 proxy found.");

			Done1.Reset();
			Done2.Reset();

			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Error2 = new ManualResetEvent(false);
			ManualResetEvent Closed1 = new ManualResetEvent(false);
			ManualResetEvent Closed2 = new ManualResetEvent(false);

			Proxy2.OnOpen += (sender, e) =>
			{
				e.AcceptStream((sender2, e2) =>
				{
					if (Encoding.ASCII.GetString(e2.Data) == "Hello1")
					{
						Done2.Set();
						e2.Stream.Send(Encoding.ASCII.GetBytes("Hello2"));
						e2.Stream.CloseWhenDone();
					}
					else
						Error2.Set();
				}, (sender2, e2) =>
				{
					Closed2.Set();
				}, null);
			};

			Proxy1.InitiateSession(this.client2.FullJID, (sender, e) =>
			{
				if (e.Ok)
				{
					e.Stream.OnDataReceived += (sender2, e2) =>
					{
						if (Encoding.ASCII.GetString(e2.Data) == "Hello2")
							Done1.Set();
						else
							Error1.Set();
					};

					e.Stream.OnStateChange += (sender2, e2) =>
					{
						if (e.Stream.State == Socks5State.Offline)
							Closed1.Set();
					};

					e.Stream.Send(Encoding.ASCII.GetBytes("Hello1"));
				}
			}, null);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done1, Error1 }, 10000), "Did not receive message 1.");
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000), "Did not receive message 2.");
			Assert.IsTrue(Closed1.WaitOne(10000), "Client 1 did not close properly.");
			Assert.IsTrue(Closed2.WaitOne(10000), "Client 2 did not close properly.");
		}

	}
}
