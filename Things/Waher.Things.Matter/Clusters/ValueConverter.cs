using System;
using System.Globalization;
using System.Text;
using Waher.Things.Matter.DataModel;
using Waher.Things.Matter.Tlv;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Kind of value, as interpreted from Matter data types.
	/// </summary>
	public enum ValueKind
	{
		/// <summary>
		/// Unknown or unsupported type.
		/// </summary>
		Unknown,

		/// <summary>
		/// Boolean value.
		/// </summary>
		Boolean,

		/// <summary>
		/// Unsigned integer.
		/// </summary>
		UnsignedInteger,

		/// <summary>
		/// Signed integer.
		/// </summary>
		SignedInteger,

		/// <summary>
		/// Single precision floating-point number.
		/// </summary>
		Single,

		/// <summary>
		/// Double precision floating-point number.
		/// </summary>
		Double,

		/// <summary>
		/// Character string.
		/// </summary>
		String,

		/// <summary>
		/// Octet string.
		/// </summary>
		Bytes,

		/// <summary>
		/// Enumeration.
		/// </summary>
		Enum,

		/// <summary>
		/// Bitmap.
		/// </summary>
		Bitmap,

		/// <summary>
		/// Structure.
		/// </summary>
		Struct,

		/// <summary>
		/// List.
		/// </summary>
		List,

		/// <summary>
		/// Temperature, in 0.01 °C.
		/// </summary>
		Temperature,

		/// <summary>
		/// Percent (0-100).
		/// </summary>
		Percent,

		/// <summary>
		/// Percent in hundredths (0-10000).
		/// </summary>
		Percent100ths,

		/// <summary>
		/// Elapsed time in seconds.
		/// </summary>
		ElapsedSeconds,

		/// <summary>
		/// Seconds since the Matter epoch.
		/// </summary>
		EpochSeconds,

		/// <summary>
		/// Microseconds since the Matter epoch.
		/// </summary>
		EpochMicroseconds,

		/// <summary>
		/// Milliseconds since the UNIX epoch.
		/// </summary>
		PosixMilliseconds,

		/// <summary>
		/// Power in mW.
		/// </summary>
		PowerMilliwatts,

		/// <summary>
		/// Current in mA.
		/// </summary>
		CurrentMilliamperes,

		/// <summary>
		/// Voltage in mV.
		/// </summary>
		VoltageMillivolts,

		/// <summary>
		/// Energy in mWh.
		/// </summary>
		EnergyMilliwattHours
	}

	/// <summary>
	/// Converts Matter values to sensor data fields and control parameter values.
	/// </summary>
	public static class ValueConverter
	{
		/// <summary>
		/// Classifies a Matter data type.
		/// </summary>
		/// <param name="ClusterId">Cluster ID (for resolving named types).</param>
		/// <param name="TypeName">Type name.</param>
		/// <returns>Kind of value.</returns>
		public static ValueKind Classify(uint ClusterId, string TypeName)
		{
			if (string.IsNullOrEmpty(TypeName))
				return ValueKind.Unknown;

			switch (TypeName)
			{
				case "boolean":
					return ValueKind.Boolean;

				case "int8u":
				case "int16u":
				case "int24u":
				case "int32u":
				case "int40u":
				case "int48u":
				case "int56u":
				case "int64u":
				case "vendor_id":
				case "group_id":
				case "endpoint_no":
				case "fabric_idx":
				case "entry_idx":
				case "data_ver":
				case "event_no":
				case "node_id":
				case "subject_id":
				case "fabric_id":
				case "cluster_id":
				case "attrib_id":
				case "field_id":
				case "event_id":
				case "command_id":
				case "action_id":
				case "trans_id":
				case "devtype_id":
				case "status":
				case "systime_us":
				case "systime_ms":
				case "tag":
				case "namespace":
				case "priority":
					return ValueKind.UnsignedInteger;

				case "int8s":
				case "int16s":
				case "int24s":
				case "int32s":
				case "int40s":
				case "int48s":
				case "int56s":
				case "int64s":
				case "money":
					return ValueKind.SignedInteger;

				case "single":
					return ValueKind.Single;

				case "double":
					return ValueKind.Double;

				case "char_string":
				case "long_char_string":
					return ValueKind.String;

				case "octet_string":
				case "long_octet_string":
				case "ipadr":
				case "ipv4adr":
				case "ipv6adr":
				case "ipv6pre":
				case "hwadr":
					return ValueKind.Bytes;

				case "enum8":
				case "enum16":
					return ValueKind.Enum;

				case "bitmap8":
				case "bitmap16":
				case "bitmap24":
				case "bitmap32":
				case "bitmap64":
					return ValueKind.Bitmap;

				case "array":
					return ValueKind.List;

				case "temperature":
					return ValueKind.Temperature;

				case "percent":
					return ValueKind.Percent;

				case "percent100ths":
					return ValueKind.Percent100ths;

				case "elapsed_s":
					return ValueKind.ElapsedSeconds;

				case "epoch_s":
					return ValueKind.EpochSeconds;

				case "epoch_us":
					return ValueKind.EpochMicroseconds;

				case "posix_ms":
					return ValueKind.PosixMilliseconds;

				case "power_mw":
					return ValueKind.PowerMilliwatts;

				case "amperage_ma":
					return ValueKind.CurrentMilliamperes;

				case "voltage_mv":
					return ValueKind.VoltageMillivolts;

				case "energy_mwh":
					return ValueKind.EnergyMilliwattHours;
			}

			DataTypeDefinition Definition = MatterDataModel.FindDataType(ClusterId, TypeName);

			switch (Definition)
			{
				case EnumDefinition _:
					return ValueKind.Enum;

				case BitmapDefinition _:
					return ValueKind.Bitmap;

				case StructDefinition _:
					return ValueKind.Struct;

				default:
					return ValueKind.Unknown;
			}
		}

		/// <summary>
		/// If the kind of value is unsigned.
		/// </summary>
		/// <param name="Kind">Kind of value.</param>
		/// <returns>If unsigned.</returns>
		public static bool IsUnsigned(ValueKind Kind)
		{
			switch (Kind)
			{
				case ValueKind.UnsignedInteger:
				case ValueKind.Enum:
				case ValueKind.Bitmap:
				case ValueKind.Percent:
				case ValueKind.Percent100ths:
				case ValueKind.ElapsedSeconds:
				case ValueKind.EpochSeconds:
				case ValueKind.EpochMicroseconds:
				case ValueKind.PosixMilliseconds:
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// If the kind of value is a scalar value that can be represented as a sensor data field.
		/// </summary>
		/// <param name="Kind">Kind of value.</param>
		/// <returns>If value is scalar.</returns>
		public static bool IsScalar(ValueKind Kind)
		{
			switch (Kind)
			{
				case ValueKind.Unknown:
				case ValueKind.Struct:
				case ValueKind.List:
				case ValueKind.Bytes:
					return false;

				default:
					return true;
			}
		}

		/// <summary>
		/// Gets the range of values of an integer type.
		/// </summary>
		/// <param name="TypeName">Type name.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <returns>If range could be determined.</returns>
		public static bool TryGetIntegerRange(string TypeName, out long Min, out long Max)
		{
			switch (TypeName)
			{
				case "int8u":
				case "enum8":
				case "bitmap8":
				case "fabric_idx":
					Min = 0;
					Max = byte.MaxValue - 1;
					return true;

				case "int16u":
				case "enum16":
				case "bitmap16":
				case "vendor_id":
				case "group_id":
				case "endpoint_no":
					Min = 0;
					Max = ushort.MaxValue - 1;
					return true;

				case "int24u":
					Min = 0;
					Max = 0xfffffe;
					return true;

				case "int32u":
				case "bitmap32":
				case "elapsed_s":
				case "epoch_s":
					Min = 0;
					Max = uint.MaxValue - 1;
					return true;

				case "int8s":
					Min = sbyte.MinValue + 1;
					Max = sbyte.MaxValue;
					return true;

				case "int16s":
				case "temperature":
					Min = short.MinValue + 1;
					Max = short.MaxValue;
					return true;

				case "int24s":
					Min = -0x7fffff;
					Max = 0x7fffff;
					return true;

				case "int32s":
					Min = int.MinValue + 1;
					Max = int.MaxValue;
					return true;

				case "percent":
					Min = 0;
					Max = 100;
					return true;

				case "percent100ths":
					Min = 0;
					Max = 10000;
					return true;

				default:
					Min = long.MinValue + 1;
					Max = long.MaxValue;
					return false;
			}
		}

		/// <summary>
		/// Converts a value to a .NET object, with a type suitable for TLV encoding, given the Matter data type.
		/// </summary>
		/// <param name="Kind">Kind of value.</param>
		/// <param name="Value">Value</param>
		/// <returns>Encodable value.</returns>
		public static object ToTlvValue(ValueKind Kind, object Value)
		{
			if (Value is null)
				return null;

			switch (Kind)
			{
				case ValueKind.Boolean:
					return Convert.ToBoolean(Value);

				case ValueKind.Single:
					return Convert.ToSingle(Value);

				case ValueKind.Double:
					return Convert.ToDouble(Value);

				case ValueKind.String:
					return Value.ToString();

				case ValueKind.Temperature:
					return (long)Math.Round(Convert.ToDouble(Value) * 100);

				case ValueKind.Percent100ths:
					return (ulong)Math.Round(Convert.ToDouble(Value) * 100);

				case ValueKind.PowerMilliwatts:
				case ValueKind.CurrentMilliamperes:
				case ValueKind.VoltageMillivolts:
					return (long)Math.Round(Convert.ToDouble(Value) * 1000);

				case ValueKind.EnergyMilliwattHours:
					return (long)Math.Round(Convert.ToDouble(Value) * 1000000);

				case ValueKind.SignedInteger:
					return Convert.ToInt64(Value);

				default:
					if (IsUnsigned(Kind))
						return Convert.ToUInt64(Value);
					else
						return Value;
			}
		}

		/// <summary>
		/// Adds a sensor data field for an attribute value, using a generic mapping based on the data type of the attribute.
		/// </summary>
		/// <param name="Fields">Field builder.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="Name">Field name.</param>
		/// <param name="TypeName">Data type name.</param>
		/// <param name="Value">Value.</param>
		/// <param name="Type">Field type.</param>
		/// <param name="Writable">If the field is writable.</param>
		/// <returns>If a field was added.</returns>
		public static bool AddField(FieldBuilder Fields, uint ClusterId, string Name, string TypeName, TlvElement Value,
			FieldType Type, bool Writable)
		{
			if (Value is null || Value.IsNull)
				return false;

			ValueKind Kind = Classify(ClusterId, TypeName);

			if (Kind == ValueKind.Unknown)
			{
				switch (Value.Type)
				{
					case TlvValueType.Boolean: Kind = ValueKind.Boolean; break;
					case TlvValueType.UnsignedInteger: Kind = ValueKind.UnsignedInteger; break;
					case TlvValueType.SignedInteger: Kind = ValueKind.SignedInteger; break;
					case TlvValueType.Float: Kind = ValueKind.Single; break;
					case TlvValueType.Double: Kind = ValueKind.Double; break;
					case TlvValueType.String: Kind = ValueKind.String; break;
					default: return false;
				}
			}

			try
			{
				switch (Kind)
				{
					case ValueKind.Boolean:
						Fields.Boolean(Name, Value.AsBoolean(), Type, Writable);
						return true;

					case ValueKind.UnsignedInteger:
					case ValueKind.SignedInteger:
						if (!Value.IsInteger)
							return false;

						if (Value.Type == TlvValueType.UnsignedInteger && Value.AsUInt64() > long.MaxValue)
							Fields.String(Name, Value.AsUInt64().ToString(), Type, Writable);
						else
							Fields.Integer(Name, Value.AsInt64(), Type, Writable);
						return true;

					case ValueKind.Single:
					case ValueKind.Double:
						double d = Value.AsDouble();
						Fields.Quantity(Name, d, GetNrDecimals(d), string.Empty, Type, Writable);
						return true;

					case ValueKind.String:
						Fields.String(Name, Value.AsString(), Type, Writable);
						return true;

					case ValueKind.Enum:
						if (!Value.IsInteger)
							return false;

						long l = Value.AsInt64();
						EnumDefinition Enum = MatterDataModel.FindEnum(ClusterId, TypeName);

						if (!(Enum is null) && Enum.TryGetName(l, out string EnumName))
							Fields.String(Name, MatterDataModel.SpaceName(EnumName), Type, Writable);
						else
							Fields.Integer(Name, l, Type, Writable);
						return true;

					case ValueKind.Bitmap:
						if (!Value.IsInteger)
							return false;

						BitmapDefinition Bitmap = MatterDataModel.FindBitmap(ClusterId, TypeName);

						if (Bitmap is null)
							Fields.Integer(Name, Value.AsInt64(), Type, Writable);
						else
							Fields.String(Name, Bitmap.ToString(Value.AsUInt64()), Type, Writable);
						return true;

					case ValueKind.Temperature:
						Fields.Quantity(Name, Value.AsInt64() * 0.01, 2, "°C", Type, Writable);
						return true;

					case ValueKind.Percent:
						Fields.Quantity(Name, Value.AsDouble(), 0, "%", Type, Writable);
						return true;

					case ValueKind.Percent100ths:
						Fields.Quantity(Name, Value.AsDouble() * 0.01, 2, "%", Type, Writable);
						return true;

					case ValueKind.ElapsedSeconds:
						Fields.Quantity(Name, Value.AsDouble(), 0, "s", Type, Writable);
						return true;

					case ValueKind.EpochSeconds:
						Fields.DateTime(Name, MatterDataModel.MatterEpoch.AddSeconds(Value.AsUInt64()), Type, Writable);
						return true;

					case ValueKind.EpochMicroseconds:
						Fields.DateTime(Name, MatterDataModel.MatterEpoch.AddTicks((long)Value.AsUInt64() * 10), Type, Writable);
						return true;

					case ValueKind.PosixMilliseconds:
						Fields.DateTime(Name, UnixEpoch.AddMilliseconds(Value.AsUInt64()), Type, Writable);
						return true;

					case ValueKind.PowerMilliwatts:
						Fields.Quantity(Name, Value.AsInt64() * 0.001, 3, "W", Type, Writable);
						return true;

					case ValueKind.CurrentMilliamperes:
						Fields.Quantity(Name, Value.AsInt64() * 0.001, 3, "A", Type, Writable);
						return true;

					case ValueKind.VoltageMillivolts:
						Fields.Quantity(Name, Value.AsInt64() * 0.001, 3, "V", Type, Writable);
						return true;

					case ValueKind.EnergyMilliwattHours:
						Fields.Quantity(Name, Value.AsInt64() * 1e-6, 6, "kWh", Type, Writable);
						return true;

					default:
						return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// UNIX epoch (1970-01-01T00:00:00Z).
		/// </summary>
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Gets a suitable number of decimals to present a floating-point value.
		/// </summary>
		/// <param name="Value">Value.</param>
		/// <returns>Number of decimals.</returns>
		public static byte GetNrDecimals(double Value)
		{
			double Abs = Math.Abs(Value);

			if (Abs == 0)
				return 0;
			else if (Abs >= 1000)
				return 0;
			else if (Abs >= 100)
				return 1;
			else if (Abs >= 10)
				return 2;
			else if (Abs >= 1)
				return 3;
			else
				return (byte)Math.Min(10, 3 - (int)Math.Floor(Math.Log10(Abs)));
		}

		/// <summary>
		/// Formats event data for presentation.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="Event">Event definition, if known.</param>
		/// <param name="Data">Event data.</param>
		/// <returns>Formatted event data.</returns>
		public static string FormatEventData(uint ClusterId, EventDefinition Event, TlvElement Data)
		{
			if (Data is null || Data.IsNull)
				return string.Empty;

			if (!Data.IsStructure)
				return MatterDataModel.FormatValue(ClusterId, null, null, Data);

			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (TlvElement Member in Data.Children)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");

				FieldDefinition Field = Member.Tag.IsContextSpecific ? Event?.GetField((int)Member.Tag.Number) : null;

				sb.Append(MatterDataModel.SpaceName(Field?.Name) ?? Member.Tag.Number.ToString(CultureInfo.InvariantCulture));
				sb.Append(": ");

				if (Field is null)
					sb.Append(MatterDataModel.FormatValue(ClusterId, null, null, Member));
				else if (Field.IsArray)
					sb.Append(MatterDataModel.FormatValue(ClusterId, Field.Type, Field.Type, Member));
				else
					sb.Append(MatterDataModel.FormatValue(ClusterId, Field.Type, null, Member));
			}

			return sb.ToString();
		}
	}
}
