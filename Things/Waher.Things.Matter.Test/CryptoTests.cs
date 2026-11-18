using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Test.TestVectors;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the cryptographic primitives used by Matter, using published test vectors.
	/// </summary>
	[TestClass]
	public sealed class CryptoTests
	{
		private static byte[] H(string s) => MatterCrypto.FromHex(s);
		private static string X(byte[] b) => MatterCrypto.ToHex(b);

		[TestMethod]
		public void Test_01_HKDF_RFC5869_1()
		{
			byte[] Okm = MatterCrypto.Hkdf(
				H("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b"),
				H("000102030405060708090a0b0c"),
				H("f0f1f2f3f4f5f6f7f8f9"), 42);

			Assert.AreEqual("3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865", X(Okm));
		}

		[TestMethod]
		public void Test_02_HKDF_RFC5869_3()
		{
			byte[] Okm = MatterCrypto.Hkdf(
				H("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b"),
				null, Array.Empty<byte>(), 42);

			Assert.AreEqual("8da4e775a563c18f715f802a063c5a31b8a11f5c5ee1879ec3454e5f3c738d2d9d201395faa4b61a96c8", X(Okm));
		}

		[TestMethod]
		public void Test_03_PBKDF2()
		{
			// RFC 7914, section 11
			byte[] Dk = MatterCrypto.Pbkdf2Sha256(Encoding.ASCII.GetBytes("passwd"), Encoding.ASCII.GetBytes("salt"), 1, 64);
			Assert.AreEqual("55ac046e56e3089fec1691c22544b605f94185216dde0465e68b9d57c20dacbc49ca9cccf179b645991664b39d77ef317c71b845b1e30bd509112041d3a19783", X(Dk));
		}

		[TestMethod]
		public void Test_04_AES_CCM_TestVectors()
		{
			int NrTested = 0;

			foreach (string[] V in AesCcmVectors.Vectors)
			{
				byte[] Key = H(V[0]);
				byte[] Nonce = H(V[1]);
				byte[] Aad = H(V[2]);
				byte[] Pt = H(V[3]);
				byte[] Ct = H(V[4]);
				byte[] Tag = H(V[5]);
				bool Valid = V[6] == "1";

				if (Key.Length != 16)
					continue;

				if (Valid)
				{
					byte[] Encrypted = AesCcm128.Encrypt(Key, Nonce, Pt, Aad, Tag.Length);
					Assert.AreEqual(X(Ct) + X(Tag), X(Encrypted));

					byte[] Decrypted = AesCcm128.Decrypt(Key, Nonce, Encrypted, Aad, Tag.Length);
					Assert.AreEqual(X(Pt), X(Decrypted));
				}
				else
				{
					Assert.ThrowsExactly<MatterSecurityException>(() =>
						AesCcm128.Decrypt(Key, Nonce, MatterCrypto.Concat(Ct, Tag), Aad, Tag.Length));
				}

				NrTested++;
			}

			Assert.IsTrue(NrTested > 10);
		}

		[TestMethod]
		public void Test_05_AES_CCM_vs_Platform()
		{
			Random Rnd = new(1234);

			for (int i = 0; i < 200; i++)
			{
				byte[] Key = new byte[16];
				byte[] Nonce = new byte[13];
				byte[] Pt = new byte[Rnd.Next(0, 300)];
				byte[] Aad = new byte[Rnd.Next(0, 60)];

				Rnd.NextBytes(Key);
				Rnd.NextBytes(Nonce);
				Rnd.NextBytes(Pt);
				Rnd.NextBytes(Aad);

				byte[] Ours = AesCcm128.Encrypt(Key, Nonce, Pt, Aad);

				using AesCcm Platform = new(Key);
				byte[] Ct = new byte[Pt.Length];
				byte[] Tag = new byte[16];
				Platform.Encrypt(Nonce, Pt, Ct, Tag, Aad);

				Assert.AreEqual(X(Ct) + X(Tag), X(Ours));

				Ours[Rnd.Next(Ours.Length)] ^= 1;
				Assert.ThrowsExactly<MatterSecurityException>(() => AesCcm128.Decrypt(Key, Nonce, Ours, Aad));
			}
		}

		[TestMethod]
		public void Test_06_P256_KeyGeneration_ECDSA()
		{
			using EcKeyPair Key = EcKeyPair.Generate();
			Assert.AreEqual(32, Key.PrivateKey.Length);
			Assert.AreEqual(65, Key.PublicKey.Length);

			byte[] Data = Encoding.UTF8.GetBytes("Hello Matter");
			byte[] Signature = Key.Sign(Data);
			Assert.AreEqual(64, Signature.Length);
			Assert.IsTrue(EcKeyPair.Verify(Key.PublicKey, Data, Signature));

			Data[0] ^= 1;
			Assert.IsFalse(EcKeyPair.Verify(Key.PublicKey, Data, Signature));

			byte[] Der = EcKeyPair.RawToDerSignature(Signature);
			Assert.AreEqual(X(Signature), X(EcKeyPair.DerToRawSignature(Der)));
		}

		[TestMethod]
		public void Test_07_P256_ECDH_vs_Platform()
		{
			for (int i = 0; i < 10; i++)
			{
				using EcKeyPair A = EcKeyPair.Generate();

				using ECDiffieHellman B = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
				ECParameters P = B.ExportParameters(false);
				byte[] BPublic = MatterCrypto.Concat(new byte[] { 4 }, P.Q.X!, P.Q.Y!);

				byte[] Secret = A.Ecdh(BPublic);

				using ECDiffieHellman APlatform = ECDiffieHellman.Create(new ECParameters()
				{
					Curve = ECCurve.NamedCurves.nistP256,
					D = A.PrivateKey,
					Q = new ECPoint() { X = MatterCrypto.Slice(A.PublicKey, 1, 32), Y = MatterCrypto.Slice(A.PublicKey, 33, 32) }
				});

				byte[] Expected = APlatform.DeriveKeyFromHash(B.PublicKey, HashAlgorithmName.SHA256);
				Assert.AreEqual(X(Expected), X(MatterCrypto.Sha256(Secret)));
			}
		}

		[TestMethod]
		public void Test_08_SPAKE2Plus_TestVectors()
		{
			foreach (string[] V in Spake2PlusVectors.Vectors)
			{
				byte[] Context = H(V[0]);
				byte[] IdP = H(V[1]);
				byte[] IdV = H(V[2]);
				BigInteger W0 = P256.FromBigEndian(H(V[3]));
				BigInteger W1 = P256.FromBigEndian(H(V[4]));
				P256Point L = P256.Decode(H(V[5]));
				BigInteger x = P256.FromBigEndian(H(V[6]));
				BigInteger y = P256.FromBigEndian(H(V[8]));

				Assert.AreEqual(V[5], X(P256.Encode(P256.MultiplyBase(W1))));

				Spake2Plus Prover = Spake2Plus.CreateProver(Context, W0, W1, x, IdP, IdV);
				Spake2Plus Verifier = Spake2Plus.CreateVerifier(Context, W0, L, y, IdP, IdV);

				byte[] ShareX = Prover.ComputeShare();
				byte[] ShareY = Verifier.ComputeShare();

				Assert.AreEqual(V[7], X(ShareX));
				Assert.AreEqual(V[9], X(ShareY));

				Prover.ProcessPeerShare(ShareY);
				Verifier.ProcessPeerShare(ShareX);

				Assert.AreEqual(V[13], X(Prover.Ke));
				Assert.AreEqual(V[13], X(Verifier.Ke));
				Assert.AreEqual(V[16], X(Prover.ConfirmationA));
				Assert.AreEqual(V[17], X(Prover.ConfirmationB));
				Assert.AreEqual(V[16], X(Verifier.ConfirmationA));
				Assert.AreEqual(V[17], X(Verifier.ConfirmationB));
			}
		}

		[TestMethod]
		public void Test_09_SPAKE2Plus_Matter_RoundTrip()
		{
			byte[] Salt = H("5350414b45322b2050424b4446205465737420");   // "SPAKE2+ PBKDF Test "
			Spake2Plus.ComputeW0W1(20202021, Salt, 1000, out BigInteger W0, out BigInteger W1);
			byte[] VerifierData = Spake2Plus.ComputeVerifier(20202021, Salt, 1000);

			Assert.AreEqual(97, VerifierData.Length);
			Assert.AreEqual(X(P256.ToBigEndian(W0, 32)), X(MatterCrypto.Slice(VerifierData, 0, 32)));

			byte[] Context = MatterCrypto.Sha256(Encoding.ASCII.GetBytes("CHIP PAKE V1 Commissioning"));
			Spake2Plus Prover = Spake2Plus.CreateProver(Context, W0, W1);
			Spake2Plus Verifier = Spake2Plus.CreateVerifier(Context, W0, P256.Decode(MatterCrypto.Slice(VerifierData, 32, 65)));

			byte[] ShareX = Prover.ComputeShare();
			byte[] ShareY = Verifier.ComputeShare();
			Prover.ProcessPeerShare(ShareY);
			Verifier.ProcessPeerShare(ShareX);

			Assert.AreEqual(X(Prover.Ke), X(Verifier.Ke));
			Assert.AreEqual(X(Prover.ConfirmationA), X(Verifier.ConfirmationA));
			Assert.AreEqual(X(Prover.ConfirmationB), X(Verifier.ConfirmationB));
		}

		[TestMethod]
		public void Test_10_P256_PointCompression()
		{
			byte[] Compressed = H("02886e2f97ace46e55ba9dd7242579f2993b64e16ef3dcab95afd497333d8fa12f");
			P256Point M = P256.Decode(Compressed);
			Assert.IsTrue(P256.IsOnCurve(M));
			Assert.AreEqual("04886e2f97ace46e55ba9dd7242579f2993b64e16ef3dcab95afd497333d8fa12f5ff355163e43ce224e0b0e65ff02ac8e5c7be09419c785e0ca547d55a12e2d20", X(P256.Encode(M)));
		}
	}
}
