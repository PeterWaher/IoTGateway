using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.RFC1155_SMI
{
	public static partial class Values
	{
		public static readonly ObjectId iso = new Int64[] { 1 };
		public static readonly ObjectId internet = new ObjectId(iso, 3 /* org */, 6 /* dod */, 1);
		public static readonly ObjectId directory = new ObjectId(internet, 1);
		public static readonly ObjectId mgmt = new ObjectId(internet, 2);
		public static readonly ObjectId experimental = new ObjectId(internet, 3);
		public static readonly ObjectId _private = new ObjectId(internet, 4);
		public static readonly ObjectId enterprises = new ObjectId(_private, 1);
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
		public Int64 number;
		public Array<Byte> _string;
		public ObjectId _object;
		public Object empty;
	}

	public class ApplicationSyntax
	{
		public ApplicationSyntax _choice;
		public NetworkAddress address;
		public Int64 counter;
		public Int64 gauge;
		public Int64 ticks;
		public Array<Byte> arbitrary;
	}

	public class NetworkAddress
	{
		public NetworkAddress _choice;
		public Array<Byte> internet;
	}

}
