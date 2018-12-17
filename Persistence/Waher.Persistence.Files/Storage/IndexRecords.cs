using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Persistence.Files.Serialization;

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
			this.keySizeLimit = KeySizeLimit;

			int i, c = this.fieldNames.Length;

			this.ascending = new bool[c];

			for (i = 0; i < c; i++)
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
		public string[] FieldNames
		{
			get { return this.fieldNames; }
		}

		/// <summary>
		/// If the corresponding field name is sorted in ascending order (true) or descending order (false).
		/// </summary>
		public bool[] Ascending
		{
			get { return this.ascending; }
		}

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
		public byte[] Serialize(Guid ObjectId, object Object, IObjectSerializer Serializer, MissingFieldAction MissingFields)
		{
			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
			int i, c = this.fieldNames.Length;

			Writer.WriteBit(true);

			for (i = 0; i < c; i++)
			{
				if (!Serializer.TryGetFieldValue(this.fieldNames[i], Object, out object Value))
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
			Type T;

			if (Value == null)
			{
				T = null;
				TypeCode = ObjectSerializer.TYPE_NULL;
			}
			else
			{
				T = Value.GetType();
				TypeCode = FilesProvider.GetFieldDataTypeCode(T);
			}

			Writer.WriteBits((byte)TypeCode, 6);

			switch (TypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					Writer.WriteBit((bool)Value);
					break;

				case ObjectSerializer.TYPE_BYTE:
					Writer.Write((byte)Value);
					break;

				case ObjectSerializer.TYPE_INT16:
					Writer.Write((short)Value);
					break;

				case ObjectSerializer.TYPE_INT32:
					Writer.Write((int)Value);
					break;

				case ObjectSerializer.TYPE_INT64:
					Writer.Write((long)Value);
					break;

				case ObjectSerializer.TYPE_SBYTE:
					Writer.Write((sbyte)Value);
					break;

				case ObjectSerializer.TYPE_UINT16:
					Writer.Write((ushort)Value);
					break;

				case ObjectSerializer.TYPE_UINT32:
					Writer.Write((uint)Value);
					break;

				case ObjectSerializer.TYPE_UINT64:
					Writer.Write((ulong)Value);
					break;

				case ObjectSerializer.TYPE_DECIMAL:
					Writer.Write((decimal)Value);
					break;

				case ObjectSerializer.TYPE_DOUBLE:
					Writer.Write((double)Value);
					break;

				case ObjectSerializer.TYPE_SINGLE:
					Writer.Write((float)Value);
					break;

				case ObjectSerializer.TYPE_DATETIME:
					Writer.Write((DateTime)Value);
					break;

				case ObjectSerializer.TYPE_DATETIMEOFFSET:
					Writer.Write((DateTimeOffset)Value);
					break;

				case ObjectSerializer.TYPE_TIMESPAN:
					Writer.Write((TimeSpan)Value);
					break;

				case ObjectSerializer.TYPE_CHAR:
					Writer.Write((char)Value);
					break;

				case ObjectSerializer.TYPE_STRING:
					Writer.Write((string)Value);
					break;

				case ObjectSerializer.TYPE_ENUM:
					Writer.Write((Enum)Value);
					break;

				case ObjectSerializer.TYPE_GUID:
					Writer.Write((Guid)Value);
					break;

				case ObjectSerializer.TYPE_NULL:
					break;

				case ObjectSerializer.TYPE_BYTEARRAY:
				case ObjectSerializer.TYPE_ARRAY:
				case ObjectSerializer.TYPE_OBJECT:
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
			if (x == null)
			{
				if (y == null)
					return 0;
				else
					return -1;
			}
			else if (y == null)
				return 1;
			else
			{
				uint BlockLimit = this.index?.IndexFile?.BlockLimit ?? uint.MaxValue;
				BinaryDeserializer xReader = new BinaryDeserializer(this.collectionName, this.encoding, x, BlockLimit);
				BinaryDeserializer yReader = new BinaryDeserializer(this.collectionName, this.encoding, y, BlockLimit);
				uint xType, yType;
				int i, j, c;
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

				for (i = 0, c = this.fieldNames.Length; i < c; i++)
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
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((byte)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((short)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((sbyte)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((ushort)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)(xReader.ReadBit() ? 1 : 0)).CompareTo(yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = ((xReader.ReadBit() ? 1 : 0).ToString()).CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_BYTE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadByte().CompareTo((byte)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadByte().CompareTo(yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((short)xReader.ReadByte()).CompareTo(yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadByte()).CompareTo(yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadByte()).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((short)xReader.ReadByte()).CompareTo((short)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((ushort)xReader.ReadByte()).CompareTo(yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)xReader.ReadByte()).CompareTo(yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadByte()).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadByte()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadByte()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadByte()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadByte()).CompareTo(yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadByte().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_INT16:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadInt16().CompareTo((short)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadInt16().CompareTo((short)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadInt16().CompareTo(yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadInt16()).CompareTo(yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadInt16()).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadInt16().CompareTo((short)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadInt16()).CompareTo((int)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadInt16()).CompareTo((long)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadInt16();
									if (l < 0)
										return Ascending ? -1 : 1;

									j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadInt16()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadInt16()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadInt16()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadInt16()).CompareTo(yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadInt16().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_INT32:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadInt32().CompareTo((int)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadInt32().CompareTo(yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadInt32()).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadInt32()).CompareTo((long)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadInt32();
									if (l < 0)
										return Ascending ? -1 : 1;

									j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadInt32()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadInt32()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadInt32()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadInt32().CompareTo((int)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadInt32().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_INT64:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadInt64().CompareTo((long)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadInt64().CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = (xReader.ReadInt64()).CompareTo((long)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadInt64();
									if (l < 0)
										return Ascending ? -1 : 1;

									j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadInt64()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadInt64()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadInt64()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadInt64().CompareTo((long)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadInt64().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_SBYTE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadSByte().CompareTo((sbyte)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((short)xReader.ReadSByte()).CompareTo((short)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((short)xReader.ReadSByte()).CompareTo(yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadSByte()).CompareTo(yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadSByte()).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((short)xReader.ReadSByte()).CompareTo((short)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadSByte()).CompareTo((int)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadSByte()).CompareTo((long)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									l = xReader.ReadSByte();
									if (l < 0)
										return Ascending ? -1 : 1;

									j = ((ulong)l).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadSByte()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadSByte()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadSByte()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadSByte()).CompareTo(yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadSByte().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_UINT16:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadUInt16().CompareTo((ushort)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadUInt16().CompareTo((ushort)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((int)xReader.ReadUInt16()).CompareTo((int)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadUInt16()).CompareTo(yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadUInt16()).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((int)xReader.ReadUInt16()).CompareTo((int)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadUInt16().CompareTo(yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((uint)xReader.ReadUInt16()).CompareTo(yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadUInt16()).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadUInt16()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadUInt16()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadUInt16()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = ((char)xReader.ReadUInt16()).CompareTo(yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadUInt16().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_UINT32:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadUInt32().CompareTo((uint)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									l = yReader.ReadInt16();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = ((long)xReader.ReadUInt32()).CompareTo(l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									l = yReader.ReadInt32();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = ((long)xReader.ReadUInt32()).CompareTo(l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									l = yReader.ReadInt64();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = ((long)xReader.ReadUInt32()).CompareTo(l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									l = yReader.ReadSByte();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = ((long)xReader.ReadUInt32()).CompareTo(l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadUInt32().CompareTo(yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadUInt32()).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadUInt32()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadUInt32()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadUInt32()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadUInt32().CompareTo((uint)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadUInt32().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_UINT64:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadUInt64().CompareTo((ulong)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									l = yReader.ReadInt16();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = xReader.ReadUInt64().CompareTo((ulong)l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									l = yReader.ReadInt32();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = xReader.ReadUInt64().CompareTo((ulong)l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									l = yReader.ReadInt64();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = xReader.ReadUInt64().CompareTo((ulong)l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									l = yReader.ReadSByte();
									if (l < 0)
										return Ascending ? 1 : -1;

									j = xReader.ReadUInt64().CompareTo((ulong)l);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadUInt64().CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadUInt64()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadUInt64()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadUInt64()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadUInt64().CompareTo((ulong)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadUInt64().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_DECIMAL:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadDecimal().CompareTo((decimal)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = xReader.ReadDecimal().CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadDecimal().CompareTo((decimal)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDecimal().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_DOUBLE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadDouble().CompareTo((double)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadDouble()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = xReader.ReadDouble().CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadDouble().CompareTo((double)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDouble().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_SINGLE:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadSingle().CompareTo((float)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadSingle()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadSingle()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadSingle().CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadSingle().CompareTo((float)yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadSingle().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
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
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDateTime().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DATETIME:
									j = xReader.ReadDateTime().ToUniversalTime().CompareTo(yReader.ReadDateTime().ToUniversalTime());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = xReader.ReadDateTime().ToUniversalTime().CompareTo(yReader.ReadDateTimeOffset().ToUniversalTime().DateTime);
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
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
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadDateTimeOffset().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DATETIME:
									j = xReader.ReadDateTimeOffset().ToUniversalTime().CompareTo((DateTimeOffset)(yReader.ReadDateTime().ToUniversalTime()));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = xReader.ReadDateTimeOffset().ToUniversalTime().CompareTo(yReader.ReadDateTimeOffset().ToUniversalTime());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
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
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadTimeSpan().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
									j = xReader.ReadTimeSpan().CompareTo(yReader.ReadTimeSpan());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_CHAR:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = ((int)xReader.ReadChar()).CompareTo((int)(yReader.ReadBit() ? 1 : 0));
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = ((long)xReader.ReadChar()).CompareTo(yReader.ReadInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadSByte());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = ((int)xReader.ReadChar()).CompareTo((int)yReader.ReadUInt16());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = ((long)xReader.ReadChar()).CompareTo((long)yReader.ReadUInt32());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = ((ulong)xReader.ReadChar()).CompareTo(yReader.ReadUInt64());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = ((decimal)xReader.ReadChar()).CompareTo(yReader.ReadDecimal());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = ((double)xReader.ReadChar()).CompareTo(yReader.ReadDouble());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = ((float)xReader.ReadChar()).CompareTo(yReader.ReadSingle());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadChar().CompareTo(yReader.ReadChar());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadChar().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_GUID:
								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_STRING:
						case ObjectSerializer.TYPE_ENUM:
							switch (yType)
							{
								case ObjectSerializer.TYPE_BOOLEAN:
									j = xReader.ReadString().CompareTo((yReader.ReadBit() ? 1 : 0).ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTE:
									j = xReader.ReadString().CompareTo(yReader.ReadByte().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT16:
									j = xReader.ReadString().CompareTo(yReader.ReadInt16().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT32:
									j = xReader.ReadString().CompareTo(yReader.ReadInt32().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_INT64:
									j = xReader.ReadString().CompareTo(yReader.ReadInt64().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SBYTE:
									j = xReader.ReadString().CompareTo(yReader.ReadSByte().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT16:
									j = xReader.ReadString().CompareTo(yReader.ReadUInt16().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT32:
									j = xReader.ReadString().CompareTo(yReader.ReadUInt32().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_UINT64:
									j = xReader.ReadString().CompareTo(yReader.ReadUInt64().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DECIMAL:
									j = xReader.ReadString().CompareTo(yReader.ReadDecimal().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DOUBLE:
									j = xReader.ReadString().CompareTo(yReader.ReadDouble().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_SINGLE:
									j = xReader.ReadString().CompareTo(yReader.ReadSingle().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_CHAR:
									j = xReader.ReadString().CompareTo(yReader.ReadChar().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_STRING:
									j = xReader.ReadString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_DATETIME:
									j = xReader.ReadString().CompareTo(yReader.ReadDateTime().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_DATETIMEOFFSET:
									j = xReader.ReadString().CompareTo(yReader.ReadDateTimeOffset().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_TIMESPAN:
									j = xReader.ReadString().CompareTo(yReader.ReadTimeSpan().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_GUID:
									j = xReader.ReadString().CompareTo(yReader.ReadGuid().ToString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
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
								case ObjectSerializer.TYPE_DECIMAL:
								case ObjectSerializer.TYPE_DOUBLE:
								case ObjectSerializer.TYPE_SINGLE:
								case ObjectSerializer.TYPE_CHAR:
								case ObjectSerializer.TYPE_NULL:
								case ObjectSerializer.TYPE_DATETIME:
								case ObjectSerializer.TYPE_DATETIMEOFFSET:
								case ObjectSerializer.TYPE_TIMESPAN:
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_STRING:
								case ObjectSerializer.TYPE_ENUM:
									j = xReader.ReadGuid().ToString().CompareTo(yReader.ReadString());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_GUID:
									j = xReader.ReadGuid().CompareTo(yReader.ReadGuid());
									if (j != 0)
										return Ascending ? j : -j;
									break;

								case ObjectSerializer.TYPE_BYTEARRAY:
								case ObjectSerializer.TYPE_ARRAY:
								case ObjectSerializer.TYPE_OBJECT:
								default:
									return Ascending ? -1 : 1;
							}
							break;

						case ObjectSerializer.TYPE_NULL:
							switch (yType)
							{
								case ObjectSerializer.TYPE_MIN:
									return Ascending ? 1 : -1;

								case ObjectSerializer.TYPE_MAX:
								default:
									return Ascending ? -1 : 1;

								case ObjectSerializer.TYPE_NULL:
									break;
							}
							break;

						case ObjectSerializer.TYPE_MIN:
							return Ascending ? -1 : 1;

						case ObjectSerializer.TYPE_MAX:
							return Ascending ? 1 : -1;

						case ObjectSerializer.TYPE_BYTEARRAY:
						case ObjectSerializer.TYPE_ARRAY:
						case ObjectSerializer.TYPE_OBJECT:
						default:
							return Ascending ? 1 : -1;
					}
				}

				return xReader.ReadGuid().CompareTo(yReader.ReadGuid());
			}
		}

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		public uint GetFullPayloadSize(BinaryDeserializer Reader)
		{
			return 0;
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
		public Guid ObjectId
		{
			get { return this.objectId; }
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Payload size.</returns>
		public int GetPayloadSize(BinaryDeserializer Reader)
		{
			return 0;
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <param name="IsBlob">If payload is a BLOB.</param>
		/// <returns>Payload size.</returns>
		public int GetPayloadSize(BinaryDeserializer Reader, out bool IsBlob)
		{
			IsBlob = false;
			return 0;
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

			int i, c;

			for (i = 0, c = this.fieldNames.Length; i < c; i++)
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
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.index?.IndexFile?.BlockLimit ?? uint.MaxValue);

			if (Reader.BytesLeft > 0 && Reader.ReadBit())
			{
				string Value;
				int i, c;

				for (i = 0, c = this.fieldNames.Length; i < c; i++)
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

						case ObjectSerializer.TYPE_INT32:
							Value = Reader.ReadInt32().ToString();
							break;

						case ObjectSerializer.TYPE_INT64:
							Value = Reader.ReadInt64().ToString();
							break;

						case ObjectSerializer.TYPE_SBYTE:
							Value = Reader.ReadSByte().ToString();
							break;

						case ObjectSerializer.TYPE_UINT16:
							Value = Reader.ReadUInt16().ToString();
							break;

						case ObjectSerializer.TYPE_UINT32:
							Value = Reader.ReadUInt32().ToString();
							break;

						case ObjectSerializer.TYPE_UINT64:
							Value = Reader.ReadUInt64().ToString();
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
							Value = Reader.ReadString();
							break;

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
			if (SortOrder == null)
				return true;

			int c = SortOrder.Length;
			int d = this.fieldNames.Length;
			if (d < c)
				return false;

			string s, s2;
			int i = 0;
			int j;
			int NrConstantsFound = 0;
			bool Ascending;

			for (j = 0; j < c; j++)
			{
				s = SortOrder[j];
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

				while (i < d)
				{
					s2 = this.fieldNames[i];

					if (s == s2)
						break;
					else if (ConstantFields == null || Array.IndexOf<string>(ConstantFields, s2) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						i++;
					}
				}

				if (i >= d)
					return false;

				if (Ascending != this.ascending[i])
					return false;

				i++;
			}

			if (ConstantFields != null)
			{
				int e = ConstantFields.Length;

				while (i < d && NrConstantsFound < e)
				{
					s2 = this.fieldNames[i];

					if (Array.IndexOf<string>(ConstantFields, s2) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						i++;
					}
				}

				return NrConstantsFound == e;
			}
			else
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
			if (SortOrder == null)
				return false;

			int c = SortOrder.Length;
			int d = this.fieldNames.Length;
			if (d < c)
				return false;

			string s, s2;
			int i = 0;
			int j;
			int NrConstantsFound = 0;
			bool Ascending;

			for (j = 0; j < c; j++)
			{
				s = SortOrder[j];
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

				while (i < d)
				{
					s2 = this.fieldNames[i];

					if (s == s2)
						break;
					else if (ConstantFields == null || Array.IndexOf<string>(ConstantFields, s2) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						i++;
					}
				}

				if (i >= d)
					return false;

				if (Ascending == this.ascending[i])
					return false;

				i++;
			}

			if (ConstantFields != null)
			{
				int e = ConstantFields.Length;

				while (i < d && NrConstantsFound < e)
				{
					s2 = this.fieldNames[i];

					if (Array.IndexOf<string>(ConstantFields, s2) < 0)
						return false;
					else
					{
						NrConstantsFound++;
						i++;
					}
				}

				return NrConstantsFound == e;
			}
			else
				return true;
		}

	}
}
