using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
	[TestClass]
	public class SignatureTests
	{
		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_01_ECDSA_NIST_P192(bool BigEndian)
		{
			Test_Signature(new NistP192(), new NistP192(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_02_ECDSA_NIST_P224(bool BigEndian)
		{
			Test_Signature(new NistP224(), new NistP224(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_03_ECDSA_NIST_P256(bool BigEndian)
		{
			Test_Signature(new NistP256(), new NistP256(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_04_ECDSA_NIST_P384(bool BigEndian)
		{
			Test_Signature(new NistP384(), new NistP384(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_05_ECDSA_NIST_P521(bool BigEndian)
		{
			Test_Signature(new NistP521(), new NistP521(), BigEndian);
		}

		[TestMethod]
        [Ignore]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_06_XEdDSA_Curve25519(bool BigEndian)
		{
			Test_Signature(new Curve25519(), new Curve25519(), BigEndian);
		}

		[TestMethod]
        [Ignore]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_07_XEdDSA_Curve448(bool BigEndian)
		{
			Test_Signature(new Curve448(), new Curve448(), BigEndian);
		}

        [TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_08_EdDSA_Ed25519(bool BigEndian)
		{
			Test_Signature(new Edwards25519(), new Edwards25519(), BigEndian);
        }

        [TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_09_EdDSA_Ed448(bool BigEndian)
		{
			Test_Signature(new Edwards448(), new Edwards448(), BigEndian);
        }

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_10_ECDSA_Brainpool_P160(bool BigEndian)
		{
			Test_Signature(new BrainpoolP160(), new BrainpoolP160(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_11_ECDSA_Brainpool_P192(bool BigEndian)
		{
			Test_Signature(new BrainpoolP192(), new BrainpoolP192(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_12_ECDSA_Brainpool_P224(bool BigEndian)
		{
			Test_Signature(new BrainpoolP224(), new BrainpoolP224(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_13_ECDSA_Brainpool_P256(bool BigEndian)
		{
			Test_Signature(new BrainpoolP256(), new BrainpoolP256(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_14_ECDSA_Brainpool_P320(bool BigEndian)
		{
			Test_Signature(new BrainpoolP320(), new BrainpoolP320(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_15_ECDSA_Brainpool_P384(bool BigEndian)
		{
			Test_Signature(new BrainpoolP384(), new BrainpoolP384(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_16_ECDSA_Brainpool_P512(bool BigEndian)
		{
			Test_Signature(new BrainpoolP512(), new BrainpoolP512(), BigEndian);
		}

		public static void Test_Signature(PrimeFieldCurve Curve1, PrimeFieldCurve Curve2, bool BigEndian)
		{
			using RandomNumberGenerator rnd = RandomNumberGenerator.Create();
			int n;
			
			for (n = 0; n < 100; n++)
			{
				byte[] Data = new byte[1024];

				rnd.GetBytes(Data);

				byte[] Signature = Curve1.Sign(Data, BigEndian);
				bool Valid = Curve2.Verify(Data,
					BigEndian ? Curve1.PublicKeyBigEndian : Curve1.PublicKey,
					BigEndian, Signature);

				Assert.IsTrue(Valid);

				Curve1.GenerateKeys();
				Curve2.GenerateKeys();
			}
		}
	}
}
