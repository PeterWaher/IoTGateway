using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Security.PKCS.Certificates.PKIX1Explicit88;
using Waher.Security.PKCS.Certificates.PKIXAttributeCertificate;
using Waher.Security.PKCS.Certificates.AttributeCertificateVersion1;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Security\Waher.Security.PKCS\Certificates\Asn1\CryptographicMessageSyntax.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Security.PKCS.Certificates.CryptographicMessageSyntax
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId id_contentType = new Asn1.ObjectId(1 /* iso */, 2 /* member_body */, 840 /* us */, 113549 /* rsadsi */, 1 /* pkcs */, 9 /* pkcs9 */, 3);
		public static readonly Asn1.ObjectId id_messageDigest = new Asn1.ObjectId(1 /* iso */, 2 /* member_body */, 840 /* us */, 113549 /* rsadsi */, 1 /* pkcs */, 9 /* pkcs9 */, 4);
		public static readonly Asn1.ObjectId id_signingTime = new Asn1.ObjectId(1 /* iso */, 2 /* member_body */, 840 /* us */, 113549 /* rsadsi */, 1 /* pkcs */, 9 /* pkcs9 */, 5);
		public static readonly Asn1.ObjectId id_countersignature = new Asn1.ObjectId(1 /* iso */, 2 /* member_body */, 840 /* us */, 113549 /* rsadsi */, 1 /* pkcs */, 9 /* pkcs9 */, 6);
	}

	public partial class ContentInfo : Asn1.Construct
	{
		public Asn1.ObjectId contentType;
		public object content;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ContentInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ContentInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ContentInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.contentType = ReadObjectIdImplicit(Reader, 0);
			this.content = ReadAnyExplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ContentInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ContentInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.contentType);
				WriteAnyImplicit(Writer, 1, this.content);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ContentInfo _Typed)) return false;
			if (!AreEqual(this.contentType, _Typed.contentType)) return false;
			if (!AreEqual(this.content, _Typed.content)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.contentType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.content);
			return _Result;
		}
	}

	public partial class SignedData : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public AlgorithmIdentifier[] digestAlgorithms;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo encapContentInfo;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices[] certificates;
		public CertificateList[] crls;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo[] signerInfos;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignedData Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignedData _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignedData();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.digestAlgorithms = ReadArray<AlgorithmIdentifier>(Reader, (Asn1F.AsnReader Reader, out AlgorithmIdentifier _Item) => AlgorithmIdentifier.TryCreate(Reader, out _Item));
			this.encapContentInfo = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo.Create(Reader);
			TryReadArray<CertificateChoices>(Reader, (Asn1F.AsnReader Reader, out CertificateChoices _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices.TryCreate(Reader, out _Item), this.certificates, out this.certificates);
			TryReadArray<CertificateList>(Reader, (Asn1F.AsnReader Reader, out CertificateList _Item) => CertificateList.TryCreate(Reader, out _Item), this.crls, out this.crls);
			this.signerInfos = ReadArray<SignerInfo>(Reader, (Asn1F.AsnReader Reader, out SignerInfo _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignedData _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignedData();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				WriteArrayImplicit(Writer, 1, this.digestAlgorithms);
				this.encapContentInfo.Write(Writer);
				if (!(this.certificates is null))
					WriteArrayImplicit(Writer, 3, this.certificates);
				if (!(this.crls is null))
					WriteArrayImplicit(Writer, 4, this.crls);
				WriteArrayImplicit(Writer, 5, this.signerInfos);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SignedData _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.digestAlgorithms, _Typed.digestAlgorithms)) return false;
			if (!AreEqual(this.encapContentInfo, _Typed.encapContentInfo)) return false;
			if (!AreEqual(this.certificates, _Typed.certificates)) return false;
			if (!AreEqual(this.crls, _Typed.crls)) return false;
			if (!AreEqual(this.signerInfos, _Typed.signerInfos)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.digestAlgorithms);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encapContentInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.certificates);
			_Result ^= _Result << 5 ^ CalcHashCode(this.crls);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signerInfos);
			return _Result;
		}
	}

	public partial class EncapsulatedContentInfo : Asn1.Construct
	{
		public Asn1.ObjectId eContentType;
		public byte[] eContent;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.eContentType = ReadObjectIdImplicit(Reader, 0);
			TryReadOctetStringExplicit(Reader, 1, this.eContent, out this.eContent);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.eContentType);
				if (!(this.eContent is null))
					WriteOctetStringImplicit(Writer, 1, this.eContent);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EncapsulatedContentInfo _Typed)) return false;
			if (!AreEqual(this.eContentType, _Typed.eContentType)) return false;
			if (!AreEqual(this.eContent, _Typed.eContent)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.eContentType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.eContent);
			return _Result;
		}
	}

	public partial class SignerInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier sid;
		public AlgorithmIdentifier digestAlgorithm;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] signedAttrs;
		public AlgorithmIdentifier signatureAlgorithm;
		public byte[] signature;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] unsignedAttrs;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.sid = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier.Create(Reader);
			this.digestAlgorithm = AlgorithmIdentifier.Create(Reader);
			TryReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item), this.signedAttrs, out this.signedAttrs);
			this.signatureAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.signature = ReadOctetStringImplicit(Reader, 5);
			TryReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item), this.unsignedAttrs, out this.unsignedAttrs);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.sid.Write(Writer);
				this.digestAlgorithm.Write(Writer);
				if (!(this.signedAttrs is null))
					WriteArrayImplicit(Writer, 3, this.signedAttrs);
				this.signatureAlgorithm.Write(Writer);
				WriteOctetStringImplicit(Writer, 5, this.signature);
				if (!(this.unsignedAttrs is null))
					WriteArrayImplicit(Writer, 6, this.unsignedAttrs);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SignerInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.sid, _Typed.sid)) return false;
			if (!AreEqual(this.digestAlgorithm, _Typed.digestAlgorithm)) return false;
			if (!AreEqual(this.signedAttrs, _Typed.signedAttrs)) return false;
			if (!AreEqual(this.signatureAlgorithm, _Typed.signatureAlgorithm)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			if (!AreEqual(this.unsignedAttrs, _Typed.unsignedAttrs)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.sid);
			_Result ^= _Result << 5 ^ CalcHashCode(this.digestAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signedAttrs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unsignedAttrs);
			return _Result;
		}
	}

	public partial class SignerIdentifier : Asn1.Construct
	{
		public enum _Options
		{
			issuerAndSerialNumber,
			subjectKeyIdentifier
		}

		public _Options _choice;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber issuerAndSerialNumber;
		public byte[] subjectKeyIdentifier;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.issuerAndSerialNumber = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber.Create(Reader);
					this._choice = _Options.issuerAndSerialNumber;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.subjectKeyIdentifier = ReadOctetStringImplicit(Reader, 1);
					this._choice = _Options.subjectKeyIdentifier;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.SignerIdentifier();
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
				case _Options.issuerAndSerialNumber:
					if (!(this.issuerAndSerialNumber is null))
						this.issuerAndSerialNumber.Write(Writer);
					break;

				case _Options.subjectKeyIdentifier:
					if (!(this.subjectKeyIdentifier is null))
						WriteOctetStringImplicit(Writer, 1, this.subjectKeyIdentifier);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SignerIdentifier _Typed)) return false;
			if (!AreEqual(this.issuerAndSerialNumber, _Typed.issuerAndSerialNumber)) return false;
			if (!AreEqual(this.subjectKeyIdentifier, _Typed.subjectKeyIdentifier)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerAndSerialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectKeyIdentifier);
			return _Result;
		}
	}

	public partial class Attribute : Asn1.Construct
	{
		public Asn1.ObjectId attrType;
		public object[] attrValues;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.attrType = ReadObjectIdImplicit(Reader, 0);
			this.attrValues = ReadArray<object>(Reader, (Asn1F.AsnReader Reader, out object _Item) => TryReadAnyImplicit(Reader, 1, default, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.attrType);
				WriteArrayImplicit(Writer, 1, this.attrValues);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Attribute _Typed)) return false;
			if (!AreEqual(this.attrType, _Typed.attrType)) return false;
			if (!AreEqual(this.attrValues, _Typed.attrValues)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.attrType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attrValues);
			return _Result;
		}
	}

	public partial class EnvelopedData : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo originatorInfo;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo[] recipientInfos;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo encryptedContentInfo;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] unprotectedAttrs;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo.TryCreate(Reader, out this.originatorInfo);
			this.recipientInfos = ReadArray<RecipientInfo>(Reader, (Asn1F.AsnReader Reader, out RecipientInfo _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo.TryCreate(Reader, out _Item));
			this.encryptedContentInfo = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo.Create(Reader);
			TryReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item), this.unprotectedAttrs, out this.unprotectedAttrs);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EnvelopedData();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				if (!(this.originatorInfo is null))
					this.originatorInfo.Write(Writer);
				WriteArrayImplicit(Writer, 2, this.recipientInfos);
				this.encryptedContentInfo.Write(Writer);
				if (!(this.unprotectedAttrs is null))
					WriteArrayImplicit(Writer, 4, this.unprotectedAttrs);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EnvelopedData _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.originatorInfo, _Typed.originatorInfo)) return false;
			if (!AreEqual(this.recipientInfos, _Typed.recipientInfos)) return false;
			if (!AreEqual(this.encryptedContentInfo, _Typed.encryptedContentInfo)) return false;
			if (!AreEqual(this.unprotectedAttrs, _Typed.unprotectedAttrs)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.originatorInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.recipientInfos);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedContentInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unprotectedAttrs);
			return _Result;
		}
	}

	public partial class OriginatorInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices[] certs;
		public CertificateList[] crls;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadArray<CertificateChoices>(Reader, (Asn1F.AsnReader Reader, out CertificateChoices _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices.TryCreate(Reader, out _Item), this.certs, out this.certs);
			TryReadArray<CertificateList>(Reader, (Asn1F.AsnReader Reader, out CertificateList _Item) => CertificateList.TryCreate(Reader, out _Item), this.crls, out this.crls);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.certs is null))
					WriteArrayImplicit(Writer, 0, this.certs);
				if (!(this.crls is null))
					WriteArrayImplicit(Writer, 1, this.crls);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is OriginatorInfo _Typed)) return false;
			if (!AreEqual(this.certs, _Typed.certs)) return false;
			if (!AreEqual(this.crls, _Typed.crls)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.certs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.crls);
			return _Result;
		}
	}

	public partial class EncryptedContentInfo : Asn1.Construct
	{
		public Asn1.ObjectId contentType;
		public AlgorithmIdentifier contentEncryptionAlgorithm;
		public byte[] encryptedContent;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.contentType = ReadObjectIdImplicit(Reader, 0);
			this.contentEncryptionAlgorithm = AlgorithmIdentifier.Create(Reader);
			TryReadOctetStringImplicit(Reader, 2, this.encryptedContent, out this.encryptedContent);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.contentType);
				this.contentEncryptionAlgorithm.Write(Writer);
				if (!(this.encryptedContent is null))
					WriteOctetStringImplicit(Writer, 2, this.encryptedContent);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EncryptedContentInfo _Typed)) return false;
			if (!AreEqual(this.contentType, _Typed.contentType)) return false;
			if (!AreEqual(this.contentEncryptionAlgorithm, _Typed.contentEncryptionAlgorithm)) return false;
			if (!AreEqual(this.encryptedContent, _Typed.encryptedContent)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.contentType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.contentEncryptionAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedContent);
			return _Result;
		}
	}

	public partial class RecipientInfo : Asn1.Construct
	{
		public enum _Options
		{
			ktri,
			kari,
			kekri,
			pwri,
			ori
		}

		public _Options _choice;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo ktri;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo kari;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo kekri;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo pwri;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo ori;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.ktri = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo.Create(Reader);
					this._choice = _Options.ktri;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.kari = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo.Create(Reader);
					this._choice = _Options.kari;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.kekri = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo.Create(Reader);
					this._choice = _Options.kekri;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.pwri = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo.Create(Reader);
					this._choice = _Options.pwri;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.ori = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo.Create(Reader);
					this._choice = _Options.ori;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo();
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
				case _Options.ktri:
					if (!(this.ktri is null))
						this.ktri.Write(Writer);
					break;

				case _Options.kari:
					if (!(this.kari is null))
						this.kari.Write(Writer);
					break;

				case _Options.kekri:
					if (!(this.kekri is null))
						this.kekri.Write(Writer);
					break;

				case _Options.pwri:
					if (!(this.pwri is null))
						this.pwri.Write(Writer);
					break;

				case _Options.ori:
					if (!(this.ori is null))
						this.ori.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is RecipientInfo _Typed)) return false;
			if (!AreEqual(this.ktri, _Typed.ktri)) return false;
			if (!AreEqual(this.kari, _Typed.kari)) return false;
			if (!AreEqual(this.kekri, _Typed.kekri)) return false;
			if (!AreEqual(this.pwri, _Typed.pwri)) return false;
			if (!AreEqual(this.ori, _Typed.ori)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ktri);
			_Result ^= _Result << 5 ^ CalcHashCode(this.kari);
			_Result ^= _Result << 5 ^ CalcHashCode(this.kekri);
			_Result ^= _Result << 5 ^ CalcHashCode(this.pwri);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ori);
			return _Result;
		}
	}

	public partial class KeyTransRecipientInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier rid;
		public AlgorithmIdentifier keyEncryptionAlgorithm;
		public byte[] encryptedKey;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.rid = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier.Create(Reader);
			this.keyEncryptionAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.encryptedKey = ReadOctetStringImplicit(Reader, 3);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyTransRecipientInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.rid.Write(Writer);
				this.keyEncryptionAlgorithm.Write(Writer);
				WriteOctetStringImplicit(Writer, 3, this.encryptedKey);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is KeyTransRecipientInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.rid, _Typed.rid)) return false;
			if (!AreEqual(this.keyEncryptionAlgorithm, _Typed.keyEncryptionAlgorithm)) return false;
			if (!AreEqual(this.encryptedKey, _Typed.encryptedKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.rid);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyEncryptionAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedKey);
			return _Result;
		}
	}

	public partial class RecipientIdentifier : Asn1.Construct
	{
		public enum _Options
		{
			issuerAndSerialNumber,
			subjectKeyIdentifier
		}

		public _Options _choice;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber issuerAndSerialNumber;
		public byte[] subjectKeyIdentifier;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.issuerAndSerialNumber = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber.Create(Reader);
					this._choice = _Options.issuerAndSerialNumber;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.subjectKeyIdentifier = ReadOctetStringImplicit(Reader, 1);
					this._choice = _Options.subjectKeyIdentifier;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientIdentifier();
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
				case _Options.issuerAndSerialNumber:
					if (!(this.issuerAndSerialNumber is null))
						this.issuerAndSerialNumber.Write(Writer);
					break;

				case _Options.subjectKeyIdentifier:
					if (!(this.subjectKeyIdentifier is null))
						WriteOctetStringImplicit(Writer, 1, this.subjectKeyIdentifier);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is RecipientIdentifier _Typed)) return false;
			if (!AreEqual(this.issuerAndSerialNumber, _Typed.issuerAndSerialNumber)) return false;
			if (!AreEqual(this.subjectKeyIdentifier, _Typed.subjectKeyIdentifier)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerAndSerialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectKeyIdentifier);
			return _Result;
		}
	}

	public partial class KeyAgreeRecipientInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey originator;
		public byte[] ukm;
		public AlgorithmIdentifier keyEncryptionAlgorithm;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey[] recipientEncryptedKeys;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.originator = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey.Create(Reader);
			TryReadOctetStringImplicit(Reader, 2, this.ukm, out this.ukm);
			this.keyEncryptionAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.recipientEncryptedKeys = ReadArray<RecipientEncryptedKey>(Reader, (Asn1F.AsnReader Reader, out RecipientEncryptedKey _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.originator.Write(Writer);
				if (!(this.ukm is null))
					WriteOctetStringImplicit(Writer, 2, this.ukm);
				this.keyEncryptionAlgorithm.Write(Writer);
				WriteArrayImplicit(Writer, 4, this.recipientEncryptedKeys);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is KeyAgreeRecipientInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.originator, _Typed.originator)) return false;
			if (!AreEqual(this.ukm, _Typed.ukm)) return false;
			if (!AreEqual(this.keyEncryptionAlgorithm, _Typed.keyEncryptionAlgorithm)) return false;
			if (!AreEqual(this.recipientEncryptedKeys, _Typed.recipientEncryptedKeys)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.originator);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ukm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyEncryptionAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.recipientEncryptedKeys);
			return _Result;
		}
	}

	public partial class OriginatorIdentifierOrKey : Asn1.Construct
	{
		public enum _Options
		{
			issuerAndSerialNumber,
			subjectKeyIdentifier,
			originatorKey
		}

		public _Options _choice;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber issuerAndSerialNumber;
		public byte[] subjectKeyIdentifier;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey originatorKey;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.issuerAndSerialNumber = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber.Create(Reader);
					this._choice = _Options.issuerAndSerialNumber;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.subjectKeyIdentifier = ReadOctetStringImplicit(Reader, 1);
					this._choice = _Options.subjectKeyIdentifier;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.originatorKey = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey.Create(Reader);
					this._choice = _Options.originatorKey;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorIdentifierOrKey();
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
				case _Options.issuerAndSerialNumber:
					if (!(this.issuerAndSerialNumber is null))
						this.issuerAndSerialNumber.Write(Writer);
					break;

				case _Options.subjectKeyIdentifier:
					if (!(this.subjectKeyIdentifier is null))
						WriteOctetStringImplicit(Writer, 1, this.subjectKeyIdentifier);
					break;

				case _Options.originatorKey:
					if (!(this.originatorKey is null))
						this.originatorKey.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is OriginatorIdentifierOrKey _Typed)) return false;
			if (!AreEqual(this.issuerAndSerialNumber, _Typed.issuerAndSerialNumber)) return false;
			if (!AreEqual(this.subjectKeyIdentifier, _Typed.subjectKeyIdentifier)) return false;
			if (!AreEqual(this.originatorKey, _Typed.originatorKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerAndSerialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectKeyIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.originatorKey);
			return _Result;
		}
	}

	public partial class OriginatorPublicKey : Asn1.Construct
	{
		public AlgorithmIdentifier algorithm;
		public byte[] publicKey;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.algorithm = AlgorithmIdentifier.Create(Reader);
			this.publicKey = ReadBitStringImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorPublicKey();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.algorithm.Write(Writer);
				WriteBitStringImplicit(Writer, 1, this.publicKey);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is OriginatorPublicKey _Typed)) return false;
			if (!AreEqual(this.algorithm, _Typed.algorithm)) return false;
			if (!AreEqual(this.publicKey, _Typed.publicKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.algorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.publicKey);
			return _Result;
		}
	}

	public partial class RecipientEncryptedKey : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier rid;
		public byte[] encryptedKey;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.rid = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier.Create(Reader);
			this.encryptedKey = ReadOctetStringImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientEncryptedKey();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.rid.Write(Writer);
				WriteOctetStringImplicit(Writer, 1, this.encryptedKey);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is RecipientEncryptedKey _Typed)) return false;
			if (!AreEqual(this.rid, _Typed.rid)) return false;
			if (!AreEqual(this.encryptedKey, _Typed.encryptedKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.rid);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedKey);
			return _Result;
		}
	}

	public partial class KeyAgreeRecipientIdentifier : Asn1.Construct
	{
		public enum _Options
		{
			issuerAndSerialNumber,
			rKeyId
		}

		public _Options _choice;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber issuerAndSerialNumber;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier rKeyId;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.issuerAndSerialNumber = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber.Create(Reader);
					this._choice = _Options.issuerAndSerialNumber;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.rKeyId = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier.Create(Reader);
					this._choice = _Options.rKeyId;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KeyAgreeRecipientIdentifier();
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
				case _Options.issuerAndSerialNumber:
					if (!(this.issuerAndSerialNumber is null))
						this.issuerAndSerialNumber.Write(Writer);
					break;

				case _Options.rKeyId:
					if (!(this.rKeyId is null))
						this.rKeyId.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is KeyAgreeRecipientIdentifier _Typed)) return false;
			if (!AreEqual(this.issuerAndSerialNumber, _Typed.issuerAndSerialNumber)) return false;
			if (!AreEqual(this.rKeyId, _Typed.rKeyId)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerAndSerialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.rKeyId);
			return _Result;
		}
	}

	public partial class RecipientKeyIdentifier : Asn1.Construct
	{
		public byte[] subjectKeyIdentifier;
		public System.TimeSpan? date;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute other;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.subjectKeyIdentifier = ReadOctetStringImplicit(Reader, 0);
			TryReadGeneralizedTimeImplicit(Reader, 1, this.date, out this.date);
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute.TryCreate(Reader, out this.other);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientKeyIdentifier();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteOctetStringImplicit(Writer, 0, this.subjectKeyIdentifier);
				if (this.date.HasValue)
					WriteGeneralizedTimeImplicit(Writer, 1, this.date.Value);
				if (!(this.other is null))
					this.other.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is RecipientKeyIdentifier _Typed)) return false;
			if (!AreEqual(this.subjectKeyIdentifier, _Typed.subjectKeyIdentifier)) return false;
			if (!AreEqual(this.date, _Typed.date)) return false;
			if (!AreEqual(this.other, _Typed.other)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectKeyIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.date);
			_Result ^= _Result << 5 ^ CalcHashCode(this.other);
			return _Result;
		}
	}

	public partial class KEKRecipientInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier kekid;
		public AlgorithmIdentifier keyEncryptionAlgorithm;
		public byte[] encryptedKey;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.kekid = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier.Create(Reader);
			this.keyEncryptionAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.encryptedKey = ReadOctetStringImplicit(Reader, 3);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKRecipientInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.kekid.Write(Writer);
				this.keyEncryptionAlgorithm.Write(Writer);
				WriteOctetStringImplicit(Writer, 3, this.encryptedKey);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is KEKRecipientInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.kekid, _Typed.kekid)) return false;
			if (!AreEqual(this.keyEncryptionAlgorithm, _Typed.keyEncryptionAlgorithm)) return false;
			if (!AreEqual(this.encryptedKey, _Typed.encryptedKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.kekid);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyEncryptionAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedKey);
			return _Result;
		}
	}

	public partial class KEKIdentifier : Asn1.Construct
	{
		public byte[] keyIdentifier;
		public System.TimeSpan? date;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute other;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.keyIdentifier = ReadOctetStringImplicit(Reader, 0);
			TryReadGeneralizedTimeImplicit(Reader, 1, this.date, out this.date);
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute.TryCreate(Reader, out this.other);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.KEKIdentifier();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteOctetStringImplicit(Writer, 0, this.keyIdentifier);
				if (this.date.HasValue)
					WriteGeneralizedTimeImplicit(Writer, 1, this.date.Value);
				if (!(this.other is null))
					this.other.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is KEKIdentifier _Typed)) return false;
			if (!AreEqual(this.keyIdentifier, _Typed.keyIdentifier)) return false;
			if (!AreEqual(this.date, _Typed.date)) return false;
			if (!AreEqual(this.other, _Typed.other)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyIdentifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.date);
			_Result ^= _Result << 5 ^ CalcHashCode(this.other);
			return _Result;
		}
	}

	public partial class PasswordRecipientInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public AlgorithmIdentifier keyDerivationAlgorithm;
		public AlgorithmIdentifier keyEncryptionAlgorithm;
		public byte[] encryptedKey;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			AlgorithmIdentifier.TryCreate(Reader, out this.keyDerivationAlgorithm);
			this.keyEncryptionAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.encryptedKey = ReadOctetStringImplicit(Reader, 3);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.PasswordRecipientInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				if (!(this.keyDerivationAlgorithm is null))
					this.keyDerivationAlgorithm.Write(Writer);
				this.keyEncryptionAlgorithm.Write(Writer);
				WriteOctetStringImplicit(Writer, 3, this.encryptedKey);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PasswordRecipientInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.keyDerivationAlgorithm, _Typed.keyDerivationAlgorithm)) return false;
			if (!AreEqual(this.keyEncryptionAlgorithm, _Typed.keyEncryptionAlgorithm)) return false;
			if (!AreEqual(this.encryptedKey, _Typed.encryptedKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyDerivationAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyEncryptionAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedKey);
			return _Result;
		}
	}

	public partial class OtherRecipientInfo : Asn1.Construct
	{
		public Asn1.ObjectId oriType;
		public object oriValue;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.oriType = ReadObjectIdImplicit(Reader, 0);
			this.oriValue = ReadAnyImplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherRecipientInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.oriType);
				WriteAnyImplicit(Writer, 1, this.oriValue);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is OtherRecipientInfo _Typed)) return false;
			if (!AreEqual(this.oriType, _Typed.oriType)) return false;
			if (!AreEqual(this.oriValue, _Typed.oriValue)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.oriType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.oriValue);
			return _Result;
		}
	}

	public partial class DigestedData : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public AlgorithmIdentifier digestAlgorithm;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo encapContentInfo;
		public byte[] digest;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.DigestedData Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.DigestedData _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.DigestedData();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.digestAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.encapContentInfo = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo.Create(Reader);
			this.digest = ReadOctetStringImplicit(Reader, 3);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.DigestedData _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.DigestedData();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.digestAlgorithm.Write(Writer);
				this.encapContentInfo.Write(Writer);
				WriteOctetStringImplicit(Writer, 3, this.digest);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is DigestedData _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.digestAlgorithm, _Typed.digestAlgorithm)) return false;
			if (!AreEqual(this.encapContentInfo, _Typed.encapContentInfo)) return false;
			if (!AreEqual(this.digest, _Typed.digest)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.digestAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encapContentInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.digest);
			return _Result;
		}
	}

	public partial class EncryptedData : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo encryptedContentInfo;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] unprotectedAttrs;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedData Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedData _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedData();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.encryptedContentInfo = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedContentInfo.Create(Reader);
			TryReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item), this.unprotectedAttrs, out this.unprotectedAttrs);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedData _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncryptedData();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.encryptedContentInfo.Write(Writer);
				if (!(this.unprotectedAttrs is null))
					WriteArrayImplicit(Writer, 2, this.unprotectedAttrs);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EncryptedData _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.encryptedContentInfo, _Typed.encryptedContentInfo)) return false;
			if (!AreEqual(this.unprotectedAttrs, _Typed.unprotectedAttrs)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encryptedContentInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unprotectedAttrs);
			return _Result;
		}
	}

	public partial class AuthenticatedData : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo originatorInfo;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo[] recipientInfos;
		public AlgorithmIdentifier macAlgorithm;
		public AlgorithmIdentifier digestAlgorithm;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo encapContentInfo;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] authAttrs;
		public byte[] mac;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] unauthAttrs;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.AuthenticatedData Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.AuthenticatedData _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.AuthenticatedData();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OriginatorInfo.TryCreate(Reader, out this.originatorInfo);
			this.recipientInfos = ReadArray<RecipientInfo>(Reader, (Asn1F.AsnReader Reader, out RecipientInfo _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.RecipientInfo.TryCreate(Reader, out _Item));
			this.macAlgorithm = AlgorithmIdentifier.Create(Reader);
			AlgorithmIdentifier.TryCreate(Reader, out this.digestAlgorithm);
			this.encapContentInfo = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.EncapsulatedContentInfo.Create(Reader);
			TryReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item), this.authAttrs, out this.authAttrs);
			this.mac = ReadOctetStringImplicit(Reader, 7);
			TryReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item), this.unauthAttrs, out this.unauthAttrs);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.AuthenticatedData _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.AuthenticatedData();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				if (!(this.originatorInfo is null))
					this.originatorInfo.Write(Writer);
				WriteArrayImplicit(Writer, 2, this.recipientInfos);
				this.macAlgorithm.Write(Writer);
				if (!(this.digestAlgorithm is null))
					this.digestAlgorithm.Write(Writer);
				this.encapContentInfo.Write(Writer);
				if (!(this.authAttrs is null))
					WriteArrayImplicit(Writer, 6, this.authAttrs);
				WriteOctetStringImplicit(Writer, 7, this.mac);
				if (!(this.unauthAttrs is null))
					WriteArrayImplicit(Writer, 8, this.unauthAttrs);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AuthenticatedData _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.originatorInfo, _Typed.originatorInfo)) return false;
			if (!AreEqual(this.recipientInfos, _Typed.recipientInfos)) return false;
			if (!AreEqual(this.macAlgorithm, _Typed.macAlgorithm)) return false;
			if (!AreEqual(this.digestAlgorithm, _Typed.digestAlgorithm)) return false;
			if (!AreEqual(this.encapContentInfo, _Typed.encapContentInfo)) return false;
			if (!AreEqual(this.authAttrs, _Typed.authAttrs)) return false;
			if (!AreEqual(this.mac, _Typed.mac)) return false;
			if (!AreEqual(this.unauthAttrs, _Typed.unauthAttrs)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.originatorInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.recipientInfos);
			_Result ^= _Result << 5 ^ CalcHashCode(this.macAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.digestAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.encapContentInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.authAttrs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.mac);
			_Result ^= _Result << 5 ^ CalcHashCode(this.unauthAttrs);
			return _Result;
		}
	}

	public partial class CertificateChoices : Asn1.Construct
	{
		public enum _Options
		{
			certificate,
			extendedCertificate,
			v1AttrCert,
			v2AttrCert
		}

		public _Options _choice;
		public Certificate certificate;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate extendedCertificate;
		public AttributeCertificateV1 v1AttrCert;
		public AttributeCertificate v2AttrCert;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.certificate = Certificate.Create(Reader);
					this._choice = _Options.certificate;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.extendedCertificate = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate.Create(Reader);
					this._choice = _Options.extendedCertificate;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.v1AttrCert = AttributeCertificateV1.Create(Reader);
					this._choice = _Options.v1AttrCert;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.v2AttrCert = AttributeCertificate.Create(Reader);
					this._choice = _Options.v2AttrCert;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CertificateChoices();
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
				case _Options.certificate:
					if (!(this.certificate is null))
						this.certificate.Write(Writer);
					break;

				case _Options.extendedCertificate:
					if (!(this.extendedCertificate is null))
						this.extendedCertificate.Write(Writer);
					break;

				case _Options.v1AttrCert:
					if (!(this.v1AttrCert is null))
						this.v1AttrCert.Write(Writer);
					break;

				case _Options.v2AttrCert:
					if (!(this.v2AttrCert is null))
						this.v2AttrCert.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CertificateChoices _Typed)) return false;
			if (!AreEqual(this.certificate, _Typed.certificate)) return false;
			if (!AreEqual(this.extendedCertificate, _Typed.extendedCertificate)) return false;
			if (!AreEqual(this.v1AttrCert, _Typed.v1AttrCert)) return false;
			if (!AreEqual(this.v2AttrCert, _Typed.v2AttrCert)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.certificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extendedCertificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.v1AttrCert);
			_Result ^= _Result << 5 ^ CalcHashCode(this.v2AttrCert);
			return _Result;
		}
	}

	public partial class IssuerAndSerialNumber : Asn1.Construct
	{
		public Name issuer;
		public System.Numerics.BigInteger serialNumber;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.issuer = Name.Create(Reader);
			this.serialNumber = ReadIntegerExplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.IssuerAndSerialNumber();
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
			if (!(Object is IssuerAndSerialNumber _Typed)) return false;
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

	public enum CMSVersion
	{
		v0 = 0,
		v1 = 1,
		v2 = 2,
		v3 = 3,
		v4 = 4
	}

	public partial class OtherKeyAttribute : Asn1.Construct
	{
		public Asn1.ObjectId keyAttrId;
		public object keyAttr;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.keyAttrId = ReadObjectIdImplicit(Reader, 0);
			TryReadAnyImplicit(Reader, 1, this.keyAttr, out this.keyAttr);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.OtherKeyAttribute();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdImplicit(Writer, 0, this.keyAttrId);
				if (!(this.keyAttr is null))
					WriteAnyImplicit(Writer, 1, this.keyAttr);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is OtherKeyAttribute _Typed)) return false;
			if (!AreEqual(this.keyAttrId, _Typed.keyAttrId)) return false;
			if (!AreEqual(this.keyAttr, _Typed.keyAttr)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyAttrId);
			_Result ^= _Result << 5 ^ CalcHashCode(this.keyAttr);
			return _Result;
		}
	}

	public partial class Time : Asn1.Construct
	{
		public enum _Options
		{
			utcTime,
			generalTime
		}

		public _Options _choice;
		public System.DateTimeOffset? utcTime;
		public System.TimeSpan? generalTime;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Time Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Time _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Time();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.utcTime = ReadUtcTimeImplicit(Reader, 0);
					this._choice = _Options.utcTime;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.generalTime = ReadGeneralizedTimeImplicit(Reader, 1);
					this._choice = _Options.generalTime;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Time _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Time();
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
				case _Options.utcTime:
					if (this.utcTime.HasValue)
						WriteUtcTimeImplicit(Writer, 0, this.utcTime.Value);
					break;

				case _Options.generalTime:
					if (this.generalTime.HasValue)
						WriteGeneralizedTimeImplicit(Writer, 1, this.generalTime.Value);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Time _Typed)) return false;
			if (!AreEqual(this.utcTime, _Typed.utcTime)) return false;
			if (!AreEqual(this.generalTime, _Typed.generalTime)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.utcTime);
			_Result ^= _Result << 5 ^ CalcHashCode(this.generalTime);
			return _Result;
		}
	}

	public partial class ExtendedCertificateOrCertificate : Asn1.Construct
	{
		public enum _Options
		{
			certificate,
			extendedCertificate
		}

		public _Options _choice;
		public Certificate certificate;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate extendedCertificate;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateOrCertificate Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateOrCertificate _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateOrCertificate();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.certificate = Certificate.Create(Reader);
					this._choice = _Options.certificate;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.extendedCertificate = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate.Create(Reader);
					this._choice = _Options.extendedCertificate;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateOrCertificate _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateOrCertificate();
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
				case _Options.certificate:
					if (!(this.certificate is null))
						this.certificate.Write(Writer);
					break;

				case _Options.extendedCertificate:
					if (!(this.extendedCertificate is null))
						this.extendedCertificate.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ExtendedCertificateOrCertificate _Typed)) return false;
			if (!AreEqual(this.certificate, _Typed.certificate)) return false;
			if (!AreEqual(this.extendedCertificate, _Typed.extendedCertificate)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.certificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extendedCertificate);
			return _Result;
		}
	}

	public partial class ExtendedCertificate : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo extendedCertificateInfo;
		public AlgorithmIdentifier signatureAlgorithm;
		public byte[] signature;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.extendedCertificateInfo = Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo.Create(Reader);
			this.signatureAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.signature = ReadBitStringImplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificate();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.extendedCertificateInfo.Write(Writer);
				this.signatureAlgorithm.Write(Writer);
				WriteBitStringImplicit(Writer, 2, this.signature);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ExtendedCertificate _Typed)) return false;
			if (!AreEqual(this.extendedCertificateInfo, _Typed.extendedCertificateInfo)) return false;
			if (!AreEqual(this.signatureAlgorithm, _Typed.signatureAlgorithm)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.extendedCertificateInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			return _Result;
		}
	}

	public partial class ExtendedCertificateInfo : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion version;
		public Certificate certificate;
		public Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute[] attributes;

		public static Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo _Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.version = ReadEnumerationImplicit<Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.CMSVersion>(Reader, 0);
			this.certificate = Certificate.Create(Reader);
			this.attributes = ReadArray<Attribute>(Reader, (Asn1F.AsnReader Reader, out Attribute _Item) => Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.Attribute.TryCreate(Reader, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.CryptographicMessageSyntax.ExtendedCertificateInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerImplicit(Writer, 0, (int)this.version);
				this.certificate.Write(Writer);
				WriteArrayImplicit(Writer, 2, this.attributes);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ExtendedCertificateInfo _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.certificate, _Typed.certificate)) return false;
			if (!AreEqual(this.attributes, _Typed.attributes)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.certificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.attributes);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
