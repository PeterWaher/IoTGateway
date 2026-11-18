using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the TLV codec, using the examples of the Matter Core Specification, Appendix A.12.
	/// </summary>
	[TestClass]
	public sealed class TlvTests
	{
		private static void AssertEncoding(string ExpectedHex, TlvElement Element)
		{
			byte[] Encoded = Element.Encode();
			Assert.AreEqual(ExpectedHex.Replace(" ", string.Empty), MatterCrypto.ToHex(Encoded), "Encoding mismatch for " + Element.ToString());

			TlvElement Decoded = TlvElement.Decode(Encoded);
			Assert.AreEqual(Element.ToString(), Decoded.ToString(), "Decoding mismatch.");
			Assert.AreEqual(MatterCrypto.ToHex(Encoded), MatterCrypto.ToHex(Decoded.Encode()), "Re-encoding mismatch.");
		}

		[TestMethod]
		public void Test_01_Booleans()
		{
			AssertEncoding("08", TlvElement.Boolean(TlvTag.Anonymous, false));
			AssertEncoding("09", TlvElement.Boolean(TlvTag.Anonymous, true));
		}

		[TestMethod]
		public void Test_02_SignedIntegers()
		{
			AssertEncoding("00 2a", TlvElement.Int(TlvTag.Anonymous, 42));
			AssertEncoding("00 ef", TlvElement.Int(TlvTag.Anonymous, -17));
			AssertEncoding("01 a6 01", TlvElement.Int(TlvTag.Anonymous, 422));
			AssertEncoding("02 f0 67 fd ff", TlvElement.Int(TlvTag.Anonymous, -170000));
			AssertEncoding("03 00 90 2f 50 09 00 00 00", TlvElement.Int(TlvTag.Anonymous, 40000000000));
		}

		[TestMethod]
		public void Test_03_UnsignedIntegers()
		{
			AssertEncoding("04 2a", TlvElement.UInt(TlvTag.Anonymous, 42));
			AssertEncoding("05 ff ff", TlvElement.UInt(TlvTag.Anonymous, 65535));
			AssertEncoding("07 ff ff ff ff ff ff ff ff", TlvElement.UInt(TlvTag.Anonymous, ulong.MaxValue));
		}

		[TestMethod]
		public void Test_04_Strings()
		{
			AssertEncoding("0c 06 48 65 6c 6c 6f 21", TlvElement.String(TlvTag.Anonymous, "Hello!"));
			AssertEncoding("0c 07 54 73 63 68 c3 bc 73", TlvElement.String(TlvTag.Anonymous, "Tschüs"));
			AssertEncoding("10 05 00 01 02 03 04", TlvElement.Bytes(TlvTag.Anonymous, new byte[] { 0, 1, 2, 3, 4 }));
		}

		[TestMethod]
		public void Test_05_Null()
		{
			AssertEncoding("14", TlvElement.Null(TlvTag.Anonymous));
		}

		[TestMethod]
		public void Test_06_FloatingPoint()
		{
			AssertEncoding("0a 00 00 00 00", TlvElement.Float(TlvTag.Anonymous, 0.0f));
			AssertEncoding("0a ab aa aa 3e", TlvElement.Float(TlvTag.Anonymous, 1.0f / 3.0f));
			AssertEncoding("0a 33 33 8f 41", TlvElement.Float(TlvTag.Anonymous, 17.9f));
			AssertEncoding("0a 00 00 80 7f", TlvElement.Float(TlvTag.Anonymous, float.PositiveInfinity));
			AssertEncoding("0a 00 00 80 ff", TlvElement.Float(TlvTag.Anonymous, float.NegativeInfinity));
			AssertEncoding("0b 00 00 00 00 00 00 00 00", TlvElement.Double(TlvTag.Anonymous, 0.0));
			AssertEncoding("0b 55 55 55 55 55 55 d5 3f", TlvElement.Double(TlvTag.Anonymous, 1.0 / 3.0));
			AssertEncoding("0b 66 66 66 66 66 e6 31 40", TlvElement.Double(TlvTag.Anonymous, 17.9));
		}

		[TestMethod]
		public void Test_07_EmptyContainers()
		{
			AssertEncoding("15 18", TlvElement.Structure(TlvTag.Anonymous));
			AssertEncoding("16 18", TlvElement.Array(TlvTag.Anonymous));
			AssertEncoding("17 18", TlvElement.List(TlvTag.Anonymous));
		}

		[TestMethod]
		public void Test_08_Containers()
		{
			AssertEncoding("15 20 00 2a 20 01 ef 18", TlvElement.Structure(TlvTag.Anonymous,
				TlvElement.Int(0, 42),
				TlvElement.Int(1, -17)));

			AssertEncoding("16 00 00 00 01 00 02 00 03 00 04 18", TlvElement.Array(TlvTag.Anonymous,
				TlvElement.Int(TlvTag.Anonymous, 0),
				TlvElement.Int(TlvTag.Anonymous, 1),
				TlvElement.Int(TlvTag.Anonymous, 2),
				TlvElement.Int(TlvTag.Anonymous, 3),
				TlvElement.Int(TlvTag.Anonymous, 4)));

			AssertEncoding("17 00 01 20 00 2a 00 02 00 03 20 00 ef 18", TlvElement.List(TlvTag.Anonymous,
				TlvElement.Int(TlvTag.Anonymous, 1),
				TlvElement.Int(0, 42),
				TlvElement.Int(TlvTag.Anonymous, 2),
				TlvElement.Int(TlvTag.Anonymous, 3),
				TlvElement.Int(0, -17)));

			AssertEncoding("16 00 2a 02 f0 67 fd ff 15 18 0a 33 33 8f 41 0c 06 48 65 6c 6c 6f 21 18", TlvElement.Array(TlvTag.Anonymous,
				TlvElement.Int(TlvTag.Anonymous, 42),
				TlvElement.Int(TlvTag.Anonymous, -170000),
				TlvElement.Structure(TlvTag.Anonymous),
				TlvElement.Float(TlvTag.Anonymous, 17.9f),
				TlvElement.String(TlvTag.Anonymous, "Hello!")));
		}

		[TestMethod]
		public void Test_09_Tags()
		{
			AssertEncoding("04 2a", TlvElement.UInt(TlvTag.Anonymous, 42));
			AssertEncoding("24 01 2a", TlvElement.UInt(1, 42));
			AssertEncoding("44 01 00 2a", TlvElement.UInt(TlvTag.CommonProfile(1), 42));
			AssertEncoding("64 a0 86 01 00 2a", TlvElement.UInt(TlvTag.CommonProfile(100000), 42));
			AssertEncoding("c4 f1 ff ed de 01 00 2a", TlvElement.UInt(TlvTag.FullyQualified(0xfff1, 0xdeed, 1), 42));
			AssertEncoding("e4 f1 ff ed de ed fe 55 aa 2a", TlvElement.UInt(TlvTag.FullyQualified(0xfff1, 0xdeed, 0xaa55feed), 42));
			AssertEncoding("d5 f1 ff ed de 01 00 c4 f1 ff ed de 55 aa 2a 18", TlvElement.Structure(TlvTag.FullyQualified(0xfff1, 0xdeed, 1),
				TlvElement.UInt(TlvTag.FullyQualified(0xfff1, 0xdeed, 0xaa55), 42)));
		}

		[TestMethod]
		public void Test_10_Writer()
		{
			TlvWriter w = new TlvWriter();
			w.StartStructure();
			w.WriteInt(0, 42);
			w.WriteInt(1, -17);
			w.EndContainer();

			Assert.AreEqual("152000" + "2a2001ef18", MatterCrypto.ToHex(w.ToArray()));
		}

		[TestMethod]
		public void Test_11_Accessors()
		{
			TlvElement E = TlvElement.Decode(MatterCrypto.FromHex("152000 2a 2001 ef 18".Replace(" ", "")));
			Assert.AreEqual(42L, E[0]!.AsInt64());
			Assert.AreEqual(-17L, E[1]!.AsInt64());
			Assert.IsNull(E[2]);
		}

		[TestMethod]
		public void Test_12_InvalidData()
		{
			Assert.ThrowsExactly<MatterDecodingException>(() => TlvElement.Decode(MatterCrypto.FromHex("15")));
			Assert.ThrowsExactly<MatterDecodingException>(() => TlvElement.Decode(MatterCrypto.FromHex("0c05414243")));
			Assert.ThrowsExactly<MatterDecodingException>(() => TlvElement.Decode(MatterCrypto.FromHex("19")));
		}
	}
}
