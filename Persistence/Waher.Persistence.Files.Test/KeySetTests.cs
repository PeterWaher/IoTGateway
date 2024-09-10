using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using Waher.Runtime.Console;

#pragma warning disable CA1416 // Validate platform compatibility

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class KeySetTests
	{
		[TestMethod]
		public void Test_01_512bits()
		{
			Test("Test_01_512bits", 512);
		}

		[TestMethod]
		public void Test_02_1024bits()
		{
			Test("Test_02_1024bits", 1024);
		}

		[TestMethod]
		public void Test_03_2048bits()
		{
			Test("Test_03_2048bits", 2048);
		}

		[TestMethod]
		public void Test_04_3072bits()
		{
			Test("Test_04_3072bits", 3072);
		}

		[TestMethod]
		public void Test_05_4096bits()
		{
			Test("Test_05_4096bits", 4096);
		}

		[TestMethod]
		public void Test_06_7680bits()
		{
			Test("Test_06_7680bits", 7680);
		}

		[TestMethod]
		public void Test_07_15360bits()
		{
			Test("Test_07_15360bits", 15360);
		}

		private static void Test(string Name, int Size)
		{
			CspParameters CspParams = new()
			{
				Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.NoPrompt,
				KeyContainerName = Name
			};

			RSACryptoServiceProvider rsa = new(Size, CspParams);

			RSAParameters Parameters = rsa.ExportParameters(true);
			int i, c = Parameters.P.Length;
			bool DoUpdate = false;
			byte b;

			for (i = 0; i < c; i++)
			{
				b = (byte)i;

				if (Parameters.P[i] != b || Parameters.Q[i] != b)
					DoUpdate = true;
			}

			if (DoUpdate)
			{
				ConsoleOut.WriteLine("Updating");

				for (i = 0; i < c; i++)
				{
					b = (byte)i;
					Parameters.P[i] = b;
					Parameters.Q[i] = b;
				}

				rsa.ImportParameters(Parameters);
			}
			else
				ConsoleOut.WriteLine("Already updated.");
		}
	}
}

#pragma warning restore CA1416 // Validate platform compatibility
