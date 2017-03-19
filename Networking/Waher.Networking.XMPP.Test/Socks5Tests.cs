using System;
using System.Collections.Generic;
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
		public void Test_03_ConnectCommand()
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
					case Socks5State.Connected:
						Client.CONNECT("Stream0001", this.client1.FullJID, this.client2.FullJID);
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						Error.Set();
						break;
				}
			};

			Client.OnResponse += (sender, e) =>
			{
				if (e.Ok)
					Done.Set();
				else
					Error.Set();
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Unable to connect.");
		}

	}
}
