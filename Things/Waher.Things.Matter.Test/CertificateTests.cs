using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Test.TestVectors;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests Matter certificate encoding, conversion to X.509 and signature validation, using SDK test vectors.
	/// </summary>
	[TestClass]
	public sealed class CertificateTests
	{
		private static byte[] H(string s) => MatterCrypto.FromHex(s);
		private static string X(byte[] b) => MatterCrypto.ToHex(b);

		[TestMethod]
		public void Test_01_TlvToX509()
		{
			int NrTested = 0;

			foreach (string[] V in CertificateVectors.Vectors)
			{
				if (string.IsNullOrEmpty(V[1]) || string.IsNullOrEmpty(V[2]) || V[0].Contains("Err"))
					continue;

				MatterCertificate Cert = MatterCertificate.DecodeTlv(H(V[1]));
				Assert.AreEqual(V[2], X(Cert.EncodeX509Der()), "X.509 conversion failed for " + V[0]);

				if (!string.IsNullOrEmpty(V[3]))
					Assert.AreEqual(V[3], X(Cert.PublicKey), "Public key mismatch for " + V[0]);

				if (!string.IsNullOrEmpty(V[4]))
					Assert.AreEqual(V[4], X(Cert.SubjectKeyId), "Subject key ID mismatch for " + V[0]);

				if (!string.IsNullOrEmpty(V[5]))
					Assert.AreEqual(V[5], X(Cert.AuthorityKeyId), "Authority key ID mismatch for " + V[0]);

				NrTested++;
			}

			Assert.IsTrue(NrTested >= 10, NrTested.ToString() + " certificates tested.");
		}

		[TestMethod]
		public void Test_02_TlvRoundTrip()
		{
			foreach (string[] V in CertificateVectors.Vectors)
			{
				if (string.IsNullOrEmpty(V[1]) || V[0].Contains("Err"))
					continue;

				MatterCertificate Cert = MatterCertificate.DecodeTlv(H(V[1]));
				MatterCertificate Copy = new()
				{
					SerialNumber = Cert.SerialNumber,
					Issuer = Cert.Issuer,
					Subject = Cert.Subject,
					NotBefore = Cert.NotBefore,
					NotAfter = Cert.NotAfter,
					PublicKey = Cert.PublicKey,
					HasBasicConstraints = Cert.HasBasicConstraints,
					IsCa = Cert.IsCa,
					PathLength = Cert.PathLength,
					KeyUsage = Cert.KeyUsage,
					ExtendedKeyUsage = Cert.ExtendedKeyUsage,
					SubjectKeyId = Cert.SubjectKeyId,
					AuthorityKeyId = Cert.AuthorityKeyId,
					Signature = Cert.Signature
				};

				foreach (byte[] Ext in Cert.FutureExtensions)
					Copy.AddFutureExtension(Ext);

				Assert.AreEqual(X(Cert.EncodeTbsDer()), X(Copy.EncodeTbsDer()), "TBS mismatch for " + V[0]);
			}
		}

		[TestMethod]
		public void Test_03_SignatureChains()
		{
			Dictionary<string, MatterCertificate> BySkid = new();
			List<KeyValuePair<string, MatterCertificate>> All = new();

			foreach (string[] V in CertificateVectors.Vectors)
			{
				if (string.IsNullOrEmpty(V[1]) || V[0].Contains("Err"))
					continue;

				MatterCertificate Cert = MatterCertificate.DecodeTlv(H(V[1]));
				if (Cert.SubjectKeyId is not null)
					BySkid[X(Cert.SubjectKeyId)] = Cert;

				All.Add(new KeyValuePair<string, MatterCertificate>(V[0], Cert));
			}

			int NrVerified = 0;

			foreach (KeyValuePair<string, MatterCertificate> P in All)
			{
				if (P.Value.AuthorityKeyId is null)
					continue;

				if (!BySkid.TryGetValue(X(P.Value.AuthorityKeyId), out MatterCertificate Issuer))
					continue;

				Assert.IsTrue(P.Value.VerifySignature(Issuer.PublicKey), "Signature verification failed for " + P.Key);
				NrVerified++;
			}

			Assert.IsTrue(NrVerified >= 8, NrVerified.ToString() + " signatures verified.");
		}

		[TestMethod]
		public void Test_04_GeneratedChain()
		{
			using EcKeyPair RootKey = EcKeyPair.Generate();
			using EcKeyPair NodeKey = EcKeyPair.Generate();

			MatterCertificate Rcac = CertificateFactory.CreateRcac(RootKey, 0x1234, 0xFAB000000000001D, DateTime.UtcNow.AddHours(-1));
			Assert.IsTrue(Rcac.VerifySignature(RootKey.PublicKey));

			MatterCertificate Noc = CertificateFactory.CreateNoc(Rcac, RootKey, NodeKey.PublicKey, 0xDEDEDEDE00010001, 0xFAB000000000001D,
				new uint[] { 0xABCD0001 }, DateTime.UtcNow.AddHours(-1));

			Assert.IsTrue(Noc.VerifySignature(RootKey.PublicKey));
			Assert.IsFalse(Noc.VerifySignature(NodeKey.PublicKey));

			byte[] Tlv = Noc.EncodeTlv();
			Assert.IsTrue(Tlv.Length <= MatterCertificate.MaxTlvSize);

			MatterCertificate Decoded = MatterCertificate.DecodeTlv(Tlv);
			Assert.IsTrue(Decoded.VerifySignature(RootKey.PublicKey));
			Assert.AreEqual(0xDEDEDEDE00010001UL, Decoded.Subject.NodeId);
			Assert.AreEqual(0xFAB000000000001DUL, Decoded.Subject.FabricId);
			Assert.AreEqual(0xABCD0001U, Decoded.Subject.CaseAuthenticatedTags[0]);

			System.Security.Cryptography.X509Certificates.X509Certificate2 X509 = new(Noc.EncodeX509Der());
			Assert.IsTrue(X509.Subject.Length > 0);
		}
	}
}
