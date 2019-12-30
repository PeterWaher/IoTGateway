using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    [TestClass]
    public partial class SHA3_224_Tests
    {
        [TestMethod]
        public void Test_01_0_bits()
        {
            SHA3_224 H = new SHA3_224();
            int i = 0;

            H.NewState += (sender, e) =>
            {
                string Expected = States0Bits[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Digest = H.ComputeVariable(new byte[0]);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("6b4e03423667dbb73b6e15454f0eb1abd4597f9a1b078e3f5b5a6bc7", s);
            Assert.AreEqual(States0Bits.Length, i);
        }

        [TestMethod]
        public void Test_02_1600_bits()
        {
            SHA3_224 H = new SHA3_224();
            int i = 0;

            H.NewState += (sender, e) =>
            {
                string Expected = States1600Bits[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Input = new byte[200];
            int j;

            for (j = 0; j < 200; j++)
                Input[j] = 0xa3;

            byte[] Digest = H.ComputeVariable(Input);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("9376816aba503f72f96ce7eb65ac095deee3be4bf9bbc2a1cb7e11e0", s);
            Assert.AreEqual(States1600Bits.Length, i);
        }

        [TestMethod]
        public void Test_03_1600_bits_Stream()
        {
            SHA3_224 H = new SHA3_224();
            byte[] Input = new byte[200];
            int j;

            for (j = 0; j < 200; j++)
                Input[j] = 0xa3;

            byte[] Digest = H.ComputeVariable(new MemoryStream(Input));
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("9376816aba503f72f96ce7eb65ac095deee3be4bf9bbc2a1cb7e11e0", s);
        }

        [TestMethod]
        public void Test_04_Performance()
        {
            byte[] Data = new byte[80 * 1024 * 1024];
            SHA3_224 H = new SHA3_224();
            H.ComputeVariable(Data);
        }
    }
}
