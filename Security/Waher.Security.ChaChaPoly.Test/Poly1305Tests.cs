using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.ChaChaPoly.Test
{
    /// <summary>
    /// Tests taken from https://tools.ietf.org/html/rfc8439, retrieved 2019-05-31
    /// </summary>
	[TestClass]
    public class Poly1305Tests
    {
        [TestMethod]
        public void Test_01_Calc_MAC()
        {
            // §2.5.2

            byte[] Key = new byte[]
            {
                0x85, 0xd6, 0xbe, 0x78, 0x57, 0x55, 0x6d, 0x33,
                0x7f, 0x44, 0x52, 0xfe, 0x42, 0xd5, 0x06, 0xa8,
                0x01, 0x03, 0x80, 0x8a, 0xfb, 0x0d, 0xb2, 0xfd,
                0x4a, 0xbf, 0xf6, 0xaf, 0x41, 0x49, 0xf5, 0x1b
            };
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Encoding.ASCII.GetBytes("Cryptographic Forum Research Group");

            Assert.AreEqual("43727970746f6772617068696320466f72756d2052657365617263682047726f7570",
                 Hashes.BinaryToString(Data));

            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("a8061dc1305136c6c22b8baf0c0127a9", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_02_A3_TestVector_1()
        {
            // §A.3

            byte[] Key = new byte[32];
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = new byte[64];
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("00000000000000000000000000000000", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_03_A3_TestVector_2()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0000000000000000000000000000000036e5f6b5c5e06070f0efca96227a863e");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("416e79207375626d697373696f6e20746f20746865204945544620696e74656e6465642062792074686520436f6e7472696275746f7220666f72207075626c69636174696f6e20617320616c6c206f722070617274206f6620616e204945544620496e7465726e65742d4472616674206f722052464320616e6420616e792073746174656d656e74206d6164652077697468696e2074686520636f6e74657874206f6620616e204945544620616374697669747920697320636f6e7369646572656420616e20224945544620436f6e747269627574696f6e222e20537563682073746174656d656e747320696e636c756465206f72616c2073746174656d656e747320696e20494554462073657373696f6e732c2061732077656c6c206173207772697474656e20616e6420656c656374726f6e696320636f6d6d756e69636174696f6e73206d61646520617420616e792074696d65206f7220706c6163652c207768696368206172652061646472657373656420746f");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("36e5f6b5c5e06070f0efca96227a863e", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_04_A3_TestVector_3()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("36e5f6b5c5e06070f0efca96227a863e00000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("416e79207375626d697373696f6e20746f20746865204945544620696e74656e6465642062792074686520436f6e7472696275746f7220666f72207075626c69636174696f6e20617320616c6c206f722070617274206f6620616e204945544620496e7465726e65742d4472616674206f722052464320616e6420616e792073746174656d656e74206d6164652077697468696e2074686520636f6e74657874206f6620616e204945544620616374697669747920697320636f6e7369646572656420616e20224945544620436f6e747269627574696f6e222e20537563682073746174656d656e747320696e636c756465206f72616c2073746174656d656e747320696e20494554462073657373696f6e732c2061732077656c6c206173207772697474656e20616e6420656c656374726f6e696320636f6d6d756e69636174696f6e73206d61646520617420616e792074696d65206f7220706c6163652c207768696368206172652061646472657373656420746f");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("f3477e7cd95417af89a6b8794c310cf0", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_05_A3_TestVector_4()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("1c9240a5eb55d38af333888604f6b5f0473917c1402b80099dca5cbc207075c0");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("2754776173206272696c6c69672c20616e642074686520736c6974687920746f7665730a446964206779726520616e642067696d626c6520696e2074686520776162653a0a416c6c206d696d737920776572652074686520626f726f676f7665732c0a416e6420746865206d6f6d65207261746873206f757467726162652e");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("4541669a7eaaee61e708dc7cbcc5eb62", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_06_A3_TestVector_5()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0200000000000000000000000000000000000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("03000000000000000000000000000000", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_07_A3_TestVector_6()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("02000000000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("02000000000000000000000000000000");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("03000000000000000000000000000000", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_08_A3_TestVector_7()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0100000000000000000000000000000000000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFF11000000000000000000000000000000");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("05000000000000000000000000000000", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_09_A3_TestVector_8()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0100000000000000000000000000000000000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFBFEFEFEFEFEFEFEFEFEFEFEFEFEFEFE01010101010101010101010101010101");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("00000000000000000000000000000000", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_10_A3_TestVector_9()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0200000000000000000000000000000000000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("FDFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("faffffffffffffffffffffffffffffff", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_11_A3_TestVector_10()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0100000000000000040000000000000000000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("E33594D7505E43B900000000000000003394D7505E4379CD01000000000000000000000000000000000000000000000001000000000000000000000000000000");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("14000000000000005500000000000000", Hashes.BinaryToString(Mac));
        }

        [TestMethod]
        public void Test_12_A3_TestVector_11()
        {
            // §A.3

            byte[] Key = Hashes.StringToBinary("0100000000000000040000000000000000000000000000000000000000000000");
            Poly1305 Authenticator = new Poly1305(Key);
            byte[] Data = Hashes.StringToBinary("E33594D7505E43B900000000000000003394D7505E4379CD010000000000000000000000000000000000000000000000");
            byte[] Mac = Authenticator.CalcMac(Data);

            Assert.AreEqual("13000000000000000000000000000000", Hashes.BinaryToString(Mac));
        }
    }
}
