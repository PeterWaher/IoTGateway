using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.SHA3.Test
{
    [TestClass]
    public partial class SHAKE256_Tests
    {
        [TestMethod]
        public void Test_01_0_bits()
        {
            SHAKE256 H = new SHAKE256(4096);
            int i = 0;

            H.NewState += (sender, e) =>
            {
                string Expected = States0Bits[i++].Replace(" ", string.Empty);
                string Actual = Hashes.BinaryToString(H.GetState()).ToUpper();
                Assert.AreEqual(Expected, Actual);
            };

            byte[] Digest = H.ComputeVariable(new byte[0]);
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("46b9dd2b0ba88d13233b3feb743eeb243fcd52ea62b81b82b50c27646ed5762fd75dc4ddd8c0f200cb05019d67b592f6fc821c49479ab48640292eacb3b7c4be141e96616fb13957692cc7edd0b45ae3dc07223c8e92937bef84bc0eab862853349ec75546f58fb7c2775c38462c5010d846c185c15111e595522a6bcd16cf86f3d122109e3b1fdd943b6aec468a2d621a7c06c6a957c62b54dafc3be87567d677231395f6147293b68ceab7a9e0c58d864e8efde4e1b9a46cbe854713672f5caaae314ed9083dab4b099f8e300f01b8650f1f4b1d8fcf3f3cb53fb8e9eb2ea203bdc970f50ae55428a91f7f53ac266b28419c3778a15fd248d339ede785fb7f5a1aaa96d313eacc890936c173cdcd0fab882c45755feb3aed96d477ff96390bf9a66d1368b208e21f7c10d04a3dbd4e360633e5db4b602601c14cea737db3dcf722632cc77851cbdde2aaf0a33a07b373445df490cc8fc1e4160ff118378f11f0477de055a81a9eda57a4a2cfb0c83929d310912f729ec6cfa36c6ac6a75837143045d791cc85eff5b21932f23861bcf23a52b5da67eaf7baae0f5fb1369db78f3ac45f8c4ac5671d85735cdddb09d2b1e34a1fc066ff4a162cb263d6541274ae2fcc865f618abe27c124cd8b074ccd516301b91875824d09958f341ef274bdab0bae316339894304e35877b0c28a9b1fd166c796b9cc258a064a8f57e27f2a", s);
            Assert.AreEqual(States0Bits.Length, i);
        }

        [TestMethod]
        public void Test_02_1600_bits()
        {
            SHAKE256 H = new SHAKE256(4096);
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
            Assert.AreEqual("cd8a920ed141aa0407a22d59288652e9d9f1a7ee0c1e7c1ca699424da84a904d2d700caae7396ece96604440577da4f3aa22aeb8857f961c4cd8e06f0ae6610b1048a7f64e1074cd629e85ad7566048efc4fb500b486a3309a8f26724c0ed628001a1099422468de726f1061d99eb9e93604d5aa7467d4b1bd6484582a384317d7f47d750b8f5499512bb85a226c4243556e696f6bd072c5aa2d9b69730244b56853d16970ad817e213e470618178001c9fb56c54fefa5fee67d2da524bb3b0b61ef0e9114a92cdbb6cccb98615cfe76e3510dd88d1cc28ff99287512f24bfafa1a76877b6f37198e3a641c68a7c42d45fa7acc10dae5f3cefb7b735f12d4e589f7a456e78c0f5e4c4471fffa5e4fa0514ae974d8c2648513b5db494cea847156d277ad0e141c24c7839064cd08851bc2e7ca109fd4e251c35bb0a04fb05b364ff8c4d8b59bc303e25328c09a882e952518e1a8ae0ff265d61c465896973d7490499dc639fb8502b39456791b1b6ec5bcc5d9ac36a6df622a070d43fed781f5f149f7b62675e7d1a4d6dec48c1c7164586eae06a51208c0b791244d307726505c3ad4b26b6822377257aa152037560a739714a3ca79bd605547c9b78dd1f596f2d4f1791bc689a0e9b799a37339c04275733740143ef5d2b58b96a363d4e08076a1a9d7846436e4dca5728b6f760eef0ca92bf0be5615e96959d767197a0beeb", s);
            Assert.AreEqual(States1600Bits.Length, i);
        }

        [TestMethod]
        public void Test_03_1600_bits_Stream()
        {
            SHAKE256 H = new SHAKE256(4096);
            byte[] Input = new byte[200];
            int j;

            for (j = 0; j < 200; j++)
                Input[j] = 0xa3;

            byte[] Digest = H.ComputeVariable(new MemoryStream(Input));
            string s = Hashes.BinaryToString(Digest);
            Assert.AreEqual("cd8a920ed141aa0407a22d59288652e9d9f1a7ee0c1e7c1ca699424da84a904d2d700caae7396ece96604440577da4f3aa22aeb8857f961c4cd8e06f0ae6610b1048a7f64e1074cd629e85ad7566048efc4fb500b486a3309a8f26724c0ed628001a1099422468de726f1061d99eb9e93604d5aa7467d4b1bd6484582a384317d7f47d750b8f5499512bb85a226c4243556e696f6bd072c5aa2d9b69730244b56853d16970ad817e213e470618178001c9fb56c54fefa5fee67d2da524bb3b0b61ef0e9114a92cdbb6cccb98615cfe76e3510dd88d1cc28ff99287512f24bfafa1a76877b6f37198e3a641c68a7c42d45fa7acc10dae5f3cefb7b735f12d4e589f7a456e78c0f5e4c4471fffa5e4fa0514ae974d8c2648513b5db494cea847156d277ad0e141c24c7839064cd08851bc2e7ca109fd4e251c35bb0a04fb05b364ff8c4d8b59bc303e25328c09a882e952518e1a8ae0ff265d61c465896973d7490499dc639fb8502b39456791b1b6ec5bcc5d9ac36a6df622a070d43fed781f5f149f7b62675e7d1a4d6dec48c1c7164586eae06a51208c0b791244d307726505c3ad4b26b6822377257aa152037560a739714a3ca79bd605547c9b78dd1f596f2d4f1791bc689a0e9b799a37339c04275733740143ef5d2b58b96a363d4e08076a1a9d7846436e4dca5728b6f760eef0ca92bf0be5615e96959d767197a0beeb", s);
        }

        [TestMethod]
        public void Test_04_Performance()
        {
            byte[] Data = new byte[80 * 1024 * 1024];
            SHAKE256 H = new SHAKE256(1024);
            H.ComputeVariable(Data);
        }
    }
}
