using System;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Mcp.Identity
{
	/// <summary>
	/// Contains a cached item that has been petitioned earlier, and approved.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	[CollectionName("McpIdentityPetitionCache")]
	[Index("McpUserName", "Uri")]
	public class CachedPetitionItem : IEncryptedProperties
	{
		/// <summary>
		/// Contains credentials for an MCP client.
		/// </summary>
		public CachedPetitionItem()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string? ObjectID { get; set; }

		/// <summary>
		/// MCP User name
		/// </summary>
		public string? McpUserName { get; set; }

		/// <summary>
		/// URI of cached item
		/// </summary>
		public string? Uri { get; set; }

		/// <summary>
		/// XML representation of object.
		/// </summary>
		[Encrypted(32)]
		public string? Xml { get; set; }

		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[]
		{
			nameof(this.Xml)
		};
	}
}