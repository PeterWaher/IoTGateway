using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp2\Asn1\COMMUNITY-BASED-SNMPv2.asn1
 *
 ********************************************************************************/

namespace Waher.Things.Snmp.Snmp2.COMMUNITY_BASED_SNMPv2
{
	public enum versionEnum
	{
		version = 1
	}

	public partial class Message : Asn1.Construct
	{
		public versionEnum version;
		public byte[] community;
		public object data;

		public static Message Create(Asn1F.AsnReader Reader)
		{
			Message _Result = new();
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

}
