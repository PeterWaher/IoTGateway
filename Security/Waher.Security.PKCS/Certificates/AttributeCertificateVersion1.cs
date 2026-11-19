using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Security.PKCS.Certificates.PKIX1Explicit88;
using Waher.Security.PKCS.Certificates.PKIX1Implicit88;
using Waher.Security.PKCS.Certificates.PKIXAttributeCertificate;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Security\Waher.Security.PKCS\Certificates\Asn1\AttributeCertificateVersion1.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Security.PKCS.Certificates.AttributeCertificateVersion1
{
	public partial class AttributeCertificateV1 : Asn1.Construct
	{
		public AttributeCertificateInfoV1 acInfo;
		public AlgorithmIdentifier signatureAlgorithm;
		public byte[] signature;

		public static AttributeCertificateV1 Create(Asn1F.AsnReader Reader)
		{
			AttributeCertificateV1 _Result = new AttributeCertificateV1();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.acInfo = AttributeCertificateInfoV1.Create(Reader);
			this.signatureAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.signature = ReadBitStringExplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AttributeCertificateV1 _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AttributeCertificateV1();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.acInfo.Write(Writer);
				this.signatureAlgorithm.Write(Writer);
				WriteBitStringExplicit(Writer, 2, this.signature);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttributeCertificateV1 _Typed)) return false;
			if (!AreEqual(this.acInfo, _Typed.acInfo)) return false;
			if (!AreEqual(this.signatureAlgorithm, _Typed.signatureAlgorithm)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.acInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			return _Result;
		}
	}

	public partial class subjectChoice : Asn1.Construct
	{
		public enum _Options
		{
			baseCertificateID,
			subjectName
		}

		public _Options _choice;
		public IssuerSerial baseCertificateID;
		public GeneralName[] subjectName;

		public static subjectChoice Create(Asn1F.AsnReader Reader)
		{
			subjectChoice _Result = new subjectChoice();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.baseCertificateID = IssuerSerial.Create(Reader);
					this._choice = _Options.baseCertificateID;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.subjectName = ReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item));
					this._choice = _Options.subjectName;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out subjectChoice _Result)
		{
			try
			{
				_Result = new subjectChoice();
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
				case _Options.baseCertificateID:
					if (!(this.baseCertificateID is null))
						this.baseCertificateID.Write(Writer);
					break;

				case _Options.subjectName:
					if (!(this.subjectName is null))
						WriteArrayExplicit(Writer, 1, this.subjectName);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is subjectChoice _Typed)) return false;
			if (!AreEqual(this.baseCertificateID, _Typed.baseCertificateID)) return false;
			if (!AreEqual(this.subjectName, _Typed.subjectName)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.baseCertificateID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectName);
			return _Result;
		}
	}

	public partial class AttributeCertificateInfoV1 : Asn1.Construct
	{
		public AttCertVersionV1? version = AttCertVersionV1.v1;
		public subjectChoice subject;
		public GeneralName[] issuer;
		public AlgorithmIdentifier signature;
		public System.Numerics.BigInteger serialNumber;
		public AttCertValidityPeriod attCertValidityPeriod;
		public Attribute[] attributes;
		public byte[] issuerUniqueID;
		public Extension[] extensions;

		public static AttributeCertificateInfoV1 Create(Asn1F.AsnReader Reader)
		{
			AttributeCertificateInfoV1 _Result = new AttributeCertificateInfoV1();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadEnumerationExplicit<AttCertVersionV1>(Reader, 0, this.version, out this.version);
			this.subject = subjectChoice.Create(Reader);
			this.issuer = ReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item));
			this.signature = AlgorithmIdentifier.Create(Reader);
			this.serialNumber = ReadIntegerExplicit(Reader, 4);
			this.attCertValidityPeriod = AttCertValidityPeriod.Create(Reader);
			this.attributes = ReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute.TryCreate(Reader, out _Item));
			TryReadBitStringExplicit(Reader, 7, this.issuerUniqueID, out this.issuerUniqueID);
			TryReadArray<Extension>(Reader, (Asn1F.AsnReader Reader, out Extension _Item) => Extension.TryCreate(Reader, out _Item), this.extensions, out this.extensions);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AttributeCertificateInfoV1 _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AttributeCertificateInfoV1();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (this.version.HasValue)
					WriteIntegerExplicit(Writer, 0, (int)this.version.Value);
				this.subject.Write(Writer);
				WriteArrayExplicit(Writer, 2, this.issuer);
				this.signature.Write(Writer);
				WriteIntegerExplicit(Writer, 4, this.serialNumber);
				this.attCertValidityPeriod.Write(Writer);
				WriteArrayExplicit(Writer, 6, this.attributes);
				if (!(this.issuerUniqueID is null))
					WriteBitStringExplicit(Writer, 7, this.issuerUniqueID);
				if (!(this.extensions is null))
					WriteArrayExplicit(Writer, 8, this.extensions);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttributeCertificateInfoV1 _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.subject, _Typed.subject)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			if (!AreEqual(this.serialNumber, _Typed.serialNumber)) return false;
			if (!AreEqual(this.attCertValidityPeriod, _Typed.attCertValidityPeriod)) return false;
			if (!AreEqual(this.attributes, _Typed.attributes)) return false;
			if (!AreEqual(this.issuerUniqueID, _Typed.issuerUniqueID)) return false;
			if (!AreEqual(this.extensions, _Typed.extensions)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subject);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			_Result ^= _Result << 5 ^ CalcHashCode(this.serialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attCertValidityPeriod);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attributes);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerUniqueID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extensions);
			return _Result;
		}
	}

	public enum AttCertVersionV1
	{
		v1 = 0
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
