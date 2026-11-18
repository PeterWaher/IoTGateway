using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.ControlParameters;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Cluster handler for the Thermostat cluster.
	/// </summary>
	public class ThermostatHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of local temperature.
		/// </summary>
		public const string TemperatureFieldName = "Temperature";

		/// <summary>
		/// Field name of heating setpoint.
		/// </summary>
		public const string HeatingSetpointFieldName = "Heating Setpoint";

		/// <summary>
		/// Field name of cooling setpoint.
		/// </summary>
		public const string CoolingSetpointFieldName = "Cooling Setpoint";

		/// <summary>
		/// Field name of system mode.
		/// </summary>
		public const string SystemModeFieldName = "System Mode";

		private const int FeatureHeating = 0;
		private const int FeatureCooling = 1;
		private const int FeatureAutoMode = 5;

		/// <summary>
		/// Cluster handler for the Thermostat cluster.
		/// </summary>
		public ThermostatHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.Thermostat ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0001, 0x0007, 0x0008, 0x0011, 0x0012, 0x0015, 0x0016, 0x0017, 0x0018,
				0x001c, 0x0029);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out double Local))
				Fields.Quantity(TemperatureFieldName, Local * 0.01, 2, "°C");

			if (Values.TryGetReported(0x0001, out double Outdoor))
				Fields.Quantity("Outdoor Temperature", Outdoor * 0.01, 2, "°C");

			if (Values.TryGetReported(0x0007, out double CoolingDemand))
				Fields.Quantity("Cooling Demand", CoolingDemand, 0, "%");

			if (Values.TryGetReported(0x0008, out double HeatingDemand))
				Fields.Quantity("Heating Demand", HeatingDemand, 0, "%");

			if (Values.TryGetReported(0x0011, out double Cooling))
				Fields.Quantity(CoolingSetpointFieldName, Cooling * 0.01, 2, "°C", FieldType.Momentary, true);

			if (Values.TryGetReported(0x0012, out double Heating))
				Fields.Quantity(HeatingSetpointFieldName, Heating * 0.01, 2, "°C", FieldType.Momentary, true);

			if (Values.TryGetReported(0x001c, out long Mode))
				Fields.String(SystemModeFieldName, GetEnumName(ClusterIds.Thermostat, "SystemModeEnum", Mode), FieldType.Status, true);

			if (Values.TryGetReported(0x0029, out long RunningState))
			{
				BitmapDefinition Bitmap = MatterDataModel.FindBitmap(ClusterIds.Thermostat, "RelayStateBitmap");
				string s = Bitmap?.ToString((ulong)RunningState) ?? RunningState.ToString();

				Fields.String("Running State", s == "-" ? "Idle" : s, FieldType.Status);
			}
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			if (Context.Cluster.HasFeature(FeatureHeating) && Context.Cluster.HasAttribute(0x0012))
			{
				double? Min = AsDouble(Context.GetCachedValue(0x0015)) * 0.01;
				double? Max = AsDouble(Context.GetCachedValue(0x0016)) * 0.01;

				Parameters.Add(new DoubleControlParameter(HeatingSetpointFieldName, Context.ClusterName, "Heating setpoint (°C):",
					"Heating setpoint, when occupied.", Min ?? 7, Max ?? 30,
					async (Node) => AsDouble(await Context.ReadAttributeAsync(0x0012)) * 0.01,
					async (Node, Value) => await Context.WriteAttributeAsync(0x0012, (long)Math.Round(Value * 100))));
			}

			if (Context.Cluster.HasFeature(FeatureCooling) && Context.Cluster.HasAttribute(0x0011))
			{
				double? Min = AsDouble(Context.GetCachedValue(0x0017)) * 0.01;
				double? Max = AsDouble(Context.GetCachedValue(0x0018)) * 0.01;

				Parameters.Add(new DoubleControlParameter(CoolingSetpointFieldName, Context.ClusterName, "Cooling setpoint (°C):",
					"Cooling setpoint, when occupied.", Min ?? 16, Max ?? 32,
					async (Node) => AsDouble(await Context.ReadAttributeAsync(0x0011)) * 0.01,
					async (Node, Value) => await Context.WriteAttributeAsync(0x0011, (long)Math.Round(Value * 100))));
			}

			if (Context.Cluster.HasAttribute(0x001c))
			{
				List<string> Options = new List<string>() { "Off" };
				List<string> Labels = new List<string>() { "Off" };
				List<long> Values = new List<long>() { 0 };

				if (Context.Cluster.HasFeature(FeatureAutoMode))
				{
					Options.Add("Auto");
					Labels.Add("Auto");
					Values.Add(1);
				}

				if (Context.Cluster.HasFeature(FeatureCooling))
				{
					Options.Add("Cool");
					Labels.Add("Cool");
					Values.Add(3);
				}

				if (Context.Cluster.HasFeature(FeatureHeating))
				{
					Options.Add("Heat");
					Labels.Add("Heat");
					Values.Add(4);
				}

				Parameters.Add(new StringControlParameter(SystemModeFieldName, Context.ClusterName, "System mode:",
					"Current operating mode of the thermostat.", Options.ToArray(), Labels.ToArray(),
					async (Node) =>
					{
						long? Mode = AsInt64(await Context.ReadAttributeAsync(0x001c));
						if (!Mode.HasValue)
							return null;

						int i = Values.IndexOf(Mode.Value);
						return i >= 0 ? Options[i] : GetEnumName(ClusterIds.Thermostat, "SystemModeEnum", Mode.Value);
					},
					async (Node, Value) =>
					{
						int i = Options.IndexOf(Value);
						if (i < 0)
							throw new ArgumentException("Invalid system mode.", nameof(Value));

						await Context.WriteAttributeAsync(0x001c, (ulong)Values[i]);
					}));
			}
		}
	}

	/// <summary>
	/// Cluster handler for the Fan Control cluster.
	/// </summary>
	public class FanControlHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of fan mode.
		/// </summary>
		public const string FanModeFieldName = "Fan Mode";

		/// <summary>
		/// Field name of fan speed setting, in percent.
		/// </summary>
		public const string SpeedSettingFieldName = "Fan Speed Setting";

		/// <summary>
		/// Field name of current fan speed, in percent.
		/// </summary>
		public const string SpeedFieldName = "Fan Speed";

		private static readonly string[] modeNames = new string[] { "Off", "Low", "Medium", "High", "On", "Auto", "Smart" };

		private static readonly int[][] sequences = new int[][]
		{
			new int[] { 0, 1, 2, 3 },
			new int[] { 0, 1, 3 },
			new int[] { 0, 1, 2, 3, 5 },
			new int[] { 0, 1, 3, 5 },
			new int[] { 0, 3, 5 },
			new int[] { 0, 3 }
		};

		/// <summary>
		/// Cluster handler for the Fan Control cluster.
		/// </summary>
		public FanControlHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.FanControl ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0001, 0x0002, 0x0003, 0x0005, 0x0006);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out long Mode))
				Fields.String(FanModeFieldName, Mode >= 0 && Mode < modeNames.Length ? modeNames[Mode] : Mode.ToString(), FieldType.Momentary, true);

			if (Values.TryGetReported(0x0002, out double Setting))
				Fields.Quantity(SpeedSettingFieldName, Setting, 0, "%", FieldType.Momentary, true);

			if (Values.TryGetReported(0x0003, out double Current))
				Fields.Quantity(SpeedFieldName, Current, 0, "%");

			if (Values.TryGetReported(0x0006, out long Speed))
				Fields.Integer("Speed", Speed);
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			long Sequence = AsInt64(Context.GetCachedValue(0x0001)) ?? 2;
			int[] Modes = Sequence >= 0 && Sequence < sequences.Length ? sequences[Sequence] : sequences[2];
			string[] Options = new string[Modes.Length];
			int i;

			for (i = 0; i < Modes.Length; i++)
				Options[i] = modeNames[Modes[i]];

			Parameters.Add(new StringControlParameter(FanModeFieldName, Context.ClusterName, "Fan mode:", "Mode of the fan.", Options,
				async (Node) =>
				{
					long? Mode = AsInt64(await Context.ReadAttributeAsync(0x0000));
					if (!Mode.HasValue)
						return null;

					return Mode.Value >= 0 && Mode.Value < modeNames.Length ? modeNames[Mode.Value] : Mode.Value.ToString();
				},
				async (Node, Value) =>
				{
					int Mode = Array.IndexOf(modeNames, Value);
					if (Mode < 0)
						throw new ArgumentException("Invalid fan mode.", nameof(Value));

					await Context.WriteAttributeAsync(0x0000, (ulong)Mode);
				}));

			if (Context.Cluster.HasAttribute(0x0002))
			{
				Parameters.Add(new Int32ControlParameter(SpeedSettingFieldName, Context.ClusterName, "Fan speed (%):",
					"Speed setting of the fan, in percent.", 0, 100,
					async (Node) =>
					{
						long? Setting = AsInt64(await Context.ReadAttributeAsync(0x0002));
						return Setting.HasValue ? (int?)Setting.Value : null;
					},
					async (Node, Value) => await Context.WriteAttributeAsync(0x0002, (ulong)Value)));
			}
		}
	}

	/// <summary>
	/// Cluster handler for the Window Covering cluster.
	/// </summary>
	public class WindowCoveringHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of lift position.
		/// </summary>
		public const string LiftFieldName = "Lift Position";

		/// <summary>
		/// Field name of tilt position.
		/// </summary>
		public const string TiltFieldName = "Tilt Position";

		private const int FeatureLift = 0;
		private const int FeatureTilt = 1;
		private const int FeaturePositionAwareLift = 2;
		private const int FeaturePositionAwareTilt = 4;

		/// <summary>
		/// Cluster handler for the Window Covering cluster.
		/// </summary>
		public WindowCoveringHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.WindowCovering ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x000a, 0x000b, 0x000c, 0x000e, 0x000f);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x000e, out double Lift))
				Fields.Quantity(LiftFieldName, Lift * 0.01, 2, "%", FieldType.Momentary, true);

			if (Values.TryGetReported(0x000f, out double Tilt))
				Fields.Quantity(TiltFieldName, Tilt * 0.01, 2, "%", FieldType.Momentary, true);

			if (Values.TryGetReported(0x000b, out double TargetLift))
				Fields.Quantity("Target Lift Position", TargetLift * 0.01, 2, "%", FieldType.Status);

			if (Values.TryGetReported(0x000c, out double TargetTilt))
				Fields.Quantity("Target Tilt Position", TargetTilt * 0.01, 2, "%", FieldType.Status);

			if (Values.TryGetReported(0x000a, out long Status))
			{
				string s;

				switch (Status & 3)
				{
					case 1: s = "Opening"; break;
					case 2: s = "Closing"; break;
					default: s = "Stopped"; break;
				}

				Fields.String("Operational Status", s, FieldType.Status);
			}
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster)
		{
			return Cluster.HasFeature(FeaturePositionAwareLift) || Cluster.HasFeature(FeaturePositionAwareTilt);
		}

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			if (Context.Cluster.HasFeature(FeatureLift) && Context.Cluster.HasFeature(FeaturePositionAwareLift))
			{
				Parameters.Add(new DoubleControlParameter(LiftFieldName, Context.ClusterName, "Lift position (%):",
					"Lift position of the window covering, in percent. 0% = fully open, 100% = fully closed.", 0, 100,
					async (Node) => AsDouble(await Context.ReadAttributeAsync(0x000e)) * 0.01,
					async (Node, Value) => await Context.InvokeAsync(0x05, Fields(TlvElement.UInt(0, (ushort)Math.Round(Value * 100))))));
			}

			if (Context.Cluster.HasFeature(FeatureTilt) && Context.Cluster.HasFeature(FeaturePositionAwareTilt))
			{
				Parameters.Add(new DoubleControlParameter(TiltFieldName, Context.ClusterName, "Tilt position (%):",
					"Tilt position of the window covering, in percent. 0% = fully open, 100% = fully closed.", 0, 100,
					async (Node) => AsDouble(await Context.ReadAttributeAsync(0x000f)) * 0.01,
					async (Node, Value) => await Context.InvokeAsync(0x08, Fields(TlvElement.UInt(0, (ushort)Math.Round(Value * 100))))));
			}
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			Commands.Add(new Commands.ClusterCommand(Context, 0x00, "Open", null, false));
			Commands.Add(new Commands.ClusterCommand(Context, 0x01, "Close", null, false));
			Commands.Add(new Commands.ClusterCommand(Context, 0x02, "Stop", null, false));
		}
	}
}
