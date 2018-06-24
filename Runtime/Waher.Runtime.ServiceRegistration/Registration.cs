using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.ServiceRegistration
{
	/// <summary>
	/// Local registration object.
	/// </summary>
	[CollectionName("Registration")]
	public class Registration
	{
		private DateTime created = DateTime.MinValue;
		private DateTime updated = DateTime.MinValue;
		private Annotation[] annotations = null;
		private string[] features = null;
		private string[] assemblies = null;
		private string objectId = null;
		private string bareJid = string.Empty;
		private string clientName = string.Empty;
		private string clientVersion = string.Empty;
		private string clientOS = string.Empty;
		private string language = string.Empty;
		private string host = string.Empty;
		private string domain = string.Empty;

		/// <summary>
		/// Local registration object.
		/// </summary>
		public Registration()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Registered features
		/// </summary>
		[DefaultValueNull]
		public string[] Features
		{
			get { return this.features; }
			set { this.features = value; }
		}

		/// <summary>
		/// Registered assemblies
		/// </summary>
		[DefaultValueNull]
		public string[] Assemblies
		{
			get { return this.assemblies; }
			set { this.assemblies = value; }
		}

		/// <summary>
		/// Registered Bare JID
		/// </summary>
		[DefaultValueStringEmpty]
		public string BareJid
		{
			get { return this.bareJid; }
			set { this.bareJid = value; }
		}

		/// <summary>
		/// Registered Client Name 
		/// </summary>
		[DefaultValueStringEmpty]
		public string ClientName
		{
			get { return this.clientName; }
			set { this.clientName = value; }
		}

		/// <summary>
		/// Registered Client Version
		/// </summary>
		[DefaultValueStringEmpty]
		public string ClientVersion
		{
			get { return this.clientVersion; }
			set { this.clientVersion = value; }
		}

		/// <summary>
		/// Registered Client OS
		/// </summary>
		[DefaultValueStringEmpty]
		public string ClientOS
		{
			get { return this.clientOS; }
			set { this.clientOS = value; }
		}

		/// <summary>
		/// Registered Client Language
		/// </summary>
		[DefaultValueStringEmpty]
		public string Language
		{
			get { return this.language; }
			set { this.language = value; }
		}

		/// <summary>
		/// Registered Host
		/// </summary>
		[DefaultValueStringEmpty]
		public string Host
		{
			get { return this.host; }
			set { this.host = value; }
		}

		/// <summary>
		/// Registered Domain
		/// </summary>
		[DefaultValueStringEmpty]
		public string Domain
		{
			get { return this.domain; }
			set { this.domain = value; }
		}

		/// <summary>
		/// When registration object was created
		/// </summary>
		public DateTime Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		/// <summary>
		/// When registration object was last updated
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		/// <summary>
		/// Registered annotations
		/// </summary>
		public Annotation[] Annotations
		{
			get => this.annotations;
			set => this.annotations = value;
		}

	}
}
