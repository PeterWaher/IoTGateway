using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// How missing fields are to be treated in an index search.
	/// </summary>
	public enum MissingFieldAction
	{
		/// <summary>
		/// Missing fields are not allowed.
		/// </summary>
		Prohibit,

		/// <summary>
		/// Missing fields will be considered to have the NULL value.
		/// </summary>
		Null,

		/// <summary>
		/// Missing fields will be considered to have the first value allowed, depending on type and sort order.
		/// </summary>
		First,

		/// <summary>
		/// Missing fields will be considered to have the last value allowed, depending on type and sort order.
		/// </summary>
		Last
	}

	/// <summary>
	/// Handles index storage of object references.
	/// </summary>
	public class IndexRecords : IRecordHandler, IComparer<byte[]>
	{
		private IndexBTreeFile index;
		private readonly string[] fieldNames;
		private readonly int fieldCount;
		private readonly bool[] ascending;
		private readonly string collectionName;
		private Guid objectId = Guid.Empty;
		private readonly Encoding encoding;
		private readonly int keySizeLimit;

		/// <summary>
		/// Handles index storage of object references.
		/// </summary>
		/// <param name="CollectionName">Name of current collection.</param>
		/// <param name="Encoding">Encoding to use for text.</param>
		/// <param name="KeySizeLimit">Upper size limit of index keys.</param>
		/// <param name="FieldNames">Field names included in the index. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		public IndexRecords(string CollectionName, Encoding Encoding, int KeySizeLimit, params string[] FieldNames)
		{
			this.collectionName = CollectionName;
			this.encoding = Encoding;
			this.fieldNames = FieldNames;
			this.fieldCount = FieldNames.Length;
			this.keySizeLimit = KeySizeLimit;

			int i;

			this.ascending = new bool[this.fieldCount];

			for (i = 0; i < this.fieldCount; i++)
			{
				if (this.fieldNames[i].StartsWith("-"))
				{
					this.fieldNames[i] = this.fieldNames[i].Substring(1);
					this.ascending[i] = false;
				}
				else
				{
					this.ascending[i] = true;

					if (this.fieldNames[i].StartsWith("+"))
						this.fieldNames[i] = this.fieldNames[i].Substring(1);
				}
			}
		}

		/// <summary>
		/// Field names included in the index.
		/// </summary>
		public string[] FieldNames => this.fieldNames;

		/// <summary>
		/// If the corresponding field name is sorted in ascending order (true) or descending order (false).
		/// </summary>
		public bool[] Ascending => this.ascending;

		/// <summary>
		/// Index file.
		/// </summary>
		internal IndexBTreeFile Index
		{
			get => this.index;
			set => this.index = value;
		}

		/// <summary>
		/// Serializes the index key for a given object and the underlying index fields.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Object">Object</param>
		/// <param name="Serializer">Serializer.</param>
		/// <param name="MissingFields">How missing fields are to be treated.</param>
		/// <returns>Serialized index, if object can be indexed using the current index, or null otherwise.</returns>
		public async Task<byte[]> Serialize(Guid ObjectId, object Object, IObjectSerializer Serializer, MissingFieldAction MissingFields)
		{
			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
			int i;

			Writer.WriteBit(true);

			for (i = 0; i < this.fieldCount; i++)
			{
				object Value = await Serializer.TryGetFieldValue(this.fieldNames[i], Object);

				if (Value is null)
				{
					switch (MissingFields)
					{
						case MissingFieldAction.Null:
							Value = null;
							break;

						case MissingFieldAction.First:
							if (this.ascending[i])
								Writer.WriteBits((byte)ObjectSerializer.TYPE_MIN, 6);
							else
								Writer.WriteBits((byte)ObjectSerializer.TYPE_MAX, 6);
							continue;

						case MissingFieldAction.Last:
							if (this.ascending[i])
								Writer.WriteBits((byte)ObjectSerializer.TYPE_MAX, 6);
							else
								Writer.WriteBits((byte)ObjectSerializer.TYPE_MIN, 6);
							continue;

						case MissingFieldAction.Prohibit:
						default:
							return null;
					}
				}

				if (!this.Serialize(Writer, Value))
					return null;
			}

			Writer.Write(ObjectId);

			return Writer.GetSerialization();
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="Value">Value to serialize.</param>
		/// <returns>If the value could be serialized.</returns>
		public bool Serialize(BinarySerializer Writer, object Value)
		{
			uint TypeCode;

			if (Value is null)
				TypeCode = ObjectSerializer.TYPE_NULL;
			else
				TypeCode = ObjectSerializer.GetFieldDataTypeCode(Value.GetType());

			switch (TypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteBit((bool)Value);
					break;

				case ObjectSerializer.TYPE_BYTE:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((byte)Value);
					break;

				case ObjectSerializer.TYPE_INT16:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((short)Value);
					break;

				case ObjectSerializer.TYPE_VARINT16:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteVariableLengthInt16((short)Value);
					break;

				case ObjectSerializer.TYPE_INT32:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((int)Value);
					break;

				case ObjectSerializer.TYPE_VARINT32:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteVariableLengthInt32((int)Value);
					break;

				case ObjectSerializer.TYPE_INT64:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((long)Value);
					break;

				case ObjectSerializer.TYPE_VARINT64:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteVariableLengthInt64((long)Value);
					break;

				case ObjectSerializer.TYPE_SBYTE:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((sbyte)Value);
					break;

				case ObjectSerializer.TYPE_UINT16:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((ushort)Value);
					break;

				case ObjectSerializer.TYPE_VARUINT16:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteVariableLengthUInt16((ushort)Value);
					break;

				case ObjectSerializer.TYPE_UINT32:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((uint)Value);
					break;

				case ObjectSerializer.TYPE_VARUINT32:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteVariableLengthUInt32((uint)Value);
					break;

				case ObjectSerializer.TYPE_UINT64:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((ulong)Value);
					break;

				case ObjectSerializer.TYPE_VARUINT64:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.WriteVariableLengthUInt64((ulong)Value);
					break;

				case ObjectSerializer.TYPE_DECIMAL:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((decimal)Value);
					break;

				case ObjectSerializer.TYPE_DOUBLE:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((double)Value);
					break;

				case ObjectSerializer.TYPE_SINGLE:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((float)Value);
					break;

				case ObjectSerializer.TYPE_DATETIME:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((DateTime)Value);
					break;

				case ObjectSerializer.TYPE_DATETIMEOFFSET:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((DateTimeOffset)Value);
					break;

				case ObjectSerializer.TYPE_TIMESPAN:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((TimeSpan)Value);
					break;

				case ObjectSerializer.TYPE_CHAR:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((char)Value);
					break;

				case ObjectSerializer.TYPE_STRING:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((string)Value);
					break;

				case ObjectSerializer.TYPE_CI_STRING:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write(((CaseInsensitiveString)Value).Value);
					break;

				case ObjectSerializer.TYPE_ENUM:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((Enum)Value);
					break;

				case ObjectSerializer.TYPE_GUID:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((Guid)Value);
					break;

				case ObjectSerializer.TYPE_NULL:
					Writer.WriteBits((byte)TypeCode, 6);
					break;

				case ObjectSerializer.TYPE_BYTEARRAY:
					Writer.WriteBits((byte)TypeCode, 6);
					Writer.Write((byte[])Value);
					break;

				case ObjectSerializer.TYPE_OBJECT:
					Writer.WriteBits((byte)ObjectSerializer.TYPE_STRING, 6);
					Writer.Write(Value?.ToString() ?? string.Empty);
					break;

				case ObjectSerializer.TYPE_ARRAY:
				default:
					return false;
			}

			return true;
		}

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.
		///		Value Meaning Less than zero x is less than y.
		///		Zero x equals y. 
		///		Greater than zero x is greater than y.</returns>
		public int Compare(object x, object y)
		{
			byte[] xBytes = (byte[])x;
			byte[] yBytes = (byte[])y;

			return this.Compare(xBytes, yBytes);
		}

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.
		///		Value Meaning Less than zero x is less than y.
		///		Zero x equals y. 
		///		Greater than zero x is greater than y.</returns>
		public int Compare(byte[] x, byte[] y)
		{
			if (x is null)
			{
				if (y is null)
					return 0;
				else
					return -1;
			}
			else if (y is null)
				return 1;
			else
			{
				uint BlockLimit = this.index?.BlockLimit ?? uint.MaxValue;
				BinaryDeserializer xReader = new BinaryDeserializer(this.collectionName, this.encoding, x, BlockLimit);
				BinaryDeserializer yReader = new BinaryDeserializer(this.collectionName, this.encoding, y, BlockLimit);
				uint xType, yType;
				int i, j;
				long l;
				bool xExists = xReader.ReadBit();
				bool yExists = yReader.ReadBit();
				bool Ascending;

				if (xExists ^ yExists)
				{
					if (xExists)
						return 1;
					else
						return -1;
				}
				else if (!xExists)
					return 0;

				for (i = 0; i < this.fieldCount; i++)
				{
					Ascending = this.ascending[i];
					xType = xReader.ReadBits(6);
					yType = yReader.ReadBits(6);

					switch (xType)
					{
						case ObjectSerializer.TYPE_BOOLEAN:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadBit().CompareTo(yReader.ReadBit());
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((byte)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((short)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((short)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((sbyte)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((ushort)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((ushort)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((uint)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = ((xReader.ReadBit() ? 1 : 0).ToString()).CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadBit()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_BYTE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadByte().CompareTo((byte)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadByte().CompareTo(yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((short)xReader.ReadByte()).CompareTo(yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((short)xReader.ReadByte()).CompareTo(yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadByte()).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadByte()).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadByte()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadByte()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((short)xReader.ReadByte()).CompareTo((short)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((ushort)xReader.ReadByte()).CompareTo(yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((ushort)xReader.ReadByte()).CompareTo(yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)xReader.ReadByte()).CompareTo(yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((uint)xReader.ReadByte()).CompareTo(yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadByte()).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)xReader.ReadByte()).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadByte()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadByte()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadByte()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadByte()).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadByte().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadByte()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_INT16:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadInt16().CompareTo((short)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadInt16().CompareTo((short)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadInt16().CompareTo(yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadInt16().CompareTo(yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadInt16()).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadInt16()).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadInt16()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadInt16()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadInt16().CompareTo((short)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadInt16()).CompareTo((int)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((int)xReader.ReadInt16()).CompareTo((int)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadInt16()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((long)xReader.ReadInt16()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadInt16();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadInt16();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadInt16()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadInt16()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadInt16()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadInt16()).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadInt16().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadInt16()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_VARINT16:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadVariableLengthInt16().CompareTo((short)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadVariableLengthInt16().CompareTo((short)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadVariableLengthInt16().CompareTo(yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadVariableLengthInt16().CompareTo(yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadVariableLengthInt16().CompareTo((short)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadVariableLengthInt16()).CompareTo((int)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((int)xReader.ReadVariableLengthInt16()).CompareTo((int)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadVariableLengthInt16()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((long)xReader.ReadVariableLengthInt16()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadVariableLengthInt16();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadVariableLengthInt16();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadVariableLengthInt16()).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadVariableLengthInt16().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadVariableLengthInt16()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_INT32:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadInt32().CompareTo((int)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadInt32().CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadInt32().CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadInt32()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadInt32()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadInt32()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((long)xReader.ReadInt32()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadInt32();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadInt32();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadInt32()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadInt32()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadInt32()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadInt32().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadInt32()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_VARINT32:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadVariableLengthInt32().CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadVariableLengthInt32().CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadVariableLengthInt32()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadVariableLengthInt32()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadVariableLengthInt32()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((long)xReader.ReadVariableLengthInt32()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadVariableLengthInt32();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadVariableLengthInt32();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadVariableLengthInt32()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadVariableLengthInt32()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadVariableLengthInt32()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadVariableLengthInt32().CompareTo((int)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadVariableLengthInt32().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadVariableLengthInt32()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_INT64:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadInt64().CompareTo((long)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadInt64().CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = xReader.ReadInt64().CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = (xReader.ReadInt64()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = (xReader.ReadInt64()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadInt64();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadInt64();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadInt64()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadInt64()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadInt64()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadInt64().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadInt64()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_VARINT64:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadVariableLengthInt64().CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = xReader.ReadVariableLengthInt64().CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = (xReader.ReadVariableLengthInt64()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = (xReader.ReadVariableLengthInt64()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadVariableLengthInt64();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadVariableLengthInt64();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadVariableLengthInt64()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadVariableLengthInt64()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadVariableLengthInt64()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadVariableLengthInt64().CompareTo((long)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadVariableLengthInt64().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadVariableLengthInt64()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_SBYTE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadSByte().CompareTo((sbyte)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((short)xReader.ReadSByte()).CompareTo((short)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((short)xReader.ReadSByte()).CompareTo(yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((short)xReader.ReadSByte()).CompareTo(yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadSByte()).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadSByte()).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadSByte()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadSByte()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((short)xReader.ReadSByte()).CompareTo((short)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadSByte()).CompareTo((int)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((int)xReader.ReadSByte()).CompareTo((int)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadSByte()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((long)xReader.ReadSByte()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadSByte();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									l = xReader.ReadSByte();
									if (l < 0)
										j = -1;
									else
										j = ((ulong)l).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadSByte()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadSByte()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadSByte()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadSByte()).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadSByte().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadSByte()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_UINT16:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadUInt16().CompareTo((ushort)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadUInt16().CompareTo((ushort)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((int)xReader.ReadUInt16()).CompareTo((int)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((int)xReader.ReadUInt16()).CompareTo((int)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadUInt16()).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadUInt16()).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadUInt16()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadUInt16()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((int)xReader.ReadUInt16()).CompareTo((int)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadUInt16().CompareTo(yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadUInt16().CompareTo(yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)xReader.ReadUInt16()).CompareTo(yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((uint)xReader.ReadUInt16()).CompareTo(yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadUInt16()).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)xReader.ReadUInt16()).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadUInt16()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadUInt16()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadUInt16()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadUInt16()).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadUInt16().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadUInt16()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_VARUINT16:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadVariableLengthUInt16().CompareTo((ushort)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadVariableLengthUInt16().CompareTo((ushort)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((int)xReader.ReadVariableLengthUInt16()).CompareTo((int)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((int)xReader.ReadVariableLengthUInt16()).CompareTo((int)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((int)xReader.ReadVariableLengthUInt16()).CompareTo((int)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadVariableLengthUInt16().CompareTo(yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadVariableLengthUInt16().CompareTo(yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((uint)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadVariableLengthUInt16()).CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadVariableLengthUInt16().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadVariableLengthUInt16()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_UINT32:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadUInt32().CompareTo((uint)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									l = yReader.ReadInt16();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_VARINT16:
									l = yReader.ReadVariableLengthInt16();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_INT32:
									l = yReader.ReadInt32();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_VARINT32:
									l = yReader.ReadVariableLengthInt32();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_INT64:
									l = yReader.ReadInt64();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_VARINT64:
									l = yReader.ReadVariableLengthInt64();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_SBYTE:
									l = yReader.ReadSByte();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadUInt32().CompareTo(yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadUInt32().CompareTo(yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadUInt32()).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)xReader.ReadUInt32()).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadUInt32()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadUInt32()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadUInt32()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadUInt32().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadUInt32()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_VARUINT32:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadVariableLengthUInt32().CompareTo((uint)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadVariableLengthUInt32().CompareTo((uint)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									l = yReader.ReadInt16();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_VARINT16:
									l = yReader.ReadVariableLengthInt16();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_INT32:
									l = yReader.ReadInt32();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_VARINT32:
									l = yReader.ReadVariableLengthInt32();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_INT64:
									l = yReader.ReadInt64();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_VARINT64:
									l = yReader.ReadVariableLengthInt64();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_SBYTE:
									l = yReader.ReadSByte();
									if (l < 0)
										j = 1;
									else
										j = ((long)xReader.ReadVariableLengthUInt32()).CompareTo(l);
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadVariableLengthUInt32().CompareTo((uint)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadVariableLengthUInt32().CompareTo((uint)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadVariableLengthUInt32().CompareTo(yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadVariableLengthUInt32().CompareTo(yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadVariableLengthUInt32()).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)xReader.ReadVariableLengthUInt32()).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadVariableLengthUInt32()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadVariableLengthUInt32()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadVariableLengthUInt32()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadVariableLengthUInt32().CompareTo((uint)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadVariableLengthUInt32().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadVariableLengthUInt32()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_UINT64:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadUInt64().CompareTo((ulong)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									l = yReader.ReadInt16();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_VARINT16:
									l = yReader.ReadVariableLengthInt16();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_INT32:
									l = yReader.ReadInt32();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_VARINT32:
									l = yReader.ReadVariableLengthInt32();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_INT64:
									l = yReader.ReadInt64();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_VARINT64:
									l = yReader.ReadVariableLengthInt64();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_SBYTE:
									l = yReader.ReadSByte();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadUInt64().CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = xReader.ReadUInt64().CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadUInt64()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadUInt64()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadUInt64()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadUInt64().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadUInt64()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_VARUINT64:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									l = yReader.ReadInt16();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_VARINT16:
									l = yReader.ReadVariableLengthInt16();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_INT32:
									l = yReader.ReadInt32();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_VARINT32:
									l = yReader.ReadVariableLengthInt32();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_INT64:
									l = yReader.ReadInt64();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_VARINT64:
									l = yReader.ReadVariableLengthInt64();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_SBYTE:
									l = yReader.ReadSByte();
									if (l < 0)
										j = 1;
									else
										j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)l);
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadVariableLengthUInt64().CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = xReader.ReadVariableLengthUInt64().CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadVariableLengthUInt64()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadVariableLengthUInt64()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadVariableLengthUInt64()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadVariableLengthUInt64().CompareTo((ulong)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadVariableLengthUInt64().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadVariableLengthUInt64()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_DECIMAL:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadDecimal().CompareTo((decimal)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = xReader.ReadDecimal().CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDecimal().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_DOUBLE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadDouble().CompareTo((double)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadDouble()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = xReader.ReadDouble().CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDouble().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadDouble()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_SINGLE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadSingle().CompareTo((float)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadSingle()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadSingle()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadSingle().CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadSingle().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadSingle()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_DATETIME:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
								case ObjectSerializer.TYPE_BYTE:
								case ObjectSerializer.TYPE_INT16:
								case ObjectSerializer.TYPE_INT32:
								case ObjectSerializer.TYPE_INT64:
								case ObjectSerializer.TYPE_SBYTE:
								case ObjectSerializer.TYPE_UINT16:
								case ObjectSerializer.TYPE_UINT32:
								case ObjectSerializer.TYPE_UINT64:
								case ObjectSerializer.TYPE_VARINT16:
								case ObjectSerializer.TYPE_VARINT32:
								case ObjectSerializer.TYPE_VARINT64:
								case ObjectSerializer.TYPE_VARUINT16:
								case ObjectSerializer.TYPE_VARUINT32:
								case ObjectSerializer.TYPE_VARUINT64:
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDateTime().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_DATETIME:
									j = xReader.ReadDateTime().ToUniversalTime().CompareTo(yReader.ReadDateTime().ToUniversalTime());
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = xReader.ReadDateTime().ToUniversalTime().CompareTo(yReader.ReadDateTimeOffset().ToUniversalTime().DateTime);
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_DATETIMEOFFSET:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
								case ObjectSerializer.TYPE_BYTE:
								case ObjectSerializer.TYPE_INT16:
								case ObjectSerializer.TYPE_INT32:
								case ObjectSerializer.TYPE_INT64:
								case ObjectSerializer.TYPE_SBYTE:
								case ObjectSerializer.TYPE_UINT16:
								case ObjectSerializer.TYPE_UINT32:
								case ObjectSerializer.TYPE_UINT64:
								case ObjectSerializer.TYPE_VARINT16:
								case ObjectSerializer.TYPE_VARINT32:
								case ObjectSerializer.TYPE_VARINT64:
								case ObjectSerializer.TYPE_VARUINT16:
								case ObjectSerializer.TYPE_VARUINT32:
								case ObjectSerializer.TYPE_VARUINT64:
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDateTimeOffset().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_DATETIME:
									j = xReader.ReadDateTimeOffset().ToUniversalTime().CompareTo((DateTimeOffset)(yReader.ReadDateTime().ToUniversalTime()));
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = xReader.ReadDateTimeOffset().ToUniversalTime().CompareTo(yReader.ReadDateTimeOffset().ToUniversalTime());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_TIMESPAN:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
								case ObjectSerializer.TYPE_BYTE:
								case ObjectSerializer.TYPE_INT16:
								case ObjectSerializer.TYPE_INT32:
								case ObjectSerializer.TYPE_INT64:
								case ObjectSerializer.TYPE_SBYTE:
								case ObjectSerializer.TYPE_UINT16:
								case ObjectSerializer.TYPE_UINT32:
								case ObjectSerializer.TYPE_UINT64:
								case ObjectSerializer.TYPE_VARINT16:
								case ObjectSerializer.TYPE_VARINT32:
								case ObjectSerializer.TYPE_VARINT64:
								case ObjectSerializer.TYPE_VARUINT16:
								case ObjectSerializer.TYPE_VARUINT32:
								case ObjectSerializer.TYPE_VARUINT64:
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_CI_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadTimeSpan().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
									j = xReader.ReadTimeSpan().CompareTo(yReader.ReadTimeSpan());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_CHAR:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = ((int)xReader.ReadChar()).CompareTo((int)(yReader.ReadBit() ? 1 : 0));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadByte());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadInt16());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadVariableLengthInt16());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadInt32());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadVariableLengthInt32());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadChar()).CompareTo(yReader.ReadInt64());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((long)xReader.ReadChar()).CompareTo(yReader.ReadVariableLengthInt64());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadSByte());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadUInt16());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadVariableLengthUInt16());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadChar()).CompareTo((long)yReader.ReadUInt32());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((long)xReader.ReadChar()).CompareTo((long)yReader.ReadVariableLengthUInt32());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadChar()).CompareTo(yReader.ReadUInt64());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((ulong)xReader.ReadChar()).CompareTo(yReader.ReadVariableLengthUInt64());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadChar()).CompareTo(yReader.ReadDecimal());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadChar()).CompareTo(yReader.ReadDouble());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadChar()).CompareTo(yReader.ReadSingle());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadChar().CompareTo(yReader.ReadChar());
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadChar().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_CI_STRING:
									j = ((CaseInsensitiveString)xReader.ReadChar().ToString()).CompareTo((CaseInsensitiveString)yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(BitConverter.GetBytes(xReader.ReadChar()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_STRING:
						case ObjectSerializer.TYPE_ENUM:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadString().CompareTo((yReader.ReadBit() ? 1 : 0).ToString());
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadString().CompareTo(yReader.ReadByte().ToString());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadString().CompareTo(yReader.ReadInt16().ToString());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = xReader.ReadString().CompareTo(yReader.ReadVariableLengthInt16().ToString());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadString().CompareTo(yReader.ReadInt32().ToString());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = xReader.ReadString().CompareTo(yReader.ReadVariableLengthInt32().ToString());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadString().CompareTo(yReader.ReadInt64().ToString());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = xReader.ReadString().CompareTo(yReader.ReadVariableLengthInt64().ToString());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadString().CompareTo(yReader.ReadSByte().ToString());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadString().CompareTo(yReader.ReadUInt16().ToString());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = xReader.ReadString().CompareTo(yReader.ReadVariableLengthUInt16().ToString());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadString().CompareTo(yReader.ReadUInt32().ToString());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = xReader.ReadString().CompareTo(yReader.ReadVariableLengthUInt32().ToString());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadString().CompareTo(yReader.ReadUInt64().ToString());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = xReader.ReadString().CompareTo(yReader.ReadVariableLengthUInt64().ToString());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = xReader.ReadString().CompareTo(yReader.ReadDecimal().ToString());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = xReader.ReadString().CompareTo(yReader.ReadDouble().ToString());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadString().CompareTo(yReader.ReadSingle().ToString());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadString().CompareTo(yReader.ReadChar().ToString());
									break;

								case ObjectSerializer.TYPE_STRING:
									j = xReader.ReadString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_DATETIME:
									j = xReader.ReadString().CompareTo(yReader.ReadDateTime().ToString());
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = xReader.ReadString().CompareTo(yReader.ReadDateTimeOffset().ToString());
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
									j = xReader.ReadString().CompareTo(yReader.ReadTimeSpan().ToString());
									break;

								case ObjectSerializer.TYPE_GUID:
									j = xReader.ReadString().CompareTo(yReader.ReadGuid().ToString());
									break;

								case ObjectSerializer.TYPE_CI_STRING:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(this.encoding.GetBytes(xReader.ReadString()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_CI_STRING:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)(yReader.ReadBit() ? 1 : 0).ToString());
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadByte().ToString());
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadInt16().ToString());
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadVariableLengthInt16().ToString());
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadInt32().ToString());
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadVariableLengthInt32().ToString());
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadInt64().ToString());
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadVariableLengthInt64().ToString());
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadSByte().ToString());
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadUInt16().ToString());
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadVariableLengthUInt16().ToString());
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadUInt32().ToString());
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadVariableLengthUInt32().ToString());
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadUInt64().ToString());
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadVariableLengthUInt64().ToString());
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadDecimal().ToString());
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadDouble().ToString());
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadSingle().ToString());
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadChar().ToString());
									break;

								case ObjectSerializer.TYPE_STRING:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_ENUM:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_DATETIME:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadDateTime().ToString());
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadDateTimeOffset().ToString());
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadTimeSpan().ToString());
									break;

								case ObjectSerializer.TYPE_GUID:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadGuid().ToString());
									break;

								case ObjectSerializer.TYPE_CI_STRING:
									j = ((CaseInsensitiveString)xReader.ReadString()).CompareTo((CaseInsensitiveString)yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(this.encoding.GetBytes(xReader.ReadString()), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_GUID:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
								case ObjectSerializer.TYPE_BYTE:
								case ObjectSerializer.TYPE_INT16:
								case ObjectSerializer.TYPE_INT32:
								case ObjectSerializer.TYPE_INT64:
								case ObjectSerializer.TYPE_SBYTE:
								case ObjectSerializer.TYPE_UINT16:
								case ObjectSerializer.TYPE_UINT32:
								case ObjectSerializer.TYPE_UINT64:
								case ObjectSerializer.TYPE_VARINT16:
								case ObjectSerializer.TYPE_VARINT32:
								case ObjectSerializer.TYPE_VARINT64:
								case ObjectSerializer.TYPE_VARUINT16:
								case ObjectSerializer.TYPE_VARUINT32:
								case ObjectSerializer.TYPE_VARUINT64:
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadGuid().ToString().CompareTo(yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_CI_STRING:
									j = ((CaseInsensitiveString)xReader.ReadGuid().ToString()).CompareTo((CaseInsensitiveString)yReader.ReadString());
									break;

								case ObjectSerializer.TYPE_GUID:
									j = xReader.ReadGuid().CompareTo(yReader.ReadGuid());
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(xReader.ReadGuid().ToByteArray(), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_NULL:
							switch (yType)
							{
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_MAX:
								default:
									j = -1;
									break;

								case ObjectSerializer.TYPE_NULL:
									j = 0;
									break;
							}
							break;

						case ObjectSerializer.TYPE_MIN:
							j = -1;
							break;

						case ObjectSerializer.TYPE_MAX:
							j = 1;
							break;

						case ObjectSerializer.TYPE_BYTEARRAY:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadBit()));
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadByte()));
									break;

								case ObjectSerializer.TYPE_INT16:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadInt16()));
									break;

								case ObjectSerializer.TYPE_VARINT16:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadVariableLengthInt16()));
									break;

								case ObjectSerializer.TYPE_INT32:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadInt32()));
									break;

								case ObjectSerializer.TYPE_VARINT32:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadVariableLengthInt32()));
									break;

								case ObjectSerializer.TYPE_INT64:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadInt64()));
									break;

								case ObjectSerializer.TYPE_VARINT64:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadVariableLengthInt64()));
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadSByte()));
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadUInt16()));
									break;

								case ObjectSerializer.TYPE_VARUINT16:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadVariableLengthUInt16()));
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadUInt32()));
									break;

								case ObjectSerializer.TYPE_VARUINT32:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadVariableLengthUInt32()));
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadUInt64()));
									break;

								case ObjectSerializer.TYPE_VARUINT64:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadVariableLengthUInt64()));
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadDouble()));
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadSingle()));
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = BinaryCompare(xReader.ReadByteArray(), BitConverter.GetBytes(yReader.ReadChar()));
									break;

								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_MIN:
									j = 1;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = BinaryCompare(xReader.ReadByteArray(), this.encoding.GetBytes(yReader.ReadString()));
									break;

								case ObjectSerializer.TYPE_CI_STRING:
									j = BinaryCompare(xReader.ReadByteArray(), this.encoding.GetBytes(yReader.ReadString()));
									break;

								case ObjectSerializer.TYPE_GUID:
									j = BinaryCompare(xReader.ReadByteArray(), yReader.ReadGuid().ToByteArray());
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
									j = BinaryCompare(xReader.ReadByteArray(), yReader.ReadByteArray());
									break;

								case ObjectSerializer.TYPE_MAX:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									j = -1;
									break;
							}
							break;

						case ObjectSerializer.TYPE_ARRAY:
						case ObjectSerializer.TYPE_OBJECT:
						default:
							j = 1;
							break;
					}

					if (j != 0)
						return Ascending ? j : -j;
				}

				return xReader.ReadGuid().CompareTo(yReader.ReadGuid());
			}
		}

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		public Task<uint> GetFullPayloadSize(BinaryDeserializer Reader)
		{
			return Task.FromResult<uint>(0);
		}

		/// <summary>
		/// Gets the key of the next record.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Key object.</returns>
		public object GetKey(BinaryDeserializer Reader)
		{
			int Pos = Reader.Position;

			if (!this.SkipKey(Reader, true))
				return null;

			int Len = Reader.Position - Pos;
			if (Len > this.keySizeLimit)
				return null;

			byte[] Key = new byte[Len];
			Array.Copy(Reader.Data, Pos, Key, 0, Len);

			return Key;
		}

		/// <summary>
		/// Object ID
		/// </summary>
		public Guid ObjectId => this.objectId;

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Payload size.</returns>
		public Task<int> GetPayloadSize(BinaryDeserializer Reader)
		{
			return Task.FromResult<int>(0);
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload, and if the object is a BLOB.</returns>
		public Task<KeyValuePair<int, bool>> GetPayloadSizeEx(BinaryDeserializer Reader)
		{
			return Task.FromResult<KeyValuePair<int, bool>>(new KeyValuePair<int, bool>(0, false));
		}

		/// <summary>
		/// Checks if the following object is a BLOB.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If the following object is a BLOB.</returns>
		public Task<bool> IsBlob(BinaryDeserializer Reader)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Gets the payload type.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Payload type.</returns>
		public string GetPayloadType(BinaryDeserializer Reader)
		{
			return string.Empty;
		}

		/// <summary>
		/// Skips the next key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If a key was skipped.</returns>
		public bool SkipKey(BinaryDeserializer Reader)
		{
			return this.SkipKey(Reader, false);
		}

		internal bool SkipKey(BinaryDeserializer Reader, bool ExtractObjectId)
		{
			if (Reader.BytesLeft == 0 || !Reader.ReadBit())
			{
				if (ExtractObjectId)
					this.objectId = Guid.Empty;

				return false;
			}

			int i;

			for (i = 0; i < this.fieldCount; i++)
			{
				switch ((uint)Reader.ReadBits(6))
				{
					case ObjectSerializer.TYPE_BOOLEAN:
						Reader.SkipBit();
						break;

					case ObjectSerializer.TYPE_BYTE:
						Reader.SkipByte();
						break;

					case ObjectSerializer.TYPE_INT16:
						Reader.SkipInt16();
						break;

					case ObjectSerializer.TYPE_INT32:
						Reader.SkipInt32();
						break;

					case ObjectSerializer.TYPE_INT64:
						Reader.SkipInt64();
						break;

					case ObjectSerializer.TYPE_SBYTE:
						Reader.SkipSByte();
						break;

					case ObjectSerializer.TYPE_UINT16:
						Reader.SkipUInt16();
						break;

					case ObjectSerializer.TYPE_UINT32:
						Reader.SkipUInt32();
						break;

					case ObjectSerializer.TYPE_UINT64:
						Reader.SkipUInt64();
						break;

					case ObjectSerializer.TYPE_VARINT16:
					case ObjectSerializer.TYPE_VARINT32:
					case ObjectSerializer.TYPE_VARINT64:
					case ObjectSerializer.TYPE_VARUINT16:
					case ObjectSerializer.TYPE_VARUINT32:
					case ObjectSerializer.TYPE_VARUINT64:
						Reader.SkipVariableLengthInteger();
						break;

					case ObjectSerializer.TYPE_DECIMAL:
						Reader.SkipDecimal();
						break;

					case ObjectSerializer.TYPE_DOUBLE:
						Reader.SkipDouble();
						break;

					case ObjectSerializer.TYPE_SINGLE:
						Reader.SkipSingle();
						break;

					case ObjectSerializer.TYPE_DATETIME:
						Reader.SkipDateTime();
						break;

					case ObjectSerializer.TYPE_DATETIMEOFFSET:
						Reader.SkipDateTimeOffset();
						break;

					case ObjectSerializer.TYPE_TIMESPAN:
						Reader.SkipTimeSpan();
						break;

					case ObjectSerializer.TYPE_CHAR:
						Reader.SkipChar();
						break;

					case ObjectSerializer.TYPE_STRING:
					case ObjectSerializer.TYPE_CI_STRING:
						Reader.SkipString();
						break;

					case ObjectSerializer.TYPE_ENUM:
						Reader.SkipEnum();
						break;

					case ObjectSerializer.TYPE_BYTEARRAY:
						Reader.SkipByteArray();
						break;

					case ObjectSerializer.TYPE_GUID:
						Reader.SkipGuid();
						break;

					case ObjectSerializer.TYPE_NULL:
					case ObjectSerializer.TYPE_MIN:
					case ObjectSerializer.TYPE_MAX:
						break;

					case ObjectSerializer.TYPE_ARRAY:
					case ObjectSerializer.TYPE_OBJECT:
					default:
						return false;
				}
			}

			if (ExtractObjectId)
				this.objectId = Reader.ReadGuid();
			else
				Reader.SkipGuid();

			return true;
		}

		/// <summary>
		/// Exports a key to XML.
		/// </summary>
		/// <param name="ObjectId">Key to export.</param>
		/// <param name="Output">XML Output.</param>
		public void ExportKey(object ObjectId, XmlWriter Output)
		{
			byte[] Bin = (byte[])ObjectId;
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.index?.BlockLimit ?? uint.MaxValue);

			if (Reader.BytesLeft > 0 && Reader.ReadBit())
			{
				string Value;
				int i;

				for (i = 0; i < this.fieldCount; i++)
				{
					switch ((uint)Reader.ReadBits(6))
					{
						case ObjectSerializer.TYPE_BOOLEAN:
							Value = Reader.ReadBit() ? "true" : "false";
							break;

						case ObjectSerializer.TYPE_BYTE:
							Value = Reader.ReadByte().ToString();
							break;

						case ObjectSerializer.TYPE_INT16:
							Value = Reader.ReadInt16().ToString();
							break;

						case ObjectSerializer.TYPE_VARINT16:
							Value = Reader.ReadVariableLengthInt16().ToString();
							break;

						case ObjectSerializer.TYPE_INT32:
							Value = Reader.ReadInt32().ToString();
							break;

						case ObjectSerializer.TYPE_VARINT32:
							Value = Reader.ReadVariableLengthInt32().ToString();
							break;

						case ObjectSerializer.TYPE_INT64:
							Value = Reader.ReadInt64().ToString();
							break;

						case ObjectSerializer.TYPE_VARINT64:
							Value = Reader.ReadVariableLengthInt64().ToString();
							break;

						case ObjectSerializer.TYPE_SBYTE:
							Value = Reader.ReadSByte().ToString();
							break;

						case ObjectSerializer.TYPE_UINT16:
							Value = Reader.ReadUInt16().ToString();
							break;

						case ObjectSerializer.TYPE_VARUINT16:
							Value = Reader.ReadVariableLengthUInt16().ToString();
							break;

						case ObjectSerializer.TYPE_UINT32:
							Value = Reader.ReadUInt32().ToString();
							break;

						case ObjectSerializer.TYPE_VARUINT32:
							Value = Reader.ReadVariableLengthUInt32().ToString();
							break;

						case ObjectSerializer.TYPE_UINT64:
							Value = Reader.ReadUInt64().ToString();
							break;

						case ObjectSerializer.TYPE_VARUINT64:
							Value = Reader.ReadVariableLengthUInt64().ToString();
							break;

						case ObjectSerializer.TYPE_DECIMAL:
							Value = Reader.ReadDecimal().ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
							break;

						case ObjectSerializer.TYPE_DOUBLE:
							Value = Reader.ReadDouble().ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
							break;

						case ObjectSerializer.TYPE_SINGLE:
							Value = Reader.ReadSingle().ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
							break;

						case ObjectSerializer.TYPE_DATETIME:
							Value = Reader.ReadDateTime().ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "z";
							break;

						case ObjectSerializer.TYPE_DATETIMEOFFSET:
							Value = Reader.ReadDateTimeOffset().ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss") + "z";
							break;

						case ObjectSerializer.TYPE_TIMESPAN:
							Value = Reader.ReadTimeSpan().ToString();
							break;

						case ObjectSerializer.TYPE_CHAR:
							Value = Reader.ReadChar().ToString();
							break;

						case ObjectSerializer.TYPE_STRING:
						case ObjectSerializer.TYPE_CI_STRING:
						case ObjectSerializer.TYPE_ENUM:
							Value = Reader.ReadString();
							break;

						case ObjectSerializer.TYPE_BYTEARRAY:
							Value = Convert.ToBase64String(Reader.ReadByteArray());
							break;

						case ObjectSerializer.TYPE_GUID:
							Value = Reader.ReadGuid().ToString();
							break;

						case ObjectSerializer.TYPE_NULL:
							Value = string.Empty;
							break;

						case ObjectSerializer.TYPE_MIN:
							Value = "MIN";
							break;

						case ObjectSerializer.TYPE_MAX:
							Value = "MAX";
							break;

						case ObjectSerializer.TYPE_ARRAY:
						case ObjectSerializer.TYPE_OBJECT:
						default:
							return;
					}

					Output.WriteAttributeString(this.fieldNames[i], Value);
				}

				Output.WriteAttributeString("objectId", Reader.ReadGuid().ToString());
			}
		}

		/// <summary>
		/// If the index ordering corresponds to a given sort order.
		/// </summary>
		/// <param name="ConstantFields">Optional array of names of fields that will be constant during the enumeration.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool SameSortOrder(string[] ConstantFields, string[] SortOrder)
		{
			if (SortOrder is null)
				return true;

			int SortLen = SortOrder.Length;
			if (SortLen == 0)
				return true;

			if (this.fieldCount < SortLen)
				return false;

			string s, s2;
			int FieldIndex = 0;
			int SortIndex;
			int NrConstantsFound = 0;
			bool Ascending;

			for (SortIndex = 0; SortIndex < SortLen; SortIndex++)
			{
				s = SortOrder[SortIndex];
				if (s.StartsWith("-"))
				{
					Ascending = false;
					s = s.Substring(1);
				}
				else
				{
					Ascending = true;

					if (s.StartsWith("+"))
						s = s.Substring(1);
				}

				while (FieldIndex < this.fieldCount)
				{
					s2 = this.fieldNames[FieldIndex];

					if (s == s2)
						break;
					else if (ConstantFields is null || Array.IndexOf(ConstantFields, s2) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						FieldIndex++;
					}
				}

				if (FieldIndex >= this.fieldCount)
					return false;

				if (Ascending != this.ascending[FieldIndex])
					return false;

				FieldIndex++;
			}

			return true;
		}

		/// <summary>
		/// If the index ordering is a reversion of a given sort order.
		/// </summary>
		/// <param name="ConstantFields">Optional array of names of fields that will be constant during the enumeration.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If the index matches the sort order. (The index ordering is allowed to be more specific.)</returns>
		public bool ReverseSortOrder(string[] ConstantFields, string[] SortOrder)
		{
			if (SortOrder is null)
				return false;

			int SortLen = SortOrder.Length;
			if (SortLen == 0)
				return true;

			if (this.fieldCount < SortLen)
				return false;

			string s, s2;
			int FieldIndex = 0;
			int SortIndex;
			int NrConstantsFound = 0;
			bool Ascending;

			for (SortIndex = 0; SortIndex < SortLen; SortIndex++)
			{
				s = SortOrder[SortIndex];
				if (s.StartsWith("-"))
				{
					Ascending = false;
					s = s.Substring(1);
				}
				else
				{
					Ascending = true;

					if (s.StartsWith("+"))
						s = s.Substring(1);
				}

				while (FieldIndex < this.fieldCount)
				{
					s2 = this.fieldNames[FieldIndex];

					if (s == s2)
						break;
					else if (ConstantFields is null || Array.IndexOf(ConstantFields, s2) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						FieldIndex++;
					}
				}

				if (FieldIndex >= this.fieldCount)
					return false;

				if (Ascending == this.ascending[FieldIndex])
					return false;

				FieldIndex++;
			}

			return true;
		}

		internal static int BinaryCompare(byte[] b1, byte[] b2)
		{
			int c1 = b1.Length;
			int c2 = b2.Length;
			int c = Math.Min(c1, c2);
			int i, j;

			for (i = 0; i < c; i++)
			{
				j = b1[i] - b2[i];
				if (j != 0)
					return j;
			}

			return c1 - c2;
		}

		/// <summary>
		/// Encodes a BLOB reference.
		/// </summary>
		/// <param name="BlobReference">Binary BLOB reference.</param>
		/// <param name="BlobData">Original BLOB data.</param>
		/// <returns>Encoded BLOB reference.</returns>
		public byte[] EncodeBlobReference(byte[] BlobReference, byte[] BlobData)
		{
			return BlobReference;
		}

	}
}
