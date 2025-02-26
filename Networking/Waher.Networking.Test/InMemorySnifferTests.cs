using System.Text;
using Waher.Networking.Sniffers;

namespace Waher.Networking.Test
{
	[TestClass]
	public sealed class InMemorySnifferTests
	{
		private InMemorySniffer? sniffer;

		[TestInitialize]
		public void TestInitialize()
		{
			this.sniffer = new InMemorySniffer();
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.sniffer is not null)
			{
				await this.sniffer.DisposeAsync();
				this.sniffer = null;
			}
		}

		private async Task<string> ReplayToString(bool IncludeTimestamp)
		{
			if (this.sniffer is null)
				return string.Empty;

			StringBuilder sb = new();

			using StringWriter Writer = new(sb);
			using TextWriterSniffer Output = new(Writer, BinaryPresentationMethod.Hexadecimal)
			{
				IncludeTimestamp = IncludeTimestamp
			};

			await this.sniffer.FlushAsync();

			this.sniffer.Replay(Output);

			await Output.FlushAsync();

			string Result = sb.ToString().Trim();
			Assert.IsFalse(string.IsNullOrEmpty(Result), "Sniffer output empty.");

			return Result;
		}

		[TestMethod]
		public async Task Test_01_Information()
		{
			this.sniffer!.Information("Test");
			Assert.AreEqual("Info: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_02_Information_Timestamp()
		{
			this.sniffer!.Information(new DateTime(2010, 1, 2, 4, 5, 6), "Test");
			Assert.AreEqual("Info (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_03_Warning()
		{
			this.sniffer!.Warning("Test");
			Assert.AreEqual("Warning: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_04_Warning_Timestamp()
		{
			this.sniffer!.Warning(new DateTime(2010, 1, 2, 4, 5, 6), "Test");
			Assert.AreEqual("Warning (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_05_Error()
		{
			this.sniffer!.Error("Test");
			Assert.AreEqual("Error: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_06_Error_Timestamp()
		{
			this.sniffer!.Error(new DateTime(2010, 1, 2, 4, 5, 6), "Test");
			Assert.AreEqual("Error (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_07_Exception_Text()
		{
			this.sniffer!.Exception("Test");
			Assert.AreEqual("Exception: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_08_Exception_Text_Timestamp()
		{
			this.sniffer!.Exception(new DateTime(2010, 1, 2, 4, 5, 6), "Test");
			Assert.AreEqual("Exception (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_09_Exception_Object()
		{
			this.sniffer!.Exception(new Exception("Test"));
			Assert.AreEqual("Exception: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_09_Exception_Object_Timestamp()
		{
			this.sniffer!.Exception(new DateTime(2010, 1, 2, 4, 5, 6), new Exception("Test"));
			Assert.AreEqual("Exception (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_10_ReceiveText()
		{
			this.sniffer!.ReceiveText("Test");
			Assert.AreEqual("Rx: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_11_ReceiveText_Timestamp()
		{
			this.sniffer!.ReceiveText(new DateTime(2010, 1, 2, 4, 5, 6), "Test");
			Assert.AreEqual("Rx (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_12_TransmitText()
		{
			this.sniffer!.TransmitText("Test");
			Assert.AreEqual("Tx: Test", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_13_TransmitText_Timestamp()
		{
			this.sniffer!.TransmitText(new DateTime(2010, 1, 2, 4, 5, 6), "Test");
			Assert.AreEqual("Tx (04:05:06): Test", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_14_ReceiveBinaryCount()
		{
			this.sniffer!.ReceiveBinary(10);
			Assert.AreEqual("Rx: <10 bytes>", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_15_ReceiveBinaryCount_Timestamp()
		{
			this.sniffer!.ReceiveBinary(new DateTime(2010, 1, 2, 4, 5, 6), 10);
			Assert.AreEqual("Rx (04:05:06): <10 bytes>", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_16_ReceiveConstantBinaryBuffer()
		{
			this.sniffer!.ReceiveBinary(true, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Rx: 01 02 03 04 05", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_17_ReceiveConstantBinaryBuffer_Timestamp()
		{
			this.sniffer!.ReceiveBinary(new DateTime(2010, 1, 2, 4, 5, 6), true, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Rx (04:05:06): 01 02 03 04 05", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_18_ReceiveRandomBinaryBuffer()
		{
			this.sniffer!.ReceiveBinary(false, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Rx: 01 02 03 04 05", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_19_ReceiveRandomBinaryBuffer_Timestamp()
		{
			this.sniffer!.ReceiveBinary(new DateTime(2010, 1, 2, 4, 5, 6), false, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Rx (04:05:06): 01 02 03 04 05", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_20_ReceiveConstantBinaryBufferSegment()
		{
			this.sniffer!.ReceiveBinary(true, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Rx: 04 05 06 07 08", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_21_ReceiveConstantBinaryBufferSegment_Timestamp()
		{
			this.sniffer!.ReceiveBinary(new DateTime(2010, 1, 2, 4, 5, 6), true, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Rx (04:05:06): 04 05 06 07 08", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_22_ReceiveRandomBinaryBufferSegment()
		{
			this.sniffer!.ReceiveBinary(false, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Rx: 04 05 06 07 08", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_23_ReceiveRandomBinaryBufferSegment_Timestamp()
		{
			this.sniffer!.ReceiveBinary(new DateTime(2010, 1, 2, 4, 5, 6), false, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Rx (04:05:06): 04 05 06 07 08", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_24_TransmitBinaryCount()
		{
			this.sniffer!.TransmitBinary(10);
			Assert.AreEqual("Tx: <10 bytes>", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_25_TransmitBinaryCount_Timestamp()
		{
			this.sniffer!.TransmitBinary(new DateTime(2010, 1, 2, 4, 5, 6), 10);
			Assert.AreEqual("Tx (04:05:06): <10 bytes>", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_26_TransmitConstantBinaryBuffer()
		{
			this.sniffer!.TransmitBinary(true, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Tx: 01 02 03 04 05", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_27_TransmitConstantBinaryBuffer_Timestamp()
		{
			this.sniffer!.TransmitBinary(new DateTime(2010, 1, 2, 4, 5, 6), true, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Tx (04:05:06): 01 02 03 04 05", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_28_TransmitRandomBinaryBuffer()
		{
			this.sniffer!.TransmitBinary(false, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Tx: 01 02 03 04 05", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_29_TransmitRandomBinaryBuffer_Timestamp()
		{
			this.sniffer!.TransmitBinary(new DateTime(2010, 1, 2, 4, 5, 6), false, [1, 2, 3, 4, 5]);
			Assert.AreEqual("Tx (04:05:06): 01 02 03 04 05", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_30_TransmitConstantBinaryBufferSegment()
		{
			this.sniffer!.TransmitBinary(true, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Tx: 04 05 06 07 08", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_31_TransmitConstantBinaryBufferSegment_Timestamp()
		{
			this.sniffer!.TransmitBinary(new DateTime(2010, 1, 2, 4, 5, 6), true, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Tx (04:05:06): 04 05 06 07 08", await this.ReplayToString(true));
		}

		[TestMethod]
		public async Task Test_32_TransmitRandomBinaryBufferSegment()
		{
			this.sniffer!.TransmitBinary(false, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Tx: 04 05 06 07 08", await this.ReplayToString(false));
		}

		[TestMethod]
		public async Task Test_33_TransmitRandomBinaryBufferSegment_Timestamp()
		{
			this.sniffer!.TransmitBinary(new DateTime(2010, 1, 2, 4, 5, 6), false, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 3, 5);
			Assert.AreEqual("Tx (04:05:06): 04 05 06 07 08", await this.ReplayToString(true));
		}

	}
}
