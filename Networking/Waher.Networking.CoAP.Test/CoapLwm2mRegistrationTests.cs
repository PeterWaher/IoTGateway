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
			this.coapClient = new CoapEndpoint([CoapEndpoint.DefaultCoapPort],
				[CoapEndpoint.DefaultCoapsPort], null, null, false, false,
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine));

			this.lwm2mClient = await Lwm2mClient.Create("Lwm2mTestClient", this.coapClient,
				new Lwm2mSecurityObject(),
				new Lwm2mServerObject(),
				new Lwm2mAccessControlObject(),
				new Lwm2mDeviceObject("Waher Data AB", "Unit Test", Environment.MachineName,
					Environment.OSVersion.VersionString, "PC", Environment.OSVersion.Platform.ToString(),
					Environment.Version.ToString()));

			await this.lwm2mClient.LoadBootstrapInfo();
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.lwm2mClient is not null)
			{
				await this.lwm2mClient.DisposeAsync();
				this.lwm2mClient = null;
			}

			if (this.coapClient is not null)
			{
				CoapResource[] Resources = this.coapClient.GetRegisteredResources();

				await this.coapClient.DisposeAsync();
				this.coapClient = null;

				Assert.AreEqual(1, Resources.Length, "There are resources still registered on the CoAP client.");
			}
		}

		[TestMethod]
		public async Task LWM2M_Registration_Test_01_Register()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

			this.lwm2mClient.OnRegistrationSuccessful += (Sender, e) =>
			{
				Done.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnRegistrationFailed += (Sender, e) =>
			{
				Error.Set();
				return Task.CompletedTask;
			};

			//this.lwm2mClient.Register(20, new Lwm2mServerReference("leshan.eclipseprojects.io"));
			await this.lwm2mClient.Register(20, new Lwm2mServerReference("leshan.eclipseprojects.io",
				new PresharedKey("testid", [1, 2, 3, 4])));

			Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 5000));
		}

		[TestMethod]
		public async Task LWM2M_Registration_Test_02_RegisterUpdate()
		{
			await this.LWM2M_Registration_Test_01_Register();

			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

			this.lwm2mClient.OnRegistrationSuccessful += (Sender, e) =>
			{
				Done.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnRegistrationFailed += (Sender, e) =>
			{
				Error.Set();
				return Task.CompletedTask;
			};

			Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 20000));
		}

		[TestMethod]
		public async Task LWM2M_Registration_Test_03_Deregister()
		{
			await this.LWM2M_Registration_Test_02_RegisterUpdate();

			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);

			this.lwm2mClient.OnDeregistrationSuccessful += (Sender, e) =>
			{
				Done.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnDeregistrationFailed += (Sender, e) =>
			{
				Error.Set();
				return Task.CompletedTask;
			};
			await this.lwm2mClient.Deregister();

			Assert.AreEqual(0, WaitHandle.WaitAny([Done, Error], 20000));
		}


	}
}
