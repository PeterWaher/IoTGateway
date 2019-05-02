using System;
using System.Globalization;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
	[TestClass]
	public class Arithmetic
	{
		[TestMethod]
		public void Test_01_Inverse()
		{
			CurvePrimeField C = new NistP256();
			int i;

			for (i = 0; i < 1000; i++)
			{
				BigInteger k = C.NextRandomNumber();
				BigInteger kInv = C.Invert(k);
				Assert.IsTrue(kInv >= BigInteger.One);
				Assert.IsTrue(kInv < C.Prime);

				BigInteger Mul = C.Multiply(k, kInv);

				Assert.IsTrue(Mul.IsOne);
			}
		}

		[TestMethod]
		public void Test_02_Negate()
		{
			CurvePrimeField C = new NistP256();
			int i;

			for (i = 0; i < 100; i++)
			{
				BigInteger k = C.NextRandomNumber();
				PointOnCurve P = C.ScalarMultiplication(k, C.PublicKey);
				PointOnCurve Q = P;
				C.Negate(ref Q);
				C.AddTo(ref P, Q);
				Assert.IsTrue(P.IsZero);
			}
		}

		[TestMethod]
		public void Test_03_Addition()
		{
			CurvePrimeField C = new NistP256();
			BigInteger k1, k2, k3;
			PointOnCurve P1, P2, P3;
			string s1, s2, s3;
			int i;

			for (i = 0; i < 100; i++)
			{
				k1 = C.NextRandomNumber();
				P1 = C.ScalarMultiplication(k1, C.PublicKey);
				s1 = P1.ToString();

				do
				{
					k2 = C.NextRandomNumber();
					P2 = C.ScalarMultiplication(k2, C.PublicKey);
					s2 = P2.ToString();
				}
				while (k2 == k1);

				Assert.AreNotEqual(P2, P1);
				Assert.AreNotEqual(s2, s1);

				do
				{
					k3 = C.NextRandomNumber();
					P3 = C.ScalarMultiplication(k3, C.PublicKey);
					s3 = P3.ToString();
				}
				while (k3 == k1 || k3 == k2);

				Assert.AreNotEqual(P3, P1);
				Assert.AreNotEqual(s3, s1);
				Assert.AreNotEqual(P3, P2);
				Assert.AreNotEqual(s3, s2);

				PointOnCurve S1 = P1;
				C.AddTo(ref S1, P2);
				C.AddTo(ref S1, P3);

				PointOnCurve S2 = P2;
				C.AddTo(ref S2, P3);
				C.AddTo(ref S2, P1);

				PointOnCurve S3 = P3;
				C.AddTo(ref S3, P1);
				C.AddTo(ref S3, P2);

				Assert.AreEqual(S1, S2);
				Assert.AreEqual(S2, S3);

				Assert.AreEqual(s1, P1.ToString());
				Assert.AreEqual(s2, P2.ToString());
				Assert.AreEqual(s3, P3.ToString());
			}
		}

		[TestMethod]
		public void Test_04_ScalarMultiplication()
		{
			CurvePrimeField C = new NistP256();
			Random Rnd = new Random();

			int k1 = Rnd.Next(1000, 2000);
			int k2 = Rnd.Next(1000, 2000);
			int k3 = Rnd.Next(1000, 2000);

			PointOnCurve P1 = C.ScalarMultiplication(k1, C.PublicKey);
			PointOnCurve P2 = C.ScalarMultiplication(k2, C.PublicKey);
			PointOnCurve P3 = C.ScalarMultiplication(k3, C.PublicKey);
			PointOnCurve P = C.ScalarMultiplication(k1 + k2 + k3, C.PublicKey);
			C.AddTo(ref P1, P2);
			C.AddTo(ref P1, P3);

			Assert.AreEqual(P, P1);

			P2 = PointOnCurve.Zero;
			k1 += k2;
			k1 += k3;

			while (k1-- > 0)
				C.AddTo(ref P2, C.PublicKey);

			Assert.AreEqual(P, P2);
		}

		[TestMethod]
		public void Test_05_Sqrt_1()
		{
			CalcSqrt(83, 673);
		}

		[TestMethod]
		public void Test_06_Sqrt_2()
		{
			MontgomeryCurve C = new Curve25519();
			CalcSqrt(-486664, C.Prime);
		}

		private void CalcSqrt(BigInteger N, BigInteger p)
		{
			BigInteger x = ModulusP.SqrtModP(N, p);
			BigInteger N1 = BigInteger.Remainder(x * x, p);

			Assert.IsTrue(BigInteger.Remainder(N - N1, p).IsZero);
		}

		[TestMethod]
		public void Test_07_Coordinates_25519()
		{
			MontgomeryCurve C = new Curve25519();

			PointOnCurve UV = C.BasePoint;
			PointOnCurve XY = C.ToXY(UV);
			PointOnCurve UV2 = C.ToUV(XY);

			Assert.AreEqual(UV.X, UV2.X);
			Assert.AreEqual(UV.Y, UV2.Y);
		}

		[TestMethod]
		public void Test_08_Coordinates_448()
		{
			MontgomeryCurve C = new Curve448();

			PointOnCurve UV = C.BasePoint;
			PointOnCurve XY = C.ToXY(UV);
			PointOnCurve UV2 = C.ToUV(XY);

			Assert.AreEqual(UV.X, UV2.X);
			Assert.AreEqual(UV.Y, UV2.Y);
		}

		[TestMethod]
		public void Test_09_CalcBits()
		{
			Assert.AreEqual(0, ModulusP.CalcBits(0));
			Assert.AreEqual(1, ModulusP.CalcBits(1));
			Assert.AreEqual(2, ModulusP.CalcBits(2));
			Assert.AreEqual(2, ModulusP.CalcBits(3));
			Assert.AreEqual(3, ModulusP.CalcBits(4));
			Assert.AreEqual(3, ModulusP.CalcBits(5));
			Assert.AreEqual(3, ModulusP.CalcBits(6));
			Assert.AreEqual(3, ModulusP.CalcBits(7));
			Assert.AreEqual(8, ModulusP.CalcBits(255));
			Assert.AreEqual(9, ModulusP.CalcBits(256));
			Assert.AreEqual(16, ModulusP.CalcBits(65535));
			Assert.AreEqual(17, ModulusP.CalcBits(65536));
		}

		[TestMethod]
		public void Test_10_X25519_TestVector_1()
		{
			byte[] A = Hashes.StringToBinary("a546e36bf0527c9d3b16154b82465edd62144c0ac1fc5a18506a2244ba449ac4");
			A[0] &= 248;
			A[31] &= 127;
			A[31] |= 64;
			BigInteger N0 = new BigInteger(A);
			BigInteger N = BigInteger.Parse("31029842492115040904895560451863089656472772604678260265531221036453811406496");
			Assert.AreEqual(N, N0);

			A = Hashes.StringToBinary("e6db6867583030db3594c1a424b15f7c726624ec26b3353b10a903a6d0ab1c4c");
			A[31] &= 127;
			BigInteger U0 = new BigInteger(A);
			BigInteger U = BigInteger.Parse("34426434033919594451155107781188821651316167215306631574996226621102155684838");
			Assert.AreEqual(U, U0);

			A = Hashes.StringToBinary("c3da55379de9c6908e94ea4df28d084f32eccf03491c71f754b4075577a28552");
			BigInteger NU0 = new BigInteger(A);
			MontgomeryCurve C = new Curve25519();
			BigInteger NU = C.ScalarMultiplication(N, U);

			Assert.AreEqual(NU0, NU);
		}

		[TestMethod]
		public void Test_11_X25519_TestVector_2()
		{
			byte[] A = Hashes.StringToBinary("4b66e9d4d1b4673c5ad22691957d6af5c11b6421e0ea01d42ca4169e7918ba0d");
			A[0] &= 248;
			A[31] &= 127;
			A[31] |= 64;
			BigInteger N0 = new BigInteger(A);
			BigInteger N = BigInteger.Parse("35156891815674817266734212754503633747128614016119564763269015315466259359304");
			Assert.AreEqual(N, N0);

			A = Hashes.StringToBinary("e5210f12786811d3f4b7959d0538ae2c31dbe7106fc03c3efc4cd549c715a493");
			A[31] &= 127;
			BigInteger U0 = new BigInteger(A);
			BigInteger U = BigInteger.Parse("8883857351183929894090759386610649319417338800022198945255395922347792736741");
			Assert.AreEqual(U, U0);

			A = Hashes.StringToBinary("95cbde9476e8907d7aade45cb4b873f88b595a68799fa152e6f8f7647aac7957");
			BigInteger NU0 = new BigInteger(A);
			MontgomeryCurve C = new Curve25519();
			BigInteger NU = C.ScalarMultiplication(N, U);

			Assert.AreEqual(NU0, NU);
		}

		[TestMethod]
		public void Test_12_X25519_TestVector_3_1()
		{
			this.X25519_TestVector_3(1, "422c8e7a6227d7bca1350b3e2bb7279f7897b87bb6854b783c60e80311ae3079");
		}

		[TestMethod]
		public void Test_13_X25519_TestVector_3_1000()
		{
			this.X25519_TestVector_3(1000, "684cf59ba83309552800ef566f2f4d3c1c3887c49360e3875f2eb94d99532c51");
		}

		[TestMethod]
		[Ignore]
		public void Test_14_X25519_TestVector_3_1000000()
		{
			this.X25519_TestVector_3(1000000, "7c3911e0ab2586fd864497297e575e6f3bc601c0883c30df5f4dd2d24f665424");
		}

		private void X25519_TestVector_3(int i, string HexResult)
		{
			BigInteger N = 9;
			BigInteger U = 9;

			byte[] A = Hashes.StringToBinary(HexResult);
			BigInteger NU0 = new BigInteger(A);
			MontgomeryCurve C = new Curve25519();
			BigInteger NU = C.ScalarMultiplication(N, U);

			while (--i > 0)
			{
				U = N;
				N = NU;

				NU = C.ScalarMultiplication(N, U);
			}

			Assert.AreEqual(NU0, NU);
		}

		[TestMethod]
		public void Test_15_X25519_ECDH()
		{
			byte[] A = Hashes.StringToBinary("77076d0a7318a57d3c16c17251b26645df4c2f87ebc0992ab177fba51db92c2a");
			A[0] &= 248;
			A[31] &= 127;
			A[31] |= 64;
			BigInteger PrivateKey = new BigInteger(A);
			Curve25519 Alice = new Curve25519(PrivateKey);

			A = Hashes.StringToBinary("8520f0098930a754748b7ddcb43ef75a0dbf3a0d26381af4eba4a98eaa9b4e6a");
			BigInteger PublicKey = new BigInteger(A);

			Assert.AreEqual(PublicKey, Alice.PublicKey.X);

			A = Hashes.StringToBinary("5dab087e624a8a4b79e17f8b83800ee66f3bb1292618b6fd1c2f8b27ff88e0eb");
			A[0] &= 248;
			A[31] &= 127;
			A[31] |= 64;
			PrivateKey = new BigInteger(A);
			Curve25519 Bob = new Curve25519(PrivateKey);

			A = Hashes.StringToBinary("de9edb7d7b7dc1b4d35b61c2ece435373f8343c85b78674dadfc7e146f882b4f");
			PublicKey = new BigInteger(A);

			Assert.AreEqual(PublicKey, Bob.PublicKey.X);

			byte[] Key1 = Alice.GetSharedKey(Bob.PublicKey, HashFunction.SHA256);
			byte[] Key2 = Bob.GetSharedKey(Alice.PublicKey, HashFunction.SHA256);
			int i, c = Key1.Length;

			Assert.AreEqual(c, Key2.Length);

			A = Hashes.StringToBinary("4a5d9d5ba4ce2de1728e3bf480350f25e07e21c947d19e3376f09b3c1e161742");
			if (A.Length != 33)
				Array.Resize<byte>(ref A, 33);

			Array.Reverse(A);   // Most significant byte first.

			A = Hashes.ComputeHash(HashFunction.SHA256, A);

			for (i = 0; i < c; i++)
			{
				Assert.AreEqual(Key1[i], Key2[i]);
				Assert.AreEqual(A[i], Key1[i]);
			}
		}

		[TestMethod]
		public void Test_16_X448_TestVector_1()
		{
			byte[] A = Hashes.StringToBinary("3d262fddf9ec8e88495266fea19a34d28882acef045104d0d1aae121700a779c984c24f8cdd78fbff44943eba368f54b29259a4f1c600ad3");
			A[0] &= 252;
			A[55] |= 128;
			Array.Resize(ref A, 57);
			BigInteger N0 = new BigInteger(A);
			BigInteger N = BigInteger.Parse("599189175373896402783756016145213256157230856085026129926891459468622403380588640249457727683869421921443004045221642549886377526240828");
			Assert.AreEqual(N, N0);

			A = Hashes.StringToBinary("06fce640fa3487bfda5f6cf2d5263f8aad88334cbd07437f020f08f9814dc031ddbdc38c19c6da2583fa5429db94ada18aa7a7fb4ef8a086");
			Array.Resize(ref A, 57);
			BigInteger U0 = new BigInteger(A);
			BigInteger U = BigInteger.Parse("382239910814107330116229961234899377031416365240571325148346555922438025162094455820962429142971339584360034337310079791515452463053830");
			Assert.AreEqual(U, U0);

			A = Hashes.StringToBinary("ce3e4ff95a60dc6697da1db1d85e6afbdf79b50a2412d7546d5f239fe14fbaadeb445fc66a01b0779d98223961111e21766282f73dd96b6f");
			Array.Resize(ref A, 57);
			BigInteger NU0 = new BigInteger(A);
			MontgomeryCurve C = new Curve448();
			BigInteger NU = C.ScalarMultiplication(N, U);

			Assert.AreEqual(NU0, NU);
		}

		[TestMethod]
		public void Test_17_X448_TestVector_2()
		{
			byte[] A = Hashes.StringToBinary("203d494428b8399352665ddca42f9de8fef600908e0d461cb021f8c538345dd77c3e4806e25f46d3315c44e0a5b4371282dd2c8d5be3095f");
			A[0] &= 252;
			A[55] |= 128;
			Array.Resize(ref A, 57);
			BigInteger N0 = new BigInteger(A);
			BigInteger N = BigInteger.Parse("633254335906970592779259481534862372382525155252028961056404001332122152890562527156973881968934311400345568203929409663925541994577184");
			Assert.AreEqual(N, N0);

			A = Hashes.StringToBinary("0fbcc2f993cd56d3305b0b7d9e55d4c1a8fb5dbb52f8e9a1e9b6201b165d015894e56c4d3570bee52fe205e28a78b91cdfbde71ce8d157db");
			Array.Resize(ref A, 57);
			BigInteger U0 = new BigInteger(A);
			BigInteger U = BigInteger.Parse("622761797758325444462922068431234180649590390024811299761625153767228042600197997696167956134770744996690267634159427999832340166786063");
			Assert.AreEqual(U, U0);

			A = Hashes.StringToBinary("884a02576239ff7a2f2f63b2db6a9ff37047ac13568e1e30fe63c4a7ad1b3ee3a5700df34321d62077e63633c575c1c954514e99da7c179d");
			Array.Resize(ref A, 57);
			BigInteger NU0 = new BigInteger(A);
			MontgomeryCurve C = new Curve448();
			BigInteger NU = C.ScalarMultiplication(N, U);

			Assert.AreEqual(NU0, NU);
		}

		[TestMethod]
		public void Test_18_X448_TestVector_3_1()
		{
			this.X448_TestVector_3(1, "3f482c8a9f19b01e6c46ee9711d9dc14fd4bf67af30765c2ae2b846a4d23a8cd0db897086239492caf350b51f833868b9bc2b3bca9cf4113");
		}

		[TestMethod]
		public void Test_19_X448_TestVector_3_1000()
		{
			this.X448_TestVector_3(1000, "aa3b4749d55b9daf1e5b00288826c467274ce3ebbdd5c17b975e09d4af6c67cf10d087202db88286e2b79fceea3ec353ef54faa26e219f38");
		}

		[TestMethod]
		[Ignore]
		public void Test_20_X448_TestVector_3_1000000()
		{
			this.X448_TestVector_3(1000000, "077f453681caca3693198420bbe515cae0002472519b3e67661a7e89cab94695c8f4bcd66e61b9b9c946da8d524de3d69bd9d9d66b997e37");
		}

		private void X448_TestVector_3(int i, string HexResult)
		{
			BigInteger N = 5;
			BigInteger U = 5;

			byte[] A = Hashes.StringToBinary(HexResult);
			BigInteger NU0 = new BigInteger(A);
			MontgomeryCurve C = new Curve448();
			BigInteger NU = C.ScalarMultiplication(N, U);

			while (--i > 0)
			{
				U = N;
				N = NU;

				NU = C.ScalarMultiplication(N, U);
			}

			Assert.AreEqual(NU0, NU);
		}

		[TestMethod]
		public void Test_21_X448_ECDH()
		{
			byte[] A = Hashes.StringToBinary("9a8f4925d1519f5775cf46b04b5800d4ee9ee8bae8bc5565d498c28dd9c9baf574a9419744897391006382a6f127ab1d9ac2d8c0a598726b");
			A[0] &= 252;
			A[55] |= 128;
			BigInteger PrivateKey = new BigInteger(A);
			Curve448 Alice = new Curve448(PrivateKey);

			A = Hashes.StringToBinary("9b08f7cc31b7e3e67d22d5aea121074a273bd2b83de09c63faa73d2c22c5d9bbc836647241d953d40c5b12da88120d53177f80e532c41fa000");
			BigInteger PublicKey = new BigInteger(A);

			Assert.AreEqual(PublicKey, Alice.PublicKey.X);

			A = Hashes.StringToBinary("1c306a7ac2a0e2e0990b294470cba339e6453772b075811d8fad0d1d6927c120bb5ee8972b0d3e21374c9c921b09d1b0366f10b65173992d");
			A[0] &= 252;
			A[55] |= 128;
			PrivateKey = new BigInteger(A);
			Curve448 Bob = new Curve448(PrivateKey);

			A = Hashes.StringToBinary("3eb7a829b0cd20f5bcfc0b599b6feccf6da4627107bdb0d4f345b43027d8b972fc3e34fb4232a13ca706dcb57aec3dae07bdc1c67bf33609");
			PublicKey = new BigInteger(A);

			Assert.AreEqual(PublicKey, Bob.PublicKey.X);

			byte[] Key1 = Alice.GetSharedKey(Bob.PublicKey, HashFunction.SHA256);
			byte[] Key2 = Bob.GetSharedKey(Alice.PublicKey, HashFunction.SHA256);
			int i, c = Key1.Length;

			Assert.AreEqual(c, Key2.Length);

			A = Hashes.StringToBinary(" 07fff4181ac6cc95ec1c16a94a0f74d12da232ce40a77552281d282bb60c0b56fd2464c335543936521c24403085d59a449a5037514a879d");
			if (A.Length != 57)
				Array.Resize<byte>(ref A, 57);

			Array.Reverse(A);   // Most significant byte first.

			A = Hashes.ComputeHash(HashFunction.SHA256, A);

			for (i = 0; i < c; i++)
			{
				Assert.AreEqual(Key1[i], Key2[i]);
				Assert.AreEqual(A[i], Key1[i]);
			}
		}

	}
}
