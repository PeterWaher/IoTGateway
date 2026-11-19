using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp2\Asn1\SNMPv2-PDU.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Things.Snmp.Snmp2.SNMPv2_PDU
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.Construct.ISet integer_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(-2147483648, 2147483647);
		public static readonly Asn1.Construct.ISet string_valueSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 65535);
		public static readonly Asn1.Construct.ISet ipAddress_valueSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet counter_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet timeticks_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet big_counter_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 18446744073709551615);
		public static readonly Asn1.Construct.ISet unsigned_integer_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly System.Numerics.BigInteger max_bindings = 2147483647;
		public static readonly Asn1.Construct.ISet request_idInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(-214783648, 214783647);
		public static readonly Asn1.Construct.ISet error_indexInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, Waher.Things.Snmp.Snmp2.SNMPv2_PDU._DefinedValues.max_bindings);
		public static readonly Asn1.Construct.ISet request_idInSet2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(-214783648, 214783647);
		public static readonly Asn1.Construct.ISet non_repeatersInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, Waher.Things.Snmp.Snmp2.SNMPv2_PDU._DefinedValues.max_bindings);
		public static readonly Asn1.Construct.ISet max_repetitionsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, Waher.Things.Snmp.Snmp2.SNMPv2_PDU._DefinedValues.max_bindings);
	}

	public partial class ObjectSyntax : Asn1.Construct
	{
		public enum _Options
		{
			simple,
			application_wide
		}

		public _Options _choice;
		public Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax simple;
		public Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax application_wide;

		public static Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax Create(Asn1F.AsnReader Reader)
		{
			Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax _Result = new Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 2: // Integer
				case 4: // OctetString
				case 6: // ObjectIdentifier
					this.simple = Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax.Create(Reader);
					this._choice = _Options.simple;
					break;

				case 64: // APPLICATION [0]
				case 65: // APPLICATION [1]
				case 66: // APPLICATION [2]
				case 67: // APPLICATION [3]
				case 68: // APPLICATION [4]
				case 70: // APPLICATION [6]
					this.application_wide = Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax.Create(Reader);
					this._choice = _Options.application_wide;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax _Result)
		{
			try
			{
				_Result = new Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax();
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
				case _Options.simple:
					if (!(this.simple is null))
						this.simple.Write(Writer);
					break;

				case _Options.application_wide:
					if (!(this.application_wide is null))
						this.application_wide.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ObjectSyntax _Typed)) return false;
			if (!AreEqual(this.simple, _Typed.simple)) return false;
			if (!AreEqual(this.application_wide, _Typed.application_wide)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.simple);
			_Result ^= _Result << 5 ^ CalcHashCode(this.application_wide);
			return _Result;
		}
	}

	public partial class SimpleSyntax : Asn1.Construct
	{
		public enum _Options
		{
			integer_value,
			string_value,
			objectID_value
		}

		public _Options _choice;
		public System.Numerics.BigInteger? integer_value;
		public byte[] string_value;
		public Asn1.ObjectId objectID_value;

		public static Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax Create(Asn1F.AsnReader Reader)
		{
			Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax _Result = new Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 2: // Integer
					this.integer_value = ReadInteger(Reader);
					if (!(Asn1.Construct.CheckInSet(this.integer_value, _DefinedValues.integer_valueInSet)))
						throw new Asn1F.AsnContentException("integer_value out of bounds.");
					this._choice = _Options.integer_value;
					break;

				case 4: // OctetString
					this.string_value = ReadOctetString(Reader);
					if (!(Asn1.Construct.CheckSize(this.string_value, _DefinedValues.string_valueSize)))
						throw new Asn1F.AsnContentException("string_value out of bounds.");
					this._choice = _Options.string_value;
					break;

				case 6: // ObjectIdentifier
					this.objectID_value = ReadObjectId(Reader);
					this._choice = _Options.objectID_value;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax _Result)
		{
			try
			{
				_Result = new Waher.Things.Snmp.Snmp2.SNMPv2_PDU.SimpleSyntax();
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
				case _Options.integer_value:
					if (!(Asn1.Construct.CheckInSet(this.integer_value, _DefinedValues.integer_valueInSet)))
						throw new Asn1F.AsnContentException("integer_value out of bounds.");
					if (this.integer_value.HasValue)
						WriteInteger(Writer, this.integer_value.Value);
					break;

				case _Options.string_value:
					if (!(Asn1.Construct.CheckSize(this.string_value, _DefinedValues.string_valueSize)))
						throw new Asn1F.AsnContentException("string_value out of bounds.");
					if (!(this.string_value is null))
						WriteOctetString(Writer, this.string_value);
					break;

				case _Options.objectID_value:
					if (!(this.objectID_value is null))
						WriteObjectId(Writer, this.objectID_value);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SimpleSyntax _Typed)) return false;
			if (!AreEqual(this.integer_value, _Typed.integer_value)) return false;
			if (!AreEqual(this.string_value, _Typed.string_value)) return false;
			if (!AreEqual(this.objectID_value, _Typed.objectID_value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.integer_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.string_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.objectID_value);
			return _Result;
		}
	}

	public partial class ApplicationSyntax : Asn1.Construct
	{
		public enum _Options
		{
			ipAddress_value,
			counter_value,
			timeticks_value,
			arbitrary_value,
			big_counter_value,
			unsigned_integer_value
		}

		public _Options _choice;
		public byte[] ipAddress_value;
		public System.Numerics.BigInteger? counter_value;
		public System.Numerics.BigInteger? timeticks_value;
		public byte[] arbitrary_value;
		public System.Numerics.BigInteger? big_counter_value;
		public System.Numerics.BigInteger? unsigned_integer_value;

		public static Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax Create(Asn1F.AsnReader Reader)
		{
			Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax _Result = new Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 64: // APPLICATION [0]
					this.ipAddress_value = ReadOctetStringImplicit(Reader, 0);
					if (!(Asn1.Construct.CheckSize(this.ipAddress_value, _DefinedValues.ipAddress_valueSize)))
						throw new Asn1F.AsnContentException("ipAddress_value out of bounds.");
					this._choice = _Options.ipAddress_value;
					break;

				case 65: // APPLICATION [1]
					this.counter_value = ReadIntegerImplicit(Reader, 1);
					if (!(Asn1.Construct.CheckInSet(this.counter_value, _DefinedValues.counter_valueInSet)))
						throw new Asn1F.AsnContentException("counter_value out of bounds.");
					this._choice = _Options.counter_value;
					break;

				case 67: // APPLICATION [3]
					this.timeticks_value = ReadIntegerImplicit(Reader, 2);
					if (!(Asn1.Construct.CheckInSet(this.timeticks_value, _DefinedValues.timeticks_valueInSet)))
						throw new Asn1F.AsnContentException("timeticks_value out of bounds.");
					this._choice = _Options.timeticks_value;
					break;

				case 68: // APPLICATION [4]
					this.arbitrary_value = ReadOctetStringImplicit(Reader, 3);
					this._choice = _Options.arbitrary_value;
					break;

				case 70: // APPLICATION [6]
					this.big_counter_value = ReadIntegerImplicit(Reader, 4);
					if (!(Asn1.Construct.CheckInSet(this.big_counter_value, _DefinedValues.big_counter_valueInSet)))
						throw new Asn1F.AsnContentException("big_counter_value out of bounds.");
					this._choice = _Options.big_counter_value;
					break;

				case 66: // APPLICATION [2]
					this.unsigned_integer_value = ReadIntegerImplicit(Reader, 5);
					if (!(Asn1.Construct.CheckInSet(this.unsigned_integer_value, _DefinedValues.unsigned_integer_valueInSet)))
						throw new Asn1F.AsnContentException("unsigned_integer_value out of bounds.");
					this._choice = _Options.unsigned_integer_value;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax _Result)
		{
			try
			{
				_Result = new Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ApplicationSyntax();
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
				case _Options.ipAddress_value:
					if (!(Asn1.Construct.CheckSize(this.ipAddress_value, _DefinedValues.ipAddress_valueSize)))
						throw new Asn1F.AsnContentException("ipAddress_value out of bounds.");
					if (!(this.ipAddress_value is null))
						WriteOctetStringImplicit(Writer, 0, this.ipAddress_value);
					break;

				case _Options.counter_value:
					if (!(Asn1.Construct.CheckInSet(this.counter_value, _DefinedValues.counter_valueInSet)))
						throw new Asn1F.AsnContentException("counter_value out of bounds.");
					if (this.counter_value.HasValue)
						WriteIntegerImplicit(Writer, 1, this.counter_value.Value);
					break;

				case _Options.timeticks_value:
					if (!(Asn1.Construct.CheckInSet(this.timeticks_value, _DefinedValues.timeticks_valueInSet)))
						throw new Asn1F.AsnContentException("timeticks_value out of bounds.");
					if (this.timeticks_value.HasValue)
						WriteIntegerImplicit(Writer, 2, this.timeticks_value.Value);
					break;

				case _Options.arbitrary_value:
					if (!(this.arbitrary_value is null))
						WriteOctetStringImplicit(Writer, 3, this.arbitrary_value);
					break;

				case _Options.big_counter_value:
					if (!(Asn1.Construct.CheckInSet(this.big_counter_value, _DefinedValues.big_counter_valueInSet)))
						throw new Asn1F.AsnContentException("big_counter_value out of bounds.");
					if (this.big_counter_value.HasValue)
						WriteIntegerImplicit(Writer, 4, this.big_counter_value.Value);
					break;

				case _Options.unsigned_integer_value:
					if (!(Asn1.Construct.CheckInSet(this.unsigned_integer_value, _DefinedValues.unsigned_integer_valueInSet)))
						throw new Asn1F.AsnContentException("unsigned_integer_value out of bounds.");
					if (this.unsigned_integer_value.HasValue)
						WriteIntegerImplicit(Writer, 5, this.unsigned_integer_value.Value);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ApplicationSyntax _Typed)) return false;
			if (!AreEqual(this.ipAddress_value, _Typed.ipAddress_value)) return false;
			if (!AreEqual(this.counter_value, _Typed.counter_value)) return false;
			if (!AreEqual(this.timeticks_value, _Typed.timeticks_value)) return false;
			if (!AreEqual(this.arbitrary_value, _Typed.arbitrary_value)) return false;
			if (!AreEqual(this.big_counter_value, _Typed.big_counter_value)) return false;
			if (!AreEqual(this.unsigned_integer_value, _Typed.unsigned_integer_value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAddress_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.counter_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.timeticks_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.arbitrary_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.big_counter_value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unsigned_integer_value);
			return _Result;
		}
	}

	public partial class PDUs : Asn1.Construct
	{
		public enum _Options
		{
			get_request,
			get_next_request,
			get_bulk_request,
			response,
			set_request,
			inform_request,
			snmpV2_trap,
			report
		}

		public _Options _choice;
		public PDU get_request;
		public PDU get_next_request;
		public BulkPDU get_bulk_request;
		public PDU response;
		public PDU set_request;
		public PDU inform_request;
		public PDU snmpV2_trap;
		public PDU report;

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

				case 133: // CONTEXT SPECIFIC [5]
					this.get_bulk_request = BulkPDU.Create(Reader);
					this._choice = _Options.get_bulk_request;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.response = PDU.Create(Reader);
					this._choice = _Options.response;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.set_request = PDU.Create(Reader);
					this._choice = _Options.set_request;
					break;

				case 134: // CONTEXT SPECIFIC [6]
					this.inform_request = PDU.Create(Reader);
					this._choice = _Options.inform_request;
					break;

				case 135: // CONTEXT SPECIFIC [7]
					this.snmpV2_trap = PDU.Create(Reader);
					this._choice = _Options.snmpV2_trap;
					break;

				case 136: // CONTEXT SPECIFIC [8]
					this.report = PDU.Create(Reader);
					this._choice = _Options.report;
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

				case _Options.get_bulk_request:
					if (!(this.get_bulk_request is null))
						this.get_bulk_request.Write(Writer);
					break;

				case _Options.response:
					if (!(this.response is null))
						this.response.Write(Writer);
					break;

				case _Options.set_request:
					if (!(this.set_request is null))
						this.set_request.Write(Writer);
					break;

				case _Options.inform_request:
					if (!(this.inform_request is null))
						this.inform_request.Write(Writer);
					break;

				case _Options.snmpV2_trap:
					if (!(this.snmpV2_trap is null))
						this.snmpV2_trap.Write(Writer);
					break;

				case _Options.report:
					if (!(this.report is null))
						this.report.Write(Writer);
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
			if (!AreEqual(this.get_bulk_request, _Typed.get_bulk_request)) return false;
			if (!AreEqual(this.response, _Typed.response)) return false;
			if (!AreEqual(this.set_request, _Typed.set_request)) return false;
			if (!AreEqual(this.inform_request, _Typed.inform_request)) return false;
			if (!AreEqual(this.snmpV2_trap, _Typed.snmpV2_trap)) return false;
			if (!AreEqual(this.report, _Typed.report)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.get_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.get_next_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.get_bulk_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.response);
			_Result ^= _Result << 5 ^ CalcHashCode(this.set_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.inform_request);
			_Result ^= _Result << 5 ^ CalcHashCode(this.snmpV2_trap);
			_Result ^= _Result << 5 ^ CalcHashCode(this.report);
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
		genErr = 5,
		noAccess = 6,
		wrongType = 7,
		wrongLength = 8,
		wrongEncoding = 9,
		wrongValue = 10,
		noCreation = 11,
		inconsistentValue = 12,
		resourceUnavailable = 13,
		commitFailed = 14,
		undoFailed = 15,
		authorizationError = 16,
		notWritable = 17,
		inconsistentName = 18
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
			if (!(Asn1.Construct.CheckInSet(this.request_id, _DefinedValues.request_idInSet)))
				throw new Asn1F.AsnContentException("request_id out of bounds.");
			this.error_status = ReadEnumeration<error_statusEnum>(Reader);
			this.error_index = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.error_index, _DefinedValues.error_indexInSet)))
				throw new Asn1F.AsnContentException("error_index out of bounds.");
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
				if (!(Asn1.Construct.CheckInSet(this.request_id, _DefinedValues.request_idInSet)))
					throw new Asn1F.AsnContentException("request_id out of bounds.");
				WriteInteger(Writer, this.request_id);
				WriteInteger(Writer, (int)this.error_status);
				if (!(Asn1.Construct.CheckInSet(this.error_index, _DefinedValues.error_indexInSet)))
					throw new Asn1F.AsnContentException("error_index out of bounds.");
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

	public partial class BulkPDU : Asn1.Construct
	{
		public System.Numerics.BigInteger request_id;
		public System.Numerics.BigInteger non_repeaters;
		public System.Numerics.BigInteger max_repetitions;
		public VarBind[] variable_bindings;

		public static BulkPDU Create(Asn1F.AsnReader Reader)
		{
			BulkPDU _Result = new BulkPDU();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.request_id = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.request_id, _DefinedValues.request_idInSet2)))
				throw new Asn1F.AsnContentException("request_id out of bounds.");
			this.non_repeaters = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.non_repeaters, _DefinedValues.non_repeatersInSet)))
				throw new Asn1F.AsnContentException("non_repeaters out of bounds.");
			this.max_repetitions = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.max_repetitions, _DefinedValues.max_repetitionsInSet)))
				throw new Asn1F.AsnContentException("max_repetitions out of bounds.");
			this.variable_bindings = ReadArray<VarBind>(Reader, (Asn1F.AsnReader Reader, out VarBind _Item) => VarBind.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out BulkPDU _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new BulkPDU();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckInSet(this.request_id, _DefinedValues.request_idInSet2)))
					throw new Asn1F.AsnContentException("request_id out of bounds.");
				WriteInteger(Writer, this.request_id);
				if (!(Asn1.Construct.CheckInSet(this.non_repeaters, _DefinedValues.non_repeatersInSet)))
					throw new Asn1F.AsnContentException("non_repeaters out of bounds.");
				WriteInteger(Writer, this.non_repeaters);
				if (!(Asn1.Construct.CheckInSet(this.max_repetitions, _DefinedValues.max_repetitionsInSet)))
					throw new Asn1F.AsnContentException("max_repetitions out of bounds.");
				WriteInteger(Writer, this.max_repetitions);
				WriteArray(Writer, this.variable_bindings);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is BulkPDU _Typed)) return false;
			if (!AreEqual(this.request_id, _Typed.request_id)) return false;
			if (!AreEqual(this.non_repeaters, _Typed.non_repeaters)) return false;
			if (!AreEqual(this.max_repetitions, _Typed.max_repetitions)) return false;
			if (!AreEqual(this.variable_bindings, _Typed.variable_bindings)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.request_id);
			_Result ^= _Result << 5 ^ CalcHashCode(this.non_repeaters);
			_Result ^= _Result << 5 ^ CalcHashCode(this.max_repetitions);
			_Result ^= _Result << 5 ^ CalcHashCode(this.variable_bindings);
			return _Result;
		}
	}

	public partial class unnamed1Choice : Asn1.Construct
	{
		public enum _Options
		{
			@value,
			unSpecified,
			noSuchObject,
			noSuchInstance,
			endOfMibView
		}

		public _Options _choice;
		public Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax @value;
		public object unSpecified;
		public object noSuchObject;
		public object noSuchInstance;
		public object endOfMibView;

		public static unnamed1Choice Create(Asn1F.AsnReader Reader)
		{
			unnamed1Choice _Result = new unnamed1Choice();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 2: // Integer
				case 4: // OctetString
				case 6: // ObjectIdentifier
				case 64: // APPLICATION [0]
				case 65: // APPLICATION [1]
				case 66: // APPLICATION [2]
				case 67: // APPLICATION [3]
				case 68: // APPLICATION [4]
				case 70: // APPLICATION [6]
					this.@value = Waher.Things.Snmp.Snmp2.SNMPv2_PDU.ObjectSyntax.Create(Reader);
					this._choice = _Options.@value;
					break;

				case 5: // Null
					this.unSpecified = ReadNull(Reader);
					this._choice = _Options.unSpecified;
					break;

				case 128: // CONTEXT SPECIFIC [0]
					this.noSuchObject = ReadNullImplicit(Reader, 2);
					this._choice = _Options.noSuchObject;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.noSuchInstance = ReadNullImplicit(Reader, 3);
					this._choice = _Options.noSuchInstance;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.endOfMibView = ReadNullImplicit(Reader, 4);
					this._choice = _Options.endOfMibView;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out unnamed1Choice _Result)
		{
			try
			{
				_Result = new unnamed1Choice();
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
				case _Options.@value:
					if (!(this.@value is null))
						this.@value.Write(Writer);
					break;

				case _Options.unSpecified:
					if (!(this.unSpecified is null))
						WriteNull(Writer);
					break;

				case _Options.noSuchObject:
					if (!(this.noSuchObject is null))
						WriteNullImplicit(Writer, 2);
					break;

				case _Options.noSuchInstance:
					if (!(this.noSuchInstance is null))
						WriteNullImplicit(Writer, 3);
					break;

				case _Options.endOfMibView:
					if (!(this.endOfMibView is null))
						WriteNullImplicit(Writer, 4);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is unnamed1Choice _Typed)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			if (!AreEqual(this.unSpecified, _Typed.unSpecified)) return false;
			if (!AreEqual(this.noSuchObject, _Typed.noSuchObject)) return false;
			if (!AreEqual(this.noSuchInstance, _Typed.noSuchInstance)) return false;
			if (!AreEqual(this.endOfMibView, _Typed.endOfMibView)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unSpecified);
			_Result ^= _Result << 5 ^ CalcHashCode(this.noSuchObject);
			_Result ^= _Result << 5 ^ CalcHashCode(this.noSuchInstance);
			_Result ^= _Result << 5 ^ CalcHashCode(this.endOfMibView);
			return _Result;
		}
	}

	public partial class VarBind : Asn1.Construct
	{
		public Asn1.ObjectId name;
		public unnamed1Choice unnamed1;

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
			this.unnamed1 = unnamed1Choice.Create(Reader);
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
				this.unnamed1.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is VarBind _Typed)) return false;
			if (!AreEqual(this.name, _Typed.name)) return false;
			if (!AreEqual(this.unnamed1, _Typed.unnamed1)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unnamed1);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
