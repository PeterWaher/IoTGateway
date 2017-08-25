using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Runtime.Inventory;
using Waher.Networking.CoAP.LWM2M;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapLwm2mClientTests
	{
		private CoapEndpoint coapClient;
		private Lwm2mClient lwm2mClient;

		[TestInitialize]
		public void TestInitialize()
		{
			this.coapClient = new CoapEndpoint(new int[] { CoapEndpoint.DefaultCoapPort },
				new int[] { CoapEndpoint.DefaultCoapsPort }, null, null, false, false,
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));

			this.lwm2mClient = new Lwm2mClient("Lwm2mTestClient", this.coapClient,
				new Lwm2mServerReference("leshan.eclipse.org"));
				//new Lwm2mServerReference("leshan.eclipse.org", new PresharedKey("testid", new byte[] { 1, 2, 3, 4 })));

			this.lwm2mClient.Add(new Lwm2mServerObject());
			this.lwm2mClient.Add(new Lwm2mAccessControlObject());
			this.lwm2mClient.Add(new Lwm2mDeviceObject());
			this.lwm2mClient.Add(new Lwm2mConnectivityMonitoringObject());
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.coapClient != null)
			{
				ulong[] Tokens = this.coapClient.GetActiveTokens();
				ushort[] MessageIDs = this.coapClient.GetActiveMessageIDs();

				this.coapClient.Dispose();
				this.coapClient = null;

				Assert.AreEqual(0, Tokens.Length, "There are tokens that have not been unregistered properly.");
				Assert.AreEqual(0, MessageIDs.Length, "There are message IDs that have not been unregistered properly.");
			}
		}

		[TestMethod]
		public void LWM2M_Client_Test_01_Discover()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.lwm2mClient.OnServerDiscovered += (sender, e) =>
			{
				Console.Out.WriteLine(e.Server.LinkDocument?.Text);
				Console.Out.WriteLine("Bootstrap interface: " + e.Server.HasBootstrapInterface.Value);
				Console.Out.WriteLine("Registration interface: " + e.Server.HasRegistrationInterface.Value);

				if (/*e.Server.HasBootstrapInterface.Value &&*/ e.Server.HasRegistrationInterface.Value)
					Done.Set();
				else
					Error.Set();
			};

			this.lwm2mClient.Discover();

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}

		[TestMethod]
		public void LWM2M_Client_Test_02_BootstrapRequest()
		{
			this.LWM2M_Client_Test_01_Discover();

			this.lwm2mClient.BootstrapRequest();
			Thread.Sleep(2000);
		}

		[TestMethod]
		public void LWM2M_Client_Test_03_Register()
		{
			this.LWM2M_Client_Test_01_Discover();

			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.lwm2mClient.OnRegistrationSuccessful += (sender, e) => Done.Set();
			this.lwm2mClient.OnRegistrationFailed += (sender, e) => Done.Set();

			this.lwm2mClient.Register(3600);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}


	}
}
