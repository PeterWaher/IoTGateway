using System;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.COMMUNITY_BASED_SNMPv2
{
	/// <summary>
	/// TODO
	/// </summary>
	public class Message
	{
		/// <summary>
		/// TODO
		/// </summary>
		public Int64 version;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> community;

		/// <summary>
		/// TODO
		/// </summary>
		public Object data;
	}

}
