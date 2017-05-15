using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Files.Serialization;
using Waher.Script;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Handles string dictionary entries.
	/// </summary>
	public class StringDictionaryRecords : IRecordHandler, IComparer<string>
	{
		private GenericObjectSerializer genericSerializer;
		private FilesProvider provider;
		private Encoding encoding;
		private string collectionName;
		private int recordStart;
		private int recordSizeLimit;

		/// <summary>
		/// Handles string dictionary entries.
		/// </summary>
		/// <param name="CollectionName">Name of current collection.</param>
		/// <param name="Encoding">Encoding to use for text.</param>
		/// <param name="RecordSizeLimit">Upper size limit of records.</param>
		/// <param name="GenericSerializer">Generic serializer.</param>
		/// <param name="Provider">Files database provider.</param>
		public StringDictionaryRecords(string CollectionName, Encoding Encoding, int RecordSizeLimit, 
			GenericObjectSerializer GenericSerializer, FilesProvider Provider)
		{
			this.collectionName = CollectionName;
			this.encoding = Encoding;
			this.recordSizeLimit = RecordSizeLimit;
			this.genericSerializer = GenericSerializer;
			this.provider = Provider;
		}

		/// <summary>
		/// Serializes a (Key,Value) pair.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Object">Value</param>
		/// <param name="Serializer">Serializer.</param>
		/// <returns>Serialized record.</returns>
		public byte[] Serialize(string Key, object Value, IObjectSerializer Serializer)
		{
			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);

			Writer.WriteBit(true);
			Writer.Write(Key);
			Serializer.Serialize(Writer, true, false, Value);

			return Writer.GetSerialization();
		}

		/// <summary>
		/// Serializes a Key pair.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <returns>Serialized record.</returns>
		public byte[] Serialize(string Key)
		{
			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
			Writer.Write(Key);
			return Writer.GetSerialization();
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

		public object GetKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft == 0 || !Reader.ReadBit())
				return null;

			return Reader.ReadString();
		}

		public bool SkipKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft == 0 || !Reader.ReadBit())
				return false;

			this.recordStart = Reader.Position;
			Reader.SkipString();
			return true;
		}

		public uint GetFullPayloadSize(BinaryDeserializer Reader)
		{
			int Pos = Reader.Position;
			uint DataType = Reader.ReadBits(6);

			switch (DataType)
			{
				case ObjectSerializer.TYPE_OBJECT:
					string TypeName = Reader.ReadString();
					IObjectSerializer Serializer;

					if (string.IsNullOrEmpty(TypeName))
						Serializer = this.genericSerializer;
					else
					{
						Type T = Types.GetType(TypeName);
						if (T != null)
							Serializer = this.provider.GetObjectSerializer(T);
						else
							Serializer = this.genericSerializer;
					}

					Reader.Position = Pos;

					Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
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

				default:
					throw new Exception("Object or value expected.");
			}

			Reader.FlushBits();

			uint Len = (uint)(Reader.Position - Pos);

			Reader.Position = Pos;

			return Len;
		}

		public int GetPayloadSize(BinaryDeserializer Reader)
		{
			int Len = (int)this.GetFullPayloadSize(Reader);
			if (Reader.Position - this.recordStart + Len > this.recordSizeLimit)
				return 4;
			else
				return Len;
		}

		public int GetPayloadSize(BinaryDeserializer Reader, out bool IsBlob)
		{
			int Len = (int)this.GetFullPayloadSize(Reader);
			if (IsBlob = (Reader.Position - this.recordStart + Len > this.recordSizeLimit))
				return 4;
			else
				return Len;
		}

		public string GetPayloadType(BinaryDeserializer Reader)
		{
			return string.Empty;
		}

		public void ExportKey(object ObjectId, XmlWriter Output)
		{
			Output.WriteAttributeString("key", ObjectId.ToString());
		}
	}
}
