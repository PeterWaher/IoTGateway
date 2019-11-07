using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.COMMUNITY_BASED_SNMPv2
{
	public class Message
	{
		public Int64 version;
		public Array<Byte> community;
		public Object data;
	}

}
