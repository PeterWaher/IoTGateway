using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Waher.Events;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;

namespace Waher.Things.Matter.Attestation
{
	/// <summary>
	/// Trust store of Product Attestation Authority (PAA) root certificates, used to validate device attestation
	/// certificate chains (Matter Core Specification, 6.2).
	/// </summary>
	public class AttestationTrustStore
	{
		private const string ResourcePrefix = "Waher.Things.Matter.Certificates.Trust.";

		private static readonly object synchObj = new object();
		private static X509CertificateInfo[] productionPaas = null;
		private static X509CertificateInfo[] developmentPaas = null;

		private readonly Dictionary<string, X509CertificateInfo> bySkid = new Dictionary<string, X509CertificateInfo>();

		/// <summary>
		/// Trust store of PAA root certificates.
		/// </summary>
		/// <param name="IncludeProductionPaas">If the embedded production PAAs (mirrored from the CSA Distributed
		/// Compliance Ledger) should be included.</param>
		/// <param name="IncludeDevelopmentPaas">If the publicly known development/test PAAs should be included.
		/// Should only be enabled during development and testing.</param>
		public AttestationTrustStore(bool IncludeProductionPaas, bool IncludeDevelopmentPaas)
		{
			if (IncludeProductionPaas)
			{
				foreach (X509CertificateInfo Cert in GetEmbedded(true))
					this.Add(Cert);
			}

			if (IncludeDevelopmentPaas)
			{
				foreach (X509CertificateInfo Cert in GetEmbedded(false))
					this.Add(Cert);
			}
		}

		/// <summary>
		/// Number of trusted PAA certificates.
		/// </summary>
		public int Count
		{
			get
			{
				lock (this.bySkid)
				{
					return this.bySkid.Count;
				}
			}
		}

		private static X509CertificateInfo[] GetEmbedded(bool Production)
		{
			lock (synchObj)
			{
				if (Production)
				{
					if (productionPaas is null)
						productionPaas = LoadEmbedded(ResourcePrefix + "Production.");

					return productionPaas;
				}
				else
				{
					if (developmentPaas is null)
						developmentPaas = LoadEmbedded(ResourcePrefix + "Development.");

					return developmentPaas;
				}
			}
		}

		private static X509CertificateInfo[] LoadEmbedded(string Prefix)
		{
			List<X509CertificateInfo> Result = new List<X509CertificateInfo>();
			Assembly A = typeof(AttestationTrustStore).Assembly;

			foreach (string Name in A.GetManifestResourceNames())
			{
				if (!Name.StartsWith(Prefix, StringComparison.Ordinal))
					continue;

				try
				{
					using (Stream s = A.GetManifestResourceStream(Name))
					using (MemoryStream ms = new MemoryStream())
					{
						s.CopyTo(ms);
						X509CertificateInfo Cert = X509CertificateInfo.Parse(ms.ToArray());

						if (Cert.IsP256)
							Result.Add(Cert);
					}
				}
				catch (Exception ex)
				{
					Log.Warning("Unable to load embedded PAA certificate " + Name + ": " + ex.Message);
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Adds a trusted PAA certificate.
		/// </summary>
		/// <param name="Certificate">Certificate.</param>
		/// <returns>If the certificate was added.</returns>
		public bool Add(X509CertificateInfo Certificate)
		{
			if (Certificate.SubjectKeyId is null || !Certificate.IsCa)
				return false;

			lock (this.bySkid)
			{
				this.bySkid[MatterCrypto.ToHex(Certificate.SubjectKeyId)] = Certificate;
			}

			return true;
		}

		/// <summary>
		/// Adds a trusted PAA certificate (DER or PEM).
		/// </summary>
		/// <param name="Encoded">DER or PEM encoding.</param>
		/// <returns>If the certificate was added.</returns>
		public bool Add(byte[] Encoded)
		{
			byte[] Der = DecodePemOrDer(Encoded);
			return this.Add(X509CertificateInfo.Parse(Der));
		}

		/// <summary>
		/// Loads all certificates (*.der, *.pem, *.crt, *.cer) in a folder.
		/// </summary>
		/// <param name="Folder">Folder.</param>
		/// <returns>Number of certificates added.</returns>
		public int LoadFolder(string Folder)
		{
			int NrAdded = 0;

			if (!Directory.Exists(Folder))
				return 0;

			foreach (string FileName in Directory.GetFiles(Folder))
			{
				string Ext = Path.GetExtension(FileName).ToLowerInvariant();
				if (Ext != ".der" && Ext != ".pem" && Ext != ".crt" && Ext != ".cer")
					continue;

				try
				{
					if (this.Add(File.ReadAllBytes(FileName)))
						NrAdded++;
				}
				catch (Exception ex)
				{
					Log.Warning("Unable to load PAA certificate " + FileName + ": " + ex.Message);
				}
			}

			return NrAdded;
		}

		/// <summary>
		/// Finds a trusted PAA by subject key identifier.
		/// </summary>
		/// <param name="SubjectKeyId">Subject key identifier.</param>
		/// <returns>Certificate, or null if not found.</returns>
		public X509CertificateInfo FindBySubjectKeyId(byte[] SubjectKeyId)
		{
			if (SubjectKeyId is null)
				return null;

			lock (this.bySkid)
			{
				if (this.bySkid.TryGetValue(MatterCrypto.ToHex(SubjectKeyId), out X509CertificateInfo Cert))
					return Cert;
				else
					return null;
			}
		}

		/// <summary>
		/// Decodes a certificate that may be PEM or DER encoded.
		/// </summary>
		/// <param name="Encoded">Encoded certificate.</param>
		/// <returns>DER encoding.</returns>
		public static byte[] DecodePemOrDer(byte[] Encoded)
		{
			if (Encoded.Length > 0 && Encoded[0] == 0x30)
				return Encoded;

			string s = Encoding.ASCII.GetString(Encoded);
			int i = s.IndexOf("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal);
			if (i < 0)
				throw new MatterDecodingException("Not a certificate.");

			i += 27;
			int j = s.IndexOf("-----END CERTIFICATE-----", i, StringComparison.Ordinal);
			if (j < 0)
				throw new MatterDecodingException("Invalid PEM certificate.");

			return Convert.FromBase64String(s.Substring(i, j - i).Replace("\r", string.Empty).Replace("\n", string.Empty).Trim());
		}
	}
}
