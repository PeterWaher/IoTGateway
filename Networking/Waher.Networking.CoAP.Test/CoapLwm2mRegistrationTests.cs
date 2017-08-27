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
	public class CoapLwm2mRegistrationTests
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
				new Lwm2mSecurityObject(),
				new Lwm2mServerObject(),
				new Lwm2mAccessControlObject(),
				new Lwm2mDeviceObject(),
				new Lwm2mConnectivityMonitoringObject());
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
		public void LWM2M_Registration_Test_01_Register()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.lwm2mClient.OnRegistrationSuccessful += (sender, e) => Done.Set();
			this.lwm2mClient.OnRegistrationFailed += (sender, e) => Error.Set();

			this.lwm2mClient.Register(20, new Lwm2mServerReference("leshan.eclipse.org"));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}

		[TestMethod]
		public void LWM2M_Registration_Test_02_RegisterUpdate()
		{
			this.LWM2M_Registration_Test_01_Register();

			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.lwm2mClient.OnRegistrationSuccessful += (sender, e) => Done.Set();
			this.lwm2mClient.OnRegistrationFailed += (sender, e) => Error.Set();

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 20000));
		}

		[TestMethod]
		public void LWM2M_Registration_Test_03_Deregister()
		{
			this.LWM2M_Registration_Test_02_RegisterUpdate();

			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.lwm2mClient.OnDeregistrationSuccessful += (sender, e) => Done.Set();
			this.lwm2mClient.OnDeregistrationFailed += (sender, e) => Error.Set();
			this.lwm2mClient.Deregister();

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 20000));
		}


	}
}
