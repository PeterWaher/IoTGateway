using System;
using System.Globalization;
using System.Text;
using Waher.Things.Matter.Asn1;
using Waher.Things.Matter.Crypto;

namespace Waher.Things.Matter.Certificates
{
	/// <summary>
	/// Parsed X.509 certificate (DER), as used for device attestation certificates (DAC, PAI, PAA).
	/// </summary>
	public class X509CertificateInfo
	{
		private X509CertificateInfo()
		{
		}

		/// <summary>
		/// DER encoding of the complete certificate.
		/// </summary>
		public byte[] Der { get; private set; }

		/// <summary>
		/// DER encoding of the TBSCertificate.
		/// </summary>
		public byte[] TbsDer { get; private set; }

		/// <summary>
		/// X.509 version (1, 2 or 3).
		/// </summary>
		public int Version { get; private set; }

		/// <summary>
		/// Serial number (big-endian, two's complement).
		/// </summary>
		public byte[] SerialNumber { get; private set; }

		/// <summary>
		/// Signature algorithm OID.
		/// </summary>
		public string SignatureAlgorithm { get; private set; }

		/// <summary>
		/// Issuer.
		/// </summary>
		public DistinguishedName Issuer { get; private set; }

		/// <summary>
		/// Subject.
		/// </summary>
		public DistinguishedName Subject { get; private set; }

		/// <summary>
		/// DER encoding of the issuer name.
		/// </summary>
		public byte[] IssuerDer { get; private set; }

		/// <summary>
		/// DER encoding of the subject name.
		/// </summary>
		public byte[] SubjectDer { get; private set; }

		/// <summary>
		/// Start of validity.
		/// </summary>
		public DateTime NotBefore { get; private set; }

		/// <summary>
		/// End of validity.
		/// </summary>
		public DateTime NotAfter { get; private set; }

		/// <summary>
		/// Public key algorithm OID.
		/// </summary>
		public string PublicKeyAlgorithm { get; private set; }

		/// <summary>
		/// Public key curve OID (for EC keys).
		/// </summary>
		public string PublicKeyCurve { get; private set; }

		/// <summary>
		/// Public key (uncompressed EC point for P-256 keys).
		/// </summary>
		public byte[] PublicKey { get; private set; }

		/// <summary>
		/// Signature (raw r || s for ECDSA).
		/// </summary>
		public byte[] Signature { get; private set; }

		/// <summary>
		/// If the basic constraints extension is present.
		/// </summary>
		public bool HasBasicConstraints { get; private set; }

		/// <summary>
		/// If the certificate is a CA certificate.
		/// </summary>
		public bool IsCa { get; private set; }

		/// <summary>
		/// Path length constraint, if any.
		/// </summary>
		public int? PathLength { get; private set; }

		/// <summary>
		/// Key usage bits (bit 0 = digitalSignature, ..., as in X.509), if present.
		/// </summary>
		public KeyUsage? KeyUsage { get; private set; }

		/// <summary>
		/// Subject key identifier.
		/// </summary>
		public byte[] SubjectKeyId { get; private set; }

		/// <summary>
		/// Authority key identifier.
		/// </summary>
		public byte[] AuthorityKeyId { get; private set; }

		/// <summary>
		/// Matter Vendor ID, if encoded in the subject.
		/// </summary>
		public ushort? VendorId { get; private set; }

		/// <summary>
		/// Matter Product ID, if encoded in the subject.
		/// </summary>
		public ushort? ProductId { get; private set; }

		/// <summary>
		/// Common name of subject.
		/// </summary>
		public string CommonName => this.Subject?.GetString(DistinguishedName.TagCommonName);

		/// <summary>
		/// If the certificate is self-signed (subject equals issuer).
		/// </summary>
		public bool IsSelfSigned => MatterCrypto.AreEqual(this.SubjectDer, this.IssuerDer);

