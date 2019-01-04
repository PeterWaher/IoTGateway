using System;
using System.IO;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Well Known Services, as defined in RFC 1010
	/// </summary>
	public class WKS : ResourceAddressRecord
	{
		private byte protocol;
		private byte[] bitMap;

		/// <summary>
		/// Well Known Services
		/// </summary>
		public WKS()
			: base()
		{
			this.protocol = 0;
			this.bitMap = null;
		}

		/// <summary>
		/// Well Known Services
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		/// <param name="EndPos">End position of record.</param>
		public WKS(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data, long EndPos)
			: base(Name, Type, Class, Ttl, Data)
		{
			this.protocol = (byte)Data.ReadByte();
			int c = (int)(EndPos - Data.Position);
			this.bitMap = new byte[c];
			Data.Read(bitMap, 0, c);
		}

		/// <summary>
		/// IP Address size.
		/// </summary>
		protected override int AddressSize => 4;

		/// <summary>
		/// Protocol Number
		/// </summary>
		[DefaultValue((byte)0)]
		public byte Protocol
		{
			get => this.protocol;
			set => this.protocol = value;
		}

		/// <summary>
		/// Service Bit-map
		/// </summary>
		[DefaultValueNull]
		public byte[] BitMap
		{
			get => this.bitMap;
			set => this.bitMap = value;
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.protocol.ToString() + 
				"\t" + this.bitMap.ToString();
		}
	}
}
