using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp1\Asn1\RFC1212.asn1
 *
 ********************************************************************************/

namespace Waher.Things.Snmp.Snmp1.RFC1212
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.Construct.ISet numberInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
		public static readonly Asn1.Construct.ISet ipAddressSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
	}

	public partial class IndexSyntax : Asn1.Construct
	{
		public enum _Options
		{
			number,
			@string,
			@object,
			address,
			ipAddress
		}

		public _Options _choice;
		public System.Numerics.BigInteger? number;
		public byte[] @string;
		public Asn1.ObjectId @object;
		public NetworkAddress address;
		public byte[] ipAddress;

		public static IndexSyntax Create(Asn1F.AsnReader Reader)
		{
			IndexSyntax _Result = new();
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
					if (!(Asn1.Construct.CheckInSet(this.number, _DefinedValues.numberInSet)))
						throw new Asn1F.AsnContentException("number out of bounds.");
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

					this.address = NetworkAddress.Create(Reader);
					this._choice = _Options.address;
					break;

				case 64: // APPLICATION [0]
					this.ipAddress = ReadOctetStringImplicit(Reader, 4);
					if (!(Asn1.Construct.CheckSize(this.ipAddress, _DefinedValues.ipAddressSize)))
						throw new Asn1F.AsnContentException("ipAddress out of bounds.");
					this._choice = _Options.ipAddress;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out IndexSyntax _Result)
		{
			try
			{
				_Result = new IndexSyntax();
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
					if (!(Asn1.Construct.CheckInSet(this.number, _DefinedValues.numberInSet)))
						throw new Asn1F.AsnContentException("number out of bounds.");
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

				case _Options.address:
					if (!(this.address is null))
						this.address.Write(Writer);
					break;

				case _Options.ipAddress:
					if (!(Asn1.Construct.CheckSize(this.ipAddress, _DefinedValues.ipAddressSize)))
						throw new Asn1F.AsnContentException("ipAddress out of bounds.");
					if (!(this.ipAddress is null))
						WriteOctetStringImplicit(Writer, 4, this.ipAddress);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IndexSyntax _Typed)) return false;
			if (!AreEqual(this.number, _Typed.number)) return false;
			if (!AreEqual(this.@string, _Typed.@string)) return false;
			if (!AreEqual(this.@object, _Typed.@object)) return false;
			if (!AreEqual(this.address, _Typed.address)) return false;
			if (!AreEqual(this.ipAddress, _Typed.ipAddress)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.number);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@string);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@object);
			_Result ^= _Result << 5 ^ CalcHashCode(this.address);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAddress);
			return _Result;
		}
	}

}
