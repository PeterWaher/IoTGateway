using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp1\Asn1\RFC1155-SMI.asn1
 *
 ********************************************************************************/

namespace Waher.Things.Snmp.Snmp1.RFC1155_SMI
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId iso = new System.Numerics.BigInteger[] { 1 };
		public static readonly Asn1.ObjectId internet = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.iso, 3 /* org */, 6 /* dod */, 1);
		public static readonly Asn1.ObjectId directory = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.internet, 1);
		public static readonly Asn1.ObjectId mgmt = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.internet, 2);
		public static readonly Asn1.ObjectId experimental = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.internet, 3);
		public static readonly Asn1.ObjectId @private = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.internet, 4);
		public static readonly Asn1.ObjectId enterprises = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.@private, 1);
		public static readonly Asn1.Construct.ISet counterInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet gaugeInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ticksInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet internetSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
	}

	public partial class ObjectSyntax : Asn1.Construct
	{
		public enum _Options
		{
			simple,
			application_wide
		}

		public _Options _choice;
		public SimpleSyntax simple;
		public ApplicationSyntax application_wide;

		public static ObjectSyntax Create(Asn1F.AsnReader Reader)
		{
			ObjectSyntax _Result = new();
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
				case 5: // Null
				case 6: // ObjectIdentifier
					this.simple = SimpleSyntax.Create(Reader);
					this._choice = _Options.simple;
					break;

				case 64: // APPLICATION [0]
				case 65: // APPLICATION [1]
				case 66: // APPLICATION [2]
				case 67: // APPLICATION [3]
				case 68: // APPLICATION [4]
					this.application_wide = ApplicationSyntax.Create(Reader);
					this._choice = _Options.application_wide;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out ObjectSyntax _Result)
		{
			try
			{
				_Result = new ObjectSyntax();
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
			number,
			@string,
			@object,
			empty
		}

		public _Options _choice;
		public System.Numerics.BigInteger? number;
		public byte[] @string;
		public Asn1.ObjectId @object;
		public object empty;

		public static SimpleSyntax Create(Asn1F.AsnReader Reader)
		{
			SimpleSyntax _Result = new();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 2: // Integer
					this.number = ReadInteger(Reader);
					this._choice = _Options.number;
					break;

				case 4: // OctetString
					this.@string = ReadOctetString(Reader);
					this._choice = _Options.@string;
					break;

				case 6: // ObjectIdentifier
					this.@object = ReadObjectId(Reader);
					this._choice = _Options.@object;
					break;

				case 5: // Null
					this.empty = ReadNull(Reader);
					this._choice = _Options.empty;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out SimpleSyntax _Result)
		{
			try
			{
				_Result = new SimpleSyntax();
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
				case _Options.number:
					if (this.number.HasValue)
						WriteInteger(Writer, this.number.Value);
					break;

				case _Options.@string:
					if (!(this.@string is null))
						WriteOctetString(Writer, this.@string);
					break;

				case _Options.@object:
					if (!(this.@object is null))
						WriteObjectId(Writer, this.@object);
					break;

				case _Options.empty:
					if (!(this.empty is null))
						WriteNull(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SimpleSyntax _Typed)) return false;
			if (!AreEqual(this.number, _Typed.number)) return false;
			if (!AreEqual(this.@string, _Typed.@string)) return false;
			if (!AreEqual(this.@object, _Typed.@object)) return false;
			if (!AreEqual(this.empty, _Typed.empty)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.number);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@string);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@object);
			_Result ^= _Result << 5 ^ CalcHashCode(this.empty);
			return _Result;
		}
	}

	public partial class ApplicationSyntax : Asn1.Construct
	{
		public enum _Options
		{
			address,
			counter,
			gauge,
			ticks,
			arbitrary
		}

		public _Options _choice;
		public NetworkAddress address;
		public System.Numerics.BigInteger? counter;
		public System.Numerics.BigInteger? gauge;
		public System.Numerics.BigInteger? ticks;
		public byte[] arbitrary;

		public static ApplicationSyntax Create(Asn1F.AsnReader Reader)
		{
			ApplicationSyntax _Result = new();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 64: // APPLICATION [0]
					this.address = NetworkAddress.Create(Reader);
					this._choice = _Options.address;
					break;

				case 65: // APPLICATION [1]
					this.counter = ReadIntegerImplicit(Reader, 1);
					if (!(Asn1.Construct.CheckInSet(this.counter, _DefinedValues.counterInSet)))
						throw new Asn1F.AsnContentException("counter out of bounds.");
					this._choice = _Options.counter;
					break;

				case 66: // APPLICATION [2]
					this.gauge = ReadIntegerImplicit(Reader, 2);
					if (!(Asn1.Construct.CheckInSet(this.gauge, _DefinedValues.gaugeInSet)))
						throw new Asn1F.AsnContentException("gauge out of bounds.");
					this._choice = _Options.gauge;
					break;

				case 67: // APPLICATION [3]
					this.ticks = ReadIntegerImplicit(Reader, 3);
					if (!(Asn1.Construct.CheckInSet(this.ticks, _DefinedValues.ticksInSet)))
						throw new Asn1F.AsnContentException("ticks out of bounds.");
					this._choice = _Options.ticks;
					break;

				case 68: // APPLICATION [4]
					this.arbitrary = ReadOctetStringImplicit(Reader, 4);
					this._choice = _Options.arbitrary;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out ApplicationSyntax _Result)
		{
			try
			{
				_Result = new ApplicationSyntax();
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
				case _Options.address:
					if (!(this.address is null))
						this.address.Write(Writer);
					break;

				case _Options.counter:
					if (!(Asn1.Construct.CheckInSet(this.counter, _DefinedValues.counterInSet)))
						throw new Asn1F.AsnContentException("counter out of bounds.");
					if (this.counter.HasValue)
						WriteIntegerImplicit(Writer, 1, this.counter.Value);
					break;

				case _Options.gauge:
					if (!(Asn1.Construct.CheckInSet(this.gauge, _DefinedValues.gaugeInSet)))
						throw new Asn1F.AsnContentException("gauge out of bounds.");
					if (this.gauge.HasValue)
						WriteIntegerImplicit(Writer, 2, this.gauge.Value);
					break;

				case _Options.ticks:
					if (!(Asn1.Construct.CheckInSet(this.ticks, _DefinedValues.ticksInSet)))
						throw new Asn1F.AsnContentException("ticks out of bounds.");
					if (this.ticks.HasValue)
						WriteIntegerImplicit(Writer, 3, this.ticks.Value);
					break;

				case _Options.arbitrary:
					if (!(this.arbitrary is null))
						WriteOctetStringImplicit(Writer, 4, this.arbitrary);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ApplicationSyntax _Typed)) return false;
			if (!AreEqual(this.address, _Typed.address)) return false;
			if (!AreEqual(this.counter, _Typed.counter)) return false;
			if (!AreEqual(this.gauge, _Typed.gauge)) return false;
			if (!AreEqual(this.ticks, _Typed.ticks)) return false;
			if (!AreEqual(this.arbitrary, _Typed.arbitrary)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.address);
			_Result ^= _Result << 5 ^ CalcHashCode(this.counter);
			_Result ^= _Result << 5 ^ CalcHashCode(this.gauge);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ticks);
			_Result ^= _Result << 5 ^ CalcHashCode(this.arbitrary);
			return _Result;
		}
	}

	public partial class NetworkAddress : Asn1.Construct
	{
		public enum _Options
		{
			internet
		}

		public _Options _choice;
		public byte[] internet;

		public static NetworkAddress Create(Asn1F.AsnReader Reader)
		{
			NetworkAddress _Result = new();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 64: // APPLICATION [0]
					this.internet = ReadOctetStringImplicit(Reader, 0);
					if (!(Asn1.Construct.CheckSize(this.internet, _DefinedValues.internetSize)))
						throw new Asn1F.AsnContentException("internet out of bounds.");
					this._choice = _Options.internet;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out NetworkAddress _Result)
		{
			try
			{
				_Result = new NetworkAddress();
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
				case _Options.internet:
					if (!(Asn1.Construct.CheckSize(this.internet, _DefinedValues.internetSize)))
						throw new Asn1F.AsnContentException("internet out of bounds.");
					if (!(this.internet is null))
						WriteOctetStringImplicit(Writer, 0, this.internet);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is NetworkAddress _Typed)) return false;
			if (!AreEqual(this.internet, _Typed.internet)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.internet);
			return _Result;
		}
	}

}
