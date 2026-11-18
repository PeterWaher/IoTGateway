using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Things.Matter.Attestation;
using Waher.Things.Matter.Commands;
using Waher.Things.Metering;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Optional tests against a physical Matter device on the local network. The tests are only run if the
	/// environment variable MATTER_TEST_PAIRING_CODE contains the manual pairing code or QR code payload of a
	/// device in commissioning mode. To test a device already used in another ecosystem (for instance Apple Home,
	/// Google Home, Amazon Alexa or SmartThings), use the "share" or "turn on pairing mode" feature of that
	/// ecosystem to get a pairing code. The device is removed from the test fabric when the tests are done.
	///
	/// Optional environment variables:
	///
	///     MATTER_TEST_DEVICE_ADDRESS   IP address of the device, if DNS-SD is not available.
	///     MATTER_TEST_DEVICE_PORT      UDP port of the device (default 5540).
	///     MATTER_TEST_ATTESTATION      Attestation policy: Strict (default), Warn or Ignore.
	///     MATTER_TEST_KEEP_DEVICE      If "1", the device is not removed from the test fabric after the tests.
	/// </summary>
	[TestClass]
	public sealed class PhysicalDeviceTests
	{
		private static MatterControllerNode controllerNode = null;
		private static MatterDeviceNode deviceNode = null;
		private static string pairingCode = null;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			pairingCode = Environment.GetEnvironmentVariable("MATTER_TEST_PAIRING_CODE");
			if (string.IsNullOrEmpty(pairingCode))
				return;

			if (!Enum.TryParse(Environment.GetEnvironmentVariable("MATTER_TEST_ATTESTATION") ?? "Strict", true, out AttestationPolicy Policy))
				Policy = AttestationPolicy.Strict;

			controllerNode = new MatterControllerNode()
			{
				NodeId = await MeteringNode.GetUniqueNodeId("Matter Physical Device Test"),
				AttestationPolicy = Policy,
				FabricLabel = "Waher Physical Test"
			};

			await MeteringTopology.Root.AddAsync(controllerNode);
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			if (!(deviceNode is null) && Environment.GetEnvironmentVariable("MATTER_TEST_KEEP_DEVICE") != "1")
			{
				try
				{
					await deviceNode.UnpairAsync();
				}
				catch (Exception ex)
				{
					Console.Out.WriteLine("Unable to remove device from fabric: " + ex.Message);
				}
			}

			if (!(controllerNode is null))
			{
				await controllerNode.StopControllerAsync();
				controllerNode = null;
			}

			deviceNode = null;
		}

		private static void AssertConfigured()
		{
			if (string.IsNullOrEmpty(pairingCode))
				Assert.Inconclusive("Environment variable MATTER_TEST_PAIRING_CODE not set. Physical device tests skipped.");
		}

		private static void AssertDevice()
		{
			AssertConfigured();

			if (deviceNode is null)
				Assert.Inconclusive("Device not commissioned.");
		}

		[TestMethod]
		public async Task Test_01_Commission()
		{
			AssertConfigured();

			string Address = Environment.GetEnvironmentVariable("MATTER_TEST_DEVICE_ADDRESS");
			if (!int.TryParse(Environment.GetEnvironmentVariable("MATTER_TEST_DEVICE_PORT"), out int Port))
				Port = MatterDeviceNode.DefaultPort;

			CommissionDeviceCommand Command = new CommissionDeviceCommand(controllerNode)
			{
				OnboardingCode = pairingCode,
				Address = Address ?? string.Empty,
				Port = Port
			};

			await Command.ExecuteCommandAsync();
			deviceNode = Command.Result;

			Assert.IsNotNull(deviceNode);
			Console.Out.WriteLine("Vendor: " + deviceNode.VendorName + " (0x" + deviceNode.VendorIdentifier.ToString("X4") + ")");
			Console.Out.WriteLine("Product: " + deviceNode.ProductName + " (0x" + deviceNode.ProductIdentifier.ToString("X4") + ")");
			Console.Out.WriteLine("Serial number: " + deviceNode.SerialNumber);
			Console.Out.WriteLine("Software version: " + deviceNode.SoftwareVersion);
		}

		[TestMethod]
		public async Task Test_02_Endpoints()
		{
			AssertDevice();

			MatterEndpointNode[] Endpoints = await deviceNode.GetEndpointNodesAsync();

			foreach (MatterEndpointNode Endpoint in Endpoints)
			{
				Console.Out.WriteLine(Endpoint.EndpointId.ToString() + ": " + Endpoint.NodeId + " [" + Endpoint.DeviceTypesDescription + "]");
				Console.Out.WriteLine("\t" + Endpoint.ClustersDescription);
			}

			Assert.IsTrue(Endpoints.Length > 0, "No application endpoints found.");
		}

		[TestMethod]
		public async Task Test_03_Readout()
		{
			AssertDevice();

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(deviceNode);

			Assert.AreEqual(0, Result.Errors.Count, "Errors reported during readout.");
			Assert.IsTrue(Result.Fields.Count > 0, "No fields reported.");
			Assert.IsNotNull(Result.Find(deviceNode.NodeId, "Vendor Name"));
		}

		[TestMethod]
		public async Task Test_04_ControlParameters()
		{
			AssertDevice();

			foreach (MatterEndpointNode Endpoint in await deviceNode.GetEndpointNodesAsync())
			{
				if (!Endpoint.IsControllable)
					continue;

				foreach (ControlParameters.ControlParameter Parameter in await Endpoint.GetControlParameters())
					Console.Out.WriteLine(Endpoint.NodeId + ": " + Parameter.Name + " = " + await Parameter.GetStringValue(Endpoint));
			}
		}

		[TestMethod]
		public async Task Test_05_Subscription()
		{
			AssertDevice();

			Assert.IsNotNull(await deviceNode.WaitForSubscriptionAsync(TimeSpan.FromSeconds(30)), "Subscription not established.");
		}

		[TestMethod]
		public async Task Test_06_ReadAllAttributes()
		{
			AssertDevice();

			QueryResult Result = await ThingsHelper.ExecuteQueryAsync(new ReadAllAttributesCommand(deviceNode, null), deviceNode);
			Assert.IsTrue(Result.Get("Attributes").Count > 0);
		}
	}
}
