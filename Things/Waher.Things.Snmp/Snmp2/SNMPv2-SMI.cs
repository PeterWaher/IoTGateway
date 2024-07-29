using System;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.SNMPv2_SMI
{
	/// <summary>
	/// TODO
	/// </summary>
	public static partial class Values
	{
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId iso = new Int64[] { 1 };

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId org = new ObjectId(iso, 3);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId dod = new ObjectId(org, 6);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId internet = new ObjectId(dod, 1);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId directory = new ObjectId(internet, 1);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId mgmt = new ObjectId(internet, 2);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId mib_2 = new ObjectId(mgmt, 1);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId transmission = new ObjectId(mib_2, 10);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId experimental = new ObjectId(internet, 3);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId _private = new ObjectId(internet, 4);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId enterprises = new ObjectId(_private, 1);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId security = new ObjectId(internet, 5);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId snmpV2 = new ObjectId(internet, 6);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId snmpDomains = new ObjectId(snmpV2, 1);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId snmpProxys = new ObjectId(snmpV2, 2);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId snmpModules = new ObjectId(snmpV2, 3);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId zeroDotZero = new ObjectId(0, 0);
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class ObjectSyntax
	{
		/// <summary>
		/// TODO
		/// </summary>
		public ObjectSyntax _choice;

		/// <summary>
		/// TODO
		/// </summary>
		public SimpleSyntax simple;

		/// <summary>
		/// TODO
		/// </summary>
		public ApplicationSyntax application_wide;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class SimpleSyntax
	{
		/// <summary>
		/// TODO
		/// </summary>
		public SimpleSyntax _choice;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 integer_value;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> string_value;

		/// <summary>
		/// TODO
		/// </summary>
		public ObjectId objectID_value;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class ApplicationSyntax
	{
		/// <summary>
		/// TODO
		/// </summary>
		public ApplicationSyntax _choice;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> ipAddress_value;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 counter_value;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 timeticks_value;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> arbitrary_value;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 big_counter_value;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 unsigned_integer_value;
	}
}
