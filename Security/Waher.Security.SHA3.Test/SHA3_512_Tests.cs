using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    [TestClass]
    public partial class SHA3_512_Tests
    {
        [TestMethod]
        public void Test_01_0_bits()
        {
            SHA3_512 H = new SHA3_512();
            int i = 0;

            H.NewState += (sender, e) =>
            {
                string Expected = States0Bits[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Digest = H.ComputeVariable(new byte[0]);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("a69f73cca23a9ac5c8b567dc185a756e97c982164fe25859e0d1dcc1475c80a615b2123af1f5f94c11e3e9402c3ac558f500199d95b6d3e301758586281dcd26", s);
            Assert.AreEqual(States0Bits.Length, i);
        }

        [TestMethod]
        public void Test_02_1600_bits()
        {
            SHA3_512 H = new SHA3_512();
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
            Assert.AreEqual("e76dfad22084a8b1467fcf2ffa58361bec7628edf5f3fdc0e4805dc48caeeca81b7c13c30adf52a3659584739a2df46be589c51ca1a4a8416df6545a1ce8ba00", s);
            Assert.AreEqual(States1600Bits.Length, i);
        }

        [TestMethod]
        public void Test_03_Performance()
        {
            byte[] Data = new byte[80 * 1024 * 1024];
            SHA3_512 H = new SHA3_512();
            H.ComputeVariable(Data);
        }
    }
}
