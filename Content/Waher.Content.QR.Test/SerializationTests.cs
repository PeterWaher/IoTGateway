using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.QR.Serialization;

namespace Waher.Content.QR.Test
{
	[TestClass]
	public class SerializationTests
	{
		[TestMethod]
		public void Test_01_Bits()
		{
			BitWriter Output = new BitWriter();

			Output.WriteBits(0b0010, 4);
			Output.WriteBits(0b000001011, 9);
			Output.WriteBits(0b01100001011, 11);
			Output.WriteBits(0b01111000110, 11);
			Output.WriteBits(0b10001011100, 11);
			Output.WriteBits(0b10110111000, 11);
			Output.WriteBits(0b10011010100, 11);
			Output.WriteBits(0b001101, 6);
			Output.WriteBits(0b0000, 4);

			Output.Pad(13, 0b11101100, 0b00010001);

			byte[] Bin = Output.ToArray();

			Assert.AreEqual(13, Bin.Length);
			Assert.AreEqual(0b00100000, Bin[0]);
			Assert.AreEqual(0b01011011, Bin[1]);
			Assert.AreEqual(0b00001011, Bin[2]);
			Assert.AreEqual(0b01111000, Bin[3]);
			Assert.AreEqual(0b11010001, Bin[4]);
			Assert.AreEqual(0b01110010, Bin[5]);
			Assert.AreEqual(0b11011100, Bin[6]);
			Assert.AreEqual(0b01001101, Bin[7]);
			Assert.AreEqual(0b01000011, Bin[8]);
			Assert.AreEqual(0b01000000, Bin[9]);
			Assert.AreEqual(0b11101100, Bin[10]);
			Assert.AreEqual(0b00010001, Bin[11]);
			Assert.AreEqual(0b11101100, Bin[12]);
		}

		[TestMethod]
		public void Test_02_Numeric()
		{
			BitWriter Output = new BitWriter();
			NumericEncoder Encoder = new NumericEncoder(Output);

			Assert.IsTrue(Encoder.Encode("8675309"));

			byte[] Bin = Output.ToArray();

			Assert.AreEqual(3, Bin.Length);
			Assert.AreEqual(0b11011000, Bin[0]);
			Assert.AreEqual(0b11100001, Bin[1]);
			Assert.AreEqual(0b00101001, Bin[2]);
		}

		[TestMethod]
		public void Test_03_Alphanumeric()
		{
			BitWriter Output = new BitWriter();
			AlphanumericEncoder Encoder = new AlphanumericEncoder(Output);

			Assert.IsTrue(Encoder.Encode("HELLO WORLD"));

			byte[] Bin = Output.ToArray();

			Assert.AreEqual(8, Bin.Length);
			Assert.AreEqual(0b01100001, Bin[0]);
			Assert.AreEqual(0b01101111, Bin[1]);
		}

		[TestMethod]
		public void Test_04_Byte()
		{
			BitWriter Output = new BitWriter();
			ByteEncoder Encoder = new ByteEncoder(Output);

			Assert.IsTrue(Encoder.Encode("Hello, world!"));

			byte[] Bin = Output.ToArray();

			Assert.AreEqual(13, Bin.Length);
			Assert.AreEqual(0b01001000, Bin[0]);
			Assert.AreEqual(0b01100101, Bin[1]);
			Assert.AreEqual(0b01101100, Bin[2]);
			Assert.AreEqual(0b01101100, Bin[3]);
			Assert.AreEqual(0b01101111, Bin[4]);
			Assert.AreEqual(0b00101100, Bin[5]);
			Assert.AreEqual(0b00100000, Bin[6]);
			Assert.AreEqual(0b01110111, Bin[7]);
			Assert.AreEqual(0b01101111, Bin[8]);
			Assert.AreEqual(0b01110010, Bin[9]);
			Assert.AreEqual(0b01101100, Bin[10]);
			Assert.AreEqual(0b01100100, Bin[11]);
			Assert.AreEqual(0b00100001, Bin[12]);
		}
	}
}
