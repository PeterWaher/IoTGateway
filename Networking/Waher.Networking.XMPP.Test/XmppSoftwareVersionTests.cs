using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.SoftwareVersion;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSoftwareVersionTests : CommunicationTests
	{
		[TestMethod]
		public void SoftwareVersion_Test_01_Server()
		{
			this.ConnectClients();
			SoftwareVersionEventArgs e = this.client1.SoftwareVersion(this.client1.Domain, 10000);
			this.Print(e);
		}

		private void Print(SoftwareVersionEventArgs e)
		{
			Console.Out.WriteLine();
			Console.Out.WriteLine("Name: " + e.Name);
			Console.Out.WriteLine("Version: " + e.Version);
			Console.Out.WriteLine("OS: " + e.OS);
		}

		[TestMethod]
		public void SoftwareVersion_Test_02_Client()
		{
			this.ConnectClients();
			SoftwareVersionEventArgs e = this.client1.SoftwareVersion(this.client1.FullJID, 10000);
			this.Print(e);
		}

	}
}
