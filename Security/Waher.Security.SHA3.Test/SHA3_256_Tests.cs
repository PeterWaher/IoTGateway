using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    [TestClass]
    public partial class SHA3_256_Tests
    {
        [TestMethod]
        public void Test_01_0_bits()
        {
            SHA3_256 H = new();
            int i = 0;

            H.NewState += (Sender, e) =>
            {
                string Expected = States0Bits[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Digest = H.ComputeVariable([]);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a", s);
            Assert.AreEqual(States0Bits.Length, i);
        }

        [TestMethod]
        public void Test_02_1600_bits()
        {
            SHA3_256 H = new();
            int i = 0;

            H.NewState += (Sender, e) =>
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
            Assert.AreEqual("79f38adec5c20307a98ef76e8324afbfd46cfd81b22e3973c65fa1bd9de31787", s);
            Assert.AreEqual(States1600Bits.Length, i);
        }

        [TestMethod]
        public void Test_03_1600_bits_Stream()
        {
            SHA3_256 H = new();
            byte[] Input = new byte[200];
            int j;

            for (j = 0; j < 200; j++)
                Input[j] = 0xa3;

            byte[] Digest = H.ComputeVariable(new MemoryStream(Input));
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("79f38adec5c20307a98ef76e8324afbfd46cfd81b22e3973c65fa1bd9de31787", s);
        }

        [TestMethod]
        public void Test_04_Performance()
        {
            byte[] Data = new byte[80 * 1024 * 1024];
            SHA3_256 H = new();
            H.ComputeVariable(Data);
        }
    }
}
