using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Handles string dictionary entries.
	/// </summary>
	public class StringDictionaryRecords : IRecordHandler, IComparer<string>
	{
		private readonly GenericObjectSerializer genericSerializer;
		private readonly FilesProvider provider;
		private readonly Encoding encoding;
		private readonly string collectionName;

		/// <summary>
		/// Handles string dictionary entries.
		/// </summary>
		/// <param name="CollectionName">Name of current collection.</param>
		/// <param name="Encoding">Encoding to use for text.</param>
		/// <param name="GenericSerializer">Generic serializer.</param>
		/// <param name="Provider">Files database provider.</param>
		public StringDictionaryRecords(string CollectionName, Encoding Encoding,
			GenericObjectSerializer GenericSerializer, FilesProvider Provider)
		{
			this.collectionName = CollectionName;
			this.encoding = Encoding;
			this.genericSerializer = GenericSerializer;
			this.provider = Provider;
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
			string xKey = (string)x;
			string yKey = (string)y;

			return this.Compare(xKey, yKey);
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
		public int Compare(string x, string y)
		{
			return string.Compare(x, y);
		}

		/// <summary>
		/// Gets the key of the next record.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <returns>Key object.</returns>
		public object GetKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft == 0 || !Reader.ReadBit())
				return null;

			return Reader.ReadString();
		}

		/// <summary>
		/// Skips the next key of the next record.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <returns>If a key was skipped.</returns>
		public bool SkipKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft == 0 || !Reader.ReadBit())
				return false;

			Reader.SkipString();
			return true;
		}

		/// <summary>
		/// Gets the full payload size of the next objet.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <returns>Full payloa size.</returns>
		public async Task<uint> GetFullPayloadSize(BinaryDeserializer Reader)
		{
			KeyValuePair<uint, uint> P = await this.GetSize(Reader);
			return P.Value;
		}

		private async Task<KeyValuePair<uint, uint>> GetSize(BinaryDeserializer Reader)
		{
			int Pos = Reader.Position;
			uint DataType = Reader.ReadBits(6);

			switch (DataType)
			{
				case ObjectSerializer.TYPE_OBJECT:
					ulong TypeCode = Reader.ReadVariableLengthUInt64();
					ulong CollectionCode = Reader.ReadVariableLengthUInt64();
					string CollectionName = await this.provider.GetFieldName(null, CollectionCode);
					string TypeName = await this.provider.GetFieldName(CollectionName, TypeCode);
					IObjectSerializer Serializer;

					if (string.IsNullOrEmpty(TypeName))
						Serializer = this.genericSerializer;
					else
					{
						Type T = Types.GetType(TypeName);
						if (T is null)
							Serializer = this.genericSerializer;
						else
							Serializer = await this.provider.GetObjectSerializer(T);
					}

					Reader.Position = Pos + 1;
					await Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, true);
					break;

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
				case ObjectSerializer.TYPE_CI_STRING:
				case ObjectSerializer.TYPE_ENUM:
					Reader.SkipString();
					break;

				case ObjectSerializer.TYPE_BYTEARRAY:
					Reader.SkipByteArray();
					break;

				case ObjectSerializer.TYPE_GUID:
					Reader.SkipGuid();
					break;

				case ObjectSerializer.TYPE_NULL:
					break;

				case ObjectSerializer.TYPE_ARRAY:
					throw new Exception("Arrays must be embedded in objects.");

				case ObjectSerializer.TYPE_MAX:     // BLOB
					return new KeyValuePair<uint, uint>(4, (uint)Reader.ReadVariableLengthUInt64());

				default:
					throw new Exception("Object or value expected.");
			}

			Reader.FlushBits();

			uint Size = (uint)(Reader.Position - Pos);
			Reader.Position = Pos;

			return new KeyValuePair<uint, uint>(Size, Size);
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <returns>Payload size.</returns>
		public async Task<int> GetPayloadSize(BinaryDeserializer Reader)
		{
			KeyValuePair<uint, uint> P = await this.GetSize(Reader);
			return (int)P.Key;
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <returns>Size of the payload, and if the object is a BLOB.</returns>
		public async Task<KeyValuePair<int, bool>> GetPayloadSizeEx(BinaryDeserializer Reader)
		{
			KeyValuePair<uint, uint> P = await this.GetSize(Reader);
			return new KeyValuePair<int, bool>((int)P.Key, P.Value > P.Key);
		}

		/// <summary>
		/// Checks if the following object is a BLOB.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If the following object is a BLOB.</returns>
		public async Task<bool> IsBlob(BinaryDeserializer Reader)
		{
			KeyValuePair<uint, uint> P = await this.GetSize(Reader);
			return P.Value > P.Key;
		}


		/// <summary>
		/// Gets the payload type.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <returns>Payload type.</returns>
		public string GetPayloadType(BinaryDeserializer Reader)
		{
			return string.Empty;
		}

		/// <summary>
		/// Exports a key.
		/// </summary>
		/// <param name="ObjectId">Key</param>
		/// <param name="Output">XML Output.</param>
		public void ExportKey(object ObjectId, XmlWriter Output)
		{
			Output.WriteAttributeString("key", ObjectId.ToString());
		}

		/// <summary>
		/// Encodes a BLOB reference.
		/// </summary>
		/// <param name="BlobReference">Binary BLOB reference.</param>
		/// <param name="BlobData">Original BLOB data.</param>
		/// <returns>Encoded BLOB reference.</returns>
		public byte[] EncodeBlobReference(byte[] BlobReference, byte[] BlobData)
		{
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, BlobReference, int.MaxValue);

			if (!Reader.ReadBit())
				return BlobReference;

			string Key = Reader.ReadString();
			int KeyPos = Reader.Position;
			int c = BlobData.Length;

			int FullPayloadLen = c - KeyPos;

			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);

			Writer.WriteBit(true);
			Writer.Write(Key);
			Writer.WriteBits(ObjectSerializer.TYPE_MAX, 6);
			Writer.WriteVariableLengthUInt64((uint)FullPayloadLen);
			Writer.WriteRaw(BlobReference, BlobReference.Length - 4, 4);

			return Writer.GetSerialization();
		}

		/// <summary>
		/// Gets BLOB information.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <param name="FullPayloadSize">Full payload size.</param>
		/// <param name="BlobBlockIndex">BLOB block index.</param>
		public void GetBlobInfo(BinaryDeserializer Reader, out uint FullPayloadSize, out uint BlobBlockIndex)
		{
			FullPayloadSize = (uint)Reader.ReadVariableLengthUInt64();
			BlobBlockIndex = Reader.ReadUInt32();
		}

	}
}
