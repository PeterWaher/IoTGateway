using System;
using System.Collections;
using System.Net;

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
		public WKS(IPAddress Address, byte Protocol, BitArray BitMap)
			: base(Address)
		{
			this.protocol = Protocol;
			this.bitMap = BitMap;
		}

		/// <summary>
		/// Protocol Number
		/// </summary>
		public byte Protocol => this.protocol;

		/// <summary>
		/// Service Bit-map
		/// </summary>
		public BitArray BitMap => this.bitMap;
	}
}
