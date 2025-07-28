using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    [TestClass]
    public partial class SHA3_384_Tests
    {
        [TestMethod]
        public void Test_01_0_bits()
        {
            SHA3_384 H = new();
            int i = 0;

            H.NewState += (Sender, e) =>
            {
                string Expected = States0Bits[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Digest = H.ComputeVariable([]);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("0c63a75b845e4f7d01107d852e4c2485c51a50aaaa94fc61995e71bbee983a2ac3713831264adb47fb6bd1e058d5f004", s);
            Assert.AreEqual(States0Bits.Length, i);
        }

        [TestMethod]
        public void Test_02_1600_bits()
        {
            SHA3_384 H = new();
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
            Assert.AreEqual("1881de2ca7e41ef95dc4732b8f5f002b189cc1e42b74168ed1732649ce1dbcdd76197a31fd55ee989f2d7050dd473e8f", s);
            Assert.AreEqual(States1600Bits.Length, i);
        }

        [TestMethod]
        public void Test_03_1600_bits_Stream()
        {
            SHA3_384 H = new();
            byte[] Input = new byte[200];
            int j;

            for (j = 0; j < 200; j++)
                Input[j] = 0xa3;

            byte[] Digest = H.ComputeVariable(new MemoryStream(Input));
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("1881de2ca7e41ef95dc4732b8f5f002b189cc1e42b74168ed1732649ce1dbcdd76197a31fd55ee989f2d7050dd473e8f", s);
        }

        [TestMethod]
        public void Test_04_Performance()
        {
            byte[] Data = new byte[80 * 1024 * 1024];
            SHA3_384 H = new();
            H.ComputeVariable(Data);
        }
    }
}
