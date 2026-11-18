using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.DataModel
{
	/// <summary>
	/// Provides metadata about the Matter data model (clusters, attributes, commands, events, data types and device types).
	/// The metadata is generated from the data model definitions of the Matter SDK.
	/// </summary>
	public static class MatterDataModel
	{
		private static readonly object synchObject = new object();
		private static Dictionary<uint, ClusterDefinition> clusters = null;
		private static Dictionary<uint, DeviceTypeDefinition> deviceTypes = null;
		private static Dictionary<string, List<DataTypeDefinition>> dataTypes = null;

		/// <summary>
		/// Name of embedded resource containing the data model.
		/// </summary>
		public const string ResourceName = "Waher.Things.Matter.DataModel.MatterDataModel.xml";

		private static void Load()
		{
			if (!(clusters is null))
				return;

			lock (synchObject)
			{
				if (!(clusters is null))
					return;

				Dictionary<uint, ClusterDefinition> Clusters = new Dictionary<uint, ClusterDefinition>();
				Dictionary<uint, DeviceTypeDefinition> DeviceTypes = new Dictionary<uint, DeviceTypeDefinition>();
				Dictionary<string, List<DataTypeDefinition>> DataTypes = new Dictionary<string, List<DataTypeDefinition>>();
				Assembly A = typeof(MatterDataModel).Assembly;

				using (Stream f = A.GetManifestResourceStream(ResourceName))
				{
					if (!(f is null))
					{
						XmlDocument Doc = new XmlDocument()
						{
							PreserveWhitespace = false
						};
						Doc.Load(f);

						foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
						{
							if (!(N is XmlElement E))
								continue;

							switch (E.LocalName)
							{
								case "C":
									ClusterDefinition Cluster = ClusterDefinition.Parse(E);
									Clusters[Cluster.Id] = Cluster;
									break;

								case "N":
									AddDataType(DataTypes, EnumDefinition.Parse(E));
									break;

								case "B":
									AddDataType(DataTypes, BitmapDefinition.Parse(E));
									break;

								case "S":
									AddDataType(DataTypes, StructDefinition.Parse(E));
									break;

								case "D":
									DeviceTypeDefinition DeviceType = DeviceTypeDefinition.Parse(E);
									DeviceTypes[DeviceType.Id] = DeviceType;
									break;
							}
						}
					}
				}

				deviceTypes = DeviceTypes;
				dataTypes = DataTypes;
				clusters = Clusters;
			}
		}

		private static void AddDataType(Dictionary<string, List<DataTypeDefinition>> DataTypes, DataTypeDefinition Definition)
		{
			if (!DataTypes.TryGetValue(Definition.Name, out List<DataTypeDefinition> List))
			{
				List = new List<DataTypeDefinition>();
				DataTypes[Definition.Name] = List;
			}

			List.Add(Definition);
		}

		/// <summary>
		/// Gets a cluster definition.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Cluster definition, or null if not found.</returns>
		public static ClusterDefinition GetCluster(uint ClusterId)
		{
			Load();
			return clusters.TryGetValue(ClusterId, out ClusterDefinition Result) ? Result : null;
		}

		/// <summary>
		/// Gets all known cluster definitions.
		/// </summary>
		/// <returns>Cluster definitions.</returns>
		public static ClusterDefinition[] GetClusters()
		{
			Load();

			ClusterDefinition[] Result = new ClusterDefinition[clusters.Count];
			clusters.Values.CopyTo(Result, 0);
			Array.Sort(Result, (x, y) => x.Id.CompareTo(y.Id));

			return Result;
		}

		/// <summary>
		/// Gets the name of a cluster.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Name of cluster.</returns>
		public static string GetClusterName(uint ClusterId)
		{
			return GetCluster(ClusterId)?.Name ?? ("Cluster 0x" + ClusterId.ToString("X4"));
		}

		/// <summary>
		/// Gets an attribute definition.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute definition, or null if not found.</returns>
		public static AttributeDefinition GetAttribute(uint ClusterId, uint AttributeId)
		{
			if (GlobalAttributes.TryGetValue(AttributeId, out AttributeDefinition Global))
				return Global;

			return GetCluster(ClusterId)?.GetAttribute(AttributeId);
		}

		/// <summary>
		/// Gets the name of an attribute.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Name of attribute.</returns>
		public static string GetAttributeName(uint ClusterId, uint AttributeId)
		{
			return GetAttribute(ClusterId, AttributeId)?.Name ?? ("Attribute 0x" + AttributeId.ToString("X4"));
		}

		/// <summary>
		/// Gets the name of a command.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="CommandId">Command ID.</param>
		/// <returns>Name of command.</returns>
		public static string GetCommandName(uint ClusterId, uint CommandId)
		{
			return GetCluster(ClusterId)?.GetCommand(CommandId)?.Name ?? ("Command 0x" + CommandId.ToString("X2"));
		}

		/// <summary>
		/// Gets the name of an event.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="EventId">Event ID.</param>
		/// <returns>Name of event.</returns>
		public static string GetEventName(uint ClusterId, uint EventId)
		{
			return GetCluster(ClusterId)?.GetEvent(EventId)?.Name ?? ("Event 0x" + EventId.ToString("X2"));
		}

		/// <summary>
		/// Gets a device type definition.
		/// </summary>
		/// <param name="DeviceTypeId">Device type ID.</param>
		/// <returns>Device type definition, or null if not found.</returns>
		public static DeviceTypeDefinition GetDeviceType(uint DeviceTypeId)
		{
			Load();
			return deviceTypes.TryGetValue(DeviceTypeId, out DeviceTypeDefinition Result) ? Result : null;
		}

		/// <summary>
		/// Gets the name of a device type.
		/// </summary>
		/// <param name="DeviceTypeId">Device type ID.</param>
		/// <returns>Name of device type.</returns>
		public static string GetDeviceTypeName(uint DeviceTypeId)
		{
			return GetDeviceType(DeviceTypeId)?.Name ?? ("Device Type 0x" + DeviceTypeId.ToString("X4"));
		}

		/// <summary>
		/// Finds a named data type definition (enumeration, bitmap or structure), valid for a given cluster.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="TypeName">Name of data type.</param>
		/// <returns>Data type definition, or null if not found.</returns>
		public static DataTypeDefinition FindDataType(uint ClusterId, string TypeName)
		{
			if (string.IsNullOrEmpty(TypeName))
				return null;

			Load();

			if (!dataTypes.TryGetValue(TypeName, out List<DataTypeDefinition> List))
				return null;

			DataTypeDefinition Global = null;

			foreach (DataTypeDefinition Definition in List)
			{
				if (Definition.Clusters.Length == 0)
					Global = Definition;
				else if (Array.IndexOf(Definition.Clusters, ClusterId) >= 0)
					return Definition;
			}

			return Global ?? List[0];
		}

		/// <summary>
		/// Finds an enumeration definition.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="TypeName">Name of data type.</param>
		/// <returns>Enumeration definition, or null if not found.</returns>
		public static EnumDefinition FindEnum(uint ClusterId, string TypeName)
		{
			return FindDataType(ClusterId, TypeName) as EnumDefinition;
		}

		/// <summary>
		/// Finds a bitmap definition.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="TypeName">Name of data type.</param>
		/// <returns>Bitmap definition, or null if not found.</returns>
		public static BitmapDefinition FindBitmap(uint ClusterId, string TypeName)
		{
			return FindDataType(ClusterId, TypeName) as BitmapDefinition;
		}

		/// <summary>
		/// Finds a structure definition.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="TypeName">Name of data type.</param>
		/// <returns>Structure definition, or null if not found.</returns>
		public static StructDefinition FindStruct(uint ClusterId, string TypeName)
		{
			return FindDataType(ClusterId, TypeName) as StructDefinition;
		}

		/// <summary>
		/// Global attributes, common to all clusters.
		/// </summary>
		public static readonly Dictionary<uint, AttributeDefinition> GlobalAttributes = new Dictionary<uint, AttributeDefinition>()
		{
			{ GlobalAttributeIds.GeneratedCommandList, new AttributeDefinition(GlobalAttributeIds.GeneratedCommandList, "GeneratedCommandList", "array", "command_id", false, false, false, false, null, null) },
			{ GlobalAttributeIds.AcceptedCommandList, new AttributeDefinition(GlobalAttributeIds.AcceptedCommandList, "AcceptedCommandList", "array", "command_id", false, false, false, false, null, null) },
			{ GlobalAttributeIds.EventList, new AttributeDefinition(GlobalAttributeIds.EventList, "EventList", "array", "event_id", false, false, true, false, null, null) },
			{ GlobalAttributeIds.AttributeList, new AttributeDefinition(GlobalAttributeIds.AttributeList, "AttributeList", "array", "attrib_id", false, false, false, false, null, null) },
			{ GlobalAttributeIds.FeatureMap, new AttributeDefinition(GlobalAttributeIds.FeatureMap, "FeatureMap", "bitmap32", null, false, false, false, false, null, null) },
			{ GlobalAttributeIds.ClusterRevision, new AttributeDefinition(GlobalAttributeIds.ClusterRevision, "ClusterRevision", "int16u", null, false, false, false, false, null, null) }
		};

		#region Value formatting

		/// <summary>
		/// Formats a value for presentation, using available metadata.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="TypeName">Type name.</param>
		/// <param name="EntryType">Entry type, for lists.</param>
		/// <param name="Value">Value.</param>
		/// <returns>Formatted value.</returns>
		public static string FormatValue(uint ClusterId, string TypeName, string EntryType, TlvElement Value)
		{
			StringBuilder sb = new StringBuilder();
			FormatValue(sb, ClusterId, TypeName, EntryType, Value, 0);
			return sb.ToString();
		}

		/// <summary>
		/// Formats an attribute value for presentation, using available metadata.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value.</param>
		/// <returns>Formatted value.</returns>
		public static string FormatAttributeValue(uint ClusterId, uint AttributeId, TlvElement Value)
		{
			AttributeDefinition Attribute = GetAttribute(ClusterId, AttributeId);
			return FormatValue(ClusterId, Attribute?.Type, Attribute?.EntryType, Value);
		}

		private static void FormatValue(StringBuilder Output, uint ClusterId, string TypeName, string EntryType, TlvElement Value, int Depth)
		{
			if (Value is null || Value.IsNull)
			{
				Output.Append("null");
				return;
			}

			if (Depth > 8)
			{
				Output.Append("...");
				return;
			}

			if (Value.IsArray || Value.IsList)
			{
				bool First = true;

				Output.Append('[');

				foreach (TlvElement Item in Value.Children)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					FormatValue(Output, ClusterId, EntryType ?? TypeName, null, Item, Depth + 1);
				}

				Output.Append(']');
				return;
			}

			if (Value.IsStructure)
			{
				StructDefinition Struct = FindStruct(ClusterId, TypeName);
				bool First = true;

				Output.Append('{');

				foreach (TlvElement Member in Value.Children)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					FieldDefinition Field = null;

					if (Member.Tag.IsContextSpecific)
					{
						Field = Struct?.GetField((int)Member.Tag.Number);

						if (!(Field is null))
							Output.Append(Field.Name);
						else if (Member.Tag.Number == FabricIndexFieldId)
							Output.Append("FabricIndex");
						else
							Output.Append(Member.Tag.Number.ToString());
					}
					else
						Output.Append(Member.Tag.ToString());

					Output.Append(": ");

					if (Field is null)
						FormatValue(Output, ClusterId, null, null, Member, Depth + 1);
					else if (Field.IsArray)
						FormatValue(Output, ClusterId, Field.Type, Field.Type, Member, Depth + 1);
					else
						FormatValue(Output, ClusterId, Field.Type, null, Member, Depth + 1);
				}

				Output.Append('}');
				return;
			}

			switch (Value.Type)
			{
				case TlvValueType.Boolean:
					Output.Append(Value.AsBoolean() ? "true" : "false");
					return;

				case TlvValueType.String:
					Output.Append(Value.AsString());
					return;

				case TlvValueType.ByteString:
					byte[] Bin = Value.AsBytes();
					int i, c = Math.Min(Bin.Length, 64);

					for (i = 0; i < c; i++)
						Output.Append(Bin[i].ToString("x2"));

					if (c < Bin.Length)
						Output.Append("...");
					return;

				case TlvValueType.Float:
				case TlvValueType.Double:
					Output.Append(Value.AsDouble().ToString(CultureInfo.InvariantCulture));
					return;

				case TlvValueType.SignedInteger:
				case TlvValueType.UnsignedInteger:
					FormatInteger(Output, ClusterId, TypeName, Value);
					return;

				default:
					Output.Append(Value.ToString());
					return;
			}
		}

		private static void FormatInteger(StringBuilder Output, uint ClusterId, string TypeName, TlvElement Value)
		{
			DataTypeDefinition DataType = FindDataType(ClusterId, TypeName);

			if (DataType is EnumDefinition Enum)
			{
				long l = Value.AsInt64();

				if (Enum.TryGetName(l, out string Name))
					Output.Append(Name);
				else
					Output.Append(l.ToString());

				return;
			}

			if (DataType is BitmapDefinition Bitmap)
			{
				Output.Append(Bitmap.ToString(Value.AsUInt64()));
				return;
			}

			switch (TypeName)
			{
				case "cluster_id":
					Output.Append(GetClusterName(Value.AsUInt32()));
					return;

				case "devtype_id":
					Output.Append(GetDeviceTypeName(Value.AsUInt32()));
					return;

				case "node_id":
				case "subject_id":
				case "fabric_id":
				case "eui64":
					Output.Append(Value.AsUInt64().ToString("X16"));
					return;

				case "vendor_id":
				case "attrib_id":
				case "command_id":
				case "event_id":
				case "bitmap8":
				case "bitmap16":
				case "bitmap32":
				case "bitmap64":
					Output.Append("0x");
					Output.Append(Value.AsUInt64().ToString("X4"));
					return;

				case "temperature":
					Output.Append((Value.AsInt64() * 0.01).ToString("F2", CultureInfo.InvariantCulture));
					Output.Append(" °C");
					return;

				case "epoch_s":
					Output.Append(XmlConvert.ToString(MatterEpoch.AddSeconds(Value.AsUInt64()), XmlDateTimeSerializationMode.Utc));
					return;

				case "epoch_us":
					Output.Append(XmlConvert.ToString(MatterEpoch.AddTicks((long)Value.AsUInt64() * 10), XmlDateTimeSerializationMode.Utc));
					return;

				default:
					if (Value.Type == TlvValueType.SignedInteger)
						Output.Append(Value.AsInt64().ToString());
					else
						Output.Append(Value.AsUInt64().ToString());
					return;
			}
		}

		/// <summary>
		/// Field ID of the fabric index field of fabric-scoped structures.
		/// </summary>
		public const uint FabricIndexFieldId = 254;

		/// <summary>
		/// Matter epoch (2000-01-01T00:00:00Z).
		/// </summary>
		public static readonly DateTime MatterEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Converts a name in Pascal case (e.g. "MeasuredValue") to a name with spaces (e.g. "Measured Value").
		/// </summary>
		/// <param name="Name">Name.</param>
		/// <returns>Name with spaces.</returns>
		public static string SpaceName(string Name)
		{
			if (string.IsNullOrEmpty(Name))
				return Name;

			StringBuilder sb = new StringBuilder();
			int i, c = Name.Length;
			char ch, Prev = (char)0;

			for (i = 0; i < c; i++)
			{
				ch = Name[i];

				if (i > 0 && char.IsUpper(ch))
				{
					bool NextIsLower = i + 1 < c && char.IsLower(Name[i + 1]);

					if (char.IsLower(Prev) || char.IsDigit(Prev) || (char.IsUpper(Prev) && NextIsLower))
						sb.Append(' ');
				}
				else if (i > 0 && char.IsDigit(ch) && char.IsLetter(Prev) && !(Prev == 'M' && i == 2 && Name.StartsWith("PM")))
					sb.Append(' ');

				sb.Append(ch);
				Prev = ch;
			}

			return sb.ToString();
		}

		#endregion

		internal static uint ParseHex(string s)
		{
			return uint.Parse(s, NumberStyles.HexNumber);
		}

		internal static uint[] ParseHexList(string s)
		{
			if (string.IsNullOrEmpty(s))
				return Array.Empty<uint>();

			string[] Parts = s.Split(',');
			uint[] Result = new uint[Parts.Length];
			int i, c = Parts.Length;

			for (i = 0; i < c; i++)
				Result[i] = ParseHex(Parts[i]);

			return Result;
		}
	}

	/// <summary>
	/// Definition of a cluster.
	/// </summary>
	public class ClusterDefinition
	{
		private readonly Dictionary<uint, AttributeDefinition> attributes = new Dictionary<uint, AttributeDefinition>();
		private readonly Dictionary<uint, CommandDefinition> requests = new Dictionary<uint, CommandDefinition>();
		private readonly Dictionary<uint, CommandDefinition> responses = new Dictionary<uint, CommandDefinition>();
		private readonly Dictionary<uint, EventDefinition> events = new Dictionary<uint, EventDefinition>();
		private readonly List<FeatureDefinition> features = new List<FeatureDefinition>();

		/// <summary>
		/// Definition of a cluster.
		/// </summary>
		/// <param name="Id">Cluster ID.</param>
		/// <param name="Name">Cluster name.</param>
		public ClusterDefinition(uint Id, string Name)
		{
			this.Id = Id;
			this.Name = Name;
		}

		/// <summary>
		/// Cluster ID.
		/// </summary>
		public uint Id { get; }

		/// <summary>
		/// Cluster name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Attributes.
		/// </summary>
		public IEnumerable<AttributeDefinition> Attributes => this.attributes.Values;

		/// <summary>
		/// Commands that can be sent to the cluster.
		/// </summary>
		public IEnumerable<CommandDefinition> Commands => this.requests.Values;

		/// <summary>
		/// Events.
		/// </summary>
		public IEnumerable<EventDefinition> Events => this.events.Values;

		/// <summary>
		/// Features.
		/// </summary>
		public IEnumerable<FeatureDefinition> Features => this.features;

		/// <summary>
		/// Gets an attribute definition.
		/// </summary>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Attribute definition, or null if not found.</returns>
		public AttributeDefinition GetAttribute(uint AttributeId)
		{
			if (this.attributes.TryGetValue(AttributeId, out AttributeDefinition Result))
				return Result;
			else if (MatterDataModel.GlobalAttributes.TryGetValue(AttributeId, out Result))
				return Result;
			else
				return null;
		}

		/// <summary>
		/// Gets a command definition (client to server).
		/// </summary>
		/// <param name="CommandId">Command ID.</param>
		/// <returns>Command definition, or null if not found.</returns>
		public CommandDefinition GetCommand(uint CommandId)
		{
			return this.requests.TryGetValue(CommandId, out CommandDefinition Result) ? Result : null;
		}

		/// <summary>
		/// Gets a command response definition (server to client).
		/// </summary>
		/// <param name="CommandId">Command ID.</param>
		/// <returns>Command definition, or null if not found.</returns>
		public CommandDefinition GetResponse(uint CommandId)
		{
			return this.responses.TryGetValue(CommandId, out CommandDefinition Result) ? Result : null;
		}

		/// <summary>
		/// Gets an event definition.
		/// </summary>
		/// <param name="EventId">Event ID.</param>
		/// <returns>Event definition, or null if not found.</returns>
		public EventDefinition GetEvent(uint EventId)
		{
			return this.events.TryGetValue(EventId, out EventDefinition Result) ? Result : null;
		}

		/// <summary>
		/// Gets the names of features supported, given a feature map.
		/// </summary>
		/// <param name="FeatureMap">Feature map.</param>
		/// <returns>Feature names.</returns>
		public string[] GetFeatureNames(uint FeatureMap)
		{
			List<string> Result = new List<string>();

			foreach (FeatureDefinition Feature in this.features)
			{
				if ((FeatureMap & (1U << Feature.Bit)) != 0)
					Result.Add(Feature.Name);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Checks if a feature is supported, given a feature map.
		/// </summary>
		/// <param name="FeatureMap">Feature map.</param>
		/// <param name="Code">Feature code.</param>
		/// <returns>If feature is supported.</returns>
		public bool HasFeature(uint FeatureMap, string Code)
		{
			foreach (FeatureDefinition Feature in this.features)
			{
				if (Feature.Code == Code)
					return (FeatureMap & (1U << Feature.Bit)) != 0;
			}

			return false;
		}

		internal static ClusterDefinition Parse(XmlElement E)
		{
			ClusterDefinition Result = new ClusterDefinition(MatterDataModel.ParseHex(E.GetAttribute("i")), E.GetAttribute("n"));

			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				switch (E2.LocalName)
				{
					case "F":
						Result.features.Add(new FeatureDefinition(int.Parse(E2.GetAttribute("b")), E2.GetAttribute("c"), E2.GetAttribute("n")));
						break;

					case "A":
						AttributeDefinition Attribute = new AttributeDefinition(MatterDataModel.ParseHex(E2.GetAttribute("i")),
							E2.GetAttribute("n"), E2.GetAttribute("t"), NullIfEmpty(E2.GetAttribute("e")),
							E2.GetAttribute("w") == "1", E2.GetAttribute("nl") == "1", E2.GetAttribute("o") == "1",
							E2.GetAttribute("tm") == "1", ParseLimit(E2.GetAttribute("min")), ParseLimit(E2.GetAttribute("max")));
						Result.attributes[Attribute.Id] = Attribute;
						break;

					case "Q":
					case "R":
						CommandDefinition Command = CommandDefinition.Parse(E2);
						if (E2.LocalName == "Q")
							Result.requests[Command.Id] = Command;
						else
							Result.responses[Command.Id] = Command;
						break;

					case "E":
						EventDefinition Event = EventDefinition.Parse(E2);
						Result.events[Event.Id] = Event;
						break;
				}
			}

			return Result;
		}

		internal static string NullIfEmpty(string s)
		{
			return string.IsNullOrEmpty(s) ? null : s;
		}

		internal static double? ParseLimit(string s)
		{
			if (string.IsNullOrEmpty(s))
				return null;

			if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				if (ulong.TryParse(s.Substring(2), NumberStyles.HexNumber, null, out ulong l))
					return l;
				else
					return null;
			}

			if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
				return d;
			else
				return null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name + " (0x" + this.Id.ToString("X4") + ")";
		}
	}

	/// <summary>
	/// Definition of a cluster feature.
	/// </summary>
	public class FeatureDefinition
	{
		/// <summary>
		/// Definition of a cluster feature.
		/// </summary>
		/// <param name="Bit">Bit number in feature map.</param>
		/// <param name="Code">Feature code.</param>
		/// <param name="Name">Feature name.</param>
		public FeatureDefinition(int Bit, string Code, string Name)
		{
			this.Bit = Bit;
			this.Code = Code;
			this.Name = Name;
		}

		/// <summary>
		/// Bit number in feature map.
		/// </summary>
		public int Bit { get; }

		/// <summary>
		/// Feature code.
		/// </summary>
		public string Code { get; }

		/// <summary>
		/// Feature name.
		/// </summary>
		public string Name { get; }
	}

	/// <summary>
	/// Definition of an attribute.
	/// </summary>
	public class AttributeDefinition
	{
		/// <summary>
		/// Definition of an attribute.
		/// </summary>
		/// <param name="Id">Attribute ID.</param>
		/// <param name="Name">Attribute name.</param>
		/// <param name="Type">Data type.</param>
		/// <param name="EntryType">Entry type, for lists.</param>
		/// <param name="Writable">If attribute is writable.</param>
		/// <param name="Nullable">If attribute is nullable.</param>
		/// <param name="Optional">If attribute is optional.</param>
		/// <param name="TimedWrite">If writes must be timed.</param>
		/// <param name="Min">Smallest value, if defined.</param>
		/// <param name="Max">Largest value, if defined.</param>
		public AttributeDefinition(uint Id, string Name, string Type, string EntryType, bool Writable, bool Nullable,
			bool Optional, bool TimedWrite, double? Min, double? Max)
		{
			this.Id = Id;
			this.Name = Name;
			this.Type = Type;
			this.EntryType = EntryType;
			this.Writable = Writable;
			this.Nullable = Nullable;
			this.Optional = Optional;
			this.TimedWrite = TimedWrite;
			this.Min = Min;
			this.Max = Max;
		}

		/// <summary>
		/// Attribute ID.
		/// </summary>
		public uint Id { get; }

		/// <summary>
		/// Attribute name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Data type.
		/// </summary>
		public string Type { get; }

		/// <summary>
		/// Entry type, for lists.
		/// </summary>
		public string EntryType { get; }

		/// <summary>
		/// If attribute is a list.
		/// </summary>
		public bool IsList => this.Type == "array" || !string.IsNullOrEmpty(this.EntryType);

		/// <summary>
		/// If attribute is writable.
		/// </summary>
		public bool Writable { get; }

		/// <summary>
		/// If attribute is nullable.
		/// </summary>
		public bool Nullable { get; }

		/// <summary>
		/// If attribute is optional.
		/// </summary>
		public bool Optional { get; }

		/// <summary>
		/// If writes must be performed using timed interactions.
		/// </summary>
		public bool TimedWrite { get; }

		/// <summary>
		/// Smallest value, if defined.
		/// </summary>
		public double? Min { get; }

		/// <summary>
		/// Largest value, if defined.
		/// </summary>
		public double? Max { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name + " (0x" + this.Id.ToString("X4") + ")";
		}
	}

	/// <summary>
	/// Definition of a field in a command, event or structure.
	/// </summary>
	public class FieldDefinition
	{
		/// <summary>
		/// Definition of a field in a command, event or structure.
		/// </summary>
		/// <param name="Id">Field ID (context tag).</param>
		/// <param name="Name">Field name.</param>
		/// <param name="Type">Data type.</param>
		/// <param name="IsArray">If the field is an array.</param>
		/// <param name="Optional">If the field is optional.</param>
		/// <param name="Nullable">If the field is nullable.</param>
		public FieldDefinition(int Id, string Name, string Type, bool IsArray, bool Optional, bool Nullable)
		{
			this.Id = Id;
			this.Name = Name;
			this.Type = Type;
			this.IsArray = IsArray;
			this.Optional = Optional;
			this.Nullable = Nullable;
		}

		/// <summary>
		/// Field ID (context tag).
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// Field name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Data type.
		/// </summary>
		public string Type { get; }

		/// <summary>
		/// If the field is an array.
		/// </summary>
		public bool IsArray { get; }

		/// <summary>
		/// If the field is optional.
		/// </summary>
		public bool Optional { get; }

		/// <summary>
		/// If the field is nullable.
		/// </summary>
		public bool Nullable { get; }

		internal static FieldDefinition Parse(XmlElement E)
		{
			return new FieldDefinition(int.Parse(E.GetAttribute("i")), E.GetAttribute("n"), E.GetAttribute("t"),
				E.GetAttribute("a") == "1", E.GetAttribute("o") == "1", E.GetAttribute("nl") == "1");
		}

		internal static FieldDefinition[] ParseFields(XmlElement E)
		{
			List<FieldDefinition> Result = new List<FieldDefinition>();

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2 && E2.LocalName == "P")
					Result.Add(Parse(E2));
			}

			return Result.ToArray();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}

	/// <summary>
	/// Definition of a command.
	/// </summary>
	public class CommandDefinition
	{
		/// <summary>
		/// Definition of a command.
		/// </summary>
		/// <param name="Id">Command ID.</param>
		/// <param name="Name">Command name.</param>
		/// <param name="Response">Name of response command, if any.</param>
		/// <param name="Timed">If command requires a timed interaction.</param>
		/// <param name="Optional">If command is optional.</param>
		/// <param name="Fields">Command fields.</param>
		public CommandDefinition(uint Id, string Name, string Response, bool Timed, bool Optional, FieldDefinition[] Fields)
		{
			this.Id = Id;
			this.Name = Name;
			this.Response = Response;
			this.Timed = Timed;
			this.Optional = Optional;
			this.Fields = Fields;
		}

		/// <summary>
		/// Command ID.
		/// </summary>
		public uint Id { get; }

		/// <summary>
		/// Command name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Name of response command, if any.
		/// </summary>
		public string Response { get; }

		/// <summary>
		/// If command requires a timed interaction.
		/// </summary>
		public bool Timed { get; }

		/// <summary>
		/// If command is optional.
		/// </summary>
		public bool Optional { get; }

		/// <summary>
		/// Command fields.
		/// </summary>
		public FieldDefinition[] Fields { get; }

		/// <summary>
		/// If the command has no mandatory fields.
		/// </summary>
		public bool HasNoMandatoryFields
		{
			get
			{
				foreach (FieldDefinition Field in this.Fields)
				{
					if (!Field.Optional)
						return false;
				}

				return true;
			}
		}

		internal static CommandDefinition Parse(XmlElement E)
		{
			return new CommandDefinition(MatterDataModel.ParseHex(E.GetAttribute("i")), E.GetAttribute("n"),
				ClusterDefinition.NullIfEmpty(E.GetAttribute("r")), E.GetAttribute("tm") == "1", E.GetAttribute("o") == "1",
				FieldDefinition.ParseFields(E));
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name + " (0x" + this.Id.ToString("X2") + ")";
		}
	}

	/// <summary>
	/// Definition of an event.
	/// </summary>
	public class EventDefinition
	{
		/// <summary>
		/// Definition of an event.
		/// </summary>
		/// <param name="Id">Event ID.</param>
		/// <param name="Name">Event name.</param>
		/// <param name="Priority">Event priority.</param>
		/// <param name="Fields">Event fields.</param>
		public EventDefinition(uint Id, string Name, string Priority, FieldDefinition[] Fields)
		{
			this.Id = Id;
			this.Name = Name;
			this.Priority = Priority;
			this.Fields = Fields;
		}

		/// <summary>
		/// Event ID.
		/// </summary>
		public uint Id { get; }

		/// <summary>
		/// Event name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Event priority (debug, info or critical).
		/// </summary>
		public string Priority { get; }

		/// <summary>
		/// Event fields.
		/// </summary>
		public FieldDefinition[] Fields { get; }

		/// <summary>
		/// Gets a field definition.
		/// </summary>
		/// <param name="FieldId">Field ID.</param>
		/// <returns>Field definition, or null if not found.</returns>
		public FieldDefinition GetField(int FieldId)
		{
			foreach (FieldDefinition Field in this.Fields)
			{
				if (Field.Id == FieldId)
					return Field;
			}

			return null;
		}

		internal static EventDefinition Parse(XmlElement E)
		{
			return new EventDefinition(MatterDataModel.ParseHex(E.GetAttribute("i")), E.GetAttribute("n"), E.GetAttribute("p"),
				FieldDefinition.ParseFields(E));
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name + " (0x" + this.Id.ToString("X2") + ")";
		}
	}

	/// <summary>
	/// Base class for named data type definitions.
	/// </summary>
	public abstract class DataTypeDefinition
	{
		/// <summary>
		/// Base class for named data type definitions.
		/// </summary>
		/// <param name="Name">Type name.</param>
		/// <param name="Clusters">Clusters in which the type is defined. Empty array means the type is global.</param>
		public DataTypeDefinition(string Name, uint[] Clusters)
		{
			this.Name = Name;
			this.Clusters = Clusters;
		}

		/// <summary>
		/// Type name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Clusters in which the type is defined. Empty array means the type is global.
		/// </summary>
		public uint[] Clusters { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}

	/// <summary>
	/// Definition of an enumeration.
	/// </summary>
	public class EnumDefinition : DataTypeDefinition
	{
		private readonly Dictionary<long, string> names = new Dictionary<long, string>();
		private readonly Dictionary<string, long> values = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
		private readonly List<KeyValuePair<long, string>> items = new List<KeyValuePair<long, string>>();

		/// <summary>
		/// Definition of an enumeration.
		/// </summary>
		/// <param name="Name">Type name.</param>
		/// <param name="BaseType">Base type.</param>
		/// <param name="Clusters">Clusters in which the type is defined.</param>
		public EnumDefinition(string Name, string BaseType, uint[] Clusters)
			: base(Name, Clusters)
		{
			this.BaseType = BaseType;
		}

		/// <summary>
		/// Base type.
		/// </summary>
		public string BaseType { get; }

		/// <summary>
		/// Enumeration items.
		/// </summary>
		public IReadOnlyList<KeyValuePair<long, string>> Items => this.items;

		/// <summary>
		/// Adds an item to the enumeration.
		/// </summary>
		/// <param name="Value">Value.</param>
		/// <param name="Name">Name.</param>
		public void Add(long Value, string Name)
		{
			this.items.Add(new KeyValuePair<long, string>(Value, Name));
			this.names[Value] = Name;
			this.values[Name] = Value;
		}

		/// <summary>
		/// Tries to get the name of an enumeration value.
		/// </summary>
		/// <param name="Value">Value.</param>
		/// <param name="Name">Name, if found.</param>
		/// <returns>If a name was found.</returns>
		public bool TryGetName(long Value, out string Name)
		{
			return this.names.TryGetValue(Value, out Name);
		}

		/// <summary>
		/// Tries to get the value of a named enumeration item.
		/// </summary>
		/// <param name="Name">Name.</param>
		/// <param name="Value">Value, if found.</param>
		/// <returns>If the name was found.</returns>
		public bool TryGetValue(string Name, out long Value)
		{
			return this.values.TryGetValue(Name, out Value);
		}

		internal static EnumDefinition Parse(XmlElement E)
		{
			EnumDefinition Result = new EnumDefinition(E.GetAttribute("n"), E.GetAttribute("t"), MatterDataModel.ParseHexList(E.GetAttribute("c")));

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2 && E2.LocalName == "V")
					Result.Add(long.Parse(E2.GetAttribute("v")), E2.GetAttribute("n"));
			}

			return Result;
		}
	}

	/// <summary>
	/// Definition of a bitmap.
	/// </summary>
	public class BitmapDefinition : DataTypeDefinition
	{
		private readonly List<KeyValuePair<ulong, string>> fields = new List<KeyValuePair<ulong, string>>();

		/// <summary>
		/// Definition of a bitmap.
		/// </summary>
		/// <param name="Name">Type name.</param>
		/// <param name="BaseType">Base type.</param>
		/// <param name="Clusters">Clusters in which the type is defined.</param>
		public BitmapDefinition(string Name, string BaseType, uint[] Clusters)
			: base(Name, Clusters)
		{
			this.BaseType = BaseType;
		}

		/// <summary>
		/// Base type.
		/// </summary>
		public string BaseType { get; }

		/// <summary>
		/// Bitmap fields (mask, name).
		/// </summary>
		public IReadOnlyList<KeyValuePair<ulong, string>> Fields => this.fields;

		/// <summary>
		/// Adds a field to the bitmap.
		/// </summary>
		/// <param name="Mask">Bit mask.</param>
		/// <param name="Name">Field name.</param>
		public void Add(ulong Mask, string Name)
		{
			this.fields.Add(new KeyValuePair<ulong, string>(Mask, Name));
		}

		/// <summary>
		/// Converts a bitmap value to a string.
		/// </summary>
		/// <param name="Value">Value.</param>
		/// <returns>String representation.</returns>
		public string ToString(ulong Value)
		{
			StringBuilder sb = new StringBuilder();
			ulong Rest = Value;

			foreach (KeyValuePair<ulong, string> Field in this.fields)
			{
				ulong Mask = Field.Key;
				ulong Masked = Value & Mask;

				if (Masked == 0)
					continue;

				Rest &= ~Mask;

				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append(Field.Value);

				if (!IsSingleBit(Mask))
				{
					int Shift = 0;

					while (((Mask >> Shift) & 1) == 0)
						Shift++;

					sb.Append('=');
					sb.Append((Masked >> Shift).ToString());
				}
			}

			if (Rest != 0)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				sb.Append("0x");
				sb.Append(Rest.ToString("X"));
			}

			if (sb.Length == 0)
				return "-";
			else
				return sb.ToString();
		}

		private static bool IsSingleBit(ulong Mask)
		{
			return Mask != 0 && (Mask & (Mask - 1)) == 0;
		}

		internal static BitmapDefinition Parse(XmlElement E)
		{
			BitmapDefinition Result = new BitmapDefinition(E.GetAttribute("n"), E.GetAttribute("t"), MatterDataModel.ParseHexList(E.GetAttribute("c")));

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2 && E2.LocalName == "M")
					Result.Add(ulong.Parse(E2.GetAttribute("m"), NumberStyles.HexNumber), E2.GetAttribute("n"));
			}

			return Result;
		}
	}

	/// <summary>
	/// Definition of a structure.
	/// </summary>
	public class StructDefinition : DataTypeDefinition
	{
		/// <summary>
		/// Definition of a structure.
		/// </summary>
		/// <param name="Name">Type name.</param>
		/// <param name="Clusters">Clusters in which the type is defined.</param>
		/// <param name="Fields">Fields.</param>
		public StructDefinition(string Name, uint[] Clusters, FieldDefinition[] Fields)
			: base(Name, Clusters)
		{
			this.Fields = Fields;
		}

		/// <summary>
		/// Structure fields.
		/// </summary>
		public FieldDefinition[] Fields { get; }

		/// <summary>
		/// Gets a field definition.
		/// </summary>
		/// <param name="FieldId">Field ID.</param>
		/// <returns>Field definition, or null if not found.</returns>
		public FieldDefinition GetField(int FieldId)
		{
			foreach (FieldDefinition Field in this.Fields)
			{
				if (Field.Id == FieldId)
					return Field;
			}

			return null;
		}

		internal static StructDefinition Parse(XmlElement E)
		{
			return new StructDefinition(E.GetAttribute("n"), MatterDataModel.ParseHexList(E.GetAttribute("c")),
				FieldDefinition.ParseFields(E));
		}
	}

	/// <summary>
	/// Definition of a device type.
	/// </summary>
	public class DeviceTypeDefinition
	{
		/// <summary>
		/// Definition of a device type.
		/// </summary>
		/// <param name="Id">Device type ID.</param>
		/// <param name="Name">Device type name.</param>
		/// <param name="Class">Device type class (Node, Utility, Simple).</param>
		public DeviceTypeDefinition(uint Id, string Name, string Class)
		{
			this.Id = Id;
			this.Name = Name;
			this.Class = Class;
		}

		/// <summary>
		/// Device type ID.
		/// </summary>
		public uint Id { get; }

		/// <summary>
		/// Device type name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Device type class (Node, Utility, Simple).
		/// </summary>
		public string Class { get; }

		/// <summary>
		/// If the device type is an application device type (and not a utility or node device type).
		/// </summary>
		public bool IsApplication => this.Class == "Simple";

		internal static DeviceTypeDefinition Parse(XmlElement E)
		{
			return new DeviceTypeDefinition(MatterDataModel.ParseHex(E.GetAttribute("i")), E.GetAttribute("n"), E.GetAttribute("c"));
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name + " (0x" + this.Id.ToString("X4") + ")";
		}
	}
}
