using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.ChaChaPoly.Test
{
    /// <summary>
    /// Tests taken from https://tools.ietf.org/html/rfc8439, retrieved 2019-05-31
    /// </summary>
	[TestClass]
    public class ChaCha20Tests
    {
        [TestMethod]
        public void Test_01_BlockFunction()
        {
            // §2.3.2

            byte[] Key = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f
            };
            byte[] Nonce = new byte[]
            {
                0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x4a, 0x00, 0x00, 0x00, 0x00
            };
            uint BlockCount = 1;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Result = Cipher.GetBytes(64);
            Assert.AreEqual("10f1e7e4d13b5915500fdd1fa32071c4c7d1f4c733c068030422aa9ac3d46c4ed2826446079faa0914c2d705d98b02a2b5129cd1de164eb9cbd083e8a2503c4e",
                Hashes.BinaryToString(Result));
        }

        [TestMethod]
        public void Test_02_Encrypt()
        {
            // §2.4.2

            byte[] Key = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f
            };
            byte[] Nonce = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4a, 0x00, 0x00, 0x00, 0x00
            };
            uint BlockCount = 1;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Data = Encoding.ASCII.GetBytes("Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.");

            Assert.AreEqual("4c616469657320616e642047656e746c656d656e206f662074686520636c617373206f66202739393a204966204920636f756c64206f6666657220796f75206f6e6c79206f6e652074697020666f7220746865206675747572652c2073756e73637265656e20776f756c642062652069742e",
                Hashes.BinaryToString(Data));

            byte[] Encrypted = Cipher.EncryptOrDecrypt(Data);

            Assert.AreEqual("6e2e359a2568f98041ba0728dd0d6981e97e7aec1d4360c20a27afccfd9fae0bf91b65c5524733ab8f593dabcd62b3571639d624e65152ab8f530c359f0861d807ca0dbf500d6a6156a38e088a22b65e52bc514d16ccf806818ce91ab77937365af90bbf74a35be6b40b8eedf2785e42874d",
                Hashes.BinaryToString(Encrypted));
        }

        [TestMethod]
        public void Test_03_A1_TestVector_1()
        {
            // §A.1

            byte[] Key = new byte[32];
            byte[] Nonce = new byte[12];
            uint BlockCount = 0;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Result = Cipher.GetBytes(64);
            Assert.AreEqual("76b8e0ada0f13d90405d6ae55386bd28bdd219b8a08ded1aa836efcc8b770dc7da41597c5157488d7724e03fb8d84a376a43b8f41518a11cc387b669b2ee6586",
                Hashes.BinaryToString(Result));
        }

        [TestMethod]
        public void Test_04_A1_TestVector_2()
        {
            // §A.1

            byte[] Key = new byte[32];
            byte[] Nonce = new byte[12];
            uint BlockCount = 1;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Result = Cipher.GetBytes(64);
            Assert.AreEqual("9f07e7be5551387a98ba977c732d080dcb0f29a048e3656912c6533e32ee7aed29b721769ce64e43d57133b074d839d531ed1f28510afb45ace10a1f4b794d6f",
                Hashes.BinaryToString(Result));
        }

        [TestMethod]
        public void Test_05_A1_TestVector_3()
        {
            // §A.1

            byte[] Key = new byte[32];
            Key[31] = 1;
            byte[] Nonce = new byte[12];
            uint BlockCount = 1;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Result = Cipher.GetBytes(64);
            Assert.AreEqual("3aeb5224ecf849929b9d828db1ced4dd832025e8018b8160b82284f3c949aa5a8eca00bbb4a73bdad192b5c42f73f2fd4e273644c8b36125a64addeb006c13a0",
                Hashes.BinaryToString(Result));
        }

        [TestMethod]
        public void Test_06_A1_TestVector_4()
        {
            // §A.1

            byte[] Key = new byte[32];
            Key[1] = 0xff;
            byte[] Nonce = new byte[12];
            uint BlockCount = 2;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Result = Cipher.GetBytes(64);
            Assert.AreEqual("72d54dfbf12ec44b362692df94137f328fea8da73990265ec1bbbea1ae9af0ca13b25aa26cb4a648cb9b9d1be65b2c0924a66c54d545ec1b7374f4872e99f096",
                Hashes.BinaryToString(Result));
        }

        [TestMethod]
        public void Test_07_A1_TestVector_5()
        {
            // §A.1

            byte[] Key = new byte[32];
            byte[] Nonce = new byte[12];
            Nonce[11] = 2;
            uint BlockCount = 0;
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Result = Cipher.GetBytes(64);
            Assert.AreEqual("c2c64d378cd536374ae204b9ef933fcd1a8b2288b3dfa49672ab765b54ee27c78a970e0e955c14f3a88e741b97c286f75f8fc299e8148362fa198a39531bed6d",
                Hashes.BinaryToString(Result));
        }

        [TestMethod]
        public void Test_08_A2_TestVector_1()
        {
            // §A.2

            byte[] Key = new byte[32];
            byte[] Nonce = new byte[12];
            uint BlockCount = 0;
            byte[] Message = new byte[64];
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Encrypted = Cipher.EncryptOrDecrypt(Message);
            Assert.AreEqual("76b8e0ada0f13d90405d6ae55386bd28bdd219b8a08ded1aa836efcc8b770dc7da41597c5157488d7724e03fb8d84a376a43b8f41518a11cc387b669b2ee6586",
                Hashes.BinaryToString(Encrypted));
        }

        [TestMethod]
        public void Test_09_A2_TestVector_2()
        {
            // §A.2

            byte[] Key = new byte[32];
            Key[31] = 1;
            byte[] Nonce = new byte[12];
            Nonce[11] = 2;
            uint BlockCount = 1;
            byte[] Message = Hashes.StringToBinary("416e79207375626d697373696f6e20746f20746865204945544620696e74656e6465642062792074686520436f6e7472696275746f7220666f72207075626c69636174696f6e20617320616c6c206f722070617274206f6620616e204945544620496e7465726e65742d4472616674206f722052464320616e6420616e792073746174656d656e74206d6164652077697468696e2074686520636f6e74657874206f6620616e204945544620616374697669747920697320636f6e7369646572656420616e20224945544620436f6e747269627574696f6e222e20537563682073746174656d656e747320696e636c756465206f72616c2073746174656d656e747320696e20494554462073657373696f6e732c2061732077656c6c206173207772697474656e20616e6420656c656374726f6e696320636f6d6d756e69636174696f6e73206d61646520617420616e792074696d65206f7220706c6163652c207768696368206172652061646472657373656420746f");
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Encrypted = Cipher.EncryptOrDecrypt(Message);
            Assert.AreEqual("a3fbf07df3fa2fde4f376ca23e82737041605d9f4f4f57bd8cff2c1d4b7955ec2a97948bd3722915c8f3d337f7d370050e9e96d647b7c39f56e031ca5eb6250d4042e02785ececfa4b4bb5e8ead0440e20b6e8db09d881a7c6132f420e52795042bdfa7773d8a9051447b3291ce1411c680465552aa6c405b7764d5e87bea85ad00f8449ed8f72d0d662ab052691ca66424bc86d2df80ea41f43abf937d3259dc4b2d0dfb48a6c9139ddd7f76966e928e635553ba76c5c879d7b35d49eb2e62b0871cdac638939e25e8a1e0ef9d5280fa8ca328b351c3c765989cbcf3daa8b6ccc3aaf9f3979c92b3720fc88dc95ed84a1be059c6499b9fda236e7e818b04b0bc39c1e876b193bfe5569753f88128cc08aaa9b63d1a16f80ef2554d7189c411f5869ca52c5b83fa36ff216b9c1d30062bebcfd2dc5bce0911934fda79a86f6e698ced759c3ff9b6477338f3da4f9cd8514ea9982ccafb341b2384dd902f3d1ab7ac61dd29c6f21ba5b862f3730e37cfdc4fd806c22f221",
                Hashes.BinaryToString(Encrypted));
        }

        [TestMethod]
        public void Test_10_A2_TestVector_3()
        {
            // §A.2

            byte[] Key = Hashes.StringToBinary("1c9240a5eb55d38af333888604f6b5f0473917c1402b80099dca5cbc207075c0");
            byte[] Nonce = new byte[12];
            Nonce[11] = 2;
            uint BlockCount = 42;
            byte[] Message = Hashes.StringToBinary("2754776173206272696c6c69672c20616e642074686520736c6974687920746f7665730a446964206779726520616e642067696d626c6520696e2074686520776162653a0a416c6c206d696d737920776572652074686520626f726f676f7665732c0a416e6420746865206d6f6d65207261746873206f757467726162652e");
            ChaCha20 Cipher = new ChaCha20(Key, BlockCount, Nonce);
            byte[] Encrypted = Cipher.EncryptOrDecrypt(Message);
            Assert.AreEqual("62e6347f95ed87a45ffae7426f27a1df5fb69110044c0d73118effa95b01e5cf166d3df2d721caf9b21e5fb14c616871fd84c54f9d65b283196c7fe4f60553ebf39c6402c42234e32a356b3e764312a61a5532055716ead6962568f87d3f3f7704c6a8d1bcd1bf4d50d6154b6da731b187b58dfd728afa36757a797ac188d1",
                Hashes.BinaryToString(Encrypted));
        }

        [TestMethod]
        public void Test_11_A4_TestVector_1()
        {
            // §A.4

            byte[] Key = new byte[32];
            byte[] Nonce = new byte[12];
            ChaCha20 Cipher = new ChaCha20(Key, 0, Nonce);
            byte[] Key2 = Cipher.GetBytes(32);
            Assert.AreEqual("76b8e0ada0f13d90405d6ae55386bd28bdd219b8a08ded1aa836efcc8b770dc7", Hashes.BinaryToString(Key2));
        }

        [TestMethod]
        public void Test_12_A4_TestVector_2()
        {
            // §A.4

            byte[] Key = Hashes.StringToBinary("0000000000000000000000000000000000000000000000000000000000000001");
            byte[] Nonce = Hashes.StringToBinary("000000000000000000000002");
            ChaCha20 Cipher = new ChaCha20(Key, 0, Nonce);
            byte[] Key2 = Cipher.GetBytes(32);
            Assert.AreEqual("ecfa254f845f647473d3cb140da9e87606cb33066c447b87bc2666dde3fbb739", Hashes.BinaryToString(Key2));
        }

        [TestMethod]
        public void Test_13_A4_TestVector_3()
        {
            // §A.4

            byte[] Key = Hashes.StringToBinary("1c9240a5eb55d38af333888604f6b5f0473917c1402b80099dca5cbc207075c0");
            byte[] Nonce = Hashes.StringToBinary("000000000000000000000002");
            ChaCha20 Cipher = new ChaCha20(Key, 0, Nonce);
            byte[] Key2 = Cipher.GetBytes(32);
            Assert.AreEqual("965e3bc6f9ec7ed9560808f4d229f94b137ff275ca9b3fcbdd59deaad23310ae", Hashes.BinaryToString(Key2));
        }
    }
}
