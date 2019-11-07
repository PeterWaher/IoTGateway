using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

namespace Waher.Things.Snmp.Snmp1.RFC1212
{
	public class IndexSyntax
	{
		public IndexSyntax _choice;
		public Int64 number;
		public Array<Byte> _string;
		public ObjectId _object;
		public NetworkAddress address;
		public Array<Byte> ipAddress;
	}

}
