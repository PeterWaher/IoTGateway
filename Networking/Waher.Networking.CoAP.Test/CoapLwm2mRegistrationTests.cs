using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.LWM2M;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapLwm2mRegistrationTests
	{
		private CoapEndpoint coapClient;
		private Lwm2mClient lwm2mClient;

		[TestInitialize]
		public async Task TestInitialize()
		{
			this.coapClient = new CoapEndpoint(new int[] { CoapEndpoint.DefaultCoapPort },
				new int[] { CoapEndpoint.DefaultCoapsPort }, null, null, false, false,
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));

			this.lwm2mClient = new Lwm2mClient("Lwm2mTestClient", this.coapClient,
				new Lwm2mSecurityObject(),
				new Lwm2mServerObject(),
				new Lwm2mAccessControlObject(),
				new Lwm2mDeviceObject("Waher Data AB", "Unit Test", Environment.MachineName,
					Environment.OSVersion.VersionString, "PC", Environment.OSVersion.Platform.ToString(),
					Environment.Version.ToString()));

			await this.lwm2mClient.LoadBootstrapInfo();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.lwm2mClient != null)
			{
				this.lwm2mClient.Dispose();
				this.lwm2mClient = null;
			}

			if (this.coapClient != null)
			{
				CoapResource[] Resources = this.coapClient.GetRegisteredResources();

				this.coapClient.Dispose();
				this.coapClient = null;

				Assert.AreEqual(1, Resources.Length, "There are resources still registered on the CoAP client.");
			}
		}

		[TestMethod]
		public void LWM2M_Registration_Test_01_Register()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.lwm2mClient.OnRegistrationSuccessful += (sender, e) => Done.Set();
			this.lwm2mClient.OnRegistrationFailed += (sender, e) => Error.Set();

			//this.lwm2mClient.Register(20, new Lwm2mServerReference("leshan.eclipse.org"));
			this.lwm2mClient.Register(20, new Lwm2mServerReference("leshan.eclipse.org",
				new PresharedKey("testid", new byte[] { 1, 2, 3, 4 })));

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
