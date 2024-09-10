using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Console;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppServiceDiscoveryTests : CommunicationTests
	{
		[TestMethod]
		public void ServiceDiscovery_Test_01_Server()
		{
			this.ConnectClients();
			try
			{
				ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client1.Domain, 10000);
				Print(e);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		private static void Print(ServiceDiscoveryEventArgs e)
		{
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Identities:");

			foreach (Identity Identity in e.Identities)
				ConsoleOut.WriteLine(Identity.ToString());

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Features:");

			foreach (string Feature in e.Features.Keys)
				ConsoleOut.WriteLine(Feature);
		}

		[TestMethod]
		public void ServiceDiscovery_Test_02_Account()
		{
			this.ConnectClients();
			try
			{
				ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client2.BareJID, 10000);
				Print(e);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void ServiceDiscovery_Test_03_Client()
		{
			this.ConnectClients();
			try
			{
				ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client2.FullJID, 10000);
				Print(e);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void ServiceDiscovery_Test_04_ServerItems()
		{
			this.ConnectClients();
			try
			{
				ServiceItemsDiscoveryEventArgs e = this.client1.ServiceItemsDiscovery(this.client1.Domain, 10000);
				Print(e);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		private static void Print(ServiceItemsDiscoveryEventArgs e)
		{
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Items:");

			foreach (Item Item in e.Items)
				ConsoleOut.WriteLine(Item.ToString());
		}

		[TestMethod]
		public void ServiceDiscovery_Test_05_ServerItemFeatures()
		{
			this.ConnectClients();
			try
			{
				ServiceItemsDiscoveryEventArgs e = this.client1.ServiceItemsDiscovery(this.client1.Domain, 10000);

				foreach (Item Item in e.Items)
				{
					ServiceDiscoveryEventArgs e2 = this.client1.ServiceDiscovery(Item.JID, 10000);

					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine(Item.ToString());
					ConsoleOut.WriteLine(new string('=', 80));

					Print(e2);
				}
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void ServiceDiscovery_Test_06_AccountItems()
		{
			this.ConnectClients();
			try
			{
				ServiceItemsDiscoveryEventArgs e = this.client1.ServiceItemsDiscovery(this.client2.BareJID, 10000);
				Print(e);
			}
			finally
			{
				this.DisposeClients();
			}
		}

	}
}
