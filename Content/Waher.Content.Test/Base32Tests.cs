using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Waher.Content.Test
{
	[TestClass]
	public class Base32Tests
	{
		[TestMethod]
		[DataRow("", "")]
		[DataRow("f", "MY======")]
		[DataRow("fo", "MZXQ====")]
		[DataRow("foo", "MZXW6===")]
		[DataRow("foob", "MZXW6YQ=")]
		[DataRow("fooba", "MZXW6YTB")]
		[DataRow("foobar", "MZXW6YTBOI======")]
		public void Test_01_Encode(string AsciiString, string Base32String)
		{
			byte[] Bin = Encoding.ASCII.GetBytes(AsciiString);
			string Encoded = Base32.Encode(Bin);
			Assert.AreEqual(Base32String, Encoded);
		}

		[TestMethod]
		[DataRow("", "")]
		[DataRow("f", "MY======")]
		[DataRow("fo", "MZXQ====")]
		[DataRow("foo", "MZXW6===")]
		[DataRow("foob", "MZXW6YQ=")]
		[DataRow("fooba", "MZXW6YTB")]
		[DataRow("foobar", "MZXW6YTBOI======")]
		[DataRow("f", "my======")]
		[DataRow("fo", "mzxq====")]
		[DataRow("foo", "mzxw6===")]
		[DataRow("foob", "mzxw6yq=")]
		[DataRow("fooba", "mzxw6ytb")]
		[DataRow("foobar", "mzxw6ytboi======")]
		[DataRow("foobar", "mzx w6yt\r\nboi======")]
		public void Test_02_Decode(string AsciiString, string Base32String)
		{
			byte[] Bin = Base32.Decode(Base32String);
			string Encoded = Encoding.ASCII.GetString(Bin);
			Assert.AreEqual(AsciiString, Encoded);
		}

		[TestMethod]
		[DataRow(new byte[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!', (byte)'\xde', (byte)'\xad', (byte)'\xbe', (byte)'\xef' }, "JBSWY3DPEHPK3PXP")]
		public void Test_03_EncodeBin(byte[] Bin, string Base32String)
		{
			string Encoded = Base32.Encode(Bin);
			Assert.AreEqual(Base32String, Encoded);
		}

		[TestMethod]
		[DataRow(new byte[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!', (byte)'\xde', (byte)'\xad', (byte)'\xbe', (byte)'\xef' }, "JBSWY3DPEHPK3PXP")]
		public void Test_04_DecodeBin(byte[] Bin, string Base32String)
		{
			byte[] Bin2 = Base32.Decode(Base32String);
			Assert.AreEqual(Convert.ToBase64String(Bin), Convert.ToBase64String(Bin2));
		}
	}
}
