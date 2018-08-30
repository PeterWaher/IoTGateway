using System;
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
		
	}
}
