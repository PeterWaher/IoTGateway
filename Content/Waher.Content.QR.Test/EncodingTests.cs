using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.QR.Encoding;

namespace Waher.Content.QR.Test
{
	[TestClass]
	public class EncodingTests
	{
		// Thanks to https://www.thonky.com/qr-code-tutorial/introduction
		// for tutorial and test vectors.

		private QrEncoder encoder;

		[TestInitialize]
		public void TestInitialize()
		{
			this.encoder = new QrEncoder();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.encoder?.Dispose();
			this.encoder = null;
		}

		[TestMethod]
		public void Test_01_GF256_Pow2()
		{
			byte[] Expected = new byte[]
			{
				1,2,4,8,16,32,64,128,29,58,116,232,205,135,19,38,76,152,45,90,
				180,117,234,201,143,3,6,12,24,48,96,192,157,39,78,156,37,74,
				148,53,106,212,181,119,238,193,159,35,70,140,5,10,20,40,80,160,
				93,186,105,210,185,111,222,161,95,190,97,194,153,47,94,188,101,
				202,137,15,30,60,120,240,253,231,211,187,107,214,177,127,254,
				225,223,163,91,182,113,226,217,175,67,134,17,34,68,136,13,26,
				52,104,208,189,103,206,129,31,62,124,248,237,199,147,59,118,
				236,197,151,51,102,204,133,23,46,92,184,109,218,169,79,158,33,
				66,132,21,42,84,168,77,154,41,82,164,85,170,73,146,57,114,228,
				213,183,115,230,209,191,99,198,145,63,126,252,229,215,179,123,
				246,241,255,227,219,171,75,150,49,98,196,149,55,110,220,165,87,
				174,65,130,25,50,100,200,141,7,14,28,56,112,224,221,167,83,166,
				81,162,89,178,121,242,249,239,195,155,43,86,172,69,138,9,18,36,
				72,144,61,122,244,245,247,243,251,235,203,139,11,22,44,88,176,
				125,250,233,207,131,27,54,108,216,173,71,142,1
			};

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(GF256.PowerOf2Table));
		}

