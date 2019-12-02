using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class KeyGenerationTests
	{
		/*
		 * | Size       |         Time                              ||| Len\(P) | Len(Q) |
		 * |-----------:|-------------:|------------:|---------------:|--------:|-------:|
		 * |   512 bits |      9,21 ms |   0.00921 s | 0.00015350 min |      32 |     32 |
		 * |   768 bits |     12,49 ms |   0.01249 s | 0.00020817 min |      48 |     48 |
		 * |  1024 bits |     13,35 ms |   0.01335 s | 0.00022250 min |      64 |     64 |
		 * |  2048 bits |     58,66 ms |   0.05866 s | 0.00097767 min |     128 |    128 |
		 * |  3072 bits |    188,52 ms |   0.18852 s | 0.00314200 min |     192 |    192 |
		 * |  4096 bits |    603,87 ms |   0.60387 s | 0.01006450 min |     256 |    256 |
		 * |  7680 bits |   2522,16 ms |   2.52216 s | 0.04203600 min |     480 |    480 |
		 * | 15360 bits | 100651,58 ms | 100.65158 s | 1.67752633 min |     960 |    960 |
		 */

		[TestMethod]
		public void Test_01_512bits()
		{
			this.Test("xTest_01_512bits", 512);
		}

		[TestMethod]
		public void Test_02_768bits()
		{
			this.Test("xTest_02_768bits", 768);
		}

		[TestMethod]
		public void Test_03_1024bits()
		{
			this.Test("Test_03_1024bits", 1024);
		}

		[TestMethod]
		public void Test_04_2048bits()
		{
			this.Test("Test_04_2048bits", 2048);
		}

		[TestMethod]
		public void Test_05_3072bits()
		{
			this.Test("Test_05_3072bits", 3072);
		}

		[TestMethod]
		public void Test_06_4096bits()
		{
			this.Test("Test_06_4096bits", 4096);
		}

		[TestMethod]
		public void Test_07_7680bits()
		{
			this.Test("Test_07_7680bits", 7680);
		}

		[TestMethod]
		public void Test_08_15360bits()
		{
			this.Test("Test_08_15360bits", 15360);
		}

		private void Test(string Name, int Size)
		{
			CspParameters CspParams = new CspParameters()
			{
				Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.NoPrompt,
				KeyContainerName = Name
			};

			Stopwatch Watch = new Stopwatch();
			Watch.Start();

			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(Size, CspParams);

			Watch.Stop();
			double Ms = (1000.0d * Watch.ElapsedTicks) / Stopwatch.Frequency;

			RSAParameters Parameters = rsa.ExportParameters(true);

			Console.Out.WriteLine("Size: " + Size.ToString());
			Console.Out.WriteLine("Time: " + Ms.ToString("F2") + " ms");
			Console.Out.WriteLine("P Len: " + Parameters.P.Length.ToString());
			Console.Out.WriteLine("Q Len: " + Parameters.Q.Length.ToString());
		}
	}
}
