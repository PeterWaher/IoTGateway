using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Things.ControlParameters;
using Waher.Things.Matter.Commands;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Test.Devices;
using Waher.Things.Metering;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the integration of a Matter bridge, exposing bridged devices, using a Matter test device based on the
	/// matter.js reference implementation.
	/// </summary>
	[TestClass]
	public sealed class BridgeTests
	{
		private static MatterTestDevice device = null;
		private static MatterControllerNode controllerNode = null;
		private static MatterDeviceNode deviceNode = null;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			device = await MatterTestDevice.StartAsync("bridge-tests", Bridge: true);

			controllerNode = new MatterControllerNode()
			{
				NodeId = await MeteringNode.GetUniqueNodeId("Matter Bridge Test"),
				TrustDevelopmentPaas = true,
				AllowTestCertificationDeclarations = true
			};

			await MeteringTopology.Root.AddAsync(controllerNode);
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			if (!(controllerNode is null))
			{
				await controllerNode.StopControllerAsync();
				controllerNode = null;
			}

			device?.Dispose();
			device = null;
		}

		[TestMethod]
		public async Task Test_01_Commission()
		{
			CommissionDeviceCommand Command = new CommissionDeviceCommand(controllerNode)
			{
				OnboardingCode = device.ManualCode,
				Address = device.LocalEndPoint.Address.ToString(),
				Port = device.Port
			};

			await Command.ExecuteCommandAsync();
			deviceNode = Command.Result;

			Assert.IsNotNull(deviceNode);
			Assert.AreEqual("Test Bridge", deviceNode.NodeId, "Node label should be used as node ID.");
			Assert.AreEqual("Waher Test Bridge", deviceNode.ProductName);
		}

		[TestMethod]
		public async Task Test_02_BridgedEndpoints()
		{
			if (deviceNode is null)
				Assert.Inconclusive("Bridge not commissioned.");

			MatterEndpointNode[] Endpoints = await deviceNode.GetEndpointNodesAsync();

			foreach (MatterEndpointNode Endpoint in Endpoints)
				Console.Out.WriteLine(Endpoint.EndpointId.ToString() + ": " + Endpoint.NodeId + " [" + Endpoint.DeviceTypesDescription + "] " + Endpoint.Label);

			Assert.AreEqual(2, Endpoints.Length, "Aggregator endpoint should not be represented by a node.");

			MatterEndpointNode Lamp = await deviceNode.FindEndpointNodeAsync(2);
			Assert.IsNotNull(Lamp);
			Assert.AreEqual("Bridged Lamp", Lamp.Label);
			Assert.AreEqual("Test Bridge, Bridged Lamp", Lamp.NodeId);
			Assert.IsNotNull(Lamp.GetCluster(ClusterIds.BridgedDeviceBasicInformation));

			MatterEndpointNode Thermometer = await deviceNode.FindEndpointNodeAsync(3);
			Assert.IsNotNull(Thermometer);
			Assert.AreEqual("Bridged Thermometer", Thermometer.Label);
		}

		[TestMethod]
		public async Task Test_03_ReadBridgedDevices()
		{
			if (deviceNode is null)
				Assert.Inconclusive("Bridge not commissioned.");

			MatterEndpointNode Lamp = await deviceNode.FindEndpointNodeAsync(2);
			MatterEndpointNode Thermometer = await deviceNode.FindEndpointNodeAsync(3);
			ReadoutResult Result = await ThingsHelper.ReadoutAsync(deviceNode);

			Assert.AreEqual(0, Result.Errors.Count);
			Assert.AreEqual(19, Result.Quantity(Thermometer, "Temperature"), 0.001);
			Assert.IsTrue(Result.Boolean(Thermometer, "Reachable"));
			Assert.AreEqual("Bridged Thermometer", Result.String(Thermometer, "Node Label"));
			Assert.IsTrue(Result.Boolean(Lamp, "Reachable"));
			Assert.IsFalse(Result.Boolean(Lamp, "On"));
		}

		[TestMethod]
		public async Task Test_04_ControlBridgedDevice()
		{
			if (deviceNode is null)
				Assert.Inconclusive("Bridge not commissioned.");

			MatterEndpointNode Lamp = await deviceNode.FindEndpointNodeAsync(2);
			BooleanControlParameter On = await Lamp.GetControlParameter("On") as BooleanControlParameter;

			Assert.IsNotNull(On);
			Assert.IsTrue(await On.Set(Lamp, true));
			Assert.IsTrue(await On.Get(Lamp));
			Assert.IsTrue(await On.Set(Lamp, false));
			Assert.IsFalse(await On.Get(Lamp));
		}

		[TestMethod]
		public async Task Test_99_RemoveFromFabric()
		{
			if (deviceNode is null)
				Assert.Inconclusive("Bridge not commissioned.");

			await deviceNode.UnpairAsync();
			await controllerNode.RemoveAsync(deviceNode);
			await deviceNode.DestroyAsync();

			deviceNode = null;
		}
	}
}
