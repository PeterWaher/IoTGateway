using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Things.ControlParameters;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.Commands;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Fabric;
using Waher.Things.Matter.Test.Devices;
using Waher.Things.Metering;
using Waher.Things.Queries;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the integration of Matter devices into the metering topology, as sensors and actuators, using a Matter
	/// test device based on the matter.js reference implementation.
	/// </summary>
	[TestClass]
	public sealed class ThingsTests
	{
		private static MatterTestDevice device = null;
		private static MatterControllerNode controllerNode = null;
		private static MatterDeviceNode deviceNode = null;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			device = await MatterTestDevice.StartAsync("things-tests");

			controllerNode = new MatterControllerNode()
			{
				NodeId = await MeteringNode.GetUniqueNodeId("Matter Things Test"),
				TrustDevelopmentPaas = true,
				AllowTestCertificationDeclarations = true,
				FabricLabel = "Waher Things Test"
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

		private static void AssertDevice()
		{
			if (deviceNode is null)
				Assert.Inconclusive("Device not commissioned.");
		}

		private static async Task<MatterEndpointNode> GetEndpoint(ushort EndpointId)
		{
			AssertDevice();

			MatterEndpointNode Result = await deviceNode.FindEndpointNodeAsync(EndpointId);
			Assert.IsNotNull(Result, "Endpoint " + EndpointId.ToString() + " not found.");

			return Result;
		}

		private static async Task<T> GetParameter<T>(MatterEndpointNode Endpoint, string Name)
			where T : ControlParameter
		{
			ControlParameter Parameter = await Endpoint.GetControlParameter(Name);

			Assert.IsNotNull(Parameter, "Control parameter " + Name + " not found on " + Endpoint.NodeId);
			Assert.IsInstanceOfType<T>(Parameter);

			return (T)Parameter;
		}

		[TestMethod]
		public async Task Test_01_DiscoverDevices()
		{
			QueryResult Result = await ThingsHelper.ExecuteQueryAsync(new DiscoverDevicesCommand(controllerNode), controllerNode);
			bool Found = false;

			foreach (Record Record in Result.Get("Devices"))
			{
				if (Record.Elements[2] is int Discriminator && Discriminator == device.Discriminator)
					Found = true;
			}

			if (!Found && Result.Messages.Count == 0 && Result.Get("Devices").Count == 0)
				Assert.Inconclusive("No commissionable devices found. DNS-SD may not be available.");

			Assert.IsTrue(Found, "Test device not found.");
		}

		[TestMethod]
		public async Task Test_02_Commission()
		{
			CommissionDeviceCommand Command = new CommissionDeviceCommand(controllerNode)
			{
				OnboardingCode = device.QrCode,
				NodeName = "Test Device"
			};

			await Command.ExecuteCommandAsync();

			deviceNode = Command.Result;

			Assert.IsNotNull(deviceNode);
			Assert.AreEqual("Test Device", deviceNode.NodeId);
			Assert.AreEqual("matter.js", deviceNode.VendorName);
			Assert.AreEqual("Waher Test Device", deviceNode.ProductName);
			Assert.AreEqual(0xfff1, deviceNode.VendorIdentifier);
			Assert.AreEqual(0x8000, deviceNode.ProductIdentifier);
			Assert.AreEqual("WTD-things-tests", deviceNode.SerialNumber);
			Assert.AreEqual("Waher Test Device", deviceNode.Model);
			Assert.AreEqual("1.0.0", deviceNode.SoftwareVersion);
			Assert.IsTrue(deviceNode.TryGetOperationalNodeId(out ulong _));
			Assert.IsFalse(string.IsNullOrEmpty(deviceNode.Host));
			Assert.AreSame(controllerNode, await deviceNode.GetParent());
		}

		[TestMethod]
		public async Task Test_03_EndpointNodes()
		{
			AssertDevice();

			MatterEndpointNode[] Endpoints = await deviceNode.GetEndpointNodesAsync();

			foreach (MatterEndpointNode Endpoint in Endpoints)
				Console.Out.WriteLine(Endpoint.EndpointId.ToString() + ": " + Endpoint.NodeId + " [" + Endpoint.DeviceTypesDescription + "] " + Endpoint.ClustersDescription);

			Assert.AreEqual(14, Endpoints.Length);

			MatterEndpointNode Light = await GetEndpoint(1);
			Assert.IsTrue(Light.HasDeviceType(DeviceTypeIds.ExtendedColorLight));
			Assert.AreEqual("Test Device, Extended Color Light", Light.NodeId);
			Assert.IsTrue(Light.IsControllable);
			Assert.IsTrue(Light.IsReadable);
			Assert.IsNotNull(Light.GetCluster(ClusterIds.ColorControl));
			Assert.IsTrue(Light.GetCluster(ClusterIds.ColorControl).HasFeature(0), "Hue/Saturation feature missing.");
			Assert.IsTrue(Light.GetCluster(ClusterIds.OnOff).HasCommand(0x02), "Toggle command missing.");
			Assert.IsNotNull(Light.GetCluster(ClusterIds.OnOff).Attributes);

			MatterEndpointNode Temperature = await GetEndpoint(2);
			Assert.IsTrue(Temperature.HasDeviceType(DeviceTypeIds.TemperatureSensor));
			Assert.IsFalse(Temperature.IsControllable);

			Assert.IsNotNull(deviceNode.RootClusters);
			Assert.IsNotNull(MatterClusterInfo.Find(deviceNode.RootClusters, ClusterIds.BasicInformation));
		}

		[TestMethod]
		public async Task Test_04_ReadoutDevice()
		{
			AssertDevice();

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(deviceNode);

			Assert.AreEqual(0, Result.Errors.Count, "Errors reported.");

			Assert.AreEqual("matter.js", Result.String(deviceNode, "Vendor Name"));
			Assert.AreEqual("Waher Test Device", Result.String(deviceNode, "Product Name"));
			Assert.AreEqual("WTD-things-tests", Result.String(deviceNode, "Serial Number"));

			Assert.IsFalse(Result.Boolean(await GetEndpoint(1), "On"));
			Assert.AreEqual(4000, Result.Quantity(await GetEndpoint(1), "Color Temperature"), 1);
			Assert.AreEqual(21.5, Result.Quantity(await GetEndpoint(2), "Temperature"), 0.001);
			Assert.AreEqual(45.5, Result.Quantity(await GetEndpoint(3), "Humidity"), 0.001);
			Assert.IsTrue(Result.Boolean(await GetEndpoint(4), "Contact"));
			Assert.IsFalse(Result.Boolean(await GetEndpoint(5), "Occupancy"));
			Assert.AreEqual(101.3, Result.Quantity(await GetEndpoint(6), "Pressure"), 0.001);
			Assert.AreEqual(1000, Result.Quantity(await GetEndpoint(7), "Illuminance"), 1);
			Assert.AreEqual(12.3, Result.Quantity(await GetEndpoint(8), "Flow"), 0.001);
			Assert.IsTrue(Result.Boolean(await GetEndpoint(9), "Locked"));
			Assert.AreEqual(50, Result.Quantity(await GetEndpoint(10), "Lift Position"), 0.001);
			Assert.AreEqual(20, Result.Quantity(await GetEndpoint(11), "Temperature"), 0.001);
			Assert.AreEqual(21, Result.Quantity(await GetEndpoint(11), "Heating Setpoint"), 0.001);
			Assert.AreEqual(26, Result.Quantity(await GetEndpoint(11), "Cooling Setpoint"), 0.001);
			Assert.AreEqual("Heat", Result.String(await GetEndpoint(11), "System Mode"));
			Assert.AreEqual("Off", Result.String(await GetEndpoint(12), "Fan Mode"));
			Assert.AreEqual("0", Result.String(await GetEndpoint(13), "Position"));
			Assert.IsTrue(Result.Boolean(await GetEndpoint(14), "On"));
			Assert.AreEqual(90, Result.Quantity(await GetEndpoint(14), "Battery"), 0.001);
			Assert.AreEqual(3, Result.Quantity(await GetEndpoint(14), "Battery Voltage"), 0.001);
		}

		[TestMethod]
		public async Task Test_05_ReadoutEndpoint()
		{
			MatterEndpointNode Temperature = await GetEndpoint(2);
			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Temperature);

			Assert.AreEqual(0, Result.Errors.Count, "Errors reported.");
			Assert.AreEqual(1, Result.Fields.Count);
			Assert.AreEqual(21.5, Result.Quantity(Temperature, "Temperature"), 0.001);

			QuantityField F = (QuantityField)Result.Get(Temperature, "Temperature");
			Assert.AreEqual("°C", F.Unit);
			Assert.AreEqual(FieldType.Momentary, F.Type);
		}

		[TestMethod]
		public async Task Test_06_ReadoutFieldFilter()
		{
			AssertDevice();

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(deviceNode, FieldType.All, new string[] { "Temperature" });

			Assert.AreEqual(2, Result.Fields.Count);

			foreach (Field Field in Result.Fields)
				Assert.AreEqual("Temperature", Field.Name);
		}

		[TestMethod]
		public async Task Test_07_ReadoutIdentityOnly()
		{
			AssertDevice();

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(deviceNode, FieldType.Identity);

			Assert.IsTrue(Result.Fields.Count > 0);

			foreach (Field Field in Result.Fields)
				Assert.IsTrue(Field.Type.HasFlag(FieldType.Identity), Field.Name + " is not an identity field.");
		}

		[TestMethod]
		public async Task Test_08_ControlOnOff()
		{
			MatterEndpointNode Light = await GetEndpoint(1);
			BooleanControlParameter On = await GetParameter<BooleanControlParameter>(Light, "On");

			Assert.IsTrue(await On.Set(Light, true));
			Assert.IsTrue(await On.Get(Light));
			Assert.IsTrue((await ThingsHelper.ReadoutAsync(Light)).Boolean(Light, "On"));

			Assert.IsTrue(await On.Set(Light, false));
			Assert.IsFalse(await On.Get(Light));
			Assert.IsFalse((await ThingsHelper.ReadoutAsync(Light)).Boolean(Light, "On"));
		}

		[TestMethod]
		public async Task Test_09_ControlLevel()
		{
			MatterEndpointNode Light = await GetEndpoint(1);
			Int32ControlParameter Level = await GetParameter<Int32ControlParameter>(Light, "Level");
			DoubleControlParameter Percent = await GetParameter<DoubleControlParameter>(Light, "Level (%)");

			Assert.IsTrue(await Level.Set(Light, 200));
			Assert.AreEqual(200, await Level.Get(Light));

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Light);
			Assert.AreEqual("200", Result.String(Light, "Level"));
			Assert.IsTrue(Result.Boolean(Light, "On"), "Light not turned on by level command.");

			Assert.IsTrue(await Percent.Set(Light, 50));
			Assert.AreEqual(50, (await Percent.Get(Light)).Value, 0.5);

			Assert.IsTrue(await Light.GetControlParameter("On") is BooleanControlParameter On && await On.Set(Light, false));
		}

		[TestMethod]
		public async Task Test_10_ControlColorTemperature()
		{
			MatterEndpointNode Light = await GetEndpoint(1);
			DoubleControlParameter Kelvin = await GetParameter<DoubleControlParameter>(Light, "Color Temperature");

			Assert.IsTrue(await Kelvin.Set(Light, 2700));
			Assert.AreEqual(2700, (await Kelvin.Get(Light)).Value, 10);

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Light);
			Assert.AreEqual(2700, Result.Quantity(Light, "Color Temperature"), 10);
			Assert.AreEqual("Color Temperature Mireds", Result.String(Light, "Color Mode"));
		}

		[TestMethod]
		public async Task Test_11_ControlColor()
		{
			MatterEndpointNode Light = await GetEndpoint(1);
			ColorControlParameter Color = await GetParameter<ColorControlParameter>(Light, "Color");

			Assert.IsTrue(await Color.Set(Light, new ColorReference(255, 0, 0)));

			ColorReference Value = await Color.Get(Light);
			Assert.IsNotNull(Value);
			Assert.AreEqual(255, Value.Red);
			Assert.AreEqual(0, Value.Green);
			Assert.AreEqual(0, Value.Blue);

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Light);
			Assert.AreEqual(0, Result.Quantity(Light, "Hue"), 0.5);
			Assert.AreEqual(100, Result.Quantity(Light, "Saturation"), 0.5);
			Assert.AreEqual("#ff0000", Result.String(Light, "Color"));

			Assert.IsTrue(await Color.Set(Light, new ColorReference(0, 0, 255)));
			Result = await ThingsHelper.ReadoutAsync(Light);
			Assert.AreEqual(240, Result.Quantity(Light, "Hue"), 1.5);
		}

		[TestMethod]
		public async Task Test_12_ControlDoorLock()
		{
			MatterEndpointNode Lock = await GetEndpoint(9);
			BooleanControlParameter Locked = await GetParameter<BooleanControlParameter>(Lock, "Locked");

			Assert.IsTrue(await Locked.Set(Lock, false));
			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Lock);
			Assert.IsFalse(Result.Boolean(Lock, "Locked"));
			Assert.AreEqual("Unlocked", Result.String(Lock, "Lock State"));

			Assert.IsTrue(await Locked.Set(Lock, true));
			Result = await ThingsHelper.ReadoutAsync(Lock);
			Assert.IsTrue(Result.Boolean(Lock, "Locked"));
			Assert.AreEqual("Locked", Result.String(Lock, "Lock State"));
		}

		[TestMethod]
		public async Task Test_13_ControlWindowCovering()
		{
			MatterEndpointNode Cover = await GetEndpoint(10);
			DoubleControlParameter Lift = await GetParameter<DoubleControlParameter>(Cover, "Lift Position");

			Assert.IsTrue(await Lift.Set(Cover, 25));

			DateTime Until = DateTime.UtcNow.AddSeconds(30);
			double? Current;

			do
			{
				Current = await Lift.Get(Cover);
				if (Current.HasValue && Math.Abs(Current.Value - 25) < 0.01)
					break;

				await Task.Delay(250);
			}
			while (DateTime.UtcNow < Until);

			Assert.AreEqual(25, Current.Value, 0.01);

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Cover);
			Assert.AreEqual(25, Result.Quantity(Cover, "Target Lift Position"), 0.01);
			Assert.AreEqual(25, Result.Quantity(Cover, "Lift Position"), 0.01);
		}

		[TestMethod]
		public async Task Test_14_ControlThermostat()
		{
			MatterEndpointNode Thermostat = await GetEndpoint(11);
			DoubleControlParameter Heating = await GetParameter<DoubleControlParameter>(Thermostat, "Heating Setpoint");
			StringControlParameter Mode = await GetParameter<StringControlParameter>(Thermostat, "System Mode");

			Assert.IsTrue(await Heating.Set(Thermostat, 22.5));
			Assert.AreEqual(22.5, (await Heating.Get(Thermostat)).Value, 0.001);

			Assert.IsTrue(await Mode.Set(Thermostat, "Cool"));
			Assert.AreEqual("Cool", await Mode.Get(Thermostat));

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Thermostat);
			Assert.AreEqual(22.5, Result.Quantity(Thermostat, "Heating Setpoint"), 0.001);
			Assert.AreEqual("Cool", Result.String(Thermostat, "System Mode"));

			Assert.IsTrue(await Mode.Set(Thermostat, "Heat"));
		}

		[TestMethod]
		public async Task Test_15_ControlFan()
		{
			MatterEndpointNode Fan = await GetEndpoint(12);
			StringControlParameter Mode = await GetParameter<StringControlParameter>(Fan, "Fan Mode");
			Int32ControlParameter Speed = await GetParameter<Int32ControlParameter>(Fan, "Fan Speed Setting");

			Assert.IsTrue(await Mode.Set(Fan, "High"));
			Assert.AreEqual("High", await Mode.Get(Fan));

			Assert.IsTrue(await Speed.Set(Fan, 50));
			Assert.AreEqual(50, await Speed.Get(Fan));

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(Fan);
			Assert.AreEqual(50, Result.Quantity(Fan, "Fan Speed Setting"), 0.001);

			Assert.IsTrue(await Mode.Set(Fan, "Off"));
			Assert.AreEqual("Off", await Mode.Get(Fan));
		}

		[TestMethod]
		public async Task Test_16_EndpointCommands()
		{
			MatterEndpointNode Light = await GetEndpoint(1);
			BooleanControlParameter On = await GetParameter<BooleanControlParameter>(Light, "On");
			bool Before = (await On.Get(Light)).Value;

			ICommand Toggle = await ThingsHelper.GetCommandAsync(Light, "Toggle");
			Assert.IsTrue(await Toggle.CanExecuteAsync(RequestOrigin.Empty));
			await Toggle.ExecuteCommandAsync();

			Assert.AreEqual(!Before, await On.Get(Light));

			IdentifyCommand Identify = await ThingsHelper.GetCommandAsync<IdentifyCommand>(Light);
			Identify.Seconds = 2;
			await Identify.ExecuteCommandAsync();

			MatterEndpointNode Cover = await GetEndpoint(10);
			ICommand Stop = await ThingsHelper.GetCommandAsync(Cover, "Stop");
			await Stop.ExecuteCommandAsync();
		}

		[TestMethod]
		public async Task Test_17_MomentaryValues()
		{
			AssertDevice();

			Assert.IsNotNull(await deviceNode.WaitForSubscriptionAsync(TimeSpan.FromSeconds(20)), "Subscription not established.");

			MatterEndpointNode Temperature = await GetEndpoint(2);
			TaskCompletionSource<double> Reported = new TaskCompletionSource<double>(TaskCreationOptions.RunContinuationsAsynchronously);

			Task Handler(IThingReference Reference, IEnumerable<Field> Values)
			{
				if (Reference.NodeId == Temperature.NodeId)
				{
					foreach (Field Field in Values)
					{
						if (Field is QuantityField Q && Field.Name == "Temperature" && Math.Abs(Q.Value - 24.68) < 0.001)
							Reported.TrySetResult(Q.Value);
					}
				}

				return Task.CompletedTask;
			}

			MeteringTopology.OnNewMomentaryValues += Handler;
			try
			{
				await device.SendCommandAsync("temperature 24.68");
				Assert.AreEqual(Reported.Task, await Task.WhenAny(Reported.Task, Task.Delay(10000)), "Momentary value not reported.");
			}
			finally
			{
				MeteringTopology.OnNewMomentaryValues -= Handler;
			}
		}

		[TestMethod]
		public async Task Test_18_MomentaryValues_Contact()
		{
			AssertDevice();

			Assert.IsNotNull(await deviceNode.WaitForSubscriptionAsync(TimeSpan.FromSeconds(20)), "Subscription not established.");

			MatterEndpointNode Contact = await GetEndpoint(4);
			TaskCompletionSource<bool> Reported = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			Task Handler(IThingReference Reference, IEnumerable<Field> Values)
			{
				if (Reference.NodeId == Contact.NodeId)
				{
					foreach (Field Field in Values)
					{
						if (Field is BooleanField B && Field.Name == "Contact" && !B.Value)
							Reported.TrySetResult(B.Value);
					}
				}

				return Task.CompletedTask;
			}

			MeteringTopology.OnNewMomentaryValues += Handler;
			try
			{
				await device.SendCommandAsync("contact false");
				Assert.AreEqual(Reported.Task, await Task.WhenAny(Reported.Task, Task.Delay(10000)), "Momentary value not reported.");
			}
			finally
			{
				MeteringTopology.OnNewMomentaryValues -= Handler;
				await device.SendCommandAsync("contact true");
			}
		}

		[TestMethod]
		public async Task Test_19_MomentaryValues_Events()
		{
			AssertDevice();

			Assert.IsNotNull(await deviceNode.WaitForSubscriptionAsync(TimeSpan.FromSeconds(20)), "Subscription not established.");

			MatterEndpointNode Switch = await GetEndpoint(13);
			TaskCompletionSource<string> InitialPress = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
			TaskCompletionSource<string> ShortRelease = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

			Task Handler(IThingReference Reference, IEnumerable<Field> Values)
			{
				if (Reference.NodeId == Switch.NodeId)
				{
					foreach (Field Field in Values)
					{
						if (Field.Name == "Switch Event")
						{
							if (Field.ValueString == "Initial Press")
								InitialPress.TrySetResult(Field.ValueString);
							else if (Field.ValueString == "Short Release")
								ShortRelease.TrySetResult(Field.ValueString);
						}
					}
				}

				return Task.CompletedTask;
			}

			MeteringTopology.OnNewMomentaryValues += Handler;
			try
			{
				await device.SendCommandAsync("press");

				Task Both = Task.WhenAll(InitialPress.Task, ShortRelease.Task);
				Assert.AreEqual(Both, await Task.WhenAny(Both, Task.Delay(10000)), "Switch events not reported.");
			}
			finally
			{
				MeteringTopology.OnNewMomentaryValues -= Handler;
			}
		}

		[TestMethod]
		public async Task Test_20_ReadAllAttributes()
		{
			AssertDevice();

			QueryResult Result = await ThingsHelper.ExecuteQueryAsync(new ReadAllAttributesCommand(deviceNode, null), deviceNode);
			List<Record> Records = Result.Get("Attributes");
			bool Found = false;

			Assert.IsTrue(Records.Count > 300, "Too few attributes reported.");

			foreach (Record Record in Records)
			{
				if (Record.Elements[1] is string Cluster && Cluster == "Basic Information" &&
					Record.Elements[2] is string Attribute && Attribute == "VendorName")
				{
					Assert.AreEqual("matter.js", Record.Elements[3]);
					Found = true;
				}
			}

			Assert.IsTrue(Found, "Vendor name not found.");

			MatterEndpointNode Light = await GetEndpoint(1);
			Result = await ThingsHelper.ExecuteQueryAsync(new ReadAllAttributesCommand(deviceNode, Light), Light);

			foreach (Record Record in Result.Get("Attributes"))
				Assert.AreEqual(1, Record.Elements[0]);
		}

		[TestMethod]
		public async Task Test_21_ReadEvents()
		{
			AssertDevice();

			QueryResult Result = await ThingsHelper.ExecuteQueryAsync(new ReadEventsCommand(deviceNode, null), deviceNode);
			List<Record> Records = Result.Get("Events");
			bool StartUp = false;

			Assert.IsTrue(Records.Count > 0, "No events reported.");

			foreach (Record Record in Records)
			{
				if (Record.Elements[4] is string Event && Event == "StartUp")
					StartUp = true;
			}

			Assert.IsTrue(StartUp, "StartUp event not reported.");
		}

		[TestMethod]
		public async Task Test_22_ScanFabric()
		{
			AssertDevice();

			MatterController Controller = await controllerNode.GetControllerAsync();
			if (Controller.Mdns is null)
				Assert.Inconclusive("DNS-SD not available.");

			QueryResult Result = await ThingsHelper.ExecuteQueryAsync(new ScanFabricCommand(controllerNode), controllerNode);
			bool Found = false;

			foreach (Record Record in Result.Get("Nodes"))
			{
				if (Record.Elements[0] is string NodeId && NodeId == deviceNode.OperationalNodeId)
				{
					Assert.AreEqual(deviceNode.NodeId, Record.Elements[3]);
					Found = true;
				}
			}

			Assert.IsTrue(Found, "Device not found in fabric.");
		}

		[TestMethod]
		public async Task Test_23_ShareDevice()
		{
			AssertDevice();

			ShareDeviceCommand Command = new ShareDeviceCommand(deviceNode)
			{
				Minutes = 3
			};

			QueryResult Result = await ThingsHelper.ExecuteQueryAsync(Command, deviceNode);
			List<Record> Records = Result.Get("Window");

			Assert.AreEqual(4, Records.Count);

			string ManualCode = Records[0].Elements[1] as string;
			string QrCode = Records[1].Elements[1] as string;

			Assert.IsFalse(string.IsNullOrEmpty(ManualCode));
			Assert.IsTrue(QrCode?.StartsWith("MT:") ?? false);

			using MatterController Controller2 = ControllerTests.CreateController();

			try
			{
				MatterNode Node2 = await Controller2.CommissionAsync(ManualCode, device.LocalEndPoint);
				Assert.AreEqual("Waher Test Device", (await Node2.ReadAttributeAsync(0, ClusterIds.BasicInformation, 0x0003)).AsString());
				await Node2.RemoveFromFabricAsync();
			}
			finally
			{
				await Controller2.StopAsync();
			}
		}

		[TestMethod]
		public async Task Test_24_Persistence()
		{
			AssertDevice();

			MatterControllerNode StoredController = await Database.FindFirstIgnoreRest<MeteringNode>(
				new FilterFieldEqualTo("NodeId", controllerNode.NodeId)) as MatterControllerNode;

			Assert.IsNotNull(StoredController);
			Assert.IsTrue(StoredController.HasFabric);

			MatterFabric Fabric = MatterFabric.Import(
				Convert.FromBase64String(StoredController.FabricInformation),
				Convert.FromBase64String(StoredController.RootCaPrivateKey),
				Convert.FromBase64String(StoredController.OperationalPrivateKey),
				Convert.FromBase64String(StoredController.IpkEpochKey));
			MatterController Controller = await controllerNode.GetControllerAsync();

			Assert.AreEqual(Controller.Fabric.FabricId, Fabric.FabricId);
			Assert.AreEqual(Controller.Fabric.ControllerNodeId, Fabric.ControllerNodeId);

			MatterEndpointNode StoredEndpoint = await Database.FindFirstIgnoreRest<MeteringNode>(
				new FilterFieldEqualTo("NodeId", (await GetEndpoint(1)).NodeId)) as MatterEndpointNode;

			Assert.IsNotNull(StoredEndpoint);
			Assert.AreEqual(1, StoredEndpoint.EndpointId);
			Assert.IsNotNull(StoredEndpoint.Clusters);
			Assert.IsNotNull(StoredEndpoint.DeviceTypes);
			Assert.IsTrue(StoredEndpoint.HasDeviceType(DeviceTypeIds.ExtendedColorLight));

			MatterClusterInfo ColorControl = StoredEndpoint.GetCluster(ClusterIds.ColorControl);
			Assert.IsNotNull(ColorControl);
			Assert.IsNotNull(ColorControl.Attributes);
			Assert.IsNotNull(ColorControl.Commands);
			Assert.IsTrue(ColorControl.HasFeature(4), "Color temperature feature not persisted.");

			// Restart controller, which forces new sessions to be established using persisted credentials.

			await controllerNode.RestartControllerAsync();

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(await GetEndpoint(2));
			Assert.AreEqual(0, Result.Errors.Count);
			Assert.AreEqual(24.68, Result.Quantity(await GetEndpoint(2), "Temperature"), 0.001);

			Assert.IsNotNull(await deviceNode.WaitForSubscriptionAsync(TimeSpan.FromSeconds(20)), "Subscription not re-established.");
		}

		[TestMethod]
		public async Task Test_25_RescanEndpoints()
		{
			AssertDevice();

			MatterEndpointNode[] Before = await deviceNode.GetEndpointNodesAsync();
			await (await ThingsHelper.GetCommandAsync<RescanEndpointsCommand>(deviceNode)).ExecuteCommandAsync();
			MatterEndpointNode[] After = await deviceNode.GetEndpointNodesAsync();

			Assert.AreEqual(Before.Length, After.Length);
		}

		[TestMethod]
		public async Task Test_26_Reconnect()
		{
			AssertDevice();

			await (await ThingsHelper.GetCommandAsync<ReconnectCommand>(deviceNode)).ExecuteCommandAsync();
			Assert.IsNotNull(await deviceNode.WaitForSubscriptionAsync(TimeSpan.FromSeconds(20)), "Subscription not re-established.");

			ReadoutResult Result = await ThingsHelper.ReadoutAsync(await GetEndpoint(3));
			Assert.AreEqual(45.5, Result.Quantity(await GetEndpoint(3), "Humidity"), 0.001);
		}

		[TestMethod]
		public async Task Test_99_RemoveFromFabric()
		{
			AssertDevice();

			string NodeId = deviceNode.NodeId;
			RemoveFromFabricCommand Command = await ThingsHelper.GetCommandAsync<RemoveFromFabricCommand>(deviceNode);

			await Command.ExecuteCommandAsync();

			Assert.IsNotNull(await Database.FindFirstIgnoreRest<MeteringNode>(new FilterFieldEqualTo("NodeId", controllerNode.NodeId)),
				"Controller node not found.");
			Assert.IsNull(await Database.FindFirstIgnoreRest<MeteringNode>(new FilterFieldEqualTo("NodeId", NodeId)),
				"Device node not deleted.");

			foreach (INode Child in await controllerNode.ChildNodes)
				Assert.AreNotEqual(NodeId, Child.NodeId);

			deviceNode = null;

			// Device should now be possible to commission again, after opening its commissioning window.
		}
	}
}
