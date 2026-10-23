using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Files.Storage.PrimaryKeys;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Handles primary storage of objects, as pairs of primary keys (GUIDs) and serialized objects.
	/// </summary>
	public class PrimaryRecords : IRecordHandler
	{
		private readonly int inlineObjectSizeLimit;

		/// <summary>
		/// Handles primary storage of objects, as pairs of primary keys (GUIDs) and serialized objects.
		/// </summary>
		/// <param name="InlineObjectSizeLimit">Maximum size of objects that are stored in-line. Larger objects will be stored as BLOBs.</param>
		public PrimaryRecords(int InlineObjectSizeLimit)
		{
			this.inlineObjectSizeLimit = InlineObjectSizeLimit;
		}

		/// <summary>
		/// Creates an empty primary key object.
		/// </summary>
		/// <returns>Primary key object.</returns>
		public IPrimaryKey CreateKey()
		{
			return new GuidPrimaryKey();
		}

		/// <summary>
		/// Reads the next primary key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Primary key object.</returns>
		public IPrimaryKey ReadPrimaryKey(BinaryDeserializer Reader)
		{
			IPrimaryKey Result = this.CreateKey();
			this.ReadPrimaryKey(Reader, Result);
			return Result;
		}

		/// <summary>
		/// Reads the next primary key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <param name="Key">Primary key object.</param>
		public void ReadPrimaryKey(BinaryDeserializer Reader, IPrimaryKey Key)
		{
			if (Reader.BytesLeft >= 17)
			{
				Guid Result = Reader.ReadGuid();

				if (Result.Equals(Guid.Empty))
					Key.Clear();
				else
					Key.Set(Result);
			}
			else
				Key.Clear();
		}

		/// <summary>
		/// Skips the next record key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If a key was skipped (true), or if no more keys are available (false).</returns>
		public bool SkipKey(BinaryDeserializer Reader)
		{
			if (Reader.BytesLeft < 17)
				return false;

			Guid Result = Reader.ReadGuid();

			if (Result.Equals(Guid.Empty))
				return false;
			else
				return true;
		}
		/// <summary>
		/// If Getting payload size and full payload size is asynchronous (true) or
		/// synchronous (false).
		/// </summary>
		public bool IsGetPayloadSizeAsync => false;

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		public uint GetFullPayloadSize(BinaryDeserializer Reader)
		{
			return (uint)Reader.ReadVariableLengthUInt64();
		}

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		public Task<uint> GetFullPayloadSizeAsync(BinaryDeserializer Reader)
		{
			return Task.FromResult(this.GetFullPayloadSize(Reader));
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload.</returns>
		public int GetPayloadSize(BinaryDeserializer Reader)
		{
			int Pos = Reader.Position;
			int Len = (int)Reader.ReadVariableLengthUInt64();

			if (16 + Reader.Position - Pos + Len > this.inlineObjectSizeLimit)
				return 4;
			else
				return Len;
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload.</returns>
		public Task<int> GetPayloadSizeAsync(BinaryDeserializer Reader)
		{
			return Task.FromResult(this.GetPayloadSize(Reader));
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload, and if the object is a BLOB.</returns>
		public KeyValuePair<int, bool> GetPayloadSizeEx(BinaryDeserializer Reader)
		{
			int Pos = Reader.Position;
			int Len = (int)Reader.ReadVariableLengthUInt64();

			if (16 + Reader.Position - Pos + Len > this.inlineObjectSizeLimit)
				return new KeyValuePair<int, bool>(4, true);
			else
				return new KeyValuePair<int, bool>(Len, false);
		}

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload, and if the object is a BLOB.</returns>
		public Task<KeyValuePair<int, bool>> GetPayloadSizeExAsync(BinaryDeserializer Reader)
		{
			return Task.FromResult(this.GetPayloadSizeEx(Reader));
		}

		/// <summary>
		/// Checks if the following object is a BLOB.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If the following object is a BLOB.</returns>
		public bool IsBlob(BinaryDeserializer Reader)
		{
			int Pos = Reader.Position;
			int Len = (int)Reader.ReadVariableLengthUInt64();

			return 16 + Reader.Position - Pos + Len > this.inlineObjectSizeLimit;
		}

		/// <summary>
		/// Checks if the following object is a BLOB.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If the following object is a BLOB.</returns>
		public Task<bool> IsBlobAsync(BinaryDeserializer Reader)
		{
			return Task.FromResult(this.IsBlob(Reader));
		}

		/// <summary>
		/// Gets the type of the payload, if any.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Payload type, if any, or null, if not.</returns>
		public string GetPayloadType(BinaryDeserializer Reader)
		{
			return Reader.ReadString();
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
				return ((Guid)x).CompareTo((Guid)y);
		}

		/// <summary>
		/// Exports a key to XML.
		/// </summary>
		/// <param name="PrimaryKey">Key to export.</param>
		/// <param name="Output">XML Output.</param>
		public void ExportKey(IPrimaryKey PrimaryKey, XmlWriter Output)
		{
			Output.WriteAttributeString("objectId", PrimaryKey.ToString());
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
