using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.SNMPv2_SMI
{
	public static partial class Values
	{
		public static readonly ObjectId iso = new Int64[] { 1 };
		public static readonly ObjectId org = new ObjectId(iso, 3);
		public static readonly ObjectId dod = new ObjectId(org, 6);
		public static readonly ObjectId internet = new ObjectId(dod, 1);
		public static readonly ObjectId directory = new ObjectId(internet, 1);
		public static readonly ObjectId mgmt = new ObjectId(internet, 2);
		public static readonly ObjectId mib_2 = new ObjectId(mgmt, 1);
		public static readonly ObjectId transmission = new ObjectId(mib_2, 10);
		public static readonly ObjectId experimental = new ObjectId(internet, 3);
		public static readonly ObjectId _private = new ObjectId(internet, 4);
		public static readonly ObjectId enterprises = new ObjectId(_private, 1);
		public static readonly ObjectId security = new ObjectId(internet, 5);
		public static readonly ObjectId snmpV2 = new ObjectId(internet, 6);
		public static readonly ObjectId snmpDomains = new ObjectId(snmpV2, 1);
		public static readonly ObjectId snmpProxys = new ObjectId(snmpV2, 2);
		public static readonly ObjectId snmpModules = new ObjectId(snmpV2, 3);
		public static readonly ObjectId zeroDotZero = new ObjectId(0, 0);
	}

	public class ObjectSyntax
	{
		public ObjectSyntax _choice;
		public SimpleSyntax simple;
		public ApplicationSyntax application_wide;
	}

	public class SimpleSyntax
	{
		public SimpleSyntax _choice;
		public Int64 integer_value;
		public Array<Byte> string_value;
		public ObjectId objectID_value;
	}

	public class ApplicationSyntax
	{
		public ApplicationSyntax _choice;
		public Array<Byte> ipAddress_value;
		public Int64 counter_value;
		public Int64 timeticks_value;
		public Array<Byte> arbitrary_value;
		public Int64 big_counter_value;
		public Int64 unsigned_integer_value;
	}

}
