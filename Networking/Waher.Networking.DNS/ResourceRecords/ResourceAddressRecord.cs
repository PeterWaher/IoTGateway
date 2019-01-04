using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Abstract base class for Resource Address Records.
	/// </summary>
	public abstract class ResourceAddressRecord : ResourceRecord
	{
		private IPAddress address;

		/// <summary>
		/// Abstract base class for Resource Address Records.
		/// </summary>
		public ResourceAddressRecord()
			: base()
		{
			this.address = null;
		}

		/// <summary>
		/// Abstract base class for Resource Address Records.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		public ResourceAddressRecord(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data)
			: base(Name, Type, Class, Ttl)
		{
			int c = this.AddressSize;
			byte[] Bin = new byte[c];
			Data.Read(Bin, 0, c);

			this.address = new IPAddress(Bin);
		}

		/// <summary>
		/// IP Address size.
		/// </summary>
		protected abstract int AddressSize
		{
			get;
		}

		/// <summary>
		/// IP address
		/// </summary>
		[IgnoreMember]
		public IPAddress Address
		{
			get => this.address;
			set => this.address = value;
		}

		/// <summary>
		/// IP Address Bytes
		/// </summary>
		[DefaultValueNull]
		public byte[] AddressBytes
		{
			get => this.address?.GetAddressBytes();
			set => this.address = value == null ? null : new IPAddress(value);
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.address?.ToString();
		}
	}
}
