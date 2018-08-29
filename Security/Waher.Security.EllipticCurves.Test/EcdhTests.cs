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

		public void Test_ECDH(CurvePrimeField Curve1, CurvePrimeField Curve2)
		{
			int n;

			for (n = 0; n < 100; n++)
			{
				byte[] Key1 = Curve1.GetSharedKey(Curve2.PublicKey, HashFunction.SHA256);
				byte[] Key2 = Curve2.GetSharedKey(Curve1.PublicKey, HashFunction.SHA256);

				int i, c;
				Assert.AreEqual(c = Key1.Length, Key2.Length);
				for (i = 0; i < c; i++)
					Assert.AreEqual(Key1[i], Key2[i]);

				Curve1.GenerateKeys();
				Curve2.GenerateKeys();
			}
		}
	}
}
