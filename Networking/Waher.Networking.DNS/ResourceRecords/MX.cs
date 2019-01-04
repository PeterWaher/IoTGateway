using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mailbox eXchange
	/// </summary>
	public class MX : ResourceRecord
	{
		private ushort preference;
		private string exchange;

		/// <summary>
		/// Mailbox eXchange
		/// </summary>
		public MX()
			: base()
		{
			this.preference = 0;
			this.exchange = string.Empty;
		}

		/// <summary>
		/// Mailbox eXchange
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public MX(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl)
		{
			this.preference = DnsResolver.ReadUInt16(Data);
			this.exchange = DnsResolver.ReadName(Data);
		}

		/// <summary>
		/// Preference given to this RR among others at the same owner.Lower values
		/// are preferred.
		/// </summary>
		[DefaultValue((ushort)0)]
		public ushort Preference
		{
			get => this.preference;
			set => this.preference = value;
		}

		/// <summary>
		/// Specifies a host willing to act as a mail exchange for the owner name
		/// </summary>
		[DefaultValueStringEmpty]
		public string Exchange
		{
			get => this.exchange;
			set => this.exchange = value;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.preference.ToString() + 
				"\t" + this.exchange;
		}
	}
}
