using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Mailbox eXchange
	/// </summary>
	public class MX : ResourceRecord
	{
		private readonly ushort preference;
		private readonly string exchange;

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
		public ushort Preference => this.preference;

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
