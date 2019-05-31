using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Security.SHA3;

namespace Waher.Security.EllipticCurves.Test
{/*
    [TestClass]
    public class XEddsaTests
    {
        [TestMethod]
        public void Test_01_XEd25519_TestVector_1()
        {
            // Ref: https://git.silence.dev/Silence/curve25519-java/blob/7587cb3fd0a1604ea2118541c9c8c80c76e81571/android/jni/ed25519/tests/tests.c

            byte[] Secret = Hashes.StringToBinary("0000000000000000bd0000000000000000000000000000000000000000000000");
            Curve25519 Curve = new Curve25519(Secret);
            byte[] Message = new byte[200];
            byte[] Signature = Curve.Sign(Message, null);   // Random bytes = array of zeroes

            Assert.AreEqual("11c7f3e6c4df9e8a5150e1db3b30f92de3a3b3aa438656545fa7390f4bcc7bb26c431d9e90643e4f0eaa0e9c557766fa69ada576d63dcaf2ac326c11d0b97702",
                Hashes.BinaryToString(Signature));

            Assert.IsTrue(Curve.Verify(Message, Curve.PublicKey, Signature));

            Signature[0] ^= 0x01;

            Assert.IsFalse(Curve.Verify(Message, Curve.PublicKey, Signature));
        }

        [TestMethod]
        public void Test_02_XEd25519_TestVector_2()
        {
            byte[] PubKey = Hashes.StringToBinary("47b5a3f4f0c98ce62fc190e0ecdb4e8948b4fb38ec3b827564a35ec88acf1a55");
            byte[] Signature = Hashes.StringToBinary("210c42dd3e2e1b5eecdeef7fc27c90f1fa148ca1c25c2fe6252a577c4b27f256e172e9d53b22b4628b8bb72af5d79b25d9f9ee0a1a65ab980d71a28506496789");
            Curve25519 Curve = new Curve25519(Secret);
            byte[] Message = new byte[200];
            byte[] Signature = Curve.Sign(Message, null);   // Random bytes = array of zeroes

            Assert.AreEqual("11c7f3e6c4df9e8a5150e1db3b30f92de3a3b3aa438656545fa7390f4bcc7bb26c431d9e90643e4f0eaa0e9c557766fa69ada576d63dcaf2ac326c11d0b97702",
                Hashes.BinaryToString(Signature));

            Assert.IsTrue(Curve.Verify(Message, Curve.PublicKey, Signature));

            Signature[0] ^= 0x01;

            Assert.IsFalse(Curve.Verify(Message, Curve.PublicKey, Signature));
        }

        [TestMethod]
        public void Test_03_Curve25519_KeyGen()
        {
            // Ref: testKeyGen() in https://git.silence.dev/Silence/curve25519-java/blob/master/tests/src/main/java/org/whispersystems/curve25519/Curve25519ProviderTest.java

            byte[] In = new byte[32];
            int i;

            In[0] = 123;

            for (i = 0; i < 1000; i++)
            {
                Curve25519 Curve = new Curve25519(In);
                In = Curve.PublicKey;
            }

            Assert.AreEqual("a23c8409f293b4426af5e5e7caee22a001c79aca1af2eacb4dddfa05f8bc7f37", Hashes.BinaryToString(In));
        }

    }*/
}
