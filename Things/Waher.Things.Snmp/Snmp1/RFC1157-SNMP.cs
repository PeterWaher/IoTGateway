using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp1\Asn1\RFC1157-SNMP.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Things.Snmp.Snmp1.RFC1157_SNMP
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.Construct.ISet time_stampInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
	}

	public enum versionEnum
	{
		version_1 = 0
	}

	public partial class Message : Asn1.Construct
	{
		public versionEnum version;
		public byte[] community;
		public object data;

		public static Message Create(Asn1F.AsnReader Reader)
		{
			Message _Result = new Message();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumeration<versionEnum>(Reader);
			this.community = ReadOctetString(Reader);
			this.data = ReadAny(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Message _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Message();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, (int)this.version);
				WriteOctetString(Writer, this.community);
				WriteAny(Writer, this.data);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Message _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.community, _Typed.community)) return false;
			if (!AreEqual(this.data, _Typed.data)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.community);
			_Result ^= _Result << 5 ^ CalcHashCode(this.data);
			return _Result;
		}
	}

	public partial class PDUs : Asn1.Construct
	{
		public enum _Options
		{
			get_request,
			get_next_request,
			get_response,
			set_request,
			trap
		}

		public _Options _choice;
		public PDU get_request;
		public PDU get_next_request;
		public PDU get_response;
		public PDU set_request;
		public Trap_PDU trap;

		public static PDUs Create(Asn1F.AsnReader Reader)
		{
			PDUs _Result = new PDUs();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.get_request = PDU.Create(Reader);
					this._choice = _Options.get_request;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.get_next_request = PDU.Create(Reader);
					this._choice = _Options.get_next_request;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.get_response = PDU.Create(Reader);
					this._choice = _Options.get_response;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.set_request = PDU.Create(Reader);
					this._choice = _Options.set_request;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.trap = Trap_PDU.Create(Reader);
					this._choice = _Options.trap;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out PDUs _Result)
		{
			try
			{
				_Result = new PDUs();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.get_request:
					if (!(this.get_request is null))
						this.get_request.Write(Writer);
					break;

				case _Options.get_next_request:
					if (!(this.get_next_request is null))
						this.get_next_request.Write(Writer);
					break;

				case _Options.get_response:
					if (!(this.get_response is null))
						this.get_response.Write(Writer);
					break;

				case _Options.set_request:
					if (!(this.set_request is null))
						this.set_request.Write(Writer);
					break;

				case _Options.trap:
					if (!(this.trap is null))
						this.trap.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PDUs _Typed)) return false;
			if (!AreEqual(this.get_request, _Typed.get_request)) return false;
			if (!AreEqual(this.get_next_request, _Typed.get_next_request)) return false;
			if (!AreEqual(this.get_response, _Typed.get_response)) return false;
			if (!AreEqual(this.set_request, _Typed.set_request)) return false;
			if (!AreEqual(this.trap, _Typed.trap)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.get_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.get_next_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.get_response);
			_Result ^= _Result << 5 ^ CalcHashCode(this.set_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.trap);
			return _Result;
		}
	}

	public enum error_statusEnum
	{
		noError = 0,
		tooBig = 1,
		noSuchName = 2,
		badValue = 3,
		readOnly = 4,
		genErr = 5
	}

	public partial class PDU : Asn1.Construct
	{
		public System.Numerics.BigInteger request_id;
		public error_statusEnum error_status;
		public System.Numerics.BigInteger error_index;
		public VarBind[] variable_bindings;

		public static PDU Create(Asn1F.AsnReader Reader)
		{
			PDU _Result = new PDU();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.request_id = ReadInteger(Reader);
			this.error_status = ReadEnumeration<error_statusEnum>(Reader);
			this.error_index = ReadInteger(Reader);
			this.variable_bindings = ReadArray<VarBind>(Reader, (Asn1F.AsnReader Reader, out VarBind _Item) => VarBind.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PDU _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PDU();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, this.request_id);
				WriteInteger(Writer, (int)this.error_status);
				WriteInteger(Writer, this.error_index);
				WriteArray(Writer, this.variable_bindings);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PDU _Typed)) return false;
			if (!AreEqual(this.request_id, _Typed.request_id)) return false;
			if (!AreEqual(this.error_status, _Typed.error_status)) return false;
			if (!AreEqual(this.error_index, _Typed.error_index)) return false;
			if (!AreEqual(this.variable_bindings, _Typed.variable_bindings)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.request_id);
			_Result ^= _Result << 5 ^ CalcHashCode(this.error_status);
			_Result ^= _Result << 5 ^ CalcHashCode(this.error_index);
			_Result ^= _Result << 5 ^ CalcHashCode(this.variable_bindings);
			return _Result;
		}
	}

	public enum generic_trapEnum
	{
		coldStart = 0,
		warmStart = 1,
		linkDown = 2,
		linkUp = 3,
		authenticationFailure = 4,
		egpNeighborLoss = 5,
		enterpriseSpecific = 6
	}

	public partial class Trap_PDU : Asn1.Construct
	{
		public Asn1.ObjectId enterprise;
		public NetworkAddress agent_addr;
		public generic_trapEnum generic_trap;
		public System.Numerics.BigInteger specific_trap;
		public System.Numerics.BigInteger time_stamp;
		public VarBind[] variable_bindings;

		public static Trap_PDU Create(Asn1F.AsnReader Reader)
		{
			Trap_PDU _Result = new Trap_PDU();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.enterprise = ReadObjectId(Reader);
			this.agent_addr = NetworkAddress.Create(Reader);
			this.generic_trap = ReadEnumeration<generic_trapEnum>(Reader);
			this.specific_trap = ReadInteger(Reader);
			this.time_stamp = ReadIntegerImplicit(Reader, 4);
			if (!(Asn1.Construct.CheckInSet(this.time_stamp, _DefinedValues.time_stampInSet)))
				throw new Asn1F.AsnContentException("time_stamp out of bounds.");
			this.variable_bindings = ReadArray<VarBind>(Reader, (Asn1F.AsnReader Reader, out VarBind _Item) => VarBind.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Trap_PDU _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Trap_PDU();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.enterprise);
				this.agent_addr.Write(Writer);
				WriteIntegerImplicit(Writer, 2, (int)this.generic_trap);
				WriteIntegerImplicit(Writer, 3, this.specific_trap);
				if (!(Asn1.Construct.CheckInSet(this.time_stamp, _DefinedValues.time_stampInSet)))
					throw new Asn1F.AsnContentException("time_stamp out of bounds.");
				WriteIntegerImplicit(Writer, 4, this.time_stamp);
				WriteArrayImplicit(Writer, 5, this.variable_bindings);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Trap_PDU _Typed)) return false;
			if (!AreEqual(this.enterprise, _Typed.enterprise)) return false;
			if (!AreEqual(this.agent_addr, _Typed.agent_addr)) return false;
			if (!AreEqual(this.generic_trap, _Typed.generic_trap)) return false;
			if (!AreEqual(this.specific_trap, _Typed.specific_trap)) return false;
			if (!AreEqual(this.time_stamp, _Typed.time_stamp)) return false;
			if (!AreEqual(this.variable_bindings, _Typed.variable_bindings)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.enterprise);
			_Result ^= _Result << 5 ^ CalcHashCode(this.agent_addr);
			_Result ^= _Result << 5 ^ CalcHashCode(this.generic_trap);
			_Result ^= _Result << 5 ^ CalcHashCode(this.specific_trap);
			_Result ^= _Result << 5 ^ CalcHashCode(this.time_stamp);
			_Result ^= _Result << 5 ^ CalcHashCode(this.variable_bindings);
			return _Result;
		}
	}

	public partial class VarBind : Asn1.Construct
	{
		public Asn1.ObjectId name;
		public ObjectSyntax @value;

		public static VarBind Create(Asn1F.AsnReader Reader)
		{
			VarBind _Result = new VarBind();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.name = ReadObjectId(Reader);
			this.@value = ObjectSyntax.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out VarBind _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new VarBind();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectId(Writer, this.name);
				this.@value.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is VarBind _Typed)) return false;
			if (!AreEqual(this.name, _Typed.name)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
