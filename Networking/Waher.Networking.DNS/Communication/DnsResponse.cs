using System;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Stored DNS Query
	/// </summary>
	[CollectionName("DnsCache")]
	[Index("Name", "Type", "Class")]
	public class DnsResponse
	{
		private Guid objectId = Guid.Empty;
		private string name = string.Empty;
		private QTYPE type = QTYPE.A;
		private QCLASS _class = QCLASS.IN;
		private DateTime expires = DateTime.MinValue;
		private ResourceRecord[] answer = null;
		private ResourceRecord[] authority = null;
		private ResourceRecord[] additional = null;
		private byte[] raw = null;

		/// <summary>
		/// Stored DNS Query
		/// </summary>
		public DnsResponse()
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
		/// Domain name
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Question TYPE
		/// </summary>
		public QTYPE Type
		{
			get => this.type;
			set => this.type = value;
		}

		/// <summary>
		/// Question CLASS
		/// </summary>
		public QCLASS Class
		{
			get => this._class;
			set => this._class = value;
		}

		/// <summary>
		/// When record expires
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Expires
		{
			get => this.expires;
			set => this.expires = value;
		}

		/// <summary>
		/// Answer resource records
		/// </summary>
		[DefaultValueNull]
		public ResourceRecord[] Answer
		{
			get => this.answer;
			set => this.answer = value;
		}

		/// <summary>
		/// Authority resource records
		/// </summary>
		[DefaultValueNull]
		public ResourceRecord[] Authority
		{
			get => this.authority;
			set => this.authority = value;
		}

		/// <summary>
		/// Additional resource records
		/// </summary>
		[DefaultValueNull]
		public ResourceRecord[] Additional
		{
			get => this.additional;
			set => this.additional = value;
		}

		/// <summary>
		/// Raw DNS message
		/// </summary>
		[DefaultValueNull]
		public byte[] Raw
		{
			get => this.raw;
			set => this.raw = value;
		}
	}
}
