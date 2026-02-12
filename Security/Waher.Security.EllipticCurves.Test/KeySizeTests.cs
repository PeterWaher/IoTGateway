using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Waher.Security.EllipticCurves.Test
{
	[TestClass]
	public class KeySizeTests
	{
		[TestMethod]
		public void Test_01_BrainpoolP160()
		{
			Test(new BrainpoolP160());
		}

		[TestMethod]
		public void Test_02_BrainpoolP192()
		{
			Test(new BrainpoolP192());
		}

		[TestMethod]
		public void Test_03_BrainpoolP224()
		{
			Test(new BrainpoolP224());
		}

		[TestMethod]
		public void Test_04_BrainpoolP256()
		{
			Test(new BrainpoolP256());
		}

		[TestMethod]
		public void Test_05_BrainpoolP320()
		{
			Test(new BrainpoolP320());
		}

		[TestMethod]
		public void Test_06_BrainpoolP384()
		{
			Test(new BrainpoolP384());
		}

		[TestMethod]
		public void Test_07_BrainpoolP512()
		{
			Test(new BrainpoolP512());
		}

		[TestMethod]
		public void Test_08_NistP192()
		{
			Test(new NistP192());
		}

		[TestMethod]
		public void Test_09_NistP224()
		{
			Test(new NistP224());
		}

		[TestMethod]
		public void Test_10_NistP256()
		{
			Test(new NistP256());
		}

		[TestMethod]
		public void Test_11_NistP384()
		{
			Test(new NistP384());
		}

		[TestMethod]
		public void Test_12_NistP521()
		{
			Test(new NistP521());
		}

		[TestMethod]
		public void Test_13_Curve25519()
		{
			Test(new Curve25519());
		}

		[TestMethod]
		public void Test_14_Curve448()
		{
			Test(new Curve448());
		}

		[TestMethod]
		public void Test_15_Edwards25519()
		{
			Test(new Edwards25519());
		}

		[TestMethod]
		public void Test_16_Edwards448()
		{
			Test(new Edwards448());
		}

		private static void Test(EllipticCurve Curve)
		{
			Console.Out.WriteLine(Curve.CurveName);
			Console.Out.WriteLine("================================");
			Console.Out.WriteLine("Order: " + Curve.Order.ToString());
			Console.Out.WriteLine("Order Bits: " + Curve.OrderBits.ToString());
			Console.Out.WriteLine("Order Bytes: " + Curve.OrderBytes.ToString());
			Console.Out.WriteLine("Public Key: " + Hashes.BinaryToString(Curve.PublicKey));

			Assert.AreEqual((Curve.OrderBits + 7) / 8, Curve.OrderBytes, "Public Key byte size incorrect.");
		}
	}
}
