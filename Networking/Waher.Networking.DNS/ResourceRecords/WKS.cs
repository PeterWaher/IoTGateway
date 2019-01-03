using System;
using System.Collections;
using System.IO;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Well Known Services, as defined in RFC 1010
	/// </summary>
	public class WKS : ResourceAddressRecord
	{
		private readonly byte protocol;
		private readonly BitArray bitMap;

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
			byte[] Bin = new byte[c];
			Data.Read(Bin, 0, c);
			BitArray BitMap = new BitArray(Bin);

			this.protocol = Protocol;
			this.bitMap = BitMap;
		}

		/// <summary>
		/// IP Address size.
		/// </summary>
		protected override int AddressSize => 4;

		/// <summary>
		/// Protocol Number
		/// </summary>
		public byte Protocol => this.protocol;

		/// <summary>
		/// Service Bit-map
		/// </summary>
		public BitArray BitMap => this.bitMap;

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
