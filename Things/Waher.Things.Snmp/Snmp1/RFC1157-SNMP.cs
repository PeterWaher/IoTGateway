using System;
using Waher.Content.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

namespace Waher.Things.Snmp.Snmp1.RFC1157_SNMP
{
	/// <summary>
	/// TODO
	/// </summary>
	public class Message
	{
		/// <summary>
		/// TODO
		/// </summary>
		public Int64 version;

		/// <summary>
		/// TODO
		/// </summary>
		public Array<Byte> community;

		/// <summary>
		/// TODO
		/// </summary>
		public Object data;
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
		public PDU get_response;

		/// <summary>
		/// TODO
		/// </summary>
		public PDU set_request;

		/// <summary>
		/// TODO
		/// </summary>
		public Trap_PDU trap;
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
	public class Trap_PDU
	{
		/// <summary>
		/// TODO
		/// </summary>
		public ObjectId enterprise;

		/// <summary>
		/// TODO
		/// </summary>
		public NetworkAddress agent_addr;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 generic_trap;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 specific_trap;

		/// <summary>
		/// TODO
		/// </summary>
		public Int64 time_stamp;

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
		public ObjectId name;

		/// <summary>
		/// TODO
		/// </summary>
		public ObjectSyntax _value;
	}
}
