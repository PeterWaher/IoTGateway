using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public void Test_02_Decode(string AsciiString, string Base32String)
		{
			byte[] Bin = Base32.Decode(Base32String);
			string Encoded = Encoding.ASCII.GetString(Bin);
			Assert.AreEqual(AsciiString, Encoded);
		}
	}
}
