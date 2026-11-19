using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Security.PKCS.Certificates.PKIX1Explicit88;
using Waher.Security.PKCS.Certificates.PKIX1Implicit88;
using Waher.Security.PKCS.Certificates.CryptographicMessageSyntax;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Security\Waher.Security.PKCS\Certificates\Asn1\PKIXCRMF.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Security.PKCS.Certificates.CRMF
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId id_pkix = new Asn1.ObjectId(1 /* iso */, 3 /* identified_organization */, 6 /* dod */, 1 /* internet */, 5 /* security */, 5 /* mechanisms */, 7);
		public static readonly Asn1.ObjectId id_pkip = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_pkix, 5);
		public static readonly Asn1.ObjectId id_regCtrl = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_pkip, 1);
		public static readonly Asn1.ObjectId id_regCtrl_regToken = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regCtrl, 1);
		public static readonly Asn1.ObjectId id_regCtrl_authenticator = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regCtrl, 2);
		public static readonly Asn1.ObjectId id_regCtrl_pkiPublicationInfo = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regCtrl, 3);
		public static readonly Asn1.ObjectId id_regCtrl_pkiArchiveOptions = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regCtrl, 4);
		public static readonly Asn1.ObjectId id_regCtrl_oldCertID = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regCtrl, 5);
		public static readonly Asn1.ObjectId id_regCtrl_protocolEncrKey = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regCtrl, 6);
		public static readonly Asn1.ObjectId id_regInfo = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_pkip, 2);
		public static readonly Asn1.ObjectId id_regInfo_utf8Pairs = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regInfo, 1);
		public static readonly Asn1.ObjectId id_regInfo_certReq = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.CRMF._DefinedValues.id_regInfo, 2);
	}

	public partial class CertReqMsg : Asn1.Construct
	{
		public CertRequest certReq;
		public ProofOfPossession pop;
		public AttributeTypeAndValue[] regInfo;

		public static CertReqMsg Create(Asn1F.AsnReader Reader)
		{
			CertReqMsg _Result = new CertReqMsg();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.certReq = CertRequest.Create(Reader);
			ProofOfPossession.TryCreate(Reader, out this.pop);
			TryReadArray<AttributeTypeAndValue>(Reader, (Asn1F.AsnReader Reader, out AttributeTypeAndValue _Item) => Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue.TryCreate(Reader, out _Item), this.regInfo, out this.regInfo);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out CertReqMsg _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new CertReqMsg();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.certReq.Write(Writer);
				if (!(this.pop is null))
					this.pop.Write(Writer);
				if (!(this.regInfo is null))
					WriteArrayImplicit(Writer, 2, this.regInfo);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CertReqMsg _Typed)) return false;
			if (!AreEqual(this.certReq, _Typed.certReq)) return false;
			if (!AreEqual(this.pop, _Typed.pop)) return false;
			if (!AreEqual(this.regInfo, _Typed.regInfo)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.certReq);
			_Result ^= _Result << 5 ^ CalcHashCode(this.pop);
			_Result ^= _Result << 5 ^ CalcHashCode(this.regInfo);
			return _Result;
		}
	}

	public partial class CertRequest : Asn1.Construct
	{
		public System.Numerics.BigInteger certReqId;
		public CertTemplate certTemplate;
		public Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue[] controls;

		public static CertRequest Create(Asn1F.AsnReader Reader)
		{
			CertRequest _Result = new CertRequest();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.certReqId = ReadIntegerImplicit(Reader, 0);
			this.certTemplate = CertTemplate.Create(Reader);
			TryReadArray<AttributeTypeAndValue>(Reader, (Asn1F.AsnReader Reader, out AttributeTypeAndValue _Item) => Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue.TryCreate(Reader, out _Item), this.controls, out this.controls);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out CertRequest _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new CertRequest();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, this.certReqId);
				this.certTemplate.Write(Writer);
				if (!(this.controls is null))
					WriteArrayImplicit(Writer, 2, this.controls);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CertRequest _Typed)) return false;
			if (!AreEqual(this.certReqId, _Typed.certReqId)) return false;
			if (!AreEqual(this.certTemplate, _Typed.certTemplate)) return false;
			if (!AreEqual(this.controls, _Typed.controls)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.certReqId);
			_Result ^= _Result << 5 ^ CalcHashCode(this.certTemplate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.controls);
			return _Result;
		}
	}

	public partial class CertTemplate : Asn1.Construct
	{
		public Version? version;
		public System.Numerics.BigInteger? serialNumber;
		public AlgorithmIdentifier signingAlg;
		public Name issuer;
		public OptionalValidity validity;
		public Name subject;
		public SubjectPublicKeyInfo publicKey;
		public byte[] issuerUID;
		public byte[] subjectUID;
		public Extension[] extensions;

		public static CertTemplate Create(Asn1F.AsnReader Reader)
		{
			CertTemplate _Result = new CertTemplate();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadEnumerationExplicit<Version>(Reader, 0, this.version, out this.version);
			TryReadIntegerImplicit(Reader, 1, this.serialNumber, out this.serialNumber);
			AlgorithmIdentifier.TryCreate(Reader, out this.signingAlg);
			Name.TryCreate(Reader, out this.issuer);
			OptionalValidity.TryCreate(Reader, out this.validity);
			Name.TryCreate(Reader, out this.subject);
			SubjectPublicKeyInfo.TryCreate(Reader, out this.publicKey);
			TryReadBitStringExplicit(Reader, 7, this.issuerUID, out this.issuerUID);
			TryReadBitStringExplicit(Reader, 8, this.subjectUID, out this.subjectUID);
			TryReadArray<Extension>(Reader, (Asn1F.AsnReader Reader, out Extension _Item) => Extension.TryCreate(Reader, out _Item), this.extensions, out this.extensions);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out CertTemplate _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new CertTemplate();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (this.version.HasValue)
					WriteIntegerImplicit(Writer, 0, (int)this.version.Value);
				if (this.serialNumber.HasValue)
					WriteIntegerImplicit(Writer, 1, this.serialNumber.Value);
				if (!(this.signingAlg is null))
					this.signingAlg.Write(Writer);
				if (!(this.issuer is null))
					this.issuer.Write(Writer);
				if (!(this.validity is null))
					this.validity.Write(Writer);
				if (!(this.subject is null))
					this.subject.Write(Writer);
				if (!(this.publicKey is null))
					this.publicKey.Write(Writer);
				if (!(this.issuerUID is null))
					WriteBitStringImplicit(Writer, 7, this.issuerUID);
				if (!(this.subjectUID is null))
					WriteBitStringImplicit(Writer, 8, this.subjectUID);
				if (!(this.extensions is null))
					WriteArrayImplicit(Writer, 9, this.extensions);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CertTemplate _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.serialNumber, _Typed.serialNumber)) return false;
			if (!AreEqual(this.signingAlg, _Typed.signingAlg)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.validity, _Typed.validity)) return false;
			if (!AreEqual(this.subject, _Typed.subject)) return false;
			if (!AreEqual(this.publicKey, _Typed.publicKey)) return false;
			if (!AreEqual(this.issuerUID, _Typed.issuerUID)) return false;
			if (!AreEqual(this.subjectUID, _Typed.subjectUID)) return false;
			if (!AreEqual(this.extensions, _Typed.extensions)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.serialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signingAlg);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.validity);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subject);
			_Result ^= _Result << 5 ^ CalcHashCode(this.publicKey);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerUID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectUID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extensions);
			return _Result;
		}
	}

	public partial class OptionalValidity : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time notBefore;
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time notAfter;

		public static OptionalValidity Create(Asn1F.AsnReader Reader)
		{
			OptionalValidity _Result = new OptionalValidity();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.TryCreate(Reader, out this.notBefore);
			Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.TryCreate(Reader, out this.notAfter);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out OptionalValidity _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new OptionalValidity();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.notBefore is null))
					this.notBefore.Write(Writer);
				if (!(this.notAfter is null))
					this.notAfter.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is OptionalValidity _Typed)) return false;
			if (!AreEqual(this.notBefore, _Typed.notBefore)) return false;
			if (!AreEqual(this.notAfter, _Typed.notAfter)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.notBefore);
			_Result ^= _Result << 5 ^ CalcHashCode(this.notAfter);
			return _Result;
		}
	}

	public partial class AttributeTypeAndValue : Asn1.Construct
	{
		public Asn1.ObjectId type;
		public object @value;

		public static Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue _Result = new Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type = ReadObjectIdImplicit(Reader, 0);
			this.@value = ReadAnyImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CRMF.AttributeTypeAndValue();
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
			if (!(Object is AttributeTypeAndValue _Typed)) return false;
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

	public partial class ProofOfPossession : Asn1.Construct
	{
		public enum _Options
		{
			raVerified,
			signature,
			keyEncipherment,
			keyAgreement
		}

		public _Options _choice;
		public object raVerified;
		public POPOSigningKey signature;
		public POPOPrivKey keyEncipherment;
		public POPOPrivKey keyAgreement;

		public static ProofOfPossession Create(Asn1F.AsnReader Reader)
		{
			ProofOfPossession _Result = new ProofOfPossession();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.raVerified = ReadNullImplicit(Reader, 0);
					this._choice = _Options.raVerified;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.signature = POPOSigningKey.Create(Reader);
					this._choice = _Options.signature;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.keyEncipherment = POPOPrivKey.Create(Reader);
					this._choice = _Options.keyEncipherment;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.keyAgreement = POPOPrivKey.Create(Reader);
					this._choice = _Options.keyAgreement;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out ProofOfPossession _Result)
		{
			try
			{
				_Result = new ProofOfPossession();
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
				case _Options.raVerified:
					if (!(this.raVerified is null))
						WriteNullImplicit(Writer, 0);
					break;

				case _Options.signature:
					if (!(this.signature is null))
						this.signature.Write(Writer);
					break;

				case _Options.keyEncipherment:
					if (!(this.keyEncipherment is null))
						this.keyEncipherment.Write(Writer);
					break;

				case _Options.keyAgreement:
					if (!(this.keyAgreement is null))
						this.keyAgreement.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ProofOfPossession _Typed)) return false;
			if (!AreEqual(this.raVerified, _Typed.raVerified)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			if (!AreEqual(this.keyEncipherment, _Typed.keyEncipherment)) return false;
			if (!AreEqual(this.keyAgreement, _Typed.keyAgreement)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.raVerified);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyEncipherment);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyAgreement);
			return _Result;
		}
	}

	public partial class POPOSigningKey : Asn1.Construct
	{
		public POPOSigningKeyInput poposkInput;
		public AlgorithmIdentifier algorithmIdentifier;
		public byte[] signature;

		public static POPOSigningKey Create(Asn1F.AsnReader Reader)
		{
			POPOSigningKey _Result = new POPOSigningKey();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			POPOSigningKeyInput.TryCreate(Reader, out this.poposkInput);
			this.algorithmIdentifier = AlgorithmIdentifier.Create(Reader);
			this.signature = ReadBitStringImplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out POPOSigningKey _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new POPOSigningKey();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.poposkInput is null))
					this.poposkInput.Write(Writer);
				this.algorithmIdentifier.Write(Writer);
				WriteBitStringImplicit(Writer, 2, this.signature);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is POPOSigningKey _Typed)) return false;
			if (!AreEqual(this.poposkInput, _Typed.poposkInput)) return false;
			if (!AreEqual(this.algorithmIdentifier, _Typed.algorithmIdentifier)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.poposkInput);
			_Result ^= _Result << 5 ^ CalcHashCode(this.algorithmIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			return _Result;
		}
	}

	public partial class authInfoChoice : Asn1.Construct
	{
		public enum _Options
		{
			sender,
			publicKeyMAC
		}

		public _Options _choice;
		public GeneralName sender;
		public PKMACValue publicKeyMAC;

		public static authInfoChoice Create(Asn1F.AsnReader Reader)
		{
			authInfoChoice _Result = new authInfoChoice();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.sender = GeneralName.Create(Reader);
					this._choice = _Options.sender;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.publicKeyMAC = PKMACValue.Create(Reader);
					this._choice = _Options.publicKeyMAC;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out authInfoChoice _Result)
		{
			try
			{
				_Result = new authInfoChoice();
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
				case _Options.sender:
					if (!(this.sender is null))
						this.sender.Write(Writer);
					break;

				case _Options.publicKeyMAC:
					if (!(this.publicKeyMAC is null))
						this.publicKeyMAC.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is authInfoChoice _Typed)) return false;
			if (!AreEqual(this.sender, _Typed.sender)) return false;
			if (!AreEqual(this.publicKeyMAC, _Typed.publicKeyMAC)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.sender);
			_Result ^= _Result << 5 ^ CalcHashCode(this.publicKeyMAC);
			return _Result;
		}
	}

	public partial class POPOSigningKeyInput : Asn1.Construct
	{
		public authInfoChoice authInfo;
		public SubjectPublicKeyInfo publicKey;

		public static POPOSigningKeyInput Create(Asn1F.AsnReader Reader)
		{
			POPOSigningKeyInput _Result = new POPOSigningKeyInput();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.authInfo = authInfoChoice.Create(Reader);
			this.publicKey = SubjectPublicKeyInfo.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out POPOSigningKeyInput _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new POPOSigningKeyInput();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.authInfo.Write(Writer);
				this.publicKey.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is POPOSigningKeyInput _Typed)) return false;
			if (!AreEqual(this.authInfo, _Typed.authInfo)) return false;
			if (!AreEqual(this.publicKey, _Typed.publicKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.authInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.publicKey);
			return _Result;
		}
	}

	public partial class PKMACValue : Asn1.Construct
	{
		public AlgorithmIdentifier algId;
		public byte[] @value;

		public static PKMACValue Create(Asn1F.AsnReader Reader)
		{
			PKMACValue _Result = new PKMACValue();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.algId = AlgorithmIdentifier.Create(Reader);
			this.@value = ReadBitStringImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PKMACValue _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PKMACValue();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.algId.Write(Writer);
				WriteBitStringImplicit(Writer, 1, this.@value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PKMACValue _Typed)) return false;
			if (!AreEqual(this.algId, _Typed.algId)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.algId);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

	public partial class PBMParameter : Asn1.Construct
	{
		public byte[] salt;
		public AlgorithmIdentifier owf;
		public System.Numerics.BigInteger iterationCount;
		public AlgorithmIdentifier mac;

		public static PBMParameter Create(Asn1F.AsnReader Reader)
		{
			PBMParameter _Result = new PBMParameter();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.salt = ReadOctetStringImplicit(Reader, 0);
			this.owf = AlgorithmIdentifier.Create(Reader);
			this.iterationCount = ReadIntegerImplicit(Reader, 2);
			this.mac = AlgorithmIdentifier.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PBMParameter _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PBMParameter();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteOctetStringImplicit(Writer, 0, this.salt);
				this.owf.Write(Writer);
				WriteIntegerImplicit(Writer, 2, this.iterationCount);
				this.mac.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PBMParameter _Typed)) return false;
			if (!AreEqual(this.salt, _Typed.salt)) return false;
			if (!AreEqual(this.owf, _Typed.owf)) return false;
			if (!AreEqual(this.iterationCount, _Typed.iterationCount)) return false;
			if (!AreEqual(this.mac, _Typed.mac)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.salt);
			_Result ^= _Result << 5 ^ CalcHashCode(this.owf);
			_Result ^= _Result << 5 ^ CalcHashCode(this.iterationCount);
			_Result ^= _Result << 5 ^ CalcHashCode(this.mac);
			return _Result;
		}
	}

	public partial class POPOPrivKey : Asn1.Construct
	{
		public enum _Options
		{
			thisMessage,
			subsequentMessage,
			dhMAC
		}

		public _Options _choice;
		public byte[] thisMessage;
		public SubsequentMessage? subsequentMessage;
		public byte[] dhMAC;

		public static POPOPrivKey Create(Asn1F.AsnReader Reader)
		{
			POPOPrivKey _Result = new POPOPrivKey();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.thisMessage = ReadBitStringImplicit(Reader, 0);
					this._choice = _Options.thisMessage;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.subsequentMessage = ReadEnumerationImplicit<SubsequentMessage>(Reader, 1);
					this._choice = _Options.subsequentMessage;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.dhMAC = ReadBitStringImplicit(Reader, 2);
					this._choice = _Options.dhMAC;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out POPOPrivKey _Result)
		{
			try
			{
				_Result = new POPOPrivKey();
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
				case _Options.thisMessage:
					if (!(this.thisMessage is null))
						WriteBitStringImplicit(Writer, 0, this.thisMessage);
					break;

				case _Options.subsequentMessage:
					if (this.subsequentMessage.HasValue)
						WriteIntegerImplicit(Writer, 1, (int)this.subsequentMessage.Value);
					break;

				case _Options.dhMAC:
					if (!(this.dhMAC is null))
						WriteBitStringImplicit(Writer, 2, this.dhMAC);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is POPOPrivKey _Typed)) return false;
			if (!AreEqual(this.thisMessage, _Typed.thisMessage)) return false;
			if (!AreEqual(this.subsequentMessage, _Typed.subsequentMessage)) return false;
			if (!AreEqual(this.dhMAC, _Typed.dhMAC)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.thisMessage);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subsequentMessage);
			_Result ^= _Result << 5 ^ CalcHashCode(this.dhMAC);
			return _Result;
		}
	}

	public enum SubsequentMessage
	{
		encrCert = 0,
		challengeResp = 1
	}

	public enum actionEnum
	{
		dontPublish = 0,
		pleasePublish = 1
	}

	public partial class PKIPublicationInfo : Asn1.Construct
	{
		public actionEnum action;
		public SinglePubInfo[] pubInfos;

		public static PKIPublicationInfo Create(Asn1F.AsnReader Reader)
		{
			PKIPublicationInfo _Result = new PKIPublicationInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.action = ReadEnumerationImplicit<actionEnum>(Reader, 0);
			TryReadArray<SinglePubInfo>(Reader, (Asn1F.AsnReader Reader, out SinglePubInfo _Item) => SinglePubInfo.TryCreate(Reader, out _Item), this.pubInfos, out this.pubInfos);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PKIPublicationInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PKIPublicationInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.action);
				if (!(this.pubInfos is null))
					WriteArrayImplicit(Writer, 1, this.pubInfos);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PKIPublicationInfo _Typed)) return false;
			if (!AreEqual(this.action, _Typed.action)) return false;
			if (!AreEqual(this.pubInfos, _Typed.pubInfos)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.action);
			_Result ^= _Result << 5 ^ CalcHashCode(this.pubInfos);
			return _Result;
		}
	}

	public enum pubMethodEnum
	{
		dontCare = 0,
		x500 = 1,
		web = 2,
		ldap = 3
	}

	public partial class SinglePubInfo : Asn1.Construct
	{
		public pubMethodEnum pubMethod;
		public GeneralName pubLocation;

		public static SinglePubInfo Create(Asn1F.AsnReader Reader)
		{
			SinglePubInfo _Result = new SinglePubInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.pubMethod = ReadEnumerationImplicit<pubMethodEnum>(Reader, 0);
			GeneralName.TryCreate(Reader, out this.pubLocation);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out SinglePubInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new SinglePubInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.pubMethod);
				if (!(this.pubLocation is null))
					this.pubLocation.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SinglePubInfo _Typed)) return false;
			if (!AreEqual(this.pubMethod, _Typed.pubMethod)) return false;
			if (!AreEqual(this.pubLocation, _Typed.pubLocation)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.pubMethod);
			_Result ^= _Result << 5 ^ CalcHashCode(this.pubLocation);
			return _Result;
		}
	}

	public partial class PKIArchiveOptions : Asn1.Construct
	{
		public enum _Options
		{
			encryptedPrivKey,
			keyGenParameters,
			archiveRemGenPrivKey
		}

		public _Options _choice;
		public Waher.Security.PKCS.Certificates.CRMF.EncryptedKey encryptedPrivKey;
		public byte[] keyGenParameters;
		public bool? archiveRemGenPrivKey;

		public static PKIArchiveOptions Create(Asn1F.AsnReader Reader)
		{
			PKIArchiveOptions _Result = new PKIArchiveOptions();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.encryptedPrivKey = Waher.Security.PKCS.Certificates.CRMF.EncryptedKey.Create(Reader);
					this._choice = _Options.encryptedPrivKey;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.keyGenParameters = ReadOctetStringImplicit(Reader, 1);
					this._choice = _Options.keyGenParameters;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.archiveRemGenPrivKey = ReadBooleanImplicit(Reader, 2);
					this._choice = _Options.archiveRemGenPrivKey;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out PKIArchiveOptions _Result)
		{
			try
			{
				_Result = new PKIArchiveOptions();
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
				case _Options.encryptedPrivKey:
					if (!(this.encryptedPrivKey is null))
						this.encryptedPrivKey.Write(Writer);
					break;

				case _Options.keyGenParameters:
					if (!(this.keyGenParameters is null))
						WriteOctetStringImplicit(Writer, 1, this.keyGenParameters);
					break;

				case _Options.archiveRemGenPrivKey:
					if (this.archiveRemGenPrivKey.HasValue)
						WriteBooleanImplicit(Writer, 2, this.archiveRemGenPrivKey.Value);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PKIArchiveOptions _Typed)) return false;
			if (!AreEqual(this.encryptedPrivKey, _Typed.encryptedPrivKey)) return false;
			if (!AreEqual(this.keyGenParameters, _Typed.keyGenParameters)) return false;
			if (!AreEqual(this.archiveRemGenPrivKey, _Typed.archiveRemGenPrivKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedPrivKey);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyGenParameters);
			_Result ^= _Result << 5 ^ CalcHashCode(this.archiveRemGenPrivKey);
			return _Result;
		}
	}

	public partial class EncryptedKey : Asn1.Construct
	{
		public enum _Options
		{
			encryptedValue,
			envelopedData
		}

		public _Options _choice;
		public EncryptedValue encryptedValue;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData envelopedData;

		public static Waher.Security.PKCS.Certificates.CRMF.EncryptedKey Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CRMF.EncryptedKey _Result = new Waher.Security.PKCS.Certificates.CRMF.EncryptedKey();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.encryptedValue = EncryptedValue.Create(Reader);
					this._choice = _Options.encryptedValue;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.envelopedData = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData.Create(Reader);
					this._choice = _Options.envelopedData;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CRMF.EncryptedKey _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CRMF.EncryptedKey();
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
				case _Options.encryptedValue:
					if (!(this.encryptedValue is null))
						this.encryptedValue.Write(Writer);
					break;

				case _Options.envelopedData:
					if (!(this.envelopedData is null))
						this.envelopedData.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EncryptedKey _Typed)) return false;
			if (!AreEqual(this.encryptedValue, _Typed.encryptedValue)) return false;
			if (!AreEqual(this.envelopedData, _Typed.envelopedData)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedValue);
			_Result ^= _Result << 5 ^ CalcHashCode(this.envelopedData);
			return _Result;
		}
	}

	public partial class EncryptedValue : Asn1.Construct
	{
		public AlgorithmIdentifier intendedAlg;
		public AlgorithmIdentifier symmAlg;
		public byte[] encSymmKey;
		public AlgorithmIdentifier keyAlg;
		public byte[] valueHint;
		public byte[] encValue;

		public static EncryptedValue Create(Asn1F.AsnReader Reader)
		{
			EncryptedValue _Result = new EncryptedValue();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			AlgorithmIdentifier.TryCreate(Reader, out this.intendedAlg);
			AlgorithmIdentifier.TryCreate(Reader, out this.symmAlg);
			TryReadBitStringImplicit(Reader, 2, this.encSymmKey, out this.encSymmKey);
			AlgorithmIdentifier.TryCreate(Reader, out this.keyAlg);
			TryReadOctetStringImplicit(Reader, 4, this.valueHint, out this.valueHint);
			this.encValue = ReadBitStringImplicit(Reader, 5);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out EncryptedValue _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new EncryptedValue();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.intendedAlg is null))
					this.intendedAlg.Write(Writer);
				if (!(this.symmAlg is null))
					this.symmAlg.Write(Writer);
				if (!(this.encSymmKey is null))
					WriteBitStringImplicit(Writer, 2, this.encSymmKey);
				if (!(this.keyAlg is null))
					this.keyAlg.Write(Writer);
				if (!(this.valueHint is null))
					WriteOctetStringImplicit(Writer, 4, this.valueHint);
				WriteBitStringImplicit(Writer, 5, this.encValue);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EncryptedValue _Typed)) return false;
			if (!AreEqual(this.intendedAlg, _Typed.intendedAlg)) return false;
			if (!AreEqual(this.symmAlg, _Typed.symmAlg)) return false;
			if (!AreEqual(this.encSymmKey, _Typed.encSymmKey)) return false;
			if (!AreEqual(this.keyAlg, _Typed.keyAlg)) return false;
			if (!AreEqual(this.valueHint, _Typed.valueHint)) return false;
			if (!AreEqual(this.encValue, _Typed.encValue)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.intendedAlg);
			_Result ^= _Result << 5 ^ CalcHashCode(this.symmAlg);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encSymmKey);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyAlg);
			_Result ^= _Result << 5 ^ CalcHashCode(this.valueHint);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encValue);
			return _Result;
		}
	}

	public partial class CertId : Asn1.Construct
	{
		public GeneralName issuer;
		public System.Numerics.BigInteger serialNumber;

		public static CertId Create(Asn1F.AsnReader Reader)
		{
			CertId _Result = new CertId();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.issuer = GeneralName.Create(Reader);
			this.serialNumber = ReadIntegerImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out CertId _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new CertId();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.issuer.Write(Writer);
				WriteIntegerImplicit(Writer, 1, this.serialNumber);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CertId _Typed)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.serialNumber, _Typed.serialNumber)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.serialNumber);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
