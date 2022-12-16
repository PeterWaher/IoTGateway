using System;
using Waher.Persistence.Attributes;

namespace Waher.Networking.XMPP.Provisioning.Cache
{
	/// <summary>
	/// Information about a query to the provisioning server.
	/// </summary>
	[CollectionName("CachedProvisioningQueries")]
	[NoBackup("State of rules is unsure at time of restoration. By not including these cached items, it is assured the current state is mirrored after a restoration.")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Xml", "Method")]
	[Index("LastUsed")]
	public class CachedQuery
    {
		private Guid objectId = Guid.Empty;
		private DateTime lastUsed = DateTime.MinValue;
		private string xml = string.Empty;
		private string method = string.Empty;
		private string response = string.Empty;

		/// <summary>
		/// Information about a query to the provisioning server.
		/// </summary>
		public CachedQuery()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// XML query.
		/// </summary>
		public string Xml
		{
			get => this.xml;
			set => this.xml = value;
		}

		/// <summary>
		/// Method
		/// </summary>
		public string Method
		{
			get => this.method;
			set => this.method = value;
		}

		/// <summary>
		/// XML Response
		/// </summary>
		[DefaultValueStringEmpty]
		public string Response
		{
			get => this.response;
			set => this.response = value;
		}

		/// <summary>
		/// Last time rule was used.
		/// </summary>
		public DateTime LastUsed
		{
			get => this.lastUsed;
			set => this.lastUsed = value;
		}

	}
}
