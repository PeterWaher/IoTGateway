using System;
using System.Collections;
using System.Net;
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
		/// <param name="Address">IP Address</param>
		/// <param name="Protocol">Protocol</param>
		/// <param name="BitMap">Bit Map of supported well-known services.</param>
		public WKS(string Name, TYPE Type, CLASS Class, uint Ttl, 
			IPAddress Address, byte Protocol, BitArray BitMap)
			: base(Name, Type, Class, Ttl, Address)
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
