using System;
using Waher.Content.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

namespace Waher.Things.Snmp.Snmp1.RFC1212
{
	/// <summary>
	/// TODO
	/// </summary>
	public class IndexSyntax
	{
		/// <summary>
		/// TODO
		/// </summary>
		public IndexSyntax _choice;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 number;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> _string;

		/// <summary>
		/// TODO
		/// </summary>
		public ObjectId _object;

		/// <summary>
		/// TODO
		/// </summary>
		public NetworkAddress address;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> ipAddress;
	}

}
