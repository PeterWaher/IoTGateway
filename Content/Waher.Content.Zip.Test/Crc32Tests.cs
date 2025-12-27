using System.Text;
using Waher.Security;

namespace Waher.Content.Zip.Test
{
	[TestClass]
	public sealed class Crc32Tests
	{
		// Ref: https://www.rfc-editor.org/rfc/rfc3720#appendix-B.4

		[TestMethod]
		[DataRow("", false, 0u)]
		[DataRow("123456789", false, 0xcbf43926u)]
		[DataRow("The quick brown fox jumps over the lazy dog", false, 0x414fa339u)]
		[DataRow("00", true, 0xd202ef8du)]
		[DataRow("01", true, 0xa505df1bu)]
		public void Test_01_Compute(string Input, bool Hex, uint ExpectedCrc)
		{
			byte[] InputBin = Hex ? Hashes.StringToBinary(Input) : Encoding.ASCII.GetBytes(Input);
			uint Crc = Crc32.Compute(InputBin);
			Assert.AreEqual(ExpectedCrc, Crc);

		}
	}
}
