using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Things.Matter.Clusters;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the data model metadata, value conversions and cluster handler selection.
	/// </summary>
	[TestClass]
	public sealed class DataModelTests
	{
		[TestMethod]
		public void Test_01_Clusters()
		{
			ClusterDefinition[] Clusters = MatterDataModel.GetClusters();
			Assert.IsTrue(Clusters.Length > 100, "Too few clusters defined.");

			Assert.AreEqual("On/Off", MatterDataModel.GetClusterName(ClusterIds.OnOff));
			Assert.AreEqual("Temperature Measurement", MatterDataModel.GetClusterName(ClusterIds.TemperatureMeasurement));
			Assert.AreEqual("Cluster 0xFC01", MatterDataModel.GetClusterName(0xfc01));
		}

		[TestMethod]
		public void Test_02_Attributes()
		{
			AttributeDefinition OnOff = MatterDataModel.GetAttribute(ClusterIds.OnOff, 0x0000);
			Assert.IsNotNull(OnOff);
			Assert.AreEqual("OnOff", OnOff.Name);
			Assert.AreEqual("boolean", OnOff.Type);
			Assert.IsFalse(OnOff.Writable);

			AttributeDefinition Setpoint = MatterDataModel.GetAttribute(ClusterIds.Thermostat, 0x0012);
			Assert.IsNotNull(Setpoint);
			Assert.AreEqual("OccupiedHeatingSetpoint", Setpoint.Name);
			Assert.AreEqual("temperature", Setpoint.Type);
			Assert.IsTrue(Setpoint.Writable);

			AttributeDefinition FeatureMap = MatterDataModel.GetAttribute(ClusterIds.OnOff, GlobalAttributeIds.FeatureMap);
			Assert.IsNotNull(FeatureMap);
			Assert.AreEqual("FeatureMap", FeatureMap.Name);

			AttributeDefinition Parts = MatterDataModel.GetAttribute(ClusterIds.Descriptor, 0x0003);
			Assert.IsTrue(Parts.IsList);
		}

		[TestMethod]
		public void Test_03_Commands()
		{
			ClusterDefinition DoorLock = MatterDataModel.GetCluster(ClusterIds.DoorLock);
			CommandDefinition Lock = DoorLock.GetCommand(0x00);

			Assert.AreEqual("LockDoor", Lock.Name);
			Assert.IsTrue(Lock.Timed);
			Assert.IsTrue(Lock.HasNoMandatoryFields);

			CommandDefinition MoveToLevel = MatterDataModel.GetCluster(ClusterIds.LevelControl).GetCommand(0x00);
			Assert.AreEqual("MoveToLevel", MoveToLevel.Name);
			Assert.IsFalse(MoveToLevel.HasNoMandatoryFields);
			Assert.AreEqual("Toggle", MatterDataModel.GetCommandName(ClusterIds.OnOff, 0x02));
		}

		[TestMethod]
		public void Test_04_Events()
		{
			Assert.AreEqual("InitialPress", MatterDataModel.GetEventName(ClusterIds.Switch, 0x01));
			Assert.AreEqual("StartUp", MatterDataModel.GetEventName(ClusterIds.BasicInformation, 0x00));
		}

		[TestMethod]
		public void Test_05_DeviceTypes()
		{
			Assert.AreEqual("On/Off Light", MatterDataModel.GetDeviceTypeName(DeviceTypeIds.OnOffLight));
			Assert.AreEqual("Temperature Sensor", MatterDataModel.GetDeviceTypeName(DeviceTypeIds.TemperatureSensor));
			Assert.IsTrue(MatterDataModel.GetDeviceType(DeviceTypeIds.OnOffLight).IsApplication);
			Assert.IsFalse(MatterDataModel.GetDeviceType(DeviceTypeIds.RootNode).IsApplication);
		}

		[TestMethod]
		public void Test_06_Enums()
		{
			EnumDefinition SystemMode = MatterDataModel.FindEnum(ClusterIds.Thermostat, "SystemModeEnum");
			Assert.IsNotNull(SystemMode);
			Assert.IsTrue(SystemMode.TryGetName(4, out string Name));
			Assert.AreEqual("Heat", Name);
			Assert.IsTrue(SystemMode.TryGetValue("Cool", out long Value));
			Assert.AreEqual(3L, Value);
		}

		[TestMethod]
		public void Test_07_Bitmaps()
		{
			BitmapDefinition Running = MatterDataModel.FindBitmap(ClusterIds.Thermostat, "RelayStateBitmap");
			Assert.IsNotNull(Running);
			Assert.AreEqual("Heat, Fan", Running.ToString(0x0005));
			Assert.AreEqual("-", Running.ToString(0));
		}

		[TestMethod]
		public void Test_08_FormatValues()
		{
			Assert.AreEqual("21.50 °C", MatterDataModel.FormatAttributeValue(ClusterIds.TemperatureMeasurement, 0x0000, TlvElement.Int(2, 2150)));
			Assert.AreEqual("Heat", MatterDataModel.FormatAttributeValue(ClusterIds.Thermostat, 0x001c, TlvElement.UInt(2, 4)));
			Assert.AreEqual("null", MatterDataModel.FormatAttributeValue(ClusterIds.TemperatureMeasurement, 0x0000, TlvElement.Null(2)));

			TlvElement DeviceTypes = TlvElement.Array(2,
				TlvElement.Structure(TlvTag.Anonymous, TlvElement.UInt(0, 0x0100U), TlvElement.UInt(1, 3U)));

			Assert.AreEqual("[{DeviceType: On/Off Light, Revision: 3}]",
				MatterDataModel.FormatAttributeValue(ClusterIds.Descriptor, 0x0000, DeviceTypes));
		}

		[TestMethod]
		public void Test_09_SpaceNames()
		{
			Assert.AreEqual("Measured Value", MatterDataModel.SpaceName("MeasuredValue"));
			Assert.AreEqual("CO State", MatterDataModel.SpaceName("COState"));
			Assert.AreEqual("PM2.5", MatterDataModel.SpaceName("PM2.5"));
			Assert.AreEqual("Initial Press", MatterDataModel.SpaceName("InitialPress"));
			Assert.AreEqual("Bat Percent Remaining", MatterDataModel.SpaceName("BatPercentRemaining"));
		}

		[TestMethod]
		public void Test_10_ValueKinds()
		{
			Assert.AreEqual(ValueKind.Boolean, ValueConverter.Classify(ClusterIds.OnOff, "boolean"));
			Assert.AreEqual(ValueKind.UnsignedInteger, ValueConverter.Classify(ClusterIds.OnOff, "int16u"));
			Assert.AreEqual(ValueKind.SignedInteger, ValueConverter.Classify(ClusterIds.OnOff, "int8s"));
			Assert.AreEqual(ValueKind.Temperature, ValueConverter.Classify(ClusterIds.Thermostat, "temperature"));
			Assert.AreEqual(ValueKind.Enum, ValueConverter.Classify(ClusterIds.Thermostat, "SystemModeEnum"));
			Assert.AreEqual(ValueKind.Bitmap, ValueConverter.Classify(ClusterIds.Thermostat, "RelayStateBitmap"));
			Assert.AreEqual(ValueKind.Struct, ValueConverter.Classify(ClusterIds.Descriptor, "DeviceTypeStruct"));
			Assert.AreEqual(ValueKind.Unknown, ValueConverter.Classify(ClusterIds.OnOff, "no-such-type"));

			Assert.IsInstanceOfType<ulong>(ValueConverter.ToTlvValue(ValueKind.Enum, 3));
			Assert.IsInstanceOfType<long>(ValueConverter.ToTlvValue(ValueKind.Temperature, 21.5));
			Assert.AreEqual(2150L, ValueConverter.ToTlvValue(ValueKind.Temperature, 21.5));
			Assert.IsInstanceOfType<float>(ValueConverter.ToTlvValue(ValueKind.Single, 1.5));
		}

		[TestMethod]
		public void Test_11_GenericFields()
		{
			ThingReference Thing = new ThingReference("Test");
			FieldBuilder Fields = new FieldBuilder(Thing, DateTime.UtcNow, null, new FieldNameRegistry());

			Assert.IsTrue(ValueConverter.AddField(Fields, ClusterIds.Thermostat, "Setpoint", "temperature", TlvElement.Int(2, 2150), FieldType.Momentary, true));
			Assert.IsTrue(ValueConverter.AddField(Fields, ClusterIds.Thermostat, "Mode", "SystemModeEnum", TlvElement.UInt(2, 4), FieldType.Momentary, true));
			Assert.IsTrue(ValueConverter.AddField(Fields, ClusterIds.OnOff, "On", "boolean", TlvElement.Boolean(2, true), FieldType.Momentary, false));
			Assert.IsTrue(ValueConverter.AddField(Fields, ClusterIds.PowerSource, "Voltage", "voltage_mv", TlvElement.Int(2, 3300), FieldType.Momentary, false));
			Assert.IsFalse(ValueConverter.AddField(Fields, ClusterIds.OnOff, "Null", "boolean", TlvElement.Null(2), FieldType.Momentary, false));

			Assert.AreEqual(4, Fields.Count);

			QuantityField Setpoint = (QuantityField)Fields.Fields[0];
			Assert.AreEqual(21.5, Setpoint.Value);
			Assert.AreEqual("°C", Setpoint.Unit);
			Assert.IsTrue(Setpoint.Writable);

			Assert.AreEqual("Heat", Fields.Fields[1].ValueString);
			Assert.IsTrue(((BooleanField)Fields.Fields[2]).Value);

			QuantityField Voltage = (QuantityField)Fields.Fields[3];
			Assert.AreEqual(3.3, Voltage.Value, 1e-9);
			Assert.AreEqual("V", Voltage.Unit);
		}

		[TestMethod]
		public void Test_12_FieldNameRegistry()
		{
			FieldNameRegistry Names = new FieldNameRegistry();

			Names.RegisterFixed("Temperature");

			Assert.AreEqual("Measured Value", Names.GetName(0x0001, 0x0000, "Measured Value", "Cluster A"));
			Assert.AreEqual("Cluster B, Measured Value", Names.GetName(0x0002, 0x0000, "Measured Value", "Cluster B"));
			Assert.AreEqual("Measured Value", Names.GetName(0x0001, 0x0000, "Measured Value", "Cluster A"));
			Assert.AreEqual("Cluster C, Temperature", Names.GetName(0x0003, 0x0000, "Temperature", "Cluster C"));
		}

		[TestMethod]
		public void Test_13_ClusterHandlers()
		{
			Assert.IsInstanceOfType<OnOffHandler>(ClusterHandlers.Get(ClusterIds.OnOff));
			Assert.IsInstanceOfType<TemperatureMeasurementHandler>(ClusterHandlers.Get(ClusterIds.TemperatureMeasurement));
			Assert.IsInstanceOfType<ConcentrationMeasurementHandler>(ClusterHandlers.Get(ClusterIds.CarbonDioxideConcentrationMeasurement));
			Assert.IsInstanceOfType<ModeHandler>(ClusterHandlers.Get(ClusterIds.RvcRunMode));
			Assert.IsInstanceOfType<InfrastructureClusterHandler>(ClusterHandlers.Get(ClusterIds.OperationalCredentials));
			Assert.IsInstanceOfType<GenericClusterHandler>(ClusterHandlers.Get(ClusterIds.SmokeCoAlarm));
			Assert.IsInstanceOfType<GenericClusterHandler>(ClusterHandlers.Get(0xfc01));
		}

		[TestMethod]
		public void Test_14_GenericControllable()
		{
			ClusterDefinition OnOff = MatterDataModel.GetCluster(ClusterIds.OnOff);

			Assert.IsTrue(ClusterHandler.IsGenericControllable(ClusterIds.OnOff, OnOff.GetAttribute(0x4001)));     // OnTime
			Assert.IsTrue(ClusterHandler.IsGenericControllable(ClusterIds.OnOff, OnOff.GetAttribute(0x4003)));     // StartUpOnOff
			Assert.IsFalse(ClusterHandler.IsGenericControllable(ClusterIds.OnOff, OnOff.GetAttribute(0x0000)));    // OnOff (read-only)
			Assert.IsFalse(ClusterHandler.IsGenericControllable(ClusterIds.OnOff, OnOff.GetAttribute(GlobalAttributeIds.FeatureMap)));
		}

		[TestMethod]
		public void Test_15_ColorConversions()
		{
			ColorReference Red = ClusterHandler.FromHsv(0, 1, 1);
			Assert.AreEqual(255, Red.Red);
			Assert.AreEqual(0, Red.Green);
			Assert.AreEqual(0, Red.Blue);

			ClusterHandler.ToHsv(new ColorReference(0, 0, 255), out double Hue, out double Saturation, out double Value);
			Assert.AreEqual(240, Hue, 0.001);
			Assert.AreEqual(1, Saturation, 0.001);
			Assert.AreEqual(1, Value, 0.001);

			ClusterHandler.ToXy(new ColorReference(255, 255, 255), out double x, out double y);
			Assert.AreEqual(0.3127, x, 0.001);
			Assert.AreEqual(0.3290, y, 0.001);

			ColorReference White = ClusterHandler.FromXy(0.3127, 0.3290);
			Assert.IsTrue(White.Red > 240 && White.Green > 240 && White.Blue > 240);
		}

		[TestMethod]
		public void Test_16_Illuminance()
		{
			Assert.AreEqual(1000, IlluminanceMeasurementHandler.ToLux(30001), 0.001);
			Assert.AreEqual(0, IlluminanceMeasurementHandler.ToLux(0));
			Assert.AreEqual(30001, IlluminanceMeasurementHandler.FromLux(1000));
		}
	}
}
