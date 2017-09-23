using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Interoperability;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppInteroperabilityTests : CommunicationTests
	{
		private InteroperabilityClient interopClient;
		private InteroperabilityServer interopServer;

		[TestInitialize]
		public override void Setup()
		{
			base.Setup();

			this.interopClient = new InteroperabilityClient(this.client1);
			this.interopServer = new InteroperabilityServer(this.client2);

			this.interopServer.OnGetInterfaces += (sender, e) =>
			{
				e.Add("Interface A", "Interface B", "Interface C", "Interface D");
			};
		}

		[TestCleanup]
		public override void TearDown()
		{
			this.interopServer.Dispose();
			this.interopServer = null;

			this.interopClient.Dispose();
			this.interopClient = null;

			base.TearDown();
		}

		[TestMethod]
		public void Interoperability_Test_01_GetInterfaces()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			string[] Interfaces = this.interopClient.GetInterfaces(this.client2.FullJID, 10000);

			Assert.AreEqual(4, Interfaces.Length);
			Assert.AreEqual("Interface A", Interfaces[0]);
			Assert.AreEqual("Interface B", Interfaces[1]);
			Assert.AreEqual("Interface C", Interfaces[2]);
			Assert.AreEqual("Interface D", Interfaces[3]);
		}
	}
}
