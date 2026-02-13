using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace Waher.Security.EllipticCurves.Test
{
	[TestClass]
	public class EcdhTests
	{
		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_01_NIST_P192(bool BigEndian)
		{
			Test_ECDH(new NistP192(), new NistP192(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_02_NIST_P224(bool BigEndian)
		{
			Test_ECDH(new NistP224(), new NistP224(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_03_NIST_P256(bool BigEndian)
		{
			Test_ECDH(new NistP256(), new NistP256(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_04_NIST_P384(bool BigEndian)
		{
			Test_ECDH(new NistP384(), new NistP384(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_05_NIST_P521(bool BigEndian)
		{
			Test_ECDH(new NistP521(), new NistP521(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_06_Curve25519(bool BigEndian)
		{
			Test_ECDH(new Curve25519(), new Curve25519(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_07_Curve448(bool BigEndian)
		{
			Test_ECDH(new Curve448(), new Curve448(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_08_Edwards25519(bool BigEndian)
		{
			Test_ECDH(new Edwards25519(), new Edwards25519(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_09_Edwards448(bool BigEndian)
		{
			Test_ECDH(new Edwards448(), new Edwards448(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_10_Brainpool_P160(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP160(), new BrainpoolP160(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_11_Brainpool_P192(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP192(), new BrainpoolP192(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_12_Brainpool_P224(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP224(), new BrainpoolP224(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_13_Brainpool_P256(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP256(), new BrainpoolP256(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_14_Brainpool_P320(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP320(), new BrainpoolP320(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_15_Brainpool_P384(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP384(), new BrainpoolP384(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_16_Brainpool_P512(bool BigEndian)
		{
			Test_ECDH(new BrainpoolP512(), new BrainpoolP512(), BigEndian);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_17_Brainpool_P224_RFC6932(bool BigEndian)
		{
			BrainpoolP224 A = new(
			[
				0x7C4B7A2C, 0x8A4BAD1F, 0xBB7D79CC, 0x0955DB7C, 0x6A4660CA, 0x64CC4778,
				0x159B495E
			]);
			BrainpoolP224 B = new(
			[
				0x63976D4A, 0xAE6CD0F6, 0xDD18DEFE, 0xF55D9656, 0x9D0507C0, 0x3E74D648,
				0x6FFA28FB
			]);

			PointOnCurve PubA = A.PublicKeyPoint;
			PointOnCurve PubB = B.PublicKeyPoint;

			BigInteger ExpectedPubAX = PrimeFieldCurve.ToBigInteger(
			[
				0xB104A67A, 0x6F6E85E1, 0x4EC1825E, 0x1539E8EC, 0xDBBF5849, 0x22367DD8,
				0x8C6BDCF2
			]);
			BigInteger ExpectedPubAY = PrimeFieldCurve.ToBigInteger(
			[
				0x46D782E7, 0xFDB5F60C, 0xD8404301, 0xAC5949C5, 0x8EDB26BC, 0x68BA0769,
				0x5B750A94
			]);
			BigInteger ExpectedPubBX = PrimeFieldCurve.ToBigInteger(
			[
				0x2A97089A, 0x9296147B, 0x71B21A4B, 0x574E1278, 0x245B536F, 0x14D8C2B9,
				0xD07A874E
			]);
			BigInteger ExpectedPubBY = PrimeFieldCurve.ToBigInteger(
			[
				0x9B900D7C, 0x77A709A7, 0x97276B8C, 0xA1BA61BB, 0x95B546FC, 0x29F862E4,
				0x4D59D25B
			]);

			Assert.AreEqual(ExpectedPubAX, PubA.X);
			Assert.AreEqual(ExpectedPubAY, PubA.Y);
			Assert.AreEqual(ExpectedPubBX, PubB.X);
			Assert.AreEqual(ExpectedPubBY, PubB.Y);

			PointOnCurve SharedA;
			PointOnCurve SharedB;

			if (BigEndian)
			{
				SharedA = A.GetSharedPoint(B.PublicKeyBigEndian, true);
				SharedB = B.GetSharedPoint(A.PublicKeyBigEndian, true);
			}
			else
			{
				SharedA = A.GetSharedPoint(B.PublicKey, false);
				SharedB = B.GetSharedPoint(A.PublicKey, false);
			}

			Assert.AreEqual(SharedA.X, SharedB.X);
			Assert.AreEqual(SharedA.Y, SharedB.Y);

			BigInteger ExpectedX = PrimeFieldCurve.ToBigInteger(
			[
				0x312DFD98, 0x783F9FB7, 0x7B970494, 0x5A73BEB6, 0xDCCBE3B6, 0x5D0F967D,
				0xCAB574EB
			]);
			BigInteger ExpectedY = PrimeFieldCurve.ToBigInteger(
			[
				0x6F800811, 0xD64114B1, 0xC48C621A, 0xB3357CF9, 0x3F496E42, 0x38696A2A,
				0x012B3C98
			]);

			Assert.AreEqual(ExpectedX, SharedB.X);
			Assert.AreEqual(ExpectedY, SharedB.Y);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_18_Brainpool_P256_RFC6932(bool BigEndian)
		{
			BrainpoolP256 A = new(
			[
				0x041EB8B1, 0xE2BC681B, 0xCE8E3996, 0x3B2E9FC4, 0x15B05283, 0x313DD1A8,
				0xBCC055F1, 0x1AE49699
			]);
			BrainpoolP256 B = new(
			[
				0x06F5240E, 0xACDB9837, 0xBC96D482, 0x74C8AA83, 0x4B6C87BA, 0x9CC3EEDD,
				0x81F99A16, 0xB8D804D3
			]);

			PointOnCurve PubA = A.PublicKeyPoint;
			PointOnCurve PubB = B.PublicKeyPoint;

			BigInteger ExpectedPubAX = PrimeFieldCurve.ToBigInteger(
			[
				0x78028496, 0xB5ECAAB3, 0xC8B6C12E, 0x45DB1E02, 0xC9E4D26B, 0x4113BC4F,
				0x015F60C5, 0xCCC0D206
			]);
			BigInteger ExpectedPubAY = PrimeFieldCurve.ToBigInteger(
			[
				0xA2AE1762, 0xA3831C1D, 0x20F03F8D, 0x1E3C0C39, 0xAFE6F09B, 0x4D44BBE8,
				0x0CD10098, 0x7B05F92B
			]);
			BigInteger ExpectedPubBX = PrimeFieldCurve.ToBigInteger(
			[
				0x8E07E219, 0xBA588916, 0xC5B06AA3, 0x0A2F464C, 0x2F2ACFC1, 0x610A3BE2,
				0xFB240B63, 0x5341F0DB
			]);
			BigInteger ExpectedPubBY = PrimeFieldCurve.ToBigInteger(
			[
				0x148EA1D7, 0xD1E7E54B, 0x9555B6C9, 0xAC90629C, 0x18B63BEE, 0x5D7AA694,
				0x9EBBF47B, 0x24FDE40D
			]);

			Assert.AreEqual(ExpectedPubAX, PubA.X);
			Assert.AreEqual(ExpectedPubAY, PubA.Y);
			Assert.AreEqual(ExpectedPubBX, PubB.X);
			Assert.AreEqual(ExpectedPubBY, PubB.Y);

			PointOnCurve SharedA;
			PointOnCurve SharedB;

			if (BigEndian)
			{
				SharedA = A.GetSharedPoint(B.PublicKeyBigEndian, true);
				SharedB = B.GetSharedPoint(A.PublicKeyBigEndian, true);
			}
			else
			{
				SharedA = A.GetSharedPoint(B.PublicKey, false);
				SharedB = B.GetSharedPoint(A.PublicKey, false);
			}

			Assert.AreEqual(SharedA.X, SharedB.X);
			Assert.AreEqual(SharedA.Y, SharedB.Y);

			BigInteger ExpectedX = PrimeFieldCurve.ToBigInteger(
			[
				0x05E94091, 0x5549E9F6, 0xA4A75693, 0x716E3746, 0x6ABA79B4, 0xBF291987,
				0x7A16DD2C, 0xC2E23708
			]);
			BigInteger ExpectedY = PrimeFieldCurve.ToBigInteger(
			[
				0x6BC23B67, 0x02BC5A01, 0x9438CEEA, 0x107DAAD8, 0xB94232FF, 0xBBC350F3,
				0xB137628F, 0xE6FD134C
			]);

			Assert.AreEqual(ExpectedX, SharedB.X);
			Assert.AreEqual(ExpectedY, SharedB.Y);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_19_Brainpool_P384_RFC6932(bool BigEndian)
		{
			BrainpoolP384 A = new(
			[
				0x014EC075, 0x5B78594B, 0xA47FB0A5, 0x6F617304, 0x5B4331E7, 0x4BA1A6F4,
				0x7322E70D, 0x79D828D9, 0x7E095884, 0xCA72B73F, 0xDABD5910, 0xDF0FA76A
			]);
			BrainpoolP384 B = new(
			[
				0x6B461CB7, 0x9BD0EA51, 0x9A87D682, 0x8815D8CE, 0x7CD9B3CA, 0xA0B5A826,
				0x2CBCD550, 0xA015C900, 0x95B976F3, 0x52995750, 0x6E1224A8, 0x61711D54
			]);

			PointOnCurve PubA = A.PublicKeyPoint;
			PointOnCurve PubB = B.PublicKeyPoint;

			BigInteger ExpectedPubAX = PrimeFieldCurve.ToBigInteger(
			[
				0x45CB26E4, 0x384DAF6F, 0xB7768853, 0x07B9A38B, 0x7AD1B5C6, 0x92E0C32F,
				0x01253327, 0x78F3B8D3, 0xF50CA358, 0x099B30DE, 0xB5EE69A9, 0x5C058B4E
			]);
			BigInteger ExpectedPubAY = PrimeFieldCurve.ToBigInteger(
			[
				0x8173A1C5, 0x4AFFA7E7, 0x81D0E1E1, 0xD12C0DC2, 0xB74F4DF5, 0x8E4A4E3A,
				0xF7026C5D, 0x32DC530A, 0x2CD89C85, 0x9BB4B4B7, 0x68497F49, 0xAB8CC859
			]);
			BigInteger ExpectedPubBX = PrimeFieldCurve.ToBigInteger(
			[
				0x01BF92A9, 0x2EE4BE8D, 0xED1A9111, 0x25C209B0, 0x3F99E316, 0x1CFCC986,
				0xDC771138, 0x3FC30AF9, 0xCE28CA33, 0x86D59E2C, 0x8D72CE1E, 0x7B4666E8
			]);
			BigInteger ExpectedPubBY = PrimeFieldCurve.ToBigInteger(
			[
				0x3289C4A3, 0xA4FEE035, 0xE39BDB88, 0x5D509D22, 0x4A142FF9, 0xFBCC5CFE,
				0x5CCBB302, 0x68EE4748, 0x7ED80448, 0x58D31D84, 0x8F7A95C6, 0x35A347AC
			]);

			Assert.AreEqual(ExpectedPubAX, PubA.X);
			Assert.AreEqual(ExpectedPubAY, PubA.Y);
			Assert.AreEqual(ExpectedPubBX, PubB.X);
			Assert.AreEqual(ExpectedPubBY, PubB.Y);

			PointOnCurve SharedA;
			PointOnCurve SharedB;

			if (BigEndian)
			{
				SharedA = A.GetSharedPoint(B.PublicKeyBigEndian, true);
				SharedB = B.GetSharedPoint(A.PublicKeyBigEndian, true);
			}
			else
			{
				SharedA = A.GetSharedPoint(B.PublicKey, false);
				SharedB = B.GetSharedPoint(A.PublicKey, false);
			}

			Assert.AreEqual(SharedA.X, SharedB.X);
			Assert.AreEqual(SharedA.Y, SharedB.Y);

			BigInteger ExpectedX = PrimeFieldCurve.ToBigInteger(
			[
				0x04CC4FF3, 0xDCCCB07A, 0xF24E0ACC, 0x529955B3, 0x6D7C8077, 0x72B92FCB,
				0xE48F3AFE, 0x9A2F370A, 0x1F98D3FA, 0x73FD0C07, 0x47C632E1, 0x2F1423EC
			]);
			BigInteger ExpectedY = PrimeFieldCurve.ToBigInteger(
			[
				0x7F465F90, 0xBD69AFB8, 0xF828A214, 0xEB9716D6, 0x6ABC59F1, 0x7AF7C75E,
				0xE7F1DE22, 0xAB5D0508, 0x5F5A01A9, 0x382D05BF, 0x72D96698, 0xFE3FF64E
			]);

			Assert.AreEqual(ExpectedX, SharedB.X);
			Assert.AreEqual(ExpectedY, SharedB.Y);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void Test_20_Brainpool_P512_RFC6932(bool BigEndian)
		{
			BrainpoolP512 A = new(
			[
				0x636B6BE0, 0x482A6C1C, 0x41AA7AE7, 0xB245E983, 0x392DB94C, 0xECEA2660,
				0xA379CFE1, 0x59559E35, 0x75818253, 0x91175FC1, 0x95D28BAC, 0x0CF03A78,
				0x41A383B9, 0x5C262B98, 0x3782874C, 0xCE6FE333
			]);
			BrainpoolP512 B = new(
			[
				0x0AF4E7F6, 0xD52EDD52, 0x907BB8DB, 0xAB3992A0, 0xBB696EC1, 0x0DF11892,
				0xFF205B66, 0xD381ECE7, 0x2314E6A6, 0xEA079CEA, 0x06961DBA, 0x5AE6422E,
				0xF2E9EE80, 0x3A1F236F, 0xB96A1799, 0xB86E5C8B
			]);

			PointOnCurve PubA = A.PublicKeyPoint;
			PointOnCurve PubB = B.PublicKeyPoint;

			BigInteger ExpectedPubAX = PrimeFieldCurve.ToBigInteger(
			[
				0x0562E68B, 0x9AF7CBFD, 0x5565C6B1, 0x6883B777, 0xFF11C199, 0x161ECC42,
				0x7A39D17E, 0xC2166499, 0x389571D6, 0xA994977C, 0x56AD8252, 0x658BA8A1,
				0xB72AE42F, 0x4FB75321, 0x51AFC3EF, 0x0971CCDA
			]);
			BigInteger ExpectedPubAY = PrimeFieldCurve.ToBigInteger(
			[
				0xA7CA2D81, 0x91E21776, 0xA89860AF, 0xBC1F582F, 0xAA308D55, 0x1C1DC613,
				0x3AF9F9C3, 0xCAD59998, 0xD7007954, 0x8140B90B, 0x1F311AFB, 0x378AA81F,
				0x51B275B2, 0xBE6B7DEE, 0x978EFC73, 0x43EA642E
			]);
			BigInteger ExpectedPubBX = PrimeFieldCurve.ToBigInteger(
			[
				0x5A7954E3, 0x2663DFF1, 0x1AE24712, 0xD87419F2, 0x6B708AC2, 0xB92877D6,
				0xBFEE2BFC, 0x43714D89, 0xBBDB6D24, 0xD807BBD3, 0xAEB7F0C3, 0x25F862E8,
				0xBADE4F74, 0x636B97EA, 0xACE739E1, 0x1720D323
			]);
			BigInteger ExpectedPubBY = PrimeFieldCurve.ToBigInteger(
			[
				0x96D14621, 0xA9283A1B, 0xED84DE8D, 0xD64836B2, 0xC0758B11, 0x441179DC,
				0x0C54C0D4, 0x9A47C038, 0x07D171DD, 0x544B72CA, 0xAEF7B7CE, 0x01C7753E,
				0x2CAD1A86, 0x1ECA55A7, 0x1954EE1B, 0xA35E04BE
			]);

			Assert.AreEqual(ExpectedPubAX, PubA.X);
			Assert.AreEqual(ExpectedPubAY, PubA.Y);
			Assert.AreEqual(ExpectedPubBX, PubB.X);
			Assert.AreEqual(ExpectedPubBY, PubB.Y);

			PointOnCurve SharedA;
			PointOnCurve SharedB;

			if (BigEndian)
			{
				SharedA = A.GetSharedPoint(B.PublicKeyBigEndian, true);
				SharedB = B.GetSharedPoint(A.PublicKeyBigEndian, true);
			}
			else
			{
				SharedA = A.GetSharedPoint(B.PublicKey, false);
				SharedB = B.GetSharedPoint(A.PublicKey, false);
			}

			Assert.AreEqual(SharedA.X, SharedB.X);
			Assert.AreEqual(SharedA.Y, SharedB.Y);

			BigInteger ExpectedX = PrimeFieldCurve.ToBigInteger(
			[
				0x1EE8321A, 0x4BBF93B9, 0xCF8921AB, 0x209850EC, 0x9B7066D1, 0x984EF08C,
				0x2BB72323, 0x6208AC8F, 0x1A483E79, 0x461A00E0, 0xD5F6921C, 0xE9D36050,
				0x2F85C812, 0xBEDEE23A, 0xC5B210E5, 0x811B191E
			]);
			BigInteger ExpectedY = PrimeFieldCurve.ToBigInteger(
			[
				0x2632095B, 0x7B936174, 0xB41FD2FA, 0xF369B1D1, 0x8DCADEED, 0x7E410A7E,
				0x251F0831, 0x097C50D0, 0x2CFED026, 0x07B6A2D5, 0xADB4C000, 0x60085622,
				0x08631875, 0xB58B54EC, 0xDA5A4F9F, 0xE9EAABA6
			]);

			Assert.AreEqual(ExpectedX, SharedB.X);
			Assert.AreEqual(ExpectedY, SharedB.Y);
		}

		public static void Test_ECDH(PrimeFieldCurve Curve1, PrimeFieldCurve Curve2, bool BigEndian)
		{
			int n;

			for (n = 0; n < 100; n++)
			{
				byte[] Key1 = Curve1.GetSharedKey(
					BigEndian ? Curve2.PublicKeyBigEndian : Curve2.PublicKey,
					BigEndian, Hashes.GetHashFunctionArray(Curve2.HashFunction));

				byte[] Key2 = Curve2.GetSharedKey(
					 BigEndian ? Curve1.PublicKeyBigEndian : Curve1.PublicKey,
					 BigEndian, Hashes.GetHashFunctionArray(Curve1.HashFunction));

				Assert.AreEqual(Hashes.BinaryToString(Key1), Hashes.BinaryToString(Key2));

				Curve1.GenerateKeys();
				Curve2.GenerateKeys();
			}
		}
	}
}
