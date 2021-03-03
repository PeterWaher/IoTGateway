using System;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Waher.Content.QR.Encoding;

namespace Waher.Content.QR.Test.VersionTests
{
	public abstract class VersionTests
	{
		protected static readonly Random rnd = new Random();
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

		public abstract CorrectionLevel Level
		{
			get;
		}

		public abstract EncodingMode Mode
		{
			get;
		}

		public abstract string Folder
		{
			get;
		}

		public abstract string GetMessage(int ForVersion);

		private void DoTest(int Version)
		{
			string Message = this.GetMessage(Version);
			QrMatrix M = this.encoder.GenerateMatrix(this.Level, Message);
			Assert.AreEqual(((Version - 1) << 2) + 21, M.Size);

			string Folder = this.Folder;
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			File.WriteAllText(Path.Combine(Path.Combine(Folder, Version.ToString() + "-Full.txt")), M.ToFullBlockText());
			File.WriteAllText(Path.Combine(Path.Combine(Folder, Version.ToString() + "-Half.txt")), M.ToHalfBlockText());
			File.WriteAllText(Path.Combine(Path.Combine(Folder, Version.ToString() + "-Quarter.txt")), M.ToQuarterBlockText());

			byte[] RGBA = M.ToRGBA();
			IntPtr Pixels = Marshal.AllocCoTaskMem(RGBA.Length);
			try
			{
				Marshal.Copy(RGBA, 0, Pixels, RGBA.Length);

				using (SKData Data = SKData.Create(Pixels, RGBA.Length))
				{
					using (SKImage Result = SKImage.FromPixels(new SKImageInfo(M.Size + 8, M.Size + 8, SKColorType.Bgra8888), Data, (M.Size + 8) << 2))
					{
						using (SKData Data2 = Result.Encode(SKEncodedImageFormat.Png, 100))
						{
							File.WriteAllBytes(Path.Combine(Path.Combine(Folder, Version.ToString() + ".png")), Data2.ToArray());
						}
					}
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(Pixels);
			}
		}

		[TestMethod]
		public void Test_01_Version_1()
		{
			this.DoTest(1);
		}

		[TestMethod]
		public void Test_02_Version_2()
		{
			this.DoTest(2);
		}

		[TestMethod]
		public void Test_03_Version_3()
		{
			this.DoTest(3);
		}

		[TestMethod]
		public void Test_04_Version_4()
		{
			this.DoTest(4);
		}

		[TestMethod]
		public void Test_05_Version_5()
		{
			this.DoTest(5);
		}

		[TestMethod]
		public void Test_06_Version_6()
		{
			this.DoTest(6);
		}

		[TestMethod]
		public void Test_07_Version_7()
		{
			this.DoTest(7);
		}

		[TestMethod]
		public void Test_08_Version_8()
		{
			this.DoTest(8);
		}

		[TestMethod]
		public void Test_09_Version_9()
		{
			this.DoTest(9);
		}

		[TestMethod]
		public void Test_10_Version_10()
		{
			this.DoTest(10);
		}

		[TestMethod]
		public void Test_11_Version_11()
		{
			this.DoTest(11);
		}

		[TestMethod]
		public void Test_12_Version_12()
		{
			this.DoTest(12);
		}

		[TestMethod]
		public void Test_13_Version_13()
		{
			this.DoTest(13);
		}

		[TestMethod]
		public void Test_14_Version_14()
		{
			this.DoTest(14);
		}

		[TestMethod]
		public void Test_15_Version_15()
		{
			this.DoTest(15);
		}

		[TestMethod]
		public void Test_16_Version_16()
		{
			this.DoTest(16);
		}

		[TestMethod]
		public void Test_17_Version_17()
		{
			this.DoTest(17);
		}

		[TestMethod]
		public void Test_18_Version_18()
		{
			this.DoTest(18);
		}

		[TestMethod]
		public void Test_19_Version_19()
		{
			this.DoTest(19);
		}

		[TestMethod]
		public void Test_20_Version_20()
		{
			this.DoTest(20);
		}

		[TestMethod]
		public void Test_21_Version_21()
		{
			this.DoTest(21);
		}

		[TestMethod]
		public void Test_22_Version_22()
		{
			this.DoTest(22);
		}

		[TestMethod]
		public void Test_23_Version_23()
		{
			this.DoTest(23);
		}

		[TestMethod]
		public void Test_24_Version_24()
		{
			this.DoTest(24);
		}

		[TestMethod]
		public void Test_25_Version_25()
		{
			this.DoTest(25);
		}

		[TestMethod]
		public void Test_26_Version_26()
		{
			this.DoTest(26);
		}

		[TestMethod]
		public void Test_27_Version_27()
		{
			this.DoTest(27);
		}

		[TestMethod]
		public void Test_28_Version_28()
		{
			this.DoTest(28);
		}

		[TestMethod]
		public void Test_29_Version_29()
		{
			this.DoTest(29);
		}

		[TestMethod]
		public void Test_30_Version_30()
		{
			this.DoTest(30);
		}

		[TestMethod]
		public void Test_31_Version_31()
		{
			this.DoTest(31);
		}

		[TestMethod]
		public void Test_32_Version_32()
		{
			this.DoTest(32);
		}

		[TestMethod]
		public void Test_33_Version_33()
		{
			this.DoTest(33);
		}

		[TestMethod]
		public void Test_34_Version_34()
		{
			this.DoTest(34);
		}

		[TestMethod]
		public void Test_35_Version_35()
		{
			this.DoTest(35);
		}

		[TestMethod]
		public void Test_36_Version_36()
		{
			this.DoTest(36);
		}

		[TestMethod]
		public void Test_37_Version_37()
		{
			this.DoTest(37);
		}

		[TestMethod]
		public void Test_38_Version_38()
		{
			this.DoTest(38);
		}

		[TestMethod]
		public void Test_39_Version_39()
		{
			this.DoTest(39);
		}

		[TestMethod]
		public void Test_40_Version_40()
		{
			this.DoTest(40);
		}
	}
}
