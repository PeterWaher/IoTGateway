using System;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.SNMPv2_PDU
{
	/// <summary>
	/// TODO
	/// </summary>
	public static partial class Values
	{
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 max_bindings = 2147483647;
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

	/// <summary>
	/// TODO
	/// </summary>
	public class PDUs
	{
		/// <summary>
		/// TODO
		/// </summary>
		public PDUs _choice;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU get_request;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU get_next_request;

		/// <summary>
		/// TODO
		/// </summary>
		public BulkPDU get_bulk_request;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU response;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU set_request;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU inform_request;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU snmpV2_trap;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU report;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class PDU
	{
		/// <summary>
		/// TODO
		/// </summary>
		public Int64 request_id;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 error_status;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 error_index;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<VarBind> variable_bindings;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class BulkPDU
	{
		/// <summary>
		/// TODO
		/// </summary>
		public Int64 request_id;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 non_repeaters;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 max_repetitions;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<VarBind> variable_bindings;
	}

	/// <summary>
	/// TODO
	/// </summary>
	public class VarBind
	{
		/// <summary>
		/// TODO
		/// </summary>
		public enum unnamed1Enum
		{
			/// <summary>
			/// TODO
			/// </summary>
			value,

			/// <summary>
			/// TODO
			/// </summary>
			unSpecified,

			/// <summary>
			/// TODO
			/// </summary>
			noSuchObject,

			/// <summary>
			/// TODO
			/// </summary>
			noSuchInstance,

			/// <summary>
			/// TODO
			/// </summary>
			endOfMibView
		}

		/// <summary>
		/// TODO
		/// </summary>
		public class unnamed1Choice
		{
			/// <summary>
			/// TODO
			/// </summary>
			public unnamed1Enum _choice;

			/// <summary>
			/// TODO
			/// </summary>
			public ObjectSyntax _value;

			/// <summary>
			/// TODO
			/// </summary>
			public Object unSpecified;

			/// <summary>
			/// TODO
			/// </summary>
			public Object noSuchObject;

			/// <summary>
			/// TODO
			/// </summary>
			public Object noSuchInstance;

			/// <summary>
			/// TODO
			/// </summary>
			public Object endOfMibView;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public ObjectId name;

		/// <summary>
		/// TODO
		/// </summary>
		public unnamed1Choice unnamed1;
	}

}
