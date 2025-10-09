using Waher.Persistence.Attributes;
using Waher.Security;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	/// <summary>
	/// A Docker BLOB reference
	/// Ref: https://github.com/Trust-Anchor-Group/NeuronDockerRegistry/blob/main/TAG.Networking.DockerRegistry/Model/DockerBlob.cs
	/// </summary>
	[CollectionName("DockerBlobs")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Digest", "Function")]
	[Index("AccountName", "Digest", "Function")]
	public class DockerBlobEncrypted
	{
		/// <summary>
		/// A Docker BLOB reference
		/// </summary>
		public DockerBlobEncrypted()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Hash function used
		/// </summary>
		[Encrypted(100)]
		public HashFunction Function { get; set; }

		/// <summary>
		/// Digest
		/// </summary>
		[Encrypted(200)]
		public byte[] Digest { get; set; }

		/// <summary>
		/// File name
		/// </summary>
		[Encrypted(100)]
		public string FileName { get; set; }

		/// <summary>
		/// Name of user account uploading the BLOB.
		/// </summary>
		[Encrypted(200)]
		public string AccountName { get; set; }
	}
}
