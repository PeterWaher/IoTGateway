using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Interface for B-tree record handlers.
	/// </summary>
	public interface IRecordHandler
	{
		/// <summary>
		/// Creates an empty primary key object.
		/// </summary>
		/// <returns>Primary key object.</returns>
		IPrimaryKey CreateKey();

		/// <summary>
		/// Reads the next primary key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Primary key object.</returns>
		IPrimaryKey ReadPrimaryKey(BinaryDeserializer Reader);

		/// <summary>
		/// Reads the next primary key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <param name="Key">Primary key object.</param>
		void ReadPrimaryKey(BinaryDeserializer Reader, IPrimaryKey Key);

		/// <summary>
		/// Skips the next record key.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If a key was skipped (true), or if no more keys are available (false).</returns>
		bool SkipKey(BinaryDeserializer Reader);

		/// <summary>
		/// If Getting payload size and full payload size is asynchronous (true) or
		/// synchronous (false).
		/// </summary>
		bool IsGetPayloadSizeAsync { get; }

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		uint GetFullPayloadSize(BinaryDeserializer Reader);

		/// <summary>
		/// Gets the full payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Full size of the payload.</returns>
		Task<uint> GetFullPayloadSizeAsync(BinaryDeserializer Reader);

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload.</returns>
		int GetPayloadSize(BinaryDeserializer Reader);

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload.</returns>
		Task<int> GetPayloadSizeAsync(BinaryDeserializer Reader);

		/// <summary>
		/// Gets the payload size.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Size of the payload, and if the object is a BLOB.</returns>
		Task<KeyValuePair<int, bool>> GetPayloadSizeEx(BinaryDeserializer Reader);

		/// <summary>
		/// Checks if the following object is a BLOB.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>If the following object is a BLOB.</returns>
		Task<bool> IsBlob(BinaryDeserializer Reader);

		/// <summary>
		/// Gets the type of the payload, if any.
		/// </summary>
		/// <param name="Reader">Binary deserializer object.</param>
		/// <returns>Payload type, if any, or null, if not.</returns>
		string GetPayloadType(BinaryDeserializer Reader);

		/// <summary>
		/// Exports a key to XML.
		/// </summary>
		/// <param name="PrimaryKey">Key to export.</param>
		/// <param name="Output">XML Output.</param>
		void ExportKey(IPrimaryKey PrimaryKey, XmlWriter Output);

		/// <summary>
		/// Encodes a BLOB reference.
		/// </summary>
		/// <param name="BlobReference">Binary BLOB reference.</param>
		/// <param name="BlobData">Original BLOB data.</param>
		/// <returns>Encoded BLOB reference.</returns>
		byte[] EncodeBlobReference(byte[] BlobReference, byte[] BlobData);
	}
}
