using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp2\Asn1\SNMPv2-SMI.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Things.Snmp.Snmp2.SNMPv2_SMI
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId iso = new System.Numerics.BigInteger[] { 1 };
		public static readonly Asn1.ObjectId org = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.iso, 3);
		public static readonly Asn1.ObjectId dod = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.org, 6);
		public static readonly Asn1.ObjectId internet = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.dod, 1);
		public static readonly Asn1.ObjectId directory = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.internet, 1);
		public static readonly Asn1.ObjectId mgmt = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.internet, 2);
		public static readonly Asn1.ObjectId mib_2 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.mgmt, 1);
		public static readonly Asn1.ObjectId transmission = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.mib_2, 10);
		public static readonly Asn1.ObjectId experimental = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.internet, 3);
		public static readonly Asn1.ObjectId @private = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.internet, 4);
		public static readonly Asn1.ObjectId enterprises = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.@private, 1);
		public static readonly Asn1.ObjectId security = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.internet, 5);
		public static readonly Asn1.ObjectId snmpV2 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.internet, 6);
		public static readonly Asn1.ObjectId snmpDomains = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.snmpV2, 1);
		public static readonly Asn1.ObjectId snmpProxys = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.snmpV2, 2);
		public static readonly Asn1.ObjectId snmpModules = new Asn1.ObjectId(Waher.Things.Snmp.Snmp2.SNMPv2_SMI._DefinedValues.snmpV2, 3);
		public static readonly Asn1.Construct.ISet integer_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(-2147483648, 2147483647);
		public static readonly Asn1.Construct.ISet string_valueSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 65535);
		public static readonly Asn1.Construct.ISet ipAddress_valueSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet counter_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet timeticks_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet big_counter_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 18446744073709551615);
		public static readonly Asn1.Construct.ISet unsigned_integer_valueInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.ObjectId zeroDotZero = new Asn1.ObjectId(0, 0);
	}

	public partial class ObjectSyntax : Asn1.Construct
	{
		public enum _Options
		{
			simple,
			application_wide
		}

		public _Options _choice;
		public Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax simple;
		public Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax application_wide;

		public static Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ObjectSyntax Create(Asn1F.AsnReader Reader)
		{
			Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ObjectSyntax _Result = new Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ObjectSyntax();
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
					this.simple = Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax.Create(Reader);
					this._choice = _Options.simple;
					break;

				case 64: // APPLICATION [0]
				case 65: // APPLICATION [1]
				case 66: // APPLICATION [2]
				case 67: // APPLICATION [3]
				case 68: // APPLICATION [4]
				case 70: // APPLICATION [6]
					this.application_wide = Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax.Create(Reader);
					this._choice = _Options.application_wide;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ObjectSyntax _Result)
		{
			try
			{
				_Result = new Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ObjectSyntax();
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

		public static Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax Create(Asn1F.AsnReader Reader)
		{
			Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax _Result = new Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax();
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

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax _Result)
		{
			try
			{
				_Result = new Waher.Things.Snmp.Snmp2.SNMPv2_SMI.SimpleSyntax();
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

		public static Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax Create(Asn1F.AsnReader Reader)
		{
			Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax _Result = new Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax();
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

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax _Result)
		{
			try
			{
				_Result = new Waher.Things.Snmp.Snmp2.SNMPv2_SMI.ApplicationSyntax();
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

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
