using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Things.Matter.Attestation;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Discovery;
using Waher.Things.Matter.Fabric;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Onboarding;
using Waher.Things.Matter.SecureChannel;
using Waher.Things.Matter.Test.Devices;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the Matter controller (commissioner and administrator) against a Matter test device, based on the
	/// matter.js reference implementation.
	/// </summary>
	[TestClass]
	public sealed class ControllerTests
	{
		private static MatterTestDevice device = null;
		private static MatterController controller = null;
		private static MatterNode node = null;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			device = await MatterTestDevice.StartAsync("controller-tests");
			controller = CreateController();
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			if (!(controller is null))
			{
				await controller.StopAsync();
				controller.Dispose();
				controller = null;
			}

			device?.Dispose();
			device = null;
		}

		internal static MatterController CreateController(bool TrustTestDevices = true)
		{
			MatterFabric Fabric = MatterFabric.Create(MatterFabric.NewFabricId(), MatterFabric.TestVendorId, MatterFabric.NewOperationalNodeId());
			MatterController Result = new MatterController(Fabric, new MatterControllerOptions()
			{
				Port = 0,
				AttestationPolicy = AttestationPolicy.Strict,
				TrustDevelopmentPaas = TrustTestDevices,
				AllowTestCertificationDeclarations = TrustTestDevices,
				FabricLabel = "Waher Test"
			});

			Result.Start();

			return Result;
		}

		private static void AssertNode()
		{
			if (node is null)
				Assert.Inconclusive("Device not commissioned.");
		}

		[TestMethod]
		public async Task Test_01_DiscoverCommissionable()
		{
			if (controller.Mdns is null)
				Assert.Inconclusive("DNS-SD not available.");

			SetupPayload Payload = SetupPayload.Parse(device.ManualCode);
			CommissionableNode[] Nodes = await controller.DiscoverCommissionableAsync(TimeSpan.FromSeconds(10), Payload);

			Assert.IsTrue(Nodes.Length > 0, "Test device not found using DNS-SD.");

			CommissionableNode Found = Nodes[0];

			Assert.AreEqual(device.Discriminator, Found.Discriminator);
			Assert.AreEqual((ushort)0xfff1, Found.VendorId);
			Assert.AreEqual((ushort)0x8000, Found.ProductId);
			Assert.IsTrue(Found.Instance.Port > 0);
		}

		[TestMethod]
		public async Task Test_02_WrongPasscode()
		{
			SetupPayload Payload = SetupPayload.Parse(device.ManualCode);
			uint Wrong = Payload.Passcode == 20202021 ? 20202022U : 20202021U;

			Payload.Passcode = Wrong;

			await Assert.ThrowsExactlyAsync<MatterSecurityException>(async () =>
				await controller.CommissionAsync(Payload, device.LocalEndPoint));
		}

		[TestMethod]
		public async Task Test_03_AttestationFailure()
		{
			using MatterController Untrusting = CreateController(false);

			try
			{
				await Assert.ThrowsExactlyAsync<MatterSecurityException>(async () =>
					await Untrusting.CommissionAsync(device.ManualCode, device.LocalEndPoint));
			}
			finally
			{
				await Untrusting.StopAsync();
			}
		}

		[TestMethod]
		public async Task Test_04_Commission()
		{
			List<string> Progress = new List<string>();

			node = await controller.CommissionAsync(device.ManualCode, null, null, (s) =>
			{
				lock (Progress)
				{
					Progress.Add(s);
				}
			});

			foreach (string s in Progress)
				Console.Out.WriteLine(s);

			Assert.IsNotNull(node);
			Assert.IsTrue(node.IsConnected);
			Assert.IsNotNull(node.CommissioningResult);
			Assert.AreEqual((ushort)0xfff1, node.CommissioningResult.VendorId);
			Assert.AreEqual((ushort)0x8000, node.CommissioningResult.ProductId);
			Assert.IsNotNull(node.CommissioningResult.Attestation);
			Assert.IsTrue(node.CommissioningResult.Attestation.Success);
			Assert.IsTrue(node.CommissioningResult.Attestation.Warnings.Count > 0, "Expected warning about test certification declaration.");
		}

		[TestMethod]
		public async Task Test_05_ReadBasicInformation_1()
		{
			AssertNode();

			ReadResult Result = await node.ReadAttributesAsync(new AttributePath(0, ClusterIds.BasicInformation, null));

			Assert.AreEqual("matter.js", Result.GetValue(0, ClusterIds.BasicInformation, 0x0001)?.AsString());
			Assert.AreEqual((ushort)0xfff1, Result.GetValue(0, ClusterIds.BasicInformation, 0x0002)?.AsUInt16());
			Assert.AreEqual("Waher Test Device", Result.GetValue(0, ClusterIds.BasicInformation, 0x0003)?.AsString());
			Assert.AreEqual("WTD-controller-tests", Result.GetValue(0, ClusterIds.BasicInformation, 0x000f)?.AsString());
		}

		[TestMethod]
		public async Task Test_05_TimeSynchronization_2()
		{
			AssertNode();

			TlvElement UtcTime = await node.ReadAttributeAsync(0, ClusterIds.TimeSynchronization, 0x0000);

			Assert.IsFalse(UtcTime.IsNull, "UTC time not set during commissioning.");

			DateTime Time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)UtcTime.AsUInt64() * 10);
			Console.Out.WriteLine("UTC time on device: " + Time.ToString("O"));

			Assert.IsTrue(Math.Abs((DateTime.UtcNow - Time).TotalMinutes) < 5, "Unexpected UTC time on device.");
		}

		[TestMethod]
		public async Task Test_06_Endpoints()
		{
			AssertNode();

			EndpointInfo[] Endpoints = await node.GetEndpointsAsync();

			Assert.AreEqual(15, Endpoints.Length);
			Assert.IsTrue(Endpoints[0].HasDeviceType(DeviceTypeIds.RootNode));
			Assert.IsTrue(Endpoints[1].HasDeviceType(DeviceTypeIds.ExtendedColorLight));
			Assert.IsTrue(Endpoints[1].ServerClusters.Contains(ClusterIds.OnOff));
			Assert.IsTrue(Endpoints[1].ServerClusters.Contains(ClusterIds.LevelControl));
			Assert.IsTrue(Endpoints[1].ServerClusters.Contains(ClusterIds.ColorControl));
			Assert.IsTrue(Endpoints[2].HasDeviceType(DeviceTypeIds.TemperatureSensor));
			Assert.IsTrue(Endpoints[9].HasDeviceType(DeviceTypeIds.DoorLock));
		}

		[TestMethod]
		public async Task Test_07_InvokeAndRead()
		{
			AssertNode();

			InvokeResult Result = await node.InvokeAsync(1, ClusterIds.OnOff, 0x01);
			Assert.IsTrue(Result.IsSuccess);
			Assert.IsTrue((await node.ReadAttributeAsync(1, ClusterIds.OnOff, 0x0000)).AsBoolean());

			Result = await node.InvokeAsync(1, ClusterIds.OnOff, 0x02);
			Assert.IsTrue(Result.IsSuccess);
			Assert.IsFalse((await node.ReadAttributeAsync(1, ClusterIds.OnOff, 0x0000)).AsBoolean());
		}

		[TestMethod]
		public async Task Test_08_InvokeUnsupportedCommand()
		{
			AssertNode();

			InvokeResult Result = await node.InvokeAsync(2, ClusterIds.OnOff, 0x01);
			Assert.IsFalse(Result.IsSuccess);
			Assert.IsNotNull(Result.Status);
			Console.Out.WriteLine(Result.Status.ToString());
		}

		[TestMethod]
		public async Task Test_09_WriteAttribute()
		{
			AssertNode();

			await node.WriteAttributeAsync(0, ClusterIds.BasicInformation, 0x0005, "Waher Label");
			Assert.AreEqual("Waher Label", (await node.ReadAttributeAsync(0, ClusterIds.BasicInformation, 0x0005)).AsString());

			await node.WriteAttributeAsync(0, ClusterIds.BasicInformation, 0x0005, "Test Device");
			Assert.AreEqual("Test Device", (await node.ReadAttributeAsync(0, ClusterIds.BasicInformation, 0x0005)).AsString());
		}

		[TestMethod]
		public async Task Test_10_WriteInvalidValue()
		{
			AssertNode();

			await Assert.ThrowsExactlyAsync<ImStatusException>(async () =>
				await node.WriteAttributeAsync(0, ClusterIds.BasicInformation, 0x0005, new string('x', 100)));
		}

		[TestMethod]
		public async Task Test_11_TimedInvoke()
		{
			AssertNode();

			InvokeResult Result = await node.InvokeAsync(9, ClusterIds.DoorLock, 0x01, TlvElement.Structure(TlvTag.Anonymous), true);
			Assert.IsTrue(Result.IsSuccess, Result.ToString());
			Assert.AreEqual(2L, (await node.ReadAttributeAsync(9, ClusterIds.DoorLock, 0x0000)).AsInt64());

			Result = await node.InvokeAsync(9, ClusterIds.DoorLock, 0x00, TlvElement.Structure(TlvTag.Anonymous), true);
			Assert.IsTrue(Result.IsSuccess, Result.ToString());
			Assert.AreEqual(1L, (await node.ReadAttributeAsync(9, ClusterIds.DoorLock, 0x0000)).AsInt64());
		}

		[TestMethod]
		public async Task Test_12_UntimedInvokeOfTimedCommand()
		{
			AssertNode();

			try
			{
				InvokeResult Result = await node.InvokeAsync(9, ClusterIds.DoorLock, 0x01, TlvElement.Structure(TlvTag.Anonymous), false);
				Assert.IsFalse(Result.IsSuccess);
				Assert.AreEqual(ImStatusCode.NeedsTimedInteraction, Result.Status.Status);
			}
			catch (ImStatusException ex)
			{
				Assert.AreEqual(ImStatusCode.NeedsTimedInteraction, ex.Status.Status);
			}
		}

		[TestMethod]
		public async Task Test_13_WildcardRead()
		{
			AssertNode();

			ReadResult Result = await node.ReadAttributesAsync(new AttributePath(null, null, null));

			Console.Out.WriteLine(Result.Attributes.Count.ToString() + " attributes read.");
			Assert.IsTrue(Result.Attributes.Count > 300);
			Assert.IsNotNull(Result.GetValue(2, ClusterIds.TemperatureMeasurement, 0x0000));
		}

		[TestMethod]
		public async Task Test_14_ReadEvents()
		{
			AssertNode();

			ReadResult Result = await node.ReadAsync(null, new EventPath[] { new EventPath(0, ClusterIds.BasicInformation, null) });

			Assert.IsTrue(Result.Events.Count > 0, "Expected StartUp event.");

			foreach (EventReport Event in Result.Events)
				Console.Out.WriteLine(Event.ToString());
		}

		[TestMethod]
		public async Task Test_15_Subscription()
		{
			AssertNode();

			using Subscription Subscription = await node.SubscribeAsync(new AttributePath[]
			{
				new AttributePath(2, ClusterIds.TemperatureMeasurement, 0x0000)
			}, null, 0, 10);

			Assert.IsTrue(Subscription.IsActive);
			Assert.AreEqual(2150L, Subscription.PrimingReport.GetValue(2, ClusterIds.TemperatureMeasurement, 0x0000)?.AsInt64());

			TaskCompletionSource<long> Reported = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);

			Subscription.OnReport += (Sender, e) =>
			{
				TlvElement Value = e.GetValue(2, ClusterIds.TemperatureMeasurement, 0x0000);
				if (!(Value is null))
					Reported.TrySetResult(Value.AsInt64());

				return Task.CompletedTask;
			};

			await device.SendCommandAsync("temperature 23.45");

			Assert.AreEqual(Reported.Task, await Task.WhenAny(Reported.Task, Task.Delay(10000)), "Report not received.");
			Assert.AreEqual(2345L, Reported.Task.Result);
		}

		[TestMethod]
		public async Task Test_16_EventSubscription()
		{
			AssertNode();

			using Subscription Subscription = await node.SubscribeAsync(null, new EventPath[]
			{
				new EventPath(13, ClusterIds.Switch, null, true)
			}, 0, 10);

			TaskCompletionSource<EventReport> Reported = new TaskCompletionSource<EventReport>(TaskCreationOptions.RunContinuationsAsynchronously);

			Subscription.OnReport += (Sender, e) =>
			{
				foreach (EventReport Event in e.Events)
				{
					if (Event.Path.EventId == 0x01)    // InitialPress
						Reported.TrySetResult(Event);
				}

				return Task.CompletedTask;
			};

			await device.SendCommandAsync("press");

			Assert.AreEqual(Reported.Task, await Task.WhenAny(Reported.Task, Task.Delay(10000)), "Event not received.");
			Assert.AreEqual(1L, Reported.Task.Result.Data[0]?.AsInt64());
		}

		[TestMethod]
		public async Task Test_17_CaseResumption()
		{
			AssertNode();

			ResumptionRecord Resumption = node.Resumption;
			Assert.IsNotNull(Resumption, "No resumption record available.");

			await node.DisconnectAsync();
			Assert.IsFalse(node.IsConnected);

			SecureSession Session = await node.ConnectAsync();
			Assert.IsTrue(node.IsConnected);
			Assert.IsFalse(Session.IsPase);

			Assert.AreEqual("Test Device", (await node.ReadAttributeAsync(0, ClusterIds.BasicInformation, 0x0005)).AsString());
		}

		[TestMethod]
		public async Task Test_18_OperationalDiscovery()
		{
			AssertNode();

			if (controller.Mdns is null)
				Assert.Inconclusive("DNS-SD not available.");

			OperationalNode Op = await controller.ResolveOperationalAsync(node.NodeId, TimeSpan.FromSeconds(10));

			Assert.IsNotNull(Op, "Operational node not found using DNS-SD.");
			Assert.AreEqual(node.NodeId, Op.NodeId);
			Assert.AreEqual(controller.Fabric.CompressedFabricId, Op.CompressedFabricId);
		}

		[TestMethod]
		public async Task Test_19_MultiAdmin()
		{
			AssertNode();

			SetupPayload Window = await node.OpenCommissioningWindowAsync(TimeSpan.FromMinutes(3));

			Assert.IsTrue(SetupPayload.IsValidPasscode(Window.Passcode));

			using MatterController Controller2 = CreateController();

			try
			{
				MatterNode Node2 = await Controller2.CommissionAsync(Window.ToManualCode(), device.LocalEndPoint);

				Assert.AreEqual("Test Device", (await Node2.ReadAttributeAsync(0, ClusterIds.BasicInformation, 0x0005)).AsString());

				ReadResult Fabrics = await node.ReadAsync(new AttributePath[] { new AttributePath(0, ClusterIds.OperationalCredentials, 0x0001) },
					null, null);
				TlvElement List = Fabrics.GetValue(0, ClusterIds.OperationalCredentials, 0x0001);

				Assert.IsNotNull(List);

				ReadResult All = await (await node.GetClientAsync()).ReadAsync(
					new AttributePath[] { new AttributePath(0, ClusterIds.OperationalCredentials, 0x0001) }, null, null, false, null);

				Assert.AreEqual(2, All.GetValue(0, ClusterIds.OperationalCredentials, 0x0001).Count, "Expected two fabrics.");

				await Node2.RemoveFromFabricAsync();

				All = await (await node.GetClientAsync()).ReadAsync(
					new AttributePath[] { new AttributePath(0, ClusterIds.OperationalCredentials, 0x0001) }, null, null, false, null);

				Assert.AreEqual(1, All.GetValue(0, ClusterIds.OperationalCredentials, 0x0001).Count, "Expected one fabric.");
			}
			finally
			{
				await Controller2.StopAsync();
			}
		}

		[TestMethod]
		public async Task Test_20_RevokeCommissioningWindow()
		{
			AssertNode();

			await node.OpenCommissioningWindowAsync(TimeSpan.FromMinutes(3));
			await node.RevokeCommissioningAsync();

			TlvElement Status = await node.ReadAttributeAsync(0, ClusterIds.AdministratorCommissioning, 0x0000);
			Assert.AreEqual(0L, Status.AsInt64(), "Commissioning window still open.");
		}

		[TestMethod]
		public async Task Test_99_RemoveFromFabric()
		{
			AssertNode();

			await node.RemoveFromFabricAsync();

			Assert.IsFalse(node.IsConnected);

			await Assert.ThrowsAsync<Exception>(async () => await node.ReadAttributeAsync(0, ClusterIds.BasicInformation, 0x0005));

			node = null;
		}
	}
}
