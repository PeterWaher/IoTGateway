using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Security.PKCS.Certificates.PKIX1Explicit88;
using Waher.Security.PKCS.Certificates.PKIX1Implicit88;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Security\Waher.Security.PKCS\Certificates\Asn1\PKIXAttributeCertificate.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Security.PKCS.Certificates.PKIXAttributeCertificate
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId id_pe_ac_auditIdentity = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pe, 4);
		public static readonly Asn1.ObjectId id_pe_aaControls = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pe, 6);
		public static readonly Asn1.ObjectId id_pe_ac_proxying = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pe, 10);
		public static readonly Asn1.ObjectId id_ce_targetInformation = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 55);
		public static readonly Asn1.ObjectId id_aca = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pkix, 10);
		public static readonly Asn1.ObjectId id_aca_authenticationInfo = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIXAttributeCertificate._DefinedValues.id_aca, 1);
		public static readonly Asn1.ObjectId id_aca_accessIdentity = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIXAttributeCertificate._DefinedValues.id_aca, 2);
		public static readonly Asn1.ObjectId id_aca_chargingIdentity = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIXAttributeCertificate._DefinedValues.id_aca, 3);
		public static readonly Asn1.ObjectId id_aca_group = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIXAttributeCertificate._DefinedValues.id_aca, 4);
		public static readonly Asn1.ObjectId id_aca_encAttrs = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIXAttributeCertificate._DefinedValues.id_aca, 6);
		public static readonly Asn1.ObjectId id_at_role = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 72);
		public static readonly Asn1.ObjectId id_at_clearance = new Asn1.ObjectId(2 /* joint_iso_ccitt */, 5 /* ds */, 1 /* module */, 5 /* selected_attribute_types */, 55 /* clearance */);
		public static readonly Asn1.Construct.ISet pathLenConstraintInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
	}

	public partial class AttributeCertificate : Asn1.Construct
	{
		public AttributeCertificateInfo acinfo;
		public AlgorithmIdentifier signatureAlgorithm;
		public byte[] signatureValue;

		public static AttributeCertificate Create(Asn1F.AsnReader Reader)
		{
			AttributeCertificate _Result = new AttributeCertificate();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.acinfo = AttributeCertificateInfo.Create(Reader);
			this.signatureAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.signatureValue = ReadBitStringImplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AttributeCertificate _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AttributeCertificate();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.acinfo.Write(Writer);
				this.signatureAlgorithm.Write(Writer);
				WriteBitStringImplicit(Writer, 2, this.signatureValue);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttributeCertificate _Typed)) return false;
			if (!AreEqual(this.acinfo, _Typed.acinfo)) return false;
			if (!AreEqual(this.signatureAlgorithm, _Typed.signatureAlgorithm)) return false;
			if (!AreEqual(this.signatureValue, _Typed.signatureValue)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.acinfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureValue);
			return _Result;
		}
	}

	public partial class AttributeCertificateInfo : Asn1.Construct
	{
		public AttCertVersion version;
		public Holder holder;
		public AttCertIssuer issuer;
		public AlgorithmIdentifier signature;
		public System.Numerics.BigInteger serialNumber;
		public AttCertValidityPeriod attrCertValidityPeriod;
		public Attribute[] attributes;
		public byte[] issuerUniqueID;
		public Extension[] extensions;

		public static AttributeCertificateInfo Create(Asn1F.AsnReader Reader)
		{
			AttributeCertificateInfo _Result = new AttributeCertificateInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<AttCertVersion>(Reader, 0);
			this.holder = Holder.Create(Reader);
			this.issuer = AttCertIssuer.Create(Reader);
			this.signature = AlgorithmIdentifier.Create(Reader);
			this.serialNumber = ReadIntegerExplicit(Reader, 4);
			this.attrCertValidityPeriod = AttCertValidityPeriod.Create(Reader);
			this.attributes = ReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute.TryCreate(Reader, out _Item));
			TryReadBitStringExplicit(Reader, 7, this.issuerUniqueID, out this.issuerUniqueID);
			TryReadArray<Extension>(Reader, (Asn1F.AsnReader Reader, out Extension _Item) => Extension.TryCreate(Reader, out _Item), this.extensions, out this.extensions);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AttributeCertificateInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AttributeCertificateInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.holder.Write(Writer);
				this.issuer.Write(Writer);
				this.signature.Write(Writer);
				WriteIntegerImplicit(Writer, 4, this.serialNumber);
				this.attrCertValidityPeriod.Write(Writer);
				WriteArrayImplicit(Writer, 6, this.attributes);
				if (!(this.issuerUniqueID is null))
					WriteBitStringImplicit(Writer, 7, this.issuerUniqueID);
				if (!(this.extensions is null))
					WriteArrayImplicit(Writer, 8, this.extensions);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttributeCertificateInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.holder, _Typed.holder)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			if (!AreEqual(this.serialNumber, _Typed.serialNumber)) return false;
			if (!AreEqual(this.attrCertValidityPeriod, _Typed.attrCertValidityPeriod)) return false;
			if (!AreEqual(this.attributes, _Typed.attributes)) return false;
			if (!AreEqual(this.issuerUniqueID, _Typed.issuerUniqueID)) return false;
			if (!AreEqual(this.extensions, _Typed.extensions)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.holder);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			_Result ^= _Result << 5 ^ CalcHashCode(this.serialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attrCertValidityPeriod);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attributes);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerUniqueID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extensions);
			return _Result;
		}
	}

	public enum AttCertVersion
	{
		v2 = 1
	}

	public partial class Holder : Asn1.Construct
	{
		public IssuerSerial baseCertificateID;
		public GeneralName[] entityName;
		public ObjectDigestInfo objectDigestInfo;

		public static Holder Create(Asn1F.AsnReader Reader)
		{
			Holder _Result = new Holder();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			IssuerSerial.TryCreate(Reader, out this.baseCertificateID);
			TryReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item), this.entityName, out this.entityName);
			ObjectDigestInfo.TryCreate(Reader, out this.objectDigestInfo);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Holder _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Holder();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.baseCertificateID is null))
					this.baseCertificateID.Write(Writer);
				if (!(this.entityName is null))
					WriteArrayImplicit(Writer, 1, this.entityName);
				if (!(this.objectDigestInfo is null))
					this.objectDigestInfo.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Holder _Typed)) return false;
			if (!AreEqual(this.baseCertificateID, _Typed.baseCertificateID)) return false;
			if (!AreEqual(this.entityName, _Typed.entityName)) return false;
			if (!AreEqual(this.objectDigestInfo, _Typed.objectDigestInfo)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.baseCertificateID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.entityName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.objectDigestInfo);
			return _Result;
		}
	}

	public enum digestedObjectTypeEnum
	{
		publicKey = 0,
		publicKeyCert = 1,
		otherObjectTypes = 2
	}

	public partial class ObjectDigestInfo : Asn1.Construct
	{
		public digestedObjectTypeEnum digestedObjectType;
		public Asn1.ObjectId otherObjectTypeID;
		public AlgorithmIdentifier digestAlgorithm;
		public byte[] objectDigest;

		public static ObjectDigestInfo Create(Asn1F.AsnReader Reader)
		{
			ObjectDigestInfo _Result = new ObjectDigestInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.digestedObjectType = ReadEnumerationImplicit<digestedObjectTypeEnum>(Reader, 0);
			TryReadObjectIdImplicit(Reader, 1, this.otherObjectTypeID, out this.otherObjectTypeID);
			this.digestAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.objectDigest = ReadBitStringImplicit(Reader, 3);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out ObjectDigestInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new ObjectDigestInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteEnumerationImplicit(Writer, 0, this.digestedObjectType);
				if (!(this.otherObjectTypeID is null))
					WriteObjectIdImplicit(Writer, 1, this.otherObjectTypeID);
				this.digestAlgorithm.Write(Writer);
				WriteBitStringImplicit(Writer, 3, this.objectDigest);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ObjectDigestInfo _Typed)) return false;
			if (!AreEqual(this.digestedObjectType, _Typed.digestedObjectType)) return false;
			if (!AreEqual(this.otherObjectTypeID, _Typed.otherObjectTypeID)) return false;
			if (!AreEqual(this.digestAlgorithm, _Typed.digestAlgorithm)) return false;
			if (!AreEqual(this.objectDigest, _Typed.objectDigest)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.digestedObjectType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.otherObjectTypeID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.digestAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.objectDigest);
			return _Result;
		}
	}

	public partial class AttCertIssuer : Asn1.Construct
	{
		public enum _Options
		{
			v1Form,
			v2Form
		}

		public _Options _choice;
		public GeneralName[] v1Form;
		public V2Form v2Form;

		public static AttCertIssuer Create(Asn1F.AsnReader Reader)
		{
			AttCertIssuer _Result = new AttCertIssuer();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.v1Form = ReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item));
					this._choice = _Options.v1Form;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.v2Form = V2Form.Create(Reader);
					this._choice = _Options.v2Form;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out AttCertIssuer _Result)
		{
			try
			{
				_Result = new AttCertIssuer();
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
				case _Options.v1Form:
					if (!(this.v1Form is null))
						WriteArrayImplicit(Writer, 0, this.v1Form);
					break;

				case _Options.v2Form:
					if (!(this.v2Form is null))
						this.v2Form.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttCertIssuer _Typed)) return false;
			if (!AreEqual(this.v1Form, _Typed.v1Form)) return false;
			if (!AreEqual(this.v2Form, _Typed.v2Form)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.v1Form);
			_Result ^= _Result << 5 ^ CalcHashCode(this.v2Form);
			return _Result;
		}
	}

	public partial class V2Form : Asn1.Construct
	{
		public GeneralName[] issuerName;
		public IssuerSerial baseCertificateID;
		public ObjectDigestInfo objectDigestInfo;

		public static V2Form Create(Asn1F.AsnReader Reader)
		{
			V2Form _Result = new V2Form();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item), this.issuerName, out this.issuerName);
			IssuerSerial.TryCreate(Reader, out this.baseCertificateID);
			ObjectDigestInfo.TryCreate(Reader, out this.objectDigestInfo);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out V2Form _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new V2Form();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.issuerName is null))
					WriteArrayImplicit(Writer, 0, this.issuerName);
				if (!(this.baseCertificateID is null))
					this.baseCertificateID.Write(Writer);
				if (!(this.objectDigestInfo is null))
					this.objectDigestInfo.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is V2Form _Typed)) return false;
			if (!AreEqual(this.issuerName, _Typed.issuerName)) return false;
			if (!AreEqual(this.baseCertificateID, _Typed.baseCertificateID)) return false;
			if (!AreEqual(this.objectDigestInfo, _Typed.objectDigestInfo)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.baseCertificateID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.objectDigestInfo);
			return _Result;
		}
	}

	public partial class IssuerSerial : Asn1.Construct
	{
		public GeneralName[] issuer;
		public System.Numerics.BigInteger serial;
		public byte[] issuerUID;

		public static IssuerSerial Create(Asn1F.AsnReader Reader)
		{
			IssuerSerial _Result = new IssuerSerial();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.issuer = ReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item));
			this.serial = ReadIntegerExplicit(Reader, 1);
			TryReadBitStringExplicit(Reader, 2, this.issuerUID, out this.issuerUID);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IssuerSerial _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IssuerSerial();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteArrayImplicit(Writer, 0, this.issuer);
				WriteIntegerImplicit(Writer, 1, this.serial);
				if (!(this.issuerUID is null))
					WriteBitStringImplicit(Writer, 2, this.issuerUID);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IssuerSerial _Typed)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.serial, _Typed.serial)) return false;
			if (!AreEqual(this.issuerUID, _Typed.issuerUID)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.serial);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerUID);
			return _Result;
		}
	}

	public partial class AttCertValidityPeriod : Asn1.Construct
	{
		public System.TimeSpan notBeforeTime;
		public System.TimeSpan notAfterTime;

		public static AttCertValidityPeriod Create(Asn1F.AsnReader Reader)
		{
			AttCertValidityPeriod _Result = new AttCertValidityPeriod();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.notBeforeTime = ReadGeneralizedTimeImplicit(Reader, 0);
			this.notAfterTime = ReadGeneralizedTimeImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AttCertValidityPeriod _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AttCertValidityPeriod();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteGeneralizedTimeImplicit(Writer, 0, this.notBeforeTime);
				WriteGeneralizedTimeImplicit(Writer, 1, this.notAfterTime);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttCertValidityPeriod _Typed)) return false;
			if (!AreEqual(this.notBeforeTime, _Typed.notBeforeTime)) return false;
			if (!AreEqual(this.notAfterTime, _Typed.notAfterTime)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.notBeforeTime);
			_Result ^= _Result << 5 ^ CalcHashCode(this.notAfterTime);
			return _Result;
		}
	}

	public partial class Target : Asn1.Construct
	{
		public enum _Options
		{
			targetName,
			targetGroup,
			targetCert
		}

		public _Options _choice;
		public GeneralName targetName;
		public GeneralName targetGroup;
		public TargetCert targetCert;

		public static Target Create(Asn1F.AsnReader Reader)
		{
			Target _Result = new Target();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.targetName = GeneralName.Create(Reader);
					this._choice = _Options.targetName;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.targetGroup = GeneralName.Create(Reader);
					this._choice = _Options.targetGroup;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.targetCert = TargetCert.Create(Reader);
					this._choice = _Options.targetCert;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Target _Result)
		{
			try
			{
				_Result = new Target();
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
				case _Options.targetName:
					if (!(this.targetName is null))
						this.targetName.Write(Writer);
					break;

				case _Options.targetGroup:
					if (!(this.targetGroup is null))
						this.targetGroup.Write(Writer);
					break;

				case _Options.targetCert:
					if (!(this.targetCert is null))
						this.targetCert.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Target _Typed)) return false;
			if (!AreEqual(this.targetName, _Typed.targetName)) return false;
			if (!AreEqual(this.targetGroup, _Typed.targetGroup)) return false;
			if (!AreEqual(this.targetCert, _Typed.targetCert)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.targetName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.targetGroup);
			_Result ^= _Result << 5 ^ CalcHashCode(this.targetCert);
			return _Result;
		}
	}

	public partial class TargetCert : Asn1.Construct
	{
		public IssuerSerial targetCertificate;
		public GeneralName targetName;
		public ObjectDigestInfo certDigestInfo;

		public static TargetCert Create(Asn1F.AsnReader Reader)
		{
			TargetCert _Result = new TargetCert();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.targetCertificate = IssuerSerial.Create(Reader);
			GeneralName.TryCreate(Reader, out this.targetName);
			ObjectDigestInfo.TryCreate(Reader, out this.certDigestInfo);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out TargetCert _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new TargetCert();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.targetCertificate.Write(Writer);
				if (!(this.targetName is null))
					this.targetName.Write(Writer);
				if (!(this.certDigestInfo is null))
					this.certDigestInfo.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is TargetCert _Typed)) return false;
			if (!AreEqual(this.targetCertificate, _Typed.targetCertificate)) return false;
			if (!AreEqual(this.targetName, _Typed.targetName)) return false;
			if (!AreEqual(this.certDigestInfo, _Typed.certDigestInfo)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.targetCertificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.targetName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.certDigestInfo);
			return _Result;
		}
	}

	public partial class valuesChoice : Asn1.Construct
	{
		public enum _Options
		{
			octets,
			oid,
			@string
		}

		public _Options _choice;
		public byte[] octets;
		public Asn1.ObjectId oid;
		public string @string;

		public static valuesChoice Create(Asn1F.AsnReader Reader)
		{
			valuesChoice _Result = new valuesChoice();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 4: // OctetString
					this.octets = ReadOctetStringImplicit(Reader, 0);
					this._choice = _Options.octets;
					break;

				case 6: // ObjectIdentifier
					this.oid = ReadObjectIdImplicit(Reader, 1);
					this._choice = _Options.oid;
					break;

				case 7: // ObjectDescriptor
				case 12: // UTF8String
				case 18: // NumericString
				case 19: // PrintableString
				case 20: // TeletexString
				case 21: // VideotexString
				case 22: // IA5String
				case 25: // GraphicString
				case 26: // ISO646String
				case 27: // GeneralString
				case 28: // UniversalString
				case 29: // UnrestrictedCharacterString
				case 30: // BMPString
					this.@string = ReadStringImplicit(Reader, 2, Asn1F.UniversalTagNumber.UTF8String);
					this._choice = _Options.@string;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out valuesChoice _Result)
		{
			try
			{
				_Result = new valuesChoice();
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
				case _Options.octets:
					if (!(this.octets is null))
						WriteOctetStringImplicit(Writer, 0, this.octets);
					break;

				case _Options.oid:
					if (!(this.oid is null))
						WriteObjectIdImplicit(Writer, 1, this.oid);
					break;

				case _Options.@string:
					if (!(this.@string is null))
						WriteStringImplicit(Writer, 2, this.@string, Asn1F.UniversalTagNumber.UTF8String);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is valuesChoice _Typed)) return false;
			if (!AreEqual(this.octets, _Typed.octets)) return false;
			if (!AreEqual(this.oid, _Typed.oid)) return false;
			if (!AreEqual(this.@string, _Typed.@string)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.octets);
			_Result ^= _Result << 5 ^ CalcHashCode(this.oid);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@string);
			return _Result;
		}
	}

	public partial class IetfAttrSyntax : Asn1.Construct
	{
		public GeneralName[] policyAuthority;
		public valuesChoice[] values;

		public static IetfAttrSyntax Create(Asn1F.AsnReader Reader)
		{
			IetfAttrSyntax _Result = new IetfAttrSyntax();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item), this.policyAuthority, out this.policyAuthority);
			this.values = ReadArray<valuesChoice>(Reader, (Asn1F.AsnReader Reader, out valuesChoice _Item) => valuesChoice.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IetfAttrSyntax _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IetfAttrSyntax();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.policyAuthority is null))
					WriteArrayImplicit(Writer, 0, this.policyAuthority);
				WriteArrayImplicit(Writer, 1, this.values);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IetfAttrSyntax _Typed)) return false;
			if (!AreEqual(this.policyAuthority, _Typed.policyAuthority)) return false;
			if (!AreEqual(this.values, _Typed.values)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.policyAuthority);
			_Result ^= _Result << 5 ^ CalcHashCode(this.values);
			return _Result;
		}
	}

	public partial class SvceAuthInfo : Asn1.Construct
	{
		public GeneralName service;
		public GeneralName ident;
		public byte[] authInfo;

		public static SvceAuthInfo Create(Asn1F.AsnReader Reader)
		{
			SvceAuthInfo _Result = new SvceAuthInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.service = GeneralName.Create(Reader);
			this.ident = GeneralName.Create(Reader);
			TryReadOctetStringImplicit(Reader, 2, this.authInfo, out this.authInfo);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out SvceAuthInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new SvceAuthInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.service.Write(Writer);
				this.ident.Write(Writer);
				if (!(this.authInfo is null))
					WriteOctetStringImplicit(Writer, 2, this.authInfo);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SvceAuthInfo _Typed)) return false;
			if (!AreEqual(this.service, _Typed.service)) return false;
			if (!AreEqual(this.ident, _Typed.ident)) return false;
			if (!AreEqual(this.authInfo, _Typed.authInfo)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.service);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ident);
			_Result ^= _Result << 5 ^ CalcHashCode(this.authInfo);
			return _Result;
		}
	}

	public partial class RoleSyntax : Asn1.Construct
	{
		public GeneralName[] roleAuthority;
		public GeneralName roleName;

		public static RoleSyntax Create(Asn1F.AsnReader Reader)
		{
			RoleSyntax _Result = new RoleSyntax();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item), this.roleAuthority, out this.roleAuthority);
			this.roleName = GeneralName.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out RoleSyntax _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new RoleSyntax();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.roleAuthority is null))
					WriteArrayImplicit(Writer, 0, this.roleAuthority);
				this.roleName.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is RoleSyntax _Typed)) return false;
			if (!AreEqual(this.roleAuthority, _Typed.roleAuthority)) return false;
			if (!AreEqual(this.roleName, _Typed.roleName)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.roleAuthority);
			_Result ^= _Result << 5 ^ CalcHashCode(this.roleName);
			return _Result;
		}
	}

	public partial class Clearance : Asn1.Construct
	{
		public Asn1.ObjectId policyId;
		public ClassList? classList = ClassList.unclassified;
		public SecurityCategory[] securityCategories;

		public static Clearance Create(Asn1F.AsnReader Reader)
		{
			Clearance _Result = new Clearance();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.policyId = ReadObjectIdImplicit(Reader, 0);
			TryReadEnumerationImplicit<ClassList>(Reader, 1, this.classList, out this.classList);
			TryReadArray<SecurityCategory>(Reader, (Asn1F.AsnReader Reader, out SecurityCategory _Item) => SecurityCategory.TryCreate(Reader, out _Item), this.securityCategories, out this.securityCategories);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Clearance _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Clearance();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.policyId);
				if (this.classList.HasValue)
					WriteBitStringImplicit(Writer, 1, this.classList.Value);
				if (!(this.securityCategories is null))
					WriteArrayImplicit(Writer, 2, this.securityCategories);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Clearance _Typed)) return false;
			if (!AreEqual(this.policyId, _Typed.policyId)) return false;
			if (!AreEqual(this.classList, _Typed.classList)) return false;
			if (!AreEqual(this.securityCategories, _Typed.securityCategories)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.policyId);
			_Result ^= _Result << 5 ^ CalcHashCode(this.classList);
			_Result ^= _Result << 5 ^ CalcHashCode(this.securityCategories);
			return _Result;
		}
	}

	[System.Flags]
	public enum ClassList
	{
		unmarked = 1,
		unclassified = 2,
		restricted = 4,
		confidential = 8,
		secret = 16,
		topSecret = 32
	}

	public partial class SecurityCategory : Asn1.Construct
	{
		public Asn1.ObjectId type;
		public object @value;

		public static SecurityCategory Create(Asn1F.AsnReader Reader)
		{
			SecurityCategory _Result = new SecurityCategory();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type = ReadObjectIdImplicit(Reader, 0);
			this.@value = ReadAnyImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out SecurityCategory _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new SecurityCategory();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.type);
				WriteAnyImplicit(Writer, 1, this.@value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SecurityCategory _Typed)) return false;
			if (!AreEqual(this.type, _Typed.type)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.type);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

	public partial class AAControls : Asn1.Construct
	{
		public System.Numerics.BigInteger? pathLenConstraint;
		public Asn1.ObjectId[] permittedAttrs;
		public Asn1.ObjectId[] excludedAttrs;
		public bool? permitUnSpecified = true;

		public static AAControls Create(Asn1F.AsnReader Reader)
		{
			AAControls _Result = new AAControls();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadIntegerImplicit(Reader, 0, this.pathLenConstraint, out this.pathLenConstraint);
			if (!(this.pathLenConstraint is null))
			{
				if (!(Asn1.Construct.CheckInSet(this.pathLenConstraint, _DefinedValues.pathLenConstraintInSet)))
					throw new Asn1F.AsnContentException("pathLenConstraint out of bounds.");
			}
			TryReadArray<Asn1.ObjectId>(Reader, (Asn1F.AsnReader Reader, out Asn1.ObjectId _Item) => TryReadObjectId(Reader, default, out _Item), this.permittedAttrs, out this.permittedAttrs);
			TryReadArray<Asn1.ObjectId>(Reader, (Asn1F.AsnReader Reader, out Asn1.ObjectId _Item) => TryReadObjectId(Reader, default, out _Item), this.excludedAttrs, out this.excludedAttrs);
			TryReadBooleanImplicit(Reader, 3, this.permitUnSpecified, out this.permitUnSpecified);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AAControls _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AAControls();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.pathLenConstraint is null))
				{
					if (!(Asn1.Construct.CheckInSet(this.pathLenConstraint, _DefinedValues.pathLenConstraintInSet)))
						throw new Asn1F.AsnContentException("pathLenConstraint out of bounds.");
				}
				if (this.pathLenConstraint.HasValue)
					WriteIntegerImplicit(Writer, 0, this.pathLenConstraint.Value);
				if (!(this.permittedAttrs is null))
					WriteArrayImplicit(Writer, 1, this.permittedAttrs);
				if (!(this.excludedAttrs is null))
					WriteArrayImplicit(Writer, 2, this.excludedAttrs);
				if (this.permitUnSpecified.HasValue)
					WriteBooleanImplicit(Writer, 3, this.permitUnSpecified.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AAControls _Typed)) return false;
			if (!AreEqual(this.pathLenConstraint, _Typed.pathLenConstraint)) return false;
			if (!AreEqual(this.permittedAttrs, _Typed.permittedAttrs)) return false;
			if (!AreEqual(this.excludedAttrs, _Typed.excludedAttrs)) return false;
			if (!AreEqual(this.permitUnSpecified, _Typed.permitUnSpecified)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.pathLenConstraint);
			_Result ^= _Result << 5 ^ CalcHashCode(this.permittedAttrs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.excludedAttrs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.permitUnSpecified);
			return _Result;
		}
	}

	public partial class ACClearAttrs : Asn1.Construct
	{
		public GeneralName acIssuer;
		public System.Numerics.BigInteger acSerial;
		public Attribute[] attrs;

		public static ACClearAttrs Create(Asn1F.AsnReader Reader)
		{
			ACClearAttrs _Result = new ACClearAttrs();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.acIssuer = GeneralName.Create(Reader);
			this.acSerial = ReadIntegerImplicit(Reader, 1);
			this.attrs = ReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out ACClearAttrs _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new ACClearAttrs();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.acIssuer.Write(Writer);
				WriteIntegerImplicit(Writer, 1, this.acSerial);
				WriteArrayImplicit(Writer, 2, this.attrs);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ACClearAttrs _Typed)) return false;
			if (!AreEqual(this.acIssuer, _Typed.acIssuer)) return false;
			if (!AreEqual(this.acSerial, _Typed.acSerial)) return false;
			if (!AreEqual(this.attrs, _Typed.attrs)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.acIssuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.acSerial);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attrs);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
