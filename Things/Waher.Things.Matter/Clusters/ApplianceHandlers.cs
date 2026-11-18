using System;
using System.Collections.Generic;
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
	/// Cluster handler for the Door Lock cluster.
	/// </summary>
	public class DoorLockHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of lock state.
		/// </summary>
		public const string LockedFieldName = "Locked";

		/// <summary>
		/// Cluster handler for the Door Lock cluster.
		/// </summary>
		public DoorLockHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.DoorLock ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0001, 0x0002, 0x0003, 0x0025);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0000, out long State))
			{
				Fields.Boolean(LockedFieldName, State == 1, FieldType.Momentary, true);
				Fields.String("Lock State", GetEnumName(ClusterIds.DoorLock, "DlLockState", State), FieldType.Status);
			}

			if (Values.TryGetReported(0x0001, out long LockType))
				Fields.String("Lock Type", GetEnumName(ClusterIds.DoorLock, "DlLockType", LockType), FieldType.Identity);

			if (Values.TryGetReported(0x0002, out bool Enabled))
				Fields.Boolean("Actuator Enabled", Enabled, FieldType.Status);

			if (Values.TryGetReported(0x0003, out long DoorState))
				Fields.String("Door State", GetEnumName(ClusterIds.DoorLock, "DoorStateEnum", DoorState), FieldType.Momentary);

			if (Values.TryGetReported(0x0025, out long OperatingMode))
				Fields.String("Operating Mode", GetEnumName(ClusterIds.DoorLock, "OperatingModeEnum", OperatingMode), FieldType.Status);
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			Parameters.Add(new BooleanControlParameter(LockedFieldName, Context.ClusterName, "Locked:", "If the door is locked.",
				async (Node) =>
				{
					long? State = AsInt64(await Context.ReadAttributeAsync(0x0000));
					return State.HasValue ? State.Value == 1 : (bool?)null;
				},
				async (Node, Value) => await Context.InvokeAsync(Value ? 0x00U : 0x01U, GetPinFields(Context), true)));
		}

		private static TlvElement GetPinFields(ClusterContext Context)
		{
			string Pin = Context.Endpoint?.PinCode;

			if (string.IsNullOrEmpty(Pin))
				return Fields();
			else
				return Fields(TlvElement.Bytes(0, System.Text.Encoding.ASCII.GetBytes(Pin)));
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			Commands.Add(new ClusterCommand(Context, 0x00, "Lock", GetPinFields(Context), true));
			Commands.Add(new ClusterCommand(Context, 0x01, "Unlock", GetPinFields(Context), true));
		}
	}

	/// <summary>
	/// Cluster handler for the Switch cluster (generic switches, buttons).
	/// </summary>
	public class SwitchHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of current position.
		/// </summary>
		public const string PositionFieldName = "Position";

		/// <summary>
		/// Field name of switch events.
		/// </summary>
		public const string EventFieldName = "Switch Event";

		/// <summary>
		/// Cluster handler for the Switch cluster (generic switches, buttons).
		/// </summary>
		public SwitchHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.Switch ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0000, 0x0001);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0001, out long Position))
				Fields.Integer(PositionFieldName, Position);

			if (Values.TryGetReported(0x0000, out long NrPositions))
				Fields.Integer("Number of Positions", NrPositions, FieldType.Identity);
		}

		/// <inheritdoc/>
		public override void AddEventFields(ClusterContext Context, EventReport Event, FieldBuilder Fields)
		{
			if (!Event.Path.EventId.HasValue)
				return;

			string Name = MatterDataModel.SpaceName(Context.Definition?.GetEvent(Event.Path.EventId.Value)?.Name) ??
				("Event " + Event.Path.EventId.Value.ToString());

			Fields.String(EventFieldName, Name);

			TlvElement Data = Event.Data;
			if (Data is null || !Data.IsStructure)
				return;

			switch (Event.Path.EventId.Value)
			{
				case 0x00:  // SwitchLatched
				case 0x01:  // InitialPress
				case 0x02:  // LongPress
				case 0x05:  // MultiPressOngoing
					long? Position = AsInt64(Data[0]);
					if (Position.HasValue)
						Fields.Integer(PositionFieldName, Position.Value);

					if (Event.Path.EventId.Value == 0x05)
					{
						long? Count = AsInt64(Data[1]);
						if (Count.HasValue)
							Fields.Integer("Presses", Count.Value);
					}
					break;

				case 0x06:  // MultiPressComplete
					long? Total = AsInt64(Data[1]);
					if (Total.HasValue)
						Fields.Integer("Presses", Total.Value);
					break;
			}
		}
	}

	/// <summary>
	/// Cluster handler for the Mode Select cluster, and clusters derived from the Mode Base cluster.
	/// </summary>
	public class ModeHandler : ClusterHandler
	{
		private static readonly uint[] modeBaseClusters = new uint[]
		{
			ClusterIds.OvenMode,
			ClusterIds.LaundryWasherMode,
			ClusterIds.RefrigeratorAndTemperatureControlledCabinetMode,
			ClusterIds.RvcRunMode,
			ClusterIds.RvcCleanMode,
			ClusterIds.DishwasherMode,
			ClusterIds.MicrowaveOvenMode,
			ClusterIds.EnergyEvseMode,
			ClusterIds.WaterHeaterMode,
			ClusterIds.DeviceEnergyManagementMode
		};

		/// <summary>
		/// Cluster handler for the Mode Select cluster, and clusters derived from the Mode Base cluster.
		/// </summary>
		public ModeHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			if (ClusterId == ClusterIds.ModeSelect || Array.IndexOf(modeBaseClusters, ClusterId) >= 0)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		private static void GetAttributeIds(uint ClusterId, out uint SupportedModes, out uint CurrentMode)
		{
			if (ClusterId == ClusterIds.ModeSelect)
			{
				SupportedModes = 0x0002;
				CurrentMode = 0x0003;
			}
			else
			{
				SupportedModes = 0x0000;
				CurrentMode = 0x0001;
			}
		}

		/// <summary>
		/// Gets the field name used for the current mode.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <returns>Field name.</returns>
		public static string GetFieldName(ClusterContext Context)
		{
			return Context.ClusterId == ClusterIds.ModeSelect ? "Mode" : Context.ClusterName;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			GetAttributeIds(Cluster.ClusterId, out uint SupportedModes, out uint CurrentMode);
			return GetPaths(EndpointId, Cluster, SupportedModes, CurrentMode);
		}

		private static Dictionary<long, string> GetModes(ClusterContext Context, TlvElement SupportedModes)
		{
			Dictionary<long, string> Result = new Dictionary<long, string>();

			if (!(SupportedModes is null) && (SupportedModes.IsArray || SupportedModes.IsList))
			{
				foreach (TlvElement Option in SupportedModes.Children)
				{
					string Label = Option[0]?.AsString();
					long? Mode = AsInt64(Option[1]);

					if (Mode.HasValue && !string.IsNullOrEmpty(Label))
						Result[Mode.Value] = Label;
				}
			}

			return Result;
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			GetAttributeIds(Context.ClusterId, out uint SupportedModesId, out uint CurrentModeId);

			if (Values.TryGetReported(CurrentModeId, out long Mode))
			{
				Dictionary<long, string> Modes = GetModes(Context, Values.Get(SupportedModesId));

				if (Modes.TryGetValue(Mode, out string Label))
					Fields.String(GetFieldName(Context), Label, FieldType.Momentary, true);
				else
					Fields.String(GetFieldName(Context), Mode.ToString(), FieldType.Momentary, true);
			}
		}

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			GetAttributeIds(Context.ClusterId, out uint SupportedModesId, out uint CurrentModeId);

			Dictionary<long, string> Modes = GetModes(Context, Context.GetCachedValue(SupportedModesId));
			List<string> Options = new List<string>();

			foreach (string Label in Modes.Values)
				Options.Add(Label);

			if (Options.Count == 0)
				return;

			Parameters.Add(new StringControlParameter(GetFieldName(Context), Context.ClusterName, "Mode:", "Current mode.",
				Options.ToArray(),
				async (Node) =>
				{
					long? Mode = AsInt64(await Context.ReadAttributeAsync(CurrentModeId));
					if (!Mode.HasValue)
						return null;

					return Modes.TryGetValue(Mode.Value, out string Label) ? Label : Mode.Value.ToString();
				},
				async (Node, Value) =>
				{
					foreach (KeyValuePair<long, string> P in Modes)
					{
						if (P.Value == Value)
						{
							InvokeResult Result = await Context.InvokeAsync(0x00, Fields(TlvElement.UInt(0, (byte)P.Key)));

							if (Context.ClusterId != ClusterIds.ModeSelect && !(Result.Fields is null))
							{
								long? Status = AsInt64(Result.Fields[0]);
								if (Status.HasValue && Status.Value != 0)
								{
									string StatusText = Result.Fields[1]?.AsString();
									throw new Exception("Unable to change mode. Status: " + Status.Value.ToString() +
										(string.IsNullOrEmpty(StatusText) ? string.Empty : " (" + StatusText + ")"));
								}
							}

							return;
						}
					}

					throw new ArgumentException("Invalid mode.", nameof(Value));
				}));
		}
	}

	/// <summary>
	/// Cluster handler for the Valve Configuration and Control cluster.
	/// </summary>
	public class ValveHandler : ClusterHandler
	{
		/// <summary>
		/// Field name of valve state.
		/// </summary>
		public const string OpenFieldName = "Open";

		/// <summary>
		/// Cluster handler for the Valve Configuration and Control cluster.
		/// </summary>
		public ValveHandler()
		{
		}

		/// <inheritdoc/>
		public override Grade Supports(uint ClusterId)
		{
			return ClusterId == ClusterIds.ValveConfigurationAndControl ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <inheritdoc/>
		public override AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return GetPaths(EndpointId, Cluster, 0x0003, 0x0004, 0x0005, 0x0006, 0x0009);
		}

		/// <inheritdoc/>
		public override void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields)
		{
			if (Values.TryGetReported(0x0004, out long State))
			{
				Fields.Boolean(OpenFieldName, State == 1, FieldType.Momentary, true);
				Fields.String("Valve State", GetEnumName(ClusterIds.ValveConfigurationAndControl, "ValveStateEnum", State), FieldType.Status);
			}

			if (Values.TryGetReported(0x0006, out double Level))
				Fields.Quantity("Valve Level", Level, 0, "%");

			if (Values.TryGetReported(0x0003, out double Remaining))
				Fields.Quantity("Remaining Duration", Remaining, 0, "s", FieldType.Status);
		}

		/// <inheritdoc/>
		public override bool HasEvents(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override bool IsControllable(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public override void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
			Parameters.Add(new BooleanControlParameter(OpenFieldName, Context.ClusterName, "Open:", "If the valve is open.",
				async (Node) =>
				{
					long? State = AsInt64(await Context.ReadAttributeAsync(0x0004));
					return State.HasValue ? State.Value == 1 : (bool?)null;
				},
				async (Node, Value) => await Context.InvokeAsync(Value ? 0x00U : 0x01U, Fields())));
		}

		/// <inheritdoc/>
		public override void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
			Commands.Add(new ClusterCommand(Context, 0x00, "Open", Fields(), false));
			Commands.Add(new ClusterCommand(Context, 0x01, "Close", Fields(), false));
		}
	}
}
