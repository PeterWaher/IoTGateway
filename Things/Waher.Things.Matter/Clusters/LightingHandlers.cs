using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.ControlParameters;
using Waher.Things.Matter.Commands;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.InteractionModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Cluster handler for the Identify cluster.
	/// </summary>
	public class IdentifyHandler : ClusterHandler
	{
		/// <summary>
		/// Cluster handler for the Identify cluster.
		/// </summary>
		public IdentifyHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.Identify ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override bool HasValues(MatterClusterInfo Cluster) => false;

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return Array.Empty<AttributePath>();
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			Commands.Add(new IdentifyCommand(Context));
		}
	}

	/// <summary>
	/// Cluster handler for the On/Off cluster.
	/// </summary>
	public class OnOffHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of on/off state.
		/// </summary>
		public const string FieldName = "On";

		/// <summary>
		/// Cluster handler for the On/Off cluster.
		/// </summary>
		public OnOffHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.OnOff ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out bool On))
				Fields.Boolean(FieldName, On, FieldType.Momentary, true);
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			Parameters.Add(new BooleanControlParameter(FieldName, Context.ClusterName, "On:", "If the device is turned on.",
				async (Node) => AsBoolean(await Context.ReadAttributeAsync(0x0000)),
				async (Node, Value) => await Context.InvokeAsync(Value ? 0x01U : 0x00U)));
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			if (Context.Cluster.HasCommand(0x02))
				Commands.Add(new ClusterCommand(Context, 0x02, "Toggle", null, false));

			if (Context.Cluster.HasCommand(0x01))
				Commands.Add(new ClusterCommand(Context, 0x01, "Turn On", null, false));

			if (Context.Cluster.HasCommand(0x00))
				Commands.Add(new ClusterCommand(Context, 0x00, "Turn Off", null, false));
		}
	}

	/// <summary>
	/// Cluster handler for the Level Control cluster.
	/// </summary>
	public class LevelControlHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of current level.
		/// </summary>
		public const string LevelFieldName = "Level";

		/// <summary>
		/// Field name of current level, in percent.
		/// </summary>
		public const string LevelPercentFieldName = "Level (%)";

		/// <summary>
		/// Cluster handler for the Level Control cluster.
		/// </summary>
		public LevelControlHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.LevelControl ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0002, 0x0003);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out double Level))
			{
				GetRange(Context, Values, out double Min, out double Max);

				Fields.Integer(LevelFieldName, (long)Level, FieldType.Momentary, true);

				if (Max > Min)
					Fields.Quantity(LevelPercentFieldName, Math.Max(0, Math.Min(100, 100.0 * (Level - Min) / (Max - Min))), 1, "%", FieldType.Momentary, true);
			}
		}

		private static void GetRange(ClusterContext Context, ClusterValues Values, out double Min, out double Max)
		{
			if (Values is null || !Values.TryGetKnown(0x0002, out Min))
				Min = AsDouble(Context.GetCachedValue(0x0002)) ?? (Context.Cluster.HasFeature(1) ? 1 : 0);

			if (Values is null || !Values.TryGetKnown(0x0003, out Max))
				Max = AsDouble(Context.GetCachedValue(0x0003)) ?? 254;
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			GetRange(Context, null, out double Min, out double Max);

			Parameters.Add(new Int32ControlParameter(LevelFieldName, Context.ClusterName, "Level:", "Current level of the device.",
				(int)Min, (int)Max,
				async (Node) =>
				{
					double? d = AsDouble(await Context.ReadAttributeAsync(0x0000));
					return d.HasValue ? (int?)d.Value : null;
				},
				async (Node, Value) => await MoveToLevel(Context, Value)));

			Parameters.Add(new DoubleControlParameter(LevelPercentFieldName, Context.ClusterName, "Level (%):", "Current level of the device, in percent.",
				0, 100,
				async (Node) =>
				{
					double? d = AsDouble(await Context.ReadAttributeAsync(0x0000));
					GetRange(Context, null, out double Min2, out double Max2);

					if (d.HasValue && Max2 > Min2)
						return Math.Round(100.0 * (d.Value - Min2) / (Max2 - Min2), 1);
					else
						return null;
				},
				async (Node, Value) =>
				{
					GetRange(Context, null, out double Min2, out double Max2);
					await MoveToLevel(Context, (int)Math.Round(Min2 + Value * (Max2 - Min2) / 100.0));
				}));
		}

		/// <summary>
		/// Moves to a level, turning on or off the device if necessary.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Level">Level.</param>
		public static async System.Threading.Tasks.Task MoveToLevel(ClusterContext Context, int Level)
		{
			if (Level < 0)
				Level = 0;
			else if (Level > 254)
				Level = 254;

			bool WithOnOff = Context.Cluster.HasCommand(0x04);
			byte Options = WithOnOff ? (byte)0 : ColorControlHandler.ExecuteIfOff;

			await Context.InvokeAsync(WithOnOff ? 0x04U : 0x00U, Fields(
				TlvElement.UInt(0, (byte)Level),
				TlvElement.UInt(1, (ushort)0),
				TlvElement.UInt(2, Options),
				TlvElement.UInt(3, Options)));
		}
	}

	/// <summary>
	/// Cluster handler for the Color Control cluster.
	/// </summary>
	public class ColorControlHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of hue.
		/// </summary>
		public const string HueFieldName = "Hue";

		/// <summary>
		/// Field name of saturation.
		/// </summary>
		public const string SaturationFieldName = "Saturation";

		/// <summary>
		/// Field name of color temperature.
		/// </summary>
		public const string ColorTemperatureFieldName = "Color Temperature";

		/// <summary>
		/// Field name of color.
		/// </summary>
		public const string ColorFieldName = "Color";

		/// <summary>
		/// Field name of color mode.
		/// </summary>
		public const string ColorModeFieldName = "Color Mode";

		/// <summary>
		/// Options bit: Execute command even if the device is turned off.
		/// </summary>
		public const byte ExecuteIfOff = 1;

		private const int FeatureHueSaturation = 0;
		private const int FeatureEnhancedHue = 1;
		private const int FeatureXy = 3;
		private const int FeatureColorTemperature = 4;

		/// <summary>
		/// Cluster handler for the Color Control cluster.
		/// </summary>
		public ColorControlHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.ColorControl ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0001, 0x0003, 0x0004, 0x0007, 0x0008, 0x400b, 0x400c);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			bool Hs = Context.Cluster.HasFeature(FeatureHueSaturation);
			bool Xy = Context.Cluster.HasFeature(FeatureXy);
			bool Ct = Context.Cluster.HasFeature(FeatureColorTemperature);

			if (Hs && Values.TryGetReported(0x0000, out double Hue))
				Fields.Quantity(HueFieldName, Hue * 360.0 / 254.0, 1, "°", FieldType.Momentary, true);

			if (Hs && Values.TryGetReported(0x0001, out double Saturation))
				Fields.Quantity(SaturationFieldName, Saturation * 100.0 / 254.0, 1, "%", FieldType.Momentary, true);

			if (Ct && Values.TryGetReported(0x0007, out double Mireds) && Mireds > 0)
				Fields.Quantity(ColorTemperatureFieldName, 1000000.0 / Mireds, 0, "K", FieldType.Momentary, true);

			if (Values.TryGetReported(0x0008, out long Mode))
				Fields.String(ColorModeFieldName, GetEnumName(ClusterIds.ColorControl, "ColorModeEnum", Mode), FieldType.Status);

			if (Values.IsAnyReported(0x0000, 0x0001, 0x0003, 0x0004, 0x0008) && (Hs || Xy))
			{
				ColorReference Color = GetColor(Context, Values);

				if (!(Color is null))
					Fields.String(ColorFieldName, "#" + Color.Red.ToString("x2") + Color.Green.ToString("x2") + Color.Blue.ToString("x2"), FieldType.Momentary, true);
			}
		}

		private static ColorReference GetColor(ClusterContext Context, ClusterValues Values)
		{
			long Mode = AsInt64(Values?.Get(0x0008) ?? Context.GetCachedValue(0x0008)) ?? -1;
			double? H = AsDouble(Values?.Get(0x0000) ?? Context.GetCachedValue(0x0000));
			double? S = AsDouble(Values?.Get(0x0001) ?? Context.GetCachedValue(0x0001));
			double? X = AsDouble(Values?.Get(0x0003) ?? Context.GetCachedValue(0x0003));
			double? Y = AsDouble(Values?.Get(0x0004) ?? Context.GetCachedValue(0x0004));

			if ((Mode == 0 || !X.HasValue) && H.HasValue && S.HasValue)
				return FromHsv(H.Value * 360.0 / 254.0, S.Value / 254.0, 1);
			else if (X.HasValue && Y.HasValue)
				return FromXy(X.Value / 65536.0, Y.Value / 65536.0);
			else
				return null;
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			bool Hs = Context.Cluster.HasFeature(FeatureHueSaturation);
			bool Xy = Context.Cluster.HasFeature(FeatureXy);
			bool Ct = Context.Cluster.HasFeature(FeatureColorTemperature);

			if (Hs || Xy)
			{
				Parameters.Add(new ColorControlParameter(ColorFieldName, Context.ClusterName, "Color:", "Color of the light.",
					async (Node) =>
					{
						await Context.ReadAttributesAsync(0x0000, 0x0001, 0x0003, 0x0004, 0x0008);
						return GetColor(Context, null);
					},
					async (Node, Value) =>
					{
						if (Hs)
						{
							ToHsv(Value, out double Hue, out double Saturation, out double _);

							await Context.InvokeAsync(0x06, Fields(
								TlvElement.UInt(0, (byte)Math.Min(254, Math.Round(Hue * 254.0 / 360.0))),
								TlvElement.UInt(1, (byte)Math.Min(254, Math.Round(Saturation * 254.0))),
								TlvElement.UInt(2, (ushort)0),
								TlvElement.UInt(3, ExecuteIfOff),
								TlvElement.UInt(4, ExecuteIfOff)));
						}
						else
						{
							ToXy(Value, out double x, out double y);

							await Context.InvokeAsync(0x07, Fields(
								TlvElement.UInt(0, (ushort)Math.Min(65279, Math.Round(x * 65536))),
								TlvElement.UInt(1, (ushort)Math.Min(65279, Math.Round(y * 65536))),
								TlvElement.UInt(2, (ushort)0),
								TlvElement.UInt(3, ExecuteIfOff),
								TlvElement.UInt(4, ExecuteIfOff)));
						}
					}));
			}

			if (Hs)
			{
				Parameters.Add(new DoubleControlParameter(HueFieldName, Context.ClusterName, "Hue (°):", "Hue of the light, in degrees.",
					0, 360,
					async (Node) =>
					{
						double? d = AsDouble(await Context.ReadAttributeAsync(0x0000));
						return d.HasValue ? Math.Round(d.Value * 360.0 / 254.0, 1) : (double?)null;
					},
					async (Node, Value) =>
					{
						await Context.InvokeAsync(0x00, Fields(
							TlvElement.UInt(0, (byte)Math.Min(254, Math.Round(Value * 254.0 / 360.0))),
							TlvElement.UInt(1, (byte)0),
							TlvElement.UInt(2, (ushort)0),
							TlvElement.UInt(3, ExecuteIfOff),
							TlvElement.UInt(4, ExecuteIfOff)));
					}));

				Parameters.Add(new DoubleControlParameter(SaturationFieldName, Context.ClusterName, "Saturation (%):", "Saturation of the light, in percent.",
					0, 100,
					async (Node) =>
					{
						double? d = AsDouble(await Context.ReadAttributeAsync(0x0001));
						return d.HasValue ? Math.Round(d.Value * 100.0 / 254.0, 1) : (double?)null;
					},
					async (Node, Value) =>
					{
						await Context.InvokeAsync(0x03, Fields(
							TlvElement.UInt(0, (byte)Math.Min(254, Math.Round(Value * 254.0 / 100.0))),
							TlvElement.UInt(1, (ushort)0),
							TlvElement.UInt(2, ExecuteIfOff),
							TlvElement.UInt(3, ExecuteIfOff)));
					}));
			}

			if (Ct)
			{
				double? MinMireds = AsDouble(Context.GetCachedValue(0x400b));
				double? MaxMireds = AsDouble(Context.GetCachedValue(0x400c));
				double? MinKelvin = MaxMireds.HasValue && MaxMireds.Value > 0 ? Math.Ceiling(1000000.0 / MaxMireds.Value) : (double?)null;
				double? MaxKelvin = MinMireds.HasValue && MinMireds.Value > 0 ? Math.Floor(1000000.0 / MinMireds.Value) : (double?)null;

				Parameters.Add(new DoubleControlParameter(ColorTemperatureFieldName, Context.ClusterName, "Color Temperature (K):",
					"Color temperature of the light, in Kelvin.", MinKelvin, MaxKelvin,
					async (Node) =>
					{
						double? d = AsDouble(await Context.ReadAttributeAsync(0x0007));
						return d.HasValue && d.Value > 0 ? Math.Round(1000000.0 / d.Value) : (double?)null;
					},
					async (Node, Value) =>
					{
						if (Value <= 0)
							throw new ArgumentOutOfRangeException(nameof(Value), "Color temperature must be positive.");

						await Context.InvokeAsync(0x0a, Fields(
							TlvElement.UInt(0, (ushort)Math.Min(65279, Math.Round(1000000.0 / Value))),
							TlvElement.UInt(1, (ushort)0),
							TlvElement.UInt(2, ExecuteIfOff),
							TlvElement.UInt(3, ExecuteIfOff)));
					}));
			}
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			if (Context.Cluster.HasCommand(0x47))
				Commands.Add(new ClusterCommand(Context, 0x47, "Stop Color Change", Fields(TlvElement.UInt(0, ExecuteIfOff), TlvElement.UInt(1, ExecuteIfOff)), false));
		}
	}
}
