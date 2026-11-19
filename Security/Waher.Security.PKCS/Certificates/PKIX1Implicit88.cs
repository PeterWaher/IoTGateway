using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Security.PKCS.Certificates.PKIX1Explicit88;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Security\Waher.Security.PKCS\Certificates\Asn1\PKIX1Implicit88.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Security.PKCS.Certificates.PKIX1Implicit88
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId id_ce = new Asn1.ObjectId(2 /* joint_iso_ccitt */, 5 /* ds */, 29);
		public static readonly Asn1.ObjectId id_ce_authorityKeyIdentifier = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 35);
		public static readonly Asn1.ObjectId id_ce_subjectKeyIdentifier = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 14);
		public static readonly Asn1.ObjectId id_ce_keyUsage = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 15);
		public static readonly Asn1.ObjectId id_ce_privateKeyUsagePeriod = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 16);
		public static readonly Asn1.ObjectId id_ce_certificatePolicies = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 32);
		public static readonly Asn1.ObjectId anyPolicy = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce_certificatePolicies, 0);
		public static readonly Asn1.Construct.ISet policyQualifierIdInSet = new Asn1.Construct.Union(new Asn1.Construct.SingleValue<Asn1.ObjectId>(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_qt_cps), new Asn1.Construct.SingleValue<Asn1.ObjectId>(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_qt_unotice));
		public static readonly Asn1.Construct.ISet ia5StringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, 200);
		public static readonly Asn1.Construct.ISet visibleStringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, 200);
		public static readonly Asn1.Construct.ISet bmpStringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, 200);
		public static readonly Asn1.Construct.ISet utf8StringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, 200);
		public static readonly Asn1.ObjectId id_ce_policyMappings = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 33);
		public static readonly Asn1.ObjectId id_ce_subjectAltName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 17);
		public static readonly Asn1.ObjectId id_ce_issuerAltName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 18);
		public static readonly Asn1.ObjectId id_ce_subjectDirectoryAttributes = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 9);
		public static readonly Asn1.ObjectId id_ce_basicConstraints = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 19);
		public static readonly Asn1.Construct.ISet pathLenConstraintInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
		public static readonly Asn1.ObjectId id_ce_nameConstraints = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 30);
		public static readonly Asn1.Construct.ISet minimumInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
		public static readonly Asn1.Construct.ISet maximumInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
		public static readonly Asn1.ObjectId id_ce_policyConstraints = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 36);
		public static readonly Asn1.Construct.ISet requireExplicitPolicyInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
		public static readonly Asn1.Construct.ISet inhibitPolicyMappingInSet = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(0);
		public static readonly Asn1.ObjectId id_ce_cRLDistributionPoints = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 31);
		public static readonly Asn1.ObjectId id_ce_extKeyUsage = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 37);
		public static readonly Asn1.ObjectId anyExtendedKeyUsage = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce_extKeyUsage, 0);
		public static readonly Asn1.ObjectId id_kp_serverAuth = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_kp, 1);
		public static readonly Asn1.ObjectId id_kp_clientAuth = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_kp, 2);
		public static readonly Asn1.ObjectId id_kp_codeSigning = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_kp, 3);
		public static readonly Asn1.ObjectId id_kp_emailProtection = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_kp, 4);
		public static readonly Asn1.ObjectId id_kp_timeStamping = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_kp, 8);
		public static readonly Asn1.ObjectId id_kp_OCSPSigning = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_kp, 9);
		public static readonly Asn1.ObjectId id_ce_inhibitAnyPolicy = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 54);
		public static readonly Asn1.ObjectId id_ce_freshestCRL = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 46);
		public static readonly Asn1.ObjectId id_pe_authorityInfoAccess = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pe, 1);
		public static readonly Asn1.ObjectId id_pe_subjectInfoAccess = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pe, 11);
		public static readonly Asn1.ObjectId id_ce_cRLNumber = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 20);
		public static readonly Asn1.ObjectId id_ce_issuingDistributionPoint = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 28);
		public static readonly Asn1.ObjectId id_ce_deltaCRLIndicator = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 27);
		public static readonly Asn1.ObjectId id_ce_cRLReasons = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 21);
		public static readonly Asn1.ObjectId id_ce_certificateIssuer = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 29);
		public static readonly Asn1.ObjectId id_ce_holdInstructionCode = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 23);
		public static readonly Asn1.ObjectId holdInstruction = new Asn1.ObjectId(2 /* joint_iso_itu_t */, 2 /* member_body */, 840 /* us */, 10040 /* x9cm */, 2);
		public static readonly Asn1.ObjectId id_holdinstruction_none = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.holdInstruction, 1);
		public static readonly Asn1.ObjectId id_holdinstruction_callissuer = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.holdInstruction, 2);
		public static readonly Asn1.ObjectId id_holdinstruction_reject = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.holdInstruction, 3);
		public static readonly Asn1.ObjectId id_ce_invalidityDate = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Implicit88._DefinedValues.id_ce, 24);
	}

	public partial class AuthorityKeyIdentifier : Asn1.Construct
	{
		public byte[] keyIdentifier;
		public GeneralName[] authorityCertIssuer;
		public System.Numerics.BigInteger? authorityCertSerialNumber;

		public static AuthorityKeyIdentifier Create(Asn1F.AsnReader Reader)
		{
			AuthorityKeyIdentifier _Result = new AuthorityKeyIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadOctetStringImplicit(Reader, 0, this.keyIdentifier, out this.keyIdentifier);
			TryReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item), this.authorityCertIssuer, out this.authorityCertIssuer);
			TryReadIntegerExplicit(Reader, 2, this.authorityCertSerialNumber, out this.authorityCertSerialNumber);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AuthorityKeyIdentifier _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AuthorityKeyIdentifier();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.keyIdentifier is null))
					WriteOctetStringImplicit(Writer, 0, this.keyIdentifier);
				if (!(this.authorityCertIssuer is null))
					WriteArrayImplicit(Writer, 1, this.authorityCertIssuer);
				if (this.authorityCertSerialNumber.HasValue)
					WriteIntegerImplicit(Writer, 2, this.authorityCertSerialNumber.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AuthorityKeyIdentifier _Typed)) return false;
			if (!AreEqual(this.keyIdentifier, _Typed.keyIdentifier)) return false;
			if (!AreEqual(this.authorityCertIssuer, _Typed.authorityCertIssuer)) return false;
			if (!AreEqual(this.authorityCertSerialNumber, _Typed.authorityCertSerialNumber)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.authorityCertIssuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.authorityCertSerialNumber);
			return _Result;
		}
	}

	[System.Flags]
	public enum KeyUsage
	{
		digitalSignature = 1,
		nonRepudiation = 2,
		keyEncipherment = 4,
		dataEncipherment = 8,
		keyAgreement = 16,
		keyCertSign = 32,
		cRLSign = 64,
		encipherOnly = 128,
		decipherOnly = 256
	}

	public partial class PrivateKeyUsagePeriod : Asn1.Construct
	{
		public System.TimeSpan? notBefore;
		public System.TimeSpan? notAfter;

		public static PrivateKeyUsagePeriod Create(Asn1F.AsnReader Reader)
		{
			PrivateKeyUsagePeriod _Result = new PrivateKeyUsagePeriod();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadGeneralizedTimeImplicit(Reader, 0, this.notBefore, out this.notBefore);
			TryReadGeneralizedTimeImplicit(Reader, 1, this.notAfter, out this.notAfter);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PrivateKeyUsagePeriod _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PrivateKeyUsagePeriod();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (this.notBefore.HasValue)
					WriteGeneralizedTimeImplicit(Writer, 0, this.notBefore.Value);
				if (this.notAfter.HasValue)
					WriteGeneralizedTimeImplicit(Writer, 1, this.notAfter.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PrivateKeyUsagePeriod _Typed)) return false;
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

	public partial class PolicyInformation : Asn1.Construct
	{
		public Asn1.ObjectId policyIdentifier;
		public PolicyQualifierInfo[] policyQualifiers;

		public static PolicyInformation Create(Asn1F.AsnReader Reader)
		{
			PolicyInformation _Result = new PolicyInformation();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.policyIdentifier = ReadObjectIdImplicit(Reader, 0);
			TryReadArray<PolicyQualifierInfo>(Reader, (Asn1F.AsnReader Reader, out PolicyQualifierInfo _Item) => PolicyQualifierInfo.TryCreate(Reader, out _Item), this.policyQualifiers, out this.policyQualifiers);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PolicyInformation _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PolicyInformation();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.policyIdentifier);
				if (!(this.policyQualifiers is null))
					WriteArrayImplicit(Writer, 1, this.policyQualifiers);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PolicyInformation _Typed)) return false;
			if (!AreEqual(this.policyIdentifier, _Typed.policyIdentifier)) return false;
			if (!AreEqual(this.policyQualifiers, _Typed.policyQualifiers)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.policyIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.policyQualifiers);
			return _Result;
		}
	}

	public partial class PolicyQualifierInfo : Asn1.Construct
	{
		public Asn1.ObjectId policyQualifierId;
		public object qualifier;

		public static PolicyQualifierInfo Create(Asn1F.AsnReader Reader)
		{
			PolicyQualifierInfo _Result = new PolicyQualifierInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.policyQualifierId = ReadObjectIdImplicit(Reader, 0);
			if (!(Asn1.Construct.CheckInSet(this.policyQualifierId, _DefinedValues.policyQualifierIdInSet)))
				throw new Asn1F.AsnContentException("policyQualifierId out of bounds.");
			this.qualifier = ReadAnyImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PolicyQualifierInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PolicyQualifierInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckInSet(this.policyQualifierId, _DefinedValues.policyQualifierIdInSet)))
					throw new Asn1F.AsnContentException("policyQualifierId out of bounds.");
				WriteObjectIdImplicit(Writer, 0, this.policyQualifierId);
				WriteAnyImplicit(Writer, 1, this.qualifier);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PolicyQualifierInfo _Typed)) return false;
			if (!AreEqual(this.policyQualifierId, _Typed.policyQualifierId)) return false;
			if (!AreEqual(this.qualifier, _Typed.qualifier)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.policyQualifierId);
			_Result ^= _Result << 5 ^ CalcHashCode(this.qualifier);
			return _Result;
		}
	}

	public partial class UserNotice : Asn1.Construct
	{
		public NoticeReference noticeRef;
		public DisplayText explicitText;

		public static UserNotice Create(Asn1F.AsnReader Reader)
		{
			UserNotice _Result = new UserNotice();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			NoticeReference.TryCreate(Reader, out this.noticeRef);
			DisplayText.TryCreate(Reader, out this.explicitText);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out UserNotice _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new UserNotice();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.noticeRef is null))
					this.noticeRef.Write(Writer);
				if (!(this.explicitText is null))
					this.explicitText.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is UserNotice _Typed)) return false;
			if (!AreEqual(this.noticeRef, _Typed.noticeRef)) return false;
			if (!AreEqual(this.explicitText, _Typed.explicitText)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.noticeRef);
			_Result ^= _Result << 5 ^ CalcHashCode(this.explicitText);
			return _Result;
		}
	}

	public partial class NoticeReference : Asn1.Construct
	{
		public DisplayText organization;
		public System.Numerics.BigInteger[] noticeNumbers;

		public static NoticeReference Create(Asn1F.AsnReader Reader)
		{
			NoticeReference _Result = new NoticeReference();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.organization = DisplayText.Create(Reader);
			this.noticeNumbers = ReadArray<System.Numerics.BigInteger>(Reader, (Asn1F.AsnReader Reader, out System.Numerics.BigInteger? _Item) => TryReadInteger(Reader, default, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out NoticeReference _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new NoticeReference();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.organization.Write(Writer);
				WriteArrayImplicit(Writer, 1, this.noticeNumbers);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is NoticeReference _Typed)) return false;
			if (!AreEqual(this.organization, _Typed.organization)) return false;
			if (!AreEqual(this.noticeNumbers, _Typed.noticeNumbers)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.organization);
			_Result ^= _Result << 5 ^ CalcHashCode(this.noticeNumbers);
			return _Result;
		}
	}

	public partial class DisplayText : Asn1.Construct
	{
		public enum _Options
		{
			ia5String,
			visibleString,
			bmpString,
			utf8String
		}

		public _Options _choice;
		public string ia5String;
		public string visibleString;
		public string bmpString;
		public string utf8String;

		public static DisplayText Create(Asn1F.AsnReader Reader)
		{
			DisplayText _Result = new DisplayText();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.ia5String = ReadStringImplicit(Reader, 0, Asn1F.UniversalTagNumber.IA5String);
					if (!(Asn1.Construct.CheckSize(this.ia5String, _DefinedValues.ia5StringSize)))
						throw new Asn1F.AsnContentException("ia5String out of bounds.");
					this._choice = _Options.ia5String;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.visibleString = ReadStringImplicit(Reader, 1, Asn1F.UniversalTagNumber.ISO646String);
					if (!(Asn1.Construct.CheckSize(this.visibleString, _DefinedValues.visibleStringSize)))
						throw new Asn1F.AsnContentException("visibleString out of bounds.");
					this._choice = _Options.visibleString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.bmpString = ReadStringImplicit(Reader, 2, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringImplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out DisplayText _Result)
		{
			try
			{
				_Result = new DisplayText();
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
				case _Options.ia5String:
					if (!(Asn1.Construct.CheckSize(this.ia5String, _DefinedValues.ia5StringSize)))
						throw new Asn1F.AsnContentException("ia5String out of bounds.");
					if (!(this.ia5String is null))
						WriteStringImplicit(Writer, 0, this.ia5String, Asn1F.UniversalTagNumber.IA5String);
					break;

				case _Options.visibleString:
					if (!(Asn1.Construct.CheckSize(this.visibleString, _DefinedValues.visibleStringSize)))
						throw new Asn1F.AsnContentException("visibleString out of bounds.");
					if (!(this.visibleString is null))
						WriteStringImplicit(Writer, 1, this.visibleString, Asn1F.UniversalTagNumber.ISO646String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringImplicit(Writer, 2, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringImplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is DisplayText _Typed)) return false;
			if (!AreEqual(this.ia5String, _Typed.ia5String)) return false;
			if (!AreEqual(this.visibleString, _Typed.visibleString)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ia5String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.visibleString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			return _Result;
		}
	}

	public partial class GeneralName : Asn1.Construct
	{
		public enum _Options
		{
			otherName,
			rfc822Name,
			dNSName,
			x400Address,
			directoryName,
			ediPartyName,
			uniformResourceIdentifier,
			iPAddress,
			registeredID
		}

		public _Options _choice;
		public AnotherName otherName;
		public string rfc822Name;
		public string dNSName;
		public ORAddress x400Address;
		public Name directoryName;
		public EDIPartyName ediPartyName;
		public string uniformResourceIdentifier;
		public byte[] iPAddress;
		public Asn1.ObjectId registeredID;

		public static GeneralName Create(Asn1F.AsnReader Reader)
		{
			GeneralName _Result = new GeneralName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.otherName = AnotherName.Create(Reader);
					this._choice = _Options.otherName;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.rfc822Name = ReadStringImplicit(Reader, 1, Asn1F.UniversalTagNumber.IA5String);
					this._choice = _Options.rfc822Name;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.dNSName = ReadStringImplicit(Reader, 2, Asn1F.UniversalTagNumber.IA5String);
					this._choice = _Options.dNSName;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.x400Address = ORAddress.Create(Reader);
					this._choice = _Options.x400Address;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.directoryName = Name.Create(Reader);
					this._choice = _Options.directoryName;
					break;

				case 133: // CONTEXT SPECIFIC [5]
					this.ediPartyName = EDIPartyName.Create(Reader);
					this._choice = _Options.ediPartyName;
					break;

				case 134: // CONTEXT SPECIFIC [6]
					this.uniformResourceIdentifier = ReadStringImplicit(Reader, 6, Asn1F.UniversalTagNumber.IA5String);
					this._choice = _Options.uniformResourceIdentifier;
					break;

				case 135: // CONTEXT SPECIFIC [7]
					this.iPAddress = ReadOctetStringImplicit(Reader, 7);
					this._choice = _Options.iPAddress;
					break;

				case 136: // CONTEXT SPECIFIC [8]
					this.registeredID = ReadObjectIdImplicit(Reader, 8);
					this._choice = _Options.registeredID;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out GeneralName _Result)
		{
			try
			{
				_Result = new GeneralName();
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
				case _Options.otherName:
					if (!(this.otherName is null))
						this.otherName.Write(Writer);
					break;

				case _Options.rfc822Name:
					if (!(this.rfc822Name is null))
						WriteStringImplicit(Writer, 1, this.rfc822Name, Asn1F.UniversalTagNumber.IA5String);
					break;

				case _Options.dNSName:
					if (!(this.dNSName is null))
						WriteStringImplicit(Writer, 2, this.dNSName, Asn1F.UniversalTagNumber.IA5String);
					break;

				case _Options.x400Address:
					if (!(this.x400Address is null))
						this.x400Address.Write(Writer);
					break;

				case _Options.directoryName:
					if (!(this.directoryName is null))
						this.directoryName.Write(Writer);
					break;

				case _Options.ediPartyName:
					if (!(this.ediPartyName is null))
						this.ediPartyName.Write(Writer);
					break;

				case _Options.uniformResourceIdentifier:
					if (!(this.uniformResourceIdentifier is null))
						WriteStringImplicit(Writer, 6, this.uniformResourceIdentifier, Asn1F.UniversalTagNumber.IA5String);
					break;

				case _Options.iPAddress:
					if (!(this.iPAddress is null))
						WriteOctetStringImplicit(Writer, 7, this.iPAddress);
					break;

				case _Options.registeredID:
					if (!(this.registeredID is null))
						WriteObjectIdImplicit(Writer, 8, this.registeredID);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is GeneralName _Typed)) return false;
			if (!AreEqual(this.otherName, _Typed.otherName)) return false;
			if (!AreEqual(this.rfc822Name, _Typed.rfc822Name)) return false;
			if (!AreEqual(this.dNSName, _Typed.dNSName)) return false;
			if (!AreEqual(this.x400Address, _Typed.x400Address)) return false;
			if (!AreEqual(this.directoryName, _Typed.directoryName)) return false;
			if (!AreEqual(this.ediPartyName, _Typed.ediPartyName)) return false;
			if (!AreEqual(this.uniformResourceIdentifier, _Typed.uniformResourceIdentifier)) return false;
			if (!AreEqual(this.iPAddress, _Typed.iPAddress)) return false;
			if (!AreEqual(this.registeredID, _Typed.registeredID)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.otherName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.rfc822Name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.dNSName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.x400Address);
			_Result ^= _Result << 5 ^ CalcHashCode(this.directoryName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ediPartyName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.uniformResourceIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.iPAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.registeredID);
			return _Result;
		}
	}

	public partial class AnotherName : Asn1.Construct
	{
		public Asn1.ObjectId type_id;
		public object @value;

		public static AnotherName Create(Asn1F.AsnReader Reader)
		{
			AnotherName _Result = new AnotherName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type_id = ReadObjectIdImplicit(Reader, 0);
			this.@value = ReadAnyExplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AnotherName _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AnotherName();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.type_id);
				WriteAnyImplicit(Writer, 1, this.@value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AnotherName _Typed)) return false;
			if (!AreEqual(this.type_id, _Typed.type_id)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.type_id);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

	public partial class EDIPartyName : Asn1.Construct
	{
		public DirectoryString nameAssigner;
		public DirectoryString partyName;

		public static EDIPartyName Create(Asn1F.AsnReader Reader)
		{
			EDIPartyName _Result = new EDIPartyName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			DirectoryString.TryCreate(Reader, out this.nameAssigner);
			this.partyName = DirectoryString.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out EDIPartyName _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new EDIPartyName();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.nameAssigner is null))
					this.nameAssigner.Write(Writer);
				this.partyName.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EDIPartyName _Typed)) return false;
			if (!AreEqual(this.nameAssigner, _Typed.nameAssigner)) return false;
			if (!AreEqual(this.partyName, _Typed.partyName)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.nameAssigner);
			_Result ^= _Result << 5 ^ CalcHashCode(this.partyName);
			return _Result;
		}
	}

	public partial class BasicConstraints : Asn1.Construct
	{
		public bool? cA = false;
		public System.Numerics.BigInteger? pathLenConstraint;

		public static BasicConstraints Create(Asn1F.AsnReader Reader)
		{
			BasicConstraints _Result = new BasicConstraints();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadBooleanImplicit(Reader, 0, this.cA, out this.cA);
			TryReadIntegerImplicit(Reader, 1, this.pathLenConstraint, out this.pathLenConstraint);
			if (!(this.pathLenConstraint is null))
			{
				if (!(Asn1.Construct.CheckInSet(this.pathLenConstraint, _DefinedValues.pathLenConstraintInSet)))
					throw new Asn1F.AsnContentException("pathLenConstraint out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out BasicConstraints _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new BasicConstraints();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (this.cA.HasValue)
					WriteBooleanImplicit(Writer, 0, this.cA.Value);
				if (!(this.pathLenConstraint is null))
				{
					if (!(Asn1.Construct.CheckInSet(this.pathLenConstraint, _DefinedValues.pathLenConstraintInSet)))
						throw new Asn1F.AsnContentException("pathLenConstraint out of bounds.");
				}
				if (this.pathLenConstraint.HasValue)
					WriteIntegerImplicit(Writer, 1, this.pathLenConstraint.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is BasicConstraints _Typed)) return false;
			if (!AreEqual(this.cA, _Typed.cA)) return false;
			if (!AreEqual(this.pathLenConstraint, _Typed.pathLenConstraint)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.cA);
			_Result ^= _Result << 5 ^ CalcHashCode(this.pathLenConstraint);
			return _Result;
		}
	}

	public partial class NameConstraints : Asn1.Construct
	{
		public GeneralSubtree[] permittedSubtrees;
		public GeneralSubtree[] excludedSubtrees;

		public static NameConstraints Create(Asn1F.AsnReader Reader)
		{
			NameConstraints _Result = new NameConstraints();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadArray<GeneralSubtree>(Reader, (Asn1F.AsnReader Reader, out GeneralSubtree _Item) => GeneralSubtree.TryCreate(Reader, out _Item), this.permittedSubtrees, out this.permittedSubtrees);
			TryReadArray<GeneralSubtree>(Reader, (Asn1F.AsnReader Reader, out GeneralSubtree _Item) => GeneralSubtree.TryCreate(Reader, out _Item), this.excludedSubtrees, out this.excludedSubtrees);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out NameConstraints _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new NameConstraints();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.permittedSubtrees is null))
					WriteArrayImplicit(Writer, 0, this.permittedSubtrees);
				if (!(this.excludedSubtrees is null))
					WriteArrayImplicit(Writer, 1, this.excludedSubtrees);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is NameConstraints _Typed)) return false;
			if (!AreEqual(this.permittedSubtrees, _Typed.permittedSubtrees)) return false;
			if (!AreEqual(this.excludedSubtrees, _Typed.excludedSubtrees)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.permittedSubtrees);
			_Result ^= _Result << 5 ^ CalcHashCode(this.excludedSubtrees);
			return _Result;
		}
	}

	public partial class GeneralSubtree : Asn1.Construct
	{
		public GeneralName @base;
		public System.Numerics.BigInteger? minimum = 0;
		public System.Numerics.BigInteger? maximum;

		public static GeneralSubtree Create(Asn1F.AsnReader Reader)
		{
			GeneralSubtree _Result = new GeneralSubtree();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.@base = GeneralName.Create(Reader);
			TryReadIntegerImplicit(Reader, 1, this.minimum, out this.minimum);
			if (!(this.minimum is null))
			{
				if (!(Asn1.Construct.CheckInSet(this.minimum, _DefinedValues.minimumInSet)))
					throw new Asn1F.AsnContentException("minimum out of bounds.");
			}
			TryReadIntegerImplicit(Reader, 2, this.maximum, out this.maximum);
			if (!(this.maximum is null))
			{
				if (!(Asn1.Construct.CheckInSet(this.maximum, _DefinedValues.maximumInSet)))
					throw new Asn1F.AsnContentException("maximum out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out GeneralSubtree _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new GeneralSubtree();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.@base.Write(Writer);
				if (!(this.minimum is null))
				{
					if (!(Asn1.Construct.CheckInSet(this.minimum, _DefinedValues.minimumInSet)))
						throw new Asn1F.AsnContentException("minimum out of bounds.");
				}
				if (this.minimum.HasValue)
					WriteIntegerImplicit(Writer, 1, this.minimum.Value);
				if (!(this.maximum is null))
				{
					if (!(Asn1.Construct.CheckInSet(this.maximum, _DefinedValues.maximumInSet)))
						throw new Asn1F.AsnContentException("maximum out of bounds.");
				}
				if (this.maximum.HasValue)
					WriteIntegerImplicit(Writer, 2, this.maximum.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is GeneralSubtree _Typed)) return false;
			if (!AreEqual(this.@base, _Typed.@base)) return false;
			if (!AreEqual(this.minimum, _Typed.minimum)) return false;
			if (!AreEqual(this.maximum, _Typed.maximum)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.@base);
			_Result ^= _Result << 5 ^ CalcHashCode(this.minimum);
			_Result ^= _Result << 5 ^ CalcHashCode(this.maximum);
			return _Result;
		}
	}

	public partial class PolicyConstraints : Asn1.Construct
	{
		public System.Numerics.BigInteger? requireExplicitPolicy;
		public System.Numerics.BigInteger? inhibitPolicyMapping;

		public static PolicyConstraints Create(Asn1F.AsnReader Reader)
		{
			PolicyConstraints _Result = new PolicyConstraints();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadIntegerImplicit(Reader, 0, this.requireExplicitPolicy, out this.requireExplicitPolicy);
			if (!(this.requireExplicitPolicy is null))
			{
				if (!(Asn1.Construct.CheckInSet(this.requireExplicitPolicy, _DefinedValues.requireExplicitPolicyInSet)))
					throw new Asn1F.AsnContentException("requireExplicitPolicy out of bounds.");
			}
			TryReadIntegerImplicit(Reader, 1, this.inhibitPolicyMapping, out this.inhibitPolicyMapping);
			if (!(this.inhibitPolicyMapping is null))
			{
				if (!(Asn1.Construct.CheckInSet(this.inhibitPolicyMapping, _DefinedValues.inhibitPolicyMappingInSet)))
					throw new Asn1F.AsnContentException("inhibitPolicyMapping out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PolicyConstraints _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PolicyConstraints();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.requireExplicitPolicy is null))
				{
					if (!(Asn1.Construct.CheckInSet(this.requireExplicitPolicy, _DefinedValues.requireExplicitPolicyInSet)))
						throw new Asn1F.AsnContentException("requireExplicitPolicy out of bounds.");
				}
				if (this.requireExplicitPolicy.HasValue)
					WriteIntegerImplicit(Writer, 0, this.requireExplicitPolicy.Value);
				if (!(this.inhibitPolicyMapping is null))
				{
					if (!(Asn1.Construct.CheckInSet(this.inhibitPolicyMapping, _DefinedValues.inhibitPolicyMappingInSet)))
						throw new Asn1F.AsnContentException("inhibitPolicyMapping out of bounds.");
				}
				if (this.inhibitPolicyMapping.HasValue)
					WriteIntegerImplicit(Writer, 1, this.inhibitPolicyMapping.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PolicyConstraints _Typed)) return false;
			if (!AreEqual(this.requireExplicitPolicy, _Typed.requireExplicitPolicy)) return false;
			if (!AreEqual(this.inhibitPolicyMapping, _Typed.inhibitPolicyMapping)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.requireExplicitPolicy);
			_Result ^= _Result << 5 ^ CalcHashCode(this.inhibitPolicyMapping);
			return _Result;
		}
	}

	public partial class DistributionPoint : Asn1.Construct
	{
		public DistributionPointName distributionPoint;
		public ReasonFlags? reasons;
		public GeneralName[] cRLIssuer;

		public static DistributionPoint Create(Asn1F.AsnReader Reader)
		{
			DistributionPoint _Result = new DistributionPoint();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			DistributionPointName.TryCreate(Reader, out this.distributionPoint);
			TryReadEnumerationImplicit<ReasonFlags>(Reader, 1, this.reasons, out this.reasons);
			TryReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item), this.cRLIssuer, out this.cRLIssuer);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out DistributionPoint _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new DistributionPoint();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.distributionPoint is null))
					this.distributionPoint.Write(Writer);
				if (this.reasons.HasValue)
					WriteBitStringImplicit(Writer, 1, this.reasons.Value);
				if (!(this.cRLIssuer is null))
					WriteArrayImplicit(Writer, 2, this.cRLIssuer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is DistributionPoint _Typed)) return false;
			if (!AreEqual(this.distributionPoint, _Typed.distributionPoint)) return false;
			if (!AreEqual(this.reasons, _Typed.reasons)) return false;
			if (!AreEqual(this.cRLIssuer, _Typed.cRLIssuer)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.distributionPoint);
			_Result ^= _Result << 5 ^ CalcHashCode(this.reasons);
			_Result ^= _Result << 5 ^ CalcHashCode(this.cRLIssuer);
			return _Result;
		}
	}

	public partial class DistributionPointName : Asn1.Construct
	{
		public enum _Options
		{
			fullName,
			nameRelativeToCRLIssuer
		}

		public _Options _choice;
		public GeneralName[] fullName;
		public AttributeTypeAndValue[] nameRelativeToCRLIssuer;

		public static DistributionPointName Create(Asn1F.AsnReader Reader)
		{
			DistributionPointName _Result = new DistributionPointName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.fullName = ReadArray<GeneralName>(Reader, (Asn1F.AsnReader Reader, out GeneralName _Item) => GeneralName.TryCreate(Reader, out _Item));
					this._choice = _Options.fullName;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.nameRelativeToCRLIssuer = ReadArray<AttributeTypeAndValue>(Reader, (Asn1F.AsnReader Reader, out AttributeTypeAndValue _Item) => AttributeTypeAndValue.TryCreate(Reader, out _Item));
					this._choice = _Options.nameRelativeToCRLIssuer;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out DistributionPointName _Result)
		{
			try
			{
				_Result = new DistributionPointName();
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
				case _Options.fullName:
					if (!(this.fullName is null))
						WriteArrayImplicit(Writer, 0, this.fullName);
					break;

				case _Options.nameRelativeToCRLIssuer:
					if (!(this.nameRelativeToCRLIssuer is null))
						WriteArrayImplicit(Writer, 1, this.nameRelativeToCRLIssuer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is DistributionPointName _Typed)) return false;
			if (!AreEqual(this.fullName, _Typed.fullName)) return false;
			if (!AreEqual(this.nameRelativeToCRLIssuer, _Typed.nameRelativeToCRLIssuer)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.fullName);
			_Result ^= _Result << 5 ^ CalcHashCode(this.nameRelativeToCRLIssuer);
			return _Result;
		}
	}

	[System.Flags]
	public enum ReasonFlags
	{
		unused = 1,
		keyCompromise = 2,
		cACompromise = 4,
		affiliationChanged = 8,
		superseded = 16,
		cessationOfOperation = 32,
		certificateHold = 64,
		privilegeWithdrawn = 128,
		aACompromise = 256
	}

	public partial class AccessDescription : Asn1.Construct
	{
		public Asn1.ObjectId accessMethod;
		public GeneralName accessLocation;

		public static AccessDescription Create(Asn1F.AsnReader Reader)
		{
			AccessDescription _Result = new AccessDescription();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.accessMethod = ReadObjectIdImplicit(Reader, 0);
			this.accessLocation = GeneralName.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AccessDescription _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AccessDescription();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.accessMethod);
				this.accessLocation.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AccessDescription _Typed)) return false;
			if (!AreEqual(this.accessMethod, _Typed.accessMethod)) return false;
			if (!AreEqual(this.accessLocation, _Typed.accessLocation)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.accessMethod);
			_Result ^= _Result << 5 ^ CalcHashCode(this.accessLocation);
			return _Result;
		}
	}

	public partial class IssuingDistributionPoint : Asn1.Construct
	{
		public DistributionPointName distributionPoint;
		public bool? onlyContainsUserCerts = false;
		public bool? onlyContainsCACerts = false;
		public ReasonFlags? onlySomeReasons;
		public bool? indirectCRL = false;
		public bool? onlyContainsAttributeCerts = false;

		public static IssuingDistributionPoint Create(Asn1F.AsnReader Reader)
		{
			IssuingDistributionPoint _Result = new IssuingDistributionPoint();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			DistributionPointName.TryCreate(Reader, out this.distributionPoint);
			TryReadBooleanImplicit(Reader, 1, this.onlyContainsUserCerts, out this.onlyContainsUserCerts);
			TryReadBooleanImplicit(Reader, 2, this.onlyContainsCACerts, out this.onlyContainsCACerts);
			TryReadEnumerationImplicit<ReasonFlags>(Reader, 3, this.onlySomeReasons, out this.onlySomeReasons);
			TryReadBooleanImplicit(Reader, 4, this.indirectCRL, out this.indirectCRL);
			TryReadBooleanImplicit(Reader, 5, this.onlyContainsAttributeCerts, out this.onlyContainsAttributeCerts);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IssuingDistributionPoint _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IssuingDistributionPoint();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.distributionPoint is null))
					this.distributionPoint.Write(Writer);
				if (this.onlyContainsUserCerts.HasValue)
					WriteBooleanImplicit(Writer, 1, this.onlyContainsUserCerts.Value);
				if (this.onlyContainsCACerts.HasValue)
					WriteBooleanImplicit(Writer, 2, this.onlyContainsCACerts.Value);
				if (this.onlySomeReasons.HasValue)
					WriteBitStringImplicit(Writer, 3, this.onlySomeReasons.Value);
				if (this.indirectCRL.HasValue)
					WriteBooleanImplicit(Writer, 4, this.indirectCRL.Value);
				if (this.onlyContainsAttributeCerts.HasValue)
					WriteBooleanImplicit(Writer, 5, this.onlyContainsAttributeCerts.Value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IssuingDistributionPoint _Typed)) return false;
			if (!AreEqual(this.distributionPoint, _Typed.distributionPoint)) return false;
			if (!AreEqual(this.onlyContainsUserCerts, _Typed.onlyContainsUserCerts)) return false;
			if (!AreEqual(this.onlyContainsCACerts, _Typed.onlyContainsCACerts)) return false;
			if (!AreEqual(this.onlySomeReasons, _Typed.onlySomeReasons)) return false;
			if (!AreEqual(this.indirectCRL, _Typed.indirectCRL)) return false;
			if (!AreEqual(this.onlyContainsAttributeCerts, _Typed.onlyContainsAttributeCerts)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.distributionPoint);
			_Result ^= _Result << 5 ^ CalcHashCode(this.onlyContainsUserCerts);
			_Result ^= _Result << 5 ^ CalcHashCode(this.onlyContainsCACerts);
			_Result ^= _Result << 5 ^ CalcHashCode(this.onlySomeReasons);
			_Result ^= _Result << 5 ^ CalcHashCode(this.indirectCRL);
			_Result ^= _Result << 5 ^ CalcHashCode(this.onlyContainsAttributeCerts);
			return _Result;
		}
	}

	public enum CRLReason
	{
		unspecified = 0,
		keyCompromise = 1,
		cACompromise = 2,
		affiliationChanged = 3,
		superseded = 4,
		cessationOfOperation = 5,
		certificateHold = 6,
		removeFromCRL = 8,
		privilegeWithdrawn = 9,
		aACompromise = 10
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
