using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.P2P.SOCKS5;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class Socks5Tests : CommunicationTests
	{
		private string jid;
		private int port;
		private string host;

		[Test]
		public void Test_01_FindProxy()
		{
			ServiceItemsDiscoveryEventArgs e = this.client1.ServiceItemsDiscovery(this.client1.Domain, 10000);
			string Component = string.Empty;

			foreach (Item Item in e.Items)
			{
				ServiceDiscoveryEventArgs e2 = this.client1.ServiceDiscovery(Item.JID, 10000);

				if (e2.Features.ContainsKey("http://jabber.org/protocol/bytestreams"))
				{
					Component = Item.JID;
					break;
				}
			}

			Assert.IsNotEmpty(Component, "No SOCKS5 proxy found.");

			XmlElement E = this.client1.IqGet(Component, "<query xmlns='http://jabber.org/protocol/bytestreams'/>", 10000);

			Assert.AreEqual("iq", E.LocalName);

			E = (XmlElement)E.FirstChild;

			Assert.AreEqual("query", E.LocalName);
			Assert.AreEqual("http://jabber.org/protocol/bytestreams", E.NamespaceURI);

			E = (XmlElement)E.FirstChild;

			Assert.AreEqual("streamhost", E.LocalName);
			Assert.AreEqual("http://jabber.org/protocol/bytestreams", E.NamespaceURI);

			this.jid = XML.Attribute(E, "jid");
			this.port = XML.Attribute(E, "port", 0);
			this.host = XML.Attribute(E, "host");

			Console.Out.WriteLine("JID: " + this.jid);
			Console.Out.WriteLine("Port: " + this.port.ToString());
			Console.Out.WriteLine("Host: " + this.host);
		}

		[Test]
		public void Test_02_ConnectSOCKS5()
		{
			ManualResetEvent Error = new ManualResetEvent(false);
			ManualResetEvent Done = new ManualResetEvent(false);
			//Socks5Client Client = new Socks5Client("proxy.kode.im", 5000, "proxy.kode.im", 
			Socks5Client Client = new Socks5Client("89.163.130.28", 7777, "proxy.draugr.de",
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));

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

		[Test]
		public void Test_03_ConnectStream()
		{
			ManualResetEvent Error = new ManualResetEvent(false);
			ManualResetEvent Done = new ManualResetEvent(false);
			//Socks5Client Client = new Socks5Client("proxy.kode.im", 5000, "proxy.kode.im", 
			Socks5Client Client = new Socks5Client("89.163.130.28", 7777, "proxy.draugr.de",
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));

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

		[Test]
		public void Test_04_ActivateStream()
		{
			ManualResetEvent Error1 = new ManualResetEvent(false);
			ManualResetEvent Done1 = new ManualResetEvent(false);
			Socks5Client Client1 = new Socks5Client("89.163.130.28", 7777, "proxy.draugr.de",
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));

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
			Socks5Client Client2 = new Socks5Client("89.163.130.28", 7777, "proxy.draugr.de",
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));

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

			this.client1.SendIqSet("proxy.draugr.de", "<query xmlns='http://jabber.org/protocol/bytestreams' sid='Stream0001'>" +
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

	}
}
