using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.SoftwareVersion;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class SoftwareVersionTests : CommunicationTests
	{
		[Test]
		public void Test_01_Server()
		{
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

		[Test]
		public void Test_02_Client()
		{
			SoftwareVersionEventArgs e = this.client1.SoftwareVersion(this.client1.FullJID, 10000);
			this.Print(e);
		}

	}
}
