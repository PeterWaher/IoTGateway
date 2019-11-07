using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.SNMPv2_PDU
{
	public static partial class Values
	{
		public static readonly Int64 max_bindings = 2147483647;
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

	public class PDUs
	{
		public PDUs _choice;
		public PDU get_request;
		public PDU get_next_request;
		public BulkPDU get_bulk_request;
		public PDU response;
		public PDU set_request;
		public PDU inform_request;
		public PDU snmpV2_trap;
		public PDU report;
	}

	public class PDU
	{
		public Int64 request_id;
		public Int64 error_status;
		public Int64 error_index;
		public Array<VarBind> variable_bindings;
	}

	public class BulkPDU
	{
		public Int64 request_id;
		public Int64 non_repeaters;
		public Int64 max_repetitions;
		public Array<VarBind> variable_bindings;
	}

	public class VarBind
	{
		public enum unnamed1Enum
		{
			value,
			unSpecified,
			noSuchObject,
			noSuchInstance,
			endOfMibView
		}

		public class unnamed1Choice
		{
			public unnamed1Enum _choice;
			public ObjectSyntax _value;
			public Object unSpecified;
			public Object noSuchObject;
			public Object noSuchInstance;
			public Object endOfMibView;
		}

		public ObjectId name;
		public unnamed1Choice unnamed1;
	}

}
