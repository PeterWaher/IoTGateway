using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;
using Waher.Things.Snmp.Snmp1.RFC1213_MIB;

namespace Waher.Things.Snmp.Snmp1.RFC1215
{
	public static partial class Values
	{
		public static readonly ObjectId snmp = new ObjectId(RFC1213_MIB.Values.mib_2, 11);
		public static readonly Int64 coldStart = 0;
		public static readonly Int64 warmStart = 1;
		public static readonly Int64 linkDown = 2;
		public static readonly Int64 linkUp = 3;
		public static readonly Int64 authenticationFailure = 4;
		public static readonly Int64 egpNeighborLoss = 5;
	}

}