		/// <summary>
		/// Parses a DER-encoded X.509 certificate.
		/// </summary>
		/// <param name="Der">DER encoding.</param>
		/// <returns>Parsed certificate.</returns>
		public static X509CertificateInfo Parse(byte[] Der)
		{
			X509CertificateInfo Result = new X509CertificateInfo()
			{
				Der = Der
			};

			DerReader Reader = new DerReader(Der);
			DerReader Cert = Reader.ReadSequence();

			Result.TbsDer = Cert.ReadEncodedElement();

			DerReader SigAlg = Cert.ReadSequence();
			string OuterSigAlg = SigAlg.ReadOid();

			byte[] SignatureBits = Cert.ReadBitString(out _);

			if (OuterSigAlg == Oids.EcdsaWithSha256)
				Result.Signature = EcKeyPair.DerToRawSignature(SignatureBits);
			else
				Result.Signature = SignatureBits;

			DerReader Tbs = new DerReader(Result.TbsDer).ReadSequence();

			if (Tbs.IsContextSpecific(0))
			{
				DerReader V = Tbs.ReadContextSpecific(0);
				Result.Version = (int)V.ReadInteger() + 1;
			}
			else
				Result.Version = 1;

			Result.SerialNumber = Tbs.ReadIntegerBytes();

			DerReader TbsSigAlg = Tbs.ReadSequence();
			Result.SignatureAlgorithm = TbsSigAlg.ReadOid();

			if (Result.SignatureAlgorithm != OuterSigAlg)
				throw new MatterDecodingException("Signature algorithm mismatch in certificate.");

			int IssuerStart = Tbs.Position;
			Result.IssuerDer = Tbs.ReadEncodedElement();
			Result.Issuer = DistinguishedName.DecodeDer(new DerReader(Result.IssuerDer));

			DerReader Validity = Tbs.ReadSequence();
			Result.NotBefore = Validity.ReadTime();
			Result.NotAfter = Validity.ReadTime();

			Result.SubjectDer = Tbs.ReadEncodedElement();
			Result.Subject = DistinguishedName.DecodeDer(new DerReader(Result.SubjectDer));

			DerReader Spki = Tbs.ReadSequence();
			DerReader Alg = Spki.ReadSequence();
			Result.PublicKeyAlgorithm = Alg.ReadOid();

			if (Alg.HasMore && Alg.PeekTag() == DerWriter.TagOid)
				Result.PublicKeyCurve = Alg.ReadOid();

			Result.PublicKey = Spki.ReadBitString(out _);

			while (Tbs.HasMore)
			{
				if (Tbs.IsContextSpecific(3))
				{
					DerReader Extensions = Tbs.ReadContextSpecific(3).ReadSequence();

					while (Extensions.HasMore)
					{
						DerReader Ext = Extensions.ReadSequence();
						string Oid = Ext.ReadOid();

						if (Ext.HasMore && Ext.PeekTag() == DerWriter.TagBoolean)
							Ext.ReadBoolean();

						byte[] Value = Ext.ReadOctetString();
						DerReader v = new DerReader(Value);

						switch (Oid)
						{
							case Oids.BasicConstraints:
								Result.HasBasicConstraints = true;
								DerReader Bc = v.ReadSequence();

								if (Bc.HasMore && Bc.PeekTag() == DerWriter.TagBoolean)
									Result.IsCa = Bc.ReadBoolean();

								if (Bc.HasMore && Bc.PeekTag() == DerWriter.TagInteger)
									Result.PathLength = (int)Bc.ReadInteger();
								break;

							case Oids.KeyUsage:
								byte[] Bits = v.ReadBitString(out _);
								uint Mask = 0;
								int i;

								for (i = 0; i < Bits.Length * 8 && i < 16; i++)
								{
									if ((Bits[i / 8] & (0x80 >> (i % 8))) != 0)
										Mask |= 1u << i;
								}

								Result.KeyUsage = (KeyUsage)Mask;
								break;

							case Oids.SubjectKeyIdentifier:
								Result.SubjectKeyId = v.ReadOctetString();
								break;

							case Oids.AuthorityKeyIdentifier:
								DerReader Aki = v.ReadSequence();

								while (Aki.HasMore)
								{
									byte[] Content = Aki.ReadElement(out byte Tag);
									if (Tag == 0x80)
										Result.AuthorityKeyId = Content;
								}
								break;
						}
					}
				}
				else
					Tbs.Skip();
			}

			Result.ParseVendorAndProduct();

			return Result;
		}

		private void ParseVendorAndProduct()
		{
			string s = this.Subject.GetByOid(Oids.MatterVendorId);
			if (!string.IsNullOrEmpty(s))
			{
				if (s.Length != 4 || !ushort.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort Vid))
					throw new MatterDecodingException("Invalid Matter Vendor ID in certificate subject.");

				this.VendorId = Vid;
			}

			s = this.Subject.GetByOid(Oids.MatterProductId);
			if (!string.IsNullOrEmpty(s))
			{
				if (s.Length != 4 || !ushort.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort Pid))
					throw new MatterDecodingException("Invalid Matter Product ID in certificate subject.");

				this.ProductId = Pid;
			}

			string Cn = this.CommonName;

			if (!string.IsNullOrEmpty(Cn))
			{
				if (!this.VendorId.HasValue && TryParseFallback(Cn, "Mvid:", out ushort Vid))
					this.VendorId = Vid;

				if (!this.ProductId.HasValue && TryParseFallback(Cn, "Mpid:", out ushort Pid))
					this.ProductId = Pid;
			}
		}

		private static bool TryParseFallback(string CommonName, string Prefix, out ushort Value)
		{
			int i = CommonName.IndexOf(Prefix, StringComparison.Ordinal);

			if (i >= 0 && i + Prefix.Length + 4 <= CommonName.Length)
			{
				string s = CommonName.Substring(i + Prefix.Length, 4);

				foreach (char ch in s)
				{
					if (!((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F')))
					{
						Value = 0;
						return false;
					}
				}

				return ushort.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Value);
			}

			Value = 0;
			return false;
		}

		/// <summary>
		/// If the public key is a P-256 key.
		/// </summary>
		public bool IsP256 => this.PublicKeyAlgorithm == Oids.EcPublicKey && this.PublicKeyCurve == Oids.Prime256v1 &&
			!(this.PublicKey is null) && this.PublicKey.Length == 65;

		/// <summary>
		/// Verifies the signature of the certificate, using the public key of the issuer.
		/// </summary>
		/// <param name="IssuerPublicKey">Public key of issuer.</param>
		/// <returns>If signature is valid.</returns>
		public bool VerifySignature(byte[] IssuerPublicKey)
		{
			if (this.SignatureAlgorithm != Oids.EcdsaWithSha256)
				return false;

			return EcKeyPair.Verify(IssuerPublicKey, this.TbsDer, this.Signature);
		}

		/// <summary>
		/// If the certificate is valid at a given point in time.
		/// </summary>
		/// <param name="TP">Point in time.</param>
		/// <returns>If within validity period.</returns>
		public bool IsValidAt(DateTime TP)
		{
			TP = TP.ToUniversalTime();
			return TP >= this.NotBefore && TP <= this.NotAfter;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this.Subject?.ToString());

			if (this.VendorId.HasValue)
			{
				sb.Append(" [VID 0x");
				sb.Append(this.VendorId.Value.ToString("X4"));

				if (this.ProductId.HasValue)
				{
					sb.Append(", PID 0x");
					sb.Append(this.ProductId.Value.ToString("X4"));
				}

				sb.Append(']');
			}

			return sb.ToString();
		}
	}
}
