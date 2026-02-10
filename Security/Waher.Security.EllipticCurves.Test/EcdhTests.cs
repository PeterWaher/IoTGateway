using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
    [TestClass]
    public class EcdhTests
    {
        [TestMethod]
        public void Test_01_NIST_P192()
        {
			Test_ECDH(new NistP192(), new NistP192());
        }

		[TestMethod]
		public void Test_02_NIST_P224()
		{
			Test_ECDH(new NistP224(), new NistP224());
		}

		[TestMethod]
		public void Test_03_NIST_P256()
		{
			Test_ECDH(new NistP256(), new NistP256());
		}

		[TestMethod]
		public void Test_04_NIST_P384()
		{
			Test_ECDH(new NistP384(), new NistP384());
		}

		[TestMethod]
		public void Test_05_NIST_P521()
		{
			Test_ECDH(new NistP521(), new NistP521());
		}

		[TestMethod]
		public void Test_06_Curve25519()
		{
			Test_ECDH(new Curve25519(), new Curve25519());
		}

		[TestMethod]
		public void Test_07_Curve448()
		{
			Test_ECDH(new Curve448(), new Curve448());
		}

        [TestMethod]
        public void Test_08_Edwards25519()
        {
			Test_ECDH(new Edwards25519(), new Edwards25519());
        }

        [TestMethod]
        public void Test_09_Edwards448()
        {
			Test_ECDH(new Edwards448(), new Edwards448());
        }

		[TestMethod]
		public void Test_10_Brainpool_P160()
		{
			Test_ECDH(new BrainpoolP160(), new BrainpoolP160());
		}

		[TestMethod]
		public void Test_11_Brainpool_P192()
		{
			Test_ECDH(new BrainpoolP192(), new BrainpoolP192());
		}

		[TestMethod]
		public void Test_12_Brainpool_P224()
		{
			Test_ECDH(new BrainpoolP224(), new BrainpoolP224());
		}

		[TestMethod]
		public void Test_13_Brainpool_P256()
		{
			Test_ECDH(new BrainpoolP256(), new BrainpoolP256());
		}

		[TestMethod]
		public void Test_14_Brainpool_P320()
		{
			Test_ECDH(new BrainpoolP320(), new BrainpoolP320());
		}

		[TestMethod]
		public void Test_15_Brainpool_P384()
		{
			Test_ECDH(new BrainpoolP384(), new BrainpoolP384());
		}

		[TestMethod]
		public void Test_16_Brainpool_P512()
		{
			Test_ECDH(new BrainpoolP512(), new BrainpoolP512());
		}

		public static void Test_ECDH(PrimeFieldCurve Curve1, PrimeFieldCurve Curve2)
		{
			int n;

			for (n = 0; n < 100; n++)
			{
				byte[] Key1 = Curve1.GetSharedKey(Curve2.PublicKey, Hashes.GetHashFunctionArray(Curve2.HashFunction));
				byte[] Key2 = Curve2.GetSharedKey(Curve1.PublicKey, Hashes.GetHashFunctionArray(Curve1.HashFunction));

                Assert.AreEqual(Hashes.BinaryToString(Key1), Hashes.BinaryToString(Key2));

				Curve1.GenerateKeys();
				Curve2.GenerateKeys();
			}
		}
	}
}
