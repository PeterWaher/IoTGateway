using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Waher.Things.Matter.Tlv
{
	/// <summary>
	/// Represents a decoded TLV element (Matter Core Specification, Appendix A).
	/// </summary>
	public class TlvElement
	{
		private TlvTag tag;
		private readonly TlvValueType type;
		private readonly object value;
		private readonly List<TlvElement> children;

		/// <summary>
		/// Represents a decoded TLV element.
		/// </summary>
		/// <param name="Tag">Tag of element.</param>
		/// <param name="Type">Value type.</param>
		/// <param name="Value">Value (not for containers).</param>
		public TlvElement(TlvTag Tag, TlvValueType Type, object Value)
		{
			this.tag = Tag;
			this.type = Type;

			switch (Type)
			{
				case TlvValueType.Structure:
				case TlvValueType.Array:
				case TlvValueType.List:
					this.children = Value as List<TlvElement> ?? new List<TlvElement>();
					this.value = null;
					break;

				default:
					this.children = null;
					this.value = Value;
					break;
			}
		}

		#region Factory methods

		/// <summary>
		/// Creates a signed integer element.
		/// </summary>
		public static TlvElement Int(TlvTag Tag, long Value) => new TlvElement(Tag, TlvValueType.SignedInteger, Value);

		/// <summary>
		/// Creates an unsigned integer element.
		/// </summary>
		public static TlvElement UInt(TlvTag Tag, ulong Value) => new TlvElement(Tag, TlvValueType.UnsignedInteger, Value);

		/// <summary>
		/// Creates a boolean element.
		/// </summary>
		public static TlvElement Boolean(TlvTag Tag, bool Value) => new TlvElement(Tag, TlvValueType.Boolean, Value);

		/// <summary>
		/// Creates a single-precision float element.
		/// </summary>
		public static TlvElement Float(TlvTag Tag, float Value) => new TlvElement(Tag, TlvValueType.Float, Value);

		/// <summary>
		/// Creates a double-precision float element.
		/// </summary>
		public static TlvElement Double(TlvTag Tag, double Value) => new TlvElement(Tag, TlvValueType.Double, Value);

		/// <summary>
		/// Creates a UTF-8 string element.
		/// </summary>
		public static TlvElement String(TlvTag Tag, string Value) => new TlvElement(Tag, TlvValueType.String, Value ?? string.Empty);

		/// <summary>
		/// Creates an octet string element.
		/// </summary>
		public static TlvElement Bytes(TlvTag Tag, byte[] Value) => new TlvElement(Tag, TlvValueType.ByteString, Value ?? System.Array.Empty<byte>());

		/// <summary>
		/// Creates a null element.
		/// </summary>
		public static TlvElement Null(TlvTag Tag) => new TlvElement(Tag, TlvValueType.Null, null);

		/// <summary>
		/// Creates a structure element.
		/// </summary>
		public static TlvElement Structure(TlvTag Tag, params TlvElement[] Members)
		{
			return new TlvElement(Tag, TlvValueType.Structure, new List<TlvElement>(Members ?? System.Array.Empty<TlvElement>()));
		}

		/// <summary>
		/// Creates an array element.
		/// </summary>
		public static TlvElement Array(TlvTag Tag, params TlvElement[] Members)
		{
			return new TlvElement(Tag, TlvValueType.Array, new List<TlvElement>(Members ?? System.Array.Empty<TlvElement>()));
		}

		/// <summary>
		/// Creates a list element.
		/// </summary>
		public static TlvElement List(TlvTag Tag, params TlvElement[] Members)
		{
			return new TlvElement(Tag, TlvValueType.List, new List<TlvElement>(Members ?? System.Array.Empty<TlvElement>()));
		}

		/// <summary>
		/// Creates an element from a .NET value.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <param name="Value">Value.</param>
		/// <returns>TLV element.</returns>
		public static TlvElement FromObject(TlvTag Tag, object Value)
		{
			switch (Value)
			{
				case null: return Null(Tag);
				case TlvElement E:
					TlvElement Copy = E.Clone();
					Copy.Tag = Tag;
					return Copy;
				case bool b: return Boolean(Tag, b);
				case byte ui8: return UInt(Tag, ui8);
				case ushort ui16: return UInt(Tag, ui16);
				case uint ui32: return UInt(Tag, ui32);
				case ulong ui64: return UInt(Tag, ui64);
				case sbyte i8: return Int(Tag, i8);
				case short i16: return Int(Tag, i16);
				case int i32: return Int(Tag, i32);
				case long i64: return Int(Tag, i64);
				case float f: return Float(Tag, f);
				case double d: return Double(Tag, d);
				case string s: return String(Tag, s);
				case byte[] Bin: return Bytes(Tag, Bin);
				case Enum e:
					Type T = Enum.GetUnderlyingType(e.GetType());
					if (T == typeof(long) || T == typeof(int) || T == typeof(short) || T == typeof(sbyte))
						return Int(Tag, Convert.ToInt64(e));
					else
						return UInt(Tag, Convert.ToUInt64(e));
				default:
					throw new ArgumentException("Unable to represent values of type " + Value.GetType().FullName + " as TLV.", nameof(Value));
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Tag of element.
		/// </summary>
		public TlvTag Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// Value type.
		/// </summary>
		public TlvValueType Type => this.type;

		/// <summary>
		/// Primitive value (null for containers and null values).
		/// </summary>
		public object Value => this.value;

		/// <summary>
		/// If element is a container (structure, array or list).
		/// </summary>
		public bool IsContainer => !(this.children is null);

		/// <summary>
		/// If element is a null value.
		/// </summary>
		public bool IsNull => this.type == TlvValueType.Null;

		/// <summary>
		/// If element is a structure.
		/// </summary>
		public bool IsStructure => this.type == TlvValueType.Structure;

		/// <summary>
		/// If element is an array.
		/// </summary>
		public bool IsArray => this.type == TlvValueType.Array;

		/// <summary>
		/// If element is a list.
		/// </summary>
		public bool IsList => this.type == TlvValueType.List;

		/// <summary>
		/// Child elements (for containers). Empty for primitive elements.
		/// </summary>
		public IReadOnlyList<TlvElement> Children => (IReadOnlyList<TlvElement>)this.children ?? System.Array.Empty<TlvElement>();

		/// <summary>
		/// Number of children.
		/// </summary>
		public int Count => this.children?.Count ?? 0;

		/// <summary>
		/// Adds a child element to a container.
		/// </summary>
		/// <param name="Child">Child element.</param>
		/// <returns>The current element.</returns>
		public TlvElement Add(TlvElement Child)
		{
			if (this.children is null)
				throw new InvalidOperationException("Element is not a container.");

			this.children.Add(Child);
			return this;
		}

		/// <summary>
		/// Gets a member of a container by context-specific tag.
		/// </summary>
		/// <param name="ContextTag">Context-specific tag.</param>
		/// <returns>Member, or null if not found.</returns>
		public TlvElement this[int ContextTag]
		{
			get
			{
				if (!(this.children is null))
				{
					foreach (TlvElement Child in this.children)
					{
						if (Child.tag.IsContext(ContextTag))
							return Child;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Tries to get a member of a container by context-specific tag.
		/// </summary>
		/// <param name="ContextTag">Context-specific tag.</param>
		/// <param name="Member">Member, if found.</param>
		/// <returns>If member was found.</returns>
		public bool TryGet(int ContextTag, out TlvElement Member)
		{
			Member = this[ContextTag];
			return !(Member is null);
		}

		/// <summary>
		/// Gets a member of a container by tag.
		/// </summary>
		/// <param name="Tag">Tag.</param>
		/// <returns>Member, or null if not found.</returns>
		public TlvElement GetMember(TlvTag Tag)
		{
			if (!(this.children is null))
			{
				foreach (TlvElement Child in this.children)
				{
					if (Child.tag == Tag)
						return Child;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets a required member, throwing an exception if not found.
		/// </summary>
		/// <param name="ContextTag">Context-specific tag.</param>
		/// <returns>Member.</returns>
		public TlvElement Required(int ContextTag)
		{
			return this[ContextTag] ?? throw new MatterDecodingException("Required TLV member with tag " + ContextTag.ToString() + " missing.");
		}

		#endregion

		#region Value accessors

		/// <summary>
		/// If the element contains an integer value.
		/// </summary>
		public bool IsInteger => this.type == TlvValueType.SignedInteger || this.type == TlvValueType.UnsignedInteger;

		/// <summary>
		/// Gets the value as an unsigned 64-bit integer.
		/// </summary>
		public ulong AsUInt64()
		{
			switch (this.type)
			{
				case TlvValueType.UnsignedInteger: return (ulong)this.value;
				case TlvValueType.SignedInteger: return unchecked((ulong)(long)this.value);
				case TlvValueType.Boolean: return (bool)this.value ? 1UL : 0UL;
				default: throw new MatterDecodingException("TLV element is not an integer (" + this.type.ToString() + ").");
			}
		}

		/// <summary>
		/// Gets the value as a signed 64-bit integer.
		/// </summary>
		public long AsInt64()
		{
			switch (this.type)
			{
				case TlvValueType.SignedInteger: return (long)this.value;
				case TlvValueType.UnsignedInteger: return unchecked((long)(ulong)this.value);
				case TlvValueType.Boolean: return (bool)this.value ? 1L : 0L;
				default: throw new MatterDecodingException("TLV element is not an integer (" + this.type.ToString() + ").");
			}
		}

		/// <summary>
		/// Gets the value as an unsigned 32-bit integer.
		/// </summary>
		public uint AsUInt32() => (uint)this.AsUInt64();

		/// <summary>
		/// Gets the value as an unsigned 16-bit integer.
		/// </summary>
		public ushort AsUInt16() => (ushort)this.AsUInt64();

		/// <summary>
		/// Gets the value as an unsigned 8-bit integer.
		/// </summary>
		public byte AsByte() => (byte)this.AsUInt64();

		/// <summary>
		/// Gets the value as a signed 32-bit integer.
		/// </summary>
		public int AsInt32() => (int)this.AsInt64();

		/// <summary>
		/// Gets the value as a boolean.
		/// </summary>
		public bool AsBoolean()
		{
			switch (this.type)
			{
				case TlvValueType.Boolean: return (bool)this.value;
				case TlvValueType.UnsignedInteger: return (ulong)this.value != 0;
				case TlvValueType.SignedInteger: return (long)this.value != 0;
				default: throw new MatterDecodingException("TLV element is not a boolean (" + this.type.ToString() + ").");
			}
		}

		/// <summary>
		/// Gets the value as a double precision floating point number.
		/// </summary>
		public double AsDouble()
		{
			switch (this.type)
			{
				case TlvValueType.Double: return (double)this.value;
				case TlvValueType.Float: return (float)this.value;
				case TlvValueType.SignedInteger: return (long)this.value;
				case TlvValueType.UnsignedInteger: return (ulong)this.value;
				default: throw new MatterDecodingException("TLV element is not numeric (" + this.type.ToString() + ").");
			}
		}

		/// <summary>
		/// Gets the value as a string.
		/// </summary>
		public string AsString()
		{
			if (this.type == TlvValueType.String)
				return (string)this.value;
			else if (this.type == TlvValueType.ByteString)
				return Encoding.UTF8.GetString((byte[])this.value);
			else
				throw new MatterDecodingException("TLV element is not a string (" + this.type.ToString() + ").");
		}

		/// <summary>
		/// Gets the value as a byte array.
		/// </summary>
		public byte[] AsBytes()
		{
			if (this.type == TlvValueType.ByteString)
				return (byte[])this.value;
			else if (this.type == TlvValueType.String)
				return Encoding.UTF8.GetBytes((string)this.value);
			else
				throw new MatterDecodingException("TLV element is not an octet string (" + this.type.ToString() + ").");
		}

		/// <summary>
		/// Gets the value as a .NET object (primitives), or null.
		/// </summary>
		public object AsObject()
		{
			if (!(this.children is null))
			{
				if (this.type == TlvValueType.Structure)
				{
					Dictionary<string, object> Result = new Dictionary<string, object>();
					foreach (TlvElement Child in this.children)
						Result[Child.tag.ToString()] = Child.AsObject();
					return Result;
				}
				else
				{
					object[] Result = new object[this.children.Count];
					int i = 0;
					foreach (TlvElement Child in this.children)
						Result[i++] = Child.AsObject();
					return Result;
				}
			}
			else
				return this.value;
		}

		#endregion

		#region Encoding

		/// <summary>
		/// Encodes the element, using a TLV writer.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		public void Encode(TlvWriter Output)
		{
			switch (this.type)
			{
				case TlvValueType.SignedInteger: Output.WriteInt(this.tag, (long)this.value); break;
				case TlvValueType.UnsignedInteger: Output.WriteUInt(this.tag, (ulong)this.value); break;
				case TlvValueType.Boolean: Output.WriteBoolean(this.tag, (bool)this.value); break;
				case TlvValueType.Float: Output.WriteFloat(this.tag, (float)this.value); break;
				case TlvValueType.Double: Output.WriteDouble(this.tag, (double)this.value); break;
				case TlvValueType.String: Output.WriteString(this.tag, (string)this.value); break;
				case TlvValueType.ByteString: Output.WriteBytes(this.tag, (byte[])this.value); break;
				case TlvValueType.Null: Output.WriteNull(this.tag); break;

				case TlvValueType.Structure:
					Output.StartStructure(this.tag);
					foreach (TlvElement Child in this.children)
						Child.Encode(Output);
					Output.EndContainer();
					break;

				case TlvValueType.Array:
					Output.StartArray(this.tag);
					foreach (TlvElement Child in this.children)
						Child.Encode(Output);
					Output.EndContainer();
					break;

				case TlvValueType.List:
					Output.StartList(this.tag);
					foreach (TlvElement Child in this.children)
						Child.Encode(Output);
					Output.EndContainer();
					break;
			}
		}

		/// <summary>
		/// Encodes the element.
		/// </summary>
		/// <returns>Encoded element.</returns>
		public byte[] Encode()
		{
			TlvWriter Output = new TlvWriter();
			this.Encode(Output);
			return Output.ToArray();
		}

		/// <summary>
		/// Creates a deep copy of the element.
		/// </summary>
		/// <returns>Copy of element.</returns>
		public TlvElement Clone()
		{
			if (this.children is null)
				return new TlvElement(this.tag, this.type, this.value);

			List<TlvElement> Children = new List<TlvElement>(this.children.Count);
			foreach (TlvElement Child in this.children)
				Children.Add(Child.Clone());

			return new TlvElement(this.tag, this.type, Children);
		}

		#endregion

		#region Decoding

		/// <summary>
		/// Decodes a single TLV element.
		/// </summary>
		/// <param name="Data">Encoded data.</param>
		/// <returns>Decoded element.</returns>
		public static TlvElement Decode(byte[] Data)
		{
			return Decode(Data, 0, Data.Length);
		}

		/// <summary>
		/// Decodes a single TLV element.
		/// </summary>
		/// <param name="Data">Encoded data.</param>
		/// <param name="Offset">Offset into buffer.</param>
		/// <param name="Count">Number of bytes available.</param>
		/// <returns>Decoded element.</returns>
		public static TlvElement Decode(byte[] Data, int Offset, int Count)
		{
			int Pos = Offset;
			int End = Offset + Count;
			TlvElement Result = DecodeElement(Data, ref Pos, End, 0);
			if (Result is null)
				throw new MatterDecodingException("Unexpected end-of-container in TLV data.");

			return Result;
		}

		/// <summary>
		/// Decodes a sequence of TLV elements.
		/// </summary>
		/// <param name="Data">Encoded data.</param>
		/// <returns>Decoded elements.</returns>
		public static TlvElement[] DecodeAll(byte[] Data)
		{
			List<TlvElement> Result = new List<TlvElement>();
			int Pos = 0;
			int End = Data.Length;

			while (Pos < End)
			{
				TlvElement E = DecodeElement(Data, ref Pos, End, 0);
				if (E is null)
					throw new MatterDecodingException("Unexpected end-of-container in TLV data.");
				Result.Add(E);
			}

			return Result.ToArray();
		}

		private static ulong ReadLittleEndian(byte[] Data, ref int Pos, int End, int NrBytes)
		{
			if (Pos + NrBytes > End)
				throw new MatterDecodingException("Unexpected end of TLV data.");

			ulong Result = 0;
			int i;

			for (i = NrBytes - 1; i >= 0; i--)
			{
				Result <<= 8;
				Result |= Data[Pos + i];
			}

			Pos += NrBytes;
			return Result;
		}

		private static TlvElement DecodeElement(byte[] Data, ref int Pos, int End, int Depth)
		{
			if (Pos >= End)
				throw new MatterDecodingException("Unexpected end of TLV data.");

			if (Depth > 32)
				throw new MatterDecodingException("TLV nesting too deep.");

			byte Control = Data[Pos++];
			TlvElementType ElementType = (TlvElementType)(Control & 0x1f);
			TlvTagControl TagControl = (TlvTagControl)(Control >> 5);

			if (ElementType == TlvElementType.EndOfContainer)
			{
				if (TagControl != TlvTagControl.Anonymous)
					throw new MatterDecodingException("Invalid end-of-container control octet.");

				return null;
			}

			if ((byte)ElementType > (byte)TlvElementType.EndOfContainer)
				throw new MatterDecodingException("Invalid TLV element type: 0x" + ((byte)ElementType).ToString("X2"));

			TlvTag Tag;

			switch (TagControl)
			{
				case TlvTagControl.Anonymous:
					Tag = TlvTag.Anonymous;
					break;

				case TlvTagControl.ContextSpecific:
					Tag = new TlvTag(TagControl, (uint)ReadLittleEndian(Data, ref Pos, End, 1), 0, 0);
					break;

				case TlvTagControl.CommonProfile2:
				case TlvTagControl.ImplicitProfile2:
					Tag = new TlvTag(TagControl, (uint)ReadLittleEndian(Data, ref Pos, End, 2), 0, 0);
					break;

				case TlvTagControl.CommonProfile4:
				case TlvTagControl.ImplicitProfile4:
					Tag = new TlvTag(TagControl, (uint)ReadLittleEndian(Data, ref Pos, End, 4), 0, 0);
					break;

				case TlvTagControl.FullyQualified6:
					ushort Vendor = (ushort)ReadLittleEndian(Data, ref Pos, End, 2);
					ushort Profile = (ushort)ReadLittleEndian(Data, ref Pos, End, 2);
					Tag = new TlvTag(TagControl, (uint)ReadLittleEndian(Data, ref Pos, End, 2), Vendor, Profile);
					break;

				case TlvTagControl.FullyQualified8:
				default:
					Vendor = (ushort)ReadLittleEndian(Data, ref Pos, End, 2);
					Profile = (ushort)ReadLittleEndian(Data, ref Pos, End, 2);
					Tag = new TlvTag(TagControl, (uint)ReadLittleEndian(Data, ref Pos, End, 4), Vendor, Profile);
					break;
			}

			switch (ElementType)
			{
				case TlvElementType.SignedInt8:
					return new TlvElement(Tag, TlvValueType.SignedInteger, (long)(sbyte)ReadLittleEndian(Data, ref Pos, End, 1));

				case TlvElementType.SignedInt16:
					return new TlvElement(Tag, TlvValueType.SignedInteger, (long)(short)ReadLittleEndian(Data, ref Pos, End, 2));

				case TlvElementType.SignedInt32:
					return new TlvElement(Tag, TlvValueType.SignedInteger, (long)(int)ReadLittleEndian(Data, ref Pos, End, 4));

				case TlvElementType.SignedInt64:
					return new TlvElement(Tag, TlvValueType.SignedInteger, (long)ReadLittleEndian(Data, ref Pos, End, 8));

				case TlvElementType.UnsignedInt8:
					return new TlvElement(Tag, TlvValueType.UnsignedInteger, ReadLittleEndian(Data, ref Pos, End, 1));

				case TlvElementType.UnsignedInt16:
					return new TlvElement(Tag, TlvValueType.UnsignedInteger, ReadLittleEndian(Data, ref Pos, End, 2));

				case TlvElementType.UnsignedInt32:
					return new TlvElement(Tag, TlvValueType.UnsignedInteger, ReadLittleEndian(Data, ref Pos, End, 4));

				case TlvElementType.UnsignedInt64:
					return new TlvElement(Tag, TlvValueType.UnsignedInteger, ReadLittleEndian(Data, ref Pos, End, 8));

				case TlvElementType.BooleanFalse:
					return new TlvElement(Tag, TlvValueType.Boolean, false);

				case TlvElementType.BooleanTrue:
					return new TlvElement(Tag, TlvValueType.Boolean, true);

				case TlvElementType.Float:
					if (Pos + 4 > End)
						throw new MatterDecodingException("Unexpected end of TLV data.");

					float f;
					if (BitConverter.IsLittleEndian)
						f = BitConverter.ToSingle(Data, Pos);
					else
					{
						byte[] Tmp = new byte[4];
						System.Array.Copy(Data, Pos, Tmp, 0, 4);
						System.Array.Reverse(Tmp);
						f = BitConverter.ToSingle(Tmp, 0);
					}
					Pos += 4;
					return new TlvElement(Tag, TlvValueType.Float, f);

				case TlvElementType.Double:
					if (Pos + 8 > End)
						throw new MatterDecodingException("Unexpected end of TLV data.");

					double d;
					if (BitConverter.IsLittleEndian)
						d = BitConverter.ToDouble(Data, Pos);
					else
					{
						byte[] Tmp = new byte[8];
						System.Array.Copy(Data, Pos, Tmp, 0, 8);
						System.Array.Reverse(Tmp);
						d = BitConverter.ToDouble(Tmp, 0);
					}
					Pos += 8;
					return new TlvElement(Tag, TlvValueType.Double, d);

				case TlvElementType.Utf8String1:
				case TlvElementType.Utf8String2:
				case TlvElementType.Utf8String4:
				case TlvElementType.Utf8String8:
					int NrLenBytes = 1 << (ElementType - TlvElementType.Utf8String1);
					ulong Len = ReadLittleEndian(Data, ref Pos, End, NrLenBytes);
					if (Len > (ulong)(End - Pos))
						throw new MatterDecodingException("Unexpected end of TLV data.");

					string s = Encoding.UTF8.GetString(Data, Pos, (int)Len);
					Pos += (int)Len;
					return new TlvElement(Tag, TlvValueType.String, s);

				case TlvElementType.ByteString1:
				case TlvElementType.ByteString2:
				case TlvElementType.ByteString4:
				case TlvElementType.ByteString8:
					NrLenBytes = 1 << (ElementType - TlvElementType.ByteString1);
					Len = ReadLittleEndian(Data, ref Pos, End, NrLenBytes);
					if (Len > (ulong)(End - Pos))
						throw new MatterDecodingException("Unexpected end of TLV data.");

					byte[] Bin = new byte[(int)Len];
					System.Array.Copy(Data, Pos, Bin, 0, (int)Len);
					Pos += (int)Len;
					return new TlvElement(Tag, TlvValueType.ByteString, Bin);

				case TlvElementType.Null:
					return new TlvElement(Tag, TlvValueType.Null, null);

				case TlvElementType.Structure:
				case TlvElementType.Array:
				case TlvElementType.List:
					List<TlvElement> Children = new List<TlvElement>();
					TlvElement Child;

					while (!((Child = DecodeElement(Data, ref Pos, End, Depth + 1)) is null))
						Children.Add(Child);

					TlvValueType ContainerType;

					if (ElementType == TlvElementType.Structure)
						ContainerType = TlvValueType.Structure;
					else if (ElementType == TlvElementType.Array)
						ContainerType = TlvValueType.Array;
					else
						ContainerType = TlvValueType.List;

					return new TlvElement(Tag, ContainerType, Children);

				default:
					throw new MatterDecodingException("Unsupported TLV element type.");
			}
		}

		#endregion

		#region String representation

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			this.ToString(sb);
			return sb.ToString();
		}

		/// <summary>
		/// Appends a JSON-like string representation of the element.
		/// </summary>
		/// <param name="Output">Output.</param>
		public void ToString(StringBuilder Output)
		{
			if (!this.tag.IsAnonymous)
			{
				Output.Append(this.tag.ToString());
				Output.Append('=');
			}

			switch (this.type)
			{
				case TlvValueType.SignedInteger:
					Output.Append(((long)this.value).ToString(CultureInfo.InvariantCulture));
					break;

				case TlvValueType.UnsignedInteger:
					Output.Append(((ulong)this.value).ToString(CultureInfo.InvariantCulture));
					Output.Append('U');
					break;

				case TlvValueType.Boolean:
					Output.Append((bool)this.value ? "true" : "false");
					break;

				case TlvValueType.Float:
					Output.Append(((float)this.value).ToString(CultureInfo.InvariantCulture));
					Output.Append('f');
					break;

				case TlvValueType.Double:
					Output.Append(((double)this.value).ToString(CultureInfo.InvariantCulture));
					break;

				case TlvValueType.String:
					Output.Append('"');
					Output.Append(((string)this.value).Replace("\\", "\\\\").Replace("\"", "\\\""));
					Output.Append('"');
					break;

				case TlvValueType.ByteString:
					Output.Append("h'");
					foreach (byte b in (byte[])this.value)
						Output.Append(b.ToString("x2"));
					Output.Append('\'');
					break;

				case TlvValueType.Null:
					Output.Append("null");
					break;

				case TlvValueType.Structure:
				case TlvValueType.Array:
				case TlvValueType.List:
					bool First = true;

					if (this.type == TlvValueType.Structure)
						Output.Append('{');
					else if (this.type == TlvValueType.Array)
						Output.Append('[');
					else
						Output.Append("[[");

					foreach (TlvElement Child in this.children)
					{
						if (First)
							First = false;
						else
							Output.Append(", ");

						Child.ToString(Output);
					}

					if (this.type == TlvValueType.Structure)
						Output.Append('}');
					else if (this.type == TlvValueType.Array)
						Output.Append(']');
					else
						Output.Append("]]");
					break;
			}
		}

		#endregion
	}
}
