using System;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.RFC1155_SMI
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
		public static readonly ObjectId internet = new ObjectId(iso, 3 /* org */, 6 /* dod */, 1);

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
		public static readonly ObjectId experimental = new ObjectId(internet, 3);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId _private = new ObjectId(internet, 4);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId enterprises = new ObjectId(_private, 1);
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
		public Object empty;
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
		public NetworkAddress address;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 counter;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 gauge;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 ticks;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> arbitrary;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class NetworkAddress
	{
		/// <summary>
		/// TODO
		/// </summary>
		public NetworkAddress _choice;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> internet;
	}

}
