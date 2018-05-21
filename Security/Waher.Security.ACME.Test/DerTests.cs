using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.ACME.Test
{
	[TestClass]
	public class DerTests
	{
		private DerEncoder derOutput;

		[TestInitialize]
		public void TestInitialize()
		{
			this.derOutput = new DerEncoder();
		}

		private void AssertEqualTo(byte[] Expected)
		{
			byte[] Bin = this.derOutput.ToArray();
			int i, c = Bin.Length;

			Assert.AreEqual(Expected.Length, c, "Output length not of expected length.");

			for (i = 0; i < c; i++)
				Assert.AreEqual(Expected[i], Bin[i], "Byte " + i.ToString() + " contained " + Bin[i].ToString("x2"), " not " + Expected[i].ToString("x2") + ", as expected.");
		}

		[TestMethod]
		public void DER_Test_01_BOOLEAN_1()
		{
			this.derOutput.BOOLEAN(false);
			this.AssertEqualTo(new byte[] { 1, 1, 0 });
		}

		[TestMethod]
		public void DER_Test_02_BOOLEAN_2()
		{
			this.derOutput.BOOLEAN(true);
			this.AssertEqualTo(new byte[] { 1, 1, 255 });
		}

		[TestMethod]
		public void DER_Test_03_INTEGER_1()
		{
			this.derOutput.INTEGER(0x8f);
			this.AssertEqualTo(new byte[] { 2, 2, 0, 0x8f });
		}

		[TestMethod]
		public void DER_Test_04_INTEGER_2()
		{
			this.derOutput.INTEGER(3);
			this.AssertEqualTo(new byte[] { 2, 1, 3 });
		}

		[TestMethod]
		public void DER_Test_05_INTEGER_3()
		{
			this.derOutput.INTEGER(-200);
			this.AssertEqualTo(new byte[] { 2, 2, 0xff, 0x38 });
		}

		[TestMethod]
		public void DER_Test_06_INTEGER_4()
		{
			this.derOutput.INTEGER(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
			this.AssertEqualTo(new byte[] { 2, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
		}

		[TestMethod]
		public void DER_Test_07_BITSTRING_1()
		{
			BitArray Bits = new BitArray(20);
			int i;

			for (i = 0; i < 20; i++)
				Bits[i] = ((i & 1) == 0);

			this.derOutput.BITSTRING(Bits);
			this.AssertEqualTo(new byte[] { 3, 4, 4, 0xaa, 0xaa, 0xa0 });
		}

		[TestMethod]
		public void DER_Test_08_OCTET_STRING_1()
		{
			this.derOutput.OCTET_STRING(new byte[] { 0x1e, 0x08, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72 });
			this.AssertEqualTo(new byte[] { 4, 10, 0x1e, 0x08, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72 });
		}

		[TestMethod]
		public void DER_Test_09_NULL_1()
		{
			this.derOutput.NULL();
			this.AssertEqualTo(new byte[] { 5, 0 });
		}

		[TestMethod]
		public void DER_Test_10_OBJECT_IDENTIFIER_1()
		{
			this.derOutput.OBJECT_IDENTIFIER("1.3.6.1.4.1.311.21.20");
			this.AssertEqualTo(new byte[] { 6, 9, 0x2b, 0x06, 0x01, 0x04, 0x01, 0x82, 0x37, 0x15, 0x14 });
		}

		[TestMethod]
		public void DER_Test_11_UNICODE_STRING_1()
		{
			this.derOutput.UNICODE_STRING("CertificateTemplate");
			this.AssertEqualTo(new byte[] { 0x1e, 0x26, 00, 0x43, 0x00, 0x65, 0x00, 0x72, 0x00, 0x74, 0x00, 0x69, 0x00, 0x66, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x65, 0x00, 0x54, 0x00, 0x65, 0x00, 0x6d, 0x00, 0x70, 0x00, 0x6c, 0x00, 0x61, 0x00, 0x74, 0x00, 0x65 });
		}

		[TestMethod]
		public void DER_Test_12_IA5_STRING_1()
		{
			this.derOutput.IA5_STRING("6.0.5361.2");
			this.AssertEqualTo(new byte[] { 0x16, 0x0a, 0x36, 0x2e, 0x30, 0x2e, 0x35, 0x33, 0x36, 0x31, 0x2e, 0x32 });
		}

		[TestMethod]
		public void DER_Test_13_PRINTABLE_STRING_1()
		{
			this.derOutput.PRINTABLE_STRING("TestCN");
			this.AssertEqualTo(new byte[] { 0x13, 0x06, 0x54, 0x65, 0x73, 0x74, 0x43, 0x4e });
		}

		[TestMethod]
		public void DER_Test_14_UTF8_STRING_1()
		{
			this.derOutput.UTF8_STRING("Hellö");
			this.AssertEqualTo(new byte[] { 0x0c, 0x06, 72, 101, 108, 108, 195, 182 });
		}

		[TestMethod]
		public void DER_Test_15_SEQUENCE_1()
		{
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");
			this.derOutput.NULL();
			this.derOutput.EndSEQUENCE();

			this.AssertEqualTo(new byte[] { 0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00 });
		}

		[TestMethod]
		public void DER_Test_16_SET_1()
		{
			this.derOutput.StartSET();
			this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");
			this.derOutput.NULL();
			this.derOutput.EndSET();

			this.AssertEqualTo(new byte[] { 0x31, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00 });
		}
	}
}
