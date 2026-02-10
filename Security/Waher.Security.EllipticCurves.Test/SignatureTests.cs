using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
	[TestClass]
	public class SignatureTests
	{
		[TestMethod]
		public void Test_01_ECDSA_NIST_P192()
		{
			Test_Signature(new NistP192(), new NistP192());
		}

		[TestMethod]
		public void Test_02_ECDSA_NIST_P224()
		{
			Test_Signature(new NistP224(), new NistP224());
		}

		[TestMethod]
		public void Test_03_ECDSA_NIST_P256()
		{
			Test_Signature(new NistP256(), new NistP256());
		}

		[TestMethod]
		public void Test_04_ECDSA_NIST_P384()
		{
			Test_Signature(new NistP384(), new NistP384());
		}

		[TestMethod]
		public void Test_05_ECDSA_NIST_P521()
		{
			Test_Signature(new NistP521(), new NistP521());
		}

		[TestMethod]
        [Ignore]
		public void Test_06_XEdDSA_Curve25519()
		{
			Test_Signature(new Curve25519(), new Curve25519());
		}

		[TestMethod]
        [Ignore]
		public void Test_07_XEdDSA_Curve448()
		{
			Test_Signature(new Curve448(), new Curve448());
		}

        [TestMethod]
        public void Test_08_EdDSA_Ed25519()
        {
			Test_Signature(new Edwards25519(), new Edwards25519());
        }

        [TestMethod]
        public void Test_09_EdDSA_Ed448()
        {
			Test_Signature(new Edwards448(), new Edwards448());
        }

		[TestMethod]
		public void Test_10_ECDSA_Brainpool_P160()
		{
			Test_Signature(new BrainpoolP160(), new BrainpoolP160());
		}

		[TestMethod]
		public void Test_11_ECDSA_Brainpool_P192()
		{
			Test_Signature(new BrainpoolP192(), new BrainpoolP192());
		}

		[TestMethod]
		public void Test_12_ECDSA_Brainpool_P224()
		{
			Test_Signature(new BrainpoolP224(), new BrainpoolP224());
		}

		[TestMethod]
		public void Test_13_ECDSA_Brainpool_P256()
		{
			Test_Signature(new BrainpoolP256(), new BrainpoolP256());
		}

		[TestMethod]
		public void Test_14_ECDSA_Brainpool_P320()
		{
			Test_Signature(new BrainpoolP320(), new BrainpoolP320());
		}

		[TestMethod]
		public void Test_15_ECDSA_Brainpool_P384()
		{
			Test_Signature(new BrainpoolP384(), new BrainpoolP384());
		}

		[TestMethod]
		public void Test_16_ECDSA_Brainpool_P512()
		{
			Test_Signature(new BrainpoolP512(), new BrainpoolP512());
		}

		public static void Test_Signature(PrimeFieldCurve Curve1, PrimeFieldCurve Curve2)
		{
			int n;
            int Ok = 0;
            int Errors = 0;

			using (RandomNumberGenerator rnd = RandomNumberGenerator.Create())
			{
				for (n = 0; n < 100; n++)
				{
					byte[] Data = new byte[1024];

					rnd.GetBytes(Data);

                    try
                    {
                        byte[] Signature = Curve1.Sign(Data);
                        bool Valid = Curve2.Verify(Data, Curve1.PublicKey, Signature);

                        if (Valid)
                            Ok++;
                        else
                            Errors++;
                    }
                    catch (Exception)
                    {
                        Errors++;
                    }

					Curve1.GenerateKeys();
					Curve2.GenerateKeys();
				}
			}

            Assert.AreEqual(0, Errors);
		}
	}
}
