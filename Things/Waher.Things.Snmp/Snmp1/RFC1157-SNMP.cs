using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

namespace Waher.Things.Snmp.Snmp1.RFC1157_SNMP
{
	public class Message
	{
		public Int64 version;
		public Array<Byte> community;
		public Object data;
	}

	public class PDUs
	{
		public PDUs _choice;
		public PDU get_request;
		public PDU get_next_request;
		public PDU get_response;
		public PDU set_request;
		public Trap_PDU trap;
	}

	public class PDU
	{
		public Int64 request_id;
		public Int64 error_status;
		public Int64 error_index;
		public Array<VarBind> variable_bindings;
	}

	public class Trap_PDU
	{
		public ObjectId enterprise;
		public NetworkAddress agent_addr;
		public Int64 generic_trap;
		public Int64 specific_trap;
		public Int64 time_stamp;
		public Array<VarBind> variable_bindings;
	}

	public class VarBind
	{
		public ObjectId name;
		public ObjectSyntax _value;
	}

}
