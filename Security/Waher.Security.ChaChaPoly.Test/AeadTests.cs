using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.ChaChaPoly.Test
{
    /// <summary>
    /// Tests taken from https://tools.ietf.org/html/rfc8439, retrieved 2019-05-31
    /// </summary>
	[TestClass]
    public class AeadTests
    {
        [TestMethod]
        public void Test_01_Encrypt()
        {
            // §2.8.2

            byte[] PlainText = Encoding.ASCII.GetBytes("Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.");

            Assert.AreEqual("4c616469657320616e642047656e746c656d656e206f662074686520636c617373206f66202739393a204966204920636f756c64206f6666657220796f75206f6e6c79206f6e652074697020666f7220746865206675747572652c2073756e73637265656e20776f756c642062652069742e",
                Hashes.BinaryToString(PlainText));

            byte[] AdditionalData = Hashes.StringToBinary("50515253c0c1c2c3c4c5c6c7");
            byte[] Key = Hashes.StringToBinary("808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f");
            byte[] Nonce = Hashes.StringToBinary("070000004041424344454647");
            AeadChaCha20Poly1305 Cipher = new AeadChaCha20Poly1305(Key, Nonce);

            byte[] Encrypted = Cipher.Encrypt(PlainText, AdditionalData, out byte[] Mac);

            Assert.AreEqual("d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116", Hashes.BinaryToString(Encrypted));
            Assert.AreEqual("1ae10b594f09e26a7e902ecbd0600691", Hashes.BinaryToString(Mac));
        }

    }
}