		[TestMethod]
		public void Test_02_GF256_Log2()
		{
			byte[] Expected = new byte[]
			{
				0,0,1,25,2,50,26,198,3,223,51,238,27,104,199,75,
				4,100,224,14,52,141,239,129,28,193,105,248,200,8,76,113,
				5,138,101,47,225,36,15,33,53,147,142,218,240,18,130,69,
				29,181,194,125,106,39,249,185,201,154,9,120,77,228,114,166,
				6,191,139,98,102,221,48,253,226,152,37,179,16,145,34,136,
				54,208,148,206,143,150,219,189,241,210,19,92,131,56,70,64,
				30,66,182,163,195,72,126,110,107,58,40,84,250,133,186,61,
				202,94,155,159,10,21,121,43,78,212,229,172,115,243,167,87,
				7,112,192,247,140,128,99,13,103,74,222,237,49,197,254,24,
				227,165,153,119,38,184,180,124,17,68,146,217,35,32,137,46,
				55,63,209,91,149,188,207,205,144,135,151,178,220,252,190,97,
				242,86,211,171,20,42,93,158,132,60,57,83,71,109,65,162,
				31,45,67,216,183,123,164,118,196,23,73,236,127,12,111,246,
				108,161,59,82,41,157,85,170,251,96,134,177,187,204,62,90,
				203,89,95,176,156,169,160,81,11,245,22,235,122,117,44,215,
				79,174,213,233,230,231,173,232,116,214,244,234,168,80,88,175
			};

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(GF256.Log2Table));
		}

		[TestMethod]
		public void Test_03_GF256Px_1()
		{
			byte[] Expected = new byte[] { 1, 1 };
			ReedSolomonEC EC = new ReedSolomonEC(1);

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(EC.GeneratorPolynomial.Coefficients));
		}

		[TestMethod]
		public void Test_04_GF256Px_2()
		{
			byte[] Expected = new byte[] { 1, 3, 2 };
			ReedSolomonEC EC = new ReedSolomonEC(2);

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(EC.GeneratorPolynomial.Coefficients));
		}

		[TestMethod]
		public void Test_05_GF256Px_3()
		{
			byte[] Expected = new byte[] { 1, 7, 14, 8 };
			ReedSolomonEC EC = new ReedSolomonEC(3);

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(EC.GeneratorPolynomial.Coefficients));
		}

		[TestMethod]
		public void Test_06_GF256Px_7()
		{
			byte[] Expected = new byte[]
			{
				GF256.PowerOf2Table[0],
				GF256.PowerOf2Table[87],
				GF256.PowerOf2Table[229],
				GF256.PowerOf2Table[146],
				GF256.PowerOf2Table[149],
				GF256.PowerOf2Table[238],
				GF256.PowerOf2Table[102],
				GF256.PowerOf2Table[21]
			};
			ReedSolomonEC EC = new ReedSolomonEC(7);

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(EC.GeneratorPolynomial.Coefficients));
		}

		[TestMethod]
		public void Test_07_GF256Px_30()
		{
			byte[] Expected = new byte[]
			{
				GF256.PowerOf2Table[0],
				GF256.PowerOf2Table[41],
				GF256.PowerOf2Table[173],
				GF256.PowerOf2Table[145],
				GF256.PowerOf2Table[152],
				GF256.PowerOf2Table[216],
				GF256.PowerOf2Table[31],
				GF256.PowerOf2Table[179],
				GF256.PowerOf2Table[182],
				GF256.PowerOf2Table[50],
				GF256.PowerOf2Table[48],
				GF256.PowerOf2Table[110],
				GF256.PowerOf2Table[86],
				GF256.PowerOf2Table[239],
				GF256.PowerOf2Table[96],
				GF256.PowerOf2Table[222],
				GF256.PowerOf2Table[125],
				GF256.PowerOf2Table[42],
				GF256.PowerOf2Table[173],
				GF256.PowerOf2Table[226],
				GF256.PowerOf2Table[193],
				GF256.PowerOf2Table[224],
				GF256.PowerOf2Table[130],
				GF256.PowerOf2Table[156],
				GF256.PowerOf2Table[37],
				GF256.PowerOf2Table[251],
				GF256.PowerOf2Table[216],
				GF256.PowerOf2Table[238],
				GF256.PowerOf2Table[40],
				GF256.PowerOf2Table[192],
				GF256.PowerOf2Table[180]
			};
			ReedSolomonEC EC = new ReedSolomonEC(30);

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(EC.GeneratorPolynomial.Coefficients));
		}

		[TestMethod]
		public void Test_08_EC_Code()
		{
			ReedSolomonEC EC = new ReedSolomonEC(10);
			byte[] Code = EC.GenerateCorrectionCode(new byte[] { 32, 91, 11, 120, 209, 114, 220, 77, 67, 64, 236, 17, 236, 17, 236, 17 });
			byte[] Expected = new byte[] { 196, 35, 39, 119, 235, 215, 231, 226, 93, 23 };
			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(Code));
		}

		[TestMethod]
		public void Test_09_TotalDataBytes_H()
		{
			int[] DataBytes = new int[]
			{
				9, 16, 26, 36, 46, 60, 66, 86, 100, 122, 140, 158, 180, 197,
				223, 253, 283, 313, 341, 385, 406, 442, 464, 514, 538, 596,
				628, 661, 701, 745, 793, 845, 901, 961, 986, 1054, 1096, 1142,
				1222, 1276
			};

			int i = 0;

			foreach (VersionInfo Info in Versions.HighVersions)
				Assert.AreEqual(DataBytes[i++], Info.TotalDataBytes);
		}

		[TestMethod]
		public void Test_10_TotalDataBytes_L()
		{
			int[] DataBytes = new int[]
			{
				19, 34, 55, 80, 108, 136, 156, 194, 232, 274, 324, 370, 428,
				461, 523, 589, 647, 721, 795, 861, 932, 1006, 1094, 1174,
				1276, 1370, 1468, 1531, 1631, 1735, 1843, 1955, 2071, 2191,
				2306, 2434, 2566, 2702, 2812, 2956
			};

			int i = 0;

			foreach (VersionInfo Info in Versions.LowVersions)
				Assert.AreEqual(DataBytes[i++], Info.TotalDataBytes);
		}

		[TestMethod]
		public void Test_11_TotalDataBytes_M()
		{
			int[] DataBytes = new int[]
			{
				16, 28, 44, 64, 86, 108, 124, 154, 182, 216, 254, 290, 334,
				365, 415, 453, 507, 563, 627, 669, 714, 782, 860, 914, 1000,
				1062, 1128, 1193, 1267, 1373, 1455, 1541, 1631, 1725, 1812,
				1914, 1992, 2102, 2216, 2334
			};

			int i = 0;

			foreach (VersionInfo Info in Versions.MediumVersions)
				Assert.AreEqual(DataBytes[i++], Info.TotalDataBytes);
		}

		[TestMethod]
		public void Test_12_TotalDataBytes_Q()
		{
			int[] DataBytes = new int[]
			{
				13, 22, 34, 48, 62, 76, 88, 110, 132, 154, 180, 206, 244,
				261, 295, 325, 367, 397, 445, 485, 512, 568, 614, 664, 718,
				754, 808, 871, 911, 985, 1033, 1115, 1171, 1231, 1286, 1354,
				1426, 1502, 1582, 1666
			};

			int i = 0;

			foreach (VersionInfo Info in Versions.QuartileVersions)
				Assert.AreEqual(DataBytes[i++], Info.TotalDataBytes);
		}

		[TestMethod]
		public void Test_13_ApplyErrorCorrection()
		{
			byte[] Message = new byte[]
			{
				67, 85, 70, 134, 87, 38, 85, 194, 119, 50, 6, 18, 6, 103, 38,
				246, 246, 66, 7, 118, 134, 242, 7, 38, 86, 22, 198, 199, 146, 6,
				182, 230, 247, 119, 50, 7, 118, 134, 87, 38, 82, 6, 134, 151, 50, 7,
				70, 247, 118, 86, 194, 6, 151, 50, 16, 236, 17, 236, 17, 236, 17, 236
			};
			byte[] FinalMessage = this.encoder.ApplyErrorCorrection(5, CorrectionLevel.Q, Message);
			byte[] Expected = new byte[]
			{
				67, 246, 182, 70, 85, 246, 230, 247, 70, 66, 247, 118, 134, 7, 119,
				86, 87, 118, 50, 194, 38, 134, 7, 6, 85, 242, 118, 151, 194, 7, 134,
				50, 119, 38, 87, 16, 50, 86, 38, 236, 6, 22, 82, 17, 18, 198, 6, 236,
				6, 199, 134, 17, 103, 146, 151, 236, 38, 6, 50, 17, 7, 236, 213, 87,
				148, 235, 199, 204, 116, 159, 11, 96, 177, 5, 45, 60, 212, 173, 115,
				202, 76, 24, 247, 182, 133, 147, 241, 124, 75, 59, 223, 157, 242, 33,
				229, 200, 238, 106, 248, 134, 76, 40, 154, 27, 195, 255, 117, 129,
				230, 172, 154, 209, 189, 82, 111, 17, 10, 2, 86, 163, 108, 131, 161,
				163, 240, 32, 111, 120, 192, 178, 39, 133, 141, 236
			};

			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(FinalMessage));
		}

		[TestMethod]
		public void Test_14_GenerateMatrix()
		{
			byte[] Message = new byte[]
			{
				67, 85, 70, 134, 87, 38, 85, 194, 119, 50, 6, 18, 6, 103, 38,
				246, 246, 66, 7, 118, 134, 242, 7, 38, 86, 22, 198, 199, 146, 6,
				182, 230, 247, 119, 50, 7, 118, 134, 87, 38, 82, 6, 134, 151, 50, 7,
				70, 247, 118, 86, 194, 6, 151, 50, 16, 236, 17, 236, 17, 236, 17, 236
			};
			QrMatrix Matrix = this.encoder.GenerateMatrix(5, CorrectionLevel.Q, Message, true);

			Console.Out.WriteLine(Matrix.ToFullBlockText());
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine(Matrix.ToHalfBlockText());
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine(Matrix.ToQuarterBlockText());
		}

		private const bool X = true;
		private const bool _ = false;
		private static readonly bool[,] penaltyMatrix = new bool[,]
		{
			{  X, X, X, X, X, X, X, _, X, X, _, _, _, _, X, X, X, X, X, X, X },
			{  X, _, _, _, _, _, X, _, X, _, _, X, _, _, X, _, _, _, _, _, X },
			{  X, _, X, X, X, _, X, _, X, _, _, X, X, _, X, _, X, X, X, _, X },
			{  X, _, X, X, X, _, X, _, X, _, _, _, _, _, X, _, X, X, X, _, X },
			{  X, _, X, X, X, _, X, _, X, _, X, _, _, _, X, _, X, X, X, _, X },
			{  X, _, _, _, _, _, X, _, _, _, X, _, _, _, X, _, _, _, _, _, X },
			{  X, X, X, X, X, X, X, _, X, _, X, _, X, _, X, X, X, X, X, X, X },
			{  _, _, _, _, _, _, _, _, X, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  _, X, X, _, X, _, X, X, _, _, _, _, X, _, X, _, X, X, X, X, X },
			{  _, X, _, _, _, _, _, _, X, X, X, X, _, _, _, _, X, _, _, _, X },
			{  _, _, X, X, _, X, X, X, _, X, X, _, _, _, X, _, X, X, _, _, _ },
			{  _, X, X, _, X, X, _, X, _, _, X, X, _, X, _, X, _, X, X, X, _ },
			{  X, _, _, _, X, _, X, _, X, _, X, X, X, _, X, X, X, _, X, _, X },
			{  _, _, _, _, _, _, _, _, X, X, _, X, _, _, X, _, _, _, X, _, X },
			{  X, X, X, X, X, X, X, _, X, _, X, _, _, _, _, X, _, X, X, _, _ },
			{  X, _, _, _, _, _, X, _, _, X, _, X, X, _, X, X, _, X, _, _, _ },
			{  X, _, X, X, X, _, X, _, X, _, X, _, _, _, X, X, X, X, X, X, X },
			{  X, _, X, X, X, _, X, _, _, X, _, X, _, X, _, X, _, _, _, X, _ },
			{  X, _, X, X, X, _, X, _, X, _, _, _, X, X, X, X, _, X, _, _, X },
			{  X, _, _, _, _, _, X, _, X, _, X, X, _, X, _, _, _, X, _, X, X },
			{  X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, _, _, _, _, X },
		};
		private static readonly bool[,] penaltyMask = new bool[,]
		{
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, X, X, X, X, X, X, X, X },
			{  _, _, _, _, _, _, X, _, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  _, _, _, _, _, _, X, _, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  _, _, _, _, _, _, X, _, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  _, _, _, _, _, _, X, _, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  _, _, _, _, _, _, X, _, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
			{  X, X, X, X, X, X, X, X, _, _, _, _, _, _, _, _, _, _, _, _, _ },
		};

		[TestMethod]
		public void Test_15_Penalty_HorizontalBands()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(88, M.PenaltyHorizontalBands());
		}

		[TestMethod]
		public void Test_16_Penalty_VerticalBands()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(92, M.PenaltyVerticalBands());
		}

		[TestMethod]
		public void Test_17_Penalty_Blocks()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(90, M.PenaltyBlocks());
		}

		[TestMethod]
		public void Test_18_Penalty_HorizontalFinderPattern()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(40, M.PenaltyHorizontalFinderPattern());
		}

		[TestMethod]
		public void Test_19_Penalty_VerticalFinderPattern()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(40, M.PenaltyVerticalFinderPattern());
		}

		[TestMethod]
		public void Test_20_Penalty_Balance()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(0, M.PenaltyBalance());
		}

		[TestMethod]
		public void Test_21_Penalty_Total()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			Assert.AreEqual(350, M.Penalty());
		}

		[TestMethod]
		public void Test_22_Penalty_Total_Mask0()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			string s1 = M.ToFullBlockText();
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask0);
			string s2 = M.ToFullBlockText();
			Assert.AreEqual(s1, s2);
			Console.Out.WriteLine(s2);
			Assert.AreEqual(350, M.Penalty());
		}

		[TestMethod]
		public void Test_23_Penalty_Total_Mask1()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask1);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(421, M.Penalty());
		}

		[TestMethod]
		public void Test_24_Penalty_Total_Mask2()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask2);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(507, M.Penalty());
		}

		[TestMethod]
		public void Test_25_Penalty_Total_Mask3()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask3);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(443, M.Penalty());
		}

		[TestMethod]
		public void Test_26_Penalty_Total_Mask4()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask4);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(553, M.Penalty());
		}

		[TestMethod]
		public void Test_27_Penalty_Total_Mask5()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask5);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(547, M.Penalty());
		}

		[TestMethod]
		public void Test_28_Penalty_Total_Mask6()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask6);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(357, M.Penalty());
		}

		[TestMethod]
		public void Test_29_Penalty_Total_Mask7()
		{
			QrMatrix M = new QrMatrix((bool[,])penaltyMatrix.Clone(), (bool[,])penaltyMask.Clone());
			M.ApplyMask(QrMatrix.Mask0);
			M.ApplyMask(QrMatrix.Mask7);
			Console.Out.WriteLine(M.ToFullBlockText());
			Assert.AreEqual(520, M.Penalty());
		}

		[TestMethod]
		public void Test_30_HelloWorld()
		{
			CorrectionLevel Level = CorrectionLevel.Q;
			string Message = "HELLO WORLD";
			KeyValuePair<byte[], VersionInfo> Encoding = this.encoder.Encode(Level, Message);
			byte[] Expected = new byte[]
			{
				0b00100000, 0b01011011, 0b00001011, 0b01111000, 0b11010001,
				0b01110010, 0b11011100, 0b01001101, 0b01000011, 0b01000000,
				0b11101100, 0b00010001, 0b11101100
			};

			Assert.AreEqual(1, Encoding.Value.Version);
			Assert.AreEqual(Convert.ToBase64String(Expected), Convert.ToBase64String(Encoding.Key));
		}

		[TestMethod]
		public void Test_31_HelloWorld_QR()
		{
			CorrectionLevel Level = CorrectionLevel.Q;
			string Message = "HELLO WORLD";
			//KeyValuePair<byte[], VersionInfo> P = this.encoder.Encode(Level, Message);
			//QrMatrix M = this.encoder.GenerateMatrix(P.Value, P.Key, true, false);
			QrMatrix M = this.encoder.GenerateMatrix(Level, Message);
			Console.Out.WriteLine(M.ToHalfBlockText());
		}

	}
}
