using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
	[TestClass]
	public class EcdsaTests
	{
		[TestMethod]
		public void Test_01_NIST_P192()
		{
			this.Test_Signature(new NistP192(), new NistP192(), HashFunction.SHA256);
		}

		[TestMethod]
		public void Test_02_NIST_P224()
		{
			this.Test_Signature(new NistP224(), new NistP224(), HashFunction.SHA256);
		}

		[TestMethod]
		public void Test_03_NIST_P256()
		{
			this.Test_Signature(new NistP256(), new NistP256(), HashFunction.SHA256);
		}

		[TestMethod]
		public void Test_04_NIST_P384()
		{
			this.Test_Signature(new NistP384(), new NistP384(), HashFunction.SHA512);
		}

		[TestMethod]
		public void Test_05_NIST_P521()
		{
			this.Test_Signature(new NistP521(), new NistP521(), HashFunction.SHA512);
		}

		[TestMethod]
		public void Test_06_Curve25519()
		{
			this.Test_Signature(new Curve25519(), new Curve25519(), HashFunction.SHA256);
		}

		[TestMethod]
		public void Test_07_Curve448()
		{
			this.Test_Signature(new Curve448(), new Curve448(), HashFunction.SHA512);
		}

		public void Test_Signature(CurvePrimeField Curve1, CurvePrimeField Curve2,
			HashFunction HashFunction)
		{
			int n;

			using (RandomNumberGenerator rnd = RandomNumberGenerator.Create())
			{
				for (n = 0; n < 100; n++)
				{
					byte[] Data = new byte[1024];

					rnd.GetBytes(Data);

					KeyValuePair<BigInteger, BigInteger> Signature = Curve1.Sign(Data, HashFunction);
					bool Valid = Curve2.Verify(Data, Curve1.PublicKey, HashFunction, Signature);

					Assert.IsTrue(Valid);

					Curve1.GenerateKeys();
					Curve2.GenerateKeys();
				}
			}
		}
	}
}
