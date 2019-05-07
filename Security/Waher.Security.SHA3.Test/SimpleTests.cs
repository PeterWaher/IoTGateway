using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    /// <summary>
    /// Tests taken from https://en.wikipedia.org/wiki/SHA-3, retrieved 2019-05-07
    /// </summary>
	[TestClass]
	public class SimpleTests
	{
		[TestMethod]
		public void Test_01_SHA3_224()
		{
			SHA3_224 H = new SHA3_224();
			byte[] Digest = H.ComputeVariable(new byte[0]);
			string s = Hashes.BinaryToString(Digest);
			Assert.AreEqual("6b4e03423667dbb73b6e15454f0eb1abd4597f9a1b078e3f5b5a6bc7", s);
		}

		[TestMethod]
		public void Test_02_SHA3_256()
		{
			SHA3_256 H = new SHA3_256();
			byte[] Digest = H.ComputeVariable(new byte[0]);
			string s = Hashes.BinaryToString(Digest);
			Assert.AreEqual("a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a", s);
		}

		[TestMethod]
		public void Test_03_SHA3_384()
		{
			SHA3_384 H = new SHA3_384();
			byte[] Digest = H.ComputeVariable(new byte[0]);
			string s = Hashes.BinaryToString(Digest);
			Assert.AreEqual("0c63a75b845e4f7d01107d852e4c2485c51a50aaaa94fc61995e71bbee983a2ac3713831264adb47fb6bd1e058d5f004", s);
		}

		[TestMethod]
		public void Test_04_SHA3_512()
		{
			SHA3_512 H = new SHA3_512();
			byte[] Digest = H.ComputeVariable(new byte[0]);
			string s = Hashes.BinaryToString(Digest);
			Assert.AreEqual("a69f73cca23a9ac5c8b567dc185a756e97c982164fe25859e0d1dcc1475c80a615b2123af1f5f94c11e3e9402c3ac558f500199d95b6d3e301758586281dcd26", s);
		}

		[TestMethod]
		public void Test_05_SHAKE128()
		{
			SHAKE128 H = new SHAKE128(256);
			byte[] Digest = H.ComputeVariable(new byte[0]);
			string s = Hashes.BinaryToString(Digest);
			Assert.AreEqual("7f9c2ba4e88f827d616045507605853ed73b8093f6efbc88eb1a6eacfa66ef26", s);
		}

		[TestMethod]
		public void Test_06_SHAKE256()
		{
			SHAKE256 H = new SHAKE256(512);
			byte[] Digest = H.ComputeVariable(new byte[0]);
			string s = Hashes.BinaryToString(Digest);
			Assert.AreEqual("46b9dd2b0ba88d13233b3feb743eeb243fcd52ea62b81b82b50c27646ed5762fd75dc4ddd8c0f200cb05019d67b592f6fc821c49479ab48640292eacb3b7c4be", s);
		}

        [TestMethod]
        public void Test_07_SHAKE128_2()
        {
            SHAKE128 H = new SHAKE128(256);
            byte[] Digest = H.ComputeVariable(Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog"));
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("f4202e3c5852f9182a0430fd8144f0a74b95e7417ecae17db0f8cfeed0e3e66e", s);
        }

        [TestMethod]
        public void Test_08_SHAKE128_3()
        {
            SHAKE128 H = new SHAKE128(256);
            byte[] Digest = H.ComputeVariable(Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dof"));
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("853f4538be0db9621a6cea659a06c1107b1f83f02b13d18297bd39d7411cf10c", s);
        }

        // Test all message sizes from 0 bytes .. c*4 bytes
    }
}
