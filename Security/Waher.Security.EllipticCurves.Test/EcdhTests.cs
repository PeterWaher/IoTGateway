using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
    [TestClass]
    public class EcdhTests
    {
        [TestMethod]
        public void Test_01_NIST_P192()
        {
			this.Test_ECDH(new NistP192(), new NistP192());
        }

		[TestMethod]
		public void Test_02_NIST_P224()
		{
			this.Test_ECDH(new NistP224(), new NistP224());
		}

		[TestMethod]
		public void Test_03_NIST_P256()
		{
			this.Test_ECDH(new NistP256(), new NistP256());
		}

		[TestMethod]
		public void Test_04_NIST_P384()
		{
			this.Test_ECDH(new NistP384(), new NistP384());
		}

		[TestMethod]
		public void Test_05_NIST_P521()
		{
			this.Test_ECDH(new NistP521(), new NistP521());
		}

		[TestMethod]
		public void Test_06_Curve25519()
		{
			this.Test_ECDH(new Curve25519(), new Curve25519());
		}

		[TestMethod]
		public void Test_07_Curve448()
		{
			this.Test_ECDH(new Curve448(), new Curve448());
		}

        [TestMethod]
        public void Test_08_Edwards25519()
        {
            this.Test_ECDH(new Edwards25519(), new Edwards25519());
        }

        [TestMethod]
        public void Test_09_Edwards448()
        {
            this.Test_ECDH(new Edwards448(), new Edwards448());
        }

        public void Test_ECDH(PrimeFieldCurve Curve1, PrimeFieldCurve Curve2)
		{
			int n;

			for (n = 0; n < 100; n++)
			{
				byte[] Key1 = Curve1.GetSharedKey(Curve2.PublicKey, Hashes.ComputeSHA256Hash);
				byte[] Key2 = Curve2.GetSharedKey(Curve1.PublicKey, Hashes.ComputeSHA256Hash);

                Assert.AreEqual(Hashes.BinaryToString(Key1), Hashes.BinaryToString(Key2));

				Curve1.GenerateKeys();
				Curve2.GenerateKeys();
			}
		}
	}
}
