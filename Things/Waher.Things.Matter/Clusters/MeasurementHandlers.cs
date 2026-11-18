using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Cluster handler for the Temperature Measurement cluster.
	/// </summary>
	public class TemperatureMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Temperature";

		/// <summary>
		/// Cluster handler for the Temperature Measurement cluster.
		/// </summary>
		public TemperatureMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.TemperatureMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out double Value))
				Fields.Quantity(FieldName, Value * 0.01, 2, "°C");
		}
	}

	/// <summary>
	/// Cluster handler for the Relative Humidity Measurement cluster.
	/// </summary>
	public class RelativeHumidityMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Humidity";

		/// <summary>
		/// Cluster handler for the Relative Humidity Measurement cluster.
		/// </summary>
		public RelativeHumidityMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.RelativeHumidityMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out double Value))
				Fields.Quantity(FieldName, Value * 0.01, 2, "%");
		}
	}

	/// <summary>
	/// Cluster handler for the Pressure Measurement cluster.
	/// </summary>
	public class PressureMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Pressure";

		/// <summary>
		/// Cluster handler for the Pressure Measurement cluster.
		/// </summary>
		public PressureMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.PressureMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0010, 0x0014);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Context.Cluster.HasFeature(0) && Values.TryGetReported(0x0010, out double Scaled) && Values.TryGetKnown(0x0014, out long Scale))
			{
				double Value = Scaled * Math.Pow(10, Scale);
				byte NrDecimals = (byte)Math.Max(0, -Scale);

				Fields.Quantity(FieldName, Value, NrDecimals, "kPa");
			}
			else if (Values.TryGetReported(0x0000, out double Value))
				Fields.Quantity(FieldName, Value * 0.1, 1, "kPa");
		}
	}

	/// <summary>
	/// Cluster handler for the Flow Measurement cluster.
	/// </summary>
	public class FlowMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Flow";

		/// <summary>
		/// Cluster handler for the Flow Measurement cluster.
		/// </summary>
		public FlowMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.FlowMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out double Value))
				Fields.Quantity(FieldName, Value * 0.1, 1, "m³/h");
		}
	}

	/// <summary>
	/// Cluster handler for the Illuminance Measurement cluster.
	/// </summary>
	public class IlluminanceMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Illuminance";

		/// <summary>
		/// Cluster handler for the Illuminance Measurement cluster.
		/// </summary>
		public IlluminanceMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.IlluminanceMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out double Value))
			{
				double Lux = ToLux(Value);
				Fields.Quantity(FieldName, Lux, ValueConverter.GetNrDecimals(Lux), "lx");
			}
		}

		/// <summary>
		/// Converts a measured value to lux.
		/// </summary>
		/// <param name="MeasuredValue">Measured value (10000 × log10(lux) + 1).</param>
		/// <returns>Illuminance, in lux.</returns>
		public static double ToLux(double MeasuredValue)
		{
			if (MeasuredValue <= 0)
				return 0;
			else
				return Math.Pow(10, (MeasuredValue - 1) / 10000.0);
		}

		/// <summary>
		/// Converts illuminance in lux to a measured value.
		/// </summary>
		/// <param name="Lux">Illuminance, in lux.</param>
		/// <returns>Measured value (10000 × log10(lux) + 1).</returns>
		public static ushort FromLux(double Lux)
		{
			if (Lux < 1)
				return 0;
			else
				return (ushort)Math.Min(0xfffe, Math.Round(10000 * Math.Log10(Lux) + 1));
		}
	}

	/// <summary>
	/// Cluster handler for the Occupancy Sensing cluster.
	/// </summary>
	public class OccupancySensingHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Occupancy";

		/// <summary>
		/// Cluster handler for the Occupancy Sensing cluster.
		/// </summary>
		public OccupancySensingHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.OccupancySensing ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out long Value))
				Fields.Boolean(FieldName, (Value & 1) != 0);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => false;
	}

	/// <summary>
	/// Cluster handler for the Boolean State cluster.
	/// </summary>
	public class BooleanStateHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the Boolean State cluster.
		/// </summary>
		public BooleanStateHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.BooleanState ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <summary>
		/// Gets the field name to use for the boolean state, based on the device type of the endpoint.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <returns>Field name.</returns>
		public static string GetFieldName(ClusterContext Context)
		{
			if (Context.HasDeviceType(DeviceTypeIds.ContactSensor))
				return "Contact";
			else if (Context.HasDeviceType(DeviceTypeIds.WaterLeakDetector))
				return "Leak";
			else if (Context.HasDeviceType(DeviceTypeIds.WaterFreezeDetector))
				return "Freeze";
			else if (Context.HasDeviceType(DeviceTypeIds.RainSensor))
				return "Rain";
			else
				return "State";
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out bool Value))
				Fields.Boolean(GetFieldName(Context), Value);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => false;
	}

	/// <summary>
	/// Cluster handler for the Concentration Measurement clusters.
	/// </summary>
	public class ConcentrationMeasurementHandler : ClusterHandler
	{
		private static readonly Dictionary<uint, string> names = new Dictionary<uint, string>()
		{
			{ ClusterIds.CarbonMonoxideConcentrationMeasurement, "CO" },
			{ ClusterIds.CarbonDioxideConcentrationMeasurement, "CO₂" },
			{ ClusterIds.NitrogenDioxideConcentrationMeasurement, "NO₂" },
			{ ClusterIds.OzoneConcentrationMeasurement, "O₃" },
			{ ClusterIds.Pm25ConcentrationMeasurement, "PM2.5" },
			{ ClusterIds.FormaldehydeConcentrationMeasurement, "Formaldehyde" },
			{ ClusterIds.Pm1ConcentrationMeasurement, "PM1" },
			{ ClusterIds.Pm10ConcentrationMeasurement, "PM10" },
			{ ClusterIds.TvocConcentrationMeasurement, "TVOC" },
			{ ClusterIds.RadonConcentrationMeasurement, "Radon" },
			{ ClusterIds.SmokeConcentrationMeasurement, "Smoke" }
		};

		private static readonly string[] units = new string[]
		{
			"ppm", "ppb", "ppt", "mg/m³", "µg/m³", "ng/m³", "1/m³", "Bq/m³"
		};

		private static readonly string[] levels = new string[]
		{
			"Unknown", "Low", "Medium", "High", "Critical"
		};

		/// <summary>
		/// Cluster handler for the Concentration Measurement clusters.
		/// </summary>
		public ConcentrationMeasurementHandler()
		{
		}

		/// <summary>
		/// Gets the base field name for a concentration measurement cluster.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Field name, or null if not a concentration measurement cluster.</returns>
		public static string GetFieldName(uint ClusterId)
		{
			return names.TryGetValue(ClusterId, out string Name) ? Name : null;
		}

		/// <summary>
		/// Gets the unit corresponding to a value of the MeasurementUnitEnum enumeration.
		/// </summary>
		/// <param name="Unit">Enumeration value.</param>
		/// <returns>Unit.</returns>
		public static string GetUnit(long Unit)
		{
			if (Unit >= 0 && Unit < units.Length)
				return units[Unit];
			else
				return string.Empty;
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return names.ContainsKey(ClusterId) ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0003, 0x0005, 0x0008, 0x000a);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			string Name = GetFieldName(Context.ClusterId);
			string Unit = Values.TryGetKnown(0x0008, out long UnitEnum) ? GetUnit(UnitEnum) : string.Empty;

			if (Values.TryGetReported(0x0000, out double Value))
				Fields.Quantity(Name, Value, ValueConverter.GetNrDecimals(Value), Unit);

			if (Values.TryGetReported(0x0003, out Value))
				Fields.Quantity(Name + ", Peak", Value, ValueConverter.GetNrDecimals(Value), Unit, FieldType.Peak);

			if (Values.TryGetReported(0x0005, out Value))
				Fields.Quantity(Name + ", Average", Value, ValueConverter.GetNrDecimals(Value), Unit, FieldType.Computed);

			if (Values.TryGetReported(0x000a, out long Level))
				Fields.String(Name + ", Level", Level >= 0 && Level < levels.Length ? levels[Level] : Level.ToString(), FieldType.Status);
		}
	}

	/// <summary>
	/// Cluster handler for the Air Quality cluster.
	/// </summary>
	public class AirQualityHandler : ClusterHandler
	{
		/// <summary>
		/// Field name.
		/// </summary>
		public const string FieldName = "Air Quality";

		/// <summary>
		/// Cluster handler for the Air Quality cluster.
		/// </summary>
		public AirQualityHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.AirQuality ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out long Value))
				Fields.String(FieldName, GetEnumName(ClusterIds.AirQuality, "AirQualityEnum", Value));
		}
	}

	/// <summary>
	/// Cluster handler for the Power Source cluster.
	/// </summary>
	public class PowerSourceHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the Power Source cluster.
		/// </summary>
		public PowerSourceHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.PowerSource ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0002, 0x0003, 0x0006, 0x000b, 0x000c, 0x000d, 0x000e, 0x000f, 0x001a);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out long Status))
				Fields.String("Power Source Status", GetEnumName(ClusterIds.PowerSource, "PowerSourceStatusEnum", Status), FieldType.Status);

			if (Values.TryGetReported(0x0002, out string Description) && !string.IsNullOrEmpty(Description))
				Fields.String("Power Source", Description, FieldType.Identity);

			if (Values.TryGetReported(0x0003, out double Voltage))
				Fields.Quantity("Input Voltage", Voltage * 0.001, 3, "V");

			if (Values.TryGetReported(0x0006, out double Current))
				Fields.Quantity("Input Current", Current * 0.001, 3, "A");

			if (Values.TryGetReported(0x000b, out double BatVoltage))
				Fields.Quantity("Battery Voltage", BatVoltage * 0.001, 3, "V");

			if (Values.TryGetReported(0x000c, out double BatPercent))
				Fields.Quantity("Battery", BatPercent * 0.5, 1, "%");

			if (Values.TryGetReported(0x000d, out double BatTime))
				Fields.Quantity("Battery Time Remaining", BatTime, 0, "s");

			if (Values.TryGetReported(0x000e, out long ChargeLevel))
				Fields.String("Battery Charge Level", GetEnumName(ClusterIds.PowerSource, "BatChargeLevelEnum", ChargeLevel), FieldType.Status);

			if (Values.TryGetReported(0x000f, out bool Replace))
				Fields.Boolean("Battery Replacement Needed", Replace, FieldType.Status);

			if (Values.TryGetReported(0x001a, out long ChargeState))
				Fields.String("Battery Charge State", GetEnumName(ClusterIds.PowerSource, "BatChargeStateEnum", ChargeState), FieldType.Status);
		}
	}

	/// <summary>
	/// Cluster handler for the Electrical Power Measurement cluster.
	/// </summary>
	public class ElectricalPowerMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the Electrical Power Measurement cluster.
		/// </summary>
		public ElectricalPowerMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.ElectricalPowerMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0004, 0x0005, 0x0006, 0x0007, 0x0008, 0x0009, 0x000a,
				0x000b, 0x000c, 0x000d, 0x000e, 0x0011, 0x0012);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			Add(Values, Fields, 0x0004, "Voltage", 0.001, 3, "V");
			Add(Values, Fields, 0x0005, "Current", 0.001, 3, "A");
			Add(Values, Fields, 0x0006, "Reactive Current", 0.001, 3, "A");
			Add(Values, Fields, 0x0007, "Apparent Current", 0.001, 3, "A");
			Add(Values, Fields, 0x0008, "Power", 0.001, 3, "W");
			Add(Values, Fields, 0x0009, "Reactive Power", 0.001, 3, "var");
			Add(Values, Fields, 0x000a, "Apparent Power", 0.001, 3, "VA");
			Add(Values, Fields, 0x000b, "RMS Voltage", 0.001, 3, "V");
			Add(Values, Fields, 0x000c, "RMS Current", 0.001, 3, "A");
			Add(Values, Fields, 0x000d, "RMS Power", 0.001, 3, "W");
			Add(Values, Fields, 0x000e, "Frequency", 0.001, 3, "Hz");
			Add(Values, Fields, 0x0011, "Power Factor", 0.0001, 4, string.Empty);
			Add(Values, Fields, 0x0012, "Neutral Current", 0.001, 3, "A");
		}

		private static void Add(ClusterValues Values, FieldBuilder Fields, uint AttributeId, string Name, double Scale, byte NrDecimals, string Unit)
		{
			if (Values.TryGetReported(AttributeId, out double Value))
				Fields.Quantity(Name, Value * Scale, NrDecimals, Unit);
		}
	}

	/// <summary>
	/// Cluster handler for the Electrical Energy Measurement cluster.
	/// </summary>
	public class ElectricalEnergyMeasurementHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the Electrical Energy Measurement cluster.
		/// </summary>
		public ElectricalEnergyMeasurementHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.ElectricalEnergyMeasurement ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0001, 0x0002, 0x0003, 0x0004);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			Add(Values, Fields, 0x0001, "Energy Imported", FieldType.Momentary);
			Add(Values, Fields, 0x0002, "Energy Exported", FieldType.Momentary);
			Add(Values, Fields, 0x0003, "Periodic Energy Imported", FieldType.Computed);
			Add(Values, Fields, 0x0004, "Periodic Energy Exported", FieldType.Computed);
		}

		private static void Add(ClusterValues Values, FieldBuilder Fields, uint AttributeId, string Name, FieldType Type)
		{
			if (Values.TryGetReported(AttributeId, out TlvElement Value))
				AddEnergy(Fields, Value, Name, Type);
		}

		private static void AddEnergy(FieldBuilder Fields, TlvElement Value, string Name, FieldType Type)
		{
			if (!(Value is null) && Value.IsStructure)
			{
				double? Energy = AsDouble(Value[0]);

				if (Energy.HasValue)
					Fields.Quantity(Name, Energy.Value * 1e-6, 6, "kWh", Type);
			}
		}

		/// <inheritdoc/>
		public override void AddEventFields(ClusterContext Context, EventReport Event, FieldBuilder Fields)
		{
			TlvElement Data = Event.Data;
			if (Data is null || !Data.IsStructure)
				return;

			switch (Event.Path.EventId)
			{
				case 0x00:  // CumulativeEnergyMeasured
					AddEnergy(Fields, Data[0], "Energy Imported", FieldType.Momentary);
					AddEnergy(Fields, Data[1], "Energy Exported", FieldType.Momentary);
					break;

				case 0x01:  // PeriodicEnergyMeasured
					AddEnergy(Fields, Data[0], "Periodic Energy Imported", FieldType.Computed);
					AddEnergy(Fields, Data[1], "Periodic Energy Exported", FieldType.Computed);
					break;
			}
		}
	}
}
