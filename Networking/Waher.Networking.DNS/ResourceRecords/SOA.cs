using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Start Of zone Authority
	/// </summary>
	public class SOA : ResourceRecord
	{
		private string mName;
		private string rName;
		private uint serial;
		private uint refresh;
		private uint retry;
		private uint expire;
		private uint minimum;

		/// <summary>
		/// Start Of zone Authority
		/// </summary>
		public SOA()
			: base()
		{
			this.mName = string.Empty;
			this.rName = string.Empty;
			this.serial = 0;
			this.refresh = 0;
			this.retry = 0;
			this.expire = 0;
			this.minimum = 0;
		}

		/// <summary>
		/// Start Of zone Authority
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public SOA(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data) 
			: base(Name, Type, Class, Ttl)
		{
			this.mName = DnsResolver.ReadName(Data);
			this.rName= DnsResolver.ReadName(Data);
			this.serial = DnsResolver.ReadUInt32(Data);
			this.refresh = DnsResolver.ReadUInt32(Data);
			this.retry = DnsResolver.ReadUInt32(Data);
			this.expire = DnsResolver.ReadUInt32(Data);
			this.minimum = DnsResolver.ReadUInt32(Data);
		}

		/// <summary>
		/// Name server that was the original or primary source of data for this zone
		/// </summary>
		[DefaultValueStringEmpty]
		public string MName
		{
			get => this.mName;
			set => this.mName = value;
		}

		/// <summary>
		/// Specifies the mailbox of the person responsible for this zone
		/// </summary>
		[DefaultValueStringEmpty]
		public string RName
		{
			get => this.rName;
			set => this.rName = value;
		}

		/// <summary>
		/// Version number of the original copy of the zone.
		/// </summary>
		[DefaultValue(0)]
		public uint Serial
		{
			get => this.serial;
			set => this.serial = value;
		}

		/// <summary>
		/// Time interval before the zone should be refreshed
		/// </summary>
		[DefaultValue(0)]
		public uint Refresh
		{
			get => this.refresh;
			set => this.refresh = value;
		}

		/// <summary>
		/// Interval that should elapse before a failed refresh should be retried
		/// </summary>
		[DefaultValue(0)]
		public uint Retry
		{
			get => this.retry;
			set => this.retry = value;
		}

		/// <summary>
		/// Specifies the upper limit on the time interval that can elapse before 
		/// the zone is no longer authoritative
		/// </summary>
		[DefaultValue(0)]
		public uint Expire
		{
			get => this.expire;
			set => this.expire = value;
		}

		/// <summary>
		/// Minimum TTL field that should be exported with any RR from this zone
		/// </summary>
		[DefaultValue(0)]
		public uint Minimum
		{
			get => this.minimum;
			set => this.minimum = value;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.mName + "\t" + this.rName +
				"\t" + this.serial + "\t" + this.refresh + "\t" + this.retry + 
				"\t" + this.expire + "\t" + this.minimum;
		}
	}
}
