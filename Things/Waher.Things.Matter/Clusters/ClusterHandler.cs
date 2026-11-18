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
	/// Abstract base class for cluster handlers.
	/// </summary>
	public abstract class ClusterHandler : IClusterHandler
	{
		/// <summary>
		/// Abstract base class for cluster handlers.
		/// </summary>
		public ClusterHandler()
		{
		}

		/// <summary>
		/// How well the handler supports a given cluster.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Support grade.</returns>
		public abstract Grade Supports(uint ClusterId);

		/// <inheritdoc/>
		public virtual bool HasValues(MatterClusterInfo Cluster) => true;

		/// <inheritdoc/>
		public virtual AttributePath[] GetReadoutPaths(ushort EndpointId, MatterClusterInfo Cluster)
		{
			return new AttributePath[] { new AttributePath(EndpointId, Cluster.ClusterId, null) };
		}

		/// <inheritdoc/>
		public virtual bool HasEvents(MatterClusterInfo Cluster) => false;

		/// <inheritdoc/>
		public abstract void AddFields(ClusterContext Context, ClusterValues Values, FieldBuilder Fields);

		/// <inheritdoc/>
		public virtual void AddEventFields(ClusterContext Context, EventReport Event, FieldBuilder Fields)
		{
			AddGenericEventField(Context, Event, Fields);
		}

		/// <inheritdoc/>
		public virtual bool IsControllable(MatterClusterInfo Cluster) => false;

		/// <inheritdoc/>
		public virtual void AddControlParameters(ClusterContext Context, List<ControlParameter> Parameters)
		{
		}

		/// <inheritdoc/>
		public virtual void AddCommands(ClusterContext Context, List<ICommand> Commands)
		{
		}

		#region Helpers

		/// <summary>
		/// Adds a field representing an event, in a generic manner.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Event">Event report.</param>
		/// <param name="Fields">Field builder.</param>
		public static void AddGenericEventField(ClusterContext Context, EventReport Event, FieldBuilder Fields)
		{
			if (Event.Path.EventId is null)
				return;

			uint EventId = Event.Path.EventId.Value;
			EventDefinition Definition = Context.Definition?.GetEvent(EventId);
			string EventName = MatterDataModel.SpaceName(Definition?.Name) ?? ("Event 0x" + EventId.ToString("X2"));
			string Name = Fields.GetEventFieldName(Context.ClusterId, EventId, EventName, Context.ClusterName);
			string Value = ValueConverter.FormatEventData(Context.ClusterId, Definition, Event.Data);

			if (string.IsNullOrEmpty(Value))
				Value = EventName;

			Fields.String(Name, Value, FieldType.Momentary);
		}

		/// <summary>
		/// Adds a field for an attribute, in a generic manner, using metadata available for the attribute.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Fields">Field builder.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Attribute value.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field is writable.</param>
		/// <returns>If a field was added.</returns>
		public static bool AddGenericField(ClusterContext Context, FieldBuilder Fields, uint AttributeId, TlvElement Value,
			FieldType Type, bool Writable)
		{
			if (GlobalAttributeIds.IsGlobal(AttributeId))
				return false;

			AttributeDefinition Attribute = Context.Definition?.GetAttribute(AttributeId);
			if (!(Attribute is null) && Attribute.IsList)
				return false;

			string Name = GetGenericFieldName(Context, AttributeId);

			return ValueConverter.AddField(Fields, Context.ClusterId, Name, Attribute?.Type, Value, Type, Writable);
		}

		/// <summary>
		/// Gets a generic field name for an attribute.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Field name.</returns>
		public static string GetGenericFieldName(ClusterContext Context, uint AttributeId)
		{
			AttributeDefinition Attribute = Context.Definition?.GetAttribute(AttributeId);
			string AttributeName = MatterDataModel.SpaceName(Attribute?.Name) ?? ("Attribute 0x" + AttributeId.ToString("X4"));

			return Context.Names.GetName(Context.ClusterId, AttributeId, AttributeName, Context.ClusterName);
		}

		/// <summary>
		/// Checks if an attribute can be controlled using a generic control parameter.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="Attribute">Attribute definition.</param>
		/// <returns>If attribute can be controlled.</returns>
		public static bool IsGenericControllable(uint ClusterId, AttributeDefinition Attribute)
		{
			if (Attribute is null || !Attribute.Writable || Attribute.IsList || GlobalAttributeIds.IsGlobal(Attribute.Id))
				return false;

			switch (ValueConverter.Classify(ClusterId, Attribute.Type))
			{
				case ValueKind.Boolean:
				case ValueKind.UnsignedInteger:
				case ValueKind.SignedInteger:
				case ValueKind.Single:
				case ValueKind.Double:
				case ValueKind.String:
				case ValueKind.Temperature:
				case ValueKind.Percent:
				case ValueKind.Percent100ths:
				case ValueKind.ElapsedSeconds:
					return true;

				case ValueKind.Enum:
					return !(MatterDataModel.FindEnum(ClusterId, Attribute.Type) is null);

				default:
					return false;
			}
		}

		/// <summary>
		/// Creates a generic control parameter for a writable attribute.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Attribute">Attribute definition.</param>
		/// <returns>Control parameter, or null if not possible to create one.</returns>
		public static ControlParameter CreateGenericControlParameter(ClusterContext Context, AttributeDefinition Attribute)
		{
			if (!IsGenericControllable(Context.ClusterId, Attribute))
				return null;

			string Name = GetGenericFieldName(Context, Attribute.Id);
			string Page = Context.ClusterName;
			string Label = MatterDataModel.SpaceName(Attribute.Name) + ":";
			string Description = Context.ClusterName + ", " + MatterDataModel.SpaceName(Attribute.Name) + ".";
			ValueKind Kind = ValueConverter.Classify(Context.ClusterId, Attribute.Type);
			uint AttributeId = Attribute.Id;

			switch (Kind)
			{
				case ValueKind.Boolean:
					return new BooleanControlParameter(Name, Page, Label, Description,
						async (Node) =>
						{
							TlvElement E = await Context.ReadAttributeAsync(AttributeId);
							return E is null || E.IsNull ? (bool?)null : E.AsBoolean();
						},
						(Node, Value) => Context.WriteAttributeAsync(AttributeId, Value));

				case ValueKind.UnsignedInteger:
				case ValueKind.SignedInteger:
				case ValueKind.Percent:
				case ValueKind.ElapsedSeconds:
					ValueConverter.TryGetIntegerRange(Attribute.Type, out long Min, out long Max);

					if (Attribute.Min.HasValue)
						Min = Math.Max(Min, (long)Attribute.Min.Value);

					if (Attribute.Max.HasValue)
						Max = Math.Min(Max, (long)Attribute.Max.Value);

					return new Int64ControlParameter(Name, Page, Label, Description, Min, Max,
						async (Node) =>
						{
							TlvElement E = await Context.ReadAttributeAsync(AttributeId);
							return E is null || !E.IsInteger ? (long?)null : E.AsInt64();
						},
						(Node, Value) => Context.WriteAttributeAsync(AttributeId, ValueConverter.ToTlvValue(Kind, Value)));

				case ValueKind.Single:
				case ValueKind.Double:
				case ValueKind.Temperature:
				case ValueKind.Percent100ths:
					double? DMin = null;
					double? DMax = null;
					double Scale = 1;

					if (Kind == ValueKind.Temperature || Kind == ValueKind.Percent100ths)
					{
						Scale = 0.01;

						if (Attribute.Min.HasValue)
							DMin = Attribute.Min.Value * Scale;

						if (Attribute.Max.HasValue)
							DMax = Attribute.Max.Value * Scale;

						if (Kind == ValueKind.Percent100ths)
						{
							DMin = DMin ?? 0;
							DMax = DMax ?? 100;
						}
					}
					else
					{
						DMin = Attribute.Min;
						DMax = Attribute.Max;
					}

					return new DoubleControlParameter(Name, Page, Label, Description, DMin, DMax,
						async (Node) =>
						{
							TlvElement E = await Context.ReadAttributeAsync(AttributeId);
							return E is null || E.IsNull ? (double?)null : E.AsDouble() * Scale;
						},
						(Node, Value) => Context.WriteAttributeAsync(AttributeId, ValueConverter.ToTlvValue(Kind, Value)));

				case ValueKind.String:
					return new StringControlParameter(Name, Page, Label, Description,
						async (Node) =>
						{
							TlvElement E = await Context.ReadAttributeAsync(AttributeId);
							return E is null || E.IsNull ? null : E.AsString();
						},
						(Node, Value) => Context.WriteAttributeAsync(AttributeId, Value ?? string.Empty));

				case ValueKind.Enum:
					EnumDefinition Enum = MatterDataModel.FindEnum(Context.ClusterId, Attribute.Type);
					List<string> Options = new List<string>();
					List<string> Labels = new List<string>();

					foreach (KeyValuePair<long, string> Item in Enum.Items)
					{
						if (Attribute.Max.HasValue && Item.Key > Attribute.Max.Value)
							continue;

						Options.Add(Item.Value);
						Labels.Add(MatterDataModel.SpaceName(Item.Value));
					}

					return new StringControlParameter(Name, Page, Label, Description, Options.ToArray(), Labels.ToArray(),
						async (Node) =>
						{
							TlvElement E = await Context.ReadAttributeAsync(AttributeId);
							if (E is null || !E.IsInteger)
								return null;

							return Enum.TryGetName(E.AsInt64(), out string s) ? s : null;
						},
						(Node, Value) =>
						{
							if (!Enum.TryGetValue(Value, out long l))
								throw new ArgumentException("Invalid value.", nameof(Value));

							return Context.WriteAttributeAsync(AttributeId, (ulong)l);
						});

				default:
					return null;
			}
		}

		/// <summary>
		/// Adds generic commands for commands that do not require any arguments.
		/// </summary>
		/// <param name="Context">Cluster context.</param>
		/// <param name="Commands">Commands.</param>
		/// <param name="Exclude">Command IDs to exclude.</param>
		public static void AddGenericCommands(ClusterContext Context, List<ICommand> Commands, params uint[] Exclude)
		{
			if (Context.Definition is null)
				return;

			List<CommandDefinition> Definitions = new List<CommandDefinition>();

			foreach (CommandDefinition Command in Context.Definition.Commands)
			{
				if (!Command.HasNoMandatoryFields)
					continue;

				if (Array.IndexOf(Exclude, Command.Id) >= 0)
					continue;

				if (Context.Cluster.Commands is null)
				{
					if (Command.Optional)
						continue;
				}
				else if (!Context.Cluster.HasCommand(Command.Id))
					continue;

				Definitions.Add(Command);
			}

			Definitions.Sort((x, y) => x.Id.CompareTo(y.Id));

			foreach (CommandDefinition Command in Definitions)
			{
				Commands.Add(new ClusterCommand(Context, Command.Id, MatterDataModel.SpaceName(Command.Name),
					null, Command.Timed));
			}
		}

		/// <summary>
		/// Gets the attribute paths of a set of attributes, if supported by the cluster.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="Cluster">Cluster information.</param>
		/// <param name="AttributeIds">Attribute IDs.</param>
		/// <returns>Attribute paths.</returns>
		public static AttributePath[] GetPaths(ushort EndpointId, MatterClusterInfo Cluster, params uint[] AttributeIds)
		{
			List<AttributePath> Result = new List<AttributePath>();

			foreach (uint AttributeId in AttributeIds)
			{
				if (Cluster.HasAttribute(AttributeId))
					Result.Add(new AttributePath(EndpointId, Cluster.ClusterId, AttributeId));
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Creates a TLV structure of command fields.
		/// </summary>
		/// <param name="Fields">Fields</param>
		/// <returns>TLV structure.</returns>
		public static TlvElement Fields(params TlvElement[] Fields)
		{
			return TlvElement.Structure(TlvTag.Anonymous, Fields);
		}

		/// <summary>
		/// Gets a nullable boolean value from a TLV element.
		/// </summary>
		/// <param name="E">TLV element.</param>
		/// <returns>Boolean value, or null.</returns>
		public static bool? AsBoolean(TlvElement E)
		{
			if (E is null || E.Type != TlvValueType.Boolean)
				return null;
			else
				return E.AsBoolean();
		}

		/// <summary>
		/// Gets a nullable numeric value from a TLV element.
		/// </summary>
		/// <param name="E">TLV element.</param>
		/// <returns>Numeric value, or null.</returns>
		public static double? AsDouble(TlvElement E)
		{
			if (E is null || E.IsNull)
				return null;

			switch (E.Type)
			{
				case TlvValueType.SignedInteger:
				case TlvValueType.UnsignedInteger:
				case TlvValueType.Float:
				case TlvValueType.Double:
					return E.AsDouble();

				default:
					return null;
			}
		}

		/// <summary>
		/// Gets a nullable integer value from a TLV element.
		/// </summary>
		/// <param name="E">TLV element.</param>
		/// <returns>Integer value, or null.</returns>
		public static long? AsInt64(TlvElement E)
		{
			if (E is null || !E.IsInteger)
				return null;
			else
				return E.AsInt64();
		}

		/// <summary>
		/// Gets the name of an enumeration value.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="EnumName">Name of enumeration type.</param>
		/// <param name="Value">Value</param>
		/// <returns>Name of value, with spaces between words.</returns>
		public static string GetEnumName(uint ClusterId, string EnumName, long Value)
		{
			EnumDefinition Enum = MatterDataModel.FindEnum(ClusterId, EnumName);

			if (!(Enum is null) && Enum.TryGetName(Value, out string Name))
				return MatterDataModel.SpaceName(Name);
			else
				return Value.ToString();
		}

		/// <summary>
		/// Converts a color reference to hue (0-360) and saturation (0-1) values.
		/// </summary>
		/// <param name="Color">Color.</param>
		/// <param name="Hue">Hue, in degrees.</param>
		/// <param name="Saturation">Saturation (0-1).</param>
		/// <param name="Value">Value (0-1).</param>
		public static void ToHsv(ColorReference Color, out double Hue, out double Saturation, out double Value)
		{
			double R = Color.Red / 255.0;
			double G = Color.Green / 255.0;
			double B = Color.Blue / 255.0;
			double Max = Math.Max(R, Math.Max(G, B));
			double Min = Math.Min(R, Math.Min(G, B));
			double Delta = Max - Min;

			if (Delta == 0)
				Hue = 0;
			else if (Max == R)
				Hue = 60 * (((G - B) / Delta) % 6);
			else if (Max == G)
				Hue = 60 * (((B - R) / Delta) + 2);
			else
				Hue = 60 * (((R - G) / Delta) + 4);

			if (Hue < 0)
				Hue += 360;

			Saturation = Max == 0 ? 0 : Delta / Max;
			Value = Max;
		}

		/// <summary>
		/// Converts hue and saturation values to a color reference.
		/// </summary>
		/// <param name="Hue">Hue, in degrees.</param>
		/// <param name="Saturation">Saturation (0-1).</param>
		/// <param name="Value">Value (0-1).</param>
		/// <returns>Color reference.</returns>
		public static ColorReference FromHsv(double Hue, double Saturation, double Value)
		{
			double C = Value * Saturation;
			double X = C * (1 - Math.Abs(((Hue / 60) % 2) - 1));
			double M = Value - C;
			double R, G, B;

			Hue %= 360;
			if (Hue < 0)
				Hue += 360;

			if (Hue < 60)
			{
				R = C; G = X; B = 0;
			}
			else if (Hue < 120)
			{
				R = X; G = C; B = 0;
			}
			else if (Hue < 180)
			{
				R = 0; G = C; B = X;
			}
			else if (Hue < 240)
			{
				R = 0; G = X; B = C;
			}
			else if (Hue < 300)
			{
				R = X; G = 0; B = C;
			}
			else
			{
				R = C; G = 0; B = X;
			}

			return new ColorReference(ToByte(R + M), ToByte(G + M), ToByte(B + M));
		}

		/// <summary>
		/// Converts a CIE 1931 xy chromaticity coordinate to a color reference (assuming full brightness).
		/// </summary>
		/// <param name="x">x coordinate.</param>
		/// <param name="y">y coordinate.</param>
		/// <returns>Color reference.</returns>
		public static ColorReference FromXy(double x, double y)
		{
			if (y <= 0)
				return new ColorReference(0, 0, 0);

			double Y = 1.0;
			double X = (Y / y) * x;
			double Z = (Y / y) * (1 - x - y);

			double R = X * 3.2406 - Y * 1.5372 - Z * 0.4986;
			double G = -X * 0.9689 + Y * 1.8758 + Z * 0.0415;
			double B = X * 0.0557 - Y * 0.2040 + Z * 1.0570;

			double Max = Math.Max(R, Math.Max(G, B));
			if (Max > 1)
			{
				R /= Max;
				G /= Max;
				B /= Max;
			}

			return new ColorReference(ToByte(GammaCorrect(R)), ToByte(GammaCorrect(G)), ToByte(GammaCorrect(B)));
		}

		/// <summary>
		/// Converts a color reference to CIE 1931 xy chromaticity coordinates.
		/// </summary>
		/// <param name="Color">Color.</param>
		/// <param name="x">x coordinate.</param>
		/// <param name="y">y coordinate.</param>
		public static void ToXy(ColorReference Color, out double x, out double y)
		{
			double R = InverseGamma(Color.Red / 255.0);
			double G = InverseGamma(Color.Green / 255.0);
			double B = InverseGamma(Color.Blue / 255.0);

			double X = R * 0.4124 + G * 0.3576 + B * 0.1805;
			double Y = R * 0.2126 + G * 0.7152 + B * 0.0722;
			double Z = R * 0.0193 + G * 0.1192 + B * 0.9505;
			double Sum = X + Y + Z;

			if (Sum <= 0)
			{
				x = 0.3127;
				y = 0.3290;
			}
			else
			{
				x = X / Sum;
				y = Y / Sum;
			}
		}

		private static double GammaCorrect(double c)
		{
			if (c <= 0)
				return 0;
			else if (c <= 0.0031308)
				return 12.92 * c;
			else
				return 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
		}

		private static double InverseGamma(double c)
		{
			if (c <= 0.04045)
				return c / 12.92;
			else
				return Math.Pow((c + 0.055) / 1.055, 2.4);
		}

		private static byte ToByte(double d)
		{
			int i = (int)Math.Round(d * 255);

			if (i < 0)
				return 0;
			else if (i > 255)
				return 255;
			else
				return (byte)i;
		}

		#endregion
	}
}
