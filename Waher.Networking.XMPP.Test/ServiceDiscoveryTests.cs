using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class ServiceDiscoveryTests : CommunicationTests
	{
		[Test]
		public void Test_01_Server()
		{
			ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client1.Domain, 10000);
			this.Print(e);
		}

		private void Print(ServiceDiscoveryEventArgs e)
		{
			Console.Out.WriteLine();
			Console.Out.WriteLine("Identities:");

			foreach (Identity Identity in e.Identities)
				Console.Out.WriteLine(Identity.ToString());

			Console.Out.WriteLine();
			Console.Out.WriteLine("Features:");
			
			foreach (string Feature in e.Features.Keys)
				Console.Out.WriteLine(Feature);
		}

		[Test]
		public void Test_02_Account()
		{
			ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client2.BareJID, 10000);
			this.Print(e);
		}

		[Test]
		public void Test_03_Client()
		{
			ServiceDiscoveryEventArgs e = this.client1.ServiceDiscovery(this.client2.FullJID, 10000);
			this.Print(e);
		}

	}
}
