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

    }
}
