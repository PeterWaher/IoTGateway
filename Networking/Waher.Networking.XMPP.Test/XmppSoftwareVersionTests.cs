using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.SoftwareVersion;
using Waher.Runtime.Console;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSoftwareVersionTests : CommunicationTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			DisposeSnifferAndLog();
		}

		[TestMethod]
		public void SoftwareVersion_Test_01_Server()
		{
			this.ConnectClients();
			SoftwareVersionEventArgs e = this.client1.SoftwareVersion(this.client1.Domain, 10000);
			Print(e);
		}

		private static void Print(SoftwareVersionEventArgs e)
		{
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Name: " + e.Name);
			ConsoleOut.WriteLine("Version: " + e.Version);
			ConsoleOut.WriteLine("OS: " + e.OS);
		}

		[TestMethod]
		public void SoftwareVersion_Test_02_Client()
		{
			this.ConnectClients();
			SoftwareVersionEventArgs e = this.client1.SoftwareVersion(this.client1.FullJID, 10000);
			Print(e);
		}

	}
}
